using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FloppyVPN
{
	/// <summary>
	/// Common data and general functions.
	/// </summary>
	public static class Utils
	{
		public static void LaunchWebsite(string url)
		{
			try
			{
				Process.Start(url);
			}
			catch
			{
				Process.Start("explorer.exe", url);
			}
		}

		public static void Exception(object sender, ThreadExceptionEventArgs e)
		{
			new MsgBox(e.Exception.Message, "Loc.errorCaption", MessageBoxIcon.Error, MessageBoxButtons.OK).ShowDialog();
		}
	}
}