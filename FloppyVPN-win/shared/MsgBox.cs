﻿using FloppyVPN.Properties;
using System.Drawing;
using System;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FloppyVPN
{
	public partial class MsgBox : ClassicForm
	{
		bool userCanCloseMsgBox = true;

		public MsgBox(string message, string caption = "FloppyVPN", MessageBoxIcon msgicon = MessageBoxIcon.Information, MessageBoxButtons msgbuttons = MessageBoxButtons.OK)
		{
			InitializeComponent();
			Task.Delay(100).GetAwaiter().GetResult();


			//set up texts:
			labelMessage.Text = message;
			SetWindowTitle(caption);


			//set up icon and play the corresponding sound:
			if (msgicon == MessageBoxIcon.Error)
			{
				picMsgIcon.BackgroundImage = Resources.msg_warning_0;
				new SoundPlayer(Resources.chord);
			}
			else if (msgicon == MessageBoxIcon.Question)
			{
				picMsgIcon.BackgroundImage = Resources.msg_question_0;
				new SoundPlayer(Resources.chimes);
			}
			else
			{
				picMsgIcon.BackgroundImage = Resources.msg_information_0;
				new SoundPlayer(Resources.notify);
			}


			//set up buttons:
			if (msgbuttons == MessageBoxButtons.YesNo)
			{
				this.DialogResult = DialogResult.No;
				buttYes.Visible = true;
				buttNo.Visible = true;
				buttYes.Focus();
			}
			else if (msgbuttons == MessageBoxButtons.RetryCancel)
			{
				this.DialogResult = DialogResult.Cancel;
				// DisableCloseBox();
				buttRetry.Visible = true;
				buttCancel.Visible = true;
				buttRetry.Focus();
			}
			else //"OK"
			{
				buttOK.Visible = true;
				buttOK.Focus();
			}

			this.Text = caption;

			HideMaximizeBox();
		}

		void MsgBox_FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = true;
			if (userCanCloseMsgBox)
			{
				e.Cancel = false;
			}
			else
			{
				new SoundPlayer(Resources.chimes).Play();
			}
		}

		void buttYes_Click(object sender, EventArgs e)
		{
			userCanCloseMsgBox = true;
			this.DialogResult = DialogResult.Yes;
			Close();
		}

		void buttNo_Click(object sender, EventArgs e)
		{
			userCanCloseMsgBox = true;
			this.DialogResult = DialogResult.No;
			Close();
		}

		void buttOK_Click(object sender, EventArgs e)
		{
			userCanCloseMsgBox = true;
			this.DialogResult = DialogResult.OK;
			Close();
		}

		void buttRetry_Click(object sender, EventArgs e)
		{
			userCanCloseMsgBox = true;
			this.DialogResult = DialogResult.Retry;
			Close();
		}

		void buttCancel_Click(object sender, EventArgs e)
		{
			userCanCloseMsgBox = true;
			this.DialogResult = DialogResult.Cancel;
			Close();
		}

		void MsgBox_Load(object sender, EventArgs e)
		{
			//move window to the cursor position but not out of working area bounds:

			Point cursorPos = Cursor.Position;

			Screen screen = Screen.FromPoint(cursorPos);

			int newX = cursorPos.X - (Width / 2);
			int newY = cursorPos.Y - (Height / 2);

			newX = Math.Max(screen.WorkingArea.Left, Math.Min(newX, screen.WorkingArea.Right - Width));
			newY = Math.Max(screen.WorkingArea.Top, Math.Min(newY, screen.WorkingArea.Bottom - Height));

			Location = new Point(newX, newY);
		}
	}
}
