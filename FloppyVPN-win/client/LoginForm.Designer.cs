﻿namespace FloppyVPN
{
	partial class LoginForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
			this.borderPanel = new System.Windows.Forms.Panel();
			this.windowMenu = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.buttCloseLoginForm = new System.Windows.Forms.ToolStripMenuItem();
			this.buttLanguage = new System.Windows.Forms.ToolStripMenuItem();
			this.buttEN = new System.Windows.Forms.ToolStripMenuItem();
			this.buttRU = new System.Windows.Forms.ToolStripMenuItem();
			this.buttUK = new System.Windows.Forms.ToolStripMenuItem();
			this.buttJA = new System.Windows.Forms.ToolStripMenuItem();
			this.groupLogin = new System.Windows.Forms.GroupBox();
			this.buttRegister = new System.Windows.Forms.Button();
			this.labelLoginEntering = new System.Windows.Forms.Label();
			this.txtLogin = new System.Windows.Forms.TextBox();
			this.buttLogin = new System.Windows.Forms.Button();
			this.borderPanel.SuspendLayout();
			this.windowMenu.SuspendLayout();
			this.groupLogin.SuspendLayout();
			this.SuspendLayout();
			// 
			// borderPanel
			// 
			this.borderPanel.Controls.Add(this.windowMenu);
			this.borderPanel.Controls.Add(this.groupLogin);
			this.borderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.borderPanel.Location = new System.Drawing.Point(5, 31);
			this.borderPanel.Margin = new System.Windows.Forms.Padding(0);
			this.borderPanel.Name = "borderPanel";
			this.borderPanel.Size = new System.Drawing.Size(336, 180);
			this.borderPanel.TabIndex = 3;
			// 
			// windowMenu
			// 
			this.windowMenu.BackColor = System.Drawing.Color.WhiteSmoke;
			this.windowMenu.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.windowMenu.GripMargin = new System.Windows.Forms.Padding(0);
			this.windowMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.windowMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.buttLanguage});
			this.windowMenu.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
			this.windowMenu.Location = new System.Drawing.Point(0, 0);
			this.windowMenu.Name = "windowMenu";
			this.windowMenu.Padding = new System.Windows.Forms.Padding(3, 2, 0, 5);
			this.windowMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.windowMenu.Size = new System.Drawing.Size(336, 29);
			this.windowMenu.TabIndex = 16;
			this.windowMenu.Text = "windowMenu";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttCloseLoginForm});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(42, 22);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// buttCloseLoginForm
			// 
			this.buttCloseLoginForm.Name = "buttCloseLoginForm";
			this.buttCloseLoginForm.Size = new System.Drawing.Size(124, 26);
			this.buttCloseLoginForm.Text = "Close";
			// 
			// buttLanguage
			// 
			this.buttLanguage.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttEN,
            this.buttRU,
            this.buttUK,
            this.buttJA});
			this.buttLanguage.Name = "buttLanguage";
			this.buttLanguage.Size = new System.Drawing.Size(85, 22);
			this.buttLanguage.Text = "Language";
			this.buttLanguage.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.buttLanguage_DropDownItemClicked);
			// 
			// buttEN
			// 
			this.buttEN.Name = "buttEN";
			this.buttEN.Size = new System.Drawing.Size(162, 26);
			this.buttEN.Text = "English";
			// 
			// buttRU
			// 
			this.buttRU.Name = "buttRU";
			this.buttRU.Size = new System.Drawing.Size(162, 26);
			this.buttRU.Text = "Русский";
			// 
			// buttUK
			// 
			this.buttUK.Name = "buttUK";
			this.buttUK.Size = new System.Drawing.Size(162, 26);
			this.buttUK.Text = "Українська";
			// 
			// buttJA
			// 
			this.buttJA.Name = "buttJA";
			this.buttJA.Size = new System.Drawing.Size(162, 26);
			this.buttJA.Text = "日本語";
			// 
			// groupLogin
			// 
			this.groupLogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(214)))), ((int)(((byte)(206)))));
			this.groupLogin.Controls.Add(this.buttRegister);
			this.groupLogin.Controls.Add(this.labelLoginEntering);
			this.groupLogin.Controls.Add(this.txtLogin);
			this.groupLogin.Controls.Add(this.buttLogin);
			this.groupLogin.Location = new System.Drawing.Point(9, 35);
			this.groupLogin.Margin = new System.Windows.Forms.Padding(2);
			this.groupLogin.Name = "groupLogin";
			this.groupLogin.Padding = new System.Windows.Forms.Padding(2);
			this.groupLogin.Size = new System.Drawing.Size(319, 147);
			this.groupLogin.TabIndex = 14;
			this.groupLogin.TabStop = false;
			this.groupLogin.Text = "Account";
			// 
			// buttRegister
			// 
			this.buttRegister.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.buttRegister.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.buttRegister.Location = new System.Drawing.Point(12, 111);
			this.buttRegister.Margin = new System.Windows.Forms.Padding(2);
			this.buttRegister.Name = "buttRegister";
			this.buttRegister.Size = new System.Drawing.Size(114, 24);
			this.buttRegister.TabIndex = 6;
			this.buttRegister.Text = "&Register...";
			this.buttRegister.UseCompatibleTextRendering = true;
			this.buttRegister.UseVisualStyleBackColor = false;
			this.buttRegister.Click += new System.EventHandler(this.buttRegister_Click);
			// 
			// labelLoginEntering
			// 
			this.labelLoginEntering.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.labelLoginEntering.Location = new System.Drawing.Point(12, 18);
			this.labelLoginEntering.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.labelLoginEntering.Name = "labelLoginEntering";
			this.labelLoginEntering.Size = new System.Drawing.Size(294, 58);
			this.labelLoginEntering.TabIndex = 5;
			this.labelLoginEntering.Text = "Please enter your login to sign in.\r\nIf you don\'t have an account, register one i" +
    "n a minute";
			this.labelLoginEntering.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelLoginEntering.UseCompatibleTextRendering = true;
			// 
			// txtLogin
			// 
			this.txtLogin.Location = new System.Drawing.Point(12, 85);
			this.txtLogin.Margin = new System.Windows.Forms.Padding(2);
			this.txtLogin.Name = "txtLogin";
			this.txtLogin.Size = new System.Drawing.Size(292, 22);
			this.txtLogin.TabIndex = 4;
			this.txtLogin.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtLogin_KeyUp);
			// 
			// buttLogin
			// 
			this.buttLogin.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.buttLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.buttLogin.Location = new System.Drawing.Point(205, 111);
			this.buttLogin.Margin = new System.Windows.Forms.Padding(2);
			this.buttLogin.Name = "buttLogin";
			this.buttLogin.Size = new System.Drawing.Size(99, 24);
			this.buttLogin.TabIndex = 3;
			this.buttLogin.Text = "&Log in";
			this.buttLogin.UseCompatibleTextRendering = true;
			this.buttLogin.UseVisualStyleBackColor = false;
			this.buttLogin.Click += new System.EventHandler(this.buttLogin_Click);
			// 
			// LoginForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(346, 217);
			this.Controls.Add(this.borderPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "LoginForm";
			this.Text = "FloppyVPN";
			this.Load += new System.EventHandler(this.LoginForm_Load);
			this.Controls.SetChildIndex(this.borderPanel, 0);
			this.borderPanel.ResumeLayout(false);
			this.borderPanel.PerformLayout();
			this.windowMenu.ResumeLayout(false);
			this.windowMenu.PerformLayout();
			this.groupLogin.ResumeLayout(false);
			this.groupLogin.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel borderPanel;
		private System.Windows.Forms.GroupBox groupLogin;
		private System.Windows.Forms.Label labelLoginEntering;
		private System.Windows.Forms.TextBox txtLogin;
		private System.Windows.Forms.Button buttLogin;
		private System.Windows.Forms.Button buttRegister;
		private System.Windows.Forms.MenuStrip windowMenu;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem buttCloseLoginForm;
		private System.Windows.Forms.ToolStripMenuItem buttLanguage;
		private System.Windows.Forms.ToolStripMenuItem buttEN;
		private System.Windows.Forms.ToolStripMenuItem buttRU;
		private System.Windows.Forms.ToolStripMenuItem buttUK;
		private System.Windows.Forms.ToolStripMenuItem buttJA;
	}
}