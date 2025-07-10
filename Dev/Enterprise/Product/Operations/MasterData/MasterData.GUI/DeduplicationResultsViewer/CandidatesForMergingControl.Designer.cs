using System.Windows.Forms;

namespace Enterprise.MasterData.GUI
{
	partial class CandidatesForMergingControl
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
			this.ButtonTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.SplitButtonBorderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.IgnoreButton = new System.Windows.Forms.ToolStripSplitButton();
			this.ForMeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ForEveryoneMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RemoveIgnoreMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MergeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContentPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTableLayoutPanel.SuspendLayout();
			this.HeaderPanel.SuspendLayout();
			this.ButtonTableLayoutPanel.SuspendLayout();
			this.SplitButtonBorderPanel.SuspendLayout();
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
			this.MainTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MainTableLayoutPanel.Name = "MainTableLayoutPanel";
			this.MainTableLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.MainTableLayoutPanel.RowCount = 2;
			this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(30)));
			this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 73, true);
			this.MainTableLayoutPanel.TabIndex = 0;
			// 
			// HeaderPanel
			// 
			this.HeaderPanel.BackColor = System.Drawing.Color.WhiteSmoke;
			this.HeaderPanel.Controls.Add(this.ButtonTableLayoutPanel);
			this.HeaderPanel.Controls.Add(this.TitleLabel);
			this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.HeaderPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.HeaderPanel.Name = "HeaderPanel";
			this.HeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 30, true);
			this.HeaderPanel.TabIndex = 0;
			// 
			// ButtonTableLayoutPanel
			// 
			this.ButtonTableLayoutPanel.AutoSize = true;
			this.ButtonTableLayoutPanel.ColumnCount = 3;
			this.ButtonTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.ButtonTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.ButtonTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.ButtonTableLayoutPanel.Controls.Add(this.SplitButtonBorderPanel, 0, 0);
			this.ButtonTableLayoutPanel.Controls.Add(this.MergeButton, 1, 0);
			this.ButtonTableLayoutPanel.Controls.Add(this.SelectButton, 2, 0);
			this.ButtonTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.ButtonTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 0, true);
			this.ButtonTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.ButtonTableLayoutPanel.Name = "ButtonTableLayoutPanel";
			this.ButtonTableLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 4, 2, 4, true);
			this.ButtonTableLayoutPanel.RowCount = 1;
			this.ButtonTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ButtonTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 30, true);
			this.ButtonTableLayoutPanel.TabIndex = 3;
			// 
			// SplitButtonBorderPanel
			// 
			this.SplitButtonBorderPanel.AutoSize = true;
			this.SplitButtonBorderPanel.BackColor = System.Drawing.Color.Silver;
			this.SplitButtonBorderPanel.Controls.Add(this.ToolStrip);
			this.SplitButtonBorderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 4, true);
			this.SplitButtonBorderPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 0, 2, 0, true);
			this.SplitButtonBorderPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 22, true);
			this.SplitButtonBorderPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 22, true);
			this.SplitButtonBorderPanel.Name = "SplitButtonBorderPanel";
			this.SplitButtonBorderPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.SplitButtonBorderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 22, true);
			this.SplitButtonBorderPanel.TabIndex = 2;
			// 
			// ToolStrip
			// 
			this.ToolStrip.BackColor = System.Drawing.SystemColors.ControlLight;
			this.ToolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ToolStrip.GripMargin = new System.Windows.Forms.Padding(0);
			this.ToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.ToolStrip.ImageScalingSize = new System.Drawing.Size(40, 40);
			this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.IgnoreButton});
			this.ToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.ToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.ToolStrip.Name = "ToolStrip";
			this.ToolStrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 20, true);
			this.ToolStrip.TabIndex = 1;
			this.ToolStrip.Text = "zToolStrip1";
			// 
			// IgnoreButton
			// 
			this.IgnoreButton.BackColor = System.Drawing.SystemColors.ControlLight;
			this.IgnoreButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.IgnoreButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IgnoreButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ForMeMenuItem,
            this.ForEveryoneMenuItem,
            this.RemoveIgnoreMenuItem});
			this.IgnoreButton.Margin = new System.Windows.Forms.Padding(0);
			this.IgnoreButton.Name = "IgnoreButton";
			this.IgnoreButton.Size = new System.Drawing.Size(27, 40);
			this.IgnoreButton.ButtonClick += new System.EventHandler(this.ForMeMenuItem_Click);
			// 
			// ForMeMenuItem
			// 
			this.ForMeMenuItem.BackColor = System.Drawing.SystemColors.Control;
			this.ForMeMenuItem.Name = "ForMeMenuItem";
			this.ForMeMenuItem.Size = new System.Drawing.Size(133, 44);
			this.ForMeMenuItem.Click += new System.EventHandler(this.ForMeMenuItem_Click);
			// 
			// ForEveryoneMenuItem
			// 
			this.ForEveryoneMenuItem.BackColor = System.Drawing.SystemColors.Control;
			this.ForEveryoneMenuItem.Name = "ForEveryoneMenuItem";
			this.ForEveryoneMenuItem.Size = new System.Drawing.Size(133, 44);
			this.ForEveryoneMenuItem.Click += new System.EventHandler(this.ForEveryoneMenuItem_Click);
			// 
			// RemoveIgnoreMenuItem
			// 
			this.RemoveIgnoreMenuItem.Name = "RemoveIgnoreMenuItem";
			this.RemoveIgnoreMenuItem.Size = new System.Drawing.Size(133, 44);
			this.RemoveIgnoreMenuItem.Click += new System.EventHandler(this.RemoveIgnoreMenuItem_Click);
			// 
			// MergeButton
			// 
			this.MergeButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("C88AA785-239D-4420-8006-83BAB41D690A", "Merge Selected");
			this.MergeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 4, true);
			this.MergeButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 0, 2, 0, true);
			this.MergeButton.Name = "MergeButton";
			this.MergeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 22, true);
			this.MergeButton.TabIndex = 0;
			this.MergeButton.ToolTipCaption = null;
			this.MergeButton.UseVisualStyleBackColor = true;
			this.MergeButton.Click += new System.EventHandler(this.MergeButton_Click);
			// 
			// SelectButton
			// 
			this.SelectButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("f9c3638e-6fcc-452b-8ada-9c8817b60bc9", "Select All/Deselect All");
			this.SelectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 4, true);
			this.SelectButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 0, 2, 0, true);
			this.SelectButton.Name = "SelectButton";
			this.SelectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 22, true);
			this.SelectButton.TabIndex = 4;
			this.SelectButton.ToolTipCaption = null;
			this.SelectButton.UseVisualStyleBackColor = true;
			this.SelectButton.Click += new System.EventHandler(this.SelectButton_Click);
			// 
			// TitleLabel
			// 
			this.TitleLabel.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("F207F73C-8927-4024-84C7-77FEC1D2A691", "No Duplicates");
			this.TitleLabel.Dock = System.Windows.Forms.DockStyle.Left;
			this.TitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TitleLabel.IsFontBold = true;
			this.TitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TitleLabel.Name = "TitleLabel";
			this.TitleLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 0, 0, 0, true);
			this.TitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 30, true);
			this.TitleLabel.TabIndex = 0;
			this.TitleLabel.UseMnemonic = false;
			// 
			// ContentPanel
			// 
			this.ContentPanel.AutoScroll = true;
			this.ContentPanel.AutoSize = true;
			this.ContentPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ContentPanel.BackColor = System.Drawing.Color.White;
			this.ContentPanel.ColumnCount = 1;
			this.ContentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 32, true);
			this.ContentPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 1, 0, 0, true);
			this.ContentPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 40, true);
			this.ContentPanel.Name = "ContentPanel";
			this.ContentPanel.RowCount = 1;
			this.ContentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ContentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 40, true);
			this.ContentPanel.TabIndex = 1;
			// 
			// CandidatesForMergingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainTableLayoutPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "CandidatesForMergingControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 83, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTableLayoutPanel.ResumeLayout(false);
			this.MainTableLayoutPanel.PerformLayout();
			this.HeaderPanel.ResumeLayout(false);
			this.HeaderPanel.PerformLayout();
			this.ButtonTableLayoutPanel.ResumeLayout(false);
			this.ButtonTableLayoutPanel.PerformLayout();
			this.SplitButtonBorderPanel.ResumeLayout(false);
			this.SplitButtonBorderPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel MainTableLayoutPanel;
		private ZArchitecture.GUI.ZPanel HeaderPanel;
		internal ZArchitecture.ZLabel TitleLabel;
		internal CargoWise.Windows.UI.KTableLayoutPanel ContentPanel;
		private ZArchitecture.GUI.ZToolStrip ToolStrip;
		private System.Windows.Forms.ToolStripSplitButton IgnoreButton;
		private System.Windows.Forms.ToolStripMenuItem ForMeMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ForEveryoneMenuItem;
		private ZArchitecture.GUI.ZPanel SplitButtonBorderPanel;
		private ZArchitecture.GUI.ZButton MergeButton;
		private CargoWise.Windows.UI.KTableLayoutPanel ButtonTableLayoutPanel;
		private ZArchitecture.GUI.ZButton SelectButton;
		private System.Windows.Forms.ToolStripMenuItem RemoveIgnoreMenuItem;
	}
}
