using System;
using System.Threading;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WarehouseDynamicWorkOrderDataContextManager))]
	sealed class WarehouseDynamicWorkOrderDataContextManagerTest : WarehouseDocketDataContextManagerTestCase<WarehouseDynamicWorkOrderDataContextManager, WhsDynamicWorkOrder>
	{
		#region TestEventContextValues

		public void TestEventContextValues()
		{
			var dynamicWorkOrder = Factory.BOFactory.New<WhsDynamicWorkOrder>();
			var manager1 = dynamicWorkOrder.GetUniversalDataContextManager() as IEventDataContextManager;
			AssertEquals("", manager1.EventContextValues.ToStringContents(o => o.Key + " - " + o.Value));

			dynamicWorkOrder.WD_ExternalReference = "DynamicWorkOrder42";
			dynamicWorkOrder.WD_ExternalReferenceSplit = 7;

			var manager2 = dynamicWorkOrder.GetUniversalDataContextManager() as IEventDataContextManager;
			AssertEquals(
@"OrderNumber - DynamicWorkOrder42
OrderNumberSplit - 7",
			manager2.EventContextValues.ToStringContents(o => o.Key + " - " + o.Value));
		}

		#endregion

		#region TestGetLogParentsForEvent

		public void TestGetLogParentsForEvent()
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
				<Type>OrderNumberSplit</Type>
				<Value>8</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var matchingDynamicWorkOrder = Factory.NewWithValidTestData<WhsDynamicWorkOrder>();
			matchingDynamicWorkOrder.WD_DocketID = "W01010";
			matchingDynamicWorkOrder.WD_ExternalReference = "W000001";
			matchingDynamicWorkOrder.WD_ExternalReferenceSplit = 8;
			matchingDynamicWorkOrder.WD_TransportReference = "TRANSPORTER";

			Factory.SaveForTesting();
			Thread.Sleep(200);

			var dummyDynamicWorkOrder = Factory.NewWithValidTestData<WhsDynamicWorkOrder>();
			dummyDynamicWorkOrder.WD_ExternalReference = "W000001";
			dummyDynamicWorkOrder.WD_TransportReference = "TRANSPORTER";
			Factory.SaveForTesting();

			var xmlEvent = new XmlEventDeserializer().Parse(workOrderLevelEventXmlText);
			var dataContextManager = new WarehouseDynamicWorkOrderDataContextManager();

			var logParents = ((IEventDataContextManager)dataContextManager).GetLogParentsForEvent(xmlEvent, Factory.BOFactory, new TestErrorLogger());
			AssertContainsExactElementsInAnyOrder(new[] { matchingDynamicWorkOrder }, logParents);
		}

		#endregion

		protected override DataContextType ExpectedDataContextType => DataContextType.WarehouseDynamicWorkOrder;

		protected override Type ExpectedDataObjectReaderType => typeof(WhsDynamicWorkOrderDataObjectReader);

		protected override Type ExpectedDataObjectWriterType => typeof(WhsDynamicWorkOrderDataObjectWriter);

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => new[] { RecipientRoleType.WDO };

		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";

			var addressData = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var clientAddress = new OrganisationDataObjectReader(addressData, new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			clientAddress.Header.OH_IsWarehouseClient = true;

			Factory.SaveForTesting();
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return
@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment Action=""MERGE"">
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>WarehouseDynamicWorkOrder</Type>
          <Key>W00001003</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <EventType>
        <Code>ATH</Code>
        <Description>Action Authorised</Description>
      </EventType>
      <ServerID>DAT</ServerID>
      <TriggerDate>2011-03-27T11:13:00</TriggerDate>
      <TriggerDescription>Test Trigger</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>

    <Order>
      <OrderNumber>ORDERME</OrderNumber>
      <OrderNumberSplit>1</OrderNumberSplit>
      <Warehouse>
        <Code>WHS</Code>
      </Warehouse>
    </Order>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <OrganizationCode>CRAHOLSYD</OrganizationCode>
        <CompanyName>CRACKERJACK HOLDINGS</CompanyName>
        <Address1>1804 Fudrucker Way</Address1>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";
			}
		}
	}
}
