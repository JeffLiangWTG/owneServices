using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WhsReceiveFlattenedDataTransferProcessor : WhsDocketFlattenedDataTransferProcessor<WhsReceive>
	{
		public WhsReceiveFlattenedDataTransferProcessor(ImportCollectionInfoImplForWhsDocketFlattened importCollectionInfo, WhsDocketCollection docketCollection)
			: base(importCollectionInfo, docketCollection)
		{
		}

		protected override void CreateHeaderCore(WhsReceive docket, WhsDocketFlattened flattenedRecord)
		{
		}
	}
}
