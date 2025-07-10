using System.Drawing;

namespace Enterprise.Rating.GUI
{
	partial class WiseRatesFilterStripControl
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
			this.searchResultsBox = new WiseRatesSearchResultsChartUserControl();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();

			this.BindingSource.SetBindingMember(this.ratesServiceViewPanel, ".");
			this.ratesServiceViewPanel.Name = "wiseRatesViewPanel";
			this.ratesServiceViewPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right) | System.Windows.Forms.AnchorStyles.Top));
			// 
			// searchResultsBox
			// 
			this.searchResultsBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.searchResultsBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("a37508bf-b45d-42ec-99c4-226976bcb858", "Search Results powered by Rates Service");
			this.searchResultsBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 1, true);
			this.searchResultsBox.Name = "searchResultsBox";
			this.searchResultsBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 130, true);
			this.searchResultsBox.TabIndex = 7;
			// 
			// FilterStripsPanel
			// 
			this.FilterStripsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 41, true);
			// 
			// WiseRatesFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "WiseRatesFilterControl";
			this.Controls.SetChildIndex(this.FilterStripsPanel, 0);
			this.Controls.SetChildIndex(this.AddStripButton, 0);
			this.Controls.SetChildIndex(this.ToolStripRecordsFoundLabel, 0);
			this.Controls.Add(this.searchResultsBox);
			this.CaptionRenderingEnabled = true;
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		WiseRatesViewPanel ratesServiceViewPanel;
		WiseRatesSearchResultsChartUserControl searchResultsBox;

		#endregion
	}
}
