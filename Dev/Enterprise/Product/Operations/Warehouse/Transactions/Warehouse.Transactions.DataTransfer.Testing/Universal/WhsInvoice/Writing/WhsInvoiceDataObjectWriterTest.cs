using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.DataTransfer.Universal;
using Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal
{
	class WhsInvoiceDataObjectWriterTest : WhsUniversalTestCase
	{
		public void TestGetEDIMessageSubType()
		{
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalShipment, ((ITopLevelDataObjectWriter)new WhsInvoiceDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).EDIMessageSubType);
		}

		public void TestGetTopLevelDataContextType()
		{
			AssertEquals(DataContextType.WarehousePeriodicInvoice, ((ITopLevelDataObjectWriter)new WhsInvoiceDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).TopLevelDataContextType);
		}

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehousePeriodicInvoice);
	}
}

