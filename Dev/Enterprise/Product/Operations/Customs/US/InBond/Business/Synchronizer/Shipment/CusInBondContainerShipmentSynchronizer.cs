using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondContainerShipmentSynchroniser : Customs.Business.CusInBondContainerSynchroniser
	{
		public CusInBondContainerShipmentSynchroniser(CusInBondContainer destination, ForwardingContainer source, ForwardingShipment shipmentSource)
			: base(destination, source, shipmentSource)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new Customs.Business.FieldSynchroniser(Destination.BC_RCInfo, Source.JC_RCInfo));
		}
	}
}
