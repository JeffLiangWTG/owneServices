
namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class NatureAndQtyOfGoodsLithiumBatteryControl
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
			this.LithiumBatteryTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LithiumBatteryTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.AWB.Business.ExportAWBRateLine);
			// 
			// volumeCalcDropEdit
			// 
			this.LithiumBatteryTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LithiumBatteryTypeDropEdit, "NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.AWB.Business.ExportAWBRateLine)(null)).NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType)));
			this.LithiumBatteryTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LithiumBatteryTypeDropEdit.Name = "lithiumBatteryTypeDropEdit";
			this.LithiumBatteryTypeDropEdit.ShowDescriptionBox = true;
			this.LithiumBatteryTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.LithiumBatteryTypeDropEdit.TabIndex = 1;
			// 
			// NatureAndQtyOfGoodsLithiumBatteryControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LithiumBatteryTypeDropEdit);
			this.Name = "NatureAndQtyOfGoodsLithiumBatteryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LithiumBatteryTypeDropEdit.ResumeLayout(true);
			this.LithiumBatteryTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZArchitecture.GUI.ZDropEdit LithiumBatteryTypeDropEdit;
	}
}