using FloppyVPN.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FloppyVPN
{
	public partial class LoginForm : ClassicForm
	{
		public LoginForm()
		{
			InitializeComponent();
			ApplyLocalizedTexts();
			this.DialogResult = DialogResult.No;
		}

		void ApplyLocalizedTexts()
		{
			fileToolStripMenuItem.Text = Loc.fileMenu;
			buttCloseLoginForm.Text = Loc.close;
			buttRegister.Text = Loc.register;
			labelLoginEntering.Text = Loc.loginCaption;
			buttLogin.Text = Loc.loginButton;
			groupLogin.Text = Loc.loginGroup;
		}

		void buttRegister_Click(object sender, EventArgs e)
		{
			Utils.LaunchWebsite($"{PathsAndLinks.websiteURL}/register");
		}

		void buttLogin_Click(object sender = null, EventArgs e = null)
		{
			if (txtLogin.Text.Length < 6)
			{
				Thread.Sleep(new Random().Next(300, 900));
				new MsgBox("Incorrect login").ShowDialog();
				return;
			}

			bool successFullyLoggedIn = Account.LogIn(txtLogin.Text);

			if (successFullyLoggedIn)
			{
				this.DialogResult = DialogResult.Yes;
				this.Close();
			}
			else
			{
				this.DialogResult = DialogResult.No;
				new MsgBox(Loc.unableToLogInText, Loc.unableToLoginCaption, MessageBoxIcon.Error).ShowDialog();
			}
		}

		void LoginForm_Load(object sender, EventArgs e)
		{
			//Task.Delay(100).GetAwaiter().GetResult();

			string savedLogin = IniFile.GetValue("login") ?? "";
			if (savedLogin != "")
			{
				txtLogin.Text = savedLogin;
				buttLogin_Click();
			}
		}

		void buttLanguage_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
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
	}
}