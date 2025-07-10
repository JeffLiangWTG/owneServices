using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	partial class CostBreakdownControl
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
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.BookingEngineRateViewModel);
            // 
            // CostBreakdownControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.CaptionRenderingEnabled = true;
            this.Name = "CostBreakdownControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 14, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion
	}
}
