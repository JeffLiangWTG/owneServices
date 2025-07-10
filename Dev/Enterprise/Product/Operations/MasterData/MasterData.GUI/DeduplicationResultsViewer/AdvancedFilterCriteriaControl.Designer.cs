namespace Enterprise.MasterData.GUI
{
	partial class AdvancedFilterCriteriaControl
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
			this.MainTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.HeaderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ExpandCollapseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContentPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTableLayoutPanel.SuspendLayout();
			this.HeaderPanel.SuspendLayout();
			this.ContentPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTableLayoutPanel
			// 
			this.MainTableLayoutPanel.AutoSize = true;
			this.MainTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MainTableLayoutPanel.BackColor = System.Drawing.Color.Silver;
			this.MainTableLayoutPanel.ColumnCount = 1;
			this.MainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainTableLayoutPanel.Controls.Add(this.HeaderPanel, 0, 0);
			this.MainTableLayoutPanel.Controls.Add(this.ContentPanel, 0, 1);
			this.MainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.MainTableLayoutPanel.Name = "MainTableLayoutPanel";
			this.MainTableLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.MainTableLayoutPanel.RowCount = 2;
			this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(30)));
			this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 122, true);
			this.MainTableLayoutPanel.TabIndex = 1;
			// 
			// HeaderPanel
			// 
			this.HeaderPanel.BackColor = System.Drawing.Color.WhiteSmoke;
			this.HeaderPanel.Controls.Add(this.ExpandCollapseButton);
			this.HeaderPanel.Controls.Add(this.TitleLabel);
			this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.HeaderPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.HeaderPanel.Name = "HeaderPanel";
			this.HeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 30, true);
			this.HeaderPanel.TabIndex = 0;
			// 
			// ExpandCollapseButton
			// 
			this.ExpandCollapseButton.AutoSize = true;
			this.ExpandCollapseButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.ExpandCollapseButton.BackColor = System.Drawing.Color.WhiteSmoke;
			this.ExpandCollapseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ExpandCollapseButton.FlatAppearance.BorderSize = 0;
			this.ExpandCollapseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
			this.ExpandCollapseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.WhiteSmoke;
			this.ExpandCollapseButton.ToolTipCaption = null;
			this.ExpandCollapseButton.UseVisualStyleBackColor = false;
			this.ExpandCollapseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(820, 0, true);
			this.ExpandCollapseButton.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 30, true);
			this.ExpandCollapseButton.Name = "ExpandCollapseButton";
			this.ExpandCollapseButton.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.ExpandCollapseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 30, true);
			this.ExpandCollapseButton.TabIndex = 1;
			this.ExpandCollapseButton.Text = "⯅ ";
			this.ExpandCollapseButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ExpandCollapseButton.Click += new System.EventHandler(this.ExpandCollapseButton_LinkClicked);
			// 
			// TitleLabel
			// 
			this.TitleLabel.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("B3CAC1A6-95BC-4D65-9C58-FFC3195C3621", "Advanced Filter Criteria");
			this.TitleLabel.Dock = System.Windows.Forms.DockStyle.Left;
			this.TitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TitleLabel.IsFontBold = true;
			this.TitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TitleLabel.Name = "TitleLabel";
			this.TitleLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 0, 0, 0, true);
			this.TitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 30, true);
			this.TitleLabel.TabIndex = 0;
			this.TitleLabel.UseMnemonic = false;
			// 
			// ContentPanel
			//
			this.ContentPanel.AutoScroll = true;
			this.ContentPanel.AutoSize = true;
			this.ContentPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ContentPanel.BackColor = System.Drawing.Color.White;
			this.ContentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.ContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 32, true);
			this.ContentPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 1, 0, 0, true);
			this.ContentPanel.Name = "ContentPanel";
			this.ContentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ContentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ContentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 89, true);
			this.ContentPanel.TabIndex = 1;
			// 
			// AdvancedFilterCriteriaControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainTableLayoutPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "AdvancedFilterCriteriaControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 132, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTableLayoutPanel.ResumeLayout(false);
			this.MainTableLayoutPanel.PerformLayout();
			this.HeaderPanel.ResumeLayout(false);
			this.HeaderPanel.PerformLayout();
			this.ContentPanel.ResumeLayout(false);
			this.ContentPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel MainTableLayoutPanel;
		private ZArchitecture.GUI.ZPanel HeaderPanel;
		private ZArchitecture.ZLabel TitleLabel;
		private CargoWise.Windows.UI.KTableLayoutPanel ContentPanel;
		private Enterprise.ZArchitecture.GUI.ZButton ExpandCollapseButton;
	}
}
