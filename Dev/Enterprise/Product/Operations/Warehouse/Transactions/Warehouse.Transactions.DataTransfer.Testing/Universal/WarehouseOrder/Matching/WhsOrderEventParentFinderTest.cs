using System.Threading;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	#region WhsOrderEventParentFinderTest

	class WhsOrderEventParentFinderTest : WhsOrderAndReceiveEventParentFinderTest<WhsOrder>
	{
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

			var dummyOrder = Factory.NewWithValidTestData<WhsOrder>();
			dummyOrder.WD_ExternalReference = "W000001";

			Factory.Save();
			Thread.Sleep(200);

			var matchingOrder = Factory.NewWithValidTestData<WhsOrder>();
			matchingOrder.WD_DocketID = "W01010";
			matchingOrder.WD_ExternalReference = "W000001";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		#endregion

		#region MatchOnReferences

		#region TestMatchOnReferencesFallBack

		public void TestMatchOnReferencesFallBack()
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
				  <Type Description=""Customs Approval Number"">CAN</Type>
          <Value>11334455</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingOrder = Factory.NewWithValidTestData<WhsOrder>();
			matchingOrder.WD_DocketID = "W01010";
			matchingOrder.WD_BOLNo = "11334455";
			matchingOrder.WD_ExternalReference = "W000001";

			var reference = matchingOrder.References.AddNew();
			reference.WX_RefType = "CAN";
			reference.WX_Reference = "11334455";

			var dummyOrder = Factory.NewWithValidTestData<WhsOrder>();
			dummyOrder.WD_ExternalReference = "W000001";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		#endregion

		#region TestDoesNotMatchOnReferencesOnly

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

			var nonMatchingOrder = Factory.NewWithValidTestData<WhsOrder>();
			nonMatchingOrder.WD_DocketID = "W01010";
			nonMatchingOrder.WD_BOLNo = "11334455";

			var reference = nonMatchingOrder.References.AddNew();
			reference.WX_RefType = "CAN";
			reference.WX_Reference = "11334455";

			AssertLogParentIsNull(orderLevelEventXmlText);
		}

		#endregion

		#endregion

		#region MatchOnTransportRefence

		#region TestMatchOnTransportReferenceFallBack

		public void TestMatchOnTransportReferenceFallBack()
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
				  <Type>TransportReference</Type>
				  <Value>TRANSPORTER</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingOrder = Factory.NewWithValidTestData<WhsOrder>();
			matchingOrder.WD_DocketID = "W01010";
			matchingOrder.WD_ExternalReference = "W000001";
			matchingOrder.WD_TransportReference = "TRANSPORTER";

			var dummyOrder = Factory.NewWithValidTestData<WhsOrder>();
			dummyOrder.WD_ExternalReference = "W000001";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		#endregion

		#region TestDoesNotMatchOnTransportReferenceOnly

		public void TestDoesNotMatchOnTransportReferenceOnly()
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
				  <Type>TransportReference</Type>
				  <Value>TRANSPORTER</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var nonMatchingOrder = Factory.NewWithValidTestData<WhsOrder>();
			nonMatchingOrder.WD_DocketID = "W01010";
			nonMatchingOrder.WD_TransportReference = "TRANSPORTER";

			AssertLogParentIsNull(orderLevelEventXmlText);
		}

		#endregion

		#endregion

		#region MatchOnClientReference

		#region TestDoesMatchOnClientReferenceAndOrderNumber

		public void TestDoesMatchOnClientReferenceAndOrderNumber()
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
				  <Type>ClientReference</Type>
				  <Value>Customer</Value>
				</Context>
				<Context>
					<Type>OrderNumber</Type>
					<Value>123</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingOrder = Factory.NewWithValidTestData<WhsOrder>();
			matchingOrder.WD_DocketID = "W01010";
			matchingOrder.WD_CustomerReference = "Customer";
			matchingOrder.WD_ExternalReference = "123";

			Factory.Save();
			Thread.Sleep(200);

			var dummyOrder = Factory.NewWithValidTestData<WhsOrder>();
			dummyOrder.WD_CustomerReference = "WrongCustomer";
			dummyOrder.WD_ExternalReference = "123";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		#endregion

		#region TestDoesNotMatchOnClientReferenceIfOrderNumberIsDifferent

		public void TestDoesNotMatchOnClientReferenceIfOrderNumberIsDifferent()
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
				  <Type>ClientReference</Type>
				  <Value>Customer</Value>
				</Context>
				<Context>
					<Type>OrderNumber</Type>
					<Value>123</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var nonMatchingOrder = Factory.NewWithValidTestData<WhsOrder>();
			nonMatchingOrder.WD_DocketID = "W01010";
			nonMatchingOrder.WD_CustomerReference = "Customer";
			nonMatchingOrder.WD_ExternalReference = "456";

			AssertLogParentIsNull(orderLevelEventXmlText);
		}

		#endregion

		#region TestMatchOnClientReference

		public void TestMatchOnClientReference()
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
				  <Type>ClientReference</Type>
				  <Value>Customer</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingOrder = Factory.NewWithValidTestData<WhsOrder>();
			matchingOrder.WD_DocketID = "W01010";
			matchingOrder.WD_CustomerReference = "Customer";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		#endregion

		#endregion

		#region MatchOnOrderNumber

		#region TestMatchOnOrderNumberPlusSplitNumberWhenSplitNumberIsZero

		public void TestMatchOnOrderNumberPlusSplitNumberWhenSplitNumberIsZero()
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

			var matchingOrder = Factory.NewWithValidTestData<WhsOrder>();
			matchingOrder.WD_DocketID = "W01010";
			matchingOrder.WD_ExternalReference = "W000001";

			var dummyOrder = Factory.NewWithValidTestData<WhsOrder>();
			dummyOrder.WD_ExternalReference = "W000001";
			dummyOrder.WD_ExternalReferenceSplit = new ZByte(2);

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		#endregion

		#region TestMatchOnOrderNumberPlusSplitNumber

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

			var matchingOrder = Factory.NewWithValidTestData<WhsOrder>();
			matchingOrder.WD_DocketID = "W01010";
			matchingOrder.WD_ExternalReference = "W000001";
			matchingOrder.WD_ExternalReferenceSplit = new ZByte(2);

			var dummyOrder = Factory.NewWithValidTestData<WhsOrder>();
			dummyOrder.WD_ExternalReference = "W000001";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		#endregion

		#region TestMatchOnOrderNumber

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

			var matchingOrder = Factory.NewWithValidTestData<WhsOrder>();
			matchingOrder.WD_DocketID = "W01010";
			matchingOrder.WD_ExternalReference = "W000001";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		#endregion

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
				  <Type>ClientReference</Type>
				  <Value>Customer</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var nonMatchingReceive = Factory.NewWithValidTestData<WhsReceive>();
			nonMatchingReceive.WD_DocketID = "W01010";
			nonMatchingReceive.WD_CustomerReference = "Customer";

			AssertLogParentIsNull(orderLevelEventXmlText);
		}

		#endregion

		#region Implementation

		protected override WhsOrderAndReceiveEventParentFinder<WhsOrder> GetNewEventParentFinder(IXmlImportLogger logger)
		{
			return new WhsOrderEventParentFinder(Factory, new WarehouseOrderDataContextManager(), logger);
		}

		protected override DataContextType DataContextType
		{
			get { return DataContextType.WarehouseOrder; }
		}

		protected override WhsOrder GetNewDocket(TestDataSimpleEnvironment data, ZString externalReference)
		{
			return Helper.CreateWhsOrder(data.Org1, data.Whs1, externalReference);
		}

		protected override WhsOrder GetNewFinalisedDocket(TestDataSimpleEnvironment data, ZString externalReference, ZString customsParentReference)
		{
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, externalReference, data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, externalReference, data.Part1, 10m);
			order.WD_CustomsParentReference = customsParentReference;
			Helper.CreatePickNew(true, true, order);

			return order;
		}

		protected override string GetRecipientRoleCollectionString()
		{
			return @"           
<RecipientRoleCollection>
	<RecipientRole>
		<Code>BWR</Code>
		<Description>Bonded Warehouse Out</Description>
	</RecipientRole>
</RecipientRoleCollection>";
		}

		#endregion
	}

	#endregion

	#region WhsOrderEventParentFinderForChangeOfInventoryTest

	class WhsOrderEventParentFinderForChangeOfInventoryTest : WhsOrderEventParentFinderTest
	{
		#region Implementation

		protected override WhsOrderAndReceiveEventParentFinder<WhsOrder> GetNewEventParentFinder(IXmlImportLogger logger)
		{
			return new WhsOrderEventParentFinder(Factory, new WarehouseBondedChangeOfInventoryDataContextManager(), logger);
		}

		protected override DataContextType DataContextType
		{
			get { return DataContextType.WarehouseBondedChangeOfInventory; }
		}

		protected override bool RejectHoldEventIfDocketIsCancelled => false;

		protected override bool RejectHoldEventIfDocketIsFinalised => false;

		protected override string GetRecipientRoleCollectionString()
		{
			return @"           
<RecipientRoleCollection>
	<RecipientRole>
		<Code>BCO</Code>
		<Description>Change of Ownership</Description>
	</RecipientRole>
</RecipientRoleCollection>";
		}

		#endregion
	}

	#endregion
}
