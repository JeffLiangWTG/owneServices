using System.Threading;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsWorkOrderEventParentFinderTest : WhsEventParentFinderTest<WarehouseWorkOrderEventParentFinder, WhsWorkOrder>
	{
		#region TestMatchGetsBestMatch

		public void TestMatchGetsBestMatch()
		{
			const string workOrderLevelEventXmlText = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>OrderNumber</Type>
				<Value>W000001</Value>
			</Context>
			<Context>
				<Type Description=""Customs Approval Number"">CAN</Type>
				<Value>11334455</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

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

			var logParent = ProcessEventXML(workOrderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingWorkOrder, logParent);
		}

		#endregion

		#region TestMatchGetsLatestWhenEqualMatches

		public void TestMatchGetsLatestWhenEqualMatches()
		{
			const string orderLevelEventXmlText = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>OrderNumber</Type>
				<Value>W000001</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var dummyWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			dummyWorkOrder.WD_ExternalReference = "W000001";

			Factory.Save();
			Thread.Sleep(200);

			var matchingWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			matchingWorkOrder.WD_DocketID = "W01010";
			matchingWorkOrder.WD_ExternalReference = "W000001";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingWorkOrder, logParent);
		}

		#endregion

		#region MatchOnReferences

		public void TestMatchOnReferencesFallBack_CAN() => TestMatchOnReferencesFallBack(WarehouseAdditionalReferenceTypes.Codes.CustomsApprovalNumber, WarehouseAdditionalReferenceTypes.Codes.CustomsApprovalNumber);
		public void TestMatchOnReferencesFallBack_HSB() => TestMatchOnReferencesFallBack(WarehouseAdditionalReferenceTypes.Codes.HouseBill, WarehouseAdditionalReferenceTypes.Codes.HouseBill);
		public void TestMatchOnReferencesFallBack_BPR() => TestMatchOnReferencesFallBack(WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);

		void TestMatchOnReferencesFallBack(string code, string description)
		{
			string orderLevelEventXmlText = $@"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>OrderNumber</Type>
				<Value>W000001</Value>
			</Context>
			<Context>
				<Type Description=""{description}"">{code}</Type>
				<Value>11334455</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

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

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingWorkOrder, logParent);
		}

		public void TestDoesNotMatchOnReferencesOnly()
		{
			const string orderLevelEventXmlText = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type Description=""Customs Approval Number"">CAN</Type>
				<Value>11334455</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var nonmatchingWorkOrder = Factory.NewWithValidTestData<WhsOrder>();
			nonmatchingWorkOrder.WD_DocketID = "W01010";
			nonmatchingWorkOrder.WD_BOLNo = "11334455";

			var reference = nonmatchingWorkOrder.References.AddNew();
			reference.WX_RefType = "CAN";
			reference.WX_Reference = "11334455";

			AssertLogParentIsNull(orderLevelEventXmlText);
		}

		#endregion

		#region MatchOnOrderNumber

		public void TestMatchOnOrderNumberPlusSplitNumber_SplitNumberIsZero()
		{
			const string orderLevelEventXmlText = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>OrderNumber</Type>
				<Value>W000001</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var matchingWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			matchingWorkOrder.WD_DocketID = "W01010";
			matchingWorkOrder.WD_ExternalReference = "W000001";

			var dummyOrder = Factory.NewWithValidTestData<WhsOrder>();
			dummyOrder.WD_ExternalReference = "W000001";
			dummyOrder.WD_ExternalReferenceSplit = new ZByte(2);

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingWorkOrder, logParent);
		}

		public void TestMatchOnOrderNumberPlusSplitNumber()
		{
			const string orderLevelEventXmlText = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>OrderNumber</Type>
				<Value>W000001</Value>
			</Context>
			<Context>
				<Type>OrderNumberSplit</Type>
				<Value>2</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var matchingWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			matchingWorkOrder.WD_DocketID = "W01010";
			matchingWorkOrder.WD_ExternalReference = "W000001";
			matchingWorkOrder.WD_ExternalReferenceSplit = new ZByte(2);

			var dummyOrder = Factory.NewWithValidTestData<WhsOrder>();
			dummyOrder.WD_ExternalReference = "W000001";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingWorkOrder, logParent);
		}

		public void TestMatchOnOrderNumber()
		{
			const string orderLevelEventXmlText = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>OrderNumber</Type>
				<Value>W000001</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var matchingWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			matchingWorkOrder.WD_DocketID = "W01010";
			matchingWorkOrder.WD_ExternalReference = "W000001";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingWorkOrder, logParent);
		}

		#endregion

		#region TestDoesNotMatchToADocketOfDifferentType

		public void TestDoesNotMatchToADocketOfDifferentType()
		{
			const string orderLevelEventXmlText = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>OrderNumber</Type>
				<Value>W000001</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var nonMatchingOrder = Factory.NewWithValidTestData<WhsOrder>();
			nonMatchingOrder.WD_DocketID = "W01010";
			nonMatchingOrder.WD_ExternalReference = "W000001";

			AssertLogParentIsNull(orderLevelEventXmlText);
		}

		#endregion

		#region Implementation

		protected override WarehouseWorkOrderEventParentFinder GetNewEventParentFinder(IXmlImportLogger logger)
			=> new WarehouseWorkOrderEventParentFinder(Factory, new WarehouseWorkOrderDataContextManager(), logger);

		#endregion
	}
}
