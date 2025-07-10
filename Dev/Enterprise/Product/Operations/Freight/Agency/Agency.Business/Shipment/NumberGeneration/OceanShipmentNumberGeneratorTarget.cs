using Enterprise.Freight.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class OceanShipmentNumberGeneratorTarget : ShipmentNumberGeneratorTarget
	{
		public override string NumberCustomisationLocation
		{
			get { return (NoResString)"Liner & Agency -> Shipment Number Customization"; } // Points to non-localised registry entry
		}

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(AgencyRegistry.Instance.OceanBillShipmentNumberCustomisation);
		}
	}
}
