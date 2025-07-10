namespace Enterprise.MarketingManager.GUI
{
	partial class WarehouseTradeDetailsGridControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.MarketingManager.GUI.CompanySpecificCurrencyFindBoxColumnStyleInfo companySpecificCurrencyFindBoxColumnStyleInfo1 = new Enterprise.MarketingManager.GUI.CompanySpecificCurrencyFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.tradeDetailsGrid = new Enterprise.MarketingManager.GUI.AutoMatchingTradeLanesGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tradeDetailsGrid)).BeginInit();
			this.tradeDetailsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.EntitySalesWrapper);
			// 
			// tradeDetailsGrid
			// 
			this.tradeDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.tradeDetailsGrid, "EntityTradeDetailsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).PA_OP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.PAP_RecurrenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).PA_StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.PAP_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).CurrentProspectPeriod.PAS_Units)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).CurrentProspectPeriod.PAS_PalletCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).CurrentProspectPeriod.PAS_LineCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).CurrentProspectPeriod.PAS_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).CurrentProspectPeriod.PAS_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).CurrentProspectPeriod.PAS_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).CurrentProspectPeriod.PAS_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).CurrentProspectPeriod.PAS_RateOffered)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).EstimatedProfit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.GlbCompany)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).Parent.CompanyForExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).CurrencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).JobCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.PAP_ExpectedTradeStartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).EntityCurrencyExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectPeriodEndType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectPeriodStart)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectPeriodEnd)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.PAP_RH_NKCommodityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.PAP_RequiresTemperatureControl)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.PAP_IsDangerous)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.PAP_RequiresPacking)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.PAP_RC_NKContainer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.PAP_RequiresCrossDock)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).CurrentProspectPeriod.PAS_CurrentRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.ConversionCertaintyLikertItemDescription)));
			this.tradeDetailsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "PA_OP";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "ProspectDetail+PAP_RecurrenceType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.ColumnName = "PA_StatusDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "ProspectDetail+PAP_F3_NKPackType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("743e66ce-4100-49f3-96d7-8e25c326a3bb", "Units", "Unit Count", "");
			zCalcEditColumnStyleInfo1.ColumnName = "CurrentProspectPeriod+PAS_Units";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CurrentProspectPeriod+PAS_PalletCount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CurrentProspectPeriod+PAS_LineCount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "CurrentProspectPeriod+PAS_Weight";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo3.ColumnName = "CurrentProspectPeriod+PAS_WeightUQ";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "CurrentProspectPeriod+PAS_Volume";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo4.ColumnName = "CurrentProspectPeriod+PAS_VolumeUQ";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "CurrentProspectPeriod+PAS_RateOffered";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "EstimatedProfit";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			companySpecificCurrencyFindBoxColumnStyleInfo1.ColumnName = "CurrencyCode";
			companySpecificCurrencyFindBoxColumnStyleInfo1.CompanyColumnName = "Parent+CompanyForExchangeRate";
			companySpecificCurrencyFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "JobCount";
			zCalcEditColumnStyleInfo8.Decimals = 0;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.ColumnName = "ProspectDetail+PAP_ExpectedTradeStartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "EntityCurrencyExchangeRate";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo5.ColumnName = "ProspectPeriodEndType";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "ProspectPeriodStart";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "ProspectPeriodEnd";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ProspectDetail+PAP_RH_NKCommodityCode";
			zCodeFindBoxColumnStyleInfo1.IsVisible = false;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo1.ColumnName = "ProspectDetail+PAP_RequiresTemperatureControl";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "ProspectDetail+PAP_IsDangerous";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.ColumnName = "ProspectDetail+PAP_RequiresPacking";
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "ProspectDetail+PAP_RC_NKContainer";
			zCodeFindBoxColumnStyleInfo2.IsVisible = false;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zCheckBoxColumnStyleInfo4.ColumnName = "ProspectDetail+PAP_RequiresCrossDock";
			zCheckBoxColumnStyleInfo4.IsVisible = false;
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.ColumnName = "CurrentProspectPeriod+PAS_CurrentRate";
			zCalcEditColumnStyleInfo10.IsVisible = false;
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo6.ColumnName = "ProspectDetail+ConversionCertaintyLikertItemDescription";
			zDropEditColumnStyleInfo6.IsVisible = false;
			zDropEditColumnStyleInfo6.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.tradeDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.tradeDetailsGrid.ColumnStyles.Add(companySpecificCurrencyFindBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.tradeDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.tradeDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.tradeDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tradeDetailsGrid.GridId = "0017a023-79d5-4f6b-9f4c-124783715942";
			this.tradeDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.tradeDetailsGrid.LayoutKey = "tradeDetailsGrid";
			this.tradeDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.tradeDetailsGrid.Name = "tradeDetailsGrid";
			this.tradeDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 196, true);
			this.tradeDetailsGrid.TabIndex = 1;
			// 
			// WarehouseTradeDetailsGridControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.tradeDetailsGrid);
			this.Name = "WarehouseTradeDetailsGridControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 200, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tradeDetailsGrid)).EndInit();
			this.tradeDetailsGrid.ResumeLayout(false);
			this.tradeDetailsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private AutoMatchingTradeLanesGrid tradeDetailsGrid;
	}
}
