using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI
{
	partial class AdditionalDetailsUserControl
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
			this.DynamicAdditionalDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.BaseMessageSendingObject);
			// 
			// DynamicAdditionalDetailsPanel
			// 
			this.DynamicAdditionalDetailsPanel.AllowDrop = true;
			this.DynamicAdditionalDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicAdditionalDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DynamicAdditionalDetailsPanel.Name = "DynamicAdditionalDetailsPanel";
			this.DynamicAdditionalDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 128, true);
			this.DynamicAdditionalDetailsPanel.TabIndex = 1;
			// 
			// AdditionalDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DynamicAdditionalDetailsPanel);
			this.Name = "AdditionalDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 128, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal DynamicLayoutPanel DynamicAdditionalDetailsPanel;
	}
}
