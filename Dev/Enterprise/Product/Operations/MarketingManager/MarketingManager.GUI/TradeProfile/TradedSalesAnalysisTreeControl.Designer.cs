namespace Enterprise.MarketingManager.GUI
{
	partial class TradedSalesAnalysisTreeControl
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
			this.descriptionColumn = new Aga.Controls.Tree.TreeColumn();
			this.grossRevenueColumn = new Aga.Controls.Tree.TreeColumn();
			this.grossWeightColumn = new Aga.Controls.Tree.TreeColumn();
			this.netVolumeColumn = new Aga.Controls.Tree.TreeColumn();
			this.descriptionNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			this.countSuffixNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			this.grossRevenueNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			this.grossWeightNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			this.grossWeightUnitNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			this.grossWeightUnitsColumn = new Aga.Controls.Tree.TreeColumn();
			this.netVolumeNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			this.netVolumeUnitNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			this.netVolumeUnitsColumn = new Aga.Controls.Tree.TreeColumn();
			this.tree = new Enterprise.ZArchitecture.GUI.ZTreeViewAdv();
			this.jobRevenueColumn = new Aga.Controls.Tree.TreeColumn();
			this.jobCostColumn = new Aga.Controls.Tree.TreeColumn();
			this.jobProfitColumn = new Aga.Controls.Tree.TreeColumn();
			this.revenueCurrencyColumn = new Aga.Controls.Tree.TreeColumn();
			this.unitCountColumn = new Aga.Controls.Tree.TreeColumn();
			this.palletCountColumn = new Aga.Controls.Tree.TreeColumn();
			this.lineCountColumn = new Aga.Controls.Tree.TreeColumn();
			this.supplierColumn = new Aga.Controls.Tree.TreeColumn();
			this.buyerColumn = new Aga.Controls.Tree.TreeColumn();
			this.lastJobRegistrationColumn = new Aga.Controls.Tree.TreeColumn();
			this.teuQuantityColumn = new Aga.Controls.Tree.TreeColumn();
			this.jobRevenueNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			this.jobCostNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			this.jobProfitNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			this.revenueCurrencyNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			this.unitCountNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			this.palletCountNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			this.lineCountNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			this.supplierCodeNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			this.buyerCodeNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			this.lastJobRegistrationNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			this.teuQuantityNodeTextBox = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.tree.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.TradePeriodGrouping);
			// 
			// descriptionColumn
			// 
			this.descriptionColumn.Header = "";
			this.descriptionColumn.Sortable = true;
			this.descriptionColumn.SortOrder = System.Windows.Forms.SortOrder.Ascending;
			this.descriptionColumn.TooltipText = null;
			this.descriptionColumn.Width = 350;
			// 
			// grossRevenueColumn
			// 
			this.grossRevenueColumn.Header = "Revenue";
			this.grossRevenueColumn.Sortable = true;
			this.grossRevenueColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.grossRevenueColumn.TooltipText = null;
			this.grossRevenueColumn.Width = 100;
			// 
			// grossWeightColumn
			// 
			this.grossWeightColumn.Header = "G. Weight";
			this.grossWeightColumn.Sortable = true;
			this.grossWeightColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.grossWeightColumn.TooltipText = null;
			this.grossWeightColumn.Width = 80;
			// 
			// netVolumeColumn
			// 
			this.netVolumeColumn.Header = "Net. Vol";
			this.netVolumeColumn.Sortable = true;
			this.netVolumeColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.netVolumeColumn.TooltipText = null;
			this.netVolumeColumn.Width = 80;
			// 
			// descriptionNodeTextBox
			// 
			this.descriptionNodeTextBox.DataPropertyName = "Description";
			this.descriptionNodeTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.descriptionNodeTextBox.IncrementalSearchEnabled = true;
			this.descriptionNodeTextBox.LeftMargin = 3;
			this.descriptionNodeTextBox.ParentColumn = this.descriptionColumn;
			// 
			// countSuffixNodeTextBox
			// 
			this.countSuffixNodeTextBox.DataPropertyName = "Count";
			this.countSuffixNodeTextBox.IncrementalSearchEnabled = true;
			this.countSuffixNodeTextBox.LeftMargin = 3;
			this.countSuffixNodeTextBox.ParentColumn = this.descriptionColumn;
			// 
			// grossRevenueNodeTextBox
			// 
			this.grossRevenueNodeTextBox.DataPropertyName = "GrossRevenue";
			this.grossRevenueNodeTextBox.IncrementalSearchEnabled = true;
			this.grossRevenueNodeTextBox.LeftMargin = 3;
			this.grossRevenueNodeTextBox.ParentColumn = this.grossRevenueColumn;
			this.grossRevenueNodeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// grossWeightNodeTextBox
			// 
			this.grossWeightNodeTextBox.DataPropertyName = "GrossWeight";
			this.grossWeightNodeTextBox.IncrementalSearchEnabled = true;
			this.grossWeightNodeTextBox.LeftMargin = 3;
			this.grossWeightNodeTextBox.ParentColumn = this.grossWeightColumn;
			this.grossWeightNodeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// grossWeightUnitNodeTextBox
			// 
			this.grossWeightUnitNodeTextBox.DataPropertyName = "WeightUnit";
			this.grossWeightUnitNodeTextBox.IncrementalSearchEnabled = true;
			this.grossWeightUnitNodeTextBox.LeftMargin = 3;
			this.grossWeightUnitNodeTextBox.ParentColumn = this.grossWeightUnitsColumn;
			// 
			// grossWeightUnitsColumn
			// 
			this.grossWeightUnitsColumn.Header = "Units";
			this.grossWeightUnitsColumn.Sortable = true;
			this.grossWeightUnitsColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.grossWeightUnitsColumn.TooltipText = null;
			this.grossWeightUnitsColumn.Width = 40;
			// 
			// netVolumeNodeTextBox
			// 
			this.netVolumeNodeTextBox.DataPropertyName = "NetVolume";
			this.netVolumeNodeTextBox.IncrementalSearchEnabled = true;
			this.netVolumeNodeTextBox.LeftMargin = 3;
			this.netVolumeNodeTextBox.ParentColumn = this.netVolumeColumn;
			this.netVolumeNodeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// netVolumeUnitNodeTextBox
			// 
			this.netVolumeUnitNodeTextBox.DataPropertyName = "VolumeUnit";
			this.netVolumeUnitNodeTextBox.IncrementalSearchEnabled = true;
			this.netVolumeUnitNodeTextBox.LeftMargin = 3;
			this.netVolumeUnitNodeTextBox.ParentColumn = this.netVolumeUnitsColumn;
			// 
			// netVolumeUnitsColumn
			// 
			this.netVolumeUnitsColumn.Header = "Units";
			this.netVolumeUnitsColumn.Sortable = true;
			this.netVolumeUnitsColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.netVolumeUnitsColumn.TooltipText = null;
			this.netVolumeUnitsColumn.Width = 40;
			// 
			// tree
			// 
			this.tree.AutoRowHeight = true;
			this.tree.BackColor = System.Drawing.SystemColors.Window;
			this.tree.Columns.Add(this.descriptionColumn);
			this.tree.Columns.Add(this.grossRevenueColumn);
			this.tree.Columns.Add(this.jobRevenueColumn);
			this.tree.Columns.Add(this.jobCostColumn);
			this.tree.Columns.Add(this.jobProfitColumn);
			this.tree.Columns.Add(this.revenueCurrencyColumn);
			this.tree.Columns.Add(this.unitCountColumn);
			this.tree.Columns.Add(this.palletCountColumn);
			this.tree.Columns.Add(this.lineCountColumn);
			this.tree.Columns.Add(this.grossWeightColumn);
			this.tree.Columns.Add(this.grossWeightUnitsColumn);
			this.tree.Columns.Add(this.netVolumeColumn);
			this.tree.Columns.Add(this.netVolumeUnitsColumn);
			this.tree.Columns.Add(this.teuQuantityColumn);
			this.tree.Columns.Add(this.supplierColumn);
			this.tree.Columns.Add(this.buyerColumn);
			this.tree.Columns.Add(this.lastJobRegistrationColumn);
			this.tree.DefaultToolTipProvider = null;
			this.tree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tree.DragDropMarkColor = System.Drawing.Color.Black;
			this.tree.ElementType = null;
			this.tree.LineColor = System.Drawing.SystemColors.ControlDark;
			this.tree.LineDashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
			this.tree.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tree.Name = "tree";
			this.tree.NodeControls.Add(this.descriptionNodeTextBox);
			this.tree.NodeControls.Add(this.countSuffixNodeTextBox);
			this.tree.NodeControls.Add(this.grossRevenueNodeTextBox);
			this.tree.NodeControls.Add(this.jobRevenueNodeTextBox);
			this.tree.NodeControls.Add(this.jobCostNodeTextBox);
			this.tree.NodeControls.Add(this.jobProfitNodeTextBox);
			this.tree.NodeControls.Add(this.revenueCurrencyNodeTextBox);
			this.tree.NodeControls.Add(this.unitCountNodeTextBox);
			this.tree.NodeControls.Add(this.palletCountNodeTextBox);
			this.tree.NodeControls.Add(this.lineCountNodeTextBox);
			this.tree.NodeControls.Add(this.grossWeightNodeTextBox);
			this.tree.NodeControls.Add(this.grossWeightUnitNodeTextBox);
			this.tree.NodeControls.Add(this.netVolumeNodeTextBox);
			this.tree.NodeControls.Add(this.netVolumeUnitNodeTextBox);
			this.tree.NodeControls.Add(this.supplierCodeNodeTextBox);
			this.tree.NodeControls.Add(this.buyerCodeNodeTextBox);
			this.tree.NodeControls.Add(this.lastJobRegistrationNodeTextBox);
			this.tree.NodeControls.Add(this.teuQuantityNodeTextBox);
			this.tree.SelectedNode = null;
			this.tree.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 250, true);
			this.tree.TabIndex = 0;
			this.tree.UseColumns = true;
			// 
			// jobRevenueColumn
			// 
			this.jobRevenueColumn.Header = "Job Revenue";
			this.jobRevenueColumn.Sortable = true;
			this.jobRevenueColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.jobRevenueColumn.TooltipText = null;
			this.jobRevenueColumn.Width = 100;
			// 
			// jobCostColumn
			// 
			this.jobCostColumn.Header = "Job Cost";
			this.jobCostColumn.Sortable = true;
			this.jobCostColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.jobCostColumn.TooltipText = null;
			this.jobCostColumn.Width = 100;
			// 
			// jobProfitColumn
			// 
			this.jobProfitColumn.Header = "Job Profit";
			this.jobProfitColumn.Sortable = true;
			this.jobProfitColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.jobProfitColumn.TooltipText = null;
			this.jobProfitColumn.Width = 100;
			// 
			// revenueCurrencyColumn
			// 
			this.revenueCurrencyColumn.Header = "Curr.";
			this.revenueCurrencyColumn.Sortable = true;
			this.revenueCurrencyColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.revenueCurrencyColumn.TooltipText = null;
			this.revenueCurrencyColumn.Width = 40;
			// 
			// unitCountColumn
			// 
			this.unitCountColumn.Header = "Unit Count";
			this.unitCountColumn.Sortable = true;
			this.unitCountColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.unitCountColumn.TooltipText = null;
			this.unitCountColumn.Width = 80;
			// 
			// palletCountColumn
			// 
			this.palletCountColumn.Header = "Pallets";
			this.palletCountColumn.Sortable = true;
			this.palletCountColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.palletCountColumn.TooltipText = null;
			this.palletCountColumn.Width = 80;
			// 
			// lineCountColumn
			// 
			this.lineCountColumn.Header = "Line Count";
			this.lineCountColumn.Sortable = true;
			this.lineCountColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.lineCountColumn.TooltipText = null;
			this.lineCountColumn.Width = 80;
			// 
			// supplierColumn
			// 
			this.supplierColumn.Header = "Supplier";
			this.supplierColumn.Sortable = true;
			this.supplierColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.supplierColumn.TooltipText = null;
			this.supplierColumn.Width = 80;
			// 
			// buyerColumn
			// 
			this.buyerColumn.Header = "Buyer";
			this.buyerColumn.Sortable = true;
			this.buyerColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.buyerColumn.TooltipText = string.Empty;
			this.buyerColumn.Width = 80;
			// 
			// lastJobRegistrationColumn
			// 
			this.lastJobRegistrationColumn.Header = "Last Job Registration";
			this.lastJobRegistrationColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.lastJobRegistrationColumn.TooltipText = null;
			this.lastJobRegistrationColumn.Width = 115;
			// 
			// teuQuantityColumn
			// 
			this.teuQuantityColumn.Header = "";
			this.teuQuantityColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.teuQuantityColumn.TooltipText = null;
			// 
			// jobRevenueNodeTextBox
			// 
			this.jobRevenueNodeTextBox.DataPropertyName = "JobRevenue";
			this.jobRevenueNodeTextBox.IncrementalSearchEnabled = true;
			this.jobRevenueNodeTextBox.LeftMargin = 3;
			this.jobRevenueNodeTextBox.ParentColumn = this.jobRevenueColumn;
			this.jobRevenueNodeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// jobCostNodeTextBox
			// 
			this.jobCostNodeTextBox.DataPropertyName = "JobCost";
			this.jobCostNodeTextBox.IncrementalSearchEnabled = true;
			this.jobCostNodeTextBox.LeftMargin = 3;
			this.jobCostNodeTextBox.ParentColumn = this.jobCostColumn;
			this.jobCostNodeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// jobProfitNodeTextBox
			// 
			this.jobProfitNodeTextBox.DataPropertyName = "JobProfit";
			this.jobProfitNodeTextBox.IncrementalSearchEnabled = true;
			this.jobProfitNodeTextBox.LeftMargin = 3;
			this.jobProfitNodeTextBox.ParentColumn = this.jobProfitColumn;
			this.jobProfitNodeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// revenueCurrencyNodeTextBox
			// 
			this.revenueCurrencyNodeTextBox.DataPropertyName = "CurrencyCode";
			this.revenueCurrencyNodeTextBox.IncrementalSearchEnabled = true;
			this.revenueCurrencyNodeTextBox.LeftMargin = 3;
			this.revenueCurrencyNodeTextBox.ParentColumn = this.revenueCurrencyColumn;
			// 
			// unitCountNodeTextBox
			// 
			this.unitCountNodeTextBox.DataPropertyName = "UnitCount";
			this.unitCountNodeTextBox.IncrementalSearchEnabled = true;
			this.unitCountNodeTextBox.LeftMargin = 3;
			this.unitCountNodeTextBox.ParentColumn = this.unitCountColumn;
			// 
			// palletCountNodeTextBox
			// 
			this.palletCountNodeTextBox.DataPropertyName = "PalletCount";
			this.palletCountNodeTextBox.IncrementalSearchEnabled = true;
			this.palletCountNodeTextBox.LeftMargin = 3;
			this.palletCountNodeTextBox.ParentColumn = this.palletCountColumn;
			// 
			// lineCountNodeTextBox
			// 
			this.lineCountNodeTextBox.DataPropertyName = "LineCount";
			this.lineCountNodeTextBox.IncrementalSearchEnabled = true;
			this.lineCountNodeTextBox.LeftMargin = 3;
			this.lineCountNodeTextBox.ParentColumn = this.lineCountColumn;
			// 
			// supplierCodeNodeTextBox
			// 
			this.supplierCodeNodeTextBox.DataPropertyName = "SupplierCode";
			this.supplierCodeNodeTextBox.IncrementalSearchEnabled = true;
			this.supplierCodeNodeTextBox.LeftMargin = 3;
			this.supplierCodeNodeTextBox.ParentColumn = this.supplierColumn;
			// 
			// buyerCodeNodeTextBox
			// 
			this.buyerCodeNodeTextBox.DataPropertyName = "BuyerCode";
			this.buyerCodeNodeTextBox.IncrementalSearchEnabled = true;
			this.buyerCodeNodeTextBox.LeftMargin = 3;
			this.buyerCodeNodeTextBox.ParentColumn = this.buyerColumn;
			// 
			// lastJobRegistrationNodeTextBox
			// 
			this.lastJobRegistrationNodeTextBox.DataPropertyName = "LastJobRegistration";
			this.lastJobRegistrationNodeTextBox.IncrementalSearchEnabled = true;
			this.lastJobRegistrationNodeTextBox.LeftMargin = 3;
			this.lastJobRegistrationNodeTextBox.ParentColumn = this.lastJobRegistrationColumn;
			// 
			// teuQuantityNodeTextBox
			// 
			this.teuQuantityNodeTextBox.DataPropertyName = "TEUQuantity";
			this.teuQuantityNodeTextBox.IncrementalSearchEnabled = true;
			this.teuQuantityNodeTextBox.LeftMargin = 3;
			this.teuQuantityNodeTextBox.ParentColumn = this.teuQuantityColumn;
			// 
			// TradedSalesAnalysisTreeControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.tree);
			this.Name = "TradedSalesAnalysisTreeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 250, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.tree.ResumeLayout(false);
			this.tree.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private SalesAnalysisNodeTextBox descriptionNodeTextBox;
		private Aga.Controls.Tree.TreeColumn descriptionColumn;
		private Aga.Controls.Tree.TreeColumn grossRevenueColumn;
		private SalesAnalysisNodeTextBox countSuffixNodeTextBox;
		private Aga.Controls.Tree.TreeColumn grossWeightColumn;
		private Aga.Controls.Tree.TreeColumn netVolumeColumn;
		private SalesAnalysisNodeTextBox grossRevenueNodeTextBox;
		private SalesAnalysisNodeTextBox grossWeightNodeTextBox;
		private SalesAnalysisNodeTextBox netVolumeNodeTextBox;
		private SalesAnalysisNodeTextBox grossWeightUnitNodeTextBox;
		private SalesAnalysisNodeTextBox netVolumeUnitNodeTextBox;
		private Enterprise.ZArchitecture.GUI.ZTreeViewAdv tree;
		private Aga.Controls.Tree.TreeColumn jobRevenueColumn;
		private Aga.Controls.Tree.TreeColumn jobCostColumn;
		private Aga.Controls.Tree.TreeColumn jobProfitColumn;
		private SalesAnalysisNodeTextBox jobRevenueNodeTextBox;
		private SalesAnalysisNodeTextBox jobCostNodeTextBox;
		private SalesAnalysisNodeTextBox jobProfitNodeTextBox;
		private SalesAnalysisNodeTextBox revenueCurrencyNodeTextBox;
		private Aga.Controls.Tree.TreeColumn revenueCurrencyColumn;
		private Aga.Controls.Tree.TreeColumn grossWeightUnitsColumn;
		private Aga.Controls.Tree.TreeColumn netVolumeUnitsColumn;
		private Aga.Controls.Tree.TreeColumn supplierColumn;
		private Aga.Controls.Tree.TreeColumn buyerColumn;
		private Aga.Controls.Tree.TreeColumn lastJobRegistrationColumn;
		private SalesAnalysisNodeTextBox supplierCodeNodeTextBox;
		private SalesAnalysisNodeTextBox buyerCodeNodeTextBox;
		private SalesAnalysisNodeTextBox lastJobRegistrationNodeTextBox;
		private Aga.Controls.Tree.TreeColumn palletCountColumn;
		private Aga.Controls.Tree.TreeColumn lineCountColumn;
		private SalesAnalysisNodeTextBox palletCountNodeTextBox;
		private SalesAnalysisNodeTextBox lineCountNodeTextBox;
		private Aga.Controls.Tree.TreeColumn unitCountColumn;
		private SalesAnalysisNodeTextBox unitCountNodeTextBox;
		private Aga.Controls.Tree.TreeColumn teuQuantityColumn;
		private SalesAnalysisNodeTextBox teuQuantityNodeTextBox;
	}
}
