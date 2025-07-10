using CargoWise.Windows.UI;

namespace Enterprise.MasterData.GUI
{
	partial class GenShapeGeographyImportProgressForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.ProgressLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.ProgressGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProgressInfoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ProgressLayoutPanel.SuspendLayout();
			this.ProgressGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 205, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 24, true);
			// 
			// ProgressLayoutPanel
			// 
			this.ProgressLayoutPanel.ColumnCount = 1;
			this.ProgressLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.ProgressLayoutPanel.Controls.Add(this.ProgressGroupBox, 0, 0);
			this.ProgressLayoutPanel.Controls.Add(this.CloseButton, 0, 1);
			this.ProgressLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProgressLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProgressLayoutPanel.Name = "ProgressLayoutPanel";
			this.ProgressLayoutPanel.RowCount = 2;
			this.ProgressLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ProgressLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(30)));
			this.ProgressLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 205, true);
			this.ProgressLayoutPanel.TabIndex = 1;
			// 
			// ProgressGroupBox
			// 
			this.ProgressGroupBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("f5e33973-f10c-4ad0-b881-baf16d75d6a0", "Import Progress Info");
			this.ProgressGroupBox.Controls.Add(this.ProgressInfoTextBox);
			this.ProgressGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProgressGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ProgressGroupBox.Name = "ProgressGroupBox";
			this.ProgressGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(541, 171, true);
			this.ProgressGroupBox.TabIndex = 0;
			this.ProgressGroupBox.TabStop = false;
			// 
			// ProgressInfoTextBox
			// 
			this.ProgressInfoTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.ProgressInfoTextBox.CaptionResourceString = null;
			this.ProgressInfoTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ProgressInfoTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProgressInfoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ProgressInfoTextBox.Multiline = true;
			this.ProgressInfoTextBox.Name = "ProgressInfoTextBox";
			this.ProgressInfoTextBox.ReadOnly = true;
			this.ProgressInfoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 155, true);
			this.ProgressInfoTextBox.TabIndex = 0;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("17ec529f-fa56-44b8-a1b6-6a1fb70d5558", "Close");
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(469, 177, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 1;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// GenShapeGeographyImportProgressForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("5ec6cd28-1b25-434b-85c8-ea6cba94d79e", "Shape Geography Import Progress Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 229, true);
			this.Controls.Add(this.ProgressLayoutPanel);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 267, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 267, true);
			this.Name = "GenShapeGeographyImportProgressForm";
			this.Text = "Shape Geography Import Progress Form";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ProgressLayoutPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ProgressLayoutPanel.ResumeLayout(false);
			this.ProgressLayoutPanel.PerformLayout();
			this.ProgressGroupBox.ResumeLayout(false);
			this.ProgressGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private KTableLayoutPanel ProgressLayoutPanel;
		private ZArchitecture.GUI.ZGroupBox ProgressGroupBox;
		protected ZArchitecture.ZTextBox ProgressInfoTextBox;
		private ZArchitecture.GUI.ZButton CloseButton;
	}
}