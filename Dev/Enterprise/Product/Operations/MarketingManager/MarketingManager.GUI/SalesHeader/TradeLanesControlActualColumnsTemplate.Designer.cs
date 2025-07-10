namespace Enterprise.MarketingManager.GUI
{
	partial class TradeLanesControlActualColumnsTemplate
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.PreColumnsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PostColumnsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PreColumnsGrid)).BeginInit();
			this.PreColumnsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PostColumnsGrid)).BeginInit();
			this.PostColumnsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.SalesHeader);
			// 
			// PreColumnsGrid
			// 
			this.PreColumnsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PreColumnsGrid, "FilterableEntitySalesCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).ActualsInformation.Status)));
			this.PreColumnsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ActualsInformation+Status";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.PreColumnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PreColumnsGrid.Dock = System.Windows.Forms.DockStyle.Left;
			this.PreColumnsGrid.GridId = "fb1f0230-dc9f-4b7b-a4ed-3e3493d10cbe";
			this.PreColumnsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PreColumnsGrid.LayoutKey = "tradeLanesGrid";
			this.PreColumnsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreColumnsGrid.Name = "PreColumnsGrid";
			this.PreColumnsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 250, true);
			this.PreColumnsGrid.TabIndex = 1;
			// 
			// PostColumnsGrid
			// 
			this.PostColumnsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PostColumnsGrid, "FilterableEntitySalesCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).OW_LatestProspectDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).ActualsInformation.CommittedYearToDateValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).ActualsInformation.CommittedYearToDateTEUQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).ActualsInformation.CommittedCurrentYearValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).ActualsInformation.CommittedCurrentYearTEUQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).ActualsInformation.CommittedAndForecastCurrentYearValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).ActualsInformation.CommittedAndForecastCurrentYearTEUQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).ActualsInformation.CommittedNextYearValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).ActualsInformation.CommittedNextYearTEUQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).ActualsInformation.CommittedAndForecastNextYearValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).ActualsInformation.CommittedAndForecastNextYearTEUQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).ActualsInformation.ActualsLastTraded)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).TotalTradedValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).TotalTradedTEUQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).ActualsInformation.TradedPeriodString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)).DateForExchangeRate)));
			this.PostColumnsGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.ColumnName = "OW_LatestProspectDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ActualsInformation+CommittedYearToDateValue";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "ActualsInformation+CommittedYearToDateTEUQuantity";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "ActualsInformation+CommittedCurrentYearValue";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "ActualsInformation+CommittedCurrentYearTEUQuantity";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "ActualsInformation+CommittedAndForecastCurrentYearValue";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "ActualsInformation+CommittedAndForecastCurrentYearTEUQuantity";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "ActualsInformation+CommittedNextYearValue";
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "ActualsInformation+CommittedNextYearTEUQuantity";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "ActualsInformation+CommittedAndForecastNextYearValue";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.ColumnName = "ActualsInformation+CommittedAndForecastNextYearTEUQuantity";
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "ActualsInformation+ActualsLastTraded";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.ColumnName = "TotalTradedValue";
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.ColumnName = "TotalTradedTEUQuantity";
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "ActualsInformation+TradedPeriodString";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo3.ColumnName = "DateForExchangeRate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PostColumnsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.PostColumnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PostColumnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PostColumnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PostColumnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.PostColumnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.PostColumnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.PostColumnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.PostColumnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.PostColumnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.PostColumnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.PostColumnsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.PostColumnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.PostColumnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.PostColumnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PostColumnsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.PostColumnsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PostColumnsGrid.GridId = "fb1f0230-dc9f-4b7b-a4ed-3e3493d10cbe";
			this.PostColumnsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PostColumnsGrid.LayoutKey = "tradeLanesGrid";
			this.PostColumnsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 0, true);
			this.PostColumnsGrid.Name = "PostColumnsGrid";
			this.PostColumnsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 250, true);
			this.PostColumnsGrid.TabIndex = 2;
			// 
			// TradeLanesControlActualColumnsTemplate
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PostColumnsGrid);
			this.Controls.Add(this.PreColumnsGrid);
			this.Name = "TradeLanesControlActualColumnsTemplate";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 250, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PreColumnsGrid)).EndInit();
			this.PreColumnsGrid.ResumeLayout(false);
			this.PreColumnsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PostColumnsGrid)).EndInit();
			this.PostColumnsGrid.ResumeLayout(false);
			this.PostColumnsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZGrid PreColumnsGrid;
		public ZArchitecture.ZGrid PostColumnsGrid;

	}
}
