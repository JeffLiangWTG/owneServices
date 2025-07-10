namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class CrewMembersUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.CrewMembersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CrewMembersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CrewMemberUserControl = new Enterprise.Customs.US.eManifest.GUI.CrewMemberUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.CrewMembersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CrewMembersGrid)).BeginInit();
			this.CrewMembersGrid.SuspendLayout();
			this.CrewMemberUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.Trip);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 280, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.CrewMembersGroupBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 320, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(60);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.CrewMemberUserControl);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(215);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(101);
			this.SplitContainer.TabIndex = 1;
			// 
			// CrewMembersGroupBox
			// 
			this.CrewMembersGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CrewMembersUserControl|3df9196b-3e4a-4e29-9dc1-34db25829379", "Crew Members/Passengers");
			this.CrewMembersGroupBox.Controls.Add(this.CrewMembersGrid);
			this.CrewMembersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CrewMembersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CrewMembersGroupBox.Name = "CrewMembersGroupBox";
			this.CrewMembersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 101, true);
			this.CrewMembersGroupBox.TabIndex = 0;
			this.CrewMembersGroupBox.TabStop = false;
			// 
			// CrewMembersGrid
			// 
			this.CrewMembersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CrewMembersGrid, "CrewMembers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).CP_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).CP_GS_NKStaff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).CP_OC_Contact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).CP_Gender)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).CP_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).CP_DateOfBirth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).CP_RN_NKNationality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.Lookups.OrgHeader_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.E2_OA_Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.Organisation.Addresses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.ContactPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.Organisation.Contacts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.E2_AddressOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.E2_CompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.E2_Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.E2_Address2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.E2_City)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.E2_Postcode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.E2_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.E2_State)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.E2_Contact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.E2_Phone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.E2_Fax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).USAddress.E2_Email)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).DriversLicense)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).CP_HasHazmatEndorsment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)).HazmatEndorsement)));
			this.CrewMembersGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CrewMembersUserControl|b9c2292a-8607-4efe-9d4c-d1977c1875da", "Type", "Crew Type", "");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CP_Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CrewMembersUserControl|c0f01609-cf81-4e1f-8dc6-d677366c110d", "Staff");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CP_GS_NKStaff";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CrewMembersUserControl|132bc629-39fc-475a-a257-5a0d77a423ad", "Contact");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CP_OC_Contact";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CrewMembersUserControl|b2853943-4cf3-4b90-8dda-e26995025811", "Gender");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CP_Gender";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CrewMembersUserControl|73ee53e7-a1dc-421e-8ccb-0d1398176066", "Full Name");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CP_FullName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CrewMembersUserControl|279d0cd5-bf04-4666-b391-adef8c6b3492", "Date Of Birth");
			zDateEditColumnStyleInfo1.ColumnName = "CP_DateOfBirth";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CrewMembersUserControl|f0c15fb8-70ee-48ed-8922-226296e3db75", "Citizenship");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CP_RN_NKNationality";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "USAddress+Lookups+OrgHeader_List";
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("8022c23a-89e6-4846-ad3f-517b5bcc2028", "US Org.", "US Organization", "US Address Organization", "A U.S. address where the driver will be at some point during this trip must be provided. This could be a delivery address to where the driver will be delivering cargo.");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "USAddress+OrganisationPK";
			zOrganisationFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("b2912cd4-3edd-49a6-bb3a-58bcf821e135", "US Address Organization");
			zOrganisationFindBoxColumnStyleInfo1.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zGuidDropEditColumnStyleInfo1.BindToList = "USAddress+Organisation+Addresses";
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("62440d11-3aa0-4c1e-a8ab-bfc1a88ce833", "US Add. Sel.", "US Add. Selector", "US Address Selector", "");
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "USAddress+E2_OA_Address";
			zGuidDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("b2912cd4-3edd-49a6-bb3a-58bcf821e135", "US Address Organization");
			zGuidDropEditColumnStyleInfo1.IsVisible = false;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			zGuidDropEditColumnStyleInfo2.BindToList = "USAddress+Organisation+Contacts";
			zGuidDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("c2ff3241-5948-4ae2-a15d-58bd29ff48ec", "US Cont. Sel.", "US Cont. Selector", "US Contact Selector", "");
			zGuidDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo2.ColumnName = "USAddress+ContactPK";
			zGuidDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("b2912cd4-3edd-49a6-bb3a-58bcf821e135", "US Address Organization");
			zGuidDropEditColumnStyleInfo2.IsVisible = false;
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("53630c0c-13c4-48ff-9977-958e53fc9ca3", "Ovr.", "Override", "US Address Override", "");
			zCheckBoxColumnStyleInfo1.ColumnName = "USAddress+E2_AddressOverride";
			zCheckBoxColumnStyleInfo1.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("b2912cd4-3edd-49a6-bb3a-58bcf821e135", "US Address Organization");
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("5a63732b-f921-43ec-8c89-c9c1a29e46ab", "US Comp.", "US Company", "US Address Company Name", "");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "USAddress+E2_CompanyName";
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("76580046-c31d-4540-915e-c6c7a97f4b28", "US Address Details");
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("3526b9ad-ce77-421e-a43d-ee51800bd93b", "US Add. 1", "US Address 1", "US Address Line 1", "");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "USAddress+E2_Address1";
			zTextBoxColumnStyleInfo3.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("76580046-c31d-4540-915e-c6c7a97f4b28", "US Address Details");
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("4d573ddb-3fb4-41f9-850e-918c4c4edba9", "US Add. 2", "US Address 2", "US Address Line 2", "");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "USAddress+E2_Address2";
			zTextBoxColumnStyleInfo4.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("76580046-c31d-4540-915e-c6c7a97f4b28", "US Address Details");
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("dcedd988-98e4-43d2-9f31-b361d17dbbe9", "US City", "US Address City", "");
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "USAddress+E2_City";
			zTextBoxColumnStyleInfo5.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("76580046-c31d-4540-915e-c6c7a97f4b28", "US Address Details");
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("e5f2c8ab-ba81-4b03-b960-86c1884402f0", "US P/C", "US Postcode", "US Address Postcode", "");
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "USAddress+E2_Postcode";
			zTextBoxColumnStyleInfo6.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("76580046-c31d-4540-915e-c6c7a97f4b28", "US Address Details");
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("681d0695-3c70-40d8-9720-7f8a54fb6e7f", "Country", "US Address Country Code", "");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "USAddress+E2_RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo3.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("76580046-c31d-4540-915e-c6c7a97f4b28", "US Address Details");
			zCodeFindBoxColumnStyleInfo3.IsVisible = false;
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("b37e36f5-36ba-4a42-919c-af1e21a55f02", "US St.", "US State", "US Address State", "");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "USAddress+E2_State";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("76580046-c31d-4540-915e-c6c7a97f4b28", "US Address Details");
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ede3a9a3-ef34-40e9-8343-f3b1cf7c4789", "US Contact", "US Contact Name", "US Address Contact Name", "");
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "USAddress+E2_Contact";
			zTextBoxColumnStyleInfo7.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("c0ad6c28-2bc2-4198-9478-49372a5a31c0", "US Address Contact");
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("041c8562-17e4-40d3-b2dc-3cd91c78a200", "US Ph.", "US Phone", "US Address Phone", "");
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo8.ColumnName = "USAddress+E2_Phone";
			zTextBoxColumnStyleInfo8.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("c0ad6c28-2bc2-4198-9478-49372a5a31c0", "US Address Contact");
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("6ccba838-68d2-4e0d-a2e1-b2c849b6c664", "US Fax", "US Address Fax", "");
			zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo9.ColumnName = "USAddress+E2_Fax";
			zTextBoxColumnStyleInfo9.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("c0ad6c28-2bc2-4198-9478-49372a5a31c0", "US Address Contact");
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("b2859e93-1415-460e-8615-e0681c8b63a3", "US Email", "US Address Email", "");
			zTextBoxColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo10.ColumnName = "USAddress+E2_Email";
			zTextBoxColumnStyleInfo10.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("c0ad6c28-2bc2-4198-9478-49372a5a31c0", "US Address Contact");
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ecb6501d-899e-4124-9f75-57203435cb9f", "License", "Drivers License", "Commercial, national or enhanced drivers license of the crew member.");
			zTextBoxColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo11.ColumnName = "DriversLicense";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ecefe5d5-3723-4497-af55-04e4fc56c7c3", "Has", "Has Hazmat", "Has Hazmat Endorsement", "Indicates that the crew member has hazardous materials endorsement but no number exists.");
			zCheckBoxColumnStyleInfo2.ColumnName = "CP_HasHazmatEndorsment";
			zCheckBoxColumnStyleInfo2.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("30f3fda6-6115-48fb-8a25-2fbd63f6b36e", "Hazmat Endorsement");
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("7d1e654d-61d5-4c6d-8917-c50386641213", "Hazmat Endorsement", "Hazardous materials endorsement of the crew member.");
			zTextBoxColumnStyleInfo12.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo12.ColumnName = "HazmatEndorsement";
			zTextBoxColumnStyleInfo12.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("30f3fda6-6115-48fb-8a25-2fbd63f6b36e", "Hazmat Endorsement");
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.CrewMembersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CrewMembersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CrewMembersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CrewMembersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CrewMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CrewMembersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CrewMembersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.CrewMembersGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.CrewMembersGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.CrewMembersGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.CrewMembersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CrewMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CrewMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CrewMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CrewMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CrewMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.CrewMembersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.CrewMembersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.CrewMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.CrewMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.CrewMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.CrewMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.CrewMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.CrewMembersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.CrewMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.CrewMembersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CrewMembersGrid.GridId = "ce045640-30ad-49f8-b85b-5ee9570ad3e7";
			this.CrewMembersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CrewMembersGrid.LayoutKey = "CrewMembersGrid";
			this.CrewMembersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CrewMembersGrid.Name = "CrewMembersGrid";
			this.CrewMembersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 82, true);
			this.CrewMembersGrid.TabIndex = 0;
			// 
			// CrewMemberUserControl
			// 
			this.CrewMemberUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CrewMemberUserControl, "CrewMembers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewMembers)).SyncRoot)))));
			this.CrewMemberUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CrewMemberUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CrewMemberUserControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 2000, true);
			this.CrewMemberUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 215, true);
			this.CrewMemberUserControl.Name = "CrewMemberUserControl";
			this.CrewMemberUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 215, true);
			this.CrewMemberUserControl.TabIndex = 0;
			// 
			// CrewMembersUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 320, true);
			this.Name = "CrewMembersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 320, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.CrewMembersGroupBox.ResumeLayout(false);
			this.CrewMembersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CrewMembersGrid)).EndInit();
			this.CrewMembersGrid.ResumeLayout(false);
			this.CrewMembersGrid.PerformLayout();
			this.CrewMemberUserControl.ResumeLayout(true);
			this.CrewMemberUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private ZArchitecture.GUI.ZGroupBox CrewMembersGroupBox;
		private ZArchitecture.ZGrid CrewMembersGrid;
		private CrewMemberUserControl CrewMemberUserControl;
	}
}
