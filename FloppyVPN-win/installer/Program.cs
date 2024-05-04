using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace FloppyVPN
{
	internal static class Program
	{
		public static readonly string programname = "FloppyVPN";
		public static readonly string publishername = "OtsoSoftware";
		public static string language = "en";
		public static readonly string commonappdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
		public static readonly string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		public static readonly string tempfolder = Path.GetTempPath();
		public static readonly string tempfile = Path.Combine(tempfolder, "FloppyVPN_archive.zip");
		public static readonly string programfiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
		public static string installfolder = Path.Combine(commonappdata, "Programs", "FloppyVPN");
		public static string pathtoinstalledexe = "";

		public static string linkToDistro = $"{PathsAndLinks.websiteURL}/setups/win/";
		//public static string linkToDriver = $"{PathsAndLinks.orchestratorURL}/setups/";



		[DllImport("user32.dll")]
		private static extern bool SetProcessDPIAware();



		[STAThread]
		static void Main()
		{
			//single instance:
			var mutex = new Mutex(true, "floppyvpninstaller8g78wegy4hnrtsjr6j6rjysj", out bool result);
			if (!result)
			{
				Environment.Exit(0);
			}
			GC.KeepAlive(mutex);

			//disable SSL checks (there was no ssl/tls in good old days):
			ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

			//detect user PC language:
			string currentculture = CultureInfo.CurrentUICulture.Name.ToLower();
			if (currentculture.Contains("ru") || currentculture.Contains("uk") || currentculture.Contains("uz") || currentculture.Contains("bel") || currentculture.Contains("kz"))
				language = "ru";

			//use custom exception handler:
			Application.ThreadException += new ThreadExceptionEventHandler(Utils.Exception);

			//fix DPI:
			if (Environment.OSVersion.Version.Major >= 6)
				SetProcessDPIAware();

			//depending on system architecture, use appropriate binary:
			Architecture arch = RuntimeInformation.ProcessArchitecture;
			switch (arch)
			{
				case Architecture.X64:
					linkToDistro += "x86_64.zip";
					break;
				case Architecture.Arm64:
					linkToDistro += "arm64.zip";
					break;
				case Architecture.X86:
					linkToDistro += "x86.zip";
					break;
				default:
					if (Environment.Is64BitOperatingSystem)
						linkToDistro += "x86_64.zip";
					else
						linkToDistro += "x86.zip";
					break;
			}


			//launch the installer interface:
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new InstallForm());
		}
	}
}
