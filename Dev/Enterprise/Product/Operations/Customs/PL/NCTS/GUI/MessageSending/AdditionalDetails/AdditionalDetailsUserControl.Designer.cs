using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI
{
	partial class AdditionalDetailsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DynamicAdditionalDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.NCTS.Business.MessageSendingObject);
			// 
			// DynamicAdditionalDetailsPanel
			//
			this.DynamicAdditionalDetailsPanel.AllowDrop = true;
			this.DynamicAdditionalDetailsPanel.AutoScroll = true;
			this.DynamicAdditionalDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicAdditionalDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DynamicAdditionalDetailsPanel.Name = "DynamicAdditionalDetailsPanel";
			this.DynamicAdditionalDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DynamicAdditionalDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 98, true);
			this.DynamicAdditionalDetailsPanel.TabIndex = 1;
			// 
			// MessageSendingFormJustificationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DynamicAdditionalDetailsPanel);
			this.Name = "AdditionalDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 98, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal DynamicLayoutPanel DynamicAdditionalDetailsPanel;
	}
}
