namespace Enterprise.Rating.GUI
{
	public partial class BulkUpdateEntriesPage
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
			this.DeSelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EntriesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.panelContentContainer = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EntriesGrid)).BeginInit();
			this.panelContentContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.Business.BulkRateUpdater);
			// 
			// DeSelectAllButton
			// 
			this.DeSelectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DeSelectAllButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateForm|aef5aa3d-95c1-4e6f-85d7-2596c2876e96", "De-Select All");
			this.DeSelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 432, true);
			this.DeSelectAllButton.Name = "DeSelectAllButton";
			this.DeSelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 23, true);
			this.DeSelectAllButton.TabIndex = 5;
			this.DeSelectAllButton.UseVisualStyleBackColor = true;
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SelectAllButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateForm|cba4cbbf-2215-4a85-8a1d-ca2244cc8d18", "Select All");
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 432, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 23, true);
			this.SelectAllButton.TabIndex = 4;
			this.SelectAllButton.UseVisualStyleBackColor = true;
			// 
			// EntriesGrid
			// 
			this.EntriesGrid.AllowNavigation = false;
			this.EntriesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EntriesGrid, "Entries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).Entries)));
			this.EntriesGrid.CaptionVisible = false;
			this.EntriesGrid.GridId = "8e4f28bc-b1af-40ea-84a5-459ac0de6349";
			this.EntriesGrid.CopySelectedRowsAllowed = true;
			this.EntriesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntriesGrid.IsWholeRowSelectedOnClick = true;
			this.EntriesGrid.LayoutKey = "EntiresGridKey";
			this.EntriesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntriesGrid.Name = "EntriesGrid";
			this.EntriesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.EntriesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 426, true);
			this.EntriesGrid.TabIndex = 3;
			// 
			// panelContentContainer
			// 
			this.panelContentContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panelContentContainer.Controls.Add(this.EntriesGrid);
			this.panelContentContainer.Controls.Add(this.DeSelectAllButton);
			this.panelContentContainer.Controls.Add(this.SelectAllButton);
			this.panelContentContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.panelContentContainer.Name = "panelContentContainer";
			this.panelContentContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 455, true);
			this.panelContentContainer.TabIndex = 6;
			// 
			// BulkUpdateEntriesPage
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.panelContentContainer);
			this.Name = "BulkUpdateEntriesPage";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 475, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EntriesGrid)).EndInit();
			this.panelContentContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		private Enterprise.ZArchitecture.GUI.ZButton DeSelectAllButton;
		private Enterprise.ZArchitecture.GUI.ZButton SelectAllButton;
		private Enterprise.ZArchitecture.ZGrid EntriesGrid;
		private Enterprise.ZArchitecture.GUI.ZPanel panelContentContainer;
	}
}
