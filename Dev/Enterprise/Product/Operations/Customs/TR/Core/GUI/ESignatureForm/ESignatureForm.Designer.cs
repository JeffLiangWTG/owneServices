namespace Enterprise.Customs.TR.GUI
{
	partial class ESignatureForm
	{/// <summary>
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
		protected new void InitializeComponent()
		{
			this.MessageContextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageContentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SignInGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SignButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CurrentUserNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PinCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageContextNationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NationalXMLContentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageContentTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageContextGroupBox.SuspendLayout();
			this.SignInGroupBox.SuspendLayout();
			this.MessageContextNationGroupBox.SuspendLayout();
			this.MessageContentTableLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 550, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(823, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.MessageSendAcknowledgeAndSign);
			// 
			// MessageContextGroupBox
			// 
			this.MessageContextGroupBox.Controls.Add(this.MessageContentsTextBox);
			this.MessageContextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 3, true);
			this.MessageContextGroupBox.Name = "MessageContextGroupBox";
			this.MessageContextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 479, true);
			this.MessageContextGroupBox.TabIndex = 4;
			this.MessageContextGroupBox.TabStop = false;
			this.MessageContextGroupBox.Text = Enterprise.Customs.TR.GUI.Res.GetString("7C3C50B9-BC6E-44C2-8EB0-633C5249F91D", "Message Context");
			this.MessageContextGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			// 
			// MessageContentsTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageContentsTextBox, "MessageTextBeforeSign");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.MessageSendAcknowledgeAndSign)(null)).MessageTextBeforeSign)));
			this.MessageContentsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageContentsTextBox.CaptionResourceString = null;
			this.MessageContentsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageContentsTextBox.Font = new System.Drawing.Font("Courier New", 8F);
			this.MessageContentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageContentsTextBox.Multiline = true;
			this.MessageContentsTextBox.Name = "MessageContentsTextBox";
			this.MessageContentsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageContentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 460, true);
			this.MessageContentsTextBox.TabIndex = 0;
			this.MessageContentsTextBox.TabStop = false;
			// 
			// SignInGroupBox
			// 
			this.SignInGroupBox.Controls.Add(this.SignButton);
			this.SignInGroupBox.Controls.Add(this.CancelButton);
			this.SignInGroupBox.Controls.Add(this.CurrentUserNameTextBox);
			this.SignInGroupBox.Controls.Add(this.PinCodeTextBox);
			this.SignInGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.SignInGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SignInGroupBox.Name = "SignInGroupBox";
			this.SignInGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(823, 61, true);
			this.SignInGroupBox.TabIndex = 3;
			this.SignInGroupBox.TabStop = false;
			this.SignInGroupBox.Text = Enterprise.Customs.TR.GUI.Res.GetString("BB0E462E-AC6E-4993-8CD0-FC6FD0C368F0", "Sign Info");
			// 
			// SignButton
			// 
			this.SignButton.IsCaptionOverridden = true;
			this.SignButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 26, true);
			this.SignButton.Name = "SignButton";
			this.SignButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SignButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SignButton.TabIndex = 2;
			this.SignButton.Text = "Sign";
			this.SignButton.Text = Enterprise.Customs.TR.GUI.Res.GetString("8D15CD96-F79F-413F-83E8-7F23B932FC00", "Sign");
			this.SignButton.ToolTipCaption = null;
			this.SignButton.UseVisualStyleBackColor = true;
			this.SignButton.Click += new System.EventHandler(this.SignButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.IsCaptionOverridden = true;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(731, 26, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 3;
			this.CancelButton.Text = Enterprise.Customs.TR.GUI.Res.GetString("5E3DF37B-4F50-4C02-895A-A1CB23C3D5CE", "&Cancel");
			this.CancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// CurrentUserNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.CurrentUserNameTextBox, "CurrentUserName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.MessageSendAcknowledgeAndSign)(null)).CurrentUserName)));
			this.CurrentUserNameTextBox.CaptionResourceString = null;
			this.CurrentUserNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 29, true);
			this.CurrentUserNameTextBox.Name = "CurrentUserNameTextBox";
			this.CurrentUserNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.CurrentUserNameTextBox.TabIndex = 0;
			this.CurrentUserNameTextBox.TabStop = false;
			// 
			// PinCodeTextBox
			// 
			this.PinCodeTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PinCodeTextBox, "PINCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.MessageSendAcknowledgeAndSign)(null)).PINCode)));
			this.PinCodeTextBox.CaptionResourceString = null;
			this.PinCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 29, true);
			this.PinCodeTextBox.Name = "PinCodeTextBox";
			this.PinCodeTextBox.PasswordChar = '*';
			this.PinCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.PinCodeTextBox.TabIndex = 1;
			this.PinCodeTextBox.TextChanged += new System.EventHandler(this.PinCodeTextBox_TextChanged);
			// 
			// MessageContextNationGroupBox
			// 
			this.MessageContextNationGroupBox.Controls.Add(this.NationalXMLContentsTextBox);
			this.MessageContextNationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 3, true);
			this.MessageContextNationGroupBox.Name = "MessageContextNationGroupBox";
			this.MessageContextNationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 479, true);
			this.MessageContextNationGroupBox.TabIndex = 5;
			this.MessageContextNationGroupBox.TabStop = false;
			this.MessageContextNationGroupBox.Text = Enterprise.Customs.TR.GUI.Res.GetString("CA1A3F68-BF48-4BAF-81E2-5FCD7E44CA75", "Message Context-National XML");
			this.MessageContextNationGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			// 
			// NationalXMLContentsTextBox
			// 
			this.BindingSource.SetBindingMember(this.NationalXMLContentsTextBox, "NationalXMLTextBeforeSign");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.MessageSendAcknowledgeAndSign)(null)).NationalXMLTextBeforeSign)));
			this.NationalXMLContentsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NationalXMLContentsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NationalXMLContentsTextBox.Font = new System.Drawing.Font("Courier New", 8F);
			this.NationalXMLContentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.NationalXMLContentsTextBox.Multiline = true;
			this.NationalXMLContentsTextBox.Name = "NationalXMLContentsTextBox";
			this.NationalXMLContentsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.NationalXMLContentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 460, true);
			this.NationalXMLContentsTextBox.TabIndex = 0;
			this.NationalXMLContentsTextBox.TabStop = false;
			// 
			// MessageContentTableLayoutPanel
			// 
			this.MessageContentTableLayoutPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.MessageContentTableLayoutPanel.ColumnCount = 2;
			this.MessageContentTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.MessageContentTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.MessageContentTableLayoutPanel.Controls.Add(this.MessageContextGroupBox, 0, 0);
			this.MessageContentTableLayoutPanel.Controls.Add(this.MessageContextNationGroupBox, 1, 0);
			this.MessageContentTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 65, true);
			this.MessageContentTableLayoutPanel.Name = "MessageContentTableLayoutPanel";
			this.MessageContentTableLayoutPanel.RowCount = 1;
			this.MessageContentTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.MessageContentTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(796, 471, true);
			this.MessageContentTableLayoutPanel.TabIndex = 1;
			// 
			// ESignatureForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(823, 574, true);
			this.Controls.Add(this.MessageContentTableLayoutPanel);
			this.Controls.Add(this.SignInGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.TR.Business.MessageSendAcknowledgeAndSign);
			this.Name = "ESignatureForm";
			this.Text = Enterprise.Customs.TR.GUI.Res.GetString("ED1C5293-25E7-4C26-A50F-843DA964140D", "E-Signature");
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SignInGroupBox, 0);
			this.Controls.SetChildIndex(this.MessageContentTableLayoutPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageContextGroupBox.ResumeLayout(false);
			this.MessageContextGroupBox.PerformLayout();
			this.SignInGroupBox.ResumeLayout(false);
			this.SignInGroupBox.PerformLayout();
			this.MessageContextNationGroupBox.ResumeLayout(false);
			this.MessageContextNationGroupBox.PerformLayout();
			this.MessageContentTableLayoutPanel.ResumeLayout(false);
			this.MessageContentTableLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MessageContextGroupBox;
		private Enterprise.ZArchitecture.ZTextBox MessageContentsTextBox;
		private ZArchitecture.ZTextBox CurrentUserNameTextBox;
		private ZArchitecture.ZTextBox PinCodeTextBox;
		private ZArchitecture.GUI.ZButton SignButton;
		private new ZArchitecture.GUI.ZButton CancelButton;
		private ZArchitecture.GUI.ZGroupBox SignInGroupBox;
		private ZArchitecture.GUI.ZGroupBox MessageContextNationGroupBox;
		private ZArchitecture.ZTextBox NationalXMLContentsTextBox;
		private CargoWise.Windows.UI.KTableLayoutPanel MessageContentTableLayoutPanel;
	}
}
