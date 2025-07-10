using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterData.GUI
{
	partial class PersonMergePreviewLabelUserControl
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
			this.HumanReadableNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MainTableLayout = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTableLayout.SuspendLayout();
			this.SuspendLayout();
			// 
			// HumanReadableNameLabel
			// 
			this.HumanReadableNameLabel.AutoSize = true;
			this.HumanReadableNameLabel.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("a4cd36c3-6f92-4194-a4f7-2b1288c174a2", "test");
			this.HumanReadableNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HumanReadableNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.HumanReadableNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.HumanReadableNameLabel.Name = "HumanReadableNameLabel";
			this.HumanReadableNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 25, true);
			this.HumanReadableNameLabel.TabIndex = 0;
			this.HumanReadableNameLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// ValueLabel
			// 
			this.ValueLabel.AutoSize = true;
			this.ValueLabel.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("ab4cf961-680c-477e-aba7-764383d9ec92", "Test");
			this.ValueLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 0, true);
			this.ValueLabel.Name = "ValueLabel";
			this.ValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 25, true);
			this.ValueLabel.TabIndex = 1;
			this.ValueLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// MainTableLayout
			// 
			this.MainTableLayout.ColumnCount = 2;
			this.MainTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.MainTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.MainTableLayout.Controls.Add(this.HumanReadableNameLabel, 0, 0);
			this.MainTableLayout.Controls.Add(this.ValueLabel, 1, 0);
			this.MainTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTableLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTableLayout.Name = "MainTableLayout";
			this.MainTableLayout.RowCount = 1;
			this.MainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainTableLayout.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 25, true);
			this.MainTableLayout.TabIndex = 2;
			// 
			// PersonMergePreviewLabelUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainTableLayout);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, 3, 3, 3, true);
			this.Name = "PersonMergePreviewLabelUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 25, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTableLayout.ResumeLayout(false);
			this.MainTableLayout.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZLabel HumanReadableNameLabel;
		public ZLabel ValueLabel;
		private KTableLayoutPanel MainTableLayout;
	}
}
