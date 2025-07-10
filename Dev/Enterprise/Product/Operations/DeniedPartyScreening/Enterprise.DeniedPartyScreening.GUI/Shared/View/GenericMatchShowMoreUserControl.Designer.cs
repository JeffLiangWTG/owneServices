namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class GenericMatchShowMoreUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ShowMoreLessLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.OtherInfoBorderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OtherInfoPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OtherItemsPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OtherInfoBorderPanel.SuspendLayout();
			this.OtherInfoPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// ShowMoreLessLabel
			// 
			this.ShowMoreLessLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ShowMoreLessLabel.IsFontBold = false;
			this.ShowMoreLessLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.ShowMoreLessLabel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(112)))), ((int)(((byte)(154)))));
			this.ShowMoreLessLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShowMoreLessLabel.Name = "ShowMoreLessLabel";
			this.ShowMoreLessLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 0, 8, 0, true);
			this.ShowMoreLessLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 23, true);
			this.ShowMoreLessLabel.TabIndex = 1;
			this.ShowMoreLessLabel.TabStop = false;
			this.ShowMoreLessLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ShowMoreLessPanel_LinkClicked);
			// 
			// OtherInfoBorderPanel
			// 
			this.OtherInfoBorderPanel.AutoSize = true;
			this.OtherInfoBorderPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.OtherInfoBorderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(234)))), ((int)(((byte)(236)))));
			this.OtherInfoBorderPanel.Controls.Add(this.OtherInfoPanel);
			this.OtherInfoBorderPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OtherInfoBorderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.OtherInfoBorderPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.OtherInfoBorderPanel.Name = "OtherInfoBorderPanel";
			this.OtherInfoBorderPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.OtherInfoBorderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 102, true);
			this.OtherInfoBorderPanel.TabIndex = 2;
			// 
			// OtherInfoPanel
			// 
			this.OtherInfoPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.OtherInfoPanel.BackColor = System.Drawing.Color.White;
			this.OtherInfoPanel.Controls.Add(this.OtherItemsPanel);
			this.OtherInfoPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OtherInfoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.OtherInfoPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.OtherInfoPanel.Name = "OtherInfoPanel";
			this.OtherInfoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(732, 101, true);
			this.OtherInfoPanel.TabIndex = 2;
			// 
			// OtherItemsPanel
			// 
			this.OtherItemsPanel.AutoSize = true;
			this.OtherItemsPanel.ColumnCount = 1;
			this.OtherItemsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.OtherItemsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OtherItemsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OtherItemsPanel.Name = "OtherItemsPanel";
			this.OtherItemsPanel.RowCount = 1;
			this.OtherItemsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.OtherItemsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(732, 0, true);
			this.OtherItemsPanel.TabIndex = 0;
			// 
			// GenericMatchShowMoreUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.OtherInfoBorderPanel);
			this.Controls.Add(this.ShowMoreLessLabel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "GenericMatchShowMoreUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 129, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OtherInfoBorderPanel.ResumeLayout(false);
			this.OtherInfoBorderPanel.PerformLayout();
			this.OtherInfoPanel.ResumeLayout(false);
			this.OtherInfoPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZLinkLabel ShowMoreLessLabel;
		private ZArchitecture.GUI.ZPanel OtherInfoPanel;
		private ZArchitecture.GUI.ZPanel OtherInfoBorderPanel;
		private CargoWise.Windows.UI.KTableLayoutPanel OtherItemsPanel;
	}
}
