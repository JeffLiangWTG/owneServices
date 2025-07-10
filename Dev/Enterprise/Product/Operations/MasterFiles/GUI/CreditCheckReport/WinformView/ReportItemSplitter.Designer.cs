namespace Enterprise.MasterFiles.GUI
{
	partial class ReportItemSplitter
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.splitterLineLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// splitterLineLabel
			// 
			this.splitterLineLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.splitterLineLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitterLineLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.splitterLineLabel, false);
			this.splitterLineLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitterLineLabel.Name = "splitterLineLabel";
			this.splitterLineLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 1, true);
			this.splitterLineLabel.TabIndex = 1;
			// 
			// ReportItemSplitter
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitterLineLabel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(13, 0, 13, 3, true);
			this.Name = "ReportItemSplitter";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 1, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel splitterLineLabel;
	}
}
