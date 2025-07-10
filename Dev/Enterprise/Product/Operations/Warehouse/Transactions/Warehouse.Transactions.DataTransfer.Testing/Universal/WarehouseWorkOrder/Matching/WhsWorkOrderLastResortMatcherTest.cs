using System.Collections.Generic;
using System.Threading;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsWorkOrderLastResortMatcherTest : WhsTestCaseWithFactory
	{
		#region TestMatchGetsBestMatch

		public void TestMatchGetsBestMatch()
		{
			var matchingWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			matchingWorkOrder.WD_DocketID = "W01010";
			matchingWorkOrder.WD_ExternalReference = "W000001";
			matchingWorkOrder.WD_TransportReference = "TRANSPORTER";

			var reference = matchingWorkOrder.References.AddNew();
			reference.WX_RefType = "CAN";
			reference.WX_Reference = "11334455";

			Factory.Save();
			Thread.Sleep(200);

			var dummyWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			dummyWorkOrder.WD_ExternalReference = "W000001";
			dummyWorkOrder.WD_TransportReference = "TRANSPORTER";

			var references = new WhsWorkOrderReferences
			{
				ExternalReference = "W000001",
				References = new List<KeyValuePair<ZString, ZString>> { new KeyValuePair<ZString, ZString>("CAN", "11334455") },
			};

			AssertMatchedOrderIsCorrect(matchingWorkOrder, references);
		}

		#endregion

		#region TestMatchGetsLatestWhenEqualMatches

		public void TestMatchGetsLatestWhenEqualMatches()
		{
			var dummyWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			dummyWorkOrder.WD_ExternalReference = "W000001";
			Factory.Save();
			Thread.Sleep(200);

			var matchingWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			matchingWorkOrder.WD_DocketID = "W01010";
			matchingWorkOrder.WD_ExternalReference = "W000001";
			Factory.Save();

			AssertMatchedOrderIsCorrect(matchingWorkOrder, new WhsWorkOrderReferences { ExternalReference = "W000001" });
		}

		#endregion

		#region MatchOnReferences

		public void TestMatchOnReferencesFallBack_CAN() => TestMatchOnReferencesFallBack(WarehouseAdditionalReferenceTypes.Codes.CustomsApprovalNumber, WarehouseAdditionalReferenceTypes.Codes.CustomsApprovalNumber);
		public void TestMatchOnReferencesFallBack_HSB() => TestMatchOnReferencesFallBack(WarehouseAdditionalReferenceTypes.Codes.HouseBill, WarehouseAdditionalReferenceTypes.Codes.HouseBill);
		public void TestMatchOnReferencesFallBack_BPR() => TestMatchOnReferencesFallBack(WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);

		void TestMatchOnReferencesFallBack(string code, string description)
		{
			var matchingWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			matchingWorkOrder.WD_DocketID = "W01010";
			matchingWorkOrder.WD_BOLNo = "11334455";
			matchingWorkOrder.WD_ExternalReference = "W000001";

			var reference = matchingWorkOrder.References.AddNew();
			reference.WX_RefType = code;
			reference.WX_Reference = "11334455";
			Factory.Save();
			Thread.Sleep(200);

			var dummyOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			dummyOrder.WD_ExternalReference = "W000001";

			var references = new WhsWorkOrderReferences
			{
				ExternalReference = "W000001",
				References = new List<KeyValuePair<ZString, ZString>> { new KeyValuePair<ZString, ZString>(code, "11334455") },
			};

			AssertMatchedOrderIsCorrect(matchingWorkOrder, references);
		}

		public void TestDoesNotMatchOnReferencesOnly()
		{
			var nonmatchingWorkOrder = Factory.NewWithValidTestData<WhsOrder>();
			nonmatchingWorkOrder.WD_DocketID = "W01010";
			nonmatchingWorkOrder.WD_BOLNo = "11334455";

			var reference = nonmatchingWorkOrder.References.AddNew();
			reference.WX_RefType = "CAN";
			reference.WX_Reference = "11334455";

			var references = new WhsWorkOrderReferences
			{
				ExternalReference = "W000001",
				References = new List<KeyValuePair<ZString, ZString>> { new KeyValuePair<ZString, ZString>("CAN", "11334455") },
			};

			AssertMatchedOrderIsNull(references);
		}

		#endregion

		#region MatchOnOrderNumber

		public void TestMatchOnOrderNumberPlusSplitNumber_SplitNumberIsZero()
		{
			var matchingWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			matchingWorkOrder.WD_DocketID = "W01010";
			matchingWorkOrder.WD_ExternalReference = "W000001";

			var dummyOrder = Factory.NewWithValidTestData<WhsOrder>();
			dummyOrder.WD_ExternalReference = "W000001";
			dummyOrder.WD_ExternalReferenceSplit = new ZByte(2);

			var references = new WhsWorkOrderReferences
			{
				ExternalReference = "W000001",
				ExternalReferenceSplit = ZByte.Zero,
			};

			AssertMatchedOrderIsCorrect(matchingWorkOrder, references);
		}

		public void TestMatchOnOrderNumberPlusSplitNumber()
		{
			var matchingWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			matchingWorkOrder.WD_DocketID = "W01010";
			matchingWorkOrder.WD_ExternalReference = "W000001";
			matchingWorkOrder.WD_ExternalReferenceSplit = new ZByte(2);

			var dummyOrder = Factory.NewWithValidTestData<WhsOrder>();
			dummyOrder.WD_ExternalReference = "W000001";

			var references = new WhsWorkOrderReferences
			{
				ExternalReference = "W000001",
				ExternalReferenceSplit = new ZByte(2),
			};

			AssertMatchedOrderIsCorrect(matchingWorkOrder, references);
		}

		public void TestMatchOnOrderNumber()
		{
			var matchingWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			matchingWorkOrder.WD_DocketID = "W01010";
			matchingWorkOrder.WD_ExternalReference = "W000001";

			var references = new WhsWorkOrderReferences
			{
				ExternalReference = "W000001",
			};

			AssertMatchedOrderIsCorrect(matchingWorkOrder, references);
		}

		public void TestMatchOnOrderNumber_NonMatchingSplit()
		{
			var matchingWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			matchingWorkOrder.WD_DocketID = "W01010";
			matchingWorkOrder.WD_ExternalReference = "W000001";

			var references = new WhsWorkOrderReferences
			{
				ExternalReference = "W000001",
				ExternalReferenceSplit = new ZByte(2),
			};

			AssertMatchedOrderIsNull(references);
		}

		#endregion

		#region TestDoesNotMatchToADocketOfDifferentType

		public void TestDoesNotMatchToADocketOfDifferentType()
		{
			var nonMatchingOrder = Factory.NewWithValidTestData<WhsOrder>();
			nonMatchingOrder.WD_DocketID = "W01010";
			nonMatchingOrder.WD_ExternalReference = "W000001";

			var references = new WhsWorkOrderReferences
			{
				ExternalReference = "W000001",
			};

			AssertMatchedOrderIsNull(references);
		}

		#endregion

		#region Implementation

		void AssertMatchedOrderIsCorrect(WhsWorkOrder matchingWorkOrder, WhsWorkOrderReferences references)
		{
			var matcher = new WhsWorkOrderLastResortMatcher(Factory, references, new DummyLogger());
			var matchedWorkOrder = matcher.GetBestMatch();
			AssertEquals("Should have matched the expected work order.", matchingWorkOrder, matchedWorkOrder);
		}

		void AssertMatchedOrderIsNull(WhsWorkOrderReferences references)
		{
			var matcher = new WhsWorkOrderLastResortMatcher(Factory, references, new DummyLogger());
			AssertNull("Should not have matched a work order.", matcher.GetBestMatch());
		}

		#endregion
	}
}
