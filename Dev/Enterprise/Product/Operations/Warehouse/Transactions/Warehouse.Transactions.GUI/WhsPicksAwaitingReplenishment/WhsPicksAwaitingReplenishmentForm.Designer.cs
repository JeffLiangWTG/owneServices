using System.Drawing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.GUI.PickFaces;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.WhsPicksAwaitingReplenishment
{
	partial class WhsPicksAwaitingReplenishmentForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            this.DetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.ClientProductAwaitingPicksGridUserControl = new Enterprise.Warehouse.Environment.GUI.PickFaces.ClientProductAwaitingPicksGridUserControl();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.DetailsPanel.SuspendLayout();
            this.ClientProductAwaitingPicksGridUserControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 570, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsPick);
            // 
            // DetailsPanel
            // 
            this.DetailsPanel.Controls.Add(this.ClientProductAwaitingPicksGridUserControl);
            this.DetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.DetailsPanel.Name = "DetailsPanel";
            this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1135, 990, true);
            this.DetailsPanel.TabIndex = 3;
            // 
            // ClientProductAwaitingPicksGridUserControl
            // 
            this.ClientProductAwaitingPicksGridUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ClientProductAwaitingPicksGridUserControl, ".");
            this.ClientProductAwaitingPicksGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ClientProductAwaitingPicksGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ClientProductAwaitingPicksGridUserControl.Name = "ClientProductAwaitingPicksGridUserControl";
            this.ClientProductAwaitingPicksGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 594, true);
            this.ClientProductAwaitingPicksGridUserControl.TabIndex = 3;
            this.ClientProductAwaitingPicksGridUserControl.PicksAwaitingGrid.ColourDeciding += PicksAwaitingGrid_ColourDeciding;
            // 
            // WhsPicksAwaitingReplenishmentForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 594, true);
            this.Controls.Add(this.DetailsPanel);
            this.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsPick);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 470, true);
            this.Name = "WhsPicksAwaitingReplenishmentForm";
            this.ShouldSerializeTabPageMethods = false;
            this.Text = "WhsPicksAwaitingReplenishmentForm";
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.DetailsPanel.ResumeLayout(false);
            this.DetailsPanel.PerformLayout();
            this.ClientProductAwaitingPicksGridUserControl.ResumeLayout(true);
            this.ClientProductAwaitingPicksGridUserControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private ZPanel DetailsPanel;
		private ClientProductAwaitingPicksGridUserControl ClientProductAwaitingPicksGridUserControl;
	}
}
