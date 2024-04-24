using System;
using System.IO;

namespace FloppyVPN
{
	public static class PathsAndLinks
	{
		public static readonly string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		public static readonly string appDir = Path.Combine(appDataDir, "FloppyVPN");
		public static readonly string startupShortcutPath = Path.Combine(PathsAndLinks.appDataDir, "Microsoft\\Windows\\Start Menu\\Programs\\Startup", "FloppyVPN.lnk");
		public static readonly string iniFilePath = Path.Combine(PathsAndLinks.appDir, "config.ini");

		public static readonly string masterServerURL = "http://localhost:1440";
		public static readonly string websiteURL = "http://localhost:1441";
	}
}
