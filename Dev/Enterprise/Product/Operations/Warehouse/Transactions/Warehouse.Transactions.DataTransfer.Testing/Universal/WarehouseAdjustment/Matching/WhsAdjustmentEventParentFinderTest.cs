using System.Threading;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsAdjustmentEventParentFinderTest : WhsEventParentFinderTest<WhsAdjustmentEventParentFinder, WhsAdjustment>
	{
		#region TestMatchGetsLatestWhenEqualMatches

		public void TestMatchGetsLatestWhenEqualMatches()
		{
			const string adjustmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>AdjustmentReference</Type>
				  <Value>W000001 W01010</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var dummyAdjustment = Factory.NewWithValidTestData<WhsAdjustment>();
			dummyAdjustment.WD_ExternalReference = "W000001";

			Factory.Save();
			Thread.Sleep(200);

			var matchingAdjustment = Factory.NewWithValidTestData<WhsAdjustment>();
			matchingAdjustment.WD_DocketID = "W01010";
			matchingAdjustment.WD_ExternalReference = "W000001";
			dummyAdjustment.WD_ExternalReference = "W000001 W01010";

			Factory.Save();

			var logParent = ProcessEventXML(adjustmentLevelEventXmlText);

			AssertLogParentIsCorrect(matchingAdjustment, logParent);
		}

		#endregion

		#region TestMatchOnAdjustmentReference

		public void TestMatchOnAdjustmentReference()
		{
			const string adjustmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>AdjustmentReference</Type>
				  <Value>W000001 W01010</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingAdjustment = Factory.NewWithValidTestData<WhsAdjustment>();
			matchingAdjustment.WD_DocketID = "W01010";
			matchingAdjustment.WD_ExternalReference = "W000001";

			var logParent = ProcessEventXML(adjustmentLevelEventXmlText);

			AssertLogParentIsCorrect(matchingAdjustment, logParent);
		}

		#endregion

		#region TestDoesNotMatchToADocketOfDifferentType

		public void TestDoesNotMatchToADocketOfDifferentType()
		{
			const string adjustmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>AdjustmentReference</Type>
				  <Value>ADJUSTME</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var nonMatchingOrder = Factory.NewWithValidTestData<WhsOrder>();
			nonMatchingOrder.WD_DocketID = "W01010";
			nonMatchingOrder.WD_ExternalReference = "ADJUSTME";

			AssertLogParentIsNull(adjustmentLevelEventXmlText);
		}

		#endregion

		#region Implementation

		protected override WhsAdjustmentEventParentFinder GetNewEventParentFinder(IXmlImportLogger logger)
		{
			return new WhsAdjustmentEventParentFinder(Factory, new WarehouseAdjustmentDataContextManager(), logger);
		}

		#endregion
	}
}
