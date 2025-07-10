namespace Enterprise.MarketingManager.GUI
{
	partial class LegacyValueStripControl
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
			this.mainTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.topLeftPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.deleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.bottomRightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.topRightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.salesProductNameLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.mainTableLayoutPanel.SuspendLayout();
			this.topLeftPanel.SuspendLayout();
			this.topRightPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgOpportunity);
			// 
			// mainTableLayoutPanel
			// 
			this.mainTableLayoutPanel.AutoSize = true;
			this.mainTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.mainTableLayoutPanel.ColumnCount = 2;
			this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.mainTableLayoutPanel.Controls.Add(this.topLeftPanel, 0, 0);
			this.mainTableLayoutPanel.Controls.Add(this.bottomRightPanel, 1, 1);
			this.mainTableLayoutPanel.Controls.Add(this.topRightPanel, 1, 0);
			this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
			this.mainTableLayoutPanel.RowCount = 2;
			this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.mainTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 29, true);
			this.mainTableLayoutPanel.TabIndex = 0;
			// 
			// topLeftPanel
			// 
			this.topLeftPanel.Controls.Add(this.deleteButton);
			this.topLeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.topLeftPanel.Name = "topLeftPanel";
			this.topLeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 23, true);
			this.topLeftPanel.TabIndex = 1;
			// 
			// deleteButton
			// 
			this.deleteButton.BackColor = System.Drawing.Color.Transparent;
			this.deleteButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.deleteButton.FlatAppearance.BorderSize = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.deleteButton, false);
			this.deleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 18, true);
			this.deleteButton.TabIndex = 0;
			this.deleteButton.TabStop = false;
			this.deleteButton.UseVisualStyleBackColor = false;
			this.deleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
			// 
			// bottomRightPanel
			// 
			this.bottomRightPanel.AutoSize = true;
			this.bottomRightPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.bottomRightPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.bottomRightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 28, true);
			this.bottomRightPanel.Name = "bottomRightPanel";
			this.bottomRightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 0, true);
			this.bottomRightPanel.TabIndex = 1;
			// 
			// topRightPanel
			// 
			this.topRightPanel.Controls.Add(this.salesProductNameLabel);
			this.topRightPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topRightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 2, true);
			this.topRightPanel.Name = "topRightPanel";
			this.topRightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 26, true);
			this.topRightPanel.TabIndex = 0;
			// 
			// salesProductNameLabel
			// 
			this.salesProductNameLabel.AllowDrop = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.salesProductNameLabel, false);
			this.salesProductNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.salesProductNameLabel.Name = "salesProductNameLabel";
			this.salesProductNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 26, true);
			this.salesProductNameLabel.TabIndex = 0;
			this.salesProductNameLabel.UseMnemonic = false;
			// 
			// OpportunitySalesHeaderStripControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainTableLayoutPanel);
			this.Name = "OpportunitySalesHeaderStripControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 2, 2, 15, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 46, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainTableLayoutPanel.ResumeLayout(false);
			this.mainTableLayoutPanel.PerformLayout();
			this.topLeftPanel.ResumeLayout(false);
			this.topLeftPanel.PerformLayout();
			this.topRightPanel.ResumeLayout(false);
			this.topRightPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel mainTableLayoutPanel;
		private Enterprise.ZArchitecture.ZLabel salesProductNameLabel;
		private Enterprise.ZArchitecture.GUI.ZPanel bottomRightPanel;
		private ZArchitecture.GUI.ZButton deleteButton;
		private ZArchitecture.GUI.ZPanel topRightPanel;
		private ZArchitecture.GUI.ZPanel topLeftPanel;

	}
}
