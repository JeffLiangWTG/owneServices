using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;
using static Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing.WhsBondedChangeOfInventoryDataObjectReaderTest;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WarehouseBondedChangeOfInventoryDataContextManager))]
	class WarehouseBondedChangeOfInventoryDataContextManagerTest : ShipmentDataContextManagerTestCase<WarehouseBondedChangeOfInventoryDataContextManager, WhsBondedChangeOfInventory>
	{
		#region TestDataContextKey

		public void TestDataContextKey()
		{
			AssertEquals("Should not have a Data Context Key.", "NON PERSISTENT WAREHOUSE BONDED CHANGE OF INVENTORY", GetDataContextManagerForTesting().DataContextKey);
		}

		#endregion

		#region TestDataContextType

		public void TestDataContextType()
		{
			AssertEquals("DataContextType should be WarehouseBondedChangeOfInventory.", DataContextType.WarehouseBondedChangeOfInventory, GetDataContextManagerForTesting().DataContextType);
		}

		#endregion

		#region TestDefaultOutputDirectory

		public void TestDefaultOutputDirectory()
		{
			AssertEquals("DefaultOutputDirectory should have no value.", null, GetDataContextManagerForTesting().DefaultOutputDirectory);
		}

		#endregion

		#region Recipient Roles

		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get { return new RecipientRoleType[] { RecipientRoleType.BCO, RecipientRoleType.BCR }; }
		}

		#endregion

		#region TestEventContextValues

		public void TestEventContextValues()
		{
			AssertEquals("EventContextValues should be empty.", false, GetDataContextManagerForTesting().EventContextValues.Any());
		}

		#endregion

		#region TestManagesEvents

		public void TestManagesEvents()
		{
			AssertEquals("ManagesEvents should be false.", true, GetDataContextManagerForTesting().ManagesEvents);
		}

		#endregion

		#region TestManagesShipments

		public void TestManagesShipments()
		{
			AssertEquals("ManagesShipments should be true.", true, GetDataContextManagerForTesting().ManagesShipments);
		}

		#endregion

		#region TestImportUniversalEvent_ChangeOfInventory

		public void TestImportUniversalEvent_ChangeOfInventory_ChangeOfOwnership()
		{
			TestImportUniversalEvent_ChangeOfInventoryCore(RecipientRoleType.BCO);
		}

		public void TestImportUniversalEvent_ChangeOfInventory_ChangeOfRegime()
		{
			TestImportUniversalEvent_ChangeOfInventoryCore(RecipientRoleType.BCR);
		}

		void TestImportUniversalEvent_ChangeOfInventoryCore(RecipientRoleType changeOfInventoryRole)
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_CustomerReference = "Customer";
			Factory.SaveForTesting();

			var universalEvent = new UniversalEvent();
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			universalEvent.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { changeOfInventoryRole }.ToRecipientRoleDetails() });
			universalEvent.EventType = ZArchitecture.Business.Events.CancelTheWarehouseJobCode;
			universalEvent.ContextCollection = new List<Context> { new Context { Type = nameof(UniversalEvent.ContextTypes.ClientReference), Value = "Customer" } };

			var logger = new TestErrorLogger { TopLevelDataObject = universalEvent };
			var iManager = (IEventDataContextManager)GetDataContextManagerForTesting();
			var parentsFound = iManager.GetLogParentsForEvent(universalEvent, Factory.BOFactory, logger);
			AssertEquals("Should find Order and Dummy ChangeOfInventory because event is for Change of Inventory.", 2, parentsFound.Length);
			AssertNotNull("Should find Order", parentsFound.Single(p => p.PK == order.PK));
			var changeOfInventoryWithOrder = (WhsBondedChangeOfInventory)parentsFound.Single(p => p is WhsBondedChangeOfInventory);
			AssertEquals("Should find Order on dummy Change of Inventory", order.PK, changeOfInventoryWithOrder.Order.PK);
			AssertNull("Receive on Change of Inventory should be null", changeOfInventoryWithOrder.Receive);

			universalEvent.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR }.ToRecipientRoleDetails() });
			AssertEquals("Should NOT find a parent, as event is NOT for Change of Inventory.", 0, iManager.GetLogParentsForEvent(universalEvent, Factory.BOFactory, logger).Length);

			var receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_CustomerReference = "Customer";
			Factory.SaveForTesting();
			universalEvent.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { changeOfInventoryRole }.ToRecipientRoleDetails() });
			parentsFound = iManager.GetLogParentsForEvent(universalEvent, Factory.BOFactory, logger);
			AssertEquals("Should find Order, Receive and Dummy ChangeOfInventory because event is for Change of Inventory.", 3, parentsFound.Length);
			AssertNotNull("Should find Order", parentsFound.Single(p => p.PK == order.PK));
			AssertNotNull("Should find Receive", parentsFound.Single(p => p.PK == receive.PK));
			var changeOfInventoryWithOrderAndReceive = (WhsBondedChangeOfInventory)parentsFound.Single(p => p is WhsBondedChangeOfInventory);
			AssertEquals("Should find Order on dummy Change of Inventory", order.PK, changeOfInventoryWithOrderAndReceive.Order.PK);
			AssertEquals("Should find Receive on dummy Change of Inventory", receive.PK, changeOfInventoryWithOrderAndReceive.Receive.PK);
		}

		#endregion

		#region TestOnUniversalEventAdded

		public void TestOnUniversalEventAdded_CancelOutOrder_ChangeOfOwnership()
		{
			TestOnUniversalEventAdded_CancelOutOrderCore(RecipientRoleType.BCO);
		}

		public void TestOnUniversalEventAdded_CancelOutOrder_ChangeOfRegime()
		{
			TestOnUniversalEventAdded_CancelOutOrderCore(RecipientRoleType.BCR);
		}

		void TestOnUniversalEventAdded_CancelOutOrderCore(RecipientRoleType changeOfInventoryRole)
		{
			var testFactory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(testFactory);

			var data = new TestDataSimpleEnvironment(testFactory);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			testFactory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.WD_CustomerReference = "Customer";
			helper.CreatePickNew(true, true, order);
			testFactory.Save();

			var universalEvent = new UniversalEvent();
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			universalEvent.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR }.ToRecipientRoleDetails() });
			universalEvent.EventType = ZArchitecture.Business.Events.CancelTheWarehouseJobCode;
			universalEvent.ContextCollection = new List<Context> { new Context { Type = nameof(UniversalEvent.ContextTypes.ClientReference), Value = "Customer" } };

			var logger = new TestErrorLogger { TopLevelDataObject = universalEvent };

			var bondedChangeOfInventory = new WhsBondedChangeOfInventory(testFactory);
			bondedChangeOfInventory.Order = order;
			var iManager = (IEventDataContextManager)bondedChangeOfInventory.GetUniversalDataContextManager();

			var uEvent = new UniversalEvent();
			uEvent.EventType = ZArchitecture.Business.Events.CancelTheWarehouseJobCode;
			iManager.OnUniversalEventAdded(logger, uEvent);

			AssertEquals(0, order.Logs.Find(helper.GetLogFilter(ZArchitecture.Business.Events.CancelledCode)).Length);

			universalEvent.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { changeOfInventoryRole }.ToRecipientRoleDetails() });
			iManager.OnUniversalEventAdded(logger, uEvent);

			AssertEquals(1, order.Logs.Find(helper.GetLogFilter(ZArchitecture.Business.Events.CancelledCode)).Length);
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
          <Type>CustomsDeclaration</Type>
        </DataSource>
      </DataSourceCollection>
      <DataTargetCollection>
        <DataTarget>
          <Type>WarehouseBondedChangeOfInventory</Type>
        </DataTarget>
      </DataTargetCollection>

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

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>BCO</Code>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>BNE</Code>
      <Name>BN - AUBNE</Name>
    </Branch>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ImporterDocumentaryAddress</AddressType>
        <OrganizationCode>CRAHOLSYD</OrganizationCode>
        <CompanyName>CRACKERJACK HOLDINGS</CompanyName>
        <Address1>1804 Fudrucker Way</Address1>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CustomsWarehouseAddress</AddressType>
        <OrganizationCode>INTHEMSYD</OrganizationCode>
        <CompanyName>In The Moment</CompanyName>
		<Address1>Unit 12, Level 3</Address1>
		<Address2>233 Here St</Address2>
		<City>ThereVille</City>
		<State>OfBliss</State>
		<Postcode>1233</Postcode>
		<Country>
          <Code>AU</Code>
          <Name>Australia</Name>
		</Country>
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

			var data = new TestDataForUniversal(Factory, new TestErrorLogger(), DataContextType.WarehouseBondedChangeOfInventory);
			data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(recipientRoles: new[] { new RecipientRoleDetail { Type = RecipientRoleType.BCO } });
			Factory.SaveForTesting();

			MockForDataContextManagerTestCase = new WarehouseCustomsDetailsProvidersMocks();
		}

		IDisposable MockForDataContextManagerTestCase;

		protected override void CleanUpForDataContextManagerTestCase()
		{
			MockForDataContextManagerTestCase?.Dispose(); // Customs have not completed their end, so cannot test end to end yet.
		}

		#endregion

		#region GetNewBusinessObjectForTesting

		protected override WhsBondedChangeOfInventory GetNewBusinessObjectForTesting()
		{
			return new WhsBondedChangeOfInventory(Factory.BOFactory);
		}

		#endregion

		#region GetDataContextManagerForTesting

		WarehouseBondedChangeOfInventoryDataContextManager GetDataContextManagerForTesting()
		{
			return (WarehouseBondedChangeOfInventoryDataContextManager)GetNewBusinessObjectForTesting().GetUniversalDataContextManager();
		}

		#endregion

		#region Helper

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory.BOFactory));
		WhsTestHelperFunctions helper;

		#endregion

		#endregion
	}
}
