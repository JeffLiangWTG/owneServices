namespace Enterprise.MasterFiles.GUI
{
	public partial class MergeOrgHeaderForm
	{

		#region Windows Form Designer generated code

		private Enterprise.ZArchitecture.GUI.ZGroupBox OldOrganisationGroupBox;
		private Enterprise.ZArchitecture.ZTextBox OldOrgCodeTextBox;
		private Enterprise.ZArchitecture.ZTextBox OldOrgNameTextBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox NewOrgGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox NewOrganisationGroupBox;
		private Enterprise.ZArchitecture.GUI.ZButton ProcessButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelMergeButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MergeOrganizationAddressesGroupBox;
		internal Enterprise.ZArchitecture.ZGrid MergeAddressesZGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MergeOrganizationContactsGroupBox;
		internal Enterprise.ZArchitecture.ZGrid MergeContactsZGrid;

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.NewOrgGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OldOrganisationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OldOrgNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OldOrgCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProcessButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NewOrganisationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CancelMergeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MergeOrganizationAddressesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MergeAddressesZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MergeOrganizationContactsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MergeContactsZGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OldOrganisationGroupBox.SuspendLayout();
			this.NewOrganisationGroupBox.SuspendLayout();
			this.MergeOrganizationAddressesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MergeAddressesZGrid)).BeginInit();
			this.MergeOrganizationContactsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MergeContactsZGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 518, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.MergeOrgHeader);
			// 
			// NewOrgGuidFindBox
			// 
			this.NewOrgGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.NewOrgGuidFindBox, "NewOrganisationPk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).NewOrganisationPk)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NewOrgGuidFindBox, false);
			this.NewOrgGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 24, true);
			this.NewOrgGuidFindBox.Name = "NewOrgGuidFindBox";
			this.NewOrgGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 20, true);
			this.NewOrgGuidFindBox.TabIndex = 0;
			// 
			// OldOrganisationGroupBox
			// 
			this.OldOrganisationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OldOrganisationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|c9310f57-6b67-4939-bb5f-4ddfeeb4d22d", "Dissolved Organization");
			this.OldOrganisationGroupBox.Controls.Add(this.OldOrgNameTextBox);
			this.OldOrganisationGroupBox.Controls.Add(this.OldOrgCodeTextBox);
			this.OldOrganisationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.OldOrganisationGroupBox.Name = "OldOrganisationGroupBox";
			this.OldOrganisationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 80, true);
			this.OldOrganisationGroupBox.TabIndex = 0;
			this.OldOrganisationGroupBox.TabStop = false;
			// 
			// OldOrgNameTextBox
			// 
			this.OldOrgNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OldOrgNameTextBox, "OldOrganisationName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrganisationName)));
			this.OldOrgNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|5d7afba1-b104-4421-9445-43faf016f549", "Name", "Name", "");
			this.OldOrgNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 48, true);
			this.OldOrgNameTextBox.Name = "OldOrgNameTextBox";
			this.OldOrgNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 20, true);
			this.OldOrgNameTextBox.TabIndex = 1;
			// 
			// OldOrgCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.OldOrgCodeTextBox, "OldOrganisationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrganisationCode)));
			this.OldOrgCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|bd804da0-7d7e-4ccd-acc1-11e893e1a77d", "Code", "Code", "");
			this.OldOrgCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 24, true);
			this.OldOrgCodeTextBox.Name = "OldOrgCodeTextBox";
			this.OldOrgCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.OldOrgCodeTextBox.TabIndex = 0;
			// 
			// ProcessButton
			// 
			this.ProcessButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ProcessButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|c9ca186d-e9fa-4a0c-beac-9580dc0ecffa", "&Merge");
			this.ProcessButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(605, 489, true);
			this.ProcessButton.Name = "ProcessButton";
			this.ProcessButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.ProcessButton.TabIndex = 2;
			this.ProcessButton.Click += new System.EventHandler(this.ProcessButton_Click);
			// 
			// NewOrganisationGroupBox
			// 
			this.NewOrganisationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.NewOrganisationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|42b33e55-82e0-4053-b435-a971abe80557", "Retained Organization");
			this.NewOrganisationGroupBox.Controls.Add(this.NewOrgGuidFindBox);
			this.NewOrganisationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 96, true);
			this.NewOrganisationGroupBox.Name = "NewOrganisationGroupBox";
			this.NewOrganisationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 56, true);
			this.NewOrganisationGroupBox.TabIndex = 1;
			this.NewOrganisationGroupBox.TabStop = false;
			// 
			// CancelMergeButton
			// 
			this.CancelMergeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelMergeButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|e266ad2b-2cec-4e19-8dbc-474cfc66d6b4", "&Cancel");
			this.CancelMergeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelMergeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(691, 489, true);
			this.CancelMergeButton.Name = "CancelMergeButton";
			this.CancelMergeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.CancelMergeButton.TabIndex = 3;
			// 
			// MergeOrganizationAddressesGroupBox
			// 
			this.MergeOrganizationAddressesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.MergeOrganizationAddressesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|b27f8996-db70-4033-8429-e2e51e11e8db", "Addresses");
			this.MergeOrganizationAddressesGroupBox.Controls.Add(this.MergeAddressesZGrid);
			this.MergeOrganizationAddressesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 158, true);
			this.MergeOrganizationAddressesGroupBox.Name = "MergeOrganizationAddressesGroupBox";
			this.MergeOrganizationAddressesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 160, true);
			this.MergeOrganizationAddressesGroupBox.TabIndex = 5;
			this.MergeOrganizationAddressesGroupBox.TabStop = false;
			// 
			// MergeAddressesZGrid
			// 
			this.MergeAddressesZGrid.AllowNavigation = false;
			this.MergeAddressesZGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MergeAddressesZGrid, "OldOrgAddressesCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgAddressesCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgAddressesCollection)).SyncRoot)).OldAddressCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgAddressesCollection)).SyncRoot)).OldAddressAddress1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgAddressesCollection)).SyncRoot)).OldAddressCity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgAddressesCollection)).SyncRoot)).Action)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.MergeOrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgAddressesCollection)).SyncRoot)).NewAddressPK)));
			this.MergeAddressesZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|5136e7f7-145d-4a56-99c7-ba07a000c83b", "Code", "Code", "");
			zTextBoxColumnStyleInfo1.ColumnName = "OldAddressCode";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|805b1577-f10d-4120-a3ba-2c50b9e2d8bb", "Address 1", "Address 1", "");
			zTextBoxColumnStyleInfo2.ColumnName = "OldAddressAddress1";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|b86f6ff7-d554-47bd-a638-48de7538b757", "City", "City", "");
			zTextBoxColumnStyleInfo3.ColumnName = "OldAddressCity";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|e9173d37-595a-40b3-a713-8594d3148b7a", "Action", "Action", "");
			zDropEditColumnStyleInfo1.ColumnName = "Action";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|922c0415-78e8-43dd-8c02-2733ca7a9c5b", "Match Address", "Match Address", "");
			zGuidDropEditColumnStyleInfo1.ColumnName = "NewAddressPK";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.MergeAddressesZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MergeAddressesZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MergeAddressesZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MergeAddressesZGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MergeAddressesZGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.MergeAddressesZGrid.GridId = "42cf8507-d0e5-4644-ab58-5f6399ac28f5";
			this.MergeAddressesZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MergeAddressesZGrid.LayoutKey = "zGrid1";
			this.MergeAddressesZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 20, true);
			this.MergeAddressesZGrid.Name = "MergeAddressesZGrid";
			this.MergeAddressesZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 134, true);
			this.MergeAddressesZGrid.TabIndex = 0;
			this.MergeAddressesZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			// 
			// MergeOrganizationContactsGroupBox
			// 
			this.MergeOrganizationContactsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.MergeOrganizationContactsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|634a7119-3d65-4b6c-8cdd-2af0a080f1a8", "Contacts");
			this.MergeOrganizationContactsGroupBox.Controls.Add(this.MergeContactsZGrid);
			this.MergeOrganizationContactsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 324, true);
			this.MergeOrganizationContactsGroupBox.Name = "MergeOrganizationContactsGroupBox";
			this.MergeOrganizationContactsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 160, true);
			this.MergeOrganizationContactsGroupBox.TabIndex = 8;
			this.MergeOrganizationContactsGroupBox.TabStop = false;
			// 
			// MergeContactsZGrid
			// 
			this.MergeContactsZGrid.AllowNavigation = false;
			this.MergeContactsZGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MergeContactsZGrid, "ContactCollectionWithoutDummyContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).ContactCollectionWithoutDummyContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgContactCollection)).SyncRoot)).OldContactName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgContactCollection)).SyncRoot)).OldContactTitle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgContactCollection)).SyncRoot)).OldContactPhone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgContactCollection)).SyncRoot)).OldContactMobile)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgContactCollection)).SyncRoot)).Action)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.MergeOrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgContactCollection)).SyncRoot)).NewContactPK)));
			this.MergeContactsZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|7e743a25-0c3f-47af-93dd-76464c846fb7", "Old Contact Name");
			zTextBoxColumnStyleInfo4.ColumnName = "OldContactName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|6041c8e9-590f-4ad0-bee0-8b8c1fe9c3c2", "Old Contact Title");
			zTextBoxColumnStyleInfo5.ColumnName = "OldContactTitle";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|f088f91a-a8aa-472d-9127-9289d8651868", "Old Contact Phone");
			zTextBoxColumnStyleInfo6.ColumnName = "OldContactPhone";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|9f8bfb12-d894-482e-bd95-ced886328371", "Old Contact Mobile");
			zTextBoxColumnStyleInfo7.ColumnName = "OldContactMobile";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|270e427b-7d1c-493e-84b4-f3ee17d56613", "Action");
			zDropEditColumnStyleInfo2.ColumnName = "Action";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|02b2b137-b98f-4b9a-9dd1-621e13999cb4", "Match Contact");
			zGuidDropEditColumnStyleInfo2.ColumnName = "NewContactPK";
			this.MergeContactsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MergeContactsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MergeContactsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MergeContactsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MergeContactsZGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.MergeContactsZGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.MergeContactsZGrid.GridId = "52e4d8f3-736c-417d-abd1-f43281cd285e";
			this.MergeContactsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MergeContactsZGrid.LayoutKey = "zGrid1";
			this.MergeContactsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 20, true);
			this.MergeContactsZGrid.Name = "MergeContactsZGrid";
			this.MergeContactsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 134, true);
			this.MergeContactsZGrid.TabIndex = 0;
			this.MergeContactsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			// 
			// MergeOrgHeaderForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 542, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeOrgHeaderForm|74ed8aed-a028-4270-8259-3163f858f60d", "Organizations");
			this.Controls.Add(this.MergeOrganizationAddressesGroupBox);
			this.Controls.Add(this.MergeOrganizationContactsGroupBox);
			this.Controls.Add(this.NewOrganisationGroupBox);
			this.Controls.Add(this.OldOrganisationGroupBox);
			this.Controls.Add(this.CancelMergeButton);
			this.Controls.Add(this.ProcessButton);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.MergeOrgHeader);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "MergeOrgHeaderForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.ProcessButton, 0);
			this.Controls.SetChildIndex(this.CancelMergeButton, 0);
			this.Controls.SetChildIndex(this.OldOrganisationGroupBox, 0);
			this.Controls.SetChildIndex(this.NewOrganisationGroupBox, 0);
			this.Controls.SetChildIndex(this.MergeOrganizationContactsGroupBox, 0);
			this.Controls.SetChildIndex(this.MergeOrganizationAddressesGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OldOrganisationGroupBox.ResumeLayout(false);
			this.OldOrganisationGroupBox.PerformLayout();
			this.NewOrganisationGroupBox.ResumeLayout(false);
			this.MergeOrganizationAddressesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MergeAddressesZGrid)).EndInit();
			this.MergeOrganizationContactsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MergeContactsZGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

	}
}
