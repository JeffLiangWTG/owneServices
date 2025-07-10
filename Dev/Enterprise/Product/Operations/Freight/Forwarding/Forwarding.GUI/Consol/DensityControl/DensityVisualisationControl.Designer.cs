using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class DensityVisualisationControl
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
			this.components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.Density);

			this.DensityPanel = new DensityPanel(this);
			this.DenseLabel = new ZLabel();
			this.MidLabel = new ZLabel();
			this.VolumetricLabel = new ZLabel();

			this.DensityPanel.Name = "DensityPanel";
			this.DensityPanel.TabIndex = 0;
			this.DensityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 19, true);

			this.DenseLabel.AutoSize = true;
			this.DenseLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DensityVisualisationControl|3fe2785f-2d7b-4ef3-af68-a2126b9c47aa", "(1:1) Dense");
			this.DenseLabel.Name = "DenseLabel";
			this.DenseLabel.TabIndex = 2;
			this.DenseLabel.Font = new Font("Tahoma", 6.3f);
			this.DenseLabel.Dock = System.Windows.Forms.DockStyle.Left;
			this.DenseLabel.Padding = new Padding(
				left: CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(1),
				top: CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(6),
				right: 0,
				bottom: 0);

			this.MidLabel.AutoSize = true;
			this.MidLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DensityVisualisationControl|f90f3dc6-1420-4eb7-ba46-9e38c0e876b2", "1:6");
			this.MidLabel.Name = "MidLabel";
			this.MidLabel.TabIndex = 3;
			this.MidLabel.Font = new Font("Tahoma", 6.3f);
			this.MidLabel.Top = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(6);

			this.VolumetricLabel.AutoSize = true;
			this.VolumetricLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DensityVisualisationControl|c6e0bd94-a7fa-434b-a4d9-afff85c87bb0", "Volumetric (1:12)");
			this.VolumetricLabel.Name = "VolumetricLabel";
			this.VolumetricLabel.TabIndex = 4;
			this.VolumetricLabel.Font = new Font("Tahoma", 6.3f);
			this.VolumetricLabel.Dock = System.Windows.Forms.DockStyle.Right;
			this.VolumetricLabel.Padding = new Padding(
				left: 0,
				top: CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(6),
				right: CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(1),
				bottom: 0);

			this.CaptionRenderingEnabled = true;

			this.Controls.Add(DensityPanel);
			this.Controls.Add(DenseLabel);
			this.Controls.Add(MidLabel);
			this.Controls.Add(VolumetricLabel);

			this.Height = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(40);
			this.Name = "DensityVisualisationControl";
		}

		#endregion

		ZLabel DenseLabel;
		ZLabel MidLabel;
		ZLabel VolumetricLabel;
		DensityPanel DensityPanel;
	}
}
