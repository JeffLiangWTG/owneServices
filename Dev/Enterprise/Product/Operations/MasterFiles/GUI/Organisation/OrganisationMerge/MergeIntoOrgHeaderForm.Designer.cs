namespace Enterprise.MasterFiles.GUI
{
	public partial class MergeIntoOrgHeaderForm
	{

		#region Windows Form Designer generated code

		private Enterprise.ZArchitecture.GUI.ZButton ProcessButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelMergeButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MergeOrganizationAddressesGroupBox;
		internal Enterprise.ZArchitecture.ZGrid MergeAddressesZGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MergeOrganizationContactsGroupBox;
		internal Enterprise.ZArchitecture.ZGrid MergeContactsZGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox NewOrganisationGroupBox;
		private Enterprise.ZArchitecture.ZTextBox NewOrgNameTextBox;
		private Enterprise.ZArchitecture.ZTextBox NewOrgCodeTextBox;

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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ProcessButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelMergeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MergeOrganizationAddressesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MergeAddressesZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MergeOrganizationContactsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MergeContactsZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NewOrganisationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NewOrgNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewOrgCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OrgsToMergeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PatternParamPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MatchThreshold = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MaxResults = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FindButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FindByPattern = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.FindByCode = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.FindByName = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.AddAnotherOldOrgButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OldOrgGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OldOrganisationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ProgressMerge = new CargoWise.Windows.UI.KProgressBar();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MergeOrganizationAddressesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MergeAddressesZGrid)).BeginInit();
			this.MergeOrganizationContactsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MergeContactsZGrid)).BeginInit();
			this.NewOrganisationGroupBox.SuspendLayout();
			this.OrgsToMergeGroupBox.SuspendLayout();
			this.PatternParamPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OldOrganisationsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 682, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 9;
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
			// ProcessButton
			// 
			this.ProcessButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ProcessButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|f5279177-93f4-45a6-8246-c2b4b862b1c1", "&Merge");
			this.ProcessButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 653, true);
			this.ProcessButton.Name = "ProcessButton";
			this.ProcessButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.ProcessButton.TabIndex = 15;
			this.ProcessButton.Click += new System.EventHandler(this.ProcessButton_Click);
			// 
			// CancelMergeButton
			// 
			this.CancelMergeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelMergeButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|980a6cf3-bcbc-4288-8283-d4781d166a81", "&Cancel");
			this.CancelMergeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelMergeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(694, 653, true);
			this.CancelMergeButton.Name = "CancelMergeButton";
			this.CancelMergeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.CancelMergeButton.TabIndex = 16;
			this.CancelMergeButton.Click += new System.EventHandler(this.CancelMergeButton_Click);
			// 
			// MergeOrganizationAddressesGroupBox
			// 
			this.MergeOrganizationAddressesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.MergeOrganizationAddressesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|5beea8d3-b427-4ec0-8e04-2ae6e1d8b52a", "Addresses");
			this.MergeOrganizationAddressesGroupBox.Controls.Add(this.MergeAddressesZGrid);
			this.MergeOrganizationAddressesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 396, true);
			this.MergeOrganizationAddressesGroupBox.Name = "MergeOrganizationAddressesGroupBox";
			this.MergeOrganizationAddressesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 122, true);
			this.MergeOrganizationAddressesGroupBox.TabIndex = 4;
			this.MergeOrganizationAddressesGroupBox.TabStop = false;
			// 
			// MergeAddressesZGrid
			// 
			this.MergeAddressesZGrid.AllowBeginDrag = false;
			this.MergeAddressesZGrid.AllowCopyToNewRowMenuItem = false;
			this.MergeAddressesZGrid.AllowDragDropWithChanges = false;
			this.MergeAddressesZGrid.AllowNavigation = false;
			this.MergeAddressesZGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MergeAddressesZGrid, "OldOrgAddressCollectionForSimilarOrgsByName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgAddressCollectionForSimilarOrgsByName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgAddressCollectionForSimilarOrgsByName)).SyncRoot)).OldAddressCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgAddressCollectionForSimilarOrgsByName)).SyncRoot)).OldAddressAddress1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgAddressCollectionForSimilarOrgsByName)).SyncRoot)).OldAddressCity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgAddressCollectionForSimilarOrgsByName)).SyncRoot)).Action)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.MergeOrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgAddressCollectionForSimilarOrgsByName)).SyncRoot)).NewAddressPK)));
			this.MergeAddressesZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|cca3641f-c9bb-4fb0-a275-3adbc133507d", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "OldAddressCode";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|8fd96731-b81e-4340-9c66-63712c0974af", "Address 1");
			zTextBoxColumnStyleInfo2.ColumnName = "OldAddressAddress1";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|4f7d2a78-85b0-4138-b59e-4ae87cdf0b28", "City");
			zTextBoxColumnStyleInfo3.ColumnName = "OldAddressCity";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|27b08a58-8196-4e77-8aa4-da35d9ec8352", "Action");
			zDropEditColumnStyleInfo1.ColumnName = "Action";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|a7d4cfce-275c-40f8-a019-c18c3197b037", "Match Address");
			zGuidDropEditColumnStyleInfo1.ColumnName = "NewAddressPK";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.MergeAddressesZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MergeAddressesZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MergeAddressesZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MergeAddressesZGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MergeAddressesZGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.MergeAddressesZGrid.GridId = "5714994e-5baf-457f-baac-f133ce649860";
			this.MergeAddressesZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MergeAddressesZGrid.LayoutKey = "zGrid1";
			this.MergeAddressesZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 20, true);
			this.MergeAddressesZGrid.Name = "MergeAddressesZGrid";
			this.MergeAddressesZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 96, true);
			this.MergeAddressesZGrid.TabIndex = 0;
			this.MergeAddressesZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			// 
			// MergeOrganizationContactsGroupBox
			// 
			this.MergeOrganizationContactsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.MergeOrganizationContactsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|b2b85b97-da1c-422d-afc0-39ab4b1e15f4", "Contacts");
			this.MergeOrganizationContactsGroupBox.Controls.Add(this.MergeContactsZGrid);
			this.MergeOrganizationContactsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 524, true);
			this.MergeOrganizationContactsGroupBox.Name = "MergeOrganizationContactsGroupBox";
			this.MergeOrganizationContactsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 122, true);
			this.MergeOrganizationContactsGroupBox.TabIndex = 5;
			this.MergeOrganizationContactsGroupBox.TabStop = false;
			// 
			// MergeContactsZGrid
			// 
			this.MergeContactsZGrid.AllowNavigation = false;
			this.MergeContactsZGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MergeContactsZGrid, "OldOrgContactCollectionForSimilarOrgsByName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgContactCollectionForSimilarOrgsByName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgContactCollectionForSimilarOrgsByName)).SyncRoot)).OldContactName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgContactCollectionForSimilarOrgsByName)).SyncRoot)).OldContactTitle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgContactCollectionForSimilarOrgsByName)).SyncRoot)).OldContactPhone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgContactCollectionForSimilarOrgsByName)).SyncRoot)).OldContactMobile)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MergeOrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgContactCollectionForSimilarOrgsByName)).SyncRoot)).Action)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.MergeOrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgContactCollectionForSimilarOrgsByName)).SyncRoot)).NewContactPK)));
			this.MergeContactsZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|f4c3a84a-4ba7-413f-b994-a9d0bebe7cd1", "Old Contact Name");
			zTextBoxColumnStyleInfo4.ColumnName = "OldContactName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|20f8566a-5d0d-40cc-9962-c0a77b68aa60", "Old Contact Title");
			zTextBoxColumnStyleInfo5.ColumnName = "OldContactTitle";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|b8462816-70fa-46d4-b8c1-bfeba53e65a5", "Old Contact Phone");
			zTextBoxColumnStyleInfo6.ColumnName = "OldContactPhone";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|97ed75b3-5500-48f8-b2ca-c5e72fab5220", "Old Contact Mobile");
			zTextBoxColumnStyleInfo7.ColumnName = "OldContactMobile";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|95559668-6b8b-42ef-8cbb-392ef0b01b70", "Action");
			zDropEditColumnStyleInfo2.ColumnName = "Action";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|904fc4c1-81e0-4b2f-a8b5-b3b3ace21f0f", "Match Contact");
			zGuidDropEditColumnStyleInfo2.ColumnName = "NewContactPK";
			this.MergeContactsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MergeContactsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MergeContactsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MergeContactsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MergeContactsZGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.MergeContactsZGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.MergeContactsZGrid.GridId = "8abc0e40-1b08-4eb9-9ca0-ba5a1bafd108";
			this.MergeContactsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MergeContactsZGrid.LayoutKey = "zGrid1";
			this.MergeContactsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 20, true);
			this.MergeContactsZGrid.Name = "MergeContactsZGrid";
			this.MergeContactsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 96, true);
			this.MergeContactsZGrid.TabIndex = 14;
			this.MergeContactsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			// 
			// NewOrganisationGroupBox
			// 
			this.NewOrganisationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.NewOrganisationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|5bc7c835-c5aa-45dc-b0b3-1d86f105bb18", "New Organization");
			this.NewOrganisationGroupBox.Controls.Add(this.NewOrgNameTextBox);
			this.NewOrganisationGroupBox.Controls.Add(this.NewOrgCodeTextBox);
			this.NewOrganisationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 12, true);
			this.NewOrganisationGroupBox.Name = "NewOrganisationGroupBox";
			this.NewOrganisationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 80, true);
			this.NewOrganisationGroupBox.TabIndex = 0;
			this.NewOrganisationGroupBox.TabStop = false;
			// 
			// NewOrgNameTextBox
			// 
			this.NewOrgNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.NewOrgNameTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.NewOrgNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|fca0446a-4a1d-49dd-a206-acc60c9c32be", "Name");
			this.NewOrgNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 48, true);
			this.NewOrgNameTextBox.Name = "NewOrgNameTextBox";
			this.NewOrgNameTextBox.ReadOnly = true;
			this.NewOrgNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 20, true);
			this.NewOrgNameTextBox.TabIndex = 1;
			// 
			// NewOrgCodeTextBox
			// 
			this.NewOrgCodeTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.NewOrgCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|f2e4f2a3-d3bf-4541-82a5-9a1bb67279e6", "Code");
			this.NewOrgCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 24, true);
			this.NewOrgCodeTextBox.Name = "NewOrgCodeTextBox";
			this.NewOrgCodeTextBox.ReadOnly = true;
			this.NewOrgCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.NewOrgCodeTextBox.TabIndex = 0;
			// 
			// OrgsToMergeGroupBox
			// 
			this.OrgsToMergeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OrgsToMergeGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|e08623e1-be4a-4d47-b123-703a813382fa", "Select Multiple Organizations To Merge Into The Above New Organization");
			this.OrgsToMergeGroupBox.Controls.Add(this.PatternParamPanel);
			this.OrgsToMergeGroupBox.Controls.Add(this.FindButton);
			this.OrgsToMergeGroupBox.Controls.Add(this.FindByPattern);
			this.OrgsToMergeGroupBox.Controls.Add(this.FindByCode);
			this.OrgsToMergeGroupBox.Controls.Add(this.FindByName);
			this.OrgsToMergeGroupBox.Controls.Add(this.AddAnotherOldOrgButton);
			this.OrgsToMergeGroupBox.Controls.Add(this.OldOrgGuidFindBox);
			this.OrgsToMergeGroupBox.Controls.Add(this.OldOrganisationsGrid);
			this.OrgsToMergeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 95, true);
			this.OrgsToMergeGroupBox.Name = "OrgsToMergeGroupBox";
			this.OrgsToMergeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 295, true);
			this.OrgsToMergeGroupBox.TabIndex = 3;
			this.OrgsToMergeGroupBox.TabStop = false;
			// 
			// PatternParamPanel
			// 
			this.PatternParamPanel.Controls.Add(this.MatchThreshold);
			this.PatternParamPanel.Controls.Add(this.MaxResults);
			this.PatternParamPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 43, true);
			this.PatternParamPanel.Name = "PatternParamPanel";
			this.PatternParamPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 28, true);
			this.PatternParamPanel.TabIndex = 13;
			// 
			// MatchThreshold
			// 
			this.BindingSource.SetBindingMember(this.MatchThreshold, "CurrentMatchThresholdCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).CurrentMatchThresholdCode)));
			this.MatchThreshold.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|d40f7660-c2f0-4432-8323-9f30ad66f281", "Match Threshold");
			this.MatchThreshold.Enabled = false;
			this.MatchThreshold.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 4, true);
			this.MatchThreshold.Name = "MatchThreshold";
			this.MatchThreshold.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.MatchThreshold.TabIndex = 6;
			// 
			// MaxResults
			// 
			this.BindingSource.SetBindingMember(this.MaxResults, "MaxResults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).MaxResults)));
			this.MaxResults.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|fbc25d92-0ef2-47cd-a501-bb59f4ef8d04", "Max Results to show");
			this.MaxResults.Decimals = 0;
			this.MaxResults.IsCalculatorEnabled = false;
			this.MaxResults.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(422, 3, true);
			this.MaxResults.Name = "MaxResults";
			this.MaxResults.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MaxResults.TabIndex = 8;
			this.MaxResults.Text = "0";
			this.MaxResults.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FindButton
			// 
			this.FindButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.FindButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|be74c668-f348-4a0f-aaa3-4e13d75dd0eb", "Find");
			this.FindButton.Font = new System.Drawing.Font(Enterprise.ZArchitecture.Core.OFont.NormalFontName, 8F, System.Drawing.FontStyle.Bold);
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(688, 41, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 23, true);
			this.FindButton.TabIndex = 9;
			this.FindButton.Click += new System.EventHandler(this.FindButton_Click);
			// 
			// FindByPattern
			// 
			this.FindByPattern.AutoCheck = false;
			this.FindByPattern.AutoSize = true;
			this.FindByPattern.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|2cd061cf-bb82-4d54-b879-3bab9735ddfd", "Find Similar Organizations by &Pattern");
			this.FindByPattern.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FindByPattern.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 20, true);
			this.FindByPattern.Name = "FindByPattern";
			this.FindByPattern.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 17, true);
			this.FindByPattern.TabIndex = 5;
			this.FindByPattern.TabStop = true;
			this.FindByPattern.UseVisualStyleBackColor = true;
			this.FindByPattern.CheckedChanged += new System.EventHandler(this.FindByPattern_CheckedChanged);
			// 
			// FindByCode
			// 
			this.FindByCode.AutoCheck = false;
			this.FindByCode.AutoSize = true;
			this.FindByCode.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|e0e5301e-c100-40c3-94da-129914872054", "Find Similar Organizations by &Code");
			this.FindByCode.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FindByCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 20, true);
			this.FindByCode.Name = "FindByCode";
			this.FindByCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 17, true);
			this.FindByCode.TabIndex = 4;
			this.FindByCode.TabStop = true;
			this.FindByCode.UseVisualStyleBackColor = true;
			this.FindByCode.CheckedChanged += new System.EventHandler(this.FindByCode_CheckedChanged);
			// 
			// FindByName
			// 
			this.FindByName.AutoCheck = false;
			this.FindByName.AutoSize = true;
			this.FindByName.Checked = true;
			this.FindByName.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|0c2cde26-7437-4ab6-b480-562be4c59097", "Find Similar Organizations by &Name");
			this.FindByName.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FindByName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 20, true);
			this.FindByName.Name = "FindByName";
			this.FindByName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 17, true);
			this.FindByName.TabIndex = 3;
			this.FindByName.TabStop = true;
			this.FindByName.UseVisualStyleBackColor = true;
			this.FindByName.CheckedChanged += new System.EventHandler(this.FindByName_CheckedChanged);
			// 
			// AddAnotherOldOrgButton
			// 
			this.AddAnotherOldOrgButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AddAnotherOldOrgButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|afd61325-2056-4677-9f72-90baaf59fc07", "Add to the list");
			this.AddAnotherOldOrgButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(670, 262, true);
			this.AddAnotherOldOrgButton.Name = "AddAnotherOldOrgButton";
			this.AddAnotherOldOrgButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 23, true);
			this.AddAnotherOldOrgButton.TabIndex = 12;
			this.AddAnotherOldOrgButton.Click += new System.EventHandler(this.AddAnotherOldOrgButton_Click);
			// 
			// OldOrgGuidFindBox
			// 
			this.OldOrgGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OldOrgGuidFindBox, "OrganisationPkForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OrganisationPkForBinding)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OldOrgGuidFindBox, false);
			this.OldOrgGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 262, true);
			this.OldOrgGuidFindBox.Name = "OldOrgGuidFindBox";
			this.OldOrgGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 20, true);
			this.OldOrgGuidFindBox.TabIndex = 11;
			// 
			// OldOrganisationsGrid
			// 
			this.OldOrganisationsGrid.AllowNavigation = false;
			this.OldOrganisationsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OldOrganisationsGrid, "OldOrgsCollectionByName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgsCollectionByName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgsCollectionByName)).SyncRoot)).OH_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgsCollectionByName)).SyncRoot)).OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgsCollectionByName)).SyncRoot)).MainAddress.OA_Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgsCollectionByName)).SyncRoot)).MainAddress.OA_Address2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgsCollectionByName)).SyncRoot)).OH_RL_NKClosestPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MergeOrgHeader)(null)).OldOrgsCollectionByName)).SyncRoot)).MainAddress.OA_City)));
			this.OldOrganisationsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|70931b3a-5f55-4f8b-aaf4-2ea9dd041ebc", "Code");
			zTextBoxColumnStyleInfo8.ColumnName = "OH_Code";
			zTextBoxColumnStyleInfo9.ColumnName = "OH_FullName";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|a485c8b8-ab3d-41f0-bd7d-d849101543ab", "Address 1");
			zTextBoxColumnStyleInfo10.ColumnName = "MainAddress+OA_Address1";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|c6a403c5-ff0a-4f39-9a3b-46062bbc81be", "Address 2");
			zTextBoxColumnStyleInfo11.ColumnName = "MainAddress+OA_Address2";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|eca53bd1-608e-48da-9718-af916ef9c07d", "UNLOCO");
			zTextBoxColumnStyleInfo12.ColumnName = "OH_RL_NKClosestPort";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo13.ColumnName = "MainAddress+OA_City";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.OldOrganisationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.OldOrganisationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.OldOrganisationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.OldOrganisationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.OldOrganisationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.OldOrganisationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.OldOrganisationsGrid.GridId = "208e497a-ddec-45d0-9c5f-560348d8926c";
			this.OldOrganisationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OldOrganisationsGrid.IsWholeRowSelectedOnClick = true;
			this.OldOrganisationsGrid.LayoutKey = "zGrid1";
			this.OldOrganisationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 77, true);
			this.OldOrganisationsGrid.Name = "OldOrganisationsGrid";
			this.OldOrganisationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 179, true);
			this.OldOrganisationsGrid.TabIndex = 10;
			this.OldOrganisationsGrid.RowsDeleting += new System.EventHandler<Enterprise.ZArchitecture.RowsDeletingEventArgs>(this.OldOrganisationsGrid_RowDeleting);
			// 
			// ProgressMerge
			// 
			this.ProgressMerge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 652, true);
			this.ProgressMerge.Name = "ProgressMerge";
			this.ProgressMerge.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 23, true);
			this.ProgressMerge.Step = 1;
			this.ProgressMerge.TabIndex = 6;
			// 
			// MergeIntoOrgHeaderForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 706, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MergeIntoOrgHeaderForm|d417fdf1-5e2d-43d5-9180-8eaff8c8b2dd", "Organizations");
			this.Controls.Add(this.ProgressMerge);
			this.Controls.Add(this.OrgsToMergeGroupBox);
			this.Controls.Add(this.NewOrganisationGroupBox);
			this.Controls.Add(this.MergeOrganizationContactsGroupBox);
			this.Controls.Add(this.MergeOrganizationAddressesGroupBox);
			this.Controls.Add(this.CancelMergeButton);
			this.Controls.Add(this.ProcessButton);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.MergeOrgHeader);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.MergeOrgHeader";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "MergeIntoOrgHeaderForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.ProcessButton, 0);
			this.Controls.SetChildIndex(this.CancelMergeButton, 0);
			this.Controls.SetChildIndex(this.MergeOrganizationAddressesGroupBox, 0);
			this.Controls.SetChildIndex(this.MergeOrganizationContactsGroupBox, 0);
			this.Controls.SetChildIndex(this.NewOrganisationGroupBox, 0);
			this.Controls.SetChildIndex(this.OrgsToMergeGroupBox, 0);
			this.Controls.SetChildIndex(this.ProgressMerge, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MergeOrganizationAddressesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MergeAddressesZGrid)).EndInit();
			this.MergeOrganizationContactsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MergeContactsZGrid)).EndInit();
			this.NewOrganisationGroupBox.ResumeLayout(false);
			this.NewOrganisationGroupBox.PerformLayout();
			this.OrgsToMergeGroupBox.ResumeLayout(false);
			this.OrgsToMergeGroupBox.PerformLayout();
			this.PatternParamPanel.ResumeLayout(false);
			this.PatternParamPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OldOrganisationsGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

	}
}
