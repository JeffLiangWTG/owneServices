using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.OceanCarrier.Module
{
	public sealed partial class CarrierShipmentHeaderFilterControl : ZFilterStripControl
	{
		public CarrierShipmentHeaderFilterControl(IBusinessObjectCollection gridCollection, CarrierShipmentHeaderFilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
