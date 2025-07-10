using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.DataTransfer.Universal;
using Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing;
using Enterprise.Warehouse.Transactions.Invoicing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal
{
	class WhsInvoiceDataObjectReaderTest : WhsUniversalTestCase
	{
		public void TestGetNewBusinessObjectThrowDataObjectReadFailureException()
		{
			var whsInvoiceDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			AssertExceptionThrown<DataObjectReadFailureException>(() => _ = new WhsInvoiceDataObjectReaderForTest(whsInvoiceDO, Logger, Factory).NewBizO);
		}

		public void TestFindMatchingBusinessObjectReturnNull()
		{
			var whsInvoiceDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var matchingBizOFinder = new WhsInvoiceDataObjectReaderForTest(whsInvoiceDO, Logger, Factory).MatchingBizOFinder;

			AssertNull(matchingBizOFinder);
		}

		public void TestGetExistingBizOReturnNull()
		{
			var whsInvoiceDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var existingBizO = new WhsInvoiceDataObjectReaderForTest(whsInvoiceDO, Logger, Factory).ExistingBizO;

			AssertNull(existingBizO);
		}

		public void TestDataContextType()
		{
			var whsInvoiceDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			AssertEquals(DataContextType.WarehousePeriodicInvoice, new WhsInvoiceDataObjectReaderForTest(whsInvoiceDO, Logger, Factory).DataContextType);
		}

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehousePeriodicInvoice);
	}

	class WhsInvoiceDataObjectReaderForTest : WhsInvoiceDataObjectReader
	{
		internal WhsInvoiceDataObjectReaderForTest(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		public IMatchingBusinessEntityFinder<WhsInvoice> MatchingBizOFinder => GetCombinedReferenceMatcher();
		public WhsInvoice NewBizO => GetNewBusinessObject();
		public WhsInvoice ExistingBizO => GetExistingBusinessObjectUsingModuleSpecificBusinessRules();
	}
}
