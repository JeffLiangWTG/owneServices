using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	class WhsReceiveFlattenedDataTransferProcessorTest : WhsDocketFlattenedDataTransferProcessorTest<WhsReceive>
	{
		#region Implementation

		protected override WhsDocketCollection GetDocketCollection(BusinessObjectFactory factory, AdhocCollectionRelationship relationship)
		{
			return new WhsReceiveCollection(factory, relationship);
		}

		protected override WhsDocketFlattenedDataTransferProcessor<WhsReceive> GetProcessor(ImportCollectionInfoImplForWhsDocketFlattened info, WhsDocketCollection docketCollection)
		{
			return new WhsReceiveFlattenedDataTransferProcessor(info, docketCollection);
		}

		#endregion
	}
}
