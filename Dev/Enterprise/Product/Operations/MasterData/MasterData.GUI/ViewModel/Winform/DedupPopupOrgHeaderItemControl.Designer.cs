namespace Enterprise.MasterData.GUI
{
	partial class DedupPopupOrgHeaderItemControl
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
			this.detailTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.fullNameCaptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.fullNameValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.codeCaptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.codeValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.containerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.containerInnerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.confidenceScorePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.scoreToolTip = new CargoWise.Windows.UI.KToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.detailTableLayoutPanel.SuspendLayout();
			this.containerPanel.SuspendLayout();
			this.containerInnerPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterData.GUI.DeduplicationResultsListDataSource);
			// 
			// detailTableLayoutPanel
			// 
			this.detailTableLayoutPanel.AutoSize = true;
			this.detailTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.detailTableLayoutPanel.BackColor = System.Drawing.Color.AliceBlue;
			this.detailTableLayoutPanel.ColumnCount = 2;
			this.detailTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.detailTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
			this.detailTableLayoutPanel.Controls.Add(this.fullNameCaptionLabel, 0, 0);
			this.detailTableLayoutPanel.Controls.Add(this.fullNameValueLabel, 1, 0);
			this.detailTableLayoutPanel.Controls.Add(this.codeCaptionLabel, 0, 1);
			this.detailTableLayoutPanel.Controls.Add(this.codeValueLabel, 1, 1);
			this.detailTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 5, true);
			this.detailTableLayoutPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 102, true);
			this.detailTableLayoutPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 43, true);
			this.detailTableLayoutPanel.Name = "detailTableLayoutPanel";
			this.detailTableLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.detailTableLayoutPanel.RowCount = 2;
			this.detailTableLayoutPanel.TabIndex = 1;
			// 
			// fullNameCaptionLabel
			// 
			this.fullNameCaptionLabel.AutoSize = true;
			this.fullNameCaptionLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.fullNameCaptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.fullNameCaptionLabel.IsFontBold = true;
			this.fullNameCaptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 4, true);
			this.fullNameCaptionLabel.Name = "fullNameCaptionLabel";
			this.fullNameCaptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 13, true);
			this.fullNameCaptionLabel.TabIndex = 1;
			this.fullNameCaptionLabel.UseMnemonic = false;
			// 
			// fullNameValueLabel
			// 
			this.fullNameValueLabel.AutoSize = true;
			this.fullNameValueLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.fullNameValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.fullNameValueLabel.IsFontBold = true;
			this.fullNameValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 4, true);
			this.fullNameValueLabel.Name = "fullNameValueLabel";
			this.fullNameValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 13, true);
			this.fullNameValueLabel.TabIndex = 1;
			this.fullNameValueLabel.UseMnemonic = false;
			// 
			// codeCaptionLabel
			// 
			this.codeCaptionLabel.AutoSize = true;
			this.codeCaptionLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.codeCaptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.codeCaptionLabel.IsFontBold = true;
			this.codeCaptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 27, true);
			this.codeCaptionLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 10, 0, 0, true);
			this.codeCaptionLabel.Name = "codeCaptionLabel";
			this.codeCaptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 13, true);
			this.codeCaptionLabel.TabIndex = 2;
			this.codeCaptionLabel.UseMnemonic = false;
			// 
			// codeValueLabel
			// 
			this.codeValueLabel.AutoSize = true;
			this.codeValueLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.codeValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.codeValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 27, true);
			this.codeValueLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 10, 0, 0, true);
			this.codeValueLabel.Name = "codeValueLabel";
			this.codeValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 12, true);
			this.codeValueLabel.TabIndex = 3;
			this.codeValueLabel.UseMnemonic = false;
			// 
			// containerPanel
			// 
			this.containerPanel.AutoSize = true;
			this.containerPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.containerPanel.Controls.Add(this.containerInnerPanel);
			this.containerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.containerPanel.Name = "containerPanel";
			this.containerPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.containerPanel.TabIndex = 0;
			// 
			// containerInnerPanel
			// 
			this.containerInnerPanel.AutoSize = true;
			this.containerInnerPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.containerInnerPanel.Controls.Add(this.confidenceScorePanel);
			this.containerInnerPanel.Controls.Add(this.detailTableLayoutPanel);
			this.containerInnerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerInnerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.containerInnerPanel.Name = "containerInnerPanel";
			this.containerInnerPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.containerInnerPanel.TabIndex = 0;
			// 
			// confidenceScorePanel
			//
			this.confidenceScorePanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.confidenceScorePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 2, true);
			this.confidenceScorePanel.Name = "confidenceScorePanel";
			this.confidenceScorePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2, 56, true);
			this.confidenceScorePanel.TabIndex = 0;
			// 
			// DedupPopupOrgHeaderItemControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.containerPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "DedupPopupOrgHeaderItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 60, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.detailTableLayoutPanel.ResumeLayout(false);
			this.detailTableLayoutPanel.PerformLayout();
			this.containerPanel.ResumeLayout(false);
			this.containerPanel.PerformLayout();
			this.containerInnerPanel.ResumeLayout(false);
			this.containerInnerPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KTableLayoutPanel detailTableLayoutPanel;
		internal ZArchitecture.ZLabel fullNameCaptionLabel;
		internal ZArchitecture.ZLabel fullNameValueLabel;
		internal ZArchitecture.ZLabel codeCaptionLabel;
		internal ZArchitecture.ZLabel codeValueLabel;
		internal ZArchitecture.GUI.ZPanel containerPanel;
		internal ZArchitecture.GUI.ZPanel containerInnerPanel;
		private ZArchitecture.GUI.ZPanel confidenceScorePanel;
		private CargoWise.Windows.UI.KToolTip scoreToolTip;
	}
}
