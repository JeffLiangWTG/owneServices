using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.GUI
{
	public class RateEntryPanelWithRelatedLines : RateEntryPanel
	{
		public RateEntryPanelWithRelatedLines()
		{
			InitializeComponent();

			// inherit from RateEntryPanel
			EntryGridPanel.Controls.Add(TemplateRateEntryGrid);
			CurrentEntryRelatedControlsPanel.Controls.Add(splitContainer2);
			CurrentEntryRelatedControlsPanelMininmum = ControlDpiScalingHelper.ScaleToCurrentDpiY(344);
			SplitterDistance = ControlDpiScalingHelper.ScaleToCurrentDpiY(428);
		}

		#region Overrides

		public override ZGrid RateEntryGrid =>
			TemplateRateEntryGrid;

		public override RateLinesAndItemsControl RateLinesAndItemsControl =>
			TemplateRateLinesAndItemsControl;

		public override CostingRateLineAndItemsControl CostingRateLineAndItemsControl =>
			TemplateCostingRateLineAndItemsControl.InnerControl;

		protected override IEnumerable<string> CategoriesWithLocations =>
			new[]
			{
				RatingConstants.RateCategory.AIR,
				RatingConstants.RateCategory.FCL,
				RatingConstants.RateCategory.LCL,
				RatingConstants.RateCategory.ORG,
				RatingConstants.RateCategory.DST,
			};

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region Component Designer generated code

		EntryGrid TemplateRateEntryGrid;
		KSplitContainer splitContainer2;
		CompanyTariffAndCostingRateLineAndItemsLazyControl TemplateCostingRateLineAndItemsControl;
		RateLinesAndItemsControl TemplateRateLinesAndItemsControl;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		readonly System.ComponentModel.Container components;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.splitContainer2 = new KSplitContainer();
			this.TemplateRateLinesAndItemsControl = new RateLinesAndItemsControl();
			this.TemplateCostingRateLineAndItemsControl = new CompanyTariffAndCostingRateLineAndItemsLazyControl();
			this.TemplateRateEntryGrid = new EntryGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.TemplateRateLinesAndItemsControl.SuspendLayout();
			this.TemplateCostingRateLineAndItemsControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TemplateRateEntryGrid)).BeginInit();
			this.TemplateRateEntryGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(RatingHeader);
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.TemplateRateLinesAndItemsControl);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.TemplateCostingRateLineAndItemsControl);
			this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 344, true);
			this.splitContainer2.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(160);
			this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(180);
			this.splitContainer2.TabIndex = 0;
			// 
			// TemplateRateLinesAndItemsControl
			// 
			this.TemplateRateLinesAndItemsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TemplateRateLinesAndItemsControl, ".");
			this.TemplateRateLinesAndItemsControl.BindTo = "SummaryRateEntries.FilteredRateLinesForBinding";
			this.TemplateRateLinesAndItemsControl.CalculatorPanelAgentRatesCheckBoxVisible = true;
			this.TemplateRateLinesAndItemsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TemplateRateLinesAndItemsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TemplateRateLinesAndItemsControl.Name = "TemplateRateLinesAndItemsControl";
			this.TemplateRateLinesAndItemsControl.RemoveAction = Enterprise.ZArchitecture.RemoveAction.RemoveAndDelete;
			this.TemplateRateLinesAndItemsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 180, true);
			this.TemplateRateLinesAndItemsControl.TabIndex = 4;
			// 
			// TemplateCostingRateLineAndItemsControl
			// 
			this.TemplateCostingRateLineAndItemsControl.AllowDrop = true;
			this.TemplateCostingRateLineAndItemsControl.BindTo = "SummaryRateEntries.RelatedRateLines";
			this.TemplateCostingRateLineAndItemsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TemplateCostingRateLineAndItemsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TemplateCostingRateLineAndItemsControl.Name = "TemplateCostingRateLineAndItemsControl";
			this.TemplateCostingRateLineAndItemsControl.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.TemplateCostingRateLineAndItemsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 161, true);
			this.TemplateCostingRateLineAndItemsControl.TabIndex = 5;
			// 
			// TemplateRateEntryGrid
			// 
			this.TemplateRateEntryGrid.AllowDrop = true;
			this.TemplateRateEntryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TemplateRateEntryGrid, "SummaryRateEntries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RatingHeader)(null)).SummaryRateEntries);
			this.TemplateRateEntryGrid.CaptionVisible = false;
			this.TemplateRateEntryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TemplateRateEntryGrid.GridId = "d7f17b47-8340-4353-9b6d-05371f1a4484";
			this.TemplateRateEntryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TemplateRateEntryGrid.LayoutKey = "TemplateRateEntryGrid";
			this.TemplateRateEntryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TemplateRateEntryGrid.Name = "TemplateRateEntryGrid";
			this.TemplateRateEntryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 253, true);
			this.TemplateRateEntryGrid.TabIndex = 0;
			this.TemplateRateEntryGrid.TabStop = false;
			// 
			// RateEntryPanelWithRelatedLines
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "RateEntryPanelWithRelatedLines";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer2.PerformLayout();
			this.TemplateRateLinesAndItemsControl.ResumeLayout(true);
			this.TemplateRateLinesAndItemsControl.PerformLayout();
			this.TemplateCostingRateLineAndItemsControl.ResumeLayout(true);
			this.TemplateCostingRateLineAndItemsControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TemplateRateEntryGrid)).EndInit();
			this.TemplateRateEntryGrid.ResumeLayout(false);
			this.TemplateRateEntryGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
