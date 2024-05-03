using System;
using System.IO;

namespace FloppyVPN
{
	public static class PathsAndLinks
	{
		public static readonly string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FloppyVPN");
		public static readonly string appDir = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
		public static readonly string startupShortcutPath = Path.Combine(appDataDir, "Microsoft\\Windows\\Start Menu\\Programs\\Startup", "FloppyVPN.lnk");
		public static readonly string iniFilePath = Path.Combine(appDataDir, "config.ini");

		public static readonly string orchestratorURL = "https://orchestrator.floppy.jp.net";
		public static readonly string websiteURL = "https://floppy.jp.net";
	}
}
