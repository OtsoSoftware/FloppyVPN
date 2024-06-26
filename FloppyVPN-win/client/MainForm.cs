﻿using FloppyVPN.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FloppyVPN
{
	public partial class MainForm : ClassicForm
	{
		private readonly string countryCode_and_countryName_separator = " | ";
		private bool ignoreUIChanges = true;

		public MainForm(bool connectAfterLaunch)
		{
			Application.ThreadException += new ThreadExceptionEventHandler(Utils.Exception);

			InitializeComponent();
			ApplyLocalizedTexts();
			trayIcon.Icon = Resources.icon_disconnected;

			LogIn();

			ignoreUIChanges = true;
			FillCountriesList();
			ignoreUIChanges = false;

			SelectCountryCode(IniFile.GetValue("cc"));

			if (connectAfterLaunch)
			{
				SetTrayMode(true);
				Connect();
			}
		}

		void SetTrayMode(bool trayStatus)
		{
			if (trayStatus == true) //hide to tray
			{
				this.Hide();
				trayIcon.Visible = true;
				this.ShowInTaskbar = false;
			}
			else //restore window
			{
				this.Show();
				trayIcon.Visible = false;
				this.ShowInTaskbar = true;
				this.WindowState = FormWindowState.Normal;
				this.Activate();
			}

			new SoundPlayer(Resources.start).Play();
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

			RefreshAccountData();

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
			buttAddToStartup.Text = Loc.addToStartup;
			buttRemoveFromStartup.Text = Loc.removeFromStartup;
			buttUpdate.Text = Loc.updateButton;
			buttLogout.Text = Loc.logoutButton;
			buttRevealIp.Text = Loc.revealIP;
		}

		void MainForm_Closing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
			{
				DialogResult dialogResult = DialogResult.Yes;

				if (Vpn.connected)
					dialogResult = new MsgBox(Loc.sureToClose, "Closement", MessageBoxIcon.Question, MessageBoxButtons.YesNo).ShowDialog();

				if (dialogResult == DialogResult.Yes)
				{
					DisconnectAndQuit();
				}
				else
				{
					e.Cancel = true;
				}
			}
			else
			{
				DisconnectAndQuit();
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
				Vpn.Disconnect();
			}
			catch
			{
			}
			finally
			{
				Environment.Exit(0);
			}
		}

		void buttShow_Click(object sender, EventArgs e)
		{
			SetTrayMode(false);
		}

		void trayIcon_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				trayMenu.Show(Cursor.Position);
				ShowInTaskbar = false;
				trayIcon.Visible = true;
			}
			else if (e.Button == MouseButtons.Left)
			{
				SetTrayMode(false);
			}
		}

		void buttConnectDisconnect_Click(object sender, EventArgs e)
		{
			if (Vpn.connected)
				Disconnect();
			else
				Connect();

			RefreshConnectionData();
		}

		void Connect()
		{
			if (Account.days_left <= 0)
			{
				new MsgBox(Loc.noDaysLeft, Loc.errorConnectingCaption, MessageBoxIcon.Error).ShowDialog();
				return;
			}

			try
			{
				// First, get config of selected country code but only if:
				// a) selected country does NOT already match the current config country code
				// b) there is currently no valid config at all
				if (Conf.CurrentCountryCode != SelectedCountryCode() || !Conf.IsValid)
				{
					Conf.Obtain(SelectedCountryCode());
				}

				Vpn.Connect();
				buttConnectDisconnect.Text = Loc.disconnect;
				buttConnectDisconnect.Image = Resources.connected;
				labelConnectionStatus.Text = Loc.statusConnected;
				pictureConnectionStatus.BackgroundImage = Resources.connected;
				pictureConnectionIllustration.Image = Resources.connection;
				buttTrayConnectDisconnect.Text = Loc.disconnect;
				buttTrayStatus.Text = Loc.statusConnected.ToUpper();
				trayIcon.Text = "FloppyVPN: " + Loc.statusConnected.ToUpper();
				trayIcon.Icon = Resources.icon_connected;

				new Task(() => { DriverAliveStatusWatchdog(); }).Start();
			}
			catch (Exception ex)
			{
				DialogResult dr = new MsgBox(ex.Message, "FloppyVPN", MessageBoxIcon.Error, MessageBoxButtons.RetryCancel).ShowDialog();
				if (dr == DialogResult.Retry)
					Connect();
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
			pictureConnectionIllustration.Image = Resources.no_connection;
			buttTrayConnectDisconnect.Text = Loc.connect;
			buttTrayStatus.Text = Loc.statusNotConnected.ToUpper();
			trayIcon.Text = "FloppyVPN: " + Loc.statusNotConnected.ToUpper();
			trayIcon.Icon = Resources.icon_disconnected;
		}

		async void DriverAliveStatusWatchdog()
		{
			for (; ; )
			{
				await Task.Delay(2000);

				if (!Vpn.connected)
					break;

				if (Process.GetProcessesByName(Vpn.processName).Length <= 0)
				{
					this.Invoke((Action)delegate { Disconnect(); });
					new MsgBox(Loc.driverDied, Loc.errorConnectingCaption, MessageBoxIcon.Error).ShowDialog();
				}
			}
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
				RefreshAccountData();
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

		void buttRefreshData_Click(object sender, EventArgs e)
		{
			RefreshAccountData();
		}

		void RefreshAccountData()
		{
			Account.LogIn(Account.login);
			labelAccountStatus.Text = $"{Loc.login}{Account.masked_login}\n{Loc.paidTill}{Account.paid_till}\n{Loc.daysLeft}{Account.days_left}";

			FillCountriesList();
		}

		void RefreshConnectionData()
		{
			stripIPpublic.Text = Loc.publicIP + Conf.IPv4Address ?? "-";
			stripIPprivate.Text = Loc.privateIP + Conf.IPv6Address ?? "-";
		}

		void FillCountriesList()
		{
			try
			{
				string selectedCountryCode = SelectedCountryCode();
				JArray available_country_codes = Conf.GetAvailableCountryCodes();

				boxCountry.Items.Clear();

				foreach (JObject available_country in available_country_codes)
				{
					string country_code = available_country["country_code"].ToString();
					string country_name = available_country["country_name"].ToString();

					boxCountry.Items.Add(string.Join(countryCode_and_countryName_separator, country_code, country_name));
				}

				SelectCountryCode(selectedCountryCode);

			}
			catch
			{
			}
		}

		void boxCountry_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				ResourceManager rm = CountriesFlags.ResourceManager;
				Bitmap myImage = (Bitmap)rm.GetObject(SelectedCountryCode().ToLower());

				pictureCountry.BackgroundImage = myImage;

				if (!ignoreUIChanges)
					IniFile.SetValue("cc", SelectedCountryCode());
			}
			catch
			{
			}
		}

		string SelectedCountryCode()
		{
			return boxCountry.Text.Split(new string[] { countryCode_and_countryName_separator }, StringSplitOptions.None)[0];
		}

		void SelectCountryCode(string country_code)
		{
			int index = boxCountry.FindString(country_code + countryCode_and_countryName_separator);
			if (index != -1) //if found country
				boxCountry.SelectedIndex = index;
			else //if not found country
				index = new Random().Next(0, boxCountry.Items.Count - 1);

			boxCountry.SelectedIndex = index;
		}

		void buttAddTime_Click(object sender, EventArgs e)
		{
			string newAlias = Communicator.GetString($"{PathsAndLinks.orchestratorURL}/Api/App/GetPaymentAlias/{Account.login}",
				out bool isSuccessful,
				out _);

			if (isSuccessful)
				Utils.LaunchWebsite($"{PathsAndLinks.websiteURL}/TopUp/{newAlias}");
			else
				new MsgBox(newAlias, "Error getting an alias").ShowDialog();
		}

		void buttSplitTunneling_Click(object sender, EventArgs e)
		{
			new MsgBox("Split tunneling is not yet implemented, sorry.");
		}

		void MainForm_Resize(object sender, EventArgs e)
		{
			if (WindowState == FormWindowState.Minimized)
			{
				SetTrayMode(true);
			}
		}

		void labelVersionCaption_Click(object sender, EventArgs e)
		{

		}
	}
}