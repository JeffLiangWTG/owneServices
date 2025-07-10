using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.OceanCarrier.Module
{
	public sealed partial class CarrierShipmentHeaderFilterControl : ZFilterStripControl
    {
		void InitializeComponent ()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo carrierShipmentReferenceColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo houseBillColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// grid
			//
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CarrierShipmentHeader)(null)).CSH_CarrierShipmentReference);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CarrierShipmentHeader)(null)).CSH_HouseBill);
			carrierShipmentReferenceColumnStyleInfo.ColumnName = "CSH_CarrierShipmentReference";
			houseBillColumnStyleInfo.ColumnName = "CSH_HouseBill";
			houseBillColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.grid.ColumnStyles.Add(carrierShipmentReferenceColumnStyleInfo);
			this.grid.ColumnStyles.Add(houseBillColumnStyleInfo);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Business.CarrierShipmentHeader);
			//
			// CampaignFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "CarrierShipmentHeaderFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}
