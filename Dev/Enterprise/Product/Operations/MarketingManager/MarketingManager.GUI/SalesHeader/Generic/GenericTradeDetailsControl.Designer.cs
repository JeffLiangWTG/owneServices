namespace Enterprise.MarketingManager.GUI
{
	partial class GenericTradeDetailsControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.MarketingManager.GUI.CompanySpecificCurrencyFindBoxColumnStyleInfo companySpecificCurrencyFindBoxColumnStyleInfo1 = new Enterprise.MarketingManager.GUI.CompanySpecificCurrencyFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo3 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			this.bottomSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.tradeDetailsGrid = new Enterprise.MarketingManager.GUI.AutoMatchingTradeLanesGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.bottomSplitContainer)).BeginInit();
			this.bottomSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tradeDetailsGrid)).BeginInit();
			this.tradeDetailsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// bottomSplitContainer
			// 
			this.bottomSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bottomSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.bottomSplitContainer.Name = "bottomSplitContainer";
			this.bottomSplitContainer.Panel1Collapsed = true;
			this.bottomSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 198, true);
			this.bottomSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(640);
			this.bottomSplitContainer.TabIndex = 0;
			// 
			// tradeDetailsGrid
			// 
			this.tradeDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.tradeDetailsGrid, "EntityTradeDetailsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).PA_TradeMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).TradeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).PA_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).PA_StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.PAP_ExpectedTradeStartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectPeriodEndType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectPeriodStart)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectPeriodEnd)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).JobCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).EstimatedProfit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.GlbCompany)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).Parent.CompanyForExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).CurrencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.PAP_RS_NKServiceLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.PAP_RH_NKCommodityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.PAP_IncoTradeTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).CurrentProspectPeriod.PAS_Units)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.PAP_Density)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).CurrentProspectPeriod.PAS_CurrentRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).CurrentProspectPeriod.PAS_RateOffered)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.ConversionCertaintyLikertItemDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.PAP_OH_ControllingAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.PAP_OH_ServiceProvider)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)).ProspectDetail.PAP_OH_Competitor)));
			this.tradeDetailsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "PA_TradeMode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zDropEditColumnStyleInfo2.ColumnName = "TradeType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zDropEditColumnStyleInfo3.ColumnName = "PA_Status";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo1.ColumnName = "PA_StatusDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "ProspectDetail+PAP_ExpectedTradeStartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "ProspectPeriodEndType";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "ProspectPeriodStart";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "ProspectPeriodEnd";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JobCount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "EstimatedProfit";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			companySpecificCurrencyFindBoxColumnStyleInfo1.ColumnName = "CurrencyCode";
			companySpecificCurrencyFindBoxColumnStyleInfo1.CompanyColumnName = "Parent+CompanyForExchangeRate";
			companySpecificCurrencyFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ProspectDetail+PAP_RS_NKServiceLevel";
			zCodeFindBoxColumnStyleInfo1.PopupCaption = "Select Service Level";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "ProspectDetail+PAP_RH_NKCommodityCode";
			zCodeFindBoxColumnStyleInfo2.PopupCaption = "Select Commodity Code";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zDropEditColumnStyleInfo5.ColumnName = "ProspectDetail+PAP_IncoTradeTerm";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CurrentProspectPeriod+PAS_Units";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo6.ColumnName = "ProspectDetail+PAP_Density";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "CurrentProspectPeriod+PAS_CurrentRate";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "CurrentProspectPeriod+PAS_RateOffered";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo7.ColumnName = "ProspectDetail+ConversionCertaintyLikertItemDescription";
			zDropEditColumnStyleInfo7.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "ProspectDetail+PAP_OH_ControllingAgent";
			zOrganisationFindBoxColumnStyleInfo1.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "ProspectDetail+PAP_OH_ServiceProvider";
			zOrganisationFindBoxColumnStyleInfo2.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo3.ColumnName = "ProspectDetail+PAP_OH_Competitor";
			zOrganisationFindBoxColumnStyleInfo3.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.tradeDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(companySpecificCurrencyFindBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.tradeDetailsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo3);
			this.tradeDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tradeDetailsGrid.GridId = "2ccfa0f4-bcad-40cd-b973-0039f3059841";
			this.tradeDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.tradeDetailsGrid.LayoutKey = "tradeDetailsGrid";
			this.tradeDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tradeDetailsGrid.Name = "tradeDetailsGrid";
			this.tradeDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 100, true);
			this.tradeDetailsGrid.TabIndex = 0;
			// 
			// GenericTradeDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.tradeDetailsGrid);
			this.Name = "GenericTradeDetailsControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.bottomSplitContainer)).EndInit();
			this.bottomSplitContainer.ResumeLayout(false);
			this.bottomSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.tradeDetailsGrid)).EndInit();
			this.tradeDetailsGrid.ResumeLayout(false);
			this.tradeDetailsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer bottomSplitContainer;
		protected AutoMatchingTradeLanesGrid tradeDetailsGrid;
	}
}
