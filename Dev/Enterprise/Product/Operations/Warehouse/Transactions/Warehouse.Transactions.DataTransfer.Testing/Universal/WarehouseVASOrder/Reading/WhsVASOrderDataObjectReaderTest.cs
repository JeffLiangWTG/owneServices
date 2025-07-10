using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.DataTransfer.CodeLists;
using Enterprise.ZArchitecture.Business;
using WarehouseDO = Enterprise.UniversalDataBuss.DataObjects.Universal.Warehouse;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsVASOrderDataObjectReaderTest : WhsUniversalTestCase
	{
		#region TestMatching_MatchesOnClientAndCustomerReference

		public void TestMatching_MatchesOnClientAndCustomerReference()
		{
			Data.SetupShipmentDataObjectAndEntitiesForVASOrderImportInDB();
			Data.ShipmentDataObject.Order.OrderNumber = "VASOrder123";

			var vasOrder = new WhsVASOrderDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Logger should have no problems logged.", false, Logger.HasErrors);
			AssertEquals("Logger should have no problems logged.", false, Logger.HasWarnings);
			AssertAddressContentMatches_CRAHOLSYD(vasOrder.Client.MainAddress);
			AssertEquals("Customer Reference should be set correctly.", "VASOrder123", vasOrder.WVO_CustomerReferenceNo);

			Data.ShipmentDataObject.Order.OrderNumber = "VASOrder456";
			var vasOrderWithDifferentCustomerRef = new WhsVASOrderDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotEquals("Should not have matched existing VAS Order.", vasOrder, vasOrderWithDifferentCustomerRef);
			AssertEquals("Customer Reference should be set correctly.", "VASOrder456", vasOrderWithDifferentCustomerRef.WVO_CustomerReferenceNo);
			AssertEquals("Logger should have no problems logged.", false, Logger.HasErrors);
			AssertEquals("Logger should have no problems logged.", false, Logger.HasWarnings);

			Data.ShipmentDataObject.Order.OrderNumber = "VASOrder123";
			Data.ShipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { GetNewAddressData_INTHEMSYD(nameof(OrganisationTypes.WarehouseClient)) });
			var vasOrderWithDifferentClient = new WhsVASOrderDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotEquals("Should not have matched existing VAS Order.", vasOrder, vasOrderWithDifferentClient);
			AssertAddressContentMatches_INTHEMSYD(vasOrderWithDifferentClient.Client.MainAddress);
			AssertEquals("Logger should have no problems logged.", false, Logger.HasErrors);
			AssertEquals("Logger should have no problems logged.", false, Logger.HasWarnings);

			Data.ShipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.VASOrderClientAddressDataObject_CRAHOLSYD });
			var matchingVASOrder = new WhsVASOrderDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Should have matched existing VAS Order.", vasOrder, matchingVASOrder);
			AssertEquals("Logger should have no problems logged.", false, Logger.HasErrors);
			AssertEquals("Logger should have no problems logged.", false, Logger.HasWarnings);
		}

		#endregion

		#region TestPopulateBusinessObject

		public void TestPopulateBusinessObject()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			data.Whs1.WW_WarehouseName = "SOMEWAREHOUSE";
			data.Whs1.Areas[0].WA_Name = "AREA4SERVICES";
			var client = new OrganisationDataObjectReader(GetNewAddressData_WUFSHIJNB(nameof(OrganisationTypes.WarehouseClient)), new TestErrorLogger(), Factory).GetMatchedOrNewForTesting().Header;
			Helper.CreateProductClientRelationShip(client, data.Part1);
			Helper.CreateProductClientRelationShip(client, data.Part2);
			Factory.SaveForTesting();

			// Setup DataObjects
			var vasOrderDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			vasOrderDO.Order = new Order(DefaultDataObjectWriterStrategy.TestInstance);
			vasOrderDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { GetNewAddressData_WUFSHIJNB(nameof(OrganisationTypes.WarehouseClient)) }));
			vasOrderDO.Order.Warehouse = new WarehouseDO { Code = "1", Name = "SOMEWAREHOUSE" };
			vasOrderDO.Order.StagingArea = "AREA4SERVICES";

			var vasOrderLine1DO = new OrderLine();
			var vasOrderLine2DO = new OrderLine();
			vasOrderDO.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>(new[] { vasOrderLine1DO, vasOrderLine2DO }));

			vasOrderLine1DO.LineNumber = 1;
			vasOrderLine1DO.Product = new Product { Code = "P1" };
			vasOrderLine1DO.OrderedQty = 5m;
			vasOrderLine1DO.PackingDate = today.AddDays(-1);
			vasOrderLine1DO.ExpiryDate = today.AddDays(7);
			vasOrderLine1DO.PartAttribute1 = "PA-1";
			vasOrderLine1DO.PartAttribute2 = "PA-2";
			vasOrderLine1DO.PartAttribute3 = "PA-3";

			vasOrderLine2DO.LineNumber = 5;
			vasOrderLine2DO.Product = new Product { Code = "P2" };
			vasOrderLine2DO.OrderedQty = 1m;
			Helper.CreateProductClientRelationShip(client, data.Part1);

			// Import DataObject
			var vasOrderBO = new WhsVASOrderDataObjectReader(vasOrderDO, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition", vasOrderBO);
			AssertAddressContentMatches_WUFSHIJNB(vasOrderBO.Client.MainAddress);
			AssertEquals("Service Area", "AREA4SERVICES", vasOrderBO.ServiceArea.WA_Name);
			AssertEquals("Should have added two lines.", 2, vasOrderBO.Lines.Count);

			var vasOrderLine1BO = vasOrderBO.Lines.SingleOrDefault(l => l.WVL_LineNumber == 1);
			var vasOrderLine2BO = vasOrderBO.Lines.SingleOrDefault(l => l.WVL_LineNumber == 5);

			AssertNotNull("Precondition", vasOrderLine1BO);
			AssertNotNull("Precondition", vasOrderLine2BO);

			AssertEquals("Product", data.Part1, vasOrderLine1BO.Product);
			AssertEquals("Quantity", 5m, vasOrderLine1BO.WVL_Quantity);
			AssertEquals("PackingDate", today.AddDays(-1), vasOrderLine1BO.WVL_PackingDate);
			AssertEquals("ExpiryDate", today.AddDays(7), vasOrderLine1BO.WVL_ExpiryDate);
			AssertEquals("PartAttribute1", "PA-1", vasOrderLine1BO.WVL_PartAttrib1);
			AssertEquals("PartAttribute2", "PA-2", vasOrderLine1BO.WVL_PartAttrib2);
			AssertEquals("PartAttribute3", "PA-3", vasOrderLine1BO.WVL_PartAttrib3);

			AssertEquals("Product", data.Part2, vasOrderLine2BO.Product);
			AssertEquals("Quantity", 1m, vasOrderLine2BO.WVL_Quantity);
			AssertEquals("PackingDate", ZDate.Empty, vasOrderLine2BO.WVL_PackingDate);
			AssertEquals("ExpiryDate", ZDate.Empty, vasOrderLine2BO.WVL_ExpiryDate);
			AssertEquals("PartAttribute1", ZString.Empty, vasOrderLine2BO.WVL_PartAttrib1);
			AssertEquals("PartAttribute2", ZString.Empty, vasOrderLine2BO.WVL_PartAttrib2);
			AssertEquals("PartAttribute3", ZString.Empty, vasOrderLine2BO.WVL_PartAttrib3);

			AssertNoExceptionThrown(() => Factory.SaveAtEndOfImport(Logger));
		}

		#endregion

		#region TestPopulateBusinessObject_AddsServiceRequestedEvent

		public void TestPopulateBusinessObject_AddsServiceRequestedEvent()
		{
			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = WorkflowDescriptors.WhsVASOrderWorkflowDescriptorCode;
			workflowTemplate.P0_GC = GlbCompany.CurrentCompany.PK;

			var trigger = workflowTemplate.WorkflowItems.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.ServiceRequestedCode;
			trigger.P9_Type = "TRG";
			trigger.P9_Description = "Test";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = ActionTypes.Codes.CreateInitialVASOrderTransfer;
			Factory.SaveForTesting();

			Data.SetupShipmentDataObjectAndEntitiesForVASOrderImportInDB();

			var vasOrder = new WhsVASOrderDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Should have added Service Requested Event to the VAS Order.", 1, vasOrder.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceRequestedCode).Count());
			Factory.SaveForTesting();
			AssertEquals("If workflow template is setup, the Trigger should be added to the VAS Order.", 1, vasOrder.WorkflowItems.Count);
			AssertEquals("Adding the Event to the VAS Order should fire the Trigger.",
				vasOrder.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceRequestedCode).Single().SL_EventTime, vasOrder.WorkflowItems[0].P9_ActualDate.ToZDateTime());

			var updatedVASOrder = new WhsVASOrderDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Should have updated the same VAS Order.", vasOrder, updatedVASOrder);
			AssertEquals("Should have added new Service Requested Event to the VAS Order.", 2, updatedVASOrder.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceRequestedCode).Count());
		}

		#endregion

		#region TestPopulateBusinessObject_AddsServiceRequestedEvent_AfterSettingClientAndWarehouse

		public void TestPopulateBusinessObject_AddsServiceRequestedEvent_AfterSettingClientAndWarehouse()
		{
			Data.SetupShipmentDataObjectAndEntitiesForVASOrderImportInDB();

			var client = Data.Orgs.CRAHOLSYD;
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = WorkflowDescriptors.WhsVASOrderWorkflowDescriptorCode;
			workflowTemplate.P0_GC = GlbCompany.CurrentCompany.PK;
			workflowTemplate.P0_WW = warehouse.PK;
			workflowTemplate.P0_OH_Client = client.PK;

			var trigger = workflowTemplate.WorkflowItems.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.ServiceRequestedCode;
			trigger.P9_Type = "TRG";
			trigger.P9_Description = "Test";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = ActionTypes.Codes.CreateInitialVASOrderTransfer;
			Factory.SaveForTesting();

			var vasOrder = new WhsVASOrderDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Should have added Service Requested Event to the VAS Order.", 1, vasOrder.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceRequestedCode).Count());
			Factory.SaveForTesting();
			AssertEquals("If workflow template is setup, the Trigger should be added to the VAS Order.", 1, vasOrder.WorkflowItems.Count);
			AssertEquals("Adding the Event to the VAS Order should fire the Trigger.",
				vasOrder.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceRequestedCode).Single().SL_EventTime, vasOrder.WorkflowItems[0].P9_ActualDate.ToZDateTime());

			var updatedVASOrder = new WhsVASOrderDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Should have updated the same VAS Order.", vasOrder, updatedVASOrder);
			AssertEquals("Should have added new Service Requested Event to the VAS Order.", 2, updatedVASOrder.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceRequestedCode).Count());
		}

		#endregion

		#region TestPopulateBusinessObject_CannotFindClientAddress

		public void TestPopulateBusinessObject_CannotFindClientAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			Factory.SaveForTesting();

			// Setup DataObject
			var vasOrderDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			vasOrderDO.Order = new Order();
			vasOrderDO.Order.Warehouse = new WarehouseDO { Code = "1" };
			vasOrderDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { CompanyName = "TEST" } }));

			// Import DataObjects
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Unable to match Client.", () => new WhsVASOrderDataObjectReader(vasOrderDO, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestPopulateBusinessObject_CannotFindServiceArea

		public void TestPopulateBusinessObject_CannotFindServiceArea()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var client = new OrganisationDataObjectReader(GetNewAddressData_CRAHOLSYD(nameof(OrganisationTypes.WarehouseClient)), new TestErrorLogger(), Factory).GetMatchedOrNewForTesting().Header;
			Factory.SaveForTesting();

			// Setup DataObject
			var vasOrderDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			vasOrderDO.Order = new Order();
			vasOrderDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { GetNewAddressData_CRAHOLSYD(nameof(OrganisationTypes.WarehouseClient)) }));
			vasOrderDO.Order.Warehouse = new WarehouseDO { Code = "A", Name = "SOMEWAREHOUSE" };
			vasOrderDO.Order.StagingArea = "AREA4SERVICES";

			// Import DataObjects
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Unable to match Warehouse: A - SOMEWAREHOUSE", () => new WhsVASOrderDataObjectReader(vasOrderDO, Logger, Factory).ReadIntoBusinessObject());

			data.Whs1.WW_WarehouseCode = "A";
			data.Whs1.WW_WarehouseName = "SOMEWAREHOUSE";
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Unable to match Service Area: AREA4SERVICES", () => new WhsVASOrderDataObjectReader(vasOrderDO, Logger, Factory).ReadIntoBusinessObject());

			data.Whs1.Areas[0].WA_Name = "AREA4SERVICES";
			AssertNoExceptionThrown(() => new WhsVASOrderDataObjectReader(vasOrderDO, Logger, Factory).ReadIntoBusinessObject());

			data.Whs1.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Unable to match Warehouse: A - SOMEWAREHOUSE", () => new WhsVASOrderDataObjectReader(vasOrderDO, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestPopulateBusinessObject_RepopulatesLines

		public void TestPopulateBusinessObject_RepopulatesLines()
		{
			var data = new TestDataForInventory(Factory.BOFactory);
			data.CreateSimpleInventory(false);
			var client = new OrganisationDataObjectReader(GetNewAddressData_CRAHOLSYD(nameof(OrganisationTypes.WarehouseClient)), new TestErrorLogger(), Factory).GetMatchedOrNewForTesting().Header;
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], client);
			vasOrder.WVO_JobID = "VO0001";
			Helper.CreateProductClientRelationShip(client, data.Part1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 2m);
			Factory.SaveForTesting();

			// Setup DataObjects
			var vasOrderDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			vasOrderDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { GetNewAddressData_CRAHOLSYD(nameof(OrganisationTypes.WarehouseClient)) }));
			vasOrderDO.DataContext = DataContextFactory.New();
			vasOrderDO.DataContext.AddDataTarget(DataContextType.WarehouseVASOrder, "VO0001");

			vasOrderDO.Order = new Order(DefaultDataObjectWriterStrategy.TestInstance);
			vasOrderDO.Order.Warehouse = new WarehouseDO { Code = data.Whs1.WW_WarehouseCode, Name = data.Whs1.WW_WarehouseName };
			vasOrderDO.Order.StagingArea = data.Whs1.Areas[0].WA_Name;

			var vasOrderLine1DO = new OrderLine();
			var vasOrderLine2DO = new OrderLine();
			vasOrderDO.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>(new[] { vasOrderLine1DO, vasOrderLine2DO }));

			vasOrderLine1DO.Product = new Product { Code = "P1" };
			vasOrderLine1DO.OrderedQty = 10m;

			vasOrderLine2DO.Product = new Product { Code = "P1" };
			vasOrderLine2DO.OrderedQty = 20m;

			// Attempt Import
			var factory2 = new UniversalObjectFactory();
			var vasOrderBO = new WhsVASOrderDataObjectReader(vasOrderDO, Logger, factory2).ReadIntoBusinessObject();
			factory2.SaveAtEndOfImport(Logger);

			AssertEquals(vasOrder.PK, vasOrderBO.PK);
			AssertEquals("precondition", 2, vasOrderBO.Lines.Count);
			AssertNotNull(vasOrderBO.Lines.SingleOrDefault(l => l.WVL_Quantity == 10m));
			AssertNotNull(vasOrderBO.Lines.SingleOrDefault(l => l.WVL_Quantity == 20m));

			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertEquals(2, factory3.Load<WhsVASOrder>(vasOrder.PK).Lines.Count);
		}

		#endregion

		#region TestPopulateBusinessObject_RejectsCommencedOrders

		public void TestPopulateBusinessObject_RejectsCommencedOrders()
		{
			Data.SetupShipmentDataObjectAndEntitiesForVASOrderImportInDB();

			var client = Data.Orgs.CRAHOLSYD;
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var product1 = Data.CreateProduct("P1");
			var product2 = Data.CreateProduct("P2");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product1, 4m, warehouse.FindLocation("A-1-1-1"), "");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", product1, 6m, warehouse.FindLocation("A-1-1-1"), "");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R3", product2, 5m, warehouse.FindLocation("A-1-1-1"), "");
			Factory.SaveForTesting();

			Data.ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>
			{
				Data.CreateOrderLine(product1, 10m),
				Data.CreateOrderLine(product2, 5m)
			});

			var vasOrder = new WhsVASOrderDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull(vasOrder);
			AssertEquals("No problems should be logged.", false, Logger.HasErrors);
			AssertEquals("No problems should be logged.", false, Logger.HasWarnings);
			Factory.SaveForTesting();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(new TestNotificationBuffer());
			AssertNotNull("Precondition: Initial Transfer successfully created.", initialTransfer);
			Factory.SaveForTesting();

			// Setup DataObjects
			var initialTransferPK = initialTransfer.PK;
			var updatedVASOrder = new WhsVASOrderDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Should update existing VAS Order.", vasOrder, updatedVASOrder);
			AssertEquals("Should delete Initial Transfer.", ZGuid.Empty, updatedVASOrder.WVO_WD_TransferIntoServiceArea);
			AssertEquals("Should delete Initial Transfer.", true, initialTransfer.IsDeleted);
			Factory.SaveForTesting();
			AssertNull("Should have deleted Initial Transfer.", new BusinessObjectFactory().Load<WhsTransfer>(initialTransferPK));
			AssertEquals("Should have deleted Initial Transfer.", true,
				new BusinessObjectFactory().Load<WhsVASOrder>(vasOrder.PK).WVO_WD_TransferIntoServiceArea.IsEmpty);

			var initialTransferWithServicedCommenced = vasOrder.GetOrCreateInitialTransfer(new TestNotificationBuffer());
			AssertNotNull("Precondition: Initial Transfer created.", initialTransferWithServicedCommenced);
			initialTransferWithServicedCommenced.Logs.AddNew(Events.ServiceCommenced);
			Factory.SaveForTesting();

			// Attempt Import should fail as Service Commenced Log has been added.
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Warehouse VAS Order WV00000001 could not be updated because it has been commenced.",
				() => new WhsVASOrderDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());

			// clear out initial Transfer
			vasOrder.WVO_WD_TransferIntoServiceArea = ZGuid.Empty;
			initialTransferWithServicedCommenced.Delete();
			Factory.SaveForTesting();

			var initialTransferWithFinalisedLine = vasOrder.GetOrCreateInitialTransfer(new TestNotificationBuffer());
			AssertNotNull("Precondition: Initial Transfer created.", initialTransferWithFinalisedLine);
			var transferLine = initialTransferWithFinalisedLine.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == product2.PK);
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			Factory.SaveForTesting();

			// Attempt Import should fail as one of the transfer lines is finalised.
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Warehouse VAS Order WV00000001 could not be updated because it has been commenced.",
				() => new WhsVASOrderDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestPopulateBusinessObject_NullOrderOrWarehouse

		public void TestPopulateBusinessObject_NullOrderOrWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			Factory.SaveForTesting();

			// Setup DataObject
			var vasOrderDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var client = new OrganisationDataObjectReader(GetNewAddressData_WUFSHIJNB(nameof(OrganisationTypes.WarehouseClient)), new TestErrorLogger(), Factory).GetMatchedOrNewForTesting().Header;
			vasOrderDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { GetNewAddressData_WUFSHIJNB(nameof(OrganisationTypes.WarehouseClient)) }));

			// Import DataObjects
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Could not import due to missing Warehouse Information.", () => new WhsVASOrderDataObjectReader(vasOrderDO, Logger, Factory).ReadIntoBusinessObject());

			vasOrderDO.Order = new Order();
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Could not import due to missing Warehouse Information.", () => new WhsVASOrderDataObjectReader(vasOrderDO, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestImport_CancelOrder

		public void TestImport_CancelOrder_Success_UpperCase() => Import_CancelOrder_Success_Core(false);

		public void TestImport_CancelOrder_Success_LowerCase() => Import_CancelOrder_Success_Core(true);

		void Import_CancelOrder_Success_Core(bool isLowerCase)
		{
			var data = new TestDataForInventory(Factory.BOFactory);
			data.CreateSimpleInventory(false);
			var client = new OrganisationDataObjectReader(GetNewAddressData_CRAHOLSYD(nameof(OrganisationTypes.WarehouseClient)), new TestErrorLogger(), Factory).GetMatchedOrNewForTesting().Header;
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], client);
			vasOrder.WVO_JobID = "VO0001";
			vasOrder.WVO_CustomerReferenceNo = "REF999";

			Factory.SaveForTesting();

			// Setup DataObjects
			var vasOrderDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			vasOrderDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { GetNewAddressData_CRAHOLSYD(nameof(OrganisationTypes.WarehouseClient)) }));
			vasOrderDO.DataContext = DataContextFactory.New();
			vasOrderDO.DataContext.AddDataTarget(DataContextType.WarehouseVASOrder, "VO0001");

			vasOrderDO.Order = new Order();
			vasOrderDO.Order.Warehouse = new WarehouseDO { Code = data.Whs1.WW_WarehouseCode, Name = data.Whs1.WW_WarehouseName };
			vasOrderDO.Order.StagingArea = data.Whs1.Areas[0].WA_Name;
			vasOrderDO.Order.OrderNumber = "REF125";
			vasOrderDO.Order.Status = new CodeDescriptionPair { Code = isLowerCase ? "can" : "CAN", Description = "Cancelled" };

			// Attempt Import
			var factory2 = new UniversalObjectFactory();
			var vasOrderBO = new WhsVASOrderDataObjectReader(vasOrderDO, Logger, factory2).ReadIntoBusinessObject();

			AssertNotNull(vasOrderBO);

			CombineAssertions(delegate
			{
				AssertEquals("Order is cancelled", true, vasOrderBO.IsCancelled);
				AssertEquals("No fields gets updated as the data object is cancelled.", "REF999", vasOrderBO.WVO_CustomerReferenceNo);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching WhsVASOrder.
Information - Populating WhsVASOrder...
Information - Matching 'WarehouseClient':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - The job WhsVASOrder - VO0001 has been canceled and no other updates were made.
Information - Updated Warehouse VAS Order VO0001 from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestImport_UpdateOrder_Fails_WhenOrderCancelled

		public void TestImport_UpdateOrder_Fails_WhenOrderCancelled()
		{
			var data = new TestDataForInventory(Factory.BOFactory);
			data.CreateSimpleInventory(false);
			var client = new OrganisationDataObjectReader(GetNewAddressData_CRAHOLSYD(nameof(OrganisationTypes.WarehouseClient)), new TestErrorLogger(), Factory).GetMatchedOrNewForTesting().Header;
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], client);
			vasOrder.WVO_JobID = "VO0001";
			vasOrder.WVO_CustomerReferenceNo = "REF999";

			Factory.SaveForTesting();

			// Setup DataObjects
			var vasOrderDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			vasOrderDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { GetNewAddressData_CRAHOLSYD(nameof(OrganisationTypes.WarehouseClient)) }));
			vasOrderDO.DataContext = DataContextFactory.New();
			vasOrderDO.DataContext.AddDataTarget(DataContextType.WarehouseVASOrder, "VO0001");

			vasOrderDO.Order = new Order();
			vasOrderDO.Order.Warehouse = new WarehouseDO { Code = data.Whs1.WW_WarehouseCode, Name = data.Whs1.WW_WarehouseName };
			vasOrderDO.Order.StagingArea = data.Whs1.Areas[0].WA_Name;
			vasOrderDO.Order.Status = new CodeDescriptionPair { Code = "CAN", Description = "Cancelled" };

			// Import and Cancel
			var factory2 = new UniversalObjectFactory();
			var vasOrderBO = new WhsVASOrderDataObjectReader(vasOrderDO, Logger, factory2).ReadIntoBusinessObject();

			// Update
			var vasOrderUpdateDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			vasOrderUpdateDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { GetNewAddressData_CRAHOLSYD(nameof(OrganisationTypes.WarehouseClient)) }));
			vasOrderUpdateDO.DataContext = DataContextFactory.New();
			vasOrderUpdateDO.DataContext.AddDataTarget(DataContextType.WarehouseVASOrder, "VO0001");

			vasOrderUpdateDO.Order = new Order();
			vasOrderUpdateDO.Order.Warehouse = new WarehouseDO { Code = data.Whs1.WW_WarehouseCode, Name = data.Whs1.WW_WarehouseName };
			vasOrderUpdateDO.Order.StagingArea = data.Whs1.Areas[0].WA_Name;
			vasOrderUpdateDO.Order.OrderNumber = "REF125";

			var vasOrderUpdateBO = new WhsVASOrderDataObjectReader(vasOrderUpdateDO, Logger, factory2).ReadIntoBusinessObject();

			AssertNotNull(vasOrderUpdateBO);

			CombineAssertions(delegate
			{
				AssertEquals("Order is cancelled", true, vasOrderUpdateBO.IsCancelled);
				AssertEquals("No fields gets updated as the data object is cancelled.", "REF999", vasOrderUpdateBO.WVO_CustomerReferenceNo);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching WhsVASOrder.
Information - Populating WhsVASOrder...
Information - Matching 'WarehouseClient':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - The job WhsVASOrder - VO0001 has been canceled and no other updates were made.
Information - Updated Warehouse VAS Order VO0001 from UniversalShipment.
Information - Successfully loaded matching WhsVASOrder.
Information - Populating WhsVASOrder...
Information - Matching 'WarehouseClient':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Error - The job WhsVASOrder - VO0001 has already been canceled and can not be updated.
Information - Updated Warehouse VAS Order VO0001 from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestImport_UpdateOrder_Fails_WhenOrderTransferred

		public void TestImport_UpdateOrder_Fails_WhenOrderTransferred()
		{
			Data.SetupShipmentDataObjectAndEntitiesForVASOrderImportInDB();

			var client = Data.Orgs.CRAHOLSYD;
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var product1 = Data.CreateProduct("P1");
			var product2 = Data.CreateProduct("P2");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product1, 4m, warehouse.FindLocation("A-1-1-1"), "");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", product1, 6m, warehouse.FindLocation("A-1-1-1"), "");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R3", product2, 5m, warehouse.FindLocation("A-1-1-1"), "");
			Factory.SaveForTesting();

			Data.ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>
			{
				Data.CreateOrderLine(product1, 10m),
				Data.CreateOrderLine(product2, 5m)
			});

			var vasOrder = new WhsVASOrderDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull(vasOrder);
			AssertEquals("No problems should be logged.", false, Logger.HasErrors);
			AssertEquals("No problems should be logged.", false, Logger.HasWarnings);
			Factory.SaveForTesting();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(new TestNotificationBuffer());
			AssertNotNull("Precondition: Initial Transfer successfully created.", initialTransfer);
			Factory.SaveForTesting();

			Data.ShipmentDataObject.Order.Status = new CodeDescriptionPair { Code = "CAN", Description = "Cancelled" };

			// Import and Cancel throws exception
			AssertExceptionThrown(typeof(DataObjectReadFailureException), $"You can only cancel VAS Orders with {WhsVASOrderStatuses.Descriptions.Entered} status.", () => new WhsVASOrderDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestImport_CancelOrder_Fails_WhenNewOrder

		public void TestImport_CancelOrder_Fails_WhenNewOrder()
		{
			var data = new TestDataForInventory(Factory.BOFactory);
			data.CreateSimpleInventory(false);
			var client = new OrganisationDataObjectReader(GetNewAddressData_CRAHOLSYD(nameof(OrganisationTypes.WarehouseClient)), new TestErrorLogger(), Factory).GetMatchedOrNewForTesting().Header;

			Factory.SaveForTesting();

			// Setup DataObjects
			var vasOrderDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			vasOrderDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { GetNewAddressData_CRAHOLSYD(nameof(OrganisationTypes.WarehouseClient)) }));

			vasOrderDO.Order = new Order();
			vasOrderDO.Order.Warehouse = new WarehouseDO { Code = data.Whs1.WW_WarehouseCode, Name = data.Whs1.WW_WarehouseName };
			vasOrderDO.Order.StagingArea = data.Whs1.Areas[0].WA_Name;
			vasOrderDO.Order.OrderNumber = "REF125";
			vasOrderDO.Order.Status = new CodeDescriptionPair { Code = "CAN", Description = "Cancelled" };

			AssertExceptionThrown(typeof(DataObjectReadFailureException), $"Cannot cancel a new VAS Order '{vasOrderDO.Order.OrderNumber}'.", () => new WhsVASOrderDataObjectReader(vasOrderDO, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);
	}
}
