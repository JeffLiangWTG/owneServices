using System;
using System.Threading;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WarehouseWorkOrderDataContextManager))]
	sealed class WarehouseWorkOrderDataContextManagerTest : WarehouseDocketDataContextManagerTestCase<WarehouseWorkOrderDataContextManager, WhsWorkOrder>
	{
		#region TestEventContextValues

		public void TestEventContextValues()
		{
			var workOrder = Factory.BOFactory.New<WhsWorkOrder>();
			var manager1 = workOrder.GetUniversalDataContextManager() as IEventDataContextManager;
			AssertEquals("", manager1.EventContextValues.ToStringContents(o => o.Key + " - " + o.Value));

			workOrder.WD_ExternalReference = "WorkOrder42";
			workOrder.WD_ExternalReferenceSplit = 7;

			var reference1 = workOrder.References.AddNew();
			reference1.WX_RefType = "CAN";
			reference1.WX_Reference = "980665";

			var reference2 = workOrder.References.AddNew();
			reference2.WX_RefType = "BPR";
			reference2.WX_Reference = "31415";

			var manager2 = workOrder.GetUniversalDataContextManager() as IEventDataContextManager;
			AssertEquals(
@"OrderNumber - WorkOrder42
OrderNumberSplit - 7
Customs Approval Number - 980665
Booking Party Reference - 31415",
			manager2.EventContextValues.ToStringContents(o => o.Key + " - " + o.Value));
		}

		#endregion

		#region TestGetShipmentDataObjectReader_GetsSourceDataObject

		public void TestGetShipmentDataObjectReader_GetsSourceDataObject()
		{
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "DUM123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "DUM456");
			consol.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "DUM789");

			var masterShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			masterShipment.DataContext = DataContextFactory.New();
			masterShipment.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM789");

			var shipmentAndCustoms = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentAndCustoms.DataContext = DataContextFactory.New();
			shipmentAndCustoms.DataContext.AddDataSource(DataContextType.ForwardingShipment, "DUM456");
			shipmentAndCustoms.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "DUM789");

			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.SubShipmentCollection.Add(masterShipment);

			masterShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			masterShipment.SubShipmentCollection.Add(shipmentAndCustoms);

			var dataContextManager = new WarehouseWorkOrderDataContextManager();

			var reader = ((IShipmentDataContextManagerInternal)dataContextManager).GetShipmentDataObjectReader(consol, Mock.Of<IXmlImportLogger>(), Factory);
			AssertType(ExpectedDataObjectReaderType, reader);

			var docketReader = reader as WhsWorkOrderDataObjectReader;
			AssertNotNull("Should be a WhsWorkOrderDataObjectReader.", docketReader);
			AssertEquals("DataObject should be shipmentAndCustoms.", shipmentAndCustoms, docketReader.DataObject);
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

			Factory.SaveForTesting();
			Thread.Sleep(200);

			var dummyWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			dummyWorkOrder.WD_ExternalReference = "W000001";
			dummyWorkOrder.WD_TransportReference = "TRANSPORTER";
			Factory.SaveForTesting();

			var xmlEvent = new XmlEventDeserializer().Parse(workOrderLevelEventXmlText);
			var dataContextManager = new WarehouseWorkOrderDataContextManager();

			var logParents = ((IEventDataContextManager)dataContextManager).GetLogParentsForEvent(xmlEvent, Factory.BOFactory, new TestErrorLogger());
			AssertContainsExactElementsInAnyOrder(new[] { matchingWorkOrder }, logParents);
		}

		#endregion

		#region Implementation

		protected override DataContextType ExpectedDataContextType => DataContextType.WarehouseWorkOrder;

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => new[] { RecipientRoleType.WWO };

		protected override Type ExpectedDataObjectReaderType => typeof(WhsWorkOrderDataObjectReader);

		protected override Type ExpectedDataObjectWriterType => typeof(WhsWorkOrderDataObjectWriter);

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
          <Type>WarehouseWorkOrder</Type>
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
		#endregion
	}
}
