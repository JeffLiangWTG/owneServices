namespace Winform_Client
{
	partial class ClientForm
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
			this.sendMessageTextBox = new System.Windows.Forms.TextBox();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.postButton = new System.Windows.Forms.Button();
			this.receivedMessageTextBox = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.uriTextBox = new System.Windows.Forms.TextBox();
			this.passwordTextBox = new System.Windows.Forms.TextBox();
			this.userNameTextBox = new System.Windows.Forms.TextBox();
			this.compressCheckBox = new System.Windows.Forms.CheckBox();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// sendMessageTextBox
			// 
			this.sendMessageTextBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.sendMessageTextBox.Location = new System.Drawing.Point(0, 94);
			this.sendMessageTextBox.Multiline = true;
			this.sendMessageTextBox.Name = "sendMessageTextBox";
			this.sendMessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.sendMessageTextBox.Size = new System.Drawing.Size(1109, 391);
			this.sendMessageTextBox.TabIndex = 1;
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter1.Location = new System.Drawing.Point(0, 485);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(1109, 5);
			this.splitter1.TabIndex = 1;
			this.splitter1.TabStop = false;
			// 
			// postButton
			// 
			this.postButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.postButton.Location = new System.Drawing.Point(0, 490);
			this.postButton.Name = "postButton";
			this.postButton.Size = new System.Drawing.Size(1109, 34);
			this.postButton.TabIndex = 2;
			this.postButton.Text = "Post";
			this.postButton.UseVisualStyleBackColor = true;
			this.postButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// receivedMessageTextBox
			// 
			this.receivedMessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.receivedMessageTextBox.Location = new System.Drawing.Point(0, 524);
			this.receivedMessageTextBox.Multiline = true;
			this.receivedMessageTextBox.Name = "receivedMessageTextBox";
			this.receivedMessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.receivedMessageTextBox.Size = new System.Drawing.Size(1109, 521);
			this.receivedMessageTextBox.TabIndex = 3;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.compressCheckBox);
			this.panel1.Controls.Add(this.label3);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.uriTextBox);
			this.panel1.Controls.Add(this.passwordTextBox);
			this.panel1.Controls.Add(this.userNameTextBox);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1109, 94);
			this.panel1.TabIndex = 0;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(652, 59);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(82, 20);
			this.label3.TabIndex = 4;
			this.label3.Text = "Password:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(259, 59);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(87, 20);
			this.label2.TabIndex = 2;
			this.label2.Text = "Username:";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(23, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(46, 20);
			this.label1.TabIndex = 0;
			this.label1.Text = "URL:";
			// 
			// uriTextBox
			// 
			this.uriTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.uriTextBox.Location = new System.Drawing.Point(75, 12);
			this.uriTextBox.Name = "uriTextBox";
			this.uriTextBox.Size = new System.Drawing.Size(1021, 26);
			this.uriTextBox.TabIndex = 1;
			// 
			// passwordTextBox
			// 
			this.passwordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.passwordTextBox.Location = new System.Drawing.Point(741, 56);
			this.passwordTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.passwordTextBox.Name = "passwordTextBox";
			this.passwordTextBox.PasswordChar = '*';
			this.passwordTextBox.Size = new System.Drawing.Size(355, 26);
			this.passwordTextBox.TabIndex = 5;
			// 
			// userNameTextBox
			// 
			this.userNameTextBox.Location = new System.Drawing.Point(353, 56);
			this.userNameTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.userNameTextBox.Name = "userNameTextBox";
			this.userNameTextBox.Size = new System.Drawing.Size(275, 26);
			this.userNameTextBox.TabIndex = 3;
			// 
			// compressCheckBox
			// 
			this.compressCheckBox.AutoSize = true;
			this.compressCheckBox.Checked = true;
			this.compressCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.compressCheckBox.Location = new System.Drawing.Point(27, 58);
			this.compressCheckBox.Name = "compressCheckBox";
			this.compressCheckBox.Size = new System.Drawing.Size(184, 24);
			this.compressCheckBox.TabIndex = 6;
			this.compressCheckBox.Text = "Compress Response";
			this.compressCheckBox.UseVisualStyleBackColor = true;
			// 
			// ClientForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1109, 1045);
			this.Controls.Add(this.receivedMessageTextBox);
			this.Controls.Add(this.postButton);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.sendMessageTextBox);
			this.Controls.Add(this.panel1);
			this.MinimumSize = new System.Drawing.Size(860, 550);
			this.Name = "ClientForm";
			this.Text = "eAdaptor Test Client";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox sendMessageTextBox;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Button postButton;
		private System.Windows.Forms.TextBox receivedMessageTextBox;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.TextBox passwordTextBox;
		private System.Windows.Forms.TextBox userNameTextBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox uriTextBox;
		private System.Windows.Forms.CheckBox compressCheckBox;
	}
}

