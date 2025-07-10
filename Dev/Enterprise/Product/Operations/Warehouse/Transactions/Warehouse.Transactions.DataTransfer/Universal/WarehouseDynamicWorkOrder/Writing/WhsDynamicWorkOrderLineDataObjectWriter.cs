using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsDynamicWorkOrderLineDataObjectWriter : WhsDocketLineDataObjectWriter<WhsDynamicWorkOrderLine>
	{
		public WhsDynamicWorkOrderLineDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void PopulateDataObject(OrderLine docketLineDataObject, WhsDynamicWorkOrderLine whsDocketLineBO)
		{
			if (whsDocketLineBO.DynamicWorkOrder.IsAssembly && whsDocketLineBO.WE_WE_ParentDocketLine.IsEmpty)
			{
				docketLineDataObject.SetOrderLineCollection(() => ProcessCollection(whsDocketLineBO.ChildComponentLinesCollection, new WhsDynamicWorkOrderLineDataObjectWriter(writeManager)));
			}
		}

		protected override void PopulateCustomsDataCore(WhsDynamicWorkOrderLine docketLineBO, OrderLine docketLineDataObject)
		{
			var writer = new WhsDynamicWorkOrderBondedWarehouseAttributeDataObjectWriter(writeManager, docketLineBO);
			docketLineDataObject.CustomsData = writer.GetDataObject(docketLineBO.CustomsData);
		}
	}
}
