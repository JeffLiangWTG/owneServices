namespace Enterprise.Rating.GUI
{
	public partial class BulkUpdatePreviewPage
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.rateLinesAndItemsControl = new Enterprise.Rating.GUI.RateLinesAndItemsControl();
			this.PreviewEntriesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.panelContainer = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviewEntriesGrid)).BeginInit();
			this.panelContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.Business.BulkRateUpdater);
			// 
			// rateLinesAndItemsControl
			// 
			this.rateLinesAndItemsControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.rateLinesAndItemsControl, ".");
			this.rateLinesAndItemsControl.BindTo = "PreviewEntries.RateLines";
			this.rateLinesAndItemsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 266, true);
			this.rateLinesAndItemsControl.Name = "rateLinesAndItemsControl";
			this.rateLinesAndItemsControl.RemoveAction = Enterprise.ZArchitecture.RemoveAction.RemoveAndDelete;
			this.rateLinesAndItemsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 189, true);
			this.rateLinesAndItemsControl.TabIndex = 4;
			// 
			// PreviewEntriesGrid
			// 
			this.PreviewEntriesGrid.AllowNavigation = false;
			this.PreviewEntriesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PreviewEntriesGrid, "PreviewEntries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).PreviewEntries)));
			this.PreviewEntriesGrid.CaptionVisible = false;
			this.PreviewEntriesGrid.GridId = "f6f7ebf5-47d5-46cd-a259-d04716e5d008";
			this.PreviewEntriesGrid.CopySelectedRowsAllowed = true;
			this.PreviewEntriesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PreviewEntriesGrid.IsWholeRowSelectedOnClick = true;
			this.PreviewEntriesGrid.LayoutKey = "PreviewEntriesGridKey";
			this.PreviewEntriesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreviewEntriesGrid.Name = "PreviewEntriesGrid";
			this.PreviewEntriesGrid.ReadOnly = true;
			this.PreviewEntriesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.PreviewEntriesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 260, true);
			this.PreviewEntriesGrid.TabIndex = 3;
			// 
			// panelContainer
			// 
			this.panelContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panelContainer.Controls.Add(this.PreviewEntriesGrid);
			this.panelContainer.Controls.Add(this.rateLinesAndItemsControl);
			this.panelContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.panelContainer.Name = "panelContainer";
			this.panelContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 455, true);
			this.panelContainer.TabIndex = 5;
			// 
			// BulkUpdatePreviewPage
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.panelContainer);
			this.Name = "BulkUpdatePreviewPage";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 475, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviewEntriesGrid)).EndInit();
			this.panelContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		private RateLinesAndItemsControl rateLinesAndItemsControl;
		private Enterprise.ZArchitecture.ZGrid PreviewEntriesGrid;
		private Enterprise.ZArchitecture.GUI.ZPanel panelContainer;
	}
}
