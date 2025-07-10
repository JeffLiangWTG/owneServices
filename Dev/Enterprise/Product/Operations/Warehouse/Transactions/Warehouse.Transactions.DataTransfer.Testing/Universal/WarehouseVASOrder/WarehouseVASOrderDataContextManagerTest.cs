using System;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WarehouseVASOrderDataContextManager))]
	class WarehouseVASOrderDataContextManagerTest : ShipmentDataContextManagerTestCase<WarehouseVASOrderDataContextManager, WhsVASOrder>
	{
		#region Supported Types

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => Array.Empty<RecipientRoleType>();

		#endregion

		#region TestDataContextKey

		public void TestDataContextKey()
		{
			var vasOrder = Factory.New<WhsVASOrder>();
			vasOrder.WVO_JobID = "VO0001";
			AssertEquals("DataContextKey should return Job ID.", "VO0001", vasOrder.GetUniversalDataContextManager().DataContextKey);
		}

		#endregion

		#region TestDataContextType

		public void TestDataContextType()
		{
			AssertEquals("DataContextType should be WarehouseVASOrder", DataContextType.WarehouseVASOrder, new WarehouseVASOrderDataContextManager().DataContextType);
		}

		#endregion

		#region TestDefaultOutputDirectory

		public void TestDefaultOutputDirectory()
		{
			AssertEquals("DefaultOutputDirectory should have no value", null, new WarehouseVASOrderDataContextManager().DefaultOutputDirectory);
		}

		#endregion

		#region TestEventContextValues

		public void TestEventContextValues()
		{
			var vasOrder = Factory.BOFactory.New<WhsVASOrder>();
			vasOrder.WVO_CustomerReferenceNo = "VASOrder123";
			var manager = vasOrder.GetUniversalDataContextManager() as IEventDataContextManager;
			AssertEquals("OrderNumber - VASOrder123", manager.EventContextValues.ToStringContents(o => o.Key + " - " + o.Value));
		}

		#endregion

		#region TestManagesShipments

		public void TestManagesShipments()
		{
			AssertEquals("ManagesShipments should be true", true, new WarehouseVASOrderDataContextManager().ManagesShipments);
		}

		#endregion

		#region Implementation

		#region Valid XML

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>WarehouseVASOrder</Type>
          <Key>VO00000001</Key>
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
      <OrderNumber>VO00000001</OrderNumber>
      <StagingArea>DEFAULT</StagingArea>
      <Warehouse>
        <Code>1</Code>
      </Warehouse>

      <OrderLineCollection>
        <OrderLine>
          <ExpiryDate>2016-02-25T00:00:00</ExpiryDate>
          <LineNumber>1</LineNumber>
          <PackingDate>2015-02-25T00:00:00</PackingDate>
          <PartAttribute1>1</PartAttribute1>
          <PartAttribute2>2</PartAttribute2>
          <PartAttribute3>3</PartAttribute3>
          <SerialNumber>SN</SerialNumber>
          <Product>
            <Code>P1</Code>
          </Product>
          <OrderedQty>1.000</OrderedQty>
        </OrderLine>
      </OrderLineCollection>
    </Order>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>WarehouseClient</AddressType>
        <OrganizationCode>CRAHOLSYD</OrganizationCode>
        <CompanyName>CRACKERJACK HOLDINGS</CompanyName>
        <Address1>1804 Fudrucker Way</Address1>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}

		#endregion

		#region SetupDataForDataContextManagerTestCase

		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var client = new OrganisationDataObjectReader(OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(OrganisationTypes.WarehouseClient)), new TestErrorLogger(), Factory).GetMatchedOrNewForTesting().Header;
			var helper = new WhsTestHelperFunctions(Factory.BOFactory);
			helper.CreateProductClientRelationShip(client, data.Part1);
			helper.CreateProductClientRelationShip(client, data.Part2);
			Factory.SaveForTesting();
		}

		#endregion

		#endregion
	}
}
