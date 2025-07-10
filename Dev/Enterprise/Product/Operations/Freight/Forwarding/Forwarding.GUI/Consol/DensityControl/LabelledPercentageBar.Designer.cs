using System.Drawing;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class LabelledPercentageBar
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
			ProgressBar = new CargoWise.Windows.UI.KProgressBar();
			ProgressValue = new ZLabel();

			ProgressBar.Name = "ProgressBar";
			ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);

			ProgressValue.Name = "ProgressValue";
			ProgressValue.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 0, true);
			ProgressValue.TextAlign = ContentAlignment.MiddleLeft;
			ProgressValue.Font = new Font("Tacoma", 6.6f);
			ProgressValue.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 17, true);
			LabelCaptionRenderProvider.SetLabelCaptionVisible(ProgressValue, false);

			components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CaptionRenderingEnabled = true;

			Controls.Add(ProgressBar);
			Controls.Add(ProgressValue);

			Name = "LabelledPercentageBar";
			Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
		}

		CargoWise.Windows.UI.KProgressBar ProgressBar;
		ZLabel ProgressValue;

		#endregion
	}
}
