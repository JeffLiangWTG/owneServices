using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Workflow.Business.Test.Triggers.LineTriggers.IntegrationTests
{
	class DtbConsignmentRunSheetTriggerTest : LineTriggerTestCase
	{
		#region TestCascadedEventTriggerFire

		#region TestCascadedEventTriggerFire_NTF

		public void TestCascadedEventTriggerFire_NTF()
		{
			AssertRunSheetInstructionLineTriggerFire((i) => SetupEmailNotification(i),
				(instruction) =>
				{
					instruction.Reload();
					instruction.Logs.GetAllLogs().Reload(true);
					AssertEquals("Event must be cascaded.", 1, instruction.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.ArrivalCode).Count());
					AssertEquals("There should be one email sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				});
		}

		#endregion

		#region TestCascadedEventTriggerFire_FLD

		public void TestCascadedEventTriggerFire_FLD()
		{
			AssertRunSheetInstructionLineTriggerFire((i) => SetupFieldNotification(i),
			instruction =>
			{
				instruction.Reload();
				instruction.Logs.GetAllLogs().Reload(true);
				AssertEquals("Event must be cascaded.", 1, instruction.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.ArrivalCode).Count());
				AssertEquals("TES", instruction.K1_FailureReason);
			});
		}

		protected override string FieldNameToUpdate => DtbConsignmentRunSheetInstructionSchema.Constants.K1_FailureReason;

		#endregion

		void AssertRunSheetInstructionLineTriggerFire(Action<ProcessTaskNotification> setupNotifications, Action<DtbConsignmentRunSheetInstruction> assertResults)
		{
			var runSheet = Helper.CreateRunSheet();
			var instruction = runSheet.RunSheetInstructions.AddNew();

			runSheet.Logs.AddNew(new EventValue(AutoEvents.Arrival, eventTime: ZDateTimeOffset.Now.AddDays(1), deferFiringWorkflow: true));

			Factory.Save();

			// create the workflow templates *after* we create the events to ensure that the adding of events does *not* fire, and instead fires when we run the log walker (mimic Glow)
			var templateTask = CreateTriggerInNewFactory(WorkflowDescriptors.DtbConsignmentRunSheetWorkflowDescriptorCode, TriggerLineTypes.Codes.RunSheetInstruction, AutoEvents.ArrivalCode);
			setupNotifications(templateTask.ProcessTaskNotifications.AddNew());
			templateTask.Factory.Save();

			AssertEquals("Precondition: No Job queue tasks must be created for runsheet.", 0, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, runSheet.PK)).Length);
			AssertEquals("Precondition: No Job queue tasks must be created for runsheet instruction.", 0, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, runSheet.RunSheetInstructions[0].PK)).Length);
			instruction.Reload();
			AssertEquals("Precondition: Instruction must not have an arrival event.", 0, instruction.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.ArrivalCode).Count());

			RunLogWalker(); // Run the logwalker enough times
			RunLogWalker();
			runSheet.Reload();
			assertResults(instruction);
			runSheet.Reload();
			AssertEquals(1, runSheet.WorkflowItems.Triggers.Count);
		}

		#endregion

		#region TestRunSheetTriggerFire_CreatingReceiveConsignments

		public void TestRunSheetTriggerFire_CreatingReceiveConsignments()
		{
			var nowDTO = ZDateTimeOffset.Now;
			var now = nowDTO.ToLocalZDateTime();
			var consignor = ConsignmentHelper.CreateOrganisation("CNR");
			var cfs = ConsignmentHelper.CreateOrganisation("CFS");
			var company = ConsignmentHelper.CreateCompany("DGF", "DGFGC", cfs);
			company.GC_RN_NKCountryCode = "AU";
			company.GC_RX_NKLocalCurrency = "AUD";

			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			warehouse.WW_WarehouseType = "TRW";
			warehouse.WW_OA_WarehouseAddress = cfs.MainAddress.PK;

			var consignment = ConsignmentHelper.CreateConsignment("CN1");
			var pickupAddress = ConsignmentHelper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageExporter, consignor.MainAddress);
			var deliveryAddress = ConsignmentHelper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS, cfs.MainAddress);
			var packageJob = consignment.PackageJob;
			var package = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "P1");

			var runSheet = ConsignmentHelper.CreateRunSheet(GlbBranch.CurrentBranch.OrgProxy, "RS1", null, nowDTO.AddHours(4), nowDTO.AddHours(5));
			var pickupInstruction = ConsignmentHelper.CreateRunSheetInstruction(runSheet, pickupAddress.PickupAction);
			var deliveryInstruction = ConsignmentHelper.CreateRunSheetInstruction(runSheet, deliveryAddress.DeliveryAction);

			CreateEquipmentItem(runSheet, "A", false);
			CreateEquipmentItem(runSheet, "B", true);
			CreateEquipmentItem(runSheet, "C", true);
			CreateEquipmentItem(runSheet, "D", false);

			Factory.Save();

			runSheet.Logs.AddNew(new EventValue(AutoEvents.Arrival, eventTime: nowDTO, deferFiringWorkflow: true));
			Factory.Save();

			var templateTask = CreateLineTriggerToPublishUXMLEventForPackageEventsInNewFactory("ATW", "TWR", AutoEvents.ArrivalCode);
			Factory.Save();

			var query = new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "CN1");
			AssertEquals("Precondition: There must be no receive consignments in the system.", 0, Factory.Load<WhsItemReceiveConsignment>(query).Length);

			RunLogWalker();
			AssertEquals("RunSheet jobs must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, runSheet.PK)).Single().SJ_Status);
			runSheet.Reload();
			var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(query);
			var receiveConsignment = receiveConsignments.Single();
			var packageState = receiveConsignment.PackageStates.Single();
			var receiveASN = packageState.ReceiveASN;
			AssertNotNull("Expected packing group must be created.", receiveASN);
			AssertEquals("Vehicle reference must be updated.", "B", receiveASN.WRP_VehicleReference);
			AssertEquals("No receive headers must be created in the system.", 0, new BusinessObjectFactory().Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Length);

			var additionalReference = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, receiveASN.PK)).Single();
			AssertEquals(WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, additionalReference.CE_EntryType);
			AssertEquals("Run sheet number must be stored as transport reference on receive ASN", "RS1", additionalReference.CE_EntryNum);
		}

		void CreateEquipmentItem(DtbConsignmentRunSheet runSheet, string shortCode, bool isVehicle)
		{
			var equipment = Helper.CreateVehicleWithEquipmentType($"XX {shortCode}", shortCode);
			equipment.RQ_ShortCode = shortCode;
			equipment.RQ_IsVehicle = isVehicle;
			Factory.Save();
			CreateConsignmentRunSheetEquipmentItem(runSheet.PK, equipment);
		}

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

		#region TestRunSheetTriggerFire_CreatingDispatchConsignments

		public void TestRunSheetTriggerFire_CreatingDispatchConsignments()
		{
			var nowDTO = ZDateTimeOffset.Now;
			var now = nowDTO.ToLocalZDateTime();
			var consignor = ConsignmentHelper.CreateOrganisation("CNR");
			var cfs = ConsignmentHelper.CreateOrganisation("CFS");
			var consignee = ConsignmentHelper.CreateOrganisation("CNE");
			var companyForCFS = ConsignmentHelper.CreateCompany("JGU", "JGUGC", cfs);
			companyForCFS.GC_RN_NKCountryCode = "AU";
			companyForCFS.GC_RX_NKLocalCurrency = "AUD";

			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			warehouse.WW_WarehouseType = "TRW";
			warehouse.WW_OA_WarehouseAddress = cfs.MainAddress.PK;

			var consignment = ConsignmentHelper.CreateConsignment("CN1");
			var pickupAddress = ConsignmentHelper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageExporter, consignor.MainAddress);
			var deliveryAddressForCFS = ConsignmentHelper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Multi, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS, cfs.MainAddress);
			var pickupAddressForCFS = ConsignmentHelper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Multi, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS, cfs.MainAddress);
			var deliveryAddress = ConsignmentHelper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageImporter, consignee.MainAddress);
			var packageJob = consignment.PackageJob;
			var package = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "P1");

			var runSheet = ConsignmentHelper.CreateRunSheet(GlbBranch.CurrentBranch.OrgProxy, "RS1", null, nowDTO.AddHours(4), nowDTO.AddHours(5));
			var pickupInstruction = ConsignmentHelper.CreateRunSheetInstruction(runSheet, pickupAddressForCFS.PickupAction);
			var deliveryInstruction = ConsignmentHelper.CreateRunSheetInstruction(runSheet, deliveryAddress.DeliveryAction);
			CreateEquipmentItem(runSheet, "A", false);
			CreateEquipmentItem(runSheet, "B", true);
			CreateEquipmentItem(runSheet, "C", true);
			CreateEquipmentItem(runSheet, "D", false);

			Factory.Save();

			var receiveUnit = CreateReceiveTransportationUnit(warehouse, "B");
			CreateReceiveConsignmentWithPackage(receiveUnit, warehouse, "P1", "CN1", consignor, consignee);

			runSheet.Logs.AddNew(new EventValue(AutoEvents.Departure, eventTime: now.ToOffset(), deferFiringWorkflow: true));
			Factory.Save();

			var templateTask = CreateLineTriggerToPublishUXMLEventForPackageEventsInNewFactory("DTW", "TWD", AutoEvents.DepartureCode);
			Factory.Save();

			var query = new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID, "CN1");
			AssertEquals("Precondition: There must be no dispatch consignments in the system.", 0, Factory.Load<WhsItemDispatchConsignment>(query).Length);

			RunLogWalker();
			AssertEquals("RunSheet jobs must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, runSheet.PK)).Single().SJ_Status);
			runSheet.Reload();

			var newFactory = new BusinessObjectFactory();
			var dispatchConsignments = newFactory.Load<WhsItemDispatchConsignment>(query);
			var dispatchConsignment = dispatchConsignments.Single();
			var packageState = dispatchConsignment.PackageStates.Single();
			AssertNull("Dispatch transportation unit must not be attached to package state.", packageState.DispatchTransportationUnit);

			var dispatchUnit = newFactory.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			AssertEquals("Vehicle reference must be updated on dispatch transportation unit.", "B", dispatchUnit.WDH_VehicleReference);

			var additionalReference = newFactory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, dispatchUnit.PK)).Single();
			AssertEquals(WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, additionalReference.CE_EntryType);
			AssertEquals("Run sheet number must be stored as transport reference on dispatch transportation unit.", "RS1", additionalReference.CE_EntryNum);
		}

		WhsItemPackageState CreateReceiveConsignmentWithPackage(WhsItemReceiveTransportationUnit receiveUnit, IWhsWarehouse warehouse, string packageId, string consignmentNumber, OrgHeader consignor, OrgHeader consignee)
		{
			var receiveASN = Factory.New<WhsItemReceiveASN>();
			receiveASN.WRP_WW_IntendedWarehouse = warehouse.PK;

			var receiveConsignment = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			receiveConsignment.WRC_WW_IntendedWarehouse = warehouse.PK;
			receiveConsignment.WRC_ConsignmentID = consignmentNumber;
			receiveConsignment.ConsignorDocAddress.OrganisationPK = consignor.PK;
			receiveConsignment.ConsigneeDocAddress.OrganisationPK = consignee.PK;

			var packageJobOnReceiveConsignment = PkgPackageJob.LoadOrCreatePackageJob(receiveConsignment);
			var package = packageJobOnReceiveConsignment.Packages.AddNew(Constants.PkgUnit.Pallet, packageId);
			var packageState = receiveConsignment.PackageStates.AddNew();
			packageState.WPS_WRH_TransitReceiveHeader = receiveUnit.PK;
			packageState.WPS_Status = "ARV";
			packageState.WPS_KP_Package = package.PK;
			packageState.WPS_WW_Warehouse = warehouse.PK;
			packageState.WPS_WRP_ReceiveExpectedPacking = receiveASN.PK;
			packageState.WPS_WL_LastLocation = receiveUnit.WRH_WL_StagingLocation;

			return packageState;
		}

		WhsItemReceiveTransportationUnit CreateReceiveTransportationUnit(IWhsWarehouse warehouse, string vehicleReference)
		{
			var unit = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			unit.WRH_VehicleReference = vehicleReference;
			unit.WRH_WW_Warehouse = warehouse.PK;
			unit.TransportCompany.OrganisationPK = GlbBranch.CurrentBranch.OrgProxy.PK;
			return unit;
		}

		ProcessTask CreateLineTriggerToPublishUXMLEventForPackageEventsInNewFactory(string relatedParty, string relatedPartyService, string eventCode)
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.DtbConsignmentRunSheetWorkflowDescriptorCode;
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "Test";
			templateTask.P9_Type = "TRG";
			templateTask.TriggerConditions.TriggerEventCode = eventCode;

			var action = templateTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "XUS";
			action.PQ_Calc_TriggerParty = relatedParty;
			action.PQ_TriggerPartyService = relatedPartyService;
			newFactory.Save();
			return templateTask;
		}

		#endregion

		#region Helper

		protected TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}
		TransportBookingConsignmentTestHelper helper;

		protected TransportConsignmentTestHelper ConsignmentHelper
		{
			get { return consignmentHelper ?? (consignmentHelper = new TransportConsignmentTestHelper(Factory)); }
		}
		TransportConsignmentTestHelper consignmentHelper;

		#endregion
	}
}
