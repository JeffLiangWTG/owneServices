namespace Enterprise.MasterData.GUI
{
	partial class OrganisationFilterContentControl
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
			this.FilterOperationUserControl = new Enterprise.MasterData.GUI.FilterOperationUserControl();
			this.DynamicFilterPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FilterOperationUserControl.SuspendLayout();
			this.DynamicFilterPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// FilterOperationUserControl
			// 
			this.FilterOperationUserControl.AllowDrop = true;
			this.FilterOperationUserControl.AutoSize = true;
			this.FilterOperationUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.FilterOperationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 60, true);
			this.FilterOperationUserControl.Name = "FilterOperationUserControl";
			this.FilterOperationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 24, true);
			this.FilterOperationUserControl.TabIndex = 3;
			// 
			// DynamicFilterPanel
			// 
			this.DynamicFilterPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 20, 0, true);
			this.DynamicFilterPanel.AutoSize = true;
			this.DynamicFilterPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.DynamicFilterPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.DynamicFilterPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DynamicFilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.DynamicFilterPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 135, true);
			this.DynamicFilterPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 45, true);
			this.DynamicFilterPanel.Name = "DynamicFilterPanel";
			this.DynamicFilterPanel.RowCount = 1;
			this.DynamicFilterPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.DynamicFilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 60, true);
			this.DynamicFilterPanel.TabIndex = 0;
			// 
			// OrganisationFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DynamicFilterPanel);
			this.Controls.Add(this.FilterOperationUserControl);
			this.Name = "OrganisationFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 84, true);
			this.AutoSize = true;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FilterOperationUserControl.ResumeLayout(true);
			this.FilterOperationUserControl.PerformLayout();
			this.DynamicFilterPanel.ResumeLayout(false);
			this.DynamicFilterPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KTableLayoutPanel DynamicFilterPanel;
		private FilterOperationUserControl FilterOperationUserControl;
	}
}
