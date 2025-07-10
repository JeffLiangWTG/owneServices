using CargoWiseOne.ResourceStrings;
namespace Enterprise.Freight.Forwarding.Documents.GUI.CertificateOfOrigin
{
	partial class AddressFormPopup
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
		private new void InitializeComponent()
		{
            this.buttonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.messageLabel = new Enterprise.ZArchitecture.ZLabel();
            this.okCancelButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.formPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.nameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.address1TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.address2TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.cityTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.stateTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.postCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.countryTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.checkboxPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.sameAsExporterCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.unknownCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.excludeFromCertificateCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.buttonPanel.SuspendLayout();
            this.okCancelButtonPanel.SuspendLayout();
            this.formPanel.SuspendLayout();
            this.checkboxPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 386, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 24, true);
            // 
            // buttonPanel
            // 
            this.buttonPanel.Controls.Add(this.messageLabel);
            this.buttonPanel.Controls.Add(this.okCancelButtonPanel);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 318, true);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 68, true);
            this.buttonPanel.TabIndex = 3;
			// 
			// messageLabel
			//
			this.messageLabel.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("AddressFormPopup|8A888291-0973-4372-AC36-3BE1595691B5", "To add a Producer to the Certificate please do so in the Address tab of the shipment.");
			this.messageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.messageLabel.IsFontBold = true;
            this.messageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 22, true);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(214, 41, true);
            this.messageLabel.TabIndex = 2;
            this.messageLabel.UseMnemonic = false;
            // 
            // okCancelButtonPanel
            // 
            this.okCancelButtonPanel.Controls.Add(this.cancelButton);
            this.okCancelButtonPanel.Controls.Add(this.okButton);
            this.okCancelButtonPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.okCancelButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 0, true);
            this.okCancelButtonPanel.Name = "okCancelButtonPanel";
            this.okCancelButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 68, true);
            this.okCancelButtonPanel.TabIndex = 1;
            // 
            // cancelButton
            // 
			this.cancelButton.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("AddressFormPopup|8B684AB3-792B-4904-8455-23AB1D8D0B6E", "Cancel");
            this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 39, true);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.cancelButton.TabIndex = 0;
            this.cancelButton.ToolTipCaption = null;
            this.cancelButton.Click += new System.EventHandler(this.CancelBtn_Click);
			// 
			// okButton
			// 
			this.okButton.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("AddressFormPopup|E158315A-28E8-407D-B577-9AC57842F174", "OK");
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 39, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 1;
			this.okButton.ToolTipCaption = null;
			this.okButton.Click += new System.EventHandler(this.OkBtn_Click);
			// 
			// formPanel
			// 
			this.formPanel.Controls.Add(this.nameTextBox);
            this.formPanel.Controls.Add(this.address1TextBox);
            this.formPanel.Controls.Add(this.address2TextBox);
            this.formPanel.Controls.Add(this.cityTextBox);
            this.formPanel.Controls.Add(this.stateTextBox);
            this.formPanel.Controls.Add(this.postCodeTextBox);
            this.formPanel.Controls.Add(this.countryTextBox);
            this.formPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.formPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.formPanel.Name = "formPanel";
            this.formPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 318, true);
            this.formPanel.TabIndex = 4;
            // 
            // nameTextBox
            // 
            this.nameTextBox.CaptionResourceString = Res.GetData("AddressFormPopup|A56CF2B9-FF94-4D00-86E3-324E53869F20", "Name:");
            this.nameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 26, true);
            this.nameTextBox.Name = "nameTextBox";
			this.nameTextBox.Enabled = false;
			this.nameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 18, true);
            this.nameTextBox.TabIndex = 1;
            // 
            // address1TextBox
            // 
			this.address1TextBox.CaptionResourceString = Res.GetData("AddressFormPopup|CC505115-25FB-49C3-AFB8-956DA8694B0C", "Address 1:");
            this.address1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 71, true);
            this.address1TextBox.Name = "address1TextBox";
			this.address1TextBox.Enabled = false;
			this.address1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 18, true);
            this.address1TextBox.TabIndex = 2;
            // 
            // address2TextBox
            // 
			this.address2TextBox.CaptionResourceString = Res.GetData("AddressFormPopup|EE6BE1CA-C0D1-4D13-9683-9A976F4A2DF3", "Address 2:");
            this.address2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 116, true);
            this.address2TextBox.Name = "address2TextBox";
			this.address2TextBox.Enabled = false;
			this.address2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 18, true);
            this.address2TextBox.TabIndex = 3;
            // 
            // cityTextBox
            // 
			this.cityTextBox.CaptionResourceString = Res.GetData("AddressFormPopup|95B8587A-C982-4F1E-8FBE-ADC8DC1EDD34", "City:");
            this.cityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 161, true);
            this.cityTextBox.Name = "cityTextBox";
			this.cityTextBox.Enabled = false;
			this.cityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 18, true);
            this.cityTextBox.TabIndex = 4;
            // 
            // stateTextBox
            // 
			this.stateTextBox.CaptionResourceString = Res.GetData("AddressFormPopup|3EAEF663-C7DC-44E1-AECE-C076294D0B4E", "State:");
            this.stateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 206, true);
            this.stateTextBox.Name = "stateTextBox";
			this.stateTextBox.Enabled = false;
			this.stateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 18, true);
            this.stateTextBox.TabIndex = 5;
            // 
            // postCodeTextBox
            // 
			this.postCodeTextBox.CaptionResourceString = Res.GetData("AddressFormPopup|D094C60A-5BFE-403C-A6E5-66CDBECED01D", "Postcode:");
            this.postCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 251, true);
            this.postCodeTextBox.Name = "postCodeTextBox";
			this.postCodeTextBox.Enabled = false;
			this.postCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 18, true);
            this.postCodeTextBox.TabIndex = 6;
            // 
            // countryFindBox
            // 
			this.countryTextBox.CaptionResourceString = Res.GetData("AddressFormPopup|CF0D02FE-5416-46CC-BEFB-3CF473698838", "Country:");
            this.countryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 296, true);
            this.countryTextBox.Name = "countryFindBox";
			this.countryTextBox.Enabled = false;
            this.countryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 18, true);
            this.countryTextBox.TabIndex = 7;
            // 
            // checkboxPanel
            // 
            this.checkboxPanel.Controls.Add(this.sameAsExporterCheckbox);
            this.checkboxPanel.Controls.Add(this.unknownCheckbox);
            this.checkboxPanel.Controls.Add(this.excludeFromCertificateCheckbox);
            this.checkboxPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.checkboxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 0, true);
            this.checkboxPanel.Name = "checkboxPanel";
            this.checkboxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 318, true);
            this.checkboxPanel.TabIndex = 5;
            // 
            // sameAsExporterCheckbox
            // 
			this.sameAsExporterCheckbox.CaptionResourceString = Res.GetData("AddressFormPopup|7FE4CD6C-D0EB-4309-9B9C-BBBF9E06EDB2", "Same As Exporter");
            this.sameAsExporterCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 26, true);
            this.sameAsExporterCheckbox.Name = "sameAsExporterCheckbox";
            this.sameAsExporterCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 24, true);
            this.sameAsExporterCheckbox.TabIndex = 1;
            this.sameAsExporterCheckbox.CheckedChanged += new System.EventHandler(this.sameAsExporterCheckbox_CheckedChanged);
            // 
            // unknownCheckbox
            // 
			this.unknownCheckbox.CaptionResourceString = Res.GetData("AddressFormPopup|03EBF54D-D5D3-42AF-827B-590D249C21F3", "Unknown");
            this.unknownCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 71, true);
            this.unknownCheckbox.Name = "unknownCheckbox";
            this.unknownCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
            this.unknownCheckbox.TabIndex = 2;
            this.unknownCheckbox.CheckedChanged += new System.EventHandler(this.unknownCheckbox_CheckedChanged);
            // 
            // excludeFromNzcftaCheckbox
            // 
			this.excludeFromCertificateCheckbox.CaptionResourceString = Res.GetData("AddressFormPopup|DE59C257-2789-4332-A745-463A50F4F036", "Exclude from {0} PDF");
            this.excludeFromCertificateCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 116, true);
            this.excludeFromCertificateCheckbox.Name = "excludeFromCertificateCheckbox";
            this.excludeFromCertificateCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 24, true);
            this.excludeFromCertificateCheckbox.TabIndex = 3;
            // 
            // AddressFormPopup
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 410, true);
            this.Controls.Add(this.checkboxPanel);
            this.Controls.Add(this.formPanel);
            this.Controls.Add(this.buttonPanel);
            this.Name = "AddressFormPopup";
            this.Text = "AddressFormPopup";
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.buttonPanel, 0);
            this.Controls.SetChildIndex(this.formPanel, 0);
            this.Controls.SetChildIndex(this.checkboxPanel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.buttonPanel.ResumeLayout(false);
            this.buttonPanel.PerformLayout();
            this.okCancelButtonPanel.ResumeLayout(false);
            this.okCancelButtonPanel.PerformLayout();
            this.formPanel.ResumeLayout(false);
            this.formPanel.PerformLayout();
            this.checkboxPanel.ResumeLayout(false);
            this.checkboxPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel buttonPanel;
		private ZArchitecture.GUI.ZPanel okCancelButtonPanel;
		private ZArchitecture.GUI.ZPanel formPanel;
		private ZArchitecture.GUI.ZPanel checkboxPanel;
		protected ZArchitecture.GUI.ZButton cancelButton;
		protected ZArchitecture.GUI.ZButton okButton;
		protected ZArchitecture.ZTextBox nameTextBox;
		protected ZArchitecture.ZTextBox address1TextBox;
		protected ZArchitecture.ZTextBox address2TextBox;
		protected ZArchitecture.ZTextBox cityTextBox;
		protected ZArchitecture.ZTextBox stateTextBox;
		protected ZArchitecture.ZTextBox postCodeTextBox;
		protected ZArchitecture.ZTextBox countryTextBox;
		protected ZArchitecture.GUI.ZCheckBox sameAsExporterCheckbox;
		protected ZArchitecture.GUI.ZCheckBox unknownCheckbox;
		protected ZArchitecture.GUI.ZCheckBox excludeFromCertificateCheckbox;
		private ZArchitecture.ZLabel messageLabel;
	}
}
