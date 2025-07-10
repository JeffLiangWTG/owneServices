using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class NatureAndQtyOfGoodsVolumeControl
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

		private void InitializeComponent()
		{
			this.volumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.AWB.Business.ExportAWBRateLine);
			// 
			// volumeCalcDropEdit
			// 
			this.volumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.volumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.AWB.Business.IExportAWBRateLine)(null)).NatureAndQtyOfGoodsVolume.Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.AWB.Business.IExportAWBRateLine)(null)).NatureAndQtyOfGoodsVolume.Unit)));
			this.volumeCalcDropEdit.BindToAmount = "NatureAndQtyOfGoodsVolume.Volume";
			this.volumeCalcDropEdit.BindToUnit = "NatureAndQtyOfGoodsVolume.Unit";
			this.volumeCalcDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("53372170-a3f2-4e6e-8206-efaabb0be9e7", "Volume");
			this.volumeCalcDropEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.volumeCalcDropEdit, false);
			this.volumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.volumeCalcDropEdit.Name = "volumeCalcDropEdit";
			this.volumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.volumeCalcDropEdit.TabIndex = 0;
			this.volumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// NatureAndQtyOfGoodsVolumeControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.volumeCalcDropEdit);
			this.Name = "NatureAndQtyOfGoodsVolumeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		private ZArchitecture.GUI.ZCalcDropEdit volumeCalcDropEdit;
	}
}