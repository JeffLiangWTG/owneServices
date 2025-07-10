using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Workflow.Business.Test.Triggers.LineTriggers.IntegrationTests
{
	class TWToLTUniversalEventsTest : WorkflowTestCase
	{
		#region TestImportFULEventOnToDeliveryDepotInstruction

		public void TestImportFULEventOnToDeliveryDepotInstruction_RunSheetNumberOnReceiveHeader()
		{
			var nowDTO = ZDateTimeOffset.Now;
			var now = nowDTO.ToLocalZDateTime();
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			warehouse.WW_OA_WarehouseAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			var consignment = Helper.CreateConsignment("CN1");
			var pickupAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Multi, GlbBranch.CurrentBranch.OrgProxy.MainAddress, ConsignmentAddressStatus.Codes.Allocated, 2);
			var package1 = consignment.PackageJob.Packages.AddNew(Constants.PkgUnit.Package, "P1");
			var package2 = consignment.PackageJob.Packages.AddNew(Constants.PkgUnit.Package, "P2");

			var pickupAction = Helper.CreateConsignmentAction(pickupAddress, ActionTypes.Codes.PickUp);
			var deliveryAction = Helper.CreateConsignmentAction(deliveryAddress, ActionTypes.Codes.Delivery);

			var truck1 = CreateTruck("ABCD1");
			var truck2 = CreateTruck("XYZ", "V2", "YYYY");
			truck1.RQ_IsVehicle = true;
			truck2.RQ_IsVehicle = true;
			var runSheetWithMatchingTruck = Helper.CreateRunSheet(GlbBranch.CurrentBranch.OrgProxy, "RS1", null, nowDTO.AddHours(10), nowDTO.AddHours(14));
			var runSheet = Helper.CreateRunSheet(GlbBranch.CurrentBranch.OrgProxy, "RS2", null, nowDTO.AddHours(4), nowDTO.AddHours(2));
			Factory.Save();
			CreateConsignmentRunSheetEquipmentItem(runSheetWithMatchingTruck.PK, truck1);
			CreateConsignmentRunSheetEquipmentItem(runSheet.PK, truck2);

			var anotherInstruction = runSheet.RunSheetInstructions.AddNew();
			var deliveryInstruction = runSheet.RunSheetInstructions.AddNew();
			deliveryInstruction.K1_Sequence = 2;
			deliveryInstruction.Actions.Add(deliveryAction);

			var receiveUnit = CreateReceiveTransportationUnit(warehouse, "ABCD1", GlbBranch.CurrentBranch.OrgProxy.PK); // vehicle reference is for truck1
			CreateAdditionalReference(receiveUnit.PK, WhsItemReceiveTransportationUnitSchema.Constants.TableName, WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, "RS2");
			CreateReceiveConsignmentWithPackage(receiveUnit, warehouse, packageId: "P1", consignmentNumber: "CN1");

			receiveUnit.Logs.AddNew(new EventValue(AutoEvents.FreightUnloaded, eventTime: ZDateTimeOffset.Now.AddHours(12), deferFiringWorkflow: true));
			Factory.Save();
			AssertEquals("Precondition", true, deliveryInstruction.IsOwnDepot);
			AssertEquals("Precondition", GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Code, deliveryInstruction.Address.Address.OA_Code);
			AssertEquals("Precondition", true, deliveryInstruction.IsDeliveringConsignments);

			// create the workflow templates *after* we create the events to ensure that the adding of events does *not* fire, and instead fires when we run the log walker (mimic Glow)
			var templateTask = CreateTriggerToPublishUXMLEventInNewFactory_Receive();
			AssertEquals("Precondition: No Job queue tasks must be created for header.", 0, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, receiveUnit.PK)).Length);
			AssertEquals("Precondition", 0, deliveryInstruction.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.FreightUnloadedCode)).Length);

			// run log walker to create the process tasks
			RunLogWalker();
			AssertEquals("Header jobs must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, receiveUnit.PK)).Single().SJ_Status);
			receiveUnit.Reload();
			var wteEventQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, ProcessTask.WorkflowEventTriggerJobQueueName);
			wteEventQuery.AddToFilter(StmJobQueueSchema.SJ_ParentID, receiveUnit.WorkflowItems.Triggers[0].PK);
			AssertEquals("WTE event must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(wteEventQuery).Single().SJ_Status);
			deliveryInstruction.Reload();

			AssertEquals("Only one unloaded event must be added to delivery instruction.", 1, Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, deliveryInstruction.PK).AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.FreightUnloadedCode)).Length);
		}

		[TestDate(2021, 2, 1, 1, 1, 1, 100)]
		public void TestSetFieldTriggersOnDatesKeepsPrecision()
		{
			DisableConstraint(AutoWhsItemDispatchTransportationUnit.Schema.TableName, "Constraint_GateInLoadCompleteGateOutTimeHasNoSecondsOrMillSeconds");

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.TransitDispatchTransportationUnit;

			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "IFC";
			action.PQ_FieldName = "<IsGatedOut>";
			action.PQ_FieldValue = "true";

			var action2 = trigger.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = "IFC";
			action2.PQ_FieldName = "<WDH_GateInTime>";
			action2.PQ_FieldValue = "<GetEventLastDateTime(Z00)>";

			Factory.Save();

			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			warehouse.WW_OA_WarehouseAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;

			var truck1 = CreateTruck("ABCD1");
			truck1.RQ_IsVehicle = true;

			var dispatchTransportationUnit = CreateDispatchTransportationUnit(warehouse, truck1.RQ_ShortCode, GlbBranch.CurrentBranch.OrgProxy.PK);
			var dateTime = new ZDateTimeOffset(2021, 1, 1, 1, 1, 1, new TimeSpan(5, 0, 0)).AddMilliseconds(1).AddTicks(1);
			dispatchTransportationUnit.WDH_GateInTime = dateTime.AddDays(-1);
			dispatchTransportationUnit.WDH_LoadCompleteTime = dateTime;
			dispatchTransportationUnit.WDH_UnitType = TransportUnitTypes.Vehicle;

			Factory.Save();

			dispatchTransportationUnit.WorkflowItems.Reload(true);
			AssertEquals("Precondition: Trigger applied from template", 1, dispatchTransportationUnit.WorkflowItems.Triggers.Count);

			var eventTime = ZDateTimeOffset.Now.AddHours(12).AddMilliseconds(100).AddTicks(100);
			dispatchTransportationUnit.Logs.AddNew(new EventValue(AutoEvents.CustomisableEvent00, eventTime: eventTime, deferFiringWorkflow: false));

			AssertEquals(new ZDateTimeOffset(2021, 2, 1, 1, 1, 0, TimeSpan.Zero), dispatchTransportationUnit.WDH_GateOutTime);

			AssertEquals("GateInTime should not change.", dateTime.AddDays(-1), dispatchTransportationUnit.WDH_GateInTime);
		}

		ProcessTask CreateTriggerToPublishUXMLEventInNewFactory_Receive()
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.TransitReceiveTransportationUnit;
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "Test";
			templateTask.P9_Type = "TRG";
			templateTask.TriggerConditions.TriggerEventCode = AutoEvents.FreightUnloadedCode;

			var action = templateTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "XUE";
			action.PQ_Calc_TriggerParty = "TPC";
			newFactory.Save();
			return templateTask;
		}

		#endregion

		#region TestImportFLOEventOnToPickupDepotInstruction

		public void TestImportFLOEventOnToPickupDepotInstruction_RunSheetNumberOnDispatchHeader()
		{
			var nowDTO = ZDateTimeOffset.Now;
			var now = nowDTO.ToLocalZDateTime();
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			warehouse.WW_OA_WarehouseAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			var area = (IWhsArea)helper.CreateWhsArea(warehouse.PK, "A1");
			area.WA_WW_Whs = warehouse.PK;
			var row = helper.CreateRowAndGenerateLocations(warehouse, "R1");
			var location = (IWhsLocation)row.Locations[0];
			location.WLV_WW_Whs = warehouse.PK;
			location.WLV_WA_PutawayArea = area.PK;

			var consignment = Helper.CreateConsignment("1");
			var pickupAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Multi, GlbBranch.CurrentBranch.OrgProxy.MainAddress, ConsignmentAddressStatus.Codes.Allocated, 2);
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			var package1 = consignment.PackageJob.Packages.AddNew(Constants.PkgUnit.Package, "1");
			var package2 = consignment.PackageJob.Packages.AddNew(Constants.PkgUnit.Package, "2");

			var pickupAction = Helper.CreateConsignmentAction(pickupAddress, ActionTypes.Codes.PickUp);

			var truck1 = CreateTruck("ABCD1");
			var truck2 = CreateTruck("XYZ", "V2", "YYYY");
			truck1.RQ_IsVehicle = true;
			truck2.RQ_IsVehicle = true;
			var oldRunSheetForSameTruck = Helper.CreateRunSheet(GlbBranch.CurrentBranch.OrgProxy, "RS1", null, nowDTO.AddHours(10), nowDTO.AddHours(14));
			var runSheet = Helper.CreateRunSheet(GlbBranch.CurrentBranch.OrgProxy, "RS3", null, nowDTO.AddHours(2), nowDTO.AddHours(3));
			Factory.Save();
			CreateConsignmentRunSheetEquipmentItem(oldRunSheetForSameTruck.PK, truck1);
			CreateConsignmentRunSheetEquipmentItem(runSheet.PK, truck2);

			var anotherInstruction = runSheet.RunSheetInstructions.AddNew();
			var pickupInstruction = runSheet.RunSheetInstructions.AddNew();
			pickupInstruction.K1_Sequence = 2;
			pickupInstruction.Actions.Add(pickupAction);

			var receiveHeader = CreateReceiveTransportationUnit(warehouse, "AFCD1", GlbBranch.CurrentBranch.OrgProxy.PK);
			CreateReceiveConsignmentWithPackage(receiveHeader, warehouse, "1", "1");
			var dispatchUnit = CreateDispatchTransportationUnit(warehouse.PK, "SSS", "ABCD1");
			CreateAdditionalReference(dispatchUnit.PK, WhsItemDispatchTransportationUnitSchema.Constants.TableName, WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, "RS3");

			dispatchUnit.Logs.AddNew(new EventValue(AutoEvents.FreightLoaded, eventTime: ZDateTimeOffset.Now.AddHours(12), deferFiringWorkflow: true));
			Factory.Save();
			AssertEquals("Precondition", true, pickupInstruction.IsOwnDepot);
			AssertEquals("Precondition", GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Code, pickupInstruction.Address.Address.OA_Code);
			AssertEquals("Precondition", true, pickupInstruction.IsPickingUpConsignments);

			// create the workflow templates *after* we create the events to ensure that the adding of events does *not* fire, and instead fires when we run the log walker (mimic Glow)
			var templateTask = CreateTriggerToPublishUXMLEventInNewFactory_Dispatch();
			AssertEquals("Precondition: No Job queue tasks must be created for header.", 0, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, dispatchUnit.PK)).Length);
			AssertEquals("Precondition", 0, pickupInstruction.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.FreightLoadedCode)).Length);

			// run log walker to create the process tasks
			RunLogWalker();
			AssertEquals("Header jobs must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, dispatchUnit.PK)).Single().SJ_Status);
			dispatchUnit.Reload();
			var wteEventQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, ProcessTask.WorkflowEventTriggerJobQueueName);
			wteEventQuery.AddToFilter(StmJobQueueSchema.SJ_ParentID, dispatchUnit.WorkflowItems.Triggers[0].PK);
			var wteEventTrigger = Factory.Load<IQueuedLog>(wteEventQuery);
			AssertEquals("Log walker recurrs and processes the WTE log immediately.", 1, wteEventTrigger.Length);
			pickupInstruction.Reload();
			AssertEquals(0, package2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DepartureCode)).Length);

			AssertEquals("Only one loaded event must be added to pickup instruction.", 1, Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, pickupInstruction.PK).AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.FreightLoadedCode)).Length);
		}

		void CreateAdditionalReference(ZGuid parentPK, string parentTableName, string additionalReferenceType, string entryNum)
		{
			var entry = Factory.New<CusEntryNumber>();
			entry.CE_ParentID = parentPK;
			entry.CE_ParentTable = parentTableName;
			entry.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			entry.CE_EntryType = additionalReferenceType;
			entry.CE_EntryNum = entryNum;
		}

		WhsItemDispatchTransportationUnit CreateDispatchTransportationUnit(ZGuid warehousePK, string referenceNumber, string vehicleReference)
		{
			var header = Factory.NewWithValidTestData<WhsItemDispatchTransportationUnit>();
			header.WDH_VehicleReference = vehicleReference;
			header.WDH_WW_Warehouse = warehousePK;
			header.TransportCompany.OrganisationPK = GlbBranch.CurrentBranch.OrgProxy.PK;

			return header;
		}

		WhsItemDispatchConsignment CreateDispatchConsignmentForPackage(WhsItemDispatchTransportationUnit dispatchUnit, WhsItemPackageState packageState, string transportConsignmentNumber, IWhsLocation location, IWhsWarehouse warehouse, WhsItemDispatchLoadList loadList = null)
		{
			var dispatchConsignment = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			dispatchConsignment.WDC_ConsignmentID = "WDC12345";
			dispatchConsignment.WDC_WW_Warehouse = warehouse.PK;

			packageState.WPS_WDH_TransitDispatchHeader = dispatchUnit.PK;
			packageState.WPS_Status = "DEP";
			packageState.WPS_IsSecure = true;
			packageState.WPS_SecurityStatus = "SEC";
			packageState.WPS_WL_LastLocation = location.PK;
			packageState.WPS_WL_ReceiveLocation = location.PK;
			packageState.WPS_WDC_TransitDispatchConsignment = dispatchConsignment.PK;

			if (loadList != null)
			{
				packageState.WPS_WDL_LoadList = loadList.PK;
			}

			CreateAdditionalReference(dispatchConsignment.PK, WhsItemDispatchConsignmentSchema.Constants.TableName, WarehouseAdditionalReferenceTypes.Codes.ConsignmentNumber, transportConsignmentNumber);

			return dispatchConsignment;
		}

		ProcessTask CreateTriggerToPublishUXMLEventInNewFactory_Dispatch()
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.TransitDispatchTransportationUnit;
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "Test";
			templateTask.P9_Type = "TRG";
			templateTask.TriggerConditions.TriggerEventCode = "FLO";

			var action = templateTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "XUE";
			action.PQ_Calc_TriggerParty = "TPC";
			newFactory.Save();
			return templateTask;
		}

		#endregion

		#region TestImportARVPackageEventOnToDeliveryDepotInstruction

		public void TestImportARVPackageEventOnToDeliveryDepotInstruction()
		{
			var bookingParty = Helper.CreateOrganisation("BKP");
			TestImportARVPackageEventOnToDeliveryDepotInstructionCore(transportCompany: GlbCompany.CurrentCompany.OrgProxy, bookingParty: bookingParty, recipientParty: MessageRecipientPartyTypeList.Codes.DeliveryCartage);
		}

		public void TestImportARVPackageEventOnToDeliveryDepotInstructionWhichExistsInBookingPartySystem()
		{
			var transportCompany = Helper.CreateOrganisation("TRC");
			TestImportARVPackageEventOnToDeliveryDepotInstructionCore(transportCompany: transportCompany, bookingParty: GlbCompany.CurrentCompany.OrgProxy, recipientParty: MessageRecipientPartyTypeList.Codes.BookingParty);
		}

		void TestImportARVPackageEventOnToDeliveryDepotInstructionCore(OrgHeader transportCompany, OrgHeader bookingParty, string recipientParty)
		{
			var nowDTO = ZDateTimeOffset.Now;
			var now = nowDTO.ToLocalZDateTime();
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			warehouse.WW_OA_WarehouseAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			var consignment1 = Helper.CreateConsignment("CN1");
			var consignment2 = Helper.CreateConsignment("CN2");
			var pickupAddress = Helper.CreateConsignmentAddress(consignment1, ConsignmentAddressTypes.Codes.PickUp);
			var deliveryAddressForConsignment1 = Helper.CreateConsignmentAddress(consignment1, ConsignmentAddressTypes.Codes.Multi, GlbBranch.CurrentBranch.OrgProxy.MainAddress, ConsignmentAddressStatus.Codes.Allocated, 2);
			var deliveryAddressForConsignment2 = Helper.CreateConsignmentAddress(consignment2, ConsignmentAddressTypes.Codes.Multi, GlbBranch.CurrentBranch.OrgProxy.MainAddress, ConsignmentAddressStatus.Codes.Allocated, 1);
			var package1 = consignment1.PackageJob.Packages.AddNew(Constants.PkgUnit.Package, "P1");
			var package2 = consignment1.PackageJob.Packages.AddNew(Constants.PkgUnit.Package, "P2");

			var pickupAction = Helper.CreateConsignmentAction(pickupAddress, ActionTypes.Codes.PickUp);
			var deliveryAction1 = Helper.CreateConsignmentAction(deliveryAddressForConsignment1, ActionTypes.Codes.Delivery);
			var deliveryAction2 = Helper.CreateConsignmentAction(deliveryAddressForConsignment2, ActionTypes.Codes.Delivery);

			Helper.CreatePackageDivot(pickupAction, package1);
			Helper.CreatePackageDivot(pickupAction, package2);
			Helper.CreatePackageDivot(deliveryAction1, package1);
			Helper.CreatePackageDivot(deliveryAction1, package2);

			var truck = CreateTruck("ABCD1");
			truck.RQ_IsVehicle = true;
			var oldRunSheetForSameTruck = Helper.CreateRunSheet(GlbBranch.CurrentBranch.OrgProxy, "RS1", null, nowDTO.AddHours(-10), nowDTO.AddHours(-11));
			var runSheetWithSameStartAndEndTimes = Helper.CreateRunSheet(GlbBranch.CurrentBranch.OrgProxy, "RS2", null, nowDTO.AddHours(-9), nowDTO.AddHours(-8));
			var runSheet = Helper.CreateRunSheet(GlbBranch.CurrentBranch.OrgProxy, "RS3", null, nowDTO.AddHours(-9), nowDTO.AddHours(4));
			Factory.Save();
			CreateConsignmentRunSheetEquipmentItem(oldRunSheetForSameTruck.PK, truck);
			CreateConsignmentRunSheetEquipmentItem(runSheet.PK, truck);

			var anotherInstruction = runSheet.RunSheetInstructions.AddNew();
			var deliveryInstruction = runSheet.RunSheetInstructions.AddNew();
			deliveryInstruction.Actions.Add(deliveryAction1);
			deliveryInstruction.Actions.Add(deliveryAction2);
			deliveryInstruction.K1_Sequence = 2;

			var header = CreateReceiveTransportationUnit(warehouse, "ABCD1", transportCompany.PK);
			var packageState1InConsignment = CreateReceiveConsignmentWithPackage(header, warehouse, "P1", "CN1", bookingParty.PK);
			var packageId1InConsignment = packageState1InConsignment.Package;
			var receiveConsignment = packageState1InConsignment.ReceiveConsignment;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			receiveConsignment.Logs.AddNew(new EventValue(AutoEvents.AddedARecordToTheSystem, eventTime: ZDateTimeOffset.Now.AddHours(1), deferFiringWorkflow: true)); // Line Triggers need at least one event on the header level to create these trigger templates.
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			var parameters = new Dictionary<string, string>
			{
				{ EventConstants.EventReferenceParameters.Codes.Facility, EventConstants.Facilities.Code.Depot },
				{ EventConstants.EventReferenceParameters.Codes.Location, GlbBranch.CurrentBranch.OrgProxy.MainAddress.City },
				{ EventConstants.EventReferenceParameters.Codes.Department, Constants.Departments.TransitWarehouse }
			};
			packageState1InConsignment.Package.Logs.AddNew(new EventValue(AutoEvents.Arrival, eventTime: now.AddHours(12).ToOffset(), deferFiringWorkflow: true, parameters: parameters));
			Factory.Save();
			AssertEquals("Precondition", true, deliveryInstruction.IsOwnDepot);
			AssertEquals("Precondition", GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Code, deliveryInstruction.Address.Address.OA_Code);
			AssertEquals("Precondition", true, deliveryInstruction.IsDeliveringConsignments);

			// create the workflow templates *after* we create the events to ensure that the adding of events does *not* fire, and instead fires when we run the log walker (mimic Glow)
			var templateTask = CreatePackageLineTrigger(AutoEvents.ArrivalCode, recipientParty);
			deliveryAction1.Reload();
			package1.Reload();
			package2.Reload();
			AssertEquals("Precondition: No Job queue tasks must be created for header.", 0, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, receiveConsignment.PK)).Length);
			AssertEquals("Precondition", 0, deliveryAction1.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ArrivalCode)).Length);
			AssertEquals("Precondition", 0, package1.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ArrivalCode)).Length);
			AssertEquals("Precondition", 0, package2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ArrivalCode)).Length);

			// run log walker to create the process tasks
			RunLogWalker();
			AssertEquals("Line trigger must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, packageState1InConsignment.Package.PK)).Single().SJ_Status);
			receiveConsignment.Reload();
			deliveryAction1.Reload();
			package1.Reload();
			var logOnDeliveryAction = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, deliveryAction1.PK).AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.ArrivalCode)).Single();
			AssertEquals(EventConstants.Facilities.Code.Depot, logOnDeliveryAction.Parameters.Single(p => p.Key == EventConstants.EventReferenceParameters.Codes.Facility).Value);
			AssertEquals(Constants.Departments.TransitWarehouse, logOnDeliveryAction.Parameters.Single(p => p.Key == EventConstants.EventReferenceParameters.Codes.Department).Value);
			AssertEquals(now.AddHours(12), logOnDeliveryAction.SL_EventTime);

			var logOnPackage1 = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, deliveryAction1.PK).AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.ArrivalCode)).Single();
			AssertEquals(EventConstants.Facilities.Code.Depot, logOnDeliveryAction.Parameters.Single(p => p.Key == EventConstants.EventReferenceParameters.Codes.Facility).Value);
			AssertEquals(Constants.Departments.TransitWarehouse, logOnDeliveryAction.Parameters.Single(p => p.Key == EventConstants.EventReferenceParameters.Codes.Department).Value);
			AssertEquals(now.AddHours(12), logOnDeliveryAction.SL_EventTime);
			AssertEquals(0, Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, package2.PK).AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.ArrivalCode)).Length);
		}

		#endregion

		#region TestImportDEPPackageEventOnToPickupDepotInstruction

		public void TestImportDEPPackageEventOnToPickupDepotInstruction()
		{
			var bookingParty = Helper.CreateOrganisation("BKP");
			TestImportDEPPackageEventOnToPickupDepotInstructionCore(transportCompany: GlbBranch.CurrentBranch.OrgProxy, bookingParty: bookingParty, recipientParty: MessageRecipientPartyTypeList.Codes.PickupCartage);
		}

		public void TestImportDEPPackageEventOnToPickupDepotInstructionWhichExistsInBookingPartySystem()
		{
			var transportCompany = Helper.CreateOrganisation("TRC");
			TestImportDEPPackageEventOnToPickupDepotInstructionCore(transportCompany: transportCompany, bookingParty: GlbBranch.CurrentBranch.OrgProxy, recipientParty: MessageRecipientPartyTypeList.Codes.BookingParty);
		}

		void TestImportDEPPackageEventOnToPickupDepotInstructionCore(OrgHeader transportCompany, OrgHeader bookingParty, string recipientParty)
		{
			var nowDTO = ZDateTimeOffset.Now;
			var now = nowDTO.ToLocalZDateTime();
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			warehouse.WW_OA_WarehouseAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			var area = (IWhsArea)helper.CreateWhsArea(warehouse.PK, "A1");
			area.WA_WW_Whs = warehouse.PK;
			var row = helper.CreateRowAndGenerateLocations(warehouse, "R1");
			var location = (IWhsLocation)row.Locations[0];
			location.WLV_WW_Whs = warehouse.PK;
			location.WLV_WA_PutawayArea = area.PK;

			var consignment1 = Helper.CreateConsignment("CN1");
			var consignment2 = Helper.CreateConsignment("CN2");
			var pickupDepotAddressForConsignment1 = Helper.CreateConsignmentAddress(consignment1, ConsignmentAddressTypes.Codes.Multi, GlbBranch.CurrentBranch.OrgProxy.MainAddress);
			var pickupDepotAddressForConsignment2 = Helper.CreateConsignmentAddress(consignment2, ConsignmentAddressTypes.Codes.Multi, GlbBranch.CurrentBranch.OrgProxy.MainAddress);
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment1, ConsignmentAddressTypes.Codes.Delivery);
			var package1 = consignment1.PackageJob.Packages.AddNew(Constants.PkgUnit.Package, "P1");
			var package2 = consignment1.PackageJob.Packages.AddNew(Constants.PkgUnit.Package, "P2");

			var pickupAction1 = Helper.CreateConsignmentAction(pickupDepotAddressForConsignment1, ActionTypes.Codes.PickUp);
			var pickupAction2 = Helper.CreateConsignmentAction(pickupDepotAddressForConsignment2, ActionTypes.Codes.PickUp);
			var deliveryAction = Helper.CreateConsignmentAction(deliveryAddress, ActionTypes.Codes.Delivery);

			Helper.CreatePackageDivot(pickupAction1, package1);
			Helper.CreatePackageDivot(pickupAction1, package2);
			Helper.CreatePackageDivot(deliveryAction, package1);
			Helper.CreatePackageDivot(deliveryAction, package2);

			var truck = CreateTruck("ABCD1");
			truck.RQ_IsVehicle = true;
			var oldRunSheetForSameTruck = Helper.CreateRunSheet(GlbBranch.CurrentBranch.OrgProxy, "RS1", truck.PK, nowDTO.AddHours(-10), nowDTO.AddHours(-11));
			var runSheetWithSameStartAndEndTimes = Helper.CreateRunSheet(GlbBranch.CurrentBranch.OrgProxy, "RS2", null, nowDTO.AddHours(-9), nowDTO.AddHours(-8));
			var runSheet = Helper.CreateRunSheet(GlbBranch.CurrentBranch.OrgProxy, "RS3", truck.PK, nowDTO.AddHours(-9), nowDTO.AddHours(4));
			Factory.Save();
			CreateConsignmentRunSheetEquipmentItem(oldRunSheetForSameTruck.PK, truck);
			CreateConsignmentRunSheetEquipmentItem(runSheet.PK, truck);

			var pickupInstruction = runSheet.RunSheetInstructions.AddNew();
			pickupInstruction.Actions.Add(pickupAction1);
			pickupInstruction.K1_Sequence = 1;
			var anotherInstruction = runSheet.RunSheetInstructions.AddNew();
			anotherInstruction.K1_Sequence = 2;
			Factory.Save();

			var header = CreateReceiveTransportationUnit(warehouse, "XXXX", transportCompany.PK);
			var packageState = CreateReceiveConsignmentWithPackage(header, warehouse, "P1", "CN1", bookingParty.PK);
			var dispatchHeader = CreateDispatchTransportationUnit(warehouse.PK, "SSS", "ABCD1");
			var loadList = Factory.NewWithValidTestData<WhsItemDispatchLoadList>();
			loadList.WDL_JobID = "DLL001";
			loadList.WDL_WW_Warehouse = warehouse.PK;
			var dispatchConsignment = CreateDispatchConsignmentForPackage(dispatchHeader, packageState, "CN1", location, warehouse, loadList);
			CreateAdditionalReference(dispatchHeader.PK, WhsItemDispatchTransportationUnitSchema.Constants.TableName, WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, "RS3");

			var parameters = new Dictionary<string, string>
			{
				{ EventConstants.EventReferenceParameters.Codes.Facility, EventConstants.Facilities.Code.Depot },
				{ EventConstants.EventReferenceParameters.Codes.Location, GlbBranch.CurrentBranch.OrgProxy.MainAddress.City },
				{ EventConstants.EventReferenceParameters.Codes.Department, Constants.Departments.TransitWarehouse }
			};
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			packageState.ReceiveConsignment.Logs.AddNew(new EventValue(AutoEvents.AddedARecordToTheSystem, eventTime: ZDateTimeOffset.Now.AddHours(1), deferFiringWorkflow: true)); // Line Triggers need at least one event on the header level to create these trigger templates.
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			packageState.Package.Logs.AddNew(new EventValue(AutoEvents.Departure, eventTime: now.AddHours(12).ToOffset(), deferFiringWorkflow: true, parameters: parameters));
			Factory.Save();
			AssertEquals("Precondition", true, pickupInstruction.IsOwnDepot);
			AssertEquals("Precondition", GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Code, pickupInstruction.Address.Address.OA_Code);
			AssertEquals("Precondition", true, pickupInstruction.IsPickingUpConsignments);

			// create the workflow templates *after* we create the events to ensure that the adding of events does *not* fire, and instead fires when we run the log walker (mimic Glow)
			CreatePackageLineTrigger(AutoEvents.DepartureCode, recipientParty);
			pickupAction1.Reload();
			package1.Reload();
			package2.Reload();
			var receiveConsignment = packageState.ReceiveConsignment;
			AssertEquals("Precondition: No Job queue tasks must be created for header.", 0, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, receiveConsignment.PK)).Length);
			AssertEquals("Precondition", 0, pickupAction1.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DepartureCode)).Length);
			AssertEquals("Precondition", 0, package1.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DepartureCode)).Length);
			AssertEquals("Precondition", 0, package2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DepartureCode)).Length);

			// run log walker to create the process tasks
			RunLogWalker();
			AssertEquals("Line trigger must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, packageState.Package.PK)).Single().SJ_Status);
			receiveConsignment.Reload();
			pickupAction1.Reload();
			package1.Reload();
			var logOnPickupAction = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, pickupAction1.PK).AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.DepartureCode)).Single();
			AssertEquals(EventConstants.Facilities.Code.Depot, logOnPickupAction.Parameters.Single(p => p.Key == EventConstants.EventReferenceParameters.Codes.Facility).Value);
			AssertEquals(Constants.Departments.TransitWarehouse, logOnPickupAction.Parameters.Single(p => p.Key == EventConstants.EventReferenceParameters.Codes.Department).Value);
			AssertEquals(now.AddHours(12), logOnPickupAction.SL_EventTime);

			var logOnPickup1 = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, pickupAction1.PK).AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.DepartureCode)).Single();
			AssertEquals(EventConstants.Facilities.Code.Depot, logOnPickupAction.Parameters.Single(p => p.Key == EventConstants.EventReferenceParameters.Codes.Facility).Value);
			AssertEquals(Constants.Departments.TransitWarehouse, logOnPickupAction.Parameters.Single(p => p.Key == EventConstants.EventReferenceParameters.Codes.Department).Value);
			AssertEquals(now.AddHours(12), logOnPickupAction.SL_EventTime);
			AssertEquals(0, Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, package2.PK).AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.DepartureCode)).Length);
		}

		#endregion

		#region CreateTruck

		RefEquipment CreateTruck(string shortCode, string vehicleCode = "V1", string registrationCode = "XXXX")
		{
			var truck = Helper.CreateVehicleWithEquipmentType(vehicleCode, registrationCode);
			truck.RQ_ShortCode = shortCode;
			return truck;
		}

		#endregion

		#region CreateConsignmentRunSheetEquipmentItem

		void CreateConsignmentRunSheetEquipmentItem(ZGuid consignmentRunSheetPK, RefEquipment equipment)
		{
			var dtbEquipmentItemPK = new CargoWise.Database.TestFramework.ObjectModel.DtbEquipmentItem()
			{
				LTE_ParentID = consignmentRunSheetPK.ToGuid(),
				LTE_ParentTableCode = DtbConsignmentRunSheetSchema.Constants.Prefix,
				LTE_RQ_Equipment = equipment.PK.ToGuid(),
				LTE_RC_EquipmentType = equipment.RQ_RC_RoadContainerType.ToGuid()
			}.InsertAndReturnObject(TestConnection).PK;
		}

		#endregion

		#region CreateReceiveConsignment

		WhsItemPackageState CreateReceiveConsignmentWithPackage(WhsItemReceiveTransportationUnit transportationUnit, IWhsWarehouse warehouse, string packageId, string consignmentNumber, ZGuid? bookingPartyPK = null)
		{
			var receiveConsignment = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			receiveConsignment.WRC_WW_IntendedWarehouse = warehouse.PK;
			receiveConsignment.WRC_ConsignmentID = consignmentNumber;

			if (bookingPartyPK.HasValue)
			{
				receiveConsignment.BookingPartyDocAddress.OrganisationPK = bookingPartyPK.Value;
			}

			var packageJobOnReceiveConsignment = PkgPackageJob.LoadOrCreatePackageJob(receiveConsignment);
			var package = packageJobOnReceiveConsignment.Packages.AddNew(Constants.PkgUnit.Package, packageId);
			var packageState = receiveConsignment.PackageStates.AddNew();
			packageState.WPS_WRH_TransitReceiveHeader = transportationUnit.PK;
			packageState.WPS_Status = "ARV";
			packageState.WPS_KP_Package = package.PK;
			packageState.WPS_WW_Warehouse = warehouse.PK;
			packageState.WPS_WL_LastLocation = transportationUnit.WRH_WL_StagingLocation;

			CreateAdditionalReference(receiveConsignment.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, WarehouseAdditionalReferenceTypes.Codes.ConsignmentNumber, consignmentNumber);

			return packageState;
		}

		WhsItemReceiveTransportationUnit CreateReceiveTransportationUnit(IWhsWarehouse warehouse, string vehicleReference, ZGuid transportCompanyPK)
		{
			var transportationUnit = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			transportationUnit.WRH_VehicleReference = vehicleReference;
			transportationUnit.WRH_WW_Warehouse = warehouse.PK;
			transportationUnit.TransportCompany.OrganisationPK = transportCompanyPK;
			return transportationUnit;
		}

		WhsItemDispatchTransportationUnit CreateDispatchTransportationUnit(IWhsWarehouse warehouse, string vehicleReference, ZGuid transportCompanyPK)
		{
			var transportationUnit = Factory.NewWithValidTestData<WhsItemDispatchTransportationUnit>();
			transportationUnit.WDH_VehicleReference = vehicleReference;
			transportationUnit.WDH_WW_Warehouse = warehouse.PK;
			transportationUnit.TransportCompany.OrganisationPK = transportCompanyPK;
			return transportationUnit;
		}

		#endregion

		#region CreatePackageLineTrigger

		static ProcessTask CreatePackageLineTrigger(string eventCode, string triggerParty)
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.TransitReceiveConsignment;
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "Test";
			templateTask.P9_Type = "TRG";
			templateTask.P9_LineTriggerType = "PKG";
			templateTask.TriggerConditions.TriggerEventCode = eventCode;

			var action = templateTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "XUE";
			action.PQ_Calc_TriggerParty = triggerParty;
			newFactory.Save();
			return templateTask;
		}

		#endregion

		#region Implementation

		#region Helper

		protected TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}
		TransportConsignmentTestHelper helper;

		#endregion

		#region DisableConstraint

		static IDisposable DisableConstraint(string tableName, string constraintName, DbConnection connection = null)
		{
			var dbConn = connection ?? Db.Connection;
			return new DisposableAction(
				 () => dbConn.ExecuteNonQuery($"IF OBJECT_ID('{constraintName}', 'C') IS NOT NULL ALTER TABLE {tableName} NOCHECK CONSTRAINT[{constraintName}]"),
				 () => dbConn.ExecuteNonQuery($"IF OBJECT_ID('{constraintName}', 'C') IS NOT NULL ALTER TABLE {tableName} CHECK CONSTRAINT[{constraintName}]"));
		}

		#endregion

		#endregion
	}
}
