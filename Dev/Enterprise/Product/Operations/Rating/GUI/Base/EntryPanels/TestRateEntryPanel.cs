using Enterprise.ZArchitecture;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Rating.GUI
{
	public class TestRateEntryPanel : RateEntryPanel
	{
		public TestRateEntryPanel()
		{
			InitializeComponent();
		}

		#region Overrides

		public override ZGrid RateEntryGrid
		{
			get { return TemplateRateEntryGrid; }
		}

		public override RateLinesAndItemsControl RateLinesAndItemsControl
		{
			get { return TemplateRateLinesAndItemsControl; }
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

		#region Component Designer generated code

		private EntryGrid TemplateRateEntryGrid;
		private RateLinesAndItemsControl TemplateRateLinesAndItemsControl;
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
			this.TemplateRateEntryGrid = new EntryGrid();
			this.TemplateRateLinesAndItemsControl = new RateLinesAndItemsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TemplateRateEntryGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.RatingHeader);
			// 
			// EntryGridPanel
			// 
			this.EntryGridPanel.Controls.Add(this.TemplateRateEntryGrid);
			// 
			// EntryRelatedControlsPanel
			// 
			CurrentEntryRelatedControlsPanel.Controls.Add(this.TemplateRateLinesAndItemsControl);
			CurrentEntryRelatedControlsPanelMininmum = 160;
			SplitterDistance = 436;
			// 
			// TemplateRateEntryGrid
			// 
			this.TemplateRateEntryGrid.AllowDrop = true;
			this.TemplateRateEntryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TemplateRateEntryGrid, "SummaryRateEntries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RatingHeader)(null)).SummaryRateEntries);
			this.TemplateRateEntryGrid.CaptionVisible = false;
			this.TemplateRateEntryGrid.GridId = "0d5eac41-5841-41d6-9f49-b6a71785c267";
			this.TemplateRateEntryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TemplateRateEntryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TemplateRateEntryGrid.LayoutKey = "TemplateRateEntryGrid";
			this.TemplateRateEntryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TemplateRateEntryGrid.Name = "TemplateRateEntryGrid";
			this.TemplateRateEntryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 436, true);
			this.TemplateRateEntryGrid.TabIndex = 8;
			// 
			// TemplateRateLinesAndItemsControl
			// 
			this.BindingSource.SetBindingMember(this.TemplateRateLinesAndItemsControl, ".");
			this.TemplateRateLinesAndItemsControl.BindTo = "SummaryRateEntries.RateLines";
			this.TemplateRateLinesAndItemsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TemplateRateLinesAndItemsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TemplateRateLinesAndItemsControl.Name = "TemplateRateLinesAndItemsControl";
			this.TemplateRateLinesAndItemsControl.RemoveAction = Enterprise.ZArchitecture.RemoveAction.RemoveAndDelete;
			this.TemplateRateLinesAndItemsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 160, true);
			this.TemplateRateLinesAndItemsControl.TabIndex = 8;
			// 
			// TestRateEntryPanel
			// 
			this.Name = "TestRateEntryPanel";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 600, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TemplateRateEntryGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
