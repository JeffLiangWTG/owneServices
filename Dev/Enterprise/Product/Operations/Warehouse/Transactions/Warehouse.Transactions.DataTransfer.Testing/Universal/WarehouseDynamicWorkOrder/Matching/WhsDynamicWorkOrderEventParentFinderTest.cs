using System.Threading;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	internal class WhsDynamicWorkOrderEventParentFinderTest : WhsEventParentFinderTest<WarehouseDynamicWorkOrderEventParentFinder, WhsDynamicWorkOrder>
	{
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

			var matchingDynamicWorkOrder = Factory.NewWithValidTestData<WhsDynamicWorkOrder>();
			matchingDynamicWorkOrder.WD_DocketID = "W01010";
			matchingDynamicWorkOrder.WD_ExternalReference = "W000001";

			var dummyOrder = Factory.NewWithValidTestData<WhsOrder>();
			dummyOrder.WD_ExternalReference = "W000001";
			dummyOrder.WD_ExternalReferenceSplit = new ZByte(2);

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingDynamicWorkOrder, logParent);
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

			var matchingDynamicWorkOrder = Factory.NewWithValidTestData<WhsDynamicWorkOrder>();
			matchingDynamicWorkOrder.WD_DocketID = "W01010";
			matchingDynamicWorkOrder.WD_ExternalReference = "W000001";
			matchingDynamicWorkOrder.WD_ExternalReferenceSplit = new ZByte(2);

			var dummyOrder = Factory.NewWithValidTestData<WhsOrder>();
			dummyOrder.WD_ExternalReference = "W000001";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingDynamicWorkOrder, logParent);
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

			var matchingDynamicWorkOrder = Factory.NewWithValidTestData<WhsDynamicWorkOrder>();
			matchingDynamicWorkOrder.WD_DocketID = "W01010";
			matchingDynamicWorkOrder.WD_ExternalReference = "W000001";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingDynamicWorkOrder, logParent);
		}

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

			var nonMatchingOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			nonMatchingOrder.WD_DocketID = "W01010";
			nonMatchingOrder.WD_ExternalReference = "W000001";

			AssertLogParentIsNull(orderLevelEventXmlText);
		}

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

			var dummyOrder = Factory.NewWithValidTestData<WhsDynamicWorkOrder>();
			dummyOrder.WD_ExternalReference = "W000001";

			Factory.Save();
			Thread.Sleep(200);

			var matchingDynamicWorkOrder = Factory.NewWithValidTestData<WhsDynamicWorkOrder>();
			matchingDynamicWorkOrder.WD_DocketID = "W01010";
			matchingDynamicWorkOrder.WD_ExternalReference = "W000001";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingDynamicWorkOrder, logParent);
		}

		protected override WarehouseDynamicWorkOrderEventParentFinder GetNewEventParentFinder(IXmlImportLogger logger)
		{
			return new WarehouseDynamicWorkOrderEventParentFinder(Factory, new WarehouseDynamicWorkOrderDataContextManager(), logger);
		}
	}
}
