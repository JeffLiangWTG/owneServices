using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
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

namespace Enterprise.Workflow.Business.Test.Triggers
{
	class DtbConsignmentRunSheetTriggerTest : TriggerTestCase
	{
		#region TestRunSheetTriggerFire_EmailNotification

		public void TestRunSheetTriggerFire_EmailNotification()
		{
			var runSheet = Helper.CreateRunSheet();
			runSheet.Logs.AddNew(new EventValue(AutoEvents.Arrival, eventTime: ZDateTimeOffset.Now.AddDays(1), deferFiringWorkflow: true));
			Factory.Save();
			AssertEquals("Precondition.", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var templateTask = CreateTriggerInNewFactory(WorkflowDescriptors.DtbConsignmentRunSheetWorkflowDescriptorCode, "", AutoEvents.ArrivalCode);
			SetupEmailNotification(templateTask.ProcessTaskNotifications.AddNew());
			templateTask.Factory.Save();
			RunLogWalker();
			runSheet.Reload();
			RunLogWalker();

			AssertEquals("There should be one email sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#endregion

		#region TestRunSheetTriggerFire_ImportLoosePackageIdsIntoReceiveConsignment

		public void TestRunSheetTriggerFire_ImportLoosePackageIdsIntoReceiveConsignment()
		{
			var data = new TestData(Factory, ConsignmentHelper);
			var runSheet = data.RunSheet;

			runSheet.Logs.AddNew(new EventValue(AutoEvents.Arrival, eventTime: ZDateTimeOffset.Now, deferFiringWorkflow: true));
			// create the workflow templates *after* we create the events to ensure that the adding of events does *not* fire, and instead fires when we run the log walker (mimic Glow)
			var templateTask = CreateLineTriggerToPublishUXMLEventForPackageEventsInNewFactory();
			Factory.Save();

			var query = new ZQuery(WhsItemReceiveConsignmentSchema.WRC_WW_IntendedWarehouse, data.Warehouse.PK);
			var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(query);
			AssertEquals("Precondition: No receive consignment exists.", 0, receiveConsignments.Length);

			// run log walker to create the process tasks
			RunLogWalker();
			AssertEquals("Job queue tasks must be created for runsheet.", 1, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, runSheet.PK)).Length);
			runSheet.Reload();
			var wteEventQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, ProcessTask.WorkflowEventTriggerJobQueueName);
			wteEventQuery.AddToFilter(StmJobQueueSchema.SJ_ParentID, runSheet.WorkflowItems.Triggers[0].PK);
			var wteEventTrigger = Factory.Load<IQueuedLog>(wteEventQuery);
			AssertEquals("WTE event must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(wteEventQuery).Single().SJ_Status);

			var receiveConsignmentsAfterRunLogWalker = Factory.Load<WhsItemReceiveConsignment>(query);
			var receiveConsignment = receiveConsignmentsAfterRunLogWalker.Single();

			AssertEquals("Loose package ids must be converted to concrete packages.", 4, receiveConsignment.PackageStates.Count);
			var packages = receiveConsignment.PackageStates.Select(p => p.Package);
			AssertPackageInfo(packages.Single(p => p.KP_PackageID == "P1"), Constants.PkgUnit.Pallet, 1);
			AssertPackageInfo(packages.Single(p => p.KP_PackageID == "P2"), Constants.PkgUnit.Package, 1);
			AssertPackageInfo(packages.Single(p => p.KP_PackageID == "LP1"), Constants.PkgUnit.Package, 1);
			AssertPackageInfo(packages.Single(p => p.KP_PackageID == "LP2"), Constants.PkgUnit.Package, 1);
		}

		public void TestRunSheetTriggerFire_ImportLoosePackageIdsIntoReceiveConsignment_HasExistedReceiveConsignment()
		{
			var data = new TestData(Factory, ConsignmentHelper);
			var runSheet = data.RunSheet;

			#region First send UXML

			runSheet.Logs.AddNew(new EventValue(AutoEvents.Arrival, eventTime: ZDateTimeOffset.Now, deferFiringWorkflow: true));
			// create the workflow templates *after* we create the events to ensure that the adding of events does *not* fire, and instead fires when we run the log walker (mimic Glow)
			var templateTask = CreateLineTriggerToPublishUXMLEventForPackageEventsInNewFactory();
			Factory.Save();

			var query = new ZQuery(WhsItemReceiveConsignmentSchema.WRC_WW_IntendedWarehouse, data.Warehouse.PK);
			var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(query);
			AssertEquals("Precondition: No receive consignment exists.", 0, receiveConsignments.Length);

			// run log walker to create the process tasks
			RunLogWalker();
			AssertEquals("Job queue tasks must be created for runsheet.", 1, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, runSheet.PK)).Length);
			runSheet.Reload();
			var wteEventQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, ProcessTask.WorkflowEventTriggerJobQueueName);
			wteEventQuery.AddToFilter(StmJobQueueSchema.SJ_ParentID, runSheet.WorkflowItems.Triggers[0].PK);
			AssertEquals("WTE event must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(wteEventQuery).Single().SJ_Status);

			var receiveConsignmentsAfterRunLogWalker = Factory.Load<WhsItemReceiveConsignment>(query);
			var receiveConsignment = receiveConsignmentsAfterRunLogWalker.Single();
			var packagePKs = receiveConsignment.PackageStates.Select(p => p.Package.PK);
			AssertEquals("Loose package ids must be converted to concrete packages.", 4, receiveConsignment.PackageStates.Count);

			#endregion

			#region Send UXML again

			runSheet.Logs.AddNew(new EventValue(AutoEvents.Arrival, eventTime: ZDateTimeOffset.Now, deferFiringWorkflow: true));
			Factory.Save();

			RunLogWalker();
			runSheet.Reload();
			AssertEquals("New WTE event must be processed.", true, Factory.Load<IQueuedLog>(wteEventQuery).All(w => w.SJ_Status == JobQueueStatus.StatusProcessed));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveConsignmentsAfterReSendEvent = newFactory.Load<WhsItemReceiveConsignment>(query);
			var receiveConsignmentAfterReSendEvent = receiveConsignmentsAfterReSendEvent.Single();

			AssertEquals("Should not create new Packages.", 4, receiveConsignmentAfterReSendEvent.PackageStates.Count);
			var packagesAfterReSendEvent = receiveConsignmentAfterReSendEvent.PackageStates.Select(p => p.Package);

			AssertPackageInfo(packagesAfterReSendEvent.Single(p => p.KP_PackageID == "P1"), Constants.PkgUnit.Pallet, 1);
			AssertPackageInfo(packagesAfterReSendEvent.Single(p => p.KP_PackageID == "P2"), Constants.PkgUnit.Package, 1);
			AssertPackageInfo(packagesAfterReSendEvent.Single(p => p.KP_PackageID == "LP1"), Constants.PkgUnit.Package, 1);
			AssertPackageInfo(packagesAfterReSendEvent.Single(p => p.KP_PackageID == "LP2"), Constants.PkgUnit.Package, 1);

			#endregion
		}

