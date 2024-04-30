using System;
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
		//DPI fix:
		[DllImport("user32.dll")]
		private static extern bool SetProcessDPIAware();


		[STAThread]
		static void Main(string[] args)
		{
			//only single instance:
			Mutex mutex = new Mutex(true, "FloppyVPN_ClientLXN9K60SVT3dcJJ3P8eSQQi", out bool result);
			if (!result)
			{
				new MsgBox(Loc.alreadylaunched);
				Environment.Exit(0);
			}
			GC.KeepAlive(mutex);

			//DPI fix:
			if (Environment.OSVersion.Version.Major >= 6)
				SetProcessDPIAware();
			Application.SetCompatibleTextRenderingDefault(false);

			//use custom exception handler:
			Application.ThreadException += new ThreadExceptionEventHandler(Utils.Exception);

			//create appdata folder
			try
			{
				Directory.CreateDirectory(PathsAndLinks.appDataDir);
			}
			catch
			{
			}

			//get chosen language or detect it:
			Loc.lang = IniFile.GetValue("lang");
			if (Loc.lang.Length != 2)
				Loc.lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();
			Loc.Alize();

			//check for driver existance:
			if (!File.Exists(Vpn.pathToDriver))
			{
				new MsgBox(Loc.cantFindDriver, "FloppyVPN", MessageBoxIcon.Error).ShowDialog();
				Environment.Exit(0);
			}

			//flag for auto-connect on launch if set to do so:
			bool connectAfterLaunch = false;
			if (args.Length > 0 && (args[0].Contains("--connect-after-launch") || args[0].Contains("-cal")))
				connectAfterLaunch = true;

			//kill possible driver running from previous sessions:
			Vpn.Disconnect();

			//restore last used country code:
			Conf.CurrentCountryCode = IniFile.GetValue("cc") != "" ? IniFile.GetValue("cc") : null;

			//finally, start the gui:
			Application.Run(new MainForm(connectAfterLaunch));
		}
	}
}