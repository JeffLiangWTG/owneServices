using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class DuplicateAlertControl
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

			if (ResultsListPanel != null)
			{
				ResultsListPanel.Dispose();
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
			this.ResultsListPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.HeaderLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DisplayDetailsAndFixesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.TopPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// ResultsListPanel
			// 
			this.ResultsListPanel.AutoSize = true;
			this.ResultsListPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResultsListPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ResultsListPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ResultsListPanel.Name = "ResultsListPanel";
			this.ResultsListPanel.TabIndex = 2;
			// 
			// HeaderLabel
			// 
			this.HeaderLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("835a5c11-738a-4638-89f2-9026c51abc97", "Potential Duplicates");
			this.HeaderLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.HeaderLabel.IsFontBold = true;
			this.HeaderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.HeaderLabel.Name = "HeaderLabel";
			this.HeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 22, true);
			// 
			// DisplayDetailsAndFixesButton
			// 
			this.DisplayDetailsAndFixesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DisplayDetailsAndFixesButton.BackColor = System.Drawing.Color.Transparent;
			this.DisplayDetailsAndFixesButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ca0828a6-7b99-4ff6-a5c5-fcd8f6e1abf1", "View Matches");
			this.DisplayDetailsAndFixesButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DisplayDetailsAndFixesButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.DisplayDetailsAndFixesButton.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.Warning1616;
			this.DisplayDetailsAndFixesButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.DisplayDetailsAndFixesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 163, true);
			this.DisplayDetailsAndFixesButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.DisplayDetailsAndFixesButton.Name = "DisplayDetailsAndFixesButton";
			this.DisplayDetailsAndFixesButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DisplayDetailsAndFixesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 24, true);
			this.DisplayDetailsAndFixesButton.TabIndex = 3;
			this.DisplayDetailsAndFixesButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.DisplayDetailsAndFixesButton.ToolTipCaption = null;
			this.DisplayDetailsAndFixesButton.UseVisualStyleBackColor = false;
			// 
			// CloseButton
			// 
			this.CloseButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CloseButton.FlatAppearance.BorderSize = 0;
			this.CloseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
			this.CloseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Firebrick;
			this.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.CloseButton.Font = new System.Drawing.Font("Comic Sans MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CloseButton.ForeColor = System.Drawing.Color.DimGray;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(233, 1, true);
			this.CloseButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 22, true);
			this.CloseButton.TabIndex = 1;
			this.CloseButton.ToolTipCaption = ResString.GetMultilingualString("4355499f-0adc-48f3-a8f0-1e57b81aec49", "Close");
			this.CloseButton.Text = "X";
			this.CloseButton.UseCompatibleTextRendering = true;
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// MainPanel
			// 
			this.MainPanel.ColumnCount = 1;
			this.MainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainPanel.Controls.Add(this.TopPanel, 0, 0);
			this.MainPanel.Controls.Add(this.DisplayDetailsAndFixesButton, 0, 2);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.RowCount = 3;
			this.MainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(24)));
			this.MainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(26)));
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 188, true);
			// 
			// TopPanel
			// 
			this.TopPanel.ColumnCount = 2;
			this.TopPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TopPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
			this.TopPanel.Controls.Add(this.HeaderLabel, 0, 0);
			this.TopPanel.Controls.Add(this.CloseButton, 1, 0);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.RowCount = 1;
			this.TopPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 24, true);
			// 
			// DuplicateAlertControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.MainPanel);
			this.Name = "DuplicateAlertControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 188, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		public ZArchitecture.ZLabel HeaderLabel;
		internal ZArchitecture.GUI.ZButton DisplayDetailsAndFixesButton;
		private ZArchitecture.GUI.ZButton CloseButton;
		private CargoWise.Windows.UI.KTableLayoutPanel MainPanel;
		private CargoWise.Windows.UI.KTableLayoutPanel TopPanel;
		internal Enterprise.ZArchitecture.GUI.ZPanel ResultsListPanel;
	}
}
