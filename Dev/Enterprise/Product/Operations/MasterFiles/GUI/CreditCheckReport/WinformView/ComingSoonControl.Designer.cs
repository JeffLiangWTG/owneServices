namespace Enterprise.MasterFiles.GUI
{
	partial class ComingSoonControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.comingSoonControlTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.comingSoonPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.comingSoonLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.comingSoonControlTableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comingSoonPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.GUI.ComingSoonModel);
			// 
			// comingSoonControlTableLayoutPanel
			// 
			this.comingSoonControlTableLayoutPanel.ColumnCount = 1;
			this.comingSoonControlTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.comingSoonControlTableLayoutPanel.Controls.Add(this.comingSoonPictureBox, 0, 1);
			this.comingSoonControlTableLayoutPanel.Controls.Add(this.comingSoonLabel, 0, 2);
			this.comingSoonControlTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.comingSoonControlTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.comingSoonControlTableLayoutPanel.Name = "comingSoonControlTableLayoutPanel";
			this.comingSoonControlTableLayoutPanel.RowCount = 4;
			this.comingSoonControlTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.comingSoonControlTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200)));
			this.comingSoonControlTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(48)));
			this.comingSoonControlTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.comingSoonControlTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 667, true);
			this.comingSoonControlTableLayoutPanel.TabIndex = 0;
			// 
			// comingSoonPictureBox
			// 
			this.comingSoonPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.comingSoonPictureBox.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.ComingSoonBackground;
			this.comingSoonPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 209, true);
			this.comingSoonPictureBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.comingSoonPictureBox.Name = "comingSoonPictureBox";
			this.comingSoonPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 200, true);
			this.comingSoonPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.comingSoonPictureBox.TabIndex = 1;
			this.comingSoonPictureBox.TabStop = false;
			// 
			// comingSoonLabel
			// 
			this.comingSoonLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comingSoonLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.comingSoonLabel, "ComingSoonString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.ComingSoonModel)(null)).ComingSoonString)));
			this.comingSoonLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)(((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.Bold) 
            | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.comingSoonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 411, true);
			this.comingSoonLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.comingSoonLabel.Name = "comingSoonLabel";
			this.comingSoonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 19, true);
			this.comingSoonLabel.TabIndex = 4;
			this.comingSoonLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ComingSoonControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.comingSoonControlTableLayoutPanel);
			this.Name = "ComingSoonControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 667, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.comingSoonControlTableLayoutPanel.ResumeLayout(false);
			this.comingSoonControlTableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comingSoonPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel comingSoonControlTableLayoutPanel;
		private ZArchitecture.GUI.ZPictureBox comingSoonPictureBox;
		private ZArchitecture.ZLabel comingSoonLabel;
	}
}
