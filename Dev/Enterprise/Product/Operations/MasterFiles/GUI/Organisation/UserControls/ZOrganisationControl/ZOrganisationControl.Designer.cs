namespace Enterprise.MasterFiles.GUI
{
	public partial class ZOrganisationControl
	{

		#region Component Designer generated code

		Enterprise.MasterFiles.GUI.ZOrganisationFindBox.Bare fOrganisationFindBox;
		Enterprise.ZArchitecture.ZLabel FullNameLabel;
		internal Enterprise.ZArchitecture.ZLabel AddressLabel;
		Enterprise.ZArchitecture.ZLabel PhoneLabel;
		Enterprise.ZArchitecture.ZLabel MobileLabel;
		Enterprise.ZArchitecture.ZLabel FaxLabel;
		Enterprise.ZArchitecture.ZLabel EmailLabel;
		Enterprise.ZArchitecture.ZLabel WebLabel;
		Enterprise.ZArchitecture.GUI.ZLinkLabel WebLink;
		protected Enterprise.ZArchitecture.GUI.ZTabControl DetailsTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage ContactInfoTab;
		protected Enterprise.ZArchitecture.GUI.ZTabPage AddressTab;
		protected Enterprise.ZArchitecture.GUI.ZTabPage AdditionalAddressInfoTab;
		protected ZDocAdditionalAddressInfoControl AdditionalAddressInfoControl;
		protected Enterprise.ZArchitecture.GUI.ZLinkLabel ContactsLink;
		protected Enterprise.ZArchitecture.GUI.ZLinkLabel AddressesLink;
		Enterprise.ZArchitecture.ZLabel FullNameOnlyLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZGroupBox GroupBox;
		private System.ComponentModel.IContainer components;

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.fOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox.Bare();
			this.AddressLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContactsLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.WebLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.WebLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PhoneLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MobileLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FaxLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EmailLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FullNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.AddressTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalAddressInfoTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalAddressInfoControl = new ZDocAdditionalAddressInfoControl();
			this.ContactInfoTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AddressesLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FullNameOnlyLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.fOrganisationFindBox.SuspendLayout();
			this.DetailsTabControl.SuspendLayout();
			this.AddressTab.SuspendLayout();
			this.AdditionalAddressInfoTab.SuspendLayout();
			this.ContactInfoTab.SuspendLayout();
			this.GroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// fOrganisationFindBox
			// 
			this.fOrganisationFindBox.AllowDrop = true;
			this.fOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.fOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.fOrganisationFindBox.Name = "fOrganisationFindBox";
			this.fOrganisationFindBox.PreBoundMaxLength = 11;
			this.fOrganisationFindBox.ShowDescriptionBox = false;
			this.fOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.fOrganisationFindBox.TabIndex = 0;
			// 
			// AddressLabel
			// 
			this.AddressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.AddressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 13, true);
			this.AddressLabel.Name = "AddressLabel";
			this.AddressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 75, true);
			this.AddressLabel.TabIndex = 1;
			this.AddressLabel.Text = "Address\r\nAddress\r\nAddress\r\nAddress\r\nAddress\r\nAddress";
			this.AddressLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.AddressLabel.UseCompatibleTextRendering = true;
			this.AddressLabel.UseMnemonic = false;
			// 
			// ContactsLink
			// 
			this.ContactsLink.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationControl|3dc1c5f7-92ca-4c54-9796-b4489a7f1041", "Contacts", "The Contacts link.");
			this.ContactsLink.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ContactsLink.IsFontBold = false;
			this.ContactsLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.ContactsLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 18, true);
			this.ContactsLink.Name = "ContactsLink";
			this.ContactsLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 16, true);
			this.ContactsLink.TabIndex = 1;
			this.ContactsLink.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ContactsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ContactsLink_LinkClicked);
			// 
			// WebLink
			// 
			this.WebLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.WebLink.IsFontBold = false;
			this.WebLink.LinkArea = new System.Windows.Forms.LinkArea(0, 23);
			this.WebLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(39, 64, true);
			this.WebLink.Name = "WebLink";
			this.WebLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 13, true);
			this.WebLink.TabIndex = 9;
			this.WebLink.Text = "www.cargowise.com";
			this.WebLink.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.WebLink.UseCompatibleTextRendering = true;
			this.WebLink.UseMnemonic = false;
			this.WebLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.WebLink_LinkClicked);
			// 
			// WebLabel
			// 
			this.WebLabel.AutoSize = true;
			this.WebLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 64, true);
			this.WebLabel.Name = "WebLabel";
			this.WebLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 13, true);
			this.WebLabel.TabIndex = 8;
			this.WebLabel.Text = "Web:";
			this.WebLabel.UseMnemonic = false;
			// 
			// PhoneLabel
			// 
			this.PhoneLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PhoneLabel.AutoSize = true;
			this.PhoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 8, true);
			this.PhoneLabel.Name = "PhoneLabel";
			this.PhoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 13, true);
			this.PhoneLabel.TabIndex = 4;
			this.PhoneLabel.Text = "Phone";
			this.PhoneLabel.UseMnemonic = false;
			// 
			// MobileLabel
			// 
			this.MobileLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MobileLabel.AutoSize = true;
			this.MobileLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 22, true);
			this.MobileLabel.Name = "MobileLabel";
			this.MobileLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 13, true);
			this.MobileLabel.TabIndex = 5;
			this.MobileLabel.Text = "Mobile";
			this.MobileLabel.UseMnemonic = false;
			// 
			// FaxLabel
			// 
			this.FaxLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FaxLabel.AutoSize = true;
			this.FaxLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 50, true);
			this.FaxLabel.Name = "FaxLabel";
			this.FaxLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 13, true);
			this.FaxLabel.TabIndex = 7;
			this.FaxLabel.Text = "Fax";
			this.FaxLabel.UseMnemonic = false;
			// 
			// EmailLabel
			// 
			this.EmailLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.EmailLabel.AutoSize = true;
			this.EmailLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 36, true);
			this.EmailLabel.Name = "EmailLabel";
			this.EmailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 13, true);
			this.EmailLabel.TabIndex = 6;
			this.EmailLabel.Text = "Email";
			this.EmailLabel.UseMnemonic = false;
			// 
			// FullNameLabel
			// 
			this.FullNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FullNameLabel.IsFontBold = true;
			this.FullNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 1, true);
			this.FullNameLabel.Name = "FullNameLabel";
			this.FullNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 12, true);
			this.FullNameLabel.TabIndex = 11;
			this.FullNameLabel.Text = "Full Name";
			this.FullNameLabel.UseMnemonic = false;
			// 
			// DetailsTabControl
			// 
			this.DetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DetailsTabControl.Controls.Add(this.AddressTab);
			this.DetailsTabControl.Controls.Add(this.AdditionalAddressInfoTab);
			this.DetailsTabControl.Controls.Add(this.ContactInfoTab);
			this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 39, true);
			this.DetailsTabControl.Name = "DetailsTabControl";
			this.DetailsTabControl.SelectedIndex = 0;
			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 108, true);
			this.DetailsTabControl.TabIndex = 3;
			this.DetailsTabControl.TabStop = false;
			// 
			// AddressTab
			// 
			this.AddressTab.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationControl|5f0ec6da-1b91-4354-bb28-0442115c50fd", "Address", "The Address.");
			this.AddressTab.Controls.Add(this.AddressLabel);
			this.AddressTab.Controls.Add(this.FullNameLabel);
			this.AddressTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AddressTab.Name = "AddressTab";
			this.AddressTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 85, true);
			this.AddressTab.TabIndex = 0;
			// 
			// AdditionalAddressInfoTab
			// 
			this.AdditionalAddressInfoTab.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationControl|9CCF2336-CD02-4C5A-A813-F98ECF5B8920", "Additional Info", "The Additional Address Info.");
			this.AdditionalAddressInfoTab.Controls.Add(this.AdditionalAddressInfoControl);
			this.AdditionalAddressInfoTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalAddressInfoTab.Name = "AdditionalAddressInfoTab";
			this.AdditionalAddressInfoTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 85, true);
			this.AdditionalAddressInfoTab.TabIndex = 1;
			// 
			// AdditionalAddressInfoControl
			// 
			this.AdditionalAddressInfoControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.AdditionalAddressInfoControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalAddressInfoControl.Name = "AdditionalAddressInfoControl";
			this.AdditionalAddressInfoControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 85, true);
			this.AdditionalAddressInfoControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.AdditionalAddressInfoControl.TabIndex = 1;
			this.AdditionalAddressInfoControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 0, true);
			// 
			// ContactInfoTab
			// 
			this.ContactInfoTab.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationControl|91624ac6-a73a-41ce-8873-66436e05f8ed", "Contact", "The Contact.");
			this.ContactInfoTab.Controls.Add(this.FaxLabel);
			this.ContactInfoTab.Controls.Add(this.EmailLabel);
			this.ContactInfoTab.Controls.Add(this.PhoneLabel);
			this.ContactInfoTab.Controls.Add(this.MobileLabel);
			this.ContactInfoTab.Controls.Add(this.WebLink);
			this.ContactInfoTab.Controls.Add(this.WebLabel);
			this.ContactInfoTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContactInfoTab.Name = "ContactInfoTab";
			this.ContactInfoTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 83, true);
			this.ContactInfoTab.TabIndex = 2;
			// 
			// AddressesLink
			// 
			this.AddressesLink.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationControl|0d5bab8c-08c4-4b8f-9019-2f718df81058", "Addr.", "The Addresses link.");
			this.AddressesLink.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.AddressesLink.IsFontBold = false;
			this.AddressesLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.AddressesLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 18, true);
			this.AddressesLink.Name = "AddressesLink";
			this.AddressesLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 16, true);
			this.AddressesLink.TabIndex = 2;
			this.AddressesLink.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.AddressesLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.AddressesLink_LinkClicked);
			// 
			// GroupBox
			// 
			this.GroupBox.Controls.Add(this.FullNameOnlyLabel);
			this.GroupBox.Controls.Add(this.ContactsLink);
			this.GroupBox.Controls.Add(this.AddressesLink);
			this.GroupBox.Controls.Add(this.DetailsTabControl);
			this.GroupBox.Controls.Add(this.fOrganisationFindBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GroupBox, false);
			this.GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupBox.Name = "GroupBox";
			this.GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.GroupBox.TabIndex = 4;
			this.GroupBox.TabStop = false;
			// 
			// FullNameOnlyLabel
			// 
			this.FullNameOnlyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FullNameOnlyLabel.BackColor = System.Drawing.Color.Transparent;
			this.FullNameOnlyLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationControl|cda0f416-bce7-43ff-82ff-0db9dc9ed801", "Full Name");
			this.FullNameOnlyLabel.IsFontBold = true;
			this.FullNameOnlyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 39, true);
			this.FullNameOnlyLabel.Name = "FullNameOnlyLabel";
			this.FullNameOnlyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 12, true);
			this.FullNameOnlyLabel.TabIndex = 12;
			this.FullNameOnlyLabel.UseMnemonic = false;
			this.FullNameOnlyLabel.Visible = false;
			// 
			// ZOrganisationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GroupBox);
			this.Name = "ZOrganisationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.fOrganisationFindBox.ResumeLayout(true);
			this.fOrganisationFindBox.PerformLayout();
			this.DetailsTabControl.ResumeLayout(false);
			this.DetailsTabControl.PerformLayout();
			this.AddressTab.ResumeLayout(false);
			this.AddressTab.PerformLayout();
			this.AdditionalAddressInfoTab.ResumeLayout(false);
			this.AdditionalAddressInfoTab.PerformLayout();
			this.ContactInfoTab.ResumeLayout(false);
			this.ContactInfoTab.PerformLayout();
			this.GroupBox.ResumeLayout(false);
			this.GroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
