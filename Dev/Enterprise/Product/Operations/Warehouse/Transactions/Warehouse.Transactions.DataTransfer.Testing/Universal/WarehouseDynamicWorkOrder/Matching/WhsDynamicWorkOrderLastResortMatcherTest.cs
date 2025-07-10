using System.Threading;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsDynamicWorkOrderLastResortMatcherTest : WhsTestCaseWithFactory
	{
		public void TestMatchGetsBestMatch()
		{
			var matchingDynamicWorkOrder = Factory.NewWithValidTestData<WhsDynamicWorkOrder>();
			matchingDynamicWorkOrder.WD_DocketID = "W01010";
			matchingDynamicWorkOrder.WD_ExternalReference = "W000001";
			matchingDynamicWorkOrder.WD_ExternalReferenceSplit = 2;

			Factory.Save();
			Thread.Sleep(200);

			var dummyWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			dummyWorkOrder.WD_ExternalReference = "W000001";

			var references = new WhsDynamicWorkOrderReferences
			{
				ExternalReference = "W000001",
				ExternalReferenceSplit = 2,
			};

			var matcher = new WhsDynamicWorkOrderLastResortMatcher(Factory, references, new DummyLogger());
			var matchedDynamicWorkOrder = matcher.GetBestMatch();
			AssertEquals("Should have matched the expected dynamic work order.", matchingDynamicWorkOrder, matchedDynamicWorkOrder);
		}

		public void TestMatchGetsLatestWhenEqualMatches()
		{
			var dummyWorkOrder = Factory.NewWithValidTestData<WhsDynamicWorkOrder>();
			dummyWorkOrder.WD_ExternalReference = "W000001";
			Factory.Save();
			Thread.Sleep(200);

			var matchingDynamicWorkOrder = Factory.NewWithValidTestData<WhsDynamicWorkOrder>();
			matchingDynamicWorkOrder.WD_DocketID = "W01010";
			matchingDynamicWorkOrder.WD_ExternalReference = "W000001";
			Factory.Save();

			var matcher = new WhsDynamicWorkOrderLastResortMatcher(Factory, new WhsDynamicWorkOrderReferences { ExternalReference = "W000001" }, new DummyLogger());
			var matchedDynamicWorkOrder = matcher.GetBestMatch();
			AssertEquals("Should have matched the expected dynamic work order.", matchingDynamicWorkOrder, matchedDynamicWorkOrder);
		}

		public void TestMatchOnOrderNumberPlusSplitNumber_SplitNumberIsZero()
		{
			var matchingDynamicWorkOrder = Factory.NewWithValidTestData<WhsDynamicWorkOrder>();
			matchingDynamicWorkOrder.WD_DocketID = "W01010";
			matchingDynamicWorkOrder.WD_ExternalReference = "W000001";

			var dummyOrder = Factory.NewWithValidTestData<WhsOrder>();
			dummyOrder.WD_ExternalReference = "W000001";
			dummyOrder.WD_ExternalReferenceSplit = new ZByte(2);

			var references = new WhsDynamicWorkOrderReferences
			{
				ExternalReference = "W000001",
				ExternalReferenceSplit = ZByte.Zero,
			};

			var matcher = new WhsDynamicWorkOrderLastResortMatcher(Factory, references, new DummyLogger());
			var matchedDynamicWorkOrder = matcher.GetBestMatch();
			AssertEquals("Should have matched the expected dynamic work order.", matchingDynamicWorkOrder, matchedDynamicWorkOrder);
		}

		public void TestMatchOnOrderNumber()
		{
			var matchingDynamicWorkOrder = Factory.NewWithValidTestData<WhsDynamicWorkOrder>();
			matchingDynamicWorkOrder.WD_DocketID = "W01010";
			matchingDynamicWorkOrder.WD_ExternalReference = "W000001";

			var references = new WhsDynamicWorkOrderReferences
			{
				ExternalReference = "W000001",
			};

			var matcher = new WhsDynamicWorkOrderLastResortMatcher(Factory, references, new DummyLogger());
			var matchedDynamicWorkOrder = matcher.GetBestMatch();
			AssertEquals("Should have matched the expected dynamic work order.", matchingDynamicWorkOrder, matchedDynamicWorkOrder);
		}

		public void TestMatchOnOrderNumber_NonMatchingSplit()
		{
			var dynamicWorkOrder = Factory.NewWithValidTestData<WhsDynamicWorkOrder>();
			dynamicWorkOrder.WD_DocketID = "W01010";
			dynamicWorkOrder.WD_ExternalReference = "W000001";

			var references = new WhsDynamicWorkOrderReferences
			{
				ExternalReference = "W000001",
				ExternalReferenceSplit = new ZByte(2),
			};

			var matcher = new WhsDynamicWorkOrderLastResortMatcher(Factory, references, new DummyLogger());
			AssertNull(matcher.GetBestMatch());
		}

		public void TestDoesNotMatchToADocketOfDifferentType()
		{
			var workOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			workOrder.WD_DocketID = "W01010";
			workOrder.WD_ExternalReference = "W000001";

			var references = new WhsDynamicWorkOrderReferences
			{
				ExternalReference = "W000001",
			};

			var matcher = new WhsDynamicWorkOrderLastResortMatcher(Factory, references, new DummyLogger());
			AssertNull(matcher.GetBestMatch());
		}
	}
}
