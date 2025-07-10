namespace Enterprise.Freight.Agency.Business
{
	using CargoWise.Types;
	using Enterprise.Freight.Business;

	class BillOfLadingTransportSupporter : AgencyShipmentTransportSupporter<BillOfLading>
	{
		public BillOfLadingTransportSupporter(BillOfLading shipment)
			: base(shipment) { }

		protected override void NotifySailingChangedCore(Transport transport, ZGuid previousValue)
		{
			base.NotifySailingChangedCore(transport, previousValue);
			if (transport.IsPersistent)
			{
				Parent.TransportSailingHasChanges = true;
			}
		}
	}
}
