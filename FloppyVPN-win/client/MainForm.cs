using FloppyVPN.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FloppyVPN
{
	public partial class MainForm : ClassicForm
	{
		public MainForm(bool connectAfterLaunch)
		{
			Application.ThreadException += new ThreadExceptionEventHandler(Utils.Exception);

			InitializeComponent();
			ApplyLocalizedTexts();

			LogIn();

			FillCountriesList();
			SelectCountryCode(ConnectionConfig.CurrentCountryCode);

			if (connectAfterLaunch)
			{
				ToTray(true);
				Connect();
			}
		}

		void ToTray(bool hideIsTrue_showIsFalse)
		{
			if (hideIsTrue_showIsFalse)
			{
				this.Hide();
				trayIcon.Visible = true;
				this.ShowInTaskbar = false;
			}
			else
			{
				this.Show();
				trayIcon.Visible = false;
				this.ShowInTaskbar = true;
			}
		}

		void ApplyLocalizedTexts()
		{
			if (Vpn.connected)
			{
				buttConnectDisconnect.Text = Loc.disconnect;
				labelConnectionStatus.Text = Loc.statusConnected;
			}
			else
			{
				buttConnectDisconnect.Text = Loc.connect;
				labelConnectionStatus.Text = Loc.statusNotConnected;
			}

			buttRefreshData_Click();

			if (File.Exists(PathsAndLinks.startupShortcutPath))
				buttStartupStatus.Text = Loc.startupStatusAdded;
			else
				buttStartupStatus.Text = Loc.startupStatusNotAdded;

			fileToolStripMenuItem.Text = Loc.fileMenu;
			optionsToolStripMenuItem.Text = Loc.optionsMenu;
			helpToolStripMenuItem.Text = Loc.helpMenu;
			buttWebsite.Text = Loc.websiteButton;
			buttExit.Text = Loc.exitButton;
			groupConnection.Text = Loc.connectionGroup;
			groupAccount.Text = Loc.accountGroup;
			buttAddTime.Text = Loc.addTimeButton;
			buttStartup.Text = Loc.startupButton;
			buttLanguage.Text = Loc.languageButton;
			buttRefreshData.Text = Loc.refreshbutton;
			//labelCurrentIpCaption.Text = Loc.currentIP;
			buttAddToStartup.Text = Loc.addToStartup;
			buttRemoveFromStartup.Text = Loc.removeFromStartup;
			buttUpdate.Text = Loc.updateButton;
			buttLogout.Text = Loc.logoutButton;
			buttRevealIp.Text = Loc.revealIP;
		}

		void MainForm_Closing(object sender, FormClosingEventArgs e)
		{
			DialogResult dialogResult = DialogResult.Yes;

			if (Vpn.connected)
				dialogResult = new MsgBox("Are you sure to close FLoppyVPN even though it is connected right now???", "Caution!", MessageBoxIcon.Question, MessageBoxButtons.YesNo).ShowDialog();

			if (dialogResult == DialogResult.Yes)
			{
				DisconnectAndQuit();
			}
			else
			{
				e.Cancel = true;
			}
		}

		void boxClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		void DisconnectAndQuit(object sender = null, EventArgs e = null)
		{
			try
			{
				trayIcon.Visible = false;
				Disconnect();
			}
			catch
			{
			}

			Environment.Exit(0);
		}

		void buttShow_Click(object sender, EventArgs e)
		{

		}

		void trayIcon_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				trayMenu.Show(Cursor.Position);
			}
			else if (e.Button == MouseButtons.Left)
			{
				Show();
				ShowInTaskbar = true;
				trayIcon.Visible = false;
				new SoundPlayer(Resources.chimes).Play();
			}
		}

		void buttConnectDisconnect_Click(object sender, EventArgs e)
		{
			if (Vpn.connected)
				Disconnect();
			else
				Connect();

			buttRefreshData_Click();
		}

		void Connect()
		{
			try
			{
				// First, get config of selected country code but only if:
				// a) selected country does NOT already match the current config country code
				// b) there is currently no valid config at all
				if (ConnectionConfig.CurrentCountryCode != SelectedCountryCode() || !ConnectionConfig.IsValid)
				{
					ConnectionConfig.Obtain(SelectedCountryCode());
				}

				Vpn.Connect();
				buttConnectDisconnect.Text = Loc.disconnect;
				buttConnectDisconnect.Image = Resources.connected;
				labelConnectionStatus.Text = Loc.statusConnected;
				pictureConnectionStatus.BackgroundImage = Resources.connected;
				pictureConnectionIllustration.BackgroundImage = Resources.connection;

				buttTrayConnectDisconnect.Text = Loc.disconnect;
			}
			catch (Exception ex)
			{
				new MsgBox(ex.Message, "FloppyVPN", MessageBoxIcon.Error, MessageBoxButtons.RetryCancel).ShowDialog();
			}
		}

		void Disconnect()
		{
			Vpn.Disconnect();
			Vpn.connected = false;
			buttConnectDisconnect.Text = Loc.connect;
			buttConnectDisconnect.Image = Resources.disconnected;
			labelConnectionStatus.Text = Loc.statusNotConnected;
			pictureConnectionStatus.BackgroundImage = Resources.disconnected;
			pictureConnectionIllustration.BackgroundImage = Resources.no_connection;

			buttTrayConnectDisconnect.Text = Loc.connect;
		}

		private void buttLanguage_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			if (e.ClickedItem == buttEN)
				Loc.lang = "en";
			else if (e.ClickedItem == buttRU)
				Loc.lang = "ru";
			else if (e.ClickedItem == buttUK)
				Loc.lang = "uk";
			else if (e.ClickedItem == buttJA)
				Loc.lang = "ja";

			IniFile.SetValue("lang", Loc.lang);

			Loc.Alize();
			ApplyLocalizedTexts();
		}

		void LogIn()
		{
			try
			{
				this.Hide();
				this.ShowInTaskbar = false;
			}
			catch
			{
			}

			DialogResult logined = new LoginForm().ShowDialog();
			if (logined == DialogResult.Yes)
			{
				buttRefreshData_Click();
				this.Show();
				this.ShowInTaskbar = true;
			}
			else
			{
				Environment.Exit(0);
			}
		}

		void LogOut()
		{
			Disconnect();
			Account.LogOut();
			Task.Delay(1000);
			LogIn();
		}

		void buttLogout_Click(object sender, EventArgs e)
		{
			DialogResult dialogResult = new MsgBox(Loc.logoutPrompt, "FloppyVPN", MessageBoxIcon.Question, MessageBoxButtons.YesNo).ShowDialog();
			if (dialogResult == DialogResult.Yes)
				LogOut();
		}

		void buttUpdate_Click(object sender, EventArgs e)
		{

		}

		void buttWebsite_Click(object sender, EventArgs e)
		{
			Utils.LaunchWebsite(PathsAndLinks.websiteURL);
		}

		void buttRefreshData_Click(object sender = null, EventArgs e = null)
		{
			Account.LogIn(Account.login);
			labelAccountStatus.Text = $"Login: {Account.masked_login}\nPaid till: {Account.paid_till}\nDays left: {Account.days_left}";

			stripIPpublic.Text = Loc.publicIP + ConnectionConfig.IPv4Address ?? "-";
			stripIPprivate.Text = Loc.privateIP + ConnectionConfig.IPv6Address ?? "-";

			FillCountriesList();
		}

		void FillCountriesList()
		{
			string selectedCountryCode = SelectedCountryCode();
			string[] available_country_codes = ConnectionConfig.GetAvailableCountryCodes();

			boxCountry.Items.Clear();
			boxCountry.Items.AddRange(available_country_codes);

			SelectCountryCode(selectedCountryCode);
		}

		void boxCountry_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		string SelectedCountryCode()
		{
			return boxCountry.Text;
		}

		void SelectCountryCode(string country_code)
		{
			int index = boxCountry.FindString(country_code);
			if (index != -1) //if found last country
				boxCountry.SelectedIndex = index;
			else //if not found last country
				index = new Random().Next(0, boxCountry.Items.Count - 1);

			boxCountry.SelectedIndex = index;
		}

		void buttAddTime_Click(object sender, EventArgs e)
		{
			string newAlias = Communicator.GetString($"{PathsAndLinks.orchestratorURL}/Api/App/GetPaymentAlias/{Account.login}",
				out bool isSuccessful,
				out _);

			if (isSuccessful)
				Utils.LaunchWebsite($"{PathsAndLinks.websiteURL}/Account/TopUp/{newAlias}");
			else
				new MsgBox(newAlias, "Error getting an alias").ShowDialog();
		}

		void buttSplitTunneling_Click(object sender, EventArgs e)
		{
			new MsgBox("Split tunneling is not yet implemented, sorry.");
		}
	}
}