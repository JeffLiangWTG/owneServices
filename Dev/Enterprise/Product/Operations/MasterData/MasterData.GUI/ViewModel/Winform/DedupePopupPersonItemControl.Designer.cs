namespace Enterprise.MasterData.GUI
{
	partial class DedupePopupPersonItemControl
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
			this.containerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.containerInnerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.confidenceScorePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.contentTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.fullnameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.personTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.mainInfoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.scoreToolTip = new CargoWise.Windows.UI.KToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.containerPanel.SuspendLayout();
			this.containerInnerPanel.SuspendLayout();
			this.contentTableLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// containerPanel
			// 
			this.containerPanel.AutoSize = true;
			this.containerPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.containerPanel.Controls.Add(this.containerInnerPanel);
			this.containerPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.containerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.containerPanel.Name = "containerPanel";
			this.containerPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.containerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 60, true);
			this.containerPanel.TabIndex = 0;
			// 
			// containerInnerPanel
			// 
			this.containerInnerPanel.AutoSize = true;
			this.containerInnerPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.containerInnerPanel.Controls.Add(this.confidenceScorePanel);
			this.containerInnerPanel.Controls.Add(this.contentTableLayoutPanel);
			this.containerInnerPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.containerInnerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.containerInnerPanel.Name = "containerInnerPanel";
			this.containerInnerPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 5, 5, 5, true);
			this.containerInnerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 58, true);
			this.containerInnerPanel.TabIndex = 0;
			// 
			// confidenceScorePanel
			// 
			this.confidenceScorePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.confidenceScorePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 2, true);
			this.confidenceScorePanel.Name = "confidenceScorePanel";
			this.confidenceScorePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2, 58, true);
			this.confidenceScorePanel.TabIndex = 0;
			// 
			// contentTableLayoutPanel
			// 
			this.contentTableLayoutPanel.AutoSize = true;
			this.contentTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.contentTableLayoutPanel.BackColor = System.Drawing.Color.AliceBlue;
			this.contentTableLayoutPanel.ColumnCount = 2;
			this.contentTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
			this.contentTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68F));
			this.contentTableLayoutPanel.Controls.Add(this.fullnameLabel, 0, 0);
			this.contentTableLayoutPanel.Controls.Add(this.personTypeLabel, 0, 1);
			this.contentTableLayoutPanel.Controls.Add(this.mainInfoLabel, 1, 1);
			this.contentTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.contentTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 5, true);
			this.contentTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, true);
			this.contentTableLayoutPanel.Name = "contentTableLayoutPanel";
			this.contentTableLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.contentTableLayoutPanel.RowCount = 2;
			this.contentTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(16)));
			this.contentTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.contentTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 49, true);
			this.contentTableLayoutPanel.TabIndex = 0;
			// 
			// fullnameLabel
			// 
			this.fullnameLabel.AutoSize = true;
			this.contentTableLayoutPanel.SetColumnSpan(this.fullnameLabel, 2);
			this.fullnameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.fullnameLabel.IsFontBold = true;
			this.fullnameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.fullnameLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.fullnameLabel.Name = "fullnameLabel";
			this.fullnameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.fullnameLabel.TabIndex = 1;
			this.fullnameLabel.UseMnemonic = false;
			// 
			// personTypeLabel
			// 
			this.personTypeLabel.AutoSize = true;
			this.personTypeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.personTypeLabel.IsFontBold = true;
			this.personTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 30, true);
			this.personTypeLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 10, 0, 0, true);
			this.personTypeLabel.Name = "personTypeLabel";
			this.personTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.personTypeLabel.TabIndex = 1;
			this.personTypeLabel.UseMnemonic = false;
			// 
			// mainInfoLabel
			//
			this.mainInfoLabel.AutoEllipsis = true;
			this.mainInfoLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainInfoLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.mainInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 30, true);
			this.mainInfoLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 10, 0, 0, true);
			this.mainInfoLabel.Name = "mainInfoLabel";
			this.mainInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.mainInfoLabel.TabIndex = 2;
			this.mainInfoLabel.UseMnemonic = false;
			// 
			// DedupePopupPersonItemControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.containerPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 53, true);
			this.Name = "DedupePopupPersonItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 60, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.containerPanel.ResumeLayout(false);
			this.containerPanel.PerformLayout();
			this.containerInnerPanel.ResumeLayout(false);
			this.containerInnerPanel.PerformLayout();
			this.contentTableLayoutPanel.ResumeLayout(false);
			this.contentTableLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel containerPanel;
		private ZArchitecture.GUI.ZPanel containerInnerPanel;
		private CargoWise.Windows.UI.KTableLayoutPanel contentTableLayoutPanel;
		private ZArchitecture.GUI.ZPanel confidenceScorePanel;
		private ZArchitecture.ZLabel fullnameLabel;
		private ZArchitecture.ZLabel personTypeLabel;
		private ZArchitecture.ZLabel mainInfoLabel;
		private CargoWise.Windows.UI.KToolTip scoreToolTip;
	}
}
