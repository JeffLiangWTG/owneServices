using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Rating.GUI
{
	public class SummaryEntryPanel : RateEntryPanel, IEntryTabPage
	{
		public SummaryEntryPanel()
		{
			InitializeComponent();
			this.SetReadOnlyIncludingChildren(true);
			Tag = RatingConstants.RateCategory.SummaryRatesCategory;
		}

		#region Overrides

		public override ZGrid RateEntryGrid
		{
			get { return SummaryRateEntryGrid; }
		}

		public override RateLinesAndItemsControl RateLinesAndItemsControl
		{
			get { return SummaryRateLinesAndItemsControl; }
		}

		public override CostingRateLineAndItemsControl CostingRateLineAndItemsControl
		{
			get { return null; }
		}

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

		private ZGrid SummaryRateEntryGrid;
		private RateLinesAndItemsControl SummaryRateLinesAndItemsControl;

		#region Component Designer generated code

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private readonly System.ComponentModel.Container components;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SummaryRateEntryGrid = new ZGrid();
			this.SummaryRateLinesAndItemsControl = new RateLinesAndItemsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SummaryRateEntryGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(RatingHeader);
			// 
			// EntryGridPanel
			// 
			EntryGridPanel.Controls.Add(this.SummaryRateEntryGrid);
			// 
			// EntryRelatedControlsPanel
			// 
			CurrentEntryRelatedControlsPanel.Controls.Add(this.SummaryRateLinesAndItemsControl);
			CurrentEntryRelatedControlsPanelMininmum = 160;
			SplitterDistance = 436;
			// 
			// SummaryRateEntryGrid
			// 
			this.SummaryRateEntryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SummaryRateEntryGrid, "SummaryRateEntries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RatingHeader)(null)).SummaryRateEntries);
			this.SummaryRateEntryGrid.CaptionVisible = false;
			this.SummaryRateEntryGrid.GridId = "ea658592-8ef6-40ec-bb02-24325882d5b0";
			this.SummaryRateEntryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SummaryRateEntryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SummaryRateEntryGrid.LayoutKey = "SummaryRateEntryGrid";
			this.SummaryRateEntryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SummaryRateEntryGrid.Name = "SummaryRateEntryGrid";
			this.SummaryRateEntryGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.SummaryRateEntryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 436, true);
			this.SummaryRateEntryGrid.TabIndex = 3;
			// 
			// SummaryRateLinesAndItemsControl
			// 
			this.BindingSource.SetBindingMember(this.SummaryRateLinesAndItemsControl, ".");
			this.SummaryRateLinesAndItemsControl.BindTo = "SummaryRateEntries.RateLines";
			this.SummaryRateLinesAndItemsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SummaryRateLinesAndItemsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SummaryRateLinesAndItemsControl.Name = "SummaryRateLinesAndItemsControl";
			this.SummaryRateLinesAndItemsControl.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.SummaryRateLinesAndItemsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 160, true);
			this.SummaryRateLinesAndItemsControl.TabIndex = 4;
			// 
			// SummaryEntryPanel
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "SummaryEntryPanel";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 600, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SummaryRateEntryGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
