using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FloppyVPN
{
	internal static class Vpn
	{
		private static readonly string ConfigFolderPath = "/etc/amnezia/amneziawg/";
		private static readonly string ServerConfigFileName = "awg0.conf";
		private static readonly string ServerConfigFilePath = Path.Combine(ConfigFolderPath, ServerConfigFileName);

		public static string CreateClientConfig(ulong config_id)
		{
			// Generate client keys
			string clientPrivateKey = ShellExecute("awg genkey");
			string clientPublicKey = ShellExecute($"echo \"{clientPrivateKey}\" | awg pubkey");
			string clientPSK = ShellExecute("awg genpsk");

			string clientName = $"client{config_id}";
			byte addressNumber = GetFreeAddressNumber();

			bool ipv6supported = false;
			if (Config.cache["ipv6_support"].ToString() == bool.TrueString)
				ipv6supported = true;

			// Update server config
			File.AppendAllText(ServerConfigFilePath, $@"
#{clientName}
[Peer]
PublicKey = {clientPublicKey}
PresharedKey = {clientPSK}
AllowedIPs = 10.7.0.{addressNumber}/32{(ipv6supported ? $", fd0d:86fa:c3bc::{addressNumber}/128" : "")}
");

			// Refresh interface gracefully
			RefreshInterface();

			// Generate user config
			string newClientConfig = 
$@"[Interface]
Address = 10.7.0.{addressNumber}/32{(ipv6supported ? $", fd0d:86fa:c3bc::{addressNumber}/128" : "")}
DNS = {Config.cache["dnsv4"]}{(ipv6supported ? $", {Config.cache["dnsv6"]}" : "")}
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
Endpoint = {Config.cache["domain_name_or_ipv4"]}:{Config.cache["vpn_listen_port"]}
PublicKey = {Config.cache["server_public_key"]}
PresharedKey = {clientPSK}
AllowedIPs = 0.0.0.0/0{(ipv6supported ? ", ::/0" : "")}
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
			string[] configLines = File.ReadAllLines(ServerConfigFilePath);

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

					if (removeSection && line.StartsWith("#"))
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

		public static void CreateServerConfigIfNotYet()
		{
			if (!string.IsNullOrEmpty(Config.Get("server_public_key")) && !string.IsNullOrEmpty(Config.Get("server_private_key")) && File.Exists(ServerConfigFilePath))
				return;

			Vpn.DownInterface();
			Thread.Sleep(1000);

			//var process = Process.Start(new ProcessStartInfo
			//{
			//	FileName = WGCommand,
			//	Arguments = "genkey",
			//	RedirectStandardOutput = true,
			//	UseShellExecute = false
			//});

			//string serverPrivateKey = process.StandardOutput.ReadToEnd().Trim();
			string serverPrivateKey = ShellExecute("awg genkey");
			//process.WaitForExit();

			Config.Set("server_private_key", serverPrivateKey);


			//string serverPublicKey = process.StandardOutput.ReadToEnd().Trim();
			string serverPublicKey = ShellExecute($"echo \"{serverPrivateKey}\" | awg pubkey");

			Config.Set("server_public_key", serverPublicKey);

			Console.WriteLine($"Generated and set the server private and public keys:\n{serverPrivateKey}\n{serverPublicKey}");

			bool ipv6supported = false;
			if (Config.Get("ipv6_support") == bool.TrueString)
				ipv6supported = true;

			//creating server config:
			File.WriteAllText(ServerConfigFilePath, 
$@"# ENDPOINT {Config.Get("ipv4_address")}{(ipv6supported ? $"\n# ENDPOINT {Config.Get("ipv6_address")}" : "")}
[Interface]
Address = 10.7.0.0/24{(ipv6supported ? "\nAddress = fd0d:86fa:c3bc::1/64" : "")}
PrivateKey = {Config.Get("server_private_key")}
ListenPort = {Config.Get("vpn_listen_port")}
PostUp = iptables -t nat -A POSTROUTING -s 10.7.0.0/24 -o {Config.Get("internet_interface_name")} -j SNAT --to-source {Config.Get("ipv4_address")}{(ipv6supported ? $"\nPostUp = ip6tables -t nat -A POSTROUTING -s fd0d:86fa:c3bc::1/64 -o {Config.Get("internet_interface_name")} -j SNAT --to-source {Config.Get("ipv6_address")}" : "")}
PostDown = iptables -t nat -D POSTROUTING -s 10.7.0.0/24 -o {Config.Get("internet_interface_name")} -j SNAT --to-source {Config.Get("ipv4_address")}{(ipv6supported ? $"\nPostDown = ip6tables -t nat -D POSTROUTING -s fd0d:86fa:c3bc::1/64 -o {Config.Get("internet_interface_name")} -j SNAT --to-source {Config.Get("ipv6_address")}" : "")}
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

		/// <summary>
		/// Refreshes interface configuration gracefully, 
		/// without affecting existing connections
		/// </summary>
		private static void RefreshInterface()
		{
			ShellExecute($"awg syncconf awg0 <(awg-quick strip awg0)");
		}

		public static void UpInterface()
		{
			ShellExecute("awg-quick up awg0");
		}

		public static void DownInterface()
		{
			ShellExecute("awg-quick down awg0");
		}

		private static string ShellExecute(string command)
		{
			var process = Process.Start(new ProcessStartInfo
			{
				FileName = "/bin/bash",
				Arguments = $"-c \"{command}\"",
				RedirectStandardOutput = true,
				UseShellExecute = false
			});

			string output = process.StandardOutput.ReadToEnd().Trim().Trim('\n');
			process.WaitForExit();
			return output;
		}
	}
}