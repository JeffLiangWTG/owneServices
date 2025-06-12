namespace CargoWise.eAdaptorSampleWebClient
{
	partial class MainForm
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
			this.FilePath = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btMessageFileBrowse = new System.Windows.Forms.Button();
			this.MessageFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.label2 = new System.Windows.Forms.Label();
			this.SenderId = new System.Windows.Forms.TextBox();
			this.Password = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.RecipientId = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.ServiceAddress = new System.Windows.Forms.TextBox();
			this.PingCommand = new System.Windows.Forms.Button();
			this.NotificationList = new System.Windows.Forms.ListBox();
			this.SendMessage = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// FilePath
			// 
			this.FilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FilePath.Location = new System.Drawing.Point(100, 35);
			this.FilePath.Name = "FilePath";
			this.FilePath.ReadOnly = true;
			this.FilePath.Size = new System.Drawing.Size(435, 20);
			this.FilePath.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(10, 39);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(69, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Message File";
			// 
			// btMessageFileBrowse
			// 
			this.btMessageFileBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btMessageFileBrowse.Location = new System.Drawing.Point(542, 32);
			this.btMessageFileBrowse.Name = "btMessageFileBrowse";
			this.btMessageFileBrowse.Size = new System.Drawing.Size(75, 23);
			this.btMessageFileBrowse.TabIndex = 2;
			this.btMessageFileBrowse.Text = "Browse";
			this.btMessageFileBrowse.UseVisualStyleBackColor = true;
			this.btMessageFileBrowse.Click += new System.EventHandler(this.btMessageFileBrowse_Click);
			// 
			// MessageFileDialog
			// 
			this.MessageFileDialog.AddExtension = false;
			this.MessageFileDialog.DefaultExt = "xml";
			this.MessageFileDialog.Filter = "XmlInterchange files|*.xml";
			this.MessageFileDialog.RestoreDirectory = true;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(10, 98);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(53, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Sender Id";
			// 
			// SenderId
			// 
			this.SenderId.Location = new System.Drawing.Point(100, 93);
			this.SenderId.Name = "SenderId";
			this.SenderId.Size = new System.Drawing.Size(228, 20);
			this.SenderId.TabIndex = 4;
			this.SenderId.Text = "Sender Id";
			// 
			// Password
			// 
			this.Password.Location = new System.Drawing.Point(100, 122);
			this.Password.Name = "Password";
			this.Password.Size = new System.Drawing.Size(228, 20);
			this.Password.TabIndex = 5;
			this.Password.Text = "Password";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(10, 127);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(53, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Password";
			// 
			// RecipientId
			// 
			this.RecipientId.Location = new System.Drawing.Point(100, 64);
			this.RecipientId.Name = "RecipientId";
			this.RecipientId.Size = new System.Drawing.Size(228, 20);
			this.RecipientId.TabIndex = 3;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(10, 68);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(64, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Recipient Id";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(10, 9);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(84, 13);
			this.label5.TabIndex = 9;
			this.label5.Text = "Service Address";
			// 
			// ServiceAddress
			// 
			this.ServiceAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ServiceAddress.Location = new System.Drawing.Point(100, 6);
			this.ServiceAddress.Name = "ServiceAddress";
			this.ServiceAddress.Size = new System.Drawing.Size(435, 20);
			this.ServiceAddress.TabIndex = 0;
			// 
			// PingCommand
			// 
			this.PingCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PingCommand.Location = new System.Drawing.Point(542, 4);
			this.PingCommand.Name = "PingCommand";
			this.PingCommand.Size = new System.Drawing.Size(75, 23);
			this.PingCommand.TabIndex = 1;
			this.PingCommand.Text = "Ping";
			this.PingCommand.UseVisualStyleBackColor = true;
			this.PingCommand.Click += new System.EventHandler(this.PingCommand_Click);
			// 
			// NotificationList
			// 
			this.NotificationList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.NotificationList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.NotificationList.FormattingEnabled = true;
			this.NotificationList.HorizontalScrollbar = true;
			this.NotificationList.Location = new System.Drawing.Point(12, 157);
			this.NotificationList.Name = "NotificationList";
			this.NotificationList.Size = new System.Drawing.Size(605, 277);
			this.NotificationList.TabIndex = 7;
			this.NotificationList.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.NotificationList_DrawItem);
			// 
			// SendMessage
			// 
			this.SendMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.SendMessage.Location = new System.Drawing.Point(350, 62);
			this.SendMessage.Name = "SendMessage";
			this.SendMessage.Size = new System.Drawing.Size(107, 80);
			this.SendMessage.TabIndex = 6;
			this.SendMessage.Text = "Send";
			this.SendMessage.UseVisualStyleBackColor = true;
			this.SendMessage.Click += new System.EventHandler(this.SendMessage_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(624, 442);
			this.Controls.Add(this.SendMessage);
			this.Controls.Add(this.NotificationList);
			this.Controls.Add(this.PingCommand);
			this.Controls.Add(this.ServiceAddress);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.RecipientId);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.Password);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.SenderId);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btMessageFileBrowse);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.FilePath);
			this.MinimumSize = new System.Drawing.Size(640, 480);
			this.Name = "MainForm";
			this.Text = "eAdaptor Sample Client";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox FilePath;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btMessageFileBrowse;
		private System.Windows.Forms.OpenFileDialog MessageFileDialog;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox SenderId;
		private System.Windows.Forms.TextBox Password;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox RecipientId;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox ServiceAddress;
		private System.Windows.Forms.Button PingCommand;
		private System.Windows.Forms.ListBox NotificationList;
		private System.Windows.Forms.Button SendMessage;
	}
}

