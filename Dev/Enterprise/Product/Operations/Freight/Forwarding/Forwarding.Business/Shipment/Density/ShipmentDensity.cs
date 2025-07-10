using System;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ShipmentDensity : Density
	{
		public ShipmentDensity(ForwardingShipment parentShipment)
		{
			shipment = parentShipment;
			shipment.JS_Calc_ActualVolumeWeightInfo.ValueChanged += JS_Calc_ActualVolumeWeight_ValueChanged;

			RefreshAllValues();
		}

		#region Density Property Overrides

		protected override bool IsRefreshAllowed => !shipment.IsDeleted;

		protected override bool IsChargeableByWeight => shipment.IsShipmentChargeableByWeight;

		protected override ZDecimal TotalWeight => shipment.JS_ActualWeight;

		protected override ZDecimal TotalVolume => shipment.JS_ActualVolume;

		protected override ZDecimal CalculatedVolumeWeight => shipment.JS_Calc_ActualVolumeWeight;

		#endregion

		#region Implementation

		void JS_Calc_ActualVolumeWeight_ValueChanged(object sender, EventArgs e)
		{
			RefreshAllValues();
		}

		readonly ForwardingShipment shipment;

		#endregion
	}
}
