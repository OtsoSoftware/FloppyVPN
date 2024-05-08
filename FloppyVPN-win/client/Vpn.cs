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
		private static readonly string pathToConf = Path.GetFullPath(Path.Combine(PathsAndLinks.appDataDir, "FloppyVPN.conf"));
		private static readonly string tunnelName = "FloppyVPN";
		public static readonly string processName = "floppydriver";
		public static readonly string pathToDriver = Path.GetFullPath(Path.Combine(PathsAndLinks.appDir, "driver", $"{processName}.exe"));


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
				psi.Arguments = $"/installtunnelservice \"{pathToConf}\"";
				psi.RedirectStandardOutput = true;
				psi.RedirectStandardError = true;
				psi.UseShellExecute = false;
				psi.CreateNoWindow = true;
				int exitCode = 0;

				using (Process process = new Process { StartInfo = psi })
				{
					process.Start();
					process.WaitForExit();
					exitCode = process.ExitCode;
				}

				Task.Delay(new Random().Next(2000, 2200)).GetAwaiter().GetResult();

				if (Process.GetProcessesByName(processName).Length > 0)
					connected = true;
				else
					throw new Exception($"{Loc.driverDied}: {exitCode}");
			}
			catch (Exception ex)
			{
				throw new Exception($"Error using driver: {ex.Message}");
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
					process.WaitForExit();

					Thread.Sleep(50);

					//try
					//{
					//	foreach (Process _process in Process.GetProcessesByName(processName))
					//		_process.Kill();
					//}
					//catch
					//{
					//}
				}
			}
			catch
			{
			}
			finally
			{
				File.Delete(pathToConf);
			}

			connected = false;
		}
	}
}