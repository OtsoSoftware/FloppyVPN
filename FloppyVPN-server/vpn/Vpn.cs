using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FloppyVPN
{
	internal static class Vpn
	{
		private static readonly string ConfigFolderPath = "/etc/amnezia/amneziawg/";
		private static readonly string ServerConfigFileName = "awg0.conf";
		private static readonly string ServerInterfaceFilePath = Path.Combine(ConfigFolderPath, ServerConfigFileName);
		private static readonly string ServerInterfaceName = "awg0";
		private static readonly string WGCommand = "awg";

		public static string CreateClientConfig(ulong config_id)
		{
			// Generate client keys
			string clientPrivateKey = GenerateKey();
			string clientPublicKey = GenerateKey();

			string clientPSK = GeneratePSK();

			string clientFileName = $"client{config_id}";
			string clientName = $"client{config_id}";
			byte addressNumber = GetFreeAddressNumber();

			// Update server config
			File.AppendAllText(ServerInterfaceFilePath, $@"

#{clientName}
[Peer]
PublicKey = {clientPublicKey}
PresharedKey = {clientPSK}
AllowedIPs = 10.7.0.{addressNumber}/32, fd0d:86fa:c3bc::{addressNumber}/128
");

			// Refresh interface gently
			RefreshInterface();

			string newClientConfig = $@"
[Interface]
Address = 10.7.0.{addressNumber}/32
Address = fd0d:86fa:c3bc::{addressNumber}/128
DNS = 1.1.1.1, 1.0.0.1
DNS = 2606:4700:4700::1111, 2606:4700:4700::1001
PrivateKey = {clientPrivateKey}
Jc = 5
Jmin = 20
Jmax = 1180
S1 = 127
S2 = 59
H1 = 1012599525
H2 = 1289564734
H3 = 1183456273
H4 = 2112541503

[Peer]
Endpoint = {Config.cache["ipv4_address"]}:51235
Endpoint = {Config.cache["ipv6_address"]}:51235
PublicKey = {Config.cache["server_public_key"]}
PresharedKey = {clientPSK}
AllowedIPs = 0.0.0.0/0, ::/0
PersistentKeepalive = 25
";

			return newClientConfig;
		}

		/// <summary>
		/// This solves the problem of running out of IP addresses when creating config(s) MANY TIMES.
		/// because one config may be recreated hundreds times, not to mention that there are much more
		/// than one config. 
		/// This method returns either the new address, or an address that has been deleted 
		/// (basically, from the gap).
		/// Though, the limit of configs per a server is 252.
		/// </summary>
		/// <returns>A number symbolyzing address that can be used to create a new config</returns>
		public static byte GetFreeAddressNumber()
		{
			// Read the server config file
			string serverConfigFilePath = Path.Combine(ConfigFolderPath, ServerConfigFileName);
			string[] configLines = File.ReadAllLines(serverConfigFilePath);

			// Extract address numbers from existing client configs
			var addressNumbers = new HashSet<byte>();
			foreach (string line in configLines)
			{
				//ipv4:
				Match match = Regex.Match(line, @"10\.7\.0\.(\d+)/32");
				if (match.Success)
				{
					if (byte.TryParse(match.Groups[1].Value, out byte addressNumber))
					{
						addressNumbers.Add(addressNumber);
					}
				}

				//ipv6:
				match = Regex.Match(line, @"fd0d:86fa:c3bc::(\d+)/128");
				if (match.Success)
				{
					if (byte.TryParse(match.Groups[1].Value, out byte addressNumber))
					{
						addressNumbers.Add(addressNumber);
					}
				}
			}

			// Fibd the first available address number
			byte firstFreeAddressNumber = Enumerable.Range(2, 252)
				.Select(i => (byte)i)
				.FirstOrDefault(i => !addressNumbers.Contains(i));

			return firstFreeAddressNumber;
		}

		public static void DeleteConfig(ulong id)
		{
			//remove client from server config:
			string serverConfigFilePath = Path.Combine(ConfigFolderPath, ServerConfigFileName);
			string[] lines = File.ReadAllLines(serverConfigFilePath);

			using (StreamWriter writer = File.CreateText(serverConfigFilePath))
			{
				bool removeSection = false;
				foreach (string line in lines)
				{
					if (line.Contains($"client{id}"))
					{
						removeSection = true;
						continue;
					}

					if (removeSection && line.StartsWith("["))
					{
						removeSection = false;
					}

					if (!removeSection)
					{
						writer.WriteLine(line);
					}
				}
			}

			// Refresh server interface
			RefreshInterface();
		}

		private static string GenerateKey()
		{
			Process process = Process.Start(new ProcessStartInfo
			{
				FileName = WGCommand,
				Arguments = "genkey",
				RedirectStandardOutput = true,
				UseShellExecute = false
			});

			string key = process.StandardOutput.ReadToEnd().Trim();
			process.WaitForExit();
			return key;
		}

		private static string GeneratePSK()
		{
			var process = Process.Start(new ProcessStartInfo
			{
				FileName = WGCommand,
				Arguments = "genpsk",
				RedirectStandardOutput = true,
				UseShellExecute = false
			});

			string psk = process.StandardOutput.ReadToEnd().Trim();
			process.WaitForExit();
			return psk;
		}

		public static void GenerateServerConfigIfNotYet()
		{
			if (!string.IsNullOrEmpty(Config.Get("server_public_key")) && !string.IsNullOrEmpty(Config.Get("server_private_key")) && File.Exists(ServerInterfaceFilePath))
				return;

			var process = Process.Start(new ProcessStartInfo
			{
				FileName = WGCommand,
				Arguments = "genkey",
				RedirectStandardOutput = true,
				UseShellExecute = false
			});

			string serverPrivateKey = process.StandardOutput.ReadToEnd().Trim();
			process.WaitForExit();

			Config.Set("server_private_key", serverPrivateKey);

			process = Process.Start(new ProcessStartInfo
			{
				FileName = WGCommand,
				Arguments = $"pubkey <<< \"{serverPrivateKey}\"",
				RedirectStandardOutput = true,
				UseShellExecute = false
			});

			string serverPublicKey = process.StandardOutput.ReadToEnd().Trim();
			process.WaitForExit();

			Config.Set("server_public_key", serverPublicKey);

			Console.WriteLine($"Generated and set the server private and public keys: \n{serverPrivateKey}\n{serverPublicKey}");

			//creating server config:
				File.WriteAllText(ServerInterfaceFilePath, $@"
# ENDPOINT {Config.Get("ipv4_address")}
# ENDPOINT {Config.Get("ipv6_address")}
[Interface]
Address = 10.7.0.0/24
Address = fd0d:86fa:c3bc::1/64
PrivateKey = {Config.Get("server_public_key")}
ListenPort = {Config.Get("listen_port")}
PostUp = iptables -t nat -A POSTROUTING -s 10.7.0.0/24 -o eth0 -j SNAT --to-source {Config.Get("ipv4_address")}
PostUp = ip6tables -t nat -A POSTROUTING -s fd0d:86fa:c3bc::1/64 -o eth0 -j SNAT --to-source {Config.Get("ipv6_address")} // Add IPv6 NAT rule
PostDown = iptables -t nat -D POSTROUTING -s 10.7.0.0/24 -o eth0 -j SNAT --to-source {Config.Get("ipv4_address")}
PostDown = ip6tables -t nat -D POSTROUTING -s fd0d:86fa:c3bc::1/64 -o eth0 -j SNAT --to-source {Config.Get("ipv6_address")} // Add IPv6 NAT rule
Jc = 5
Jmin = 20
Jmax = 1180
S1 = 127
S2 = 59
H1 = 1012599525
H2 = 1289564734
H3 = 1183456273
H4 = 2112541503
");
		}

		private static void RefreshInterface()
		{
			ExecuteShellCommand($"syncconf {ServerInterfaceName} <(awg-quick strip {ServerInterfaceName})");
		}

		private static string ExecuteShellCommand(string command)
		{
			var process = Process.Start(new ProcessStartInfo
			{
				FileName = "/bin/bash",
				Arguments = $"-c \"{command}\"",
				RedirectStandardOutput = true,
				UseShellExecute = false
			});

			string output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			return output;
		}
	}
}