using System.Drawing;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class DensityCube
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
			this.DensityFactorLabel = new Enterprise.ZArchitecture.ZLabel();

			this.DensityFactorLabel.ForeColor = System.Drawing.Color.White;
			this.DensityFactorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.DensityFactorLabel.Name = "DensityFactorLabel";
			this.DensityFactorLabel.TextAlign = ContentAlignment.TopCenter;
			this.DensityFactorLabel.Font = new Font("Tahoma", 6.3f);
			this.DensityFactorLabel.Height = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(16);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(DensityFactorLabel, false);

			this.components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CaptionRenderingEnabled = true;

			this.Height = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(19);
			this.Top = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(0);
			this.Controls.Add(DensityFactorLabel);
			this.Name = "DensityCube";
		}

		public Enterprise.ZArchitecture.ZLabel DensityFactorLabel;

		#endregion
	}
}
