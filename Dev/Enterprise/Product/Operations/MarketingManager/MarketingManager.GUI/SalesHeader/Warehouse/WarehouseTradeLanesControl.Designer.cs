namespace Enterprise.MarketingManager.GUI
{
	partial class WarehouseTradeLanesControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.MarketingManager.GUI.CompanySpecificCurrencyFindBoxColumnStyleInfo companySpecificCurrencyFindBoxColumnStyleInfo1 = new Enterprise.MarketingManager.GUI.CompanySpecificCurrencyFindBoxColumnStyleInfo();
			this.tradeLanesGrid = new Enterprise.MarketingManager.GUI.AutoMatchingTradeLanesGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tradeLanesGrid)).BeginInit();
			this.tradeLanesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// tradeLanesGrid
			// 
			this.tradeLanesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.tradeLanesGrid, "FilterableEntitySalesCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).OW_ServiceDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).OW_WW)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).Warehouse.WW_WarehouseName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).OW_OriginID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).OW_OH_Buyer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).OW_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).OW_Calc_TotalAnnualCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).OW_Calc_TotalAnnualPalletCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).OW_Calc_TotalAnnualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).OW_Calc_TotalAnnualVolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).TotalEstimatedAnnualValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).CommittedAnnualValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).CommittedMonthlyValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).PipelineValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).UnsuccessfulValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).EntityExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.GlbCompany)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).CompanyForExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).TotalRevenueCurrencyCode)));
			this.tradeLanesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "OW_ServiceDescription";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OW_WW";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.ColumnName = "Warehouse+WW_WarehouseName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("af78bb1e-892a-471e-9c4b-addc68cef40d", "Location");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "OW_OriginID";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "OW_OH_Buyer";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "OW_OH_Supplier";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "OW_Calc_TotalAnnualCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "OW_Calc_TotalAnnualPalletCount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "OW_Calc_TotalAnnualVolume";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "OW_Calc_TotalAnnualVolumeUQ";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "TotalEstimatedAnnualValue";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "CommittedAnnualValue";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "CommittedMonthlyValue";
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "PipelineValue";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "UnsuccessfulValue";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "EntityExchangeRate";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			companySpecificCurrencyFindBoxColumnStyleInfo1.ColumnName = "TotalRevenueCurrencyCode";
			companySpecificCurrencyFindBoxColumnStyleInfo1.CompanyColumnName = "CompanyForExchangeRate";
			companySpecificCurrencyFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			companySpecificCurrencyFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.tradeLanesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.tradeLanesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.tradeLanesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.tradeLanesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.tradeLanesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.tradeLanesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.tradeLanesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.tradeLanesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.tradeLanesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.tradeLanesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.tradeLanesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.tradeLanesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.tradeLanesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.tradeLanesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.tradeLanesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.tradeLanesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.tradeLanesGrid.ColumnStyles.Add(companySpecificCurrencyFindBoxColumnStyleInfo1);
			this.tradeLanesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tradeLanesGrid.GridId = "fb1f0230-dc9f-4b7b-a4ed-3e3493d10cbe";
			this.tradeLanesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.tradeLanesGrid.LayoutKey = "tradeLanesGrid";
			this.tradeLanesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tradeLanesGrid.Name = "tradeLanesGrid";
			this.tradeLanesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 250, true);
			this.tradeLanesGrid.TabIndex = 1;
			// 
			// WarehouseTradeLanesControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.tradeLanesGrid);
			this.Name = "WarehouseTradeLanesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 250, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tradeLanesGrid)).EndInit();
			this.tradeLanesGrid.ResumeLayout(false);
			this.tradeLanesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected AutoMatchingTradeLanesGrid tradeLanesGrid;
	}
}
