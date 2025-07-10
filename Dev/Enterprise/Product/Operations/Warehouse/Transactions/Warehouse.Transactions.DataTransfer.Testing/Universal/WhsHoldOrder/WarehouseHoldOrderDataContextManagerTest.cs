using System;
using System.Linq;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WarehouseHoldOrderDataContextManager))]
	class WarehouseHoldOrderDataContextManagerTest : ShipmentDataContextManagerTestCase<WarehouseHoldOrderDataContextManager, WhsHoldOrder>
	{
		#region Supported Types

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => Array.Empty<RecipientRoleType>();

		#endregion

		#region TestDataContextKey

		public void TestDataContextKey()
		{
			AssertEquals("NON PERSISTENT HOLD ORDER", new WhsHoldOrder(Factory.BOFactory).GetUniversalDataContextManager().DataContextKey);
		}

		#endregion

		#region TestDataContextType

		public void TestDataContextType()
		{
			AssertEquals("DataContextType should be WarehouseHoldOrder", DataContextType.WarehouseHoldOrder, new WarehouseHoldOrderDataContextManager().DataContextType);
		}

		#endregion

		#region TestDefaultOutputDirectory

		public void TestDefaultOutputDirectory()
		{
			AssertEquals("DefaultOutputDirectory should have no value", null, new WarehouseHoldOrderDataContextManager().DefaultOutputDirectory);
		}

		#endregion

		#region TestEventContextValues

		public void TestEventContextValues()
		{
			var manager = new WhsHoldOrder(Factory.BOFactory).GetUniversalDataContextManager() as IEventDataContextManager;
			AssertEquals(false, manager.EventContextValues.Any());
		}

		#endregion

		#region TestManagesEvents

		public void TestManagesEvents()
		{
			AssertEquals("ManagesEvents should be false", false, new WarehouseHoldOrderDataContextManager().ManagesEvents);
		}

		#endregion

		#region TestManagesShipments

		public void TestManagesShipments()
		{
			AssertEquals("ManagesShipments should be true", true, new WarehouseHoldOrderDataContextManager().ManagesShipments);
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
          <Type>WarehouseHoldOrder</Type>
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
      <OrderNumber>HOLD ORDER</OrderNumber>
      <Warehouse>
        <Code>1</Code>
      </Warehouse>

      <OrderLineCollection>
        <OrderLine>
          <Product>
            <Code>P1</Code>
          </Product>
          <OrderedQty>10.000</OrderedQty>
          <OriginalHoldCode>
			<Code></Code>
          </OriginalHoldCode>
          <CurrentHoldCode>
			<Code>HEL</Code>
          </CurrentHoldCode>		
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
			helper.CreateWhsReceiveWithInventory(client, data.Whs1, "R1", data.Part1, 50m);
			Factory.SaveForTesting();
		}

		#endregion

		#region GetNewBusinessObjectForTesting

		protected override WhsHoldOrder GetNewBusinessObjectForTesting()
		{
			return new WhsHoldOrder(Factory.BOFactory);
		}

		#endregion

		#endregion
	}
}
