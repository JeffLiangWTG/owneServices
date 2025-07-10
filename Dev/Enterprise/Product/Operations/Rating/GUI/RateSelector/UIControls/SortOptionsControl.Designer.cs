using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	partial class SortOptionsControl
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
            this.buttonsFlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
            this.sortOptionsLabel = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.buttonsFlowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.SortableRatesViewModel);
            // 
            // buttonsFlowLayoutPanel
            // 
            this.buttonsFlowLayoutPanel.AutoSize = true;
            this.buttonsFlowLayoutPanel.Controls.Add(this.sortOptionsLabel);
            this.buttonsFlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.buttonsFlowLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.buttonsFlowLayoutPanel.Name = "buttonsFlowLayoutPanel";
            this.buttonsFlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 29, true);
            this.buttonsFlowLayoutPanel.TabIndex = 1;
            // 
            // sortOptionsLabel
            // 
            this.sortOptionsLabel.AutoSize = true;
            this.sortOptionsLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("02e38664-fa94-4ffb-82f9-587fd2aaf1b4", "Sort Options");
            this.sortOptionsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.sortOptionsLabel.ForeColor = System.Drawing.Color.White;
            this.sortOptionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.sortOptionsLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.sortOptionsLabel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 23, true);
            this.sortOptionsLabel.Name = "sortOptionsLabel";
            this.sortOptionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 23, true);
            this.sortOptionsLabel.TabIndex = 2;
            this.sortOptionsLabel.UseMnemonic = false;
            // 
            // SortOptionsControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.buttonsFlowLayoutPanel);
            this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.Name = "SortOptionsControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 29, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.buttonsFlowLayoutPanel.ResumeLayout(false);
            this.buttonsFlowLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KFlowLayoutPanel buttonsFlowLayoutPanel;
		private ZArchitecture.ZLabel sortOptionsLabel;
	}
}
