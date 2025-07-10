using System.Threading;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	#region WhsReceiveEventParentFinderTest

	class WhsReceiveEventParentFinderTest : WhsOrderAndReceiveEventParentFinderTest<WhsReceive>
	{
		#region TestMatchGetsBestMatch

		public void TestMatchGetsBestMatch()
		{
			const string receiveLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
					<Context>
					  <Type>ReceiveReference</Type>
					  <Value>W000001</Value>
					</Context>
				  <Context>
				    <Type Description=""Customs Approval Number"">CAN</Type>
            <Value>11334455</Value>
				  </Context>
					<Context>
					  <Type>TransportReference</Type>
					  <Value>TRANSPORTER</Value>
					</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingReceive = Factory.NewWithValidTestData<WhsReceive>();
			matchingReceive.WD_DocketID = "W01010";
			matchingReceive.WD_ExternalReference = "W000001";
			matchingReceive.WD_TransportReference = "TRANSPORTER";

			var reference = matchingReceive.References.AddNew();
			reference.WX_RefType = "CAN";
			reference.WX_Reference = "11334455";

			Factory.Save();
			Thread.Sleep(200);

			var dummyReceive = Factory.NewWithValidTestData<WhsReceive>();
			dummyReceive.WD_ExternalReference = "W000001";
			dummyReceive.WD_TransportReference = "TRANSPORTER";

			var logParent = ProcessEventXML(receiveLevelEventXmlText);

			AssertLogParentIsCorrect(matchingReceive, logParent);
		}

		#endregion

		#region TestMatchGetsLatestWhenEqualMatches

		public void TestMatchGetsLatestWhenEqualMatches()
		{
			const string receiveLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ReceiveReference</Type>
				  <Value>W000001</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var dummyReceive = Factory.NewWithValidTestData<WhsReceive>();
			dummyReceive.WD_ExternalReference = "W000001";

			Factory.Save();
			Thread.Sleep(200);

			var matchingReceive = Factory.NewWithValidTestData<WhsReceive>();
			matchingReceive.WD_DocketID = "W01010";
			matchingReceive.WD_ExternalReference = "W000001";

			var logParent = ProcessEventXML(receiveLevelEventXmlText);

			AssertLogParentIsCorrect(matchingReceive, logParent);
		}

		#endregion

		#region MatchOnReference

		#region TestDoesNotMatchOnReferencesOnly

		public void TestDoesNotMatchOnReferencesOnly()
		{
			const string receiveLevelEventXmlText = @"
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

			var matchingReceive = Factory.NewWithValidTestData<WhsReceive>();
			matchingReceive.WD_DocketID = "W01010";
			matchingReceive.WD_BOLNo = "11334455";

			var reference = matchingReceive.References.AddNew();
			reference.WX_RefType = "CAN";
			reference.WX_Reference = "11334455";

			AssertLogParentIsNull(receiveLevelEventXmlText);
		}

		#endregion

		#region TestMatchOnReferencesAsFallBack

		public void TestMatchOnReferencesAsFallBack()
		{
			const string receiveLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ReceiveReference</Type>
				  <Value>W000001</Value>
				</Context>
				<Context>
				  <Type Description=""Customs Approval Number"">CAN</Type>
          <Value>11334455</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingReceive = Factory.NewWithValidTestData<WhsReceive>();
			matchingReceive.WD_DocketID = "W01010";
			matchingReceive.WD_BOLNo = "11334455";
			matchingReceive.WD_ExternalReference = "W000001";

			var reference = matchingReceive.References.AddNew();
			reference.WX_RefType = "CAN";
			reference.WX_Reference = "11334455";

			Factory.Save();
			Thread.Sleep(200);

			var dummyReceive = Factory.NewWithValidTestData<WhsReceive>();
			dummyReceive.WD_ExternalReference = "W000001";

			var logParent = ProcessEventXML(receiveLevelEventXmlText);

			AssertLogParentIsCorrect(matchingReceive, logParent);
		}

		#endregion

		#endregion

		#region MatchOnTransportReference

		#region TestDoesNotMatchOnTransportReferenceOnly

		public void TestDoesNotMatchOnTransportReferenceOnly()
		{
			const string receiveLevelEventXmlText = @"
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

			var whsReceive = Factory.NewWithValidTestData<WhsReceive>();
			whsReceive.WD_DocketID = "W01010";
			whsReceive.WD_TransportReference = "TRANSPORTER";

			AssertLogParentIsNull(receiveLevelEventXmlText);
		}

		#endregion

		#region TestMatchOnTransportReferenceAsFallBack

		public void TestMatchOnTransportReferenceAsFallBack()
		{
			const string receiveLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ReceiveReference</Type>
				  <Value>W000001</Value>
				</Context>
				<Context>
				  <Type>TransportReference</Type>
				  <Value>TRANSPORTER</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingReceive = Factory.NewWithValidTestData<WhsReceive>();
			matchingReceive.WD_DocketID = "W01010";
			matchingReceive.WD_ExternalReference = "W000001";
			matchingReceive.WD_TransportReference = "TRANSPORTER";

			Factory.Save();
			Thread.Sleep(200);

			var dummyReceive = Factory.NewWithValidTestData<WhsReceive>();
			dummyReceive.WD_ExternalReference = "W000001";

			var logParent = ProcessEventXML(receiveLevelEventXmlText);

			AssertLogParentIsCorrect(matchingReceive, logParent);
		}

		#endregion

		#endregion

		#region MatchOnClientReference

		#region TestDoesMatchOnClientReferenceAndReceiveReference

		public void TestDoesMatchOnClientReferenceAndReceiveReference()
		{
			const string receiveLevelEventXmlText = @"
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
					<Type>ReceiveReference</Type>
					<Value>123</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingReceive = Factory.NewWithValidTestData<WhsReceive>();
			matchingReceive.WD_DocketID = "W01010";
			matchingReceive.WD_CustomerReference = "Customer";
			matchingReceive.WD_ExternalReference = "123";

			Factory.Save();
			Thread.Sleep(200);

			var dummyReceive = Factory.NewWithValidTestData<WhsReceive>();
			dummyReceive.WD_CustomerReference = "WrongCustomer";
			dummyReceive.WD_ExternalReference = "123";

			var logParent = ProcessEventXML(receiveLevelEventXmlText);

			AssertLogParentIsCorrect(matchingReceive, logParent);
		}

		#endregion

		#region TestDoesNotMatchOnClientReferenceIfReceiveReferenceIsDifferent

		public void TestDoesNotMatchOnClientReferenceIfReceiveReferenceIsDifferent()
		{
			const string receiveLevelEventXmlText = @"
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
					<Type>ReceiveReference</Type>
					<Value>123</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var nonMatchingReceive = Factory.NewWithValidTestData<WhsReceive>();
			nonMatchingReceive.WD_DocketID = "W01010";
			nonMatchingReceive.WD_CustomerReference = "Customer";
			nonMatchingReceive.WD_ExternalReference = "456";

			AssertLogParentIsNull(receiveLevelEventXmlText);
		}

		#endregion

		#region TestMatchOnClientReference

		public void TestMatchOnClientReference()
		{
			const string receiveLevelEventXmlText = @"
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

			var matchingReceive = Factory.NewWithValidTestData<WhsReceive>();
			matchingReceive.WD_DocketID = "W01010";
			matchingReceive.WD_CustomerReference = "Customer";

			var logParent = ProcessEventXML(receiveLevelEventXmlText);

			AssertLogParentIsCorrect(matchingReceive, logParent);
		}

		#endregion

		#endregion

		#region MatchOnReceiveReference

		#region TestMatchOnReceiveReferencePlusSplitNumber

		public void TestMatchOnReceiveReferencePlusSplitNumber()
		{
			const string receiveLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ReceiveReference</Type>
				  <Value>W000001</Value>
				</Context>
				<Context>
				  <Type>ReceiveReferenceSplit</Type>
				  <Value>2</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingReceive = Factory.NewWithValidTestData<WhsReceive>();
			matchingReceive.WD_DocketID = "W01010";
			matchingReceive.WD_ExternalReference = "W000001";
			matchingReceive.WD_ExternalReferenceSplit = new ZByte(2);

			Factory.Save();
			Thread.Sleep(200);

			var dummyReceive = Factory.NewWithValidTestData<WhsReceive>();
			dummyReceive.WD_ExternalReference = "W000001";

			var logParent = ProcessEventXML(receiveLevelEventXmlText);

			AssertLogParentIsCorrect(matchingReceive, logParent);
		}

		#endregion

		#region TestMatchOnReceiveReference

		public void TestMatchOnReceiveReference()
		{
			const string receiveLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ReceiveReference</Type>
				  <Value>W000001</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingReceive = Factory.NewWithValidTestData<WhsReceive>();
			matchingReceive.WD_DocketID = "W01010";
			matchingReceive.WD_ExternalReference = "W000001";

			var logParent = ProcessEventXML(receiveLevelEventXmlText);

			AssertLogParentIsCorrect(matchingReceive, logParent);
		}

		#endregion

		#endregion

		#region TestDoesNotMatchToADocketOfDifferentType

		public void TestDoesNotMatchToADocketOfDifferentType()
		{
			const string receiveLevelEventXmlText = @"
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

			var nonMatchingOrder = Factory.NewWithValidTestData<WhsOrder>();
			nonMatchingOrder.WD_DocketID = "W01010";
			nonMatchingOrder.WD_CustomerReference = "Customer";

			AssertLogParentIsNull(receiveLevelEventXmlText);
		}

		#endregion

		#region Implementation

		protected override DataContextType DataContextType
		{
			get { return DataContextType.WarehouseReceive; }
		}

		protected override WhsOrderAndReceiveEventParentFinder<WhsReceive> GetNewEventParentFinder(IXmlImportLogger logger)
		{
			return new WhsReceiveEventParentFinder(Factory, new WarehouseReceiveDataContextManager(), logger);
		}

		protected override WhsReceive GetNewDocket(TestDataSimpleEnvironment data, ZString externalReference)
		{
			return Helper.CreateWhsReceive(data.Org1, data.Whs1, externalReference);
		}

		protected override WhsReceive GetNewFinalisedDocket(TestDataSimpleEnvironment data, ZString externalReference, ZString customsParentReference)
		{
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, externalReference, data.Part1, 10m);
			receive.WD_CustomsParentReference = customsParentReference;
			return receive;
		}

		protected override string GetRecipientRoleCollectionString()
		{
			return @"           
<RecipientRoleCollection>
	<RecipientRole>
		<Code>BWI</Code>
		<Description>Bonded Warehouse In</Description>
	</RecipientRole>
</RecipientRoleCollection>";
		}

		#endregion
	}

	#endregion

	#region WhsReceiveEventParentFinderForChangeOfInventoryTest

	class WhsReceiveEventParentFinderForChangeOfInventoryTest : WhsReceiveEventParentFinderTest
	{
		#region Implementation

		protected override DataContextType DataContextType
		{
			get { return DataContextType.WarehouseBondedChangeOfInventory; }
		}

		protected override WhsOrderAndReceiveEventParentFinder<WhsReceive> GetNewEventParentFinder(IXmlImportLogger logger)
		{
			return new WhsReceiveEventParentFinder(Factory, new WarehouseBondedChangeOfInventoryDataContextManager(), logger);
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
