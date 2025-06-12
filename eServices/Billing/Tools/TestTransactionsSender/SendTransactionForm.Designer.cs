namespace CargoWise.eServices.Billing.TestTransactionsSender
{
	partial class SendTransactionForm
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
			this.serverComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label17 = new System.Windows.Forms.Label();
			this.branchTextBox = new System.Windows.Forms.TextBox();
			this.label16 = new System.Windows.Forms.Label();
			this.versionTextBox = new System.Windows.Forms.TextBox();
			this.label15 = new System.Windows.Forms.Label();
			this.reference5TextBox = new System.Windows.Forms.TextBox();
			this.label14 = new System.Windows.Forms.Label();
			this.categoryTextBox = new System.Windows.Forms.TextBox();
			this.label12 = new System.Windows.Forms.Label();
			this.serviceOccuredUTCDateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.label11 = new System.Windows.Forms.Label();
			this.reference4TextBox = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.reference3TextBox = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.reference2TextBox = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.reference1TextBox = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.reportingSourceTextBox = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.billableCountTextBox = new System.Windows.Forms.TextBox();
			this.clientNumberTextBox = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.clientIDTextBox = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.priceItemCodeTextBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.clientStaffCodeTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.sendButton = new System.Windows.Forms.Button();
			this.logRichTextBox = new System.Windows.Forms.RichTextBox();
			this.label13 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.MessageTrackingIDTextBox = new System.Windows.Forms.TextBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// serverComboBox
			// 
			this.serverComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.serverComboBox.FormattingEnabled = true;
			this.serverComboBox.Items.AddRange(new object[] {
            "Test",
            "Production"});
			this.serverComboBox.Location = new System.Drawing.Point(81, 15);
			this.serverComboBox.Name = "serverComboBox";
			this.serverComboBox.Size = new System.Drawing.Size(160, 28);
			this.serverComboBox.TabIndex = 0;
			this.serverComboBox.SelectedIndexChanged += new System.EventHandler(this.OnServerComboBoxSelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(14, 18);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(59, 20);
			this.label1.TabIndex = 1;
			this.label1.Text = "Server:";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label18);
			this.groupBox1.Controls.Add(this.MessageTrackingIDTextBox);
			this.groupBox1.Controls.Add(this.label17);
			this.groupBox1.Controls.Add(this.branchTextBox);
			this.groupBox1.Controls.Add(this.label16);
			this.groupBox1.Controls.Add(this.versionTextBox);
			this.groupBox1.Controls.Add(this.label15);
			this.groupBox1.Controls.Add(this.reference5TextBox);
			this.groupBox1.Controls.Add(this.label14);
			this.groupBox1.Controls.Add(this.categoryTextBox);
			this.groupBox1.Controls.Add(this.label12);
			this.groupBox1.Controls.Add(this.serviceOccuredUTCDateTimePicker);
			this.groupBox1.Controls.Add(this.label11);
			this.groupBox1.Controls.Add(this.reference4TextBox);
			this.groupBox1.Controls.Add(this.label10);
			this.groupBox1.Controls.Add(this.reference3TextBox);
			this.groupBox1.Controls.Add(this.label9);
			this.groupBox1.Controls.Add(this.reference2TextBox);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.reference1TextBox);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.reportingSourceTextBox);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.billableCountTextBox);
			this.groupBox1.Controls.Add(this.clientNumberTextBox);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.clientIDTextBox);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.priceItemCodeTextBox);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.clientStaffCodeTextBox);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Location = new System.Drawing.Point(14, 52);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(408, 618);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Transaction";
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(98, 145);
			this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(64, 20);
			this.label17.TabIndex = 29;
			this.label17.Text = "Branch:";
			// 
			// branchTextBox
			// 
			this.branchTextBox.Location = new System.Drawing.Point(170, 142);
			this.branchTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.branchTextBox.Name = "branchTextBox";
			this.branchTextBox.Size = new System.Drawing.Size(112, 26);
			this.branchTextBox.TabIndex = 28;
			this.branchTextBox.Text = "TST";
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(96, 541);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(67, 20);
			this.label16.TabIndex = 27;
			this.label16.Text = "Version:";
			// 
			// versionTextBox
			// 
			this.versionTextBox.Location = new System.Drawing.Point(170, 538);
			this.versionTextBox.Name = "versionTextBox";
			this.versionTextBox.Size = new System.Drawing.Size(112, 26);
			this.versionTextBox.TabIndex = 26;
			this.versionTextBox.Text = "0";
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(66, 466);
			this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(97, 20);
			this.label15.TabIndex = 25;
			this.label15.Text = "Reference5:";
			// 
			// reference5TextBox
			// 
			this.reference5TextBox.Location = new System.Drawing.Point(170, 463);
			this.reference5TextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.reference5TextBox.Name = "reference5TextBox";
			this.reference5TextBox.Size = new System.Drawing.Size(224, 26);
			this.reference5TextBox.TabIndex = 24;
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(87, 31);
			this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(77, 20);
			this.label14.TabIndex = 23;
			this.label14.Text = "Category:";
			// 
			// categoryTextBox
			// 
			this.categoryTextBox.Location = new System.Drawing.Point(170, 28);
			this.categoryTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.categoryTextBox.Name = "categoryTextBox";
			this.categoryTextBox.Size = new System.Drawing.Size(112, 26);
			this.categoryTextBox.TabIndex = 22;
			this.categoryTextBox.Text = "TST";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(0, 505);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(157, 20);
			this.label12.TabIndex = 21;
			this.label12.Text = "ServiceOccuredUTC:";
			// 
			// serviceOccuredUTCDateTimePicker
			// 
			this.serviceOccuredUTCDateTimePicker.CustomFormat = "dd/MM/yyyy HH:mm:ss";
			this.serviceOccuredUTCDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.serviceOccuredUTCDateTimePicker.Location = new System.Drawing.Point(170, 502);
			this.serviceOccuredUTCDateTimePicker.Name = "serviceOccuredUTCDateTimePicker";
			this.serviceOccuredUTCDateTimePicker.Size = new System.Drawing.Size(224, 26);
			this.serviceOccuredUTCDateTimePicker.TabIndex = 20;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(66, 428);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(97, 20);
			this.label11.TabIndex = 19;
			this.label11.Text = "Reference4:";
			// 
			// reference4TextBox
			// 
			this.reference4TextBox.Location = new System.Drawing.Point(170, 425);
			this.reference4TextBox.Name = "reference4TextBox";
			this.reference4TextBox.Size = new System.Drawing.Size(224, 26);
			this.reference4TextBox.TabIndex = 18;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(66, 394);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(97, 20);
			this.label10.TabIndex = 17;
			this.label10.Text = "Reference3:";
			// 
			// reference3TextBox
			// 
			this.reference3TextBox.Location = new System.Drawing.Point(170, 391);
			this.reference3TextBox.Name = "reference3TextBox";
			this.reference3TextBox.Size = new System.Drawing.Size(224, 26);
			this.reference3TextBox.TabIndex = 16;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(66, 358);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(97, 20);
			this.label9.TabIndex = 15;
			this.label9.Text = "Reference2:";
			// 
			// reference2TextBox
			// 
			this.reference2TextBox.Location = new System.Drawing.Point(170, 355);
			this.reference2TextBox.Name = "reference2TextBox";
			this.reference2TextBox.Size = new System.Drawing.Size(224, 26);
			this.reference2TextBox.TabIndex = 14;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(66, 323);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(97, 20);
			this.label8.TabIndex = 13;
			this.label8.Text = "Reference1:";
			// 
			// reference1TextBox
			// 
			this.reference1TextBox.Location = new System.Drawing.Point(170, 320);
			this.reference1TextBox.Name = "reference1TextBox";
			this.reference1TextBox.Size = new System.Drawing.Size(224, 26);
			this.reference1TextBox.TabIndex = 12;
			this.reference1TextBox.Text = "REF1";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(30, 254);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(134, 20);
			this.label7.TabIndex = 11;
			this.label7.Text = "ReportingSource:";
			// 
			// reportingSourceTextBox
			// 
			this.reportingSourceTextBox.Location = new System.Drawing.Point(170, 251);
			this.reportingSourceTextBox.Name = "reportingSourceTextBox";
			this.reportingSourceTextBox.Size = new System.Drawing.Size(112, 26);
			this.reportingSourceTextBox.TabIndex = 10;
			this.reportingSourceTextBox.Text = "TTS";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(57, 288);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(106, 20);
			this.label6.TabIndex = 9;
			this.label6.Text = "BillableCount:";
			// 
			// billableCountTextBox
			// 
			this.billableCountTextBox.Location = new System.Drawing.Point(170, 285);
			this.billableCountTextBox.Name = "billableCountTextBox";
			this.billableCountTextBox.Size = new System.Drawing.Size(112, 26);
			this.billableCountTextBox.TabIndex = 8;
			this.billableCountTextBox.Text = "1";
			// 
			// clientNumberTextBox
			// 
			this.clientNumberTextBox.Location = new System.Drawing.Point(170, 180);
			this.clientNumberTextBox.Name = "clientNumberTextBox";
			this.clientNumberTextBox.Size = new System.Drawing.Size(112, 26);
			this.clientNumberTextBox.TabIndex = 7;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(56, 183);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(109, 20);
			this.label5.TabIndex = 6;
			this.label5.Text = "ClientNumber:";
			// 
			// clientIDTextBox
			// 
			this.clientIDTextBox.Location = new System.Drawing.Point(170, 103);
			this.clientIDTextBox.Name = "clientIDTextBox";
			this.clientIDTextBox.Size = new System.Drawing.Size(112, 26);
			this.clientIDTextBox.TabIndex = 5;
			this.clientIDTextBox.Text = "TSTTSTTST";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(94, 106);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(70, 20);
			this.label4.TabIndex = 4;
			this.label4.Text = "ClientID:";
			// 
			// priceItemCodeTextBox
			// 
			this.priceItemCodeTextBox.Location = new System.Drawing.Point(170, 66);
			this.priceItemCodeTextBox.Name = "priceItemCodeTextBox";
			this.priceItemCodeTextBox.Size = new System.Drawing.Size(112, 26);
			this.priceItemCodeTextBox.TabIndex = 3;
			this.priceItemCodeTextBox.Text = "TST";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(46, 69);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(118, 20);
			this.label3.TabIndex = 2;
			this.label3.Text = "PriceItemCode:";
			// 
			// clientStaffCodeTextBox
			// 
			this.clientStaffCodeTextBox.Location = new System.Drawing.Point(170, 215);
			this.clientStaffCodeTextBox.Name = "clientStaffCodeTextBox";
			this.clientStaffCodeTextBox.Size = new System.Drawing.Size(112, 26);
			this.clientStaffCodeTextBox.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(40, 218);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(126, 20);
			this.label2.TabIndex = 0;
			this.label2.Text = "ClientStaffCode:";
			// 
			// sendButton
			// 
			this.sendButton.Location = new System.Drawing.Point(338, 678);
			this.sendButton.Name = "sendButton";
			this.sendButton.Size = new System.Drawing.Size(84, 29);
			this.sendButton.TabIndex = 3;
			this.sendButton.Text = "Send";
			this.sendButton.UseVisualStyleBackColor = true;
			this.sendButton.Click += new System.EventHandler(this.OnSendButtonClick);
			// 
			// logRichTextBox
			// 
			this.logRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.logRichTextBox.Location = new System.Drawing.Point(428, 58);
			this.logRichTextBox.Name = "logRichTextBox";
			this.logRichTextBox.Size = new System.Drawing.Size(439, 628);
			this.logRichTextBox.TabIndex = 4;
			this.logRichTextBox.Text = "";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(424, 34);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(48, 20);
			this.label13.TabIndex = 5;
			this.label13.Text = "Logs:";
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(8, 577);
			this.label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(155, 20);
			this.label18.TabIndex = 31;
			this.label18.Text = "MessageTrackingID:";
			// 
			// MessageTrackingIDTextBox
			// 
			this.MessageTrackingIDTextBox.Location = new System.Drawing.Point(170, 574);
			this.MessageTrackingIDTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MessageTrackingIDTextBox.Name = "MessageTrackingIDTextBox";
			this.MessageTrackingIDTextBox.Size = new System.Drawing.Size(224, 26);
			this.MessageTrackingIDTextBox.TabIndex = 30;
			// 
			// SendTransactionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(882, 714);
			this.Controls.Add(this.label13);
			this.Controls.Add(this.logRichTextBox);
			this.Controls.Add(this.sendButton);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.serverComboBox);
			this.MinimumSize = new System.Drawing.Size(895, 564);
			this.Name = "SendTransactionForm";
			this.ShowIcon = false;
			this.Text = "Send Test Transaction";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox serverComboBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox clientStaffCodeTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox priceItemCodeTextBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox clientIDTextBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox clientNumberTextBox;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox billableCountTextBox;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox reportingSourceTextBox;
		private System.Windows.Forms.DateTimePicker serviceOccuredUTCDateTimePicker;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.TextBox reference4TextBox;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TextBox reference3TextBox;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox reference2TextBox;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox reference1TextBox;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Button sendButton;
		private System.Windows.Forms.RichTextBox logRichTextBox;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.TextBox categoryTextBox;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.TextBox reference5TextBox;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.TextBox versionTextBox;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.TextBox branchTextBox;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.TextBox MessageTrackingIDTextBox;
	}
}