		class TestData
		{
			public TestData(BusinessObjectFactory factory, TransportConsignmentTestHelper consignmentHelper)
			{
				var consignor = consignmentHelper.CreateOrganisation("CNR");
				var cfs = consignmentHelper.CreateOrganisation("CFS");
				var newCompany = factory.New<GlbCompany>();
				newCompany.GC_Code = "JGU";
				newCompany.GC_Name = "JGUGC";
				newCompany.GC_RN_NKCountryCode = "AU";
				newCompany.GC_RX_NKLocalCurrency = "AUD";
				newCompany.GC_OH_OrgProxy = cfs.PK;

				var helper = ObjectFactory.New<IWhsTransactionTestHelper>(factory);
				Warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1");
				Warehouse.WW_WarehouseType = "TRW";
				Warehouse.WW_OA_WarehouseAddress = cfs.MainAddress.PK;

				var consignment = consignmentHelper.CreateConsignment();
				var pickupAddress = consignmentHelper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageExporter, consignor.MainAddress);
				var deliveryAddress = consignmentHelper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS, cfs.MainAddress);
				var packageJob = consignment.PackageJob;
				var package1 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "P1");
				var package2 = packageJob.Packages.AddNew(Constants.PkgUnit.Package, "P2");
				var package3 = packageJob.Packages.AddNew(Constants.PkgUnit.Box, 5);
				var loosePackageID1 = packageJob.Packages.AddNew(Constants.PkgUnit.Box, "LP1");
				var loosePackageID2 = packageJob.Packages.AddNew(Constants.PkgUnit.Package, "LP2");
				packageJob.UnassignPackageIDs(new[] { loosePackageID1, loosePackageID2 });

				AssertEquals("Precondition: Package Header must be removed", ZGuid.Empty, loosePackageID1.KP_KPH_PackageHeader);
				AssertEquals("Precondition: Package Header must be removed", ZGuid.Empty, loosePackageID2.KP_KPH_PackageHeader);
				AssertContainsExactElementsInAnyOrder("Precondition: LoosePackageIDs must been created", new[] { "LP1", "LP2" }, packageJob.LoosePackageIDs.Select(l => l.KPH_PackageID));

				RunSheet = consignmentHelper.CreateRunSheet();
				var pickupInstruction = consignmentHelper.CreateRunSheetInstruction(RunSheet, pickupAddress.PickupAction);
				var deliveryInstruction = consignmentHelper.CreateRunSheetInstruction(RunSheet, deliveryAddress.DeliveryAction);

				factory.Save();

				AssertEquals("Precondition: Existing concrete packages", 5, packageJob.Packages.Count);
				AssertContainsExactElementsInAnyOrder("Precondition: Existing loose package IDs", new[] { "LP1", "LP2" }, packageJob.LoosePackageIDs.Select(l => l.KPH_PackageID));
			}

			public DtbConsignmentRunSheet RunSheet { get; }

			public IWhsWarehouse Warehouse { get; }
		}

		void AssertPackageInfo(PkgPackage package, string expectedPackType, int expectedPackQty)
		{
			AssertEquals(expectedPackType, package.KP_F3_NKPackType);
			AssertEquals(expectedPackQty, package.KP_PackageQty);
		}

		ProcessTask CreateLineTriggerToPublishUXMLEventForPackageEventsInNewFactory()
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.DtbConsignmentRunSheetWorkflowDescriptorCode;
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "Test";
			templateTask.P9_Type = "TRG";
			templateTask.TriggerConditions.TriggerEventCode = "ARV";

			var action = templateTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "XUS";
			action.PQ_Calc_TriggerParty = "ATW";
			action.PQ_TriggerPartyService = "TWR";
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
