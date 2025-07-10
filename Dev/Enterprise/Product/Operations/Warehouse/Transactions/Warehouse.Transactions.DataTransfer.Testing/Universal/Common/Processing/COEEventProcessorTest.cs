using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class COEEventProcessorTest : WhsTestCaseWithFactory
	{
		public void TestGetEventRelatedOrderLines_NoDataContextFound()
		{
			var universalEvent = new UniversalEvent();
			universalEvent.EventReference = "APE";
			universalEvent.EventTime = new ZDateTimeOffset(2024, 03, 28, 10, 07, 32);
			universalEvent.ContextCollection = new List<UniversalDataBuss.DataObjects.Universal.Context>();
			var logger = new TestErrorLogger();
			var processor = new COEEventProcessorForTest(Factory, logger, universalEvent);
			processor.Execute();
			Assert("There should be a log for missing event context data.", logger.Logs.Contains("The COE event misses declaration context data."));
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
			var processor = new COEEventProcessorForTest(Factory, logger, universalEvent);
			processor.Execute();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Order Line should have ExitDate additional info.", new string[] { "ExitDate|2024-03-28 10:07:32" }, line.CustomsData.BondedAdditionalInfo.Cast<WhsBWAAddInfo>().Select(x => x.KeyString + "|" + x.ValueString));
				Assert("There should be a log for the update.", logger.Logs.Contains("1 order line was updated with exit date."));
			});

			var universalEvent2 = new UniversalEvent();
			universalEvent.EventReference = "APE";
			universalEvent.EventTime = new ZDateTimeOffset(2024, 12, 12, 12, 12, 12);
			universalEvent.ContextCollection = new List<UniversalDataBuss.DataObjects.Universal.Context>();
			universalEvent.ContextCollection.Add(new UniversalDataBuss.DataObjects.Universal.Context() { Type = nameof(IXmlEventValueObjectContextValueList.DeclarationReference), Value = "BGM1" });
			universalEvent.ContextCollection.Add(new UniversalDataBuss.DataObjects.Universal.Context() { Type = nameof(IXmlEventValueObjectContextValueList.EntryNumber), Value = "CRF1" });
			var processor2 = new COEEventProcessorForTest(Factory, logger, universalEvent);
			processor2.Execute();
			AssertContainsExactElementsInAnyOrder("No duplicate ExitDate if event is processed twice.", new string[] { "ExitDate|2024-03-28 10:07:32" }, line.CustomsData.BondedAdditionalInfo.Cast<WhsBWAAddInfo>().Select(x => x.KeyString + "|" + x.ValueString));
		}

		public void TestExecute_NoEventTime()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_ExternalReference = "BGM1";
			order.WD_CustomerReference = "CRF1";
			var line = Factory.NewWithValidTestData<WhsOrderLine>();
			line.WE_WD = order.PK;
			Factory.Save();

			var universalEvent = new UniversalEvent();
			universalEvent.EventReference = "APE";
			universalEvent.ContextCollection = new List<UniversalDataBuss.DataObjects.Universal.Context>();
			universalEvent.ContextCollection.Add(new UniversalDataBuss.DataObjects.Universal.Context() { Type = nameof(IXmlEventValueObjectContextValueList.DeclarationReference), Value = "BGM1" });
			universalEvent.ContextCollection.Add(new UniversalDataBuss.DataObjects.Universal.Context() { Type = nameof(IXmlEventValueObjectContextValueList.EntryNumber), Value = "CRF1" });
			var logger = new TestErrorLogger();
			var processor = new COEEventProcessorForTest(Factory, logger, universalEvent);
			processor.Execute();
			CombineAssertions(() =>
			{
				AssertEquals("Order line add infos should not have been updated.", 0, line.CustomsData.BondedAdditionalInfo.Cast<WhsBWAAddInfo>().Count());
				Assert("There should be a log for the missing EventTime.", logger.Logs.Contains("The COE event misses Event Time information."));
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
			var processor = new COEEventProcessorForTest(Factory, logger, universalEvent);
			processor.Execute();
			Assert("There should be a log for no order line found.", logger.Logs.Contains("No order line matching the event was found."));
		}

		class COEEventProcessorForTest : COEEventProcessor
		{
			public COEEventProcessorForTest(BusinessObjectFactory factory, IXmlImportLogger logger, UniversalEvent xmlEvent) : base(factory, logger, xmlEvent) { }

			public void AddExitDateToMatchingOrderLinesExposed() => AddExitDateToMatchingOrderLines();
		}
	}
}
