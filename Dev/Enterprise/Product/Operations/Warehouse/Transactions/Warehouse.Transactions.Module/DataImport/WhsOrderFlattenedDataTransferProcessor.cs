using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WhsOrderFlattenedDataTransferProcessor : WhsDocketFlattenedDataTransferProcessor<WhsOrder>
	{
		public WhsOrderFlattenedDataTransferProcessor(ImportCollectionInfoImplForWhsDocketFlattened importCollectionInfo, WhsDocketCollection docketCollection)
			: base(importCollectionInfo, docketCollection)
		{
		}

		protected override void CreateHeaderCore(WhsOrder docket, WhsDocketFlattened flattenedRecord)
		{
			SetJobDocAddress((NoResString)"Consignee", flattenedRecord, docket.ConsigneeDocAddress); // Programmatic prefix for properties
		}
	}
}
