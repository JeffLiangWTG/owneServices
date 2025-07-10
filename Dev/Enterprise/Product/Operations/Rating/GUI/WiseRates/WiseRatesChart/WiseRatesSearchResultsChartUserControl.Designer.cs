using System.Drawing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI
{
	partial class WiseRatesSearchResultsChartUserControl
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
            this.plotPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblSearchResults = new Enterprise.ZArchitecture.ZLabel();
            this.rowsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblErrorsWarnings = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
            this.lblRawData = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(WiseRates.Api.Model.RatesSearchResponse);
            // 
            // plotPanel
            // 
            this.plotPanel.BackColor = System.Drawing.Color.Transparent;
            this.plotPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 4, true);
            this.plotPanel.Name = "plotPanel";
            this.plotPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 134, true);
            this.plotPanel.TabIndex = 0;
            // 
            // lblSearchResults
            // 
            this.lblSearchResults.AutoSize = true;
            this.lblSearchResults.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("lblSearchResults", "Search Results");
            this.lblSearchResults.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
            this.lblSearchResults.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
            this.lblSearchResults.Name = "lblSearchResults";
            this.lblSearchResults.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 17, true);
            this.lblSearchResults.TabIndex = 1;
            this.lblSearchResults.Visible = false;
            // 
            // rowsPanel
            // 
            this.rowsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 25, true);
            this.rowsPanel.Name = "rowsPanel";
            this.rowsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 79, true);
            this.rowsPanel.TabIndex = 2;
            // 
            // lblErrorsWarnings
            // 
            this.lblErrorsWarnings.AutoSize = true;
            this.lblErrorsWarnings.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("lblErrorsWarnings", " ");
            this.lblErrorsWarnings.IsFontBold = false;
            this.lblErrorsWarnings.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 108, true);
            this.lblErrorsWarnings.Name = "lblErrorsWarnings";
            this.lblErrorsWarnings.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 13, true);
            this.lblErrorsWarnings.TabIndex = 3;
            this.lblErrorsWarnings.Visible = false;
            // 
            // lblRawData
            // 
            this.lblRawData.AutoSize = true;
            this.lblRawData.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("lblRawData", " ");
            this.lblRawData.IsFontBold = false;
            this.lblRawData.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 125, true);
            this.lblRawData.Name = "lblRawData";
            this.lblRawData.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 13, true);
            this.lblRawData.TabIndex = 4;
            this.lblRawData.Visible = false;
            // 
            // WiseRatesSearchResultsChartUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.lblRawData);
            this.Controls.Add(this.lblErrorsWarnings);
            this.Controls.Add(this.rowsPanel);
            this.Controls.Add(this.lblSearchResults);
            this.Controls.Add(this.plotPanel);
            this.Name = "WiseRatesSearchResultsChartUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 143, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZPanel plotPanel;
		private Enterprise.ZArchitecture.ZLabel lblSearchResults;
		private Enterprise.ZArchitecture.GUI.ZPanel rowsPanel;
		private Enterprise.ZArchitecture.GUI.ZLinkLabel lblErrorsWarnings;
		private Enterprise.ZArchitecture.GUI.ZLinkLabel lblRawData;

		#endregion

		//private CargoWise.Windows.UI.KElementHost PieChartControlHost;
	}
}
