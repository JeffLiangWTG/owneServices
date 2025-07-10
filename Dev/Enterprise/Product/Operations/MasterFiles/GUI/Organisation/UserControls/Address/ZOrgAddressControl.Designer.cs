namespace Enterprise.MasterFiles.GUI
{
	public partial class ZOrgAddressControl
	{

		#region Component Designer generated code

		protected Enterprise.ZArchitecture.GUI.Internal.ZAddressFindBox.Bare OrganisationFindBox;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox GroupBox;
		internal protected Enterprise.ZArchitecture.GUI.ZLinkLabel ContactsLink;
		internal protected Enterprise.ZArchitecture.GUI.ZLinkLabel AddressesLink;
		protected Enterprise.ZArchitecture.GUI.ZTabControl DetailsTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage AddressTab;
		internal Enterprise.ZArchitecture.ZLabel AddressLabel;
		protected Enterprise.ZArchitecture.GUI.ZTabPage ContactInfoTab;
		protected Enterprise.ZArchitecture.ZLabel FaxLabel;
		protected Enterprise.ZArchitecture.ZLabel EmailLabel;
		protected Enterprise.ZArchitecture.ZLabel PhoneLabel;
		protected Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEdit.Bare AddressDropEdit;
		protected Enterprise.ZArchitecture.ZLabel zLabel3;
		protected Enterprise.ZArchitecture.ZLabel zLabel2;
		protected Enterprise.ZArchitecture.ZLabel zLabel1;
		private System.ComponentModel.IContainer components;

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.OrganisationFindBox = new Enterprise.ZArchitecture.GUI.Internal.ZAddressFindBox.Bare();
			this.GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContactsLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.AddressesLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.DetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.AddressTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AddressDropEdit = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEdit.Bare();
			this.AddressLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContactInfoTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.FaxLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EmailLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PhoneLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OrganisationFindBox.SuspendLayout();
			this.GroupBox.SuspendLayout();
			this.DetailsTabControl.SuspendLayout();
			this.AddressTab.SuspendLayout();
			this.AddressDropEdit.SuspendLayout();
			this.ContactInfoTab.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.ZAddress);
			// 
			// OrganisationFindBox
			// 
			this.OrganisationFindBox.AllowDrop = true;
			this.OrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.OrganisationFindBox.Name = "OrganisationFindBox";
			this.OrganisationFindBox.PreBoundMaxLength = 10;
			this.OrganisationFindBox.ShowDescriptionBox = false;
			this.OrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.OrganisationFindBox.TabIndex = 0;
			// 
			// GroupBox
			// 
			this.GroupBox.Controls.Add(this.ContactsLink);
			this.GroupBox.Controls.Add(this.AddressesLink);
			this.GroupBox.Controls.Add(this.OrganisationFindBox);
			this.GroupBox.Controls.Add(this.DetailsTabControl);
			this.GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupBox.Name = "GroupBox";
			this.GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.GroupBox.TabIndex = 5;
			this.GroupBox.TabStop = false;
			// 
			// ContactsLink
			// 
			this.ContactsLink.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrgAddressControl|3dc1c5f7-92ca-4c54-9796-b4489a7f1041", "Contacts");
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
			// AddressesLink
			// 
			this.AddressesLink.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrgAddressControl|0d5bab8c-08c4-4b8f-9019-2f718df81058", "Addr.");
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
			// DetailsTabControl
			// 
			this.DetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DetailsTabControl.Controls.Add(this.AddressTab);
			this.DetailsTabControl.Controls.Add(this.ContactInfoTab);
			this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 39, true);
			this.DetailsTabControl.Name = "DetailsTabControl";
			this.DetailsTabControl.SelectedIndex = 0;
			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 111, true);
			this.DetailsTabControl.TabIndex = 3;
			this.DetailsTabControl.TabStop = false;
			// 
			// AddressTab
			// 
			this.AddressTab.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrgAddressControl|5f0ec6da-1b91-4354-bb28-0442115c50fd", "Address");
			this.AddressTab.Controls.Add(this.AddressDropEdit);
			this.AddressTab.Controls.Add(this.AddressLabel);
			this.AddressTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AddressTab.Name = "AddressTab";
			this.AddressTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 84, true);
			this.AddressTab.TabIndex = 0;
			// 
			// AddressDropEdit
			// 
			this.AddressDropEdit.AllowDrop = true;
			this.AddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AddressDropEdit.Name = "AddressDropEdit";
			this.AddressDropEdit.PreBoundMaxLength = 28;
			this.AddressDropEdit.ShowDescriptionBox = false;
			this.AddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.AddressDropEdit.TabIndex = 12;
			// 
			// AddressLabel
			// 
			this.AddressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AddressLabel, "AddressFullFormatted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.ZAddress)(null)).AddressFull)));
			this.AddressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AddressLabel.Name = "AddressLabel";
			this.AddressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 61, true);
			this.AddressLabel.TabIndex = 1;
			this.AddressLabel.Text = "Address\r\nAddress\r\nAddress\r\nAddress\r\nAddress\r\nAddress";
			this.AddressLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.AddressLabel.UseCompatibleTextRendering = true;
			this.AddressLabel.UseMnemonic = false;
			// 
			// ContactInfoTab
			// 
			this.ContactInfoTab.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrgAddressControl|91624ac6-a73a-41ce-8873-66436e05f8ed", "Contact");
			this.ContactInfoTab.Controls.Add(this.zLabel3);
			this.ContactInfoTab.Controls.Add(this.zLabel2);
			this.ContactInfoTab.Controls.Add(this.zLabel1);
			this.ContactInfoTab.Controls.Add(this.FaxLabel);
			this.ContactInfoTab.Controls.Add(this.EmailLabel);
			this.ContactInfoTab.Controls.Add(this.PhoneLabel);
			this.ContactInfoTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContactInfoTab.Name = "ContactInfoTab";
			this.ContactInfoTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 84, true);
			this.ContactInfoTab.TabIndex = 1;
			// 
			// zLabel3
			// 
			this.zLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zLabel3, "Email");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.ZAddress)(null)).Email)));
			this.zLabel3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrgAddressControl|01c963fa-8368-473f-981d-e82d1f8a88f3", "Email");
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 31, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 13, true);
			this.zLabel3.TabIndex = 12;
			this.zLabel3.UseMnemonic = false;
			// 
			// zLabel2
			// 
			this.zLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zLabel2, "Fax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.ZAddress)(null)).Fax)));
			this.zLabel2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrgAddressControl|905f6d68-c693-4ed9-8c1c-251a352cb487", "Fax");
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 17, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 13, true);
			this.zLabel2.TabIndex = 11;
			this.zLabel2.UseMnemonic = false;
			// 
			// zLabel1
			// 
			this.zLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zLabel1, "PhoneAndMobile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.ZAddress)(null)).PhoneAndMobile)));
			this.zLabel1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrgAddressControl|ef2b54b1-9b69-490f-99a6-48c9e95f2c84", "Phone / Mobile");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 3, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 13, true);
			this.zLabel1.TabIndex = 10;
			this.zLabel1.UseMnemonic = false;
			// 
			// FaxLabel
			// 
			this.FaxLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FaxLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrgAddressControl|213c4abc-2325-4e0c-9006-566abe8a1a52", "Fax:");
			this.FaxLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 16, true);
			this.FaxLabel.Name = "FaxLabel";
			this.FaxLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.FaxLabel.TabIndex = 8;
			this.FaxLabel.UseMnemonic = false;
			// 
			// EmailLabel
			// 
			this.EmailLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.EmailLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrgAddressControl|4ad9f6e5-dabc-4f73-a691-4eab883019b5", "Email:");
			this.EmailLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 29, true);
			this.EmailLabel.Name = "EmailLabel";
			this.EmailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.EmailLabel.TabIndex = 9;
			this.EmailLabel.UseMnemonic = false;
			// 
			// PhoneLabel
			// 
			this.PhoneLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PhoneLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrgAddressControl|5fd60c0e-6c85-471a-a3f9-db52e0d730c6", "Ph:");
			this.PhoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 3, true);
			this.PhoneLabel.Name = "PhoneLabel";
			this.PhoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.PhoneLabel.TabIndex = 7;
			this.PhoneLabel.UseMnemonic = false;
			// 
			// ZOrgAddressControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GroupBox);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.Name = "ZOrgAddressControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OrganisationFindBox.ResumeLayout(true);
			this.OrganisationFindBox.PerformLayout();
			this.GroupBox.ResumeLayout(false);
			this.GroupBox.PerformLayout();
			this.DetailsTabControl.ResumeLayout(false);
			this.DetailsTabControl.PerformLayout();
			this.AddressTab.ResumeLayout(false);
			this.AddressTab.PerformLayout();
			this.AddressDropEdit.ResumeLayout(true);
			this.AddressDropEdit.PerformLayout();
			this.ContactInfoTab.ResumeLayout(false);
			this.ContactInfoTab.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
