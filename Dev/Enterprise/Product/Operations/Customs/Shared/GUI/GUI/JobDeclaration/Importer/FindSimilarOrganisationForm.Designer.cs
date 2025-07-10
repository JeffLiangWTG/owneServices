namespace Enterprise.Customs.GUI
{
	partial class FindSimilarOrganisationForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Enterprise.ZArchitecture.ZTextBox CityTextBox;
		private Enterprise.ZArchitecture.ZTextBox BusinessRegNoTextBox;
		private Enterprise.ZArchitecture.ZTextBox PostCodeTextBox;
		private Enterprise.ZArchitecture.ZTextBox Address2TextBox;
		protected Enterprise.ZArchitecture.ZTextBox Address1TextBox;
		private Enterprise.ZArchitecture.ZTextBox PhoneTextBox;
		private Enterprise.ZArchitecture.ZTextBox NameTextBox;
		private Enterprise.ZArchitecture.GUI.ZDisplayGrid SimilarOrgMatchesBoundGrid;
		private Enterprise.ZArchitecture.GUI.ZButton CreateNewButton;
		internal Enterprise.ZArchitecture.ZLabel NoRecordsLabel;
		private Enterprise.ZArchitecture.GUI.ZButton SkipButton;
		internal Enterprise.ZArchitecture.GUI.ZButton SkipAllButton;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox UNLOCOFindBox;
		private Enterprise.ZArchitecture.ZLabel OrganisationCodeLabel;
		private Enterprise.ZArchitecture.ZLabel HeadingLabel;
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.ZLabel SelectOrganisationLabel;

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CreateNewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BusinessRegNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Address2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Address1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SimilarOrgMatchesBoundGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
			this.SkipButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SkipAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OrganisationCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SelectOrganisationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NoRecordsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UNLOCOFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.HeadingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SimilarOrgMatchesBoundGrid)).BeginInit();
			this.SimilarOrgMatchesBoundGrid.SuspendLayout();
			this.UNLOCOFindBox.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 448, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 25, true);
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(332);
			//
			// ErrorStatusBarPanel
			//
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(333);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			//
			// CreateNewButton
			//
			this.CreateNewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CreateNewButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("B97078DB-86B7-4405-9351-2B7EF9005B39", "Create New");
			this.CreateNewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 418, true);
			this.CreateNewButton.Name = "CreateNewButton";
			this.CreateNewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CreateNewButton.TabIndex = 22;
			this.CreateNewButton.ToolTipCaption = null;
			this.CreateNewButton.Click += new System.EventHandler(this.CreateNewButton_Click);
			//
			// CityTextBox
			//
			this.BindingSource.SetBindingMember(this.CityTextBox, "MainAddress.OA_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MainAddress.OA_City)));
			this.CityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 112, true);
			this.CityTextBox.Name = "CityTextBox";
			this.CityTextBox.ReadOnly = true;
			this.CityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 19, true);
			this.CityTextBox.TabIndex = 8;
			//
			// BusinessRegNoTextBox
			//
			this.BindingSource.SetBindingMember(this.BusinessRegNoTextBox, "PrimaryRegistrationNumber+Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).PrimaryRegistrationNumber.Number)));
			this.BusinessRegNoTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("399cf2a8-29b0-4089-a6c9-b114a298a3b0", "Business Reg No");
			this.BusinessRegNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 88, true);
			this.BusinessRegNoTextBox.Name = "BusinessRegNoTextBox";
			this.BusinessRegNoTextBox.ReadOnly = true;
			this.BusinessRegNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 19, true);
			this.BusinessRegNoTextBox.TabIndex = 14;
			//
			// PostCodeTextBox
			//
			this.BindingSource.SetBindingMember(this.PostCodeTextBox, "MainAddress.OA_PostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MainAddress.OA_PostCode)));
			this.PostCodeTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("8c677b1f-e124-402c-9b65-4664a4861970", "Post code");
			this.PostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 40, true);
			this.PostCodeTextBox.Name = "PostCodeTextBox";
			this.PostCodeTextBox.ReadOnly = true;
			this.PostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 19, true);
			this.PostCodeTextBox.TabIndex = 10;
			//
			// Address2TextBox
			//
			this.BindingSource.SetBindingMember(this.Address2TextBox, "MainAddress.OA_Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MainAddress.OA_Address2)));
			this.Address2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 88, true);
			this.Address2TextBox.Name = "Address2TextBox";
			this.Address2TextBox.ReadOnly = true;
			this.Address2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 19, true);
			this.Address2TextBox.TabIndex = 6;
			//
			// Address1TextBox
			//
			this.BindingSource.SetBindingMember(this.Address1TextBox, "MainAddress.OA_Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MainAddress.OA_Address1)));
			this.Address1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 64, true);
			this.Address1TextBox.Name = "Address1TextBox";
			this.Address1TextBox.ReadOnly = true;
			this.Address1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 19, true);
			this.Address1TextBox.TabIndex = 5;
			//
			// PhoneTextBox
			//
			this.BindingSource.SetBindingMember(this.PhoneTextBox, "MainAddress.OA_Phone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MainAddress.OA_Phone)));
			this.PhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 64, true);
			this.PhoneTextBox.Name = "PhoneTextBox";
			this.PhoneTextBox.ReadOnly = true;
			this.PhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 19, true);
			this.PhoneTextBox.TabIndex = 12;
			//
			// NameTextBox
			//
			this.BindingSource.SetBindingMember(this.NameTextBox, "OH_FullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_FullName)));
			this.NameTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("6b76c8f7-62fe-401b-88db-8a239844f815", "Name");
			this.NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 40, true);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.ReadOnly = true;
			this.NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 19, true);
			this.NameTextBox.TabIndex = 3;
			//
			// SimilarOrgMatchesBoundGrid
			//
			this.SimilarOrgMatchesBoundGrid.AllowNavigation = false;
			this.SimilarOrgMatchesBoundGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SimilarOrgMatchesBoundGrid, "SimilarOrgMatches");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OS_Rank)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).LocalBusinessNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_Address2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_City)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_Email)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_Fax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_Phone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_PostCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_State)));
			this.SimilarOrgMatchesBoundGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "";
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("13493067-98C9-41F5-A4BE-F465CA984277", "Rank");
			zCalcEditColumnStyleInfo2.ColumnName = "OS_Rank";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo12.ColumnName = "OH_Code";
			zTextBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.ColumnName = "OH_FullName";
			zTextBoxColumnStyleInfo13.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo14.Caption = "";
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("F1F13364-DFB8-4D21-B26F-8655E9984B78", "Business Reg No.");
			zTextBoxColumnStyleInfo14.ColumnName = "LocalBusinessNumber";
			zTextBoxColumnStyleInfo14.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo15.Caption = "";
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("8F23A4C0-E2BF-4053-BE14-B148E6A97E8B", "Calc. _ Address 1");
			zTextBoxColumnStyleInfo15.ColumnName = "OH_Calc_Address1";
			zTextBoxColumnStyleInfo15.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo16.Caption = "";
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("77067934-CE1A-4304-A3B6-1FAC6922FC8D", "Calc. _ Address 2");
			zTextBoxColumnStyleInfo16.ColumnName = "OH_Calc_Address2";
			zTextBoxColumnStyleInfo16.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.Caption = "";
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("821949F4-0097-49AA-B798-9AC3BCCF3B26", "Calc. _ City");
			zTextBoxColumnStyleInfo17.ColumnName = "OH_Calc_City";
			zTextBoxColumnStyleInfo17.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo18.Caption = "";
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("49055D60-D57C-403B-BE88-60B9EC21603A", "Calc. _ Email");
			zTextBoxColumnStyleInfo18.ColumnName = "OH_Calc_Email";
			zTextBoxColumnStyleInfo18.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo19.Caption = "";
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("788D7B4C-FD8F-4878-A1D5-710906BF92C7", "Calc. _ Fax");
			zTextBoxColumnStyleInfo19.ColumnName = "OH_Calc_Fax";
			zTextBoxColumnStyleInfo19.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo20.Caption = "";
			zTextBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("DBC30B37-04A6-461E-8752-A046AB4487F1", "Calc. _ Phone");
			zTextBoxColumnStyleInfo20.ColumnName = "OH_Calc_Phone";
			zTextBoxColumnStyleInfo20.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo21.Caption = "";
			zTextBoxColumnStyleInfo21.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("73BAB76E-8C38-4F8F-90E6-E2EDDD40A782", "Calc. _ Post Code");
			zTextBoxColumnStyleInfo21.ColumnName = "OH_Calc_PostCode";
			zTextBoxColumnStyleInfo21.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo22.Caption = "";
			zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("8B86FE18-DDB9-4D6A-96BA-A9E14D253E92", "Calc. _ State");
			zTextBoxColumnStyleInfo22.ColumnName = "OH_Calc_State";
			zTextBoxColumnStyleInfo22.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.SimilarOrgMatchesBoundGrid.GridId = "125600a0-8503-4a1b-8325-0a887260e2ce";
			this.SimilarOrgMatchesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SimilarOrgMatchesBoundGrid.IsWholeRowSelectedOnClick = true;
			this.SimilarOrgMatchesBoundGrid.LayoutKey = "zGrid1";
			this.SimilarOrgMatchesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 168, true);
			this.SimilarOrgMatchesBoundGrid.Name = "SimilarOrgMatchesBoundGrid";
			this.SimilarOrgMatchesBoundGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.SimilarOrgMatchesBoundGrid.ShouldSetErrorsOnTabPage = false;
			this.SimilarOrgMatchesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 244, true);
			this.SimilarOrgMatchesBoundGrid.TabIndex = 18;
			this.SimilarOrgMatchesBoundGrid.DoubleClick += new System.EventHandler(this.SelectOrganisationFromGrid);
			//
			// SkipButton
			//
			this.SkipButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SkipButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ADDE88F7-AD35-4A2D-BDC5-E7A46674C794", "Skip");
			this.SkipButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 418, true);
			this.SkipButton.Name = "SkipButton";
			this.SkipButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SkipButton.TabIndex = 21;
			this.SkipButton.ToolTipCaption = null;
			this.SkipButton.Click += new System.EventHandler(this.SkipButton_Click);
			//
			// SkipAllButton
			//
			this.SkipAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SkipAllButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("846B2856-765F-40C0-8497-80292207E6D8", "Skip All");
			this.SkipAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 418, true);
			this.SkipAllButton.Name = "SkipAllButton";
			this.SkipAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SkipAllButton.TabIndex = 20;
			this.SkipAllButton.ToolTipCaption = null;
			this.SkipAllButton.Click += new System.EventHandler(this.SkipAllButton_Click);
			//
			// OrganisationCodeLabel
			//
			this.OrganisationCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.OrganisationCodeLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OrganisationCodeLabel, false);
			this.OrganisationCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 8, true);
			this.OrganisationCodeLabel.Name = "OrganisationCodeLabel";
			this.OrganisationCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 23, true);
			this.OrganisationCodeLabel.TabIndex = 1;
			this.OrganisationCodeLabel.UseMnemonic = false;
			//
			// SelectOrganisationLabel
			//
			this.SelectOrganisationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SelectOrganisationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 144, true);
			this.SelectOrganisationLabel.Name = "SelectOrganisationLabel";
			this.SelectOrganisationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 23, true);
			this.SelectOrganisationLabel.TabIndex = 17;
			this.SelectOrganisationLabel.UseMnemonic = false;
			//
			// NoRecordsLabel
			//
			this.NoRecordsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.NoRecordsLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("C9057445-EF1A-4900-BAB4-D328FC5C5486", "No similar organizations found.");
			this.NoRecordsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.NoRecordsLabel.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.NoRecordsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 192, true);
			this.NoRecordsLabel.Name = "NoRecordsLabel";
			this.NoRecordsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 16, true);
			this.NoRecordsLabel.TabIndex = 19;
			this.NoRecordsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.NoRecordsLabel.UseMnemonic = false;
			this.NoRecordsLabel.Visible = false;
			//
			// UNLOCOFindBox
			//
			this.UNLOCOFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UNLOCOFindBox, "OH_RL_NKClosestPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_RL_NKClosestPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Lookups.ClosestPorts)));
			this.UNLOCOFindBox.BindToList = "Lookups.ClosestPorts";
			this.UNLOCOFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("fe1e3c2e-f549-4b89-95ae-fa21744b1f77", "Closest Port");
			this.UNLOCOFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 112, true);
			this.UNLOCOFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.UNLOCOFindBox.Name = "UNLOCOFindBox";
			this.UNLOCOFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.UNLOCOFindBox.ParentType = null;
			this.UNLOCOFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 19, true);
			this.UNLOCOFindBox.TabIndex = 16;
			//
			// HeadingLabel
			//
			this.HeadingLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CA038290-D1DC-4526-B06B-9FDE5249E276", "Cannot find Organization with organization code: ");
			this.HeadingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.HeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.HeadingLabel.Name = "HeadingLabel";
			this.HeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 23, true);
			this.HeadingLabel.TabIndex = 0;
			this.HeadingLabel.UseMnemonic = false;
			//
			// OKButton
			//
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("0CED19BD-5613-4EBB-B1F3-97EA71ED1734", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 418, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 23;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new System.EventHandler(this.SelectOrganisationFromGrid);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			//
			// FindSimilarOrganisationForm
			//
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("49337451-B40A-451D-8215-36816E760CEF", "Select Organization");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 473, true);
			this.Controls.Add(this.OrganisationCodeLabel);
			this.Controls.Add(this.UNLOCOFindBox);
			this.Controls.Add(this.NoRecordsLabel);
			this.Controls.Add(this.SelectOrganisationLabel);
			this.Controls.Add(this.SimilarOrgMatchesBoundGrid);
			this.Controls.Add(this.CityTextBox);
			this.Controls.Add(this.BusinessRegNoTextBox);
			this.Controls.Add(this.PostCodeTextBox);
			this.Controls.Add(this.Address2TextBox);
			this.Controls.Add(this.Address1TextBox);
			this.Controls.Add(this.PhoneTextBox);
			this.Controls.Add(this.NameTextBox);
			this.Controls.Add(this.CreateNewButton);
			this.Controls.Add(this.SkipButton);
			this.Controls.Add(this.SkipAllButton);
			this.Controls.Add(this.HeadingLabel);
			this.Controls.Add(this.OKButton);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 448, true);
			this.Name = "FindSimilarOrganisationForm";
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.HeadingLabel, 0);
			this.Controls.SetChildIndex(this.SkipAllButton, 0);
			this.Controls.SetChildIndex(this.SkipButton, 0);
			this.Controls.SetChildIndex(this.CreateNewButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.NameTextBox, 0);
			this.Controls.SetChildIndex(this.PhoneTextBox, 0);
			this.Controls.SetChildIndex(this.Address1TextBox, 0);
			this.Controls.SetChildIndex(this.Address2TextBox, 0);
			this.Controls.SetChildIndex(this.PostCodeTextBox, 0);
			this.Controls.SetChildIndex(this.BusinessRegNoTextBox, 0);
			this.Controls.SetChildIndex(this.CityTextBox, 0);
			this.Controls.SetChildIndex(this.SimilarOrgMatchesBoundGrid, 0);
			this.Controls.SetChildIndex(this.SelectOrganisationLabel, 0);
			this.Controls.SetChildIndex(this.NoRecordsLabel, 0);
			this.Controls.SetChildIndex(this.UNLOCOFindBox, 0);
			this.Controls.SetChildIndex(this.OrganisationCodeLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SimilarOrgMatchesBoundGrid)).EndInit();
			this.SimilarOrgMatchesBoundGrid.ResumeLayout(false);
			this.SimilarOrgMatchesBoundGrid.PerformLayout();
			this.UNLOCOFindBox.ResumeLayout(true);
			this.UNLOCOFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
	}
}
