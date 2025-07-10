namespace Enterprise.MasterFiles.GUI
{
	partial class ReportsControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.chooseYourReportLabel = new Enterprise.ZArchitecture.ZLabel();
			this.providedByLabel = new Enterprise.ZArchitecture.ZLabel();
			this.illionPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.chooseYourReportPannel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.flowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.illionPictureBox)).BeginInit();
			this.chooseYourReportPannel.SuspendLayout();
			this.flowLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.GUI.ReportsModel);
			// 
			// chooseYourReportLabel
			// 
			this.chooseYourReportLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.chooseYourReportLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e569391f-7c23-4878-bb27-0b9ede1654f9", "Choose your report");
			this.chooseYourReportLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.chooseYourReportLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.chooseYourReportLabel.Name = "chooseYourReportLabel";
			this.chooseYourReportLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 40, true);
			this.chooseYourReportLabel.TabIndex = 2;
			this.chooseYourReportLabel.UseMnemonic = false;
			// 
			// providedByLabel
			// 
			this.providedByLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.providedByLabel.AutoSize = true;
			this.providedByLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7708d3be-ea81-4528-88a8-431260b70965", "Provided by:");
			this.providedByLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.providedByLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(237, 0, true);
			this.providedByLabel.Name = "providedByLabel";
			this.providedByLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 40, true);
			this.providedByLabel.TabIndex = 3;
			this.providedByLabel.UseMnemonic = false;
			// 
			// illionPictureBox
			// 
			this.illionPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.illionPictureBox.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.IllionLogo;
			this.illionPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 0, true);
			this.illionPictureBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.illionPictureBox.Name = "illionPictureBox";
			this.illionPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 40, true);
			this.illionPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.illionPictureBox.TabIndex = 4;
			this.illionPictureBox.TabStop = false;
			// 
			// chooseYourReportPannel
			// 
			this.chooseYourReportPannel.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.chooseYourReportPannel.ColumnCount = 3;
			this.chooseYourReportPannel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
			this.chooseYourReportPannel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.chooseYourReportPannel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.chooseYourReportPannel.Controls.Add(this.chooseYourReportLabel, 0, 0);
			this.chooseYourReportPannel.Controls.Add(this.providedByLabel, 1, 0);
			this.chooseYourReportPannel.Controls.Add(this.illionPictureBox, 2, 0);
			this.chooseYourReportPannel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.chooseYourReportPannel.Name = "chooseYourReportPannel";
			this.chooseYourReportPannel.RowCount = 1;
			this.chooseYourReportPannel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.chooseYourReportPannel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 40, true);
			this.chooseYourReportPannel.TabIndex = 1;
			// 
			// flowLayoutPanel
			// 
			this.flowLayoutPanel.AutoSize = true;
			this.flowLayoutPanel.Controls.Add(this.chooseYourReportPannel);
			this.flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.flowLayoutPanel.Name = "flowLayoutPanel";
			this.flowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 520, true);
			this.flowLayoutPanel.TabIndex = 0;
			// 
			// ReportsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.flowLayoutPanel);
			this.Name = "ReportsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 520, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.illionPictureBox)).EndInit();
			this.chooseYourReportPannel.ResumeLayout(false);
			this.chooseYourReportPannel.PerformLayout();
			this.flowLayoutPanel.ResumeLayout(false);
			this.flowLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KFlowLayoutPanel flowLayoutPanel;
		private Enterprise.ZArchitecture.ZLabel chooseYourReportLabel;
		private Enterprise.ZArchitecture.ZLabel providedByLabel;

		private ZArchitecture.GUI.ZPictureBox illionPictureBox;
		private CargoWise.Windows.UI.KTableLayoutPanel chooseYourReportPannel;
	}
}
