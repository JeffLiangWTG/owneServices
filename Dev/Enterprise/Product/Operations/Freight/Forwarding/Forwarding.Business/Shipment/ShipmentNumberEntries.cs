using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ShipmentNumberEntries : NonPersistentBusinessObjectCollection<ShipmentNumberEntry>
	{
		public ShipmentNumberEntries(ForwardingConsol consol, BusinessObjectFactory factory)
			: base(factory)
		{
			this.consol = consol;
		}
		readonly ForwardingConsol consol;

		public override void Load()
		{
			RemoveAndDeleteAll();

			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				ShipmentNumberEntry entry = new ShipmentNumberEntry(shipment);
				entry.AllowBlankShipmentNumber();
				Add(entry);
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("AddNew not supported");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
