namespace Enterprise.MarketingManager.GUI
{
	partial class GenericTradeLanesControl
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
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.MarketingManager.GUI.CompanySpecificCurrencyFindBoxColumnStyleInfo companySpecificCurrencyFindBoxColumnStyleInfo1 = new Enterprise.MarketingManager.GUI.CompanySpecificCurrencyFindBoxColumnStyleInfo();
			this.bottomSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.tradeLanesGrid = new Enterprise.MarketingManager.GUI.AutoMatchingTradeLanesGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.bottomSplitContainer)).BeginInit();
			this.bottomSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tradeLanesGrid)).BeginInit();
			this.tradeLanesGrid.SuspendLayout();
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
			// tradeLanesGrid
			// 
			this.tradeLanesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.tradeLanesGrid, "FilterableEntitySalesCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).OW_OriginID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).OW_OH_Buyer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).OW_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).TotalEstimatedAnnualValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).CommittedAnnualValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).CommittedMonthlyValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).PipelineValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).UnsuccessfulValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.GlbCompany)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).CompanyForExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).TotalRevenueCurrencyCode)));
			this.tradeLanesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b7ae14dc-0e33-4ac6-83ab-f1b328efdc60", "Location");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OW_OriginID";
			zGuidFindBoxColumnStyleInfo1.IsVisible = false;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "OW_OH_Buyer";
			zOrganisationFindBoxColumnStyleInfo1.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "OW_OH_Supplier";
			zOrganisationFindBoxColumnStyleInfo2.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TotalEstimatedAnnualValue";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CommittedAnnualValue";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CommittedMonthlyValue";
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "PipelineValue";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "UnsuccessfulValue";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			companySpecificCurrencyFindBoxColumnStyleInfo1.ColumnName = "TotalRevenueCurrencyCode";
			companySpecificCurrencyFindBoxColumnStyleInfo1.CompanyColumnName = "CompanyForExchangeRate";
			companySpecificCurrencyFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			companySpecificCurrencyFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.tradeLanesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.tradeLanesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.tradeLanesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.tradeLanesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.tradeLanesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.tradeLanesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.tradeLanesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.tradeLanesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.tradeLanesGrid.ColumnStyles.Add(companySpecificCurrencyFindBoxColumnStyleInfo1);
			this.tradeLanesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tradeLanesGrid.GridId = "fb1f0230-dc9f-4b7b-a4ed-3e3493d10cbe";
			this.tradeLanesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.tradeLanesGrid.LayoutKey = "tradeProfileGrid";
			this.tradeLanesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tradeLanesGrid.Name = "tradeLanesGrid";
			this.tradeLanesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 200, true);
			this.tradeLanesGrid.TabIndex = 1;
			// 
			// GenericTradeLanesControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.tradeLanesGrid);
			this.Name = "GenericTradeLanesControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.bottomSplitContainer)).EndInit();
			this.bottomSplitContainer.ResumeLayout(false);
			this.bottomSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.tradeLanesGrid)).EndInit();
			this.tradeLanesGrid.ResumeLayout(false);
			this.tradeLanesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer bottomSplitContainer;
		protected AutoMatchingTradeLanesGrid tradeLanesGrid;
	}
}
