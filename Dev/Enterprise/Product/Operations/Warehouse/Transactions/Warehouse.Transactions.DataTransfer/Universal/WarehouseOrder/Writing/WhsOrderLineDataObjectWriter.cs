using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsOrderLineDataObjectWriter : WhsPickableDocketLineDataObjectWriter<WhsOrderLine>
	{
		public WhsOrderLineDataObjectWriter(IDataWritingManager manager, Dictionary<ZGuid, ZInt> linksDictionary)
			: base(manager)
		{
			LinksDictionary = Argument.NotNull(linksDictionary, nameof(linksDictionary));
		}

		#region LinksDictionary

		public Dictionary<ZGuid, ZInt> LinksDictionary { get; }
		ZInt orderCount = 0;

		#endregion

		protected override void PopulateDataObjectCore(OrderLine orderLineDataObject, WhsOrderLine orderLineBO)
		{
			foreach (var releaseLine in orderLineBO.ReleaseLines)
			{
				LinksDictionary.Add(releaseLine.PK, orderCount);
			}

			orderLineDataObject.Link = orderCount;
			orderCount++;

			orderLineDataObject.ReservedQuantity = orderLineBO.WE_CrossDockQuantity;
			orderLineDataObject.ShortfallQuantity = orderLineBO.WE_ShortfallQuantityCached;
			orderLineDataObject.PalletID = orderLineBO.WE_PalletID;
			if (!orderLineBO.WE_WHC_NKOrderedHeldCode.IsEmpty)
			{
				orderLineDataObject.CurrentHoldCode = ListHelper.GetWithDescription<CodeDescriptionPair9Char>(orderLineBO.WE_WHC_NKOrderedHeldCode, orderLineBO.Lookups.InventoryHeldCodeCollection);
			}
		}

		protected override void PopulateCustomsDataCore(WhsOrderLine docketLineBO, OrderLine docketLineDataObject)
		{
			var writer = new WhsOrderBondedWarehouseAttributeDataObjectWriter(writeManager, docketLineBO);
			docketLineDataObject.CustomsData = writer.GetDataObject(docketLineBO.CustomsData);
		}
	}
}
