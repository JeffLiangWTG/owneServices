using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	partial class PersonMergePreviewUserControl
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
			this.MainPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.DiscardedPropertiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RightBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RightBottomLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.MergedPropertiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LeftBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LeftBottomLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.DiscardedPropertiesGroupBox.SuspendLayout();
			this.RightBottomPanel.SuspendLayout();
			this.MergedPropertiesGroupBox.SuspendLayout();
			this.LeftBottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.ColumnCount = 2;
			this.MainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.MainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.MainPanel.Controls.Add(this.DiscardedPropertiesGroupBox, 1, 0);
			this.MainPanel.Controls.Add(this.MergedPropertiesGroupBox, 0, 0);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.RowCount = 1;
			this.MainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 217, true);
			this.MainPanel.TabIndex = 0;
			// 
			// DiscardedValuesGroupBox
			// 
			this.DiscardedPropertiesGroupBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("0e1b8a97-a859-4547-9dc6-22f9dfe6f5ac", "Discarded Values :");
			this.DiscardedPropertiesGroupBox.Controls.Add(this.RightBottomPanel);
			this.DiscardedPropertiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DiscardedPropertiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(490, 3, true);
			this.DiscardedPropertiesGroupBox.Name = "DiscardedPropertiesGroupBox";
			this.DiscardedPropertiesGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 12, 3, 3, true);
			this.DiscardedPropertiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 211, true);
			this.DiscardedPropertiesGroupBox.TabIndex = 2;
			this.DiscardedPropertiesGroupBox.TabStop = false;
			// 
			// RightBottomPanel
			// 
			this.RightBottomPanel.AutoScroll = true;
			this.RightBottomPanel.Controls.Add(this.RightBottomLayoutPanel);
			this.RightBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 25, true);
			this.RightBottomPanel.Name = "RightBottomPanel";
			this.RightBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 183, true);
			this.RightBottomPanel.TabIndex = 2;
			// 
			// RightBottomLayoutPanel
			// 
			this.RightBottomLayoutPanel.AutoSize = true;
			this.RightBottomLayoutPanel.ColumnCount = 1;
			this.RightBottomLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.RightBottomLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.RightBottomLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RightBottomLayoutPanel.Name = "RightBottomLayoutPanel";
			this.RightBottomLayoutPanel.RowCount = 1;
			this.RightBottomLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(25)));
			this.RightBottomLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 25, true);
			this.RightBottomLayoutPanel.TabIndex = 3;
			// 
			// RetainedPersonGroupBox
			// 
			this.MergedPropertiesGroupBox.Controls.Add(this.LeftBottomPanel);
			this.MergedPropertiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MergedPropertiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MergedPropertiesGroupBox.Name = "MergedPropertiesGroupBox";
			this.MergedPropertiesGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 12, 3, 3, true);
			this.MergedPropertiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 211, true);
			this.MergedPropertiesGroupBox.TabIndex = 1;
			this.MergedPropertiesGroupBox.TabStop = false;
			this.MergedPropertiesGroupBox.Text = "After Merge Values :";
			// 
			// LeftBottomPanel
			// 
			this.LeftBottomPanel.AutoScroll = true;
			this.LeftBottomPanel.Controls.Add(this.LeftBottomLayoutPanel);
			this.LeftBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LeftBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 25, true);
			this.LeftBottomPanel.Name = "LeftBottomPanel";
			this.LeftBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 183, true);
			this.LeftBottomPanel.TabIndex = 2;
			// 
			// LeftBottomLayoutPanel
			// 
			this.LeftBottomLayoutPanel.AutoSize = true;
			this.LeftBottomLayoutPanel.ColumnCount = 1;
			this.LeftBottomLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.LeftBottomLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.LeftBottomLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftBottomLayoutPanel.Name = "LeftBottomLayoutPanel";
			this.LeftBottomLayoutPanel.RowCount = 1;
			this.LeftBottomLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.LeftBottomLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 25, true);
			this.LeftBottomLayoutPanel.TabIndex = 2;
			// 
			// PersonMergePreviewUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainPanel);
			this.Name = "PersonMergePreviewUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 217, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.DiscardedPropertiesGroupBox.ResumeLayout(false);
			this.DiscardedPropertiesGroupBox.PerformLayout();
			this.RightBottomPanel.ResumeLayout(false);
			this.RightBottomPanel.PerformLayout();
			this.MergedPropertiesGroupBox.ResumeLayout(false);
			this.MergedPropertiesGroupBox.PerformLayout();
			this.LeftBottomPanel.ResumeLayout(false);
			this.LeftBottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private KTableLayoutPanel MainPanel;
		protected KTableLayoutPanel LeftBottomLayoutPanel;
		protected ZPanel LeftBottomPanel;
		protected ZPanel RightBottomPanel;
		protected KTableLayoutPanel RightBottomLayoutPanel;
		protected ZGroupBox MergedPropertiesGroupBox;
		protected ZGroupBox DiscardedPropertiesGroupBox;
	}
}
