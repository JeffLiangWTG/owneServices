using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class CrossDockedInventoryToOrderLineAttacher : ZRecordAttacher
	{
		public CrossDockedInventoryToOrderLineAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, WhsOrderLine orderLine)
			: base(destinationCollection, findBoxList, moduleID)
		{
			UnableToAttachText = Res.GetString("7288dd41-f280-4dea-8437-2b54fb412465",
@"because of one of the following reasons:

1. The order is finalized or canceled.
2. The receipt line is already attached.
3. The receipt line quantity is already allocated.
4. The receipt line is damaged.
5. There is a mismatch on warehouse, client or product.
6. There is a mismatch on a part attribute, expiry date, packing date or ordered pallet id.
7. The inventory status is In-Transit.");
			OrderLine = orderLine;
		}

		WhsPickLineCollection DestPickLineCollection => (WhsPickLineCollection)DestinationCollection;

		BusinessObjectFactory Factory => DestinationCollection.Factory;

		protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			var pk = bizO.PK;
			var result = false;

			if (OrderLine != null
				&& !IsInventoryAlreadySelected(pk))
			{
				result = ReserveStockIfAbleTo(pk);
			}

			return result;
		}

		bool ReserveStockIfAbleTo(ZGuid pk)
		{
			var inventory = Factory.Load<WhsInventoryView>(pk);
			return OrderLine.ReserveStockIfAbleTo(inventory) != null;
		}

		bool IsInventoryAlreadySelected(ZGuid pk)
		{
			return DestPickLineCollection.Any(p => p.WZ_WE_InventoryLine == pk);
		}

		readonly WhsOrderLine OrderLine;
	}
}
