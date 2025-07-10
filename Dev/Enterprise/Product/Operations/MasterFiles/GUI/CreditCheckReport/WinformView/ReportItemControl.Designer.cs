namespace Enterprise.MasterFiles.GUI
{
	partial class ReportItemControl
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
			this.reportItemTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.reportInfoPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.reportInfoTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.reportNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.moreInfoLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.reportPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.getReportFlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.creditEventPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.getReportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.creditEventToolTip = new CargoWise.Windows.UI.KToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.reportItemTableLayoutPanel.SuspendLayout();
			this.reportInfoPanel.SuspendLayout();
			this.reportInfoTableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.reportPictureBox)).BeginInit();
			this.getReportFlowLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.creditEventPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// reportItemTableLayoutPanel
			// 
			this.reportItemTableLayoutPanel.ColumnCount = 3;
			this.reportItemTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9F));
			this.reportItemTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58F));
			this.reportItemTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
			this.reportItemTableLayoutPanel.Controls.Add(this.reportInfoPanel, 1, 0);
			this.reportItemTableLayoutPanel.Controls.Add(this.reportPictureBox, 0, 0);
			this.reportItemTableLayoutPanel.Controls.Add(this.getReportFlowLayoutPanel, 2, 0);
			this.reportItemTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.reportItemTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.reportItemTableLayoutPanel.Name = "reportItemTableLayoutPanel";
			this.reportItemTableLayoutPanel.RowCount = 1;
			this.reportItemTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.reportItemTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 45, true);
			this.reportItemTableLayoutPanel.TabIndex = 0;
			// 
			// reportInfoPanel
			// 
			this.reportInfoPanel.Controls.Add(this.reportInfoTableLayoutPanel);
			this.reportInfoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.reportInfoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(34, 2, true);
			this.reportInfoPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 0, 3, true);
			this.reportInfoPanel.Name = "reportInfoPanel";
			this.reportInfoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 41, true);
			this.reportInfoPanel.TabIndex = 2;
			// 
			// reportInfoTableLayoutPanel
			// 
			this.reportInfoTableLayoutPanel.ColumnCount = 1;
			this.reportInfoTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.reportInfoTableLayoutPanel.Controls.Add(this.reportNameLabel, 0, 0);
			this.reportInfoTableLayoutPanel.Controls.Add(this.moreInfoLinkLabel, 0, 1);
			this.reportInfoTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.reportInfoTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.reportInfoTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 0, 3, true);
			this.reportInfoTableLayoutPanel.Name = "reportInfoTableLayoutPanel";
			this.reportInfoTableLayoutPanel.RowCount = 2;
			this.reportInfoTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.reportInfoTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.reportInfoTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 41, true);
			this.reportInfoTableLayoutPanel.TabIndex = 3;
			// 
			// reportNameLabel
			// 
			this.reportNameLabel.AutoSize = true;
			this.reportNameLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.reportNameLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
			this.reportNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.reportNameLabel.Name = "reportNameLabel";
			this.reportNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 16, true);
			this.reportNameLabel.TabIndex = 4;
			// 
			// moreInfoLinkLabel
			// 
			this.moreInfoLinkLabel.AutoSize = true;
			this.moreInfoLinkLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.moreInfoLinkLabel.IsFontBold = false;
			this.moreInfoLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.moreInfoLinkLabel.Name = "moreInfoLinkLabel";
			this.moreInfoLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 13, true);
			this.moreInfoLinkLabel.TabIndex = 5;
			this.moreInfoLinkLabel.TabStop = false;
			// 
			// reportPictureBox
			// 
			this.reportPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.reportPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.reportPictureBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 0, 3, true);
			this.reportPictureBox.Name = "reportPictureBox";
			this.reportPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 41, true);
			this.reportPictureBox.TabIndex = 1;
			this.reportPictureBox.TabStop = false;
			// 
			// getReportFlowLayoutPanel
			//
			this.getReportFlowLayoutPanel.Controls.Add(this.getReportButton);
			this.getReportFlowLayoutPanel.Controls.Add(this.creditEventPictureBox);
			this.getReportFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.getReportFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
			this.getReportFlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 2, true);
			this.getReportFlowLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 3, 3, true);
			this.getReportFlowLayoutPanel.Name = "getReportFlowLayoutPanel";
			this.getReportFlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 41, true);
			this.getReportFlowLayoutPanel.TabIndex = 6;
			// 
			// creditEventPictureBox
			// 
			this.creditEventPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 7, true);
			this.creditEventPictureBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 7, 2, 2, true);
			this.creditEventPictureBox.Name = "creditEventPictureBox";
			this.creditEventPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 26, true);
			this.creditEventPictureBox.TabIndex = 8;
			this.creditEventPictureBox.TabStop = false;
			// 
			// getReportButton
			// 
			this.getReportButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.getReportButton.BackColor = System.Drawing.Color.DeepSkyBlue;
			this.getReportButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.getReportButton.ForeColor = System.Drawing.Color.White;
			this.getReportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 5, true);
			this.getReportButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 5, 0, 2, true);
			this.getReportButton.Name = "getReportButton";
			this.getReportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 29, true);
			this.getReportButton.TabIndex = 7;
			this.getReportButton.ToolTipCaption = null;
			this.getReportButton.UseVisualStyleBackColor = false;
			this.getReportButton.Click += new System.EventHandler(this.GetReportButton_Click);
			// 
			// ReportItemControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.reportItemTableLayoutPanel);
			this.Name = "ReportItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 45, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.reportItemTableLayoutPanel.ResumeLayout(false);
			this.reportItemTableLayoutPanel.PerformLayout();
			this.reportInfoPanel.ResumeLayout(false);
			this.reportInfoPanel.PerformLayout();
			this.reportInfoTableLayoutPanel.ResumeLayout(false);
			this.reportInfoTableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.reportPictureBox)).EndInit();
			this.getReportFlowLayoutPanel.ResumeLayout(false);
			this.getReportFlowLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.creditEventPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel reportItemTableLayoutPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel reportInfoPanel;
		private CargoWise.Windows.UI.KTableLayoutPanel reportInfoTableLayoutPanel;
		internal ZArchitecture.GUI.ZPictureBox reportPictureBox;
		private CargoWise.Windows.UI.KFlowLayoutPanel getReportFlowLayoutPanel;
		internal ZArchitecture.GUI.ZPictureBox creditEventPictureBox;
		internal Enterprise.ZArchitecture.GUI.ZButton getReportButton;
		internal Enterprise.ZArchitecture.ZLabel reportNameLabel;
		internal Enterprise.ZArchitecture.GUI.ZLinkLabel moreInfoLinkLabel;
		internal CargoWise.Windows.UI.KToolTip creditEventToolTip;
	}
}
