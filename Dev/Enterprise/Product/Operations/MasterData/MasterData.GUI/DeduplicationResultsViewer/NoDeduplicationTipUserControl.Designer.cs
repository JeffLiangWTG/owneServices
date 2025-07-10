using System;
using System.Drawing;

namespace Enterprise.MasterData.GUI
{
	partial class NoDeduplicationTipUserControl
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
			this.TipLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTableLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTableLayoutPanel
			// 
			this.MainTableLayoutPanel.BackColor = System.Drawing.Color.LightGray;
			this.MainTableLayoutPanel.ColumnCount = 1;
			this.MainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainTableLayoutPanel.Controls.Add(this.TipLabel, 0, 0);
			this.MainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.MainTableLayoutPanel.Name = "MainTableLayoutPanel";
			this.MainTableLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.MainTableLayoutPanel.RowCount = 1;
			this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 187, true);
			this.MainTableLayoutPanel.TabIndex = 1;
			// 
			// TipLabel
			// 
			this.TipLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.TipLabel.AutoSize = true;
			this.TipLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TipLabel.ForeColor = System.Drawing.SystemColors.Control;
			this.TipLabel.IsFontBold = true;
			this.TipLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 73, true);
			this.TipLabel.Name = "TipLabel";
			this.TipLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 41, true);
			this.TipLabel.TabIndex = 0;
			this.TipLabel.UseMnemonic = false;
			// 
			// NoDeduplicationTipUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainTableLayoutPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "NoDeduplicationTipUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 197, true);
			this.SizeChanged += new EventHandler(Layout_SizeChanged);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTableLayoutPanel.ResumeLayout(false);
			this.MainTableLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel MainTableLayoutPanel;
		internal ZArchitecture.ZLabel TipLabel;
	}
}
