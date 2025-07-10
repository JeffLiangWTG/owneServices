
namespace Enterprise.Rating.GUI
{
	partial class CartageCalculatorPanel
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CartageCalculatorPanel));
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.CartageZonesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.ZonesButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.ZoneRateLineItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CartageZonesGrid)).BeginInit();
			this.ToolStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ZoneRateLineItemsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.CartageZonesGrid);
			this.SplitContainer.Panel1.Controls.Add(this.ToolStrip);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.ZoneRateLineItemsGrid);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 127, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.SplitContainer.TabIndex = 0;
			this.SplitContainer.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitContainer_SplitterMoved);
			// 
			// CartageZonesGrid
			// 
			this.CartageZonesGrid.AllowNavigation = false;
			this.CartageZonesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CartageCalculatorPanel|3adf39f8-77b1-4093-8cba-b0b1d4049b87", "Zone");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			this.CartageZonesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CartageZonesGrid.GridId = "29437ccb-27b2-4b66-a113-77522deeb129";
			this.CartageZonesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CartageZonesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CartageZonesGrid.IsWholeRowSelectedOnClick = true;
			this.CartageZonesGrid.LayoutKey = "CartageZonesGrid";
			this.CartageZonesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 0, true);
			this.CartageZonesGrid.Name = "CartageZonesGrid";
			this.CartageZonesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 127, true);
			this.CartageZonesGrid.TabIndex = 1;
			// 
			// ToolStrip
			// 
			this.ToolStrip.Dock = System.Windows.Forms.DockStyle.Left;
			this.ToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ZonesButton});
			this.ToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToolStrip.Name = "ToolStrip";
			this.ToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.ToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 127, true);
			this.ToolStrip.TabIndex = 0;
			this.ToolStrip.TextDirection = System.Windows.Forms.ToolStripTextDirection.Vertical270;
			// 
			// ZonesButton
			// 
			this.ZonesButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("D5E83607-EE2A-4CE5-8573-AC0B3F44500D", "Zones");
			this.ZonesButton.CheckOnClick = true;
			this.ZonesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.ZonesButton.Image = ((System.Drawing.Image)(resources.GetObject("ZonesButton.Image")));
			this.ZonesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ZonesButton.Name = "ZonesButton";
			this.ZonesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 43, true);
			this.ZonesButton.CheckedChanged += new System.EventHandler(this.ZonesButton_CheckedChanged);
			// 
			// ZoneRateLineItemsGrid
			// 
			this.ZoneRateLineItemsGrid.AllowNavigation = false;
			this.ZoneRateLineItemsGrid.AllowSorting = false;
			this.ZoneRateLineItemsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "TM_Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TM_Break";
			zCalcEditColumnStyleInfo1.Decimals = 1;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.ColumnName = "TM_BreakWeightVolume";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CartageCalculatorPanel|15481793-7a71-4122-9621-e9211b1613a7", "Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "TM_RelevantValue";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "TM_FlatAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "TM_BreakMinimum";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo2.ColumnName = "TM_Text";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CombinedControl|7cba3a1c-04d3-40bf-8aa4-fc28b2654700", "Reason");
			zCheckBoxColumnStyleInfo1.ColumnName = "TM_CallForPricing";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.ZoneRateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ZoneRateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ZoneRateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ZoneRateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ZoneRateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ZoneRateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ZoneRateLineItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ZoneRateLineItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ZoneRateLineItemsGrid.GridId = "ed1160cb-71a1-4597-9c07-c7c01b0c6428";
			this.ZoneRateLineItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ZoneRateLineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ZoneRateLineItemsGrid.LayoutKey = "RateLineItemsGridCartageZone";
			this.ZoneRateLineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ZoneRateLineItemsGrid.Name = "ZoneRateLineItemsGrid";
			this.ZoneRateLineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 127, true);
			this.ZoneRateLineItemsGrid.TabIndex = 0;
			// 
			// CartageCalculatorPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "CartageCalculatorPanel";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 127, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel1.PerformLayout();
			this.SplitContainer.Panel2.ResumeLayout(false);
			this.SplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CartageZonesGrid)).EndInit();
			this.ToolStrip.ResumeLayout(false);
			this.ToolStrip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ZoneRateLineItemsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private CargoWise.Windows.UI.KToolStrip ToolStrip;
		private Enterprise.ZArchitecture.GUI.ZToolStripButton ZonesButton;
		public Enterprise.ZArchitecture.ZGrid CartageZonesGrid;
		public Enterprise.ZArchitecture.ZGrid ZoneRateLineItemsGrid;
	}
}
