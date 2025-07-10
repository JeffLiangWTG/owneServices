namespace Enterprise.MarketingManager.GUI
{
	partial class SalesMatchingGrid
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
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.MatchedSalesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MatchedSalesGrid)).BeginInit();
			this.MatchedSalesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.SalesMatchingDataCollection);
			// 
			// MatchedSalesGrid
			// 
			this.MatchedSalesGrid.AllowNavigation = false;
			this.MatchedSalesGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.MatchedSalesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesMatchingData)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.SalesMatchingData)(null)).Rank)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.SalesMatchingData)(null)).BuyerCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.SalesMatchingData)(null)).SupplierCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.SalesMatchingData)(null)).EstimatedValueCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.SalesMatchingData)(null)).EstimatedValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.SalesMatchingData)(null)).TradeDetail.PA_StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MarketingManager.Business.SalesMatchingData)(null)).TradeDetail.ProspectPeriodStart)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MarketingManager.Business.SalesMatchingData)(null)).TradeDetail.ProspectPeriodEnd)));
			this.MatchedSalesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("f6552fd3-26ec-4ea5-9a32-fe7fd940605c", "Rank");
			zCalcEditColumnStyleInfo1.ColumnName = "Rank";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("dd4bdcf0-a684-4e28-b13d-5e80b49878ea", "Buyer");
			zTextBoxColumnStyleInfo1.ColumnName = "BuyerCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ab391705-48bb-4e5a-ac48-fa8de65e3535", "Supplier");
			zTextBoxColumnStyleInfo2.ColumnName = "SupplierCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("5440604e-3012-420e-bc77-b14e656ab399", "Currency");
			zTextBoxColumnStyleInfo3.ColumnName = "EstimatedValueCurrency";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("6d0b095b-04aa-4d08-8fce-c252f4f995d1", "Estimate Value");
			zCalcEditColumnStyleInfo2.ColumnName = "EstimatedValue";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.ColumnName = "TradeDetail+PA_StatusDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "TradeDetail+ProspectPeriodStart";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "TradeDetail+ProspectPeriodEnd";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.MatchedSalesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.MatchedSalesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MatchedSalesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MatchedSalesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MatchedSalesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.MatchedSalesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MatchedSalesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MatchedSalesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MatchedSalesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MatchedSalesGrid.GridId = "01395494-7eac-42ea-a6e8-e3a7f95773ea";
			this.MatchedSalesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MatchedSalesGrid.IsWholeRowSelectedOnClick = true;
			this.MatchedSalesGrid.LayoutKey = "MatchedSalesGrid";
			this.MatchedSalesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MatchedSalesGrid.Name = "MatchedSalesGrid";
			this.MatchedSalesGrid.ReadOnly = true;
			this.MatchedSalesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 281, true);
			this.MatchedSalesGrid.TabIndex = 1;
			// 
			// SalesMatchingGrid
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MatchedSalesGrid);
			this.Name = "SalesMatchingGrid";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 281, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MatchedSalesGrid)).EndInit();
			this.MatchedSalesGrid.ResumeLayout(false);
			this.MatchedSalesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid MatchedSalesGrid;
	}
}
