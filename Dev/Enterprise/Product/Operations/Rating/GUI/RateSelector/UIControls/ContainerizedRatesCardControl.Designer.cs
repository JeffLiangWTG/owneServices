namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	partial class ContainerizedRatesCardControl
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
            this.components = new System.ComponentModel.Container();
            this.tabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
            this.zTabPage1 = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.zTabPage2 = new Enterprise.ZArchitecture.GUI.ZTabPage();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.tabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.tabControl.Controls.Add(this.zTabPage1);
            this.tabControl.Controls.Add(this.zTabPage2);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.tabControl.Name = "tabControl";
            this.tabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 210, true);
            this.tabControl.TabIndex = 0;
            // 
            // zTabPage1
            // 
            this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.zTabPage1.Name = "zTabPage1";
            this.zTabPage1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 183, true);
            this.zTabPage1.TabIndex = 0;
            this.zTabPage1.Text = "zTabPage1";
            this.zTabPage1.UseVisualStyleBackColor = true;
            // 
            // zTabPage2
            // 
            this.zTabPage2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.zTabPage2.Name = "zTabPage2";
            this.zTabPage2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.zTabPage2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 183, true);
            this.zTabPage2.TabIndex = 1;
            this.zTabPage2.Text = "zTabPage2";
            this.zTabPage2.UseVisualStyleBackColor = true;
            // 
            // ContainerizedRatesCardControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.tabControl);
            this.Name = "ContainerizedRatesCardControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 210, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.tabControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl tabControl;
		private ZArchitecture.GUI.ZTabPage zTabPage1;
		private ZArchitecture.GUI.ZTabPage zTabPage2;
	}
}
