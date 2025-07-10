namespace Enterprise.MasterFiles.GUI
{
	partial class ComplianceReportConfigurationControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo12 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo13 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo14 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo15 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo16 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo17 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo18 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo19 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ParentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ParentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.GridSplitter = new CargoWise.Windows.UI.KSplitter();
			this.ChildGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChildGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ParentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).BeginInit();
			this.ParentGrid.SuspendLayout();
			this.ChildGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChildGrid)).BeginInit();
			this.ChildGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ComplianceReportConfigurationCollection);
			// 
			// ParentGroupBox
			// 
			this.ParentGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e1ffec56-1485-4c61-a5f3-7b741f057b28", "Reports");
			this.ParentGroupBox.Controls.Add(this.ParentGrid);
			this.ParentGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ParentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ParentGroupBox.Name = "ParentGroupBox";
			this.ParentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 128, true);
			this.ParentGroupBox.TabIndex = 2;
			this.ParentGroupBox.TabStop = false;
			// 
			// ParentGrid
			// 
			this.ParentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ParentGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).Country)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).ReportCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).ReportTitle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).TaxRegistrationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).RepCountryRegistrationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).ReportPeriodicity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).ReportBaseTablePrefix)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).ReportLineGrouping)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).ReportLineOrdering)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).GoodsServiceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).ReportAmountsRoundingType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).ReportAmountsRoundingTruncating)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).AmountThresholdLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).ExTaxAmountThreshold)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).TaxAmountThreshold)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).RecipientOrgPK)));
			this.ParentGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("123b2048-bb45-4602-9601-c788b9f73c25", "Country/Region");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Country";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d7ed9bd6-8508-4db7-a2d5-5fef5e1c2dad", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "ReportCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5c909c73-5074-4919-8edb-4647c1a75933", "Title");
			zTextBoxColumnStyleInfo2.ColumnName = "ReportTitle";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c4aab67f-a8f7-4ac9-88c2-5bfec79259aa", "Tax Reg.", "Tax Registration Type");
			zDropEditColumnStyleInfo1.ColumnName = "TaxRegistrationType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1a310ee7-d83c-4c8d-bb34-1d229c63bcf8", "Rep Reg.", "Report Country Registration Code");
			zDropEditColumnStyleInfo2.ColumnName = "RepCountryRegistrationCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f77f1a1d-b818-46c9-b05a-dfee00207ac5", "Periodicity");
			zDropEditColumnStyleInfo3.ColumnName = "ReportPeriodicity";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.Caption = "";
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("42f8e123-0597-4671-a700-0dca4944609f", "Table Prefix");
			zDropEditColumnStyleInfo4.ColumnName = "ReportBaseTablePrefix";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("842d327e-f940-4f5b-8611-4573c42c73f6", "Group By", "Report Line Grouping");
			zDropEditColumnStyleInfo5.ColumnName = "ReportLineGrouping";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2e81f5f3-09fd-48f8-b9fd-3832b0062c42", "Order By", "Report Line Ordering");
			zDropEditColumnStyleInfo6.ColumnName = "ReportLineOrdering";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zDropEditColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b06af180-fbd1-4a80-bb51-a6cdbc493955", "Goods/Service");
			zDropEditColumnStyleInfo7.ColumnName = "GoodsServiceType";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zDropEditColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e73fdfe5-f0d4-47a5-98cd-2a6ff97aefe8", "Round/Truncate");
			zDropEditColumnStyleInfo8.ColumnName = "ReportAmountsRoundingType";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("716eaf96-b54d-4447-ab0e-2712df7f9ecc", "Rounding");
			zCalcEditColumnStyleInfo1.ColumnName = "ReportAmountsRoundingTruncating";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zDropEditColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("30422e03-f92b-4179-937b-7f9ca0a2bcb3", "Threshold Level");
			zDropEditColumnStyleInfo9.ColumnName = "AmountThresholdLevel";
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("417f99e2-0238-4eab-93cc-dba66d273d84", "Ex Tax Threshold");
			zCalcEditColumnStyleInfo2.ColumnName = "ExTaxAmountThreshold";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("104302d9-51d0-467f-8883-9ef46ef37e0b", "Tax Threshold");
			zCalcEditColumnStyleInfo3.ColumnName = "TaxAmountThreshold";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2fc596a6-8445-49e9-b430-6644e9dd7a1e", "Recipient", "Recipient Org.", "Recipient Organization for Sub Code mapping");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "RecipientOrgPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.Caption = null;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8872117d-1774-46fb-a22f-2f05c9813296", "Include previous queued records");
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludeQueuedForPreviousPeriod";
			zCheckBoxColumnStyleInfo2.Caption = null;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7042a57b-48f2-4570-aa9d-7a51ec2e8645", "Is default report");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsDefaultReportType";
			this.ParentGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ParentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ParentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ParentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ParentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ParentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ParentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ParentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ParentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ParentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.ParentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.ParentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ParentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.ParentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ParentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ParentGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ParentGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ParentGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ParentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ParentGrid.GridId = "89db08b3-5143-4cfe-9e00-e0d758b0ed74";
			this.ParentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ParentGrid.LayoutKey = "zGrid1";
			this.ParentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ParentGrid.Name = "ParentGrid";
			this.ParentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 111, true);
			this.ParentGrid.TabIndex = 0;
			// 
			// GridSplitter
			// 
			this.GridSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.GridSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 128, true);
			this.GridSplitter.MinSize = 120;
			this.GridSplitter.Name = "GridSplitter";
			this.GridSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 2, true);
			this.GridSplitter.TabIndex = 4;
			this.GridSplitter.TabStop = false;
			// 
			// ChildGroupBox
			// 
			this.ChildGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("987bce88-b3c6-4475-a85b-a2a920a28d71", "Settings");
			this.ChildGroupBox.Controls.Add(this.ChildGrid);
			this.ChildGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 130, true);
			this.ChildGroupBox.Name = "ChildGroupBox";
			this.ChildGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 244, true);
			this.ChildGroupBox.TabIndex = 5;
			this.ChildGroupBox.TabStop = false;
			// 
			// ChildGrid
			// 
			this.ChildGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChildGrid, "Settings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).Settings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfigurationSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).Settings)).SyncRoot)).ComplianceSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfigurationSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).Settings)).SyncRoot)).LedgerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfigurationSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).Settings)).SyncRoot)).InvoiceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfigurationSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).Settings)).SyncRoot)).OriginalRule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfigurationSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).Settings)).SyncRoot)).DisbursementRule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfigurationSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).Settings)).SyncRoot)).TaxInvoiceRule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfigurationSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).Settings)).SyncRoot)).TaxRegistrationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfigurationSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).Settings)).SyncRoot)).OrganisationLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfigurationSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).Settings)).SyncRoot)).ReportingDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceReportConfigurationSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceReportConfiguration)(null)).Settings)).SyncRoot)).OrganisationCategory)));
			this.ChildGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b20010d6-1077-4b3a-be26-f8f686e5b89b", "Sub Type", "Compliance Sub Type");
			zDropEditColumnStyleInfo10.ColumnName = "ComplianceSubType";
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo11.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("23ca7100-4c38-4d6a-bd08-b299c5ce0eb8", "Ledger");
			zDropEditColumnStyleInfo11.ColumnName = "LedgerType";
			zDropEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo12.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("288f1cf4-9025-455e-bf53-8daa5745f619", "Type", "Invoice Type");
			zDropEditColumnStyleInfo12.ColumnName = "InvoiceType";
			zDropEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo13.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6d3cac44-a034-4385-b194-ba7141a0ddce", "Original", "Original Rule");
			zDropEditColumnStyleInfo13.ColumnName = "OriginalRule";
			zDropEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo14.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f37d2e4e-2d4c-42cf-a1a5-2a99920cf267", "Disbursement", "Disbursement Rule");
			zDropEditColumnStyleInfo14.ColumnName = "DisbursementRule";
			zDropEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo15.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7477364e-d01b-4aa6-a770-361a6caabdc1", "Tax Invoice", "Tax Invoice Rule");
			zDropEditColumnStyleInfo15.ColumnName = "TaxInvoiceRule";
			zDropEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo16.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9e93141e-9560-4041-b315-3f4dd48d3209", "Tax Registration", "Tax Registration Type");
			zDropEditColumnStyleInfo16.ColumnName = "TaxRegistrationType";
			zDropEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo17.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4b74c379-fbd4-49de-a7d4-932d72c67cbe", "Org. Location", "Organization Location");
			zDropEditColumnStyleInfo17.ColumnName = "OrganisationLocation";
			zDropEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo18.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("fd2fea35-43eb-4767-a2d2-1eaf4a206d6c", "Rep. Date", "Reporting Date");
			zDropEditColumnStyleInfo18.ColumnName = "ReportingDate";
			zDropEditColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo19.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("754a75e5-d380-496c-aec8-b3da04c47de5", "Org. Category", "Organization Category");
			zDropEditColumnStyleInfo19.ColumnName = "OrganisationCategory";
			zDropEditColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ChildGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.ChildGrid.ColumnStyles.Add(zDropEditColumnStyleInfo11);
			this.ChildGrid.ColumnStyles.Add(zDropEditColumnStyleInfo12);
			this.ChildGrid.ColumnStyles.Add(zDropEditColumnStyleInfo13);
			this.ChildGrid.ColumnStyles.Add(zDropEditColumnStyleInfo14);
			this.ChildGrid.ColumnStyles.Add(zDropEditColumnStyleInfo15);
			this.ChildGrid.ColumnStyles.Add(zDropEditColumnStyleInfo16);
			this.ChildGrid.ColumnStyles.Add(zDropEditColumnStyleInfo17);
			this.ChildGrid.ColumnStyles.Add(zDropEditColumnStyleInfo18);
			this.ChildGrid.ColumnStyles.Add(zDropEditColumnStyleInfo19);
			this.ChildGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildGrid.GridId = "1e923bb9-0742-426e-a71c-b6b1f5cb13d0";
			this.ChildGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChildGrid.LayoutKey = "zGrid1";
			this.ChildGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ChildGrid.Name = "ChildGrid";
			this.ChildGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 227, true);
			this.ChildGrid.TabIndex = 1;
			// 
			// ComplianceReportConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChildGroupBox);
			this.Controls.Add(this.GridSplitter);
			this.Controls.Add(this.ParentGroupBox);
			this.Name = "ComplianceReportConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 374, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ParentGroupBox.ResumeLayout(false);
			this.ParentGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).EndInit();
			this.ParentGrid.ResumeLayout(false);
			this.ParentGrid.PerformLayout();
			this.ChildGroupBox.ResumeLayout(false);
			this.ChildGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChildGrid)).EndInit();
			this.ChildGrid.ResumeLayout(false);
			this.ChildGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZGroupBox ParentGroupBox;
		internal ZArchitecture.ZGrid ParentGrid;
		CargoWise.Windows.UI.KSplitter GridSplitter;
		ZArchitecture.GUI.ZGroupBox ChildGroupBox;
		ZArchitecture.ZGrid ChildGrid;

	}
}
