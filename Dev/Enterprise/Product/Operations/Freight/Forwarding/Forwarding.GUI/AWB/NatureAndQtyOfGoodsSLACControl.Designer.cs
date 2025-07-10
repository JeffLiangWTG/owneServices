using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class NatureAndQtyOfGoodsSLACControl
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
			this.countCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.AWB.Business.ExportAWBRateLine);
			// 
			// countCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.countCalcEdit, "NatureAndQtyOfGoodsSLAC.Count");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(null)).NatureAndQtyOfGoodsSLAC.Count)));
			this.countCalcEdit.CaptionResourceString = Res.GetData("8174c4ae-5e5d-4135-8ab3-1256d3746519", "Shipper's Load and Count");
			this.countCalcEdit.DecimalPlaces = 0;
			this.countCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.countCalcEdit, false);
			this.countCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.countCalcEdit.MaxLength = 5;
			this.countCalcEdit.Name = "countCalcEdit";
			this.countCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.countCalcEdit.TabIndex = 0;
			this.countCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// NatureAndQtyOfGoodsTextControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.countCalcEdit);
			this.Name = "NatureAndQtyOfGoodsNumericControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.ZCalcEdit countCalcEdit;
	}
}
