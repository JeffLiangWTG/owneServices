using System.Windows.Forms;

namespace Enterprise.Freight.PortHubs.GUI
{
	partial class PortHubFilterStripControl
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

		///// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.filterStripPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper);
			// 
			// filterStripPanel
			// 
			this.filterStripPanel.AutoScroll = true;
			this.filterStripPanel.AutoSize = true;
			this.filterStripPanel.BackColor = System.Drawing.Color.Transparent;
			this.filterStripPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.filterStripPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.filterStripPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.filterStripPanel.Name = "filterStripPanel";
			this.filterStripPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 150, true);
			this.filterStripPanel.TabIndex = 0;
			// 
			// PortHubFilterStripControl
			// 
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.filterStripPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Name = "PortHubFilterStripControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZPanel filterStripPanel;
	}
}
