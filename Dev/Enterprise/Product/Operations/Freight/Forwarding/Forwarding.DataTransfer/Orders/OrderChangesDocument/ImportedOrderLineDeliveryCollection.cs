using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ImportedOrderLineDeliveryCollection : NonPersistentBusinessObjectCollection<ImportedOrderLineDelivery>
	{
		public ImportedOrderLineDeliveryCollection() { }

		public ImportedOrderLineDeliveryCollection(ImportedOrderCollection importedOrders)
		{
			foreach (ImportedOrder importedOrder in importedOrders)
			{
				foreach (ImportedOrderLine importedOrderLine in importedOrder.OrderLines)
				{
					AddRange(importedOrderLine.Deliveries);
				}
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("AddNew not supported");
		}
	}
}

