using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class EventProcessorHelperTest : WhsTestCaseWithFactory
	{
		public void TestGetEventBGMReference()
		{
			var universalEvent = new UniversalEvent();
			universalEvent.ContextCollection = new List<UniversalDataBuss.DataObjects.Universal.Context>();

			AssertEquals(ZString.Empty, EventProcessorHelper.GetEventBGMReference(universalEvent));

			universalEvent.ContextCollection.Add(new UniversalDataBuss.DataObjects.Universal.Context() { Type = nameof(IXmlEventValueObjectContextValueList.DeclarationReference), Value = "12345678-B0055785/1" });
			AssertEquals("12345678-B0055785/1", EventProcessorHelper.GetEventBGMReference(universalEvent));
		}

		public void TestGetEventEntryNumber()
		{
			var universalEvent = new UniversalEvent();
			universalEvent.ContextCollection = new List<UniversalDataBuss.DataObjects.Universal.Context>();

			AssertEquals(ZString.Empty, EventProcessorHelper.GetEventEntryNumber(universalEvent));

			universalEvent.ContextCollection.Add(new UniversalDataBuss.DataObjects.Universal.Context() { Type = nameof(IXmlEventValueObjectContextValueList.EntryNumber), Value = "200000001" });
			AssertEquals("200000001", EventProcessorHelper.GetEventEntryNumber(universalEvent));
		}

		public void TestGetEventRelatedOrderLines()
		{
			var matchingOrder1 = Factory.NewWithValidTestData<WhsOrder>();
			matchingOrder1.WD_ExternalReference = "BGM1";
			matchingOrder1.WD_CustomerReference = "CRF1";
			var matchingLine1 = Factory.NewWithValidTestData<WhsOrderLine>();
			matchingLine1.WE_WD = matchingOrder1.PK;

			var matchingOrder2 = Factory.NewWithValidTestData<WhsOrder>();
			matchingOrder2.WD_ExternalReference = "BGM1";
			matchingOrder2.WD_CustomerReference = "CRF1";
			var matchingLine2 = Factory.NewWithValidTestData<WhsOrderLine>();
			matchingLine2.WE_WD = matchingOrder2.PK;

			var unmatchingOrder1 = Factory.NewWithValidTestData<WhsOrder>();
			unmatchingOrder1.WD_ExternalReference = "BGM1";
			unmatchingOrder1.WD_CustomerReference = "CRF2";
			var unmatchingLine1 = Factory.NewWithValidTestData<WhsOrderLine>();
			unmatchingLine1.WE_WD = unmatchingOrder1.PK;

			var unmatchingOrder2 = Factory.NewWithValidTestData<WhsOrder>();
			unmatchingOrder2.WD_ExternalReference = "BGM2";
			unmatchingOrder2.WD_CustomerReference = "CRF1";
			var unmatchingLine2 = Factory.NewWithValidTestData<WhsOrderLine>();
			unmatchingLine2.WE_WD = unmatchingOrder2.PK;

			var unmatchingOrder3 = Factory.NewWithValidTestData<WhsOrder>();
			unmatchingOrder3.WD_ExternalReference = "";
			unmatchingOrder3.WD_CustomerReference = "";
			var unmatchingLine3 = Factory.NewWithValidTestData<WhsOrderLine>();
			unmatchingLine3.WE_WD = unmatchingOrder3.PK;
			Factory.Save();

			var universalEvent = new UniversalEvent();
			universalEvent.EventReference = "APE";
			universalEvent.ContextCollection = new List<UniversalDataBuss.DataObjects.Universal.Context>();
			universalEvent.ContextCollection.Add(new UniversalDataBuss.DataObjects.Universal.Context() { Type = nameof(IXmlEventValueObjectContextValueList.DeclarationReference), Value = "BGM1" });
			universalEvent.ContextCollection.Add(new UniversalDataBuss.DataObjects.Universal.Context() { Type = nameof(IXmlEventValueObjectContextValueList.EntryNumber), Value = "CRF1" });

			var logger = new DummyLogger();
			AssertContainsExactElementsInAnyOrder("Lines should be collected from orders strictly matching event reference", new ZGuid[] { matchingLine1.PK, matchingLine2.PK }, EventProcessorHelper.GetEventRelatedOrderLines("COE", universalEvent, null, logger, Factory).Select(x => x.PK));
		}

		public void TestGetEventRelatedOrderLines_NoDataContextFound()
		{
			var universalEvent = new UniversalEvent();
			universalEvent.EventReference = "APE";
			universalEvent.EventTime = new ZDateTimeOffset(2024, 03, 28, 10, 07, 32);
			universalEvent.ContextCollection = new List<UniversalDataBuss.DataObjects.Universal.Context>();
			var logger = new TestErrorLogger();
			EventProcessorHelper.GetEventRelatedOrderLines("COE", universalEvent, null, logger, Factory);

			Assert("There should be a log for missing event context data.", logger.Logs.Contains("The COE event misses declaration context data."));
		}
	}
}
