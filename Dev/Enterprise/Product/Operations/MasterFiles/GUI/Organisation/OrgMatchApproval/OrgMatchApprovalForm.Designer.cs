namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgMatchApprovalForm
	{

		#region Component Designer generated code

		protected Enterprise.MasterFiles.GUI.SimilarOrgMatchesModuleButtonGrid SimilarOrgMatchesModuleButtonGrid;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox OrgToBeMatchedDetailsGroupBox;
		private Enterprise.ZArchitecture.ZTextBox Address1BoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox CityBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox StateBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox ClosestPortBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox PostCodeBoundTextBox;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox CandidateOrganisationGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AddressesGroupBox;
		private Enterprise.ZArchitecture.ZTextBox SimilarFullNameBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox SimilarStreetBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox SimilarCityBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox SimilarCountryBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox SimilarPostcodeBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox SimilarOrgTypesBoundTextBox;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseBoundButton;
		protected Enterprise.ZArchitecture.GUI.ZButton MatchBoundButton;
		protected Enterprise.ZArchitecture.GUI.ZButton SupervisorMatchBoundButton;
		protected Enterprise.ZArchitecture.GUI.ZButton NewOrganisationBoundButton;
		protected Enterprise.ZArchitecture.GUI.ZButton NoMatchFoundBoundButton;
		protected Enterprise.ZArchitecture.ZTextBox CompanyNameBoundTextBox;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox ActiveMatchesGroupBox;
		private Enterprise.ZArchitecture.ZTextBox MatchedByBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox MatchedNameTextBox;
		private Enterprise.ZArchitecture.ZTextBox MatchedName2TextBox;
		private Enterprise.ZArchitecture.ZTextBox MatchedByBound2TextBox;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox PossibleMatchingOrgsGroupBox;
		private Enterprise.MasterFiles.GUI.OrgAddressGrid OrgAddressGrid;
		private Enterprise.ZArchitecture.ZLabel zLabel8;
		private Enterprise.ZArchitecture.ZTextBox PhoneBoundTextBox;
		protected Enterprise.ZArchitecture.ZTextBox OwnerCodeBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox SimilarOrgTextBox;
		private Enterprise.ZArchitecture.ZTextBox Street2TextBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ManuallySelectedOrganisationGuidFindBox;
		private Enterprise.ZArchitecture.ZTextBox BusinessRegNoTextBox;

		protected override void InitializeComponent()
		{
			this.CloseBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PossibleMatchingOrgsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ManuallySelectedOrganisationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SimilarOrgMatchesModuleButtonGrid = new Enterprise.MasterFiles.GUI.SimilarOrgMatchesModuleButtonGrid();
			this.CompanyNameBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OrgToBeMatchedDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.Street2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PhoneBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OwnerCodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PostCodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClosestPortBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StateBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CityBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Address1BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CandidateOrganisationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BusinessRegNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SimilarOrgTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SimilarOrgTypesBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SimilarPostcodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SimilarCountryBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SimilarCityBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SimilarStreetBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SimilarFullNameBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AddressesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OrgAddressGrid = new Enterprise.MasterFiles.GUI.OrgAddressGrid();
			this.MatchBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SupervisorMatchBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NewOrganisationBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NoMatchFoundBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ActiveMatchesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MatchedName2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MatchedByBound2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MatchedNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MatchedByBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PossibleMatchingOrgsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SimilarOrgMatchesModuleButtonGrid.InnerGrid)).BeginInit();
			this.OrgToBeMatchedDetailsGroupBox.SuspendLayout();
			this.CandidateOrganisationGroupBox.SuspendLayout();
			this.AddressesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrgAddressGrid)).BeginInit();
			this.ActiveMatchesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 639, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 23, true);
			this.MainStatusBar.TabIndex = 10;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(482);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(483);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgMatchApproval);
			// 
			// CloseBoundButton
			// 
			this.CloseBoundButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseBoundButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|05e4ca57-d46c-4bec-bcd5-a58ff5f8e6d9", "Close");
			this.CloseBoundButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(888, 609, true);
			this.CloseBoundButton.Name = "CloseBoundButton";
			this.CloseBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 21, true);
			this.CloseBoundButton.TabIndex = 9;
			this.CloseBoundButton.Click += new System.EventHandler(this.OnClose_Click);
			// 
			// PossibleMatchingOrgsGroupBox
			// 
			this.PossibleMatchingOrgsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.PossibleMatchingOrgsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|0ed74725-6b7e-493d-88dc-2de89344cfce", "Possible Matching Organizations");
			this.PossibleMatchingOrgsGroupBox.Controls.Add(this.ManuallySelectedOrganisationGuidFindBox);
			this.PossibleMatchingOrgsGroupBox.Controls.Add(this.SimilarOrgMatchesModuleButtonGrid);
			this.PossibleMatchingOrgsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 127, true);
			this.PossibleMatchingOrgsGroupBox.Name = "PossibleMatchingOrgsGroupBox";
			this.PossibleMatchingOrgsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 277, true);
			this.PossibleMatchingOrgsGroupBox.TabIndex = 2;
			this.PossibleMatchingOrgsGroupBox.TabStop = false;
			// 
			// ManuallySelectedOrganisationGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.ManuallySelectedOrganisationGuidFindBox, "P2_OH_ManuallySelectedOrganisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).P2_OH_ManuallySelectedOrganisation)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ManuallySelectedOrganisationGuidFindBox, false);
			this.ManuallySelectedOrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(784, 246, true);
			this.ManuallySelectedOrganisationGuidFindBox.Name = "ManuallySelectedOrganisationGuidFindBox";
			this.ManuallySelectedOrganisationGuidFindBox.PopupCaption = null;
			this.ManuallySelectedOrganisationGuidFindBox.ShowDescriptionBox = false;
			this.ManuallySelectedOrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.ManuallySelectedOrganisationGuidFindBox.TabIndex = 1;
			// 
			// SimilarOrgMatchesModuleButtonGrid
			// 
			this.BindingSource.SetBindingMember(this.SimilarOrgMatchesModuleButtonGrid, "SimilarOrgMatchesSortedByRank");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).SimilarOrgMatchesSortedByRank)));
			this.SimilarOrgMatchesModuleButtonGrid.GridId = "c9ef3595-c46a-4ee1-b0a8-ad3e7d4c346a";
			this.SimilarOrgMatchesModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// 
			// 
			this.SimilarOrgMatchesModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.SimilarOrgMatchesModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SimilarOrgMatchesModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.SimilarOrgMatchesModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SimilarOrgMatchesModuleButtonGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.SimilarOrgMatchesModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.SimilarOrgMatchesModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.SimilarOrgMatchesModuleButtonGrid.InnerGrid.Name = "Grid";
			this.SimilarOrgMatchesModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(956, 220, true);
			this.SimilarOrgMatchesModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.SimilarOrgMatchesModuleButtonGrid.InnerGrid.ColourDeciding += new System.EventHandler<Enterprise.ZArchitecture.ColourDecidingEventArgs>(this.OnSimilarOrgGrid_ColourDeciding);
			this.SimilarOrgMatchesModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SimilarOrgMatchesModuleButtonGrid.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.SimilarOrgMatchesModuleButtonGrid.Name = "SimilarOrgMatchesModuleButtonGrid";
			this.SimilarOrgMatchesModuleButtonGrid.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("E65A0680-4BEA-4068-A5F7-4EC11D9511B9", "Organization");
			this.SimilarOrgMatchesModuleButtonGrid.ReadOnly = false;
			this.SimilarOrgMatchesModuleButtonGrid.ShowAttachButton = false;
			this.SimilarOrgMatchesModuleButtonGrid.ShowDetachButton = false;
			this.SimilarOrgMatchesModuleButtonGrid.ShowNewButton = false;
			this.SimilarOrgMatchesModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 258, true);
			this.SimilarOrgMatchesModuleButtonGrid.TabIndex = 0;
			// 
			// CompanyNameBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CompanyNameBoundTextBox, "CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).CompanyName)));
			this.CompanyNameBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CompanyNameBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DummyOrgMatchApprovalForm|4fab4c83-e973-4837-aab1-7b5ea3944be3", "Company Name");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.CompanyNameBoundTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.CompanyNameBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 37, true);
			this.CompanyNameBoundTextBox.Name = "CompanyNameBoundTextBox";
			this.CompanyNameBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.CompanyNameBoundTextBox.TabIndex = 0;
			// 
			// OrgToBeMatchedDetailsGroupBox
			// 
			this.OrgToBeMatchedDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OrgToBeMatchedDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|846d64f4-a1b3-4e93-8d39-c24ec483ff62", "Find a Match for This Organization");
			this.OrgToBeMatchedDetailsGroupBox.Controls.Add(this.Street2TextBox);
			this.OrgToBeMatchedDetailsGroupBox.Controls.Add(this.PhoneBoundTextBox);
			this.OrgToBeMatchedDetailsGroupBox.Controls.Add(this.OwnerCodeBoundTextBox);
			this.OrgToBeMatchedDetailsGroupBox.Controls.Add(this.PostCodeBoundTextBox);
			this.OrgToBeMatchedDetailsGroupBox.Controls.Add(this.ClosestPortBoundTextBox);
			this.OrgToBeMatchedDetailsGroupBox.Controls.Add(this.StateBoundTextBox);
			this.OrgToBeMatchedDetailsGroupBox.Controls.Add(this.CityBoundTextBox);
			this.OrgToBeMatchedDetailsGroupBox.Controls.Add(this.Address1BoundTextBox);
			this.OrgToBeMatchedDetailsGroupBox.Controls.Add(this.CompanyNameBoundTextBox);
			this.OrgToBeMatchedDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 55, true);
			this.OrgToBeMatchedDetailsGroupBox.Name = "OrgToBeMatchedDetailsGroupBox";
			this.OrgToBeMatchedDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 67, true);
			this.OrgToBeMatchedDetailsGroupBox.TabIndex = 1;
			this.OrgToBeMatchedDetailsGroupBox.TabStop = false;
			// 
			// Street2TextBox
			// 
			this.BindingSource.SetBindingMember(this.Street2TextBox, "Street2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).Street2)));
			this.Street2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.Street2TextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Street2TextBox, false);
			this.Street2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(387, 37, true);
			this.Street2TextBox.Name = "Street2TextBox";
			this.Street2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.Street2TextBox.TabIndex = 2;
			// 
			// PhoneBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.PhoneBoundTextBox, "Phone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).Phone)));
			this.PhoneBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PhoneBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|269e11c6-af06-44e4-913a-8ebf38c0bc80", "Phone");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.PhoneBoundTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.PhoneBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(812, 37, true);
			this.PhoneBoundTextBox.Name = "PhoneBoundTextBox";
			this.PhoneBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.PhoneBoundTextBox.TabIndex = 7;
			// 
			// OwnerCodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OwnerCodeBoundTextBox, "OwnerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).OwnerCode)));
			this.OwnerCodeBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OwnerCodeBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|038987cd-1352-463e-a65d-430bdcee0cc3", "Owner Code");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.OwnerCodeBoundTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.OwnerCodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(893, 37, true);
			this.OwnerCodeBoundTextBox.Name = "OwnerCodeBoundTextBox";
			this.OwnerCodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.OwnerCodeBoundTextBox.TabIndex = 8;
			// 
			// PostCodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.PostCodeBoundTextBox, "PostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).PostCode)));
			this.PostCodeBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PostCodeBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|c68e5df4-75eb-4fc1-8688-b5cdb899cb46", "Postcode");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.PostCodeBoundTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.PostCodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(755, 37, true);
			this.PostCodeBoundTextBox.Name = "PostCodeBoundTextBox";
			this.PostCodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.PostCodeBoundTextBox.TabIndex = 6;
			// 
			// ClosestPortBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClosestPortBoundTextBox, "UNLoco");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).UNLoco)));
			this.ClosestPortBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClosestPortBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|dbd6db47-6ebc-4f6d-9a9f-969c7e965467", "UNLOCO");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ClosestPortBoundTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.ClosestPortBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(714, 37, true);
			this.ClosestPortBoundTextBox.Name = "ClosestPortBoundTextBox";
			this.ClosestPortBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.ClosestPortBoundTextBox.TabIndex = 5;
			// 
			// StateBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.StateBoundTextBox, "State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).State)));
			this.StateBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|cad48403-68ad-4d7e-904e-0fe5e34a75de", "State");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.StateBoundTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.StateBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(665, 37, true);
			this.StateBoundTextBox.Name = "StateBoundTextBox";
			this.StateBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.StateBoundTextBox.TabIndex = 4;
			// 
			// CityBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CityBoundTextBox, "City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).City)));
			this.CityBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CityBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|4e8de3fa-132d-47db-aaeb-8eacdf98a4a9", "City");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.CityBoundTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.CityBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 37, true);
			this.CityBoundTextBox.Name = "CityBoundTextBox";
			this.CityBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.CityBoundTextBox.TabIndex = 3;
			// 
			// Address1BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.Address1BoundTextBox, "Street");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).Street)));
			this.Address1BoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Address1BoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|4bbd39c8-76fb-48ba-8438-a8881d53071a", "Street");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.Address1BoundTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.Address1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(198, 37, true);
			this.Address1BoundTextBox.Name = "Address1BoundTextBox";
			this.Address1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.Address1BoundTextBox.TabIndex = 1;
			// 
			// CandidateOrganisationGroupBox
			// 
			this.CandidateOrganisationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.CandidateOrganisationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|41e588f8-13d5-4389-990c-f0b126111a0d", "Candidate Organization");
			this.CandidateOrganisationGroupBox.Controls.Add(this.BusinessRegNoTextBox);
			this.CandidateOrganisationGroupBox.Controls.Add(this.SimilarOrgTextBox);
			this.CandidateOrganisationGroupBox.Controls.Add(this.SimilarOrgTypesBoundTextBox);
			this.CandidateOrganisationGroupBox.Controls.Add(this.SimilarPostcodeBoundTextBox);
			this.CandidateOrganisationGroupBox.Controls.Add(this.SimilarCountryBoundTextBox);
			this.CandidateOrganisationGroupBox.Controls.Add(this.SimilarCityBoundTextBox);
			this.CandidateOrganisationGroupBox.Controls.Add(this.SimilarStreetBoundTextBox);
			this.CandidateOrganisationGroupBox.Controls.Add(this.SimilarFullNameBoundTextBox);
			this.CandidateOrganisationGroupBox.Controls.Add(this.AddressesGroupBox);
			this.CandidateOrganisationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 410, true);
			this.CandidateOrganisationGroupBox.Name = "CandidateOrganisationGroupBox";
			this.CandidateOrganisationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 191, true);
			this.CandidateOrganisationGroupBox.TabIndex = 3;
			this.CandidateOrganisationGroupBox.TabStop = false;
			// 
			// BusinessRegNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.BusinessRegNoTextBox, "SimilarOrgMatchesSortedByRank.OrgPatternMatch+OS_BusinessRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SimilarOrgMatchForApproval)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).SimilarOrgMatchesSortedByRank)).SyncRoot)).OrgPatternMatch.OS_BusinessRegNo)));
			this.BusinessRegNoTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BusinessRegNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 140, true);
			this.BusinessRegNoTextBox.Name = "BusinessRegNoTextBox";
			this.BusinessRegNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.BusinessRegNoTextBox.TabIndex = 6;
			// 
			// SimilarOrgTextBox
			// 
			this.BindingSource.SetBindingMember(this.SimilarOrgTextBox, "SimilarOrgMatchesSortedByRank.OrgPatternMatch+OH_Calc_Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SimilarOrgMatchForApproval)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).SimilarOrgMatchesSortedByRank)).SyncRoot)).OrgPatternMatch.OH_Calc_Address2)));
			this.SimilarOrgTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SimilarOrgTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|837ed95f-a974-41ee-a187-f27a016e5f4d", "Street 2", "Address 2", "");
			this.SimilarOrgTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 58, true);
			this.SimilarOrgTextBox.Name = "SimilarOrgTextBox";
			this.SimilarOrgTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.SimilarOrgTextBox.TabIndex = 2;
			// 
			// SimilarOrgTypesBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.SimilarOrgTypesBoundTextBox, "SimilarOrgMatchesSortedByRank.OrgPatternMatch+Header+OrganisationTypesAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SimilarOrgMatchForApproval)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).SimilarOrgMatchesSortedByRank)).SyncRoot)).OrgPatternMatch.Header.OrganisationTypesAsString)));
			this.SimilarOrgTypesBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SimilarOrgTypesBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|09db5232-009d-4157-98ec-6b70fca211f2", "Org. Type(s)", "Organization Type(s).");
			this.SimilarOrgTypesBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 161, true);
			this.SimilarOrgTypesBoundTextBox.Name = "SimilarOrgTypesBoundTextBox";
			this.SimilarOrgTypesBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.SimilarOrgTypesBoundTextBox.TabIndex = 7;
			// 
			// SimilarPostcodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.SimilarPostcodeBoundTextBox, "SimilarOrgMatchesSortedByRank.OrgPatternMatch+OH_Calc_PostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SimilarOrgMatchForApproval)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).SimilarOrgMatchesSortedByRank)).SyncRoot)).OrgPatternMatch.OH_Calc_PostCode)));
			this.SimilarPostcodeBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SimilarPostcodeBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|cf47abae-98dc-4e05-a5c1-e29e11d6a902", "P. Code", "Post Code", "");
			this.SimilarPostcodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 120, true);
			this.SimilarPostcodeBoundTextBox.Name = "SimilarPostcodeBoundTextBox";
			this.SimilarPostcodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.SimilarPostcodeBoundTextBox.TabIndex = 5;
			// 
			// SimilarCountryBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.SimilarCountryBoundTextBox, "SimilarOrgMatchesSortedByRank.ClosestPortCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SimilarOrgMatchForApproval)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).SimilarOrgMatchesSortedByRank)).SyncRoot)).ClosestPortCode)));
			this.SimilarCountryBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SimilarCountryBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|f659a281-6ab3-4857-8072-1e9981b7915a", "UNLOCO");
			this.SimilarCountryBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 99, true);
			this.SimilarCountryBoundTextBox.Name = "SimilarCountryBoundTextBox";
			this.SimilarCountryBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.SimilarCountryBoundTextBox.TabIndex = 4;
			// 
			// SimilarCityBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.SimilarCityBoundTextBox, "SimilarOrgMatchesSortedByRank.OrgPatternMatch+OH_Calc_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SimilarOrgMatchForApproval)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).SimilarOrgMatchesSortedByRank)).SyncRoot)).OrgPatternMatch.OH_Calc_City)));
			this.SimilarCityBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SimilarCityBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|23f5dcf2-267b-4707-bcbe-976c5bb61291", "City", "City", "");
			this.SimilarCityBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 79, true);
			this.SimilarCityBoundTextBox.Name = "SimilarCityBoundTextBox";
			this.SimilarCityBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.SimilarCityBoundTextBox.TabIndex = 3;
			// 
			// SimilarStreetBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.SimilarStreetBoundTextBox, "SimilarOrgMatchesSortedByRank.OrgPatternMatch+OH_Calc_Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SimilarOrgMatchForApproval)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).SimilarOrgMatchesSortedByRank)).SyncRoot)).OrgPatternMatch.OH_Calc_Address1)));
			this.SimilarStreetBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SimilarStreetBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|d99b2aad-d891-4bf7-9247-f7a49e69bd08", "Street", "Address 1", "");
			this.SimilarStreetBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 38, true);
			this.SimilarStreetBoundTextBox.Name = "SimilarStreetBoundTextBox";
			this.SimilarStreetBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.SimilarStreetBoundTextBox.TabIndex = 1;
			// 
			// SimilarFullNameBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.SimilarFullNameBoundTextBox, "SimilarOrgMatchesSortedByRank.OrgPatternMatch+OH_FullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SimilarOrgMatchForApproval)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).SimilarOrgMatchesSortedByRank)).SyncRoot)).OrgPatternMatch.OH_FullName)));
			this.SimilarFullNameBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SimilarFullNameBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|7a04e9f8-410b-4a2b-9735-f6607cd42aff", "Full Name");
			this.SimilarFullNameBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 18, true);
			this.SimilarFullNameBoundTextBox.Name = "SimilarFullNameBoundTextBox";
			this.SimilarFullNameBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.SimilarFullNameBoundTextBox.TabIndex = 0;
			// 
			// AddressesGroupBox
			// 
			this.AddressesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.AddressesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|8ed0fcf1-b432-40dc-9e29-bffc3f8ab976", "Addresses");
			this.AddressesGroupBox.Controls.Add(this.OrgAddressGrid);
			this.AddressesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(425, 8, true);
			this.AddressesGroupBox.Name = "AddressesGroupBox";
			this.AddressesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 178, true);
			this.AddressesGroupBox.TabIndex = 0;
			this.AddressesGroupBox.TabStop = false;
			// 
			// OrgAddressGrid
			// 
			this.OrgAddressGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OrgAddressGrid, "SimilarOrgMatchesSortedByRank.Addresses");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SimilarOrgMatchForApproval)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).SimilarOrgMatchesSortedByRank)).SyncRoot)).Addresses)));
			this.OrgAddressGrid.CaptionVisible = false;
			this.OrgAddressGrid.GridId = "b51ddb2c-54b8-4c78-a27b-7bbd2fd44b13";
			this.OrgAddressGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrgAddressGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgAddressGrid.LayoutKey = "OrgAddressGrid";
			this.OrgAddressGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OrgAddressGrid.Name = "OrgAddressGrid";
			this.OrgAddressGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 159, true);
			this.OrgAddressGrid.TabIndex = 0;
			// 
			// MatchBoundButton
			// 
			this.MatchBoundButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.MatchBoundButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|628c816b-9b56-4661-bc9e-6e1dec8f09c0", "Match");
			this.MatchBoundButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.MatchBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(704, 609, true);
			this.MatchBoundButton.Name = "MatchBoundButton";
			this.MatchBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 21, true);
			this.MatchBoundButton.TabIndex = 7;
			this.MatchBoundButton.Click += new System.EventHandler(this.OnMatch_Click);
			// 
			// SupervisorMatchBoundButton
			// 
			this.SupervisorMatchBoundButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SupervisorMatchBoundButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|1c1c6214-9fb3-4d6c-96de-412f6b08dc86", "Supervisor Approve");
			this.SupervisorMatchBoundButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.SupervisorMatchBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(321, 609, true);
			this.SupervisorMatchBoundButton.Name = "SupervisorMatchBoundButton";
			this.SupervisorMatchBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 21, true);
			this.SupervisorMatchBoundButton.TabIndex = 5;
			this.SupervisorMatchBoundButton.Click += new System.EventHandler(this.OnSupervisorApprove_Click);
			// 
			// NewOrganisationBoundButton
			// 
			this.NewOrganisationBoundButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.NewOrganisationBoundButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|d891c600-535e-447a-a323-157c8a1d5320", "New Organization");
			this.NewOrganisationBoundButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.NewOrganisationBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(441, 609, true);
			this.NewOrganisationBoundButton.Name = "NewOrganisationBoundButton";
			this.NewOrganisationBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 21, true);
			this.NewOrganisationBoundButton.TabIndex = 6;
			this.NewOrganisationBoundButton.Click += new System.EventHandler(this.OnNewOrganisation_Click);
			// 
			// NoMatchFoundBoundButton
			// 
			this.NoMatchFoundBoundButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NoMatchFoundBoundButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|9ab1df1e-cdd1-4e77-a77d-de692239ebbf", "No Match Found");
			this.NoMatchFoundBoundButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.NoMatchFoundBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(776, 609, true);
			this.NoMatchFoundBoundButton.Name = "NoMatchFoundBoundButton";
			this.NoMatchFoundBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 21, true);
			this.NoMatchFoundBoundButton.TabIndex = 8;
			this.NoMatchFoundBoundButton.Click += new System.EventHandler(this.OnNoMatchFound_Click);
			// 
			// ActiveMatchesGroupBox
			// 
			this.ActiveMatchesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|75984c1e-f136-4c1b-9d78-c98e4e12459c", "Active Matches");
			this.ActiveMatchesGroupBox.Controls.Add(this.MatchedName2TextBox);
			this.ActiveMatchesGroupBox.Controls.Add(this.MatchedByBound2TextBox);
			this.ActiveMatchesGroupBox.Controls.Add(this.MatchedNameTextBox);
			this.ActiveMatchesGroupBox.Controls.Add(this.MatchedByBoundTextBox);
			this.ActiveMatchesGroupBox.Enabled = false;
			this.ActiveMatchesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 6, true);
			this.ActiveMatchesGroupBox.Name = "ActiveMatchesGroupBox";
			this.ActiveMatchesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 44, true);
			this.ActiveMatchesGroupBox.TabIndex = 0;
			this.ActiveMatchesGroupBox.TabStop = false;
			// 
			// MatchedName2TextBox
			// 
			this.BindingSource.SetBindingMember(this.MatchedName2TextBox, "P2_MatchUserFullName2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).P2_MatchUserFullName2)));
			this.MatchedName2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MatchedName2TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|29ba6c4e-7bff-4c8f-8a2c-a7bf214676d4", "By");
			this.MatchedName2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 17, true);
			this.MatchedName2TextBox.Name = "MatchedName2TextBox";
			this.MatchedName2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.MatchedName2TextBox.TabIndex = 3;
			// 
			// MatchedByBound2TextBox
			// 
			this.BindingSource.SetBindingMember(this.MatchedByBound2TextBox, "P2_MatchOrgCode2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).P2_MatchOrgCode2)));
			this.MatchedByBound2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MatchedByBound2TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|f07f28ab-4d30-44da-83d2-1a0e6ec4761b", "Match 2");
			this.MatchedByBound2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 17, true);
			this.MatchedByBound2TextBox.Name = "MatchedByBound2TextBox";
			this.MatchedByBound2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.MatchedByBound2TextBox.TabIndex = 2;
			// 
			// MatchedNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.MatchedNameTextBox, "P2_MatchUserFullName1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).P2_MatchUserFullName1)));
			this.MatchedNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MatchedNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|9c019fa7-e964-4f2d-9bf2-3213c57989c3", "By");
			this.MatchedNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 17, true);
			this.MatchedNameTextBox.Name = "MatchedNameTextBox";
			this.MatchedNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.MatchedNameTextBox.TabIndex = 1;
			// 
			// MatchedByBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.MatchedByBoundTextBox, "P2_MatchOrgCode1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgMatchApproval)(null)).P2_MatchOrgCode1)));
			this.MatchedByBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MatchedByBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|248c69ea-4470-44bf-be19-d0a4f5adc482", "Match 1");
			this.MatchedByBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 17, true);
			this.MatchedByBoundTextBox.Name = "MatchedByBoundTextBox";
			this.MatchedByBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 20, true);
			this.MatchedByBoundTextBox.TabIndex = 0;
			// 
			// zLabel8
			// 
			this.zLabel8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.zLabel8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMatchApprovalForm|77feff19-02c7-4e70-95ce-a43fc25e7f3e", "Select an item in the grid on the main form to continue");
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 609, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 21, true);
			this.zLabel8.TabIndex = 4;
			// 
			// OrgMatchApprovalForm
			// 
			this.CancelButton = this.CloseBoundButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 662, true);
			this.Controls.Add(this.zLabel8);
			this.Controls.Add(this.ActiveMatchesGroupBox);
			this.Controls.Add(this.SupervisorMatchBoundButton);
			this.Controls.Add(this.NewOrganisationBoundButton);
			this.Controls.Add(this.MatchBoundButton);
			this.Controls.Add(this.CandidateOrganisationGroupBox);
			this.Controls.Add(this.OrgToBeMatchedDetailsGroupBox);
			this.Controls.Add(this.PossibleMatchingOrgsGroupBox);
			this.Controls.Add(this.CloseBoundButton);
			this.Controls.Add(this.NoMatchFoundBoundButton);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgMatchApproval);
			this.Name = "OrgMatchApprovalForm";
			this.Controls.SetChildIndex(this.NoMatchFoundBoundButton, 0);
			this.Controls.SetChildIndex(this.CloseBoundButton, 0);
			this.Controls.SetChildIndex(this.PossibleMatchingOrgsGroupBox, 0);
			this.Controls.SetChildIndex(this.OrgToBeMatchedDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.CandidateOrganisationGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MatchBoundButton, 0);
			this.Controls.SetChildIndex(this.NewOrganisationBoundButton, 0);
			this.Controls.SetChildIndex(this.SupervisorMatchBoundButton, 0);
			this.Controls.SetChildIndex(this.ActiveMatchesGroupBox, 0);
			this.Controls.SetChildIndex(this.zLabel8, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PossibleMatchingOrgsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SimilarOrgMatchesModuleButtonGrid.InnerGrid)).EndInit();
			this.OrgToBeMatchedDetailsGroupBox.ResumeLayout(false);
			this.OrgToBeMatchedDetailsGroupBox.PerformLayout();
			this.CandidateOrganisationGroupBox.ResumeLayout(false);
			this.CandidateOrganisationGroupBox.PerformLayout();
			this.AddressesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.OrgAddressGrid)).EndInit();
			this.ActiveMatchesGroupBox.ResumeLayout(false);
			this.ActiveMatchesGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

	}
}
