using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business.Test.Triggers.IntegrationTests.TransferHeaderTriggers
{
	class TWToCustomsUniversalEventsTest : WorkflowTestCase
	{
		#region TestUnloadReceiveTransportationUnit_TriggersFULEvent

		public void TestUnloadReceiveTransportationUnit_TriggersFULEvent()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			warehouse.WW_OA_WarehouseAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;

			var row = helper.CreateRowAndGenerateLocations(warehouse, "DOCK", 2, 2);
			var location = row.Locations.OfType<IWhsLocation>().First(l => l.WLV_LocationString == "DOCK-1-1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			rtu.Logs.AddNew(new EventValue(AutoEvents.FreightUnloaded, eventTime: ZDateTimeOffset.Now, deferFiringWorkflow: true));
			Factory.Save();

			// create the workflow templates *after* we create the events to ensure that the adding of events does *not* fire, and instead fires when we run the log walker (mimic Glow)
			var templateTask = CreateTriggerToPublishUXMLEventInNewFactory(AutoEvents.FreightUnloadedCode, WorkflowDescriptors.TransitReceiveTransportationUnit);
			AssertEquals("Precondition: No Job queue tasks must be created for header.", 0, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, rtu.PK)).Length);

			RunLogWalker();
			AssertEquals("Header jobs must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, rtu.PK)).Single().SJ_Status);
			rtu.Reload();
			var eventQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, "TasksAndMilestonesLoader");
			eventQuery.AddToFilter(StmJobQueueSchema.SJ_ParentID, rtu.PK);
			AssertEquals("TasksAndMilestonesLoader event must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(eventQuery).Single().SJ_Status);
		}

		#endregion

		#region TestLoadDispatchTransportationUnit_TriggersFLOEvent

		public void TestLoadDispatchTransportationUnit_TriggersFLOEvent()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			warehouse.WW_OA_WarehouseAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;

			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			dtu.Logs.AddNew(new EventValue(AutoEvents.FreightLoaded, eventTime: ZDateTimeOffset.Now, deferFiringWorkflow: true));
			Factory.Save();

			// create the workflow templates *after* we create the events to ensure that the adding of events does *not* fire, and instead fires when we run the log walker (mimic Glow)
			var templateTask = CreateTriggerToPublishUXMLEventInNewFactory(AutoEvents.FreightLoadedCode, WorkflowDescriptors.TransitDispatchTransportationUnit);
			AssertEquals("Precondition: No Job queue tasks must be created for header.", 0, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, dtu.PK)).Length);

			RunLogWalker();
			AssertEquals("Header jobs must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, dtu.PK)).Single().SJ_Status);
			dtu.Reload();
			var eventQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, "TasksAndMilestonesLoader");
			eventQuery.AddToFilter(StmJobQueueSchema.SJ_ParentID, dtu.PK);
			AssertEquals("TasksAndMilestonesLoader event must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(eventQuery).Single().SJ_Status);
		}

		#endregion

		#region TestReceiveTransportationUnitArrival_TriggersGINEvent

		public void TestSetGateInTimeOnReceiveTransportationUnit_TriggersGINEvent()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			warehouse.WW_OA_WarehouseAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;

			var row = helper.CreateRowAndGenerateLocations(warehouse, "DOCK", 2, 2);
			var location = row.Locations.OfType<IWhsLocation>().First(l => l.WLV_LocationString == "DOCK-1-1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			rtu.Logs.AddNew(new EventValue(AutoEvents.GateIn, eventTime: ZDateTimeOffset.Now, deferFiringWorkflow: true));
			Factory.Save();

			// create the workflow templates *after* we create the events to ensure that the adding of events does *not* fire, and instead fires when we run the log walker (mimic Glow)
			var templateTask = CreateTriggerToPublishUXMLEventInNewFactory(AutoEvents.GateInCode, WorkflowDescriptors.TransitReceiveTransportationUnit);
			AssertEquals("Precondition: No Job queue tasks must be created for header.", 0, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, rtu.PK)).Length);

			RunLogWalker();
			AssertEquals("Header jobs must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, rtu.PK)).Single().SJ_Status);
			rtu.Reload();
			var eventQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, "TasksAndMilestonesLoader");
			eventQuery.AddToFilter(StmJobQueueSchema.SJ_ParentID, rtu.PK);
			AssertEquals("TasksAndMilestonesLoader event must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(eventQuery).Single().SJ_Status);
		}

		#endregion

		#region TestReceiveTransportationUnitDeparture_TriggersGOUEvent

		public void TestReceiveTransportationUnitDeparture_TriggersGOUEvent()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			warehouse.WW_OA_WarehouseAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;

			var row = helper.CreateRowAndGenerateLocations(warehouse, "DOCK", 2, 2);
			var location = row.Locations.OfType<IWhsLocation>().First(l => l.WLV_LocationString == "DOCK-1-1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			rtu.Logs.AddNew(new EventValue(AutoEvents.GateOut, eventTime: ZDateTimeOffset.Now, deferFiringWorkflow: true));
			Factory.Save();

			// create the workflow templates *after* we create the events to ensure that the adding of events does *not* fire, and instead fires when we run the log walker (mimic Glow)
			var templateTask = CreateTriggerToPublishUXMLEventInNewFactory(AutoEvents.GateOutCode, WorkflowDescriptors.TransitReceiveTransportationUnit);
			AssertEquals("Precondition: No Job queue tasks must be created for header.", 0, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, rtu.PK)).Length);

			RunLogWalker();
			AssertEquals("Header jobs must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, rtu.PK)).Single().SJ_Status);
			rtu.Reload();
			var eventQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, "TasksAndMilestonesLoader");
			eventQuery.AddToFilter(StmJobQueueSchema.SJ_ParentID, rtu.PK);
			AssertEquals("TasksAndMilestonesLoader event must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(eventQuery).Single().SJ_Status);
		}

		#endregion

		#region TestDispatchTransportationUnitArrival_TriggersGINEvent

		public void TestDispatchTransportationUnitArrival_TriggersGINEvent()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			warehouse.WW_OA_WarehouseAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;

			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			dtu.Logs.AddNew(new EventValue(AutoEvents.GateIn, eventTime: ZDateTimeOffset.Now, deferFiringWorkflow: true));
			Factory.Save();

			// create the workflow templates *after* we create the events to ensure that the adding of events does *not* fire, and instead fires when we run the log walker (mimic Glow)
			var templateTask = CreateTriggerToPublishUXMLEventInNewFactory(AutoEvents.GateInCode, WorkflowDescriptors.TransitDispatchTransportationUnit);
			AssertEquals("Precondition: No Job queue tasks must be created for header.", 0, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, dtu.PK)).Length);

			RunLogWalker();
			AssertEquals("Header jobs must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, dtu.PK)).Single().SJ_Status);
			dtu.Reload();
			var eventQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, "TasksAndMilestonesLoader");
			eventQuery.AddToFilter(StmJobQueueSchema.SJ_ParentID, dtu.PK);
			AssertEquals("TasksAndMilestonesLoader event must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(eventQuery).Single().SJ_Status);
		}

		#endregion

		#region TestDispatchTransportationUnitDeparture_TriggersGOUEvent

		public void TestDispatchTransportationUnitDeparture_TriggersGOUEvent()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			warehouse.WW_OA_WarehouseAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;

			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			dtu.Logs.AddNew(new EventValue(AutoEvents.GateOut, eventTime: ZDateTimeOffset.Now, deferFiringWorkflow: true));
			Factory.Save();

			// create the workflow templates *after* we create the events to ensure that the adding of events does *not* fire, and instead fires when we run the log walker (mimic Glow)
			var templateTask = CreateTriggerToPublishUXMLEventInNewFactory(AutoEvents.GateOutCode, WorkflowDescriptors.TransitDispatchTransportationUnit);
			AssertEquals("Precondition: No Job queue tasks must be created for header.", 0, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, dtu.PK)).Length);

			RunLogWalker();
			AssertEquals("Header jobs must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, dtu.PK)).Single().SJ_Status);
			dtu.Reload();
			var eventQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, "TasksAndMilestonesLoader");
			eventQuery.AddToFilter(StmJobQueueSchema.SJ_ParentID, dtu.PK);
			AssertEquals("TasksAndMilestonesLoader event must be processed.", JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(eventQuery).Single().SJ_Status);
		}

		#endregion

		#region Implementation

		ProcessTask CreateTriggerToPublishUXMLEventInNewFactory(string eventCode, string processType)
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = processType;
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "Test";
			templateTask.P9_Type = "TRG";
			templateTask.TriggerConditions.TriggerEventCode = eventCode;

			var action = templateTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "XUE";
			action.PQ_Calc_TriggerParty = "TPC";
			newFactory.Save();
			return templateTask;
		}

		#endregion

		#region Helper

		protected WhsTransitTestHelper Helper
		{
			get { return helper ?? (helper = new WhsTransitTestHelper(Factory)); }
		}
		WhsTransitTestHelper helper;

		#endregion
	}
}
