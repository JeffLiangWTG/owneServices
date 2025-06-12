namespace CargoWise.eHub.Products.TWCustoms.TWCServiceSampleClient
{
    partial class frmMain
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.txtConfigXML = new System.Windows.Forms.TextBox();
			this.panel2 = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.txtRegistrationId = new System.Windows.Forms.TextBox();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.txtMessageBody = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.txtAttachmentFormat = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.txtAttachmentAddress = new System.Windows.Forms.TextBox();
			this.txtAttachmentName = new System.Windows.Forms.TextBox();
			this.txtMessageId = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.txtMessageType = new System.Windows.Forms.TextBox();
			this.btnAttachment = new System.Windows.Forms.Button();
			this.panel3 = new System.Windows.Forms.Panel();
			this.Address = new System.Windows.Forms.Label();
			this.AddressTextBox = new System.Windows.Forms.TextBox();
			this.btnClear = new System.Windows.Forms.Button();
			this.btnReceive = new System.Windows.Forms.Button();
			this.btnSend = new System.Windows.Forms.Button();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.groupBox1.SuspendLayout();
			this.panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.txtConfigXML);
			this.groupBox1.Controls.Add(this.panel2);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox1.Location = new System.Drawing.Point(0, 0);
			this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(11, 12, 11, 12);
			this.groupBox1.Size = new System.Drawing.Size(1191, 907);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Registration";
			// 
			// txtConfigXML
			// 
			this.txtConfigXML.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtConfigXML.Location = new System.Drawing.Point(11, 114);
			this.txtConfigXML.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.txtConfigXML.Multiline = true;
			this.txtConfigXML.Name = "txtConfigXML";
			this.txtConfigXML.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtConfigXML.Size = new System.Drawing.Size(1169, 781);
			this.txtConfigXML.TabIndex = 1;
			this.txtConfigXML.Text = resources.GetString("txtConfigXML.Text");
			this.txtConfigXML.TextChanged += new System.EventHandler(this.TxtConfigXML_TextChanged);
			// 
			// panel2
			// 
			this.panel2.AutoSize = true;
			this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panel2.Controls.Add(this.label2);
			this.panel2.Controls.Add(this.label3);
			this.panel2.Controls.Add(this.txtRegistrationId);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(11, 31);
			this.panel2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.panel2.Name = "panel2";
			this.panel2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 12);
			this.panel2.Size = new System.Drawing.Size(1169, 83);
			this.panel2.TabIndex = 12;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(32, 20);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(121, 20);
			this.label2.TabIndex = 9;
			this.label2.Text = "Registration Id :";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(32, 51);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(123, 20);
			this.label3.TabIndex = 10;
			this.label3.Text = "CX_ConfigXML:";
			// 
			// txtRegistrationId
			// 
			this.txtRegistrationId.Location = new System.Drawing.Point(159, 18);
			this.txtRegistrationId.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.txtRegistrationId.Name = "txtRegistrationId";
			this.txtRegistrationId.Size = new System.Drawing.Size(435, 26);
			this.txtRegistrationId.TabIndex = 8;
			this.txtRegistrationId.Text = "BE952818-C240-4377-A8C6-C7DDCF9815DC";
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
			this.splitContainer1.Panel1.Controls.Add(this.panel3);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.txtLog);
			this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(11, 12, 11, 12);
			this.splitContainer1.Size = new System.Drawing.Size(2356, 1858);
			this.splitContainer1.SplitterDistance = 956;
			this.splitContainer1.SplitterWidth = 5;
			this.splitContainer1.TabIndex = 3;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.splitContainer2.Name = "splitContainer2";
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.groupBox2);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.groupBox1);
			this.splitContainer2.Size = new System.Drawing.Size(2356, 907);
			this.splitContainer2.SplitterDistance = 1161;
			this.splitContainer2.TabIndex = 4;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.txtMessageBody);
			this.groupBox2.Controls.Add(this.panel1);
			this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox2.Location = new System.Drawing.Point(0, 0);
			this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Padding = new System.Windows.Forms.Padding(11, 12, 11, 12);
			this.groupBox2.Size = new System.Drawing.Size(1161, 907);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Message";
			// 
			// txtMessageBody
			// 
			this.txtMessageBody.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtMessageBody.Location = new System.Drawing.Point(11, 152);
			this.txtMessageBody.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.txtMessageBody.Multiline = true;
			this.txtMessageBody.Name = "txtMessageBody";
			this.txtMessageBody.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtMessageBody.Size = new System.Drawing.Size(1139, 743);
			this.txtMessageBody.TabIndex = 2;
			this.txtMessageBody.Text = resources.GetString("txtMessageBody.Text");
			// 
			// panel1
			// 
			this.panel1.AutoSize = true;
			this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panel1.Controls.Add(this.txtAttachmentFormat);
			this.panel1.Controls.Add(this.label6);
			this.panel1.Controls.Add(this.label7);
			this.panel1.Controls.Add(this.label8);
			this.panel1.Controls.Add(this.txtAttachmentAddress);
			this.panel1.Controls.Add(this.txtAttachmentName);
			this.panel1.Controls.Add(this.txtMessageId);
			this.panel1.Controls.Add(this.label5);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.label4);
			this.panel1.Controls.Add(this.txtMessageType);
			this.panel1.Controls.Add(this.btnAttachment);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(11, 31);
			this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 12);
			this.panel1.Size = new System.Drawing.Size(1139, 121);
			this.panel1.TabIndex = 11;
			// 
			// txtAttachmentFormat
			// 
			this.txtAttachmentFormat.Location = new System.Drawing.Point(670, 79);
			this.txtAttachmentFormat.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.txtAttachmentFormat.Name = "txtAttachmentFormat";
			this.txtAttachmentFormat.Size = new System.Drawing.Size(185, 26);
			this.txtAttachmentFormat.TabIndex = 16;
			this.txtAttachmentFormat.Text = "PDF";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(464, 82);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(193, 20);
			this.label6.TabIndex = 15;
			this.label6.Text = "Attachement File Format :";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(464, 19);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(155, 20);
			this.label7.TabIndex = 13;
			this.label7.Text = "Attachement Name :";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(464, 51);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(138, 20);
			this.label8.TabIndex = 14;
			this.label8.Text = "Attachement File :";
			// 
			// txtAttachmentAddress
			// 
			this.txtAttachmentAddress.Location = new System.Drawing.Point(670, 49);
			this.txtAttachmentAddress.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.txtAttachmentAddress.Name = "txtAttachmentAddress";
			this.txtAttachmentAddress.Size = new System.Drawing.Size(399, 26);
			this.txtAttachmentAddress.TabIndex = 12;
			// 
			// txtAttachmentName
			// 
			this.txtAttachmentName.Location = new System.Drawing.Point(670, 19);
			this.txtAttachmentName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.txtAttachmentName.Name = "txtAttachmentName";
			this.txtAttachmentName.Size = new System.Drawing.Size(185, 26);
			this.txtAttachmentName.TabIndex = 11;
			this.txtAttachmentName.Text = "1";
			// 
			// txtMessageId
			// 
			this.txtMessageId.Location = new System.Drawing.Point(160, 16);
			this.txtMessageId.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.txtMessageId.Name = "txtMessageId";
			this.txtMessageId.Size = new System.Drawing.Size(275, 26);
			this.txtMessageId.TabIndex = 5;
			this.txtMessageId.Text = "1";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(30, 79);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(122, 20);
			this.label5.TabIndex = 10;
			this.label5.Text = "Message Body :";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(30, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 20);
			this.label1.TabIndex = 7;
			this.label1.Text = "Message Id :";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(30, 51);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(120, 20);
			this.label4.TabIndex = 9;
			this.label4.Text = "Message Type :";
			// 
			// txtMessageType
			// 
			this.txtMessageType.Location = new System.Drawing.Point(160, 48);
			this.txtMessageType.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.txtMessageType.Name = "txtMessageType";
			this.txtMessageType.Size = new System.Drawing.Size(275, 26);
			this.txtMessageType.TabIndex = 8;
			this.txtMessageType.Text = "N5203";
			// 
			// btnAttachment
			// 
			this.btnAttachment.Location = new System.Drawing.Point(1068, 48);
			this.btnAttachment.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnAttachment.Name = "btnAttachment";
			this.btnAttachment.Size = new System.Drawing.Size(34, 30);
			this.btnAttachment.TabIndex = 17;
			this.btnAttachment.Text = "...";
			this.btnAttachment.UseVisualStyleBackColor = true;
			this.btnAttachment.Click += new System.EventHandler(this.BtnAttachment_Click);
			// 
			// panel3
			// 
			this.panel3.AutoSize = true;
			this.panel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panel3.Controls.Add(this.Address);
			this.panel3.Controls.Add(this.AddressTextBox);
			this.panel3.Controls.Add(this.btnClear);
			this.panel3.Controls.Add(this.btnReceive);
			this.panel3.Controls.Add(this.btnSend);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel3.Location = new System.Drawing.Point(0, 907);
			this.panel3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(2356, 49);
			this.panel3.TabIndex = 8;
			// 
			// Address
			// 
			this.Address.AutoSize = true;
			this.Address.Location = new System.Drawing.Point(1348, 14);
			this.Address.Name = "Address";
			this.Address.Size = new System.Drawing.Size(72, 20);
			this.Address.TabIndex = 12;
			this.Address.Text = "Address:";
			// 
			// AddressTextBox
			// 
			this.AddressTextBox.Location = new System.Drawing.Point(1459, 11);
			this.AddressTextBox.Name = "AddressTextBox";
			this.AddressTextBox.Size = new System.Drawing.Size(311, 26);
			this.AddressTextBox.TabIndex = 11;
			this.AddressTextBox.Text = "http://au2sp-stwc-401:9090/";
			// 
			// btnClear
			// 
			this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClear.Location = new System.Drawing.Point(1798, 4);
			this.btnClear.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(171, 41);
			this.btnClear.TabIndex = 10;
			this.btnClear.Text = "Clear Log";
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
			// 
			// btnReceive
			// 
			this.btnReceive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnReceive.Location = new System.Drawing.Point(2004, 4);
			this.btnReceive.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnReceive.Name = "btnReceive";
			this.btnReceive.Size = new System.Drawing.Size(171, 41);
			this.btnReceive.TabIndex = 9;
			this.btnReceive.Text = "Trigger Receive";
			this.btnReceive.UseVisualStyleBackColor = true;
			this.btnReceive.Click += new System.EventHandler(this.BtnReceive_Click);
			// 
			// btnSend
			// 
			this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSend.Location = new System.Drawing.Point(2174, 4);
			this.btnSend.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(171, 41);
			this.btnSend.TabIndex = 8;
			this.btnSend.Text = "Send";
			this.btnSend.UseVisualStyleBackColor = true;
			this.btnSend.Click += new System.EventHandler(this.BtnSend_Click);
			// 
			// txtLog
			// 
			this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtLog.Location = new System.Drawing.Point(11, 12);
			this.txtLog.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog.Size = new System.Drawing.Size(2334, 873);
			this.txtLog.TabIndex = 1;
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(2356, 1858);
			this.Controls.Add(this.splitContainer1);
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "frmMain";
			this.Text = "Form1";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtRegistrationId;
        private System.Windows.Forms.TextBox txtConfigXML;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtMessageId;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtMessageType;
        private System.Windows.Forms.TextBox txtMessageBody;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnReceive;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TextBox txtAttachmentFormat;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtAttachmentAddress;
        private System.Windows.Forms.TextBox txtAttachmentName;
        private System.Windows.Forms.Button btnAttachment;
		private System.Windows.Forms.Label Address;
		private System.Windows.Forms.TextBox AddressTextBox;
	}
}

