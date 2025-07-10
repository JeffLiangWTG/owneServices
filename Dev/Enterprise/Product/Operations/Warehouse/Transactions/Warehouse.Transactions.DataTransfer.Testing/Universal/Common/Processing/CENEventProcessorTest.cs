using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class CENEventProcessorTest : WhsTestCaseWithFactory
	{
		public void TestGetEventRelatedOrderLines_NoDataContextFound()
		{
			var universalEvent = new UniversalEvent();
			universalEvent.EventReference = "APE";
			universalEvent.EventTime = new ZDateTimeOffset(2024, 03, 28, 10, 07, 32);
			universalEvent.ContextCollection = new List<UniversalDataBuss.DataObjects.Universal.Context>();
			var logger = new TestErrorLogger();
			var processor = new CENEventProcessorForTest(Factory, logger, universalEvent);
			processor.Execute();
			Assert("There should be a log for missing event context data.", logger.Logs.Contains("The CEN event misses declaration context data."));
		}

		public void TestExecute()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_ExternalReference = "BGM1";
			order.WD_CustomerReference = "CRF1";
			var line = Factory.NewWithValidTestData<WhsOrderLine>();
			line.WE_WD = order.PK;
			Factory.Save();

			var universalEvent = new UniversalEvent();
			universalEvent.EventReference = "APE";
			universalEvent.EventTime = new ZDateTimeOffset(2024, 03, 28, 10, 07, 32);
			universalEvent.ContextCollection = new List<UniversalDataBuss.DataObjects.Universal.Context>();
			universalEvent.ContextCollection.Add(new UniversalDataBuss.DataObjects.Universal.Context() { Type = nameof(IXmlEventValueObjectContextValueList.DeclarationReference), Value = "BGM1" });
			universalEvent.ContextCollection.Add(new UniversalDataBuss.DataObjects.Universal.Context() { Type = nameof(IXmlEventValueObjectContextValueList.EntryNumber), Value = "CRF1" });
			var logger = new TestErrorLogger();
			var processor = new CENEventProcessorForTest(Factory, logger, universalEvent);
			processor.Execute();
			CombineAssertions(() =>
			{
				AssertEquals("WB_EntryKey should have been updated with entryNumber.", "CRF1", line.CustomsData.WB_EntryKey);
				Assert("There should be a log for the update.", logger.Logs.Contains("1 order line Entry Key was updated with Entry Number."));
			});
		}

		public void TestExecute_NoOrderLineFound()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_ExternalReference = "BGM1";
			order.WD_CustomerReference = "CRF1";
			Factory.Save();

			var universalEvent = new UniversalEvent();
			universalEvent.EventReference = "APE";
			universalEvent.EventTime = new ZDateTimeOffset(2024, 03, 28, 10, 07, 32);
			universalEvent.ContextCollection = new List<UniversalDataBuss.DataObjects.Universal.Context>();
			universalEvent.ContextCollection.Add(new UniversalDataBuss.DataObjects.Universal.Context() { Type = nameof(IXmlEventValueObjectContextValueList.DeclarationReference), Value = "BGM1" });
			universalEvent.ContextCollection.Add(new UniversalDataBuss.DataObjects.Universal.Context() { Type = nameof(IXmlEventValueObjectContextValueList.EntryNumber), Value = "CRF1" });
			var logger = new TestErrorLogger();
			var processor = new CENEventProcessorForTest(Factory, logger, universalEvent);
			processor.Execute();
			Assert("There should be a log for no order line found.", logger.Logs.Contains("No order line matching the event was found."));
		}

		class CENEventProcessorForTest : CENEventProcessor
		{
			public CENEventProcessorForTest(BusinessObjectFactory factory, IXmlImportLogger logger, UniversalEvent xmlEvent) : base(factory, logger, xmlEvent) { }

			public void SetUpEntryKeyOfMatchingOrderLinesExposed() => SetUpEntryKeyOfMatchingOrderLines();
		}
	}
}
