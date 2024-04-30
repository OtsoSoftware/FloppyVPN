using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FloppyVPN
{
	public static class Vpn
	{
		public static bool connected = false;
		private static readonly string pathToConf = Path.Combine(PathsAndLinks.appDataDir, "FloppyVPN.conf");
		private static readonly string tunnelName = "FloppyVPN";
		private static readonly string processName = "floppydriver";
		public static readonly string pathToDriver = Path.Combine(PathsAndLinks.appDir, "driver", $"{processName}.exe");


		public static void Connect()
		{
			try
			{
				try { Disconnect(); Thread.Sleep(100); } catch { }

				if (!Conf.IsValid)
					throw new Exception("Connection config is in invalid state!");
				else
					File.WriteAllText(pathToConf, Conf.ConfString, Encoding.Unicode);

				ProcessStartInfo psi = new ProcessStartInfo();
				psi.FileName = pathToDriver;
				psi.Arguments = $@"/installtunnelservice ""{pathToConf}""";
				psi.RedirectStandardOutput = true;
				psi.RedirectStandardError = true;
				psi.UseShellExecute = false;
				psi.CreateNoWindow = true;

				using (Process process = new Process { StartInfo = psi })
				{
					process.Start();
				}

				Task.Delay(new Random().Next(300, 700)).GetAwaiter().GetResult();
				connected = true;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error connecting via driver: {ex.Message}");
			}
		}

		public static void Disconnect()
		{
			try
			{
				ProcessStartInfo psi = new ProcessStartInfo();
				psi.FileName = pathToDriver;
				psi.Arguments = $"/uninstalltunnelservice \"{tunnelName}\"";
				psi.RedirectStandardOutput = true;
				psi.RedirectStandardError = true;
				psi.UseShellExecute = false;
				psi.CreateNoWindow = true;

				using (Process process = new Process { StartInfo = psi })
				{
					process.Start();
				}
			}
			catch
			{
			}
			finally
			{
				try
				{
					foreach (Process process in Process.GetProcessesByName(processName))
						process.Kill();
				}
				catch
				{
				}
			}

			connected = false;
		}
	}
}