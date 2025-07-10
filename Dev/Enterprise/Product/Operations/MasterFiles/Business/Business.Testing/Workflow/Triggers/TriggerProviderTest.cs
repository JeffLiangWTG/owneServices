using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TriggerProviderTest : TestCaseWithFactory
	{
		public void TestLoadAllMilestonesAndTriggersForEvent()
		{
			var shipment1 = (IStmALogParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var shipment2 = (IStmALogParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));

			var processTasks = new[]
			{
				CreateProcessTask(shipment1, Constants.Workflow.MilestoneType, Events.DepartureCode, null),				// [0]		X
				CreateProcessTask(shipment1, Constants.Workflow.MilestoneType, Events.DepartureCode, "SSS"),			// [1]
				CreateProcessTask(shipment1, Constants.Workflow.MilestoneType, Events.ArrivalCode, null),				// [2]
				CreateProcessTask(shipment1, Constants.Workflow.MilestoneType, Events.ArrivalCode, "SSS"),				// [3]
				CreateProcessTask(shipment1, Constants.Workflow.WorkflowTriggerType, Events.DepartureCode, null),		// [4]		X
				CreateProcessTask(shipment1, Constants.Workflow.WorkflowTriggerType, Events.DepartureCode, "SSS"),		// [5]
				CreateProcessTask(shipment1, Constants.Workflow.WorkflowTriggerType, Events.ArrivalCode, null),			// [6]
				CreateProcessTask(shipment1, Constants.Workflow.WorkflowTriggerType, Events.ArrivalCode, "SSS"),		// [7]
				CreateProcessTask(shipment1, Constants.Workflow.ExceptionType, Events.DepartureCode, null),				// [8]
				CreateProcessTask(shipment1, Constants.Workflow.ExceptionType, Events.DepartureCode, "SSS"),			// [9]
				CreateProcessTask(shipment1, Constants.Workflow.ExceptionType, Events.ArrivalCode, null),				// [10]
				CreateProcessTask(shipment1, Constants.Workflow.ExceptionType, Events.ArrivalCode, "SSS"),				// [11]

				CreateProcessTask(shipment2, Constants.Workflow.WorkflowTriggerType, Events.DepartureCode, null)		// [12]
			};

			Factory.Save();

			var log = shipment1.Logs.AddNew(Events.Departure);

			var expectedProcessTasks = new[] { processTasks[0], processTasks[4] };
			var actualProcessTasks = TriggerProvider.LoadAllMilestonesAndTriggersForEvent(shipment1, log, log.EventTimeOffset);

			AssertContainsExactElementsInAnyOrder("Process tasks", expectedProcessTasks.Select(t => ToString(t)), actualProcessTasks.Select(t => ToString(t.trigger)));
		}

		public void TestOrphanedMilestonesAndTriggersErrorReport()
		{
			var shipment = (IStmALogParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));

			var processTasks = new[]
			{
				CreateProcessTask(shipment, Constants.Workflow.MilestoneType, Events.DepartureCode, null),
				CreateProcessTask(shipment, Constants.Workflow.MilestoneType, Events.DepartureCode, "SSS"),
				CreateProcessTask(shipment, Constants.Workflow.WorkflowTriggerType, Events.DepartureCode, null),
				CreateProcessTask(shipment, Constants.Workflow.WorkflowTriggerType, Events.DepartureCode, "SSS"),
			};

			Factory.Save();

			var log = shipment.Logs.AddNew(Events.Departure);

			TestConnection.ExecuteNonQuery("DELETE FROM dbo.JOBSHIPMENT");

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var reloadedLog = (StmALog)newFactory.Load(typeof(StmALog), new ZQuery()).First(x => x.PK == log.PK);

			var actualProcessTasks = TriggerProvider.LoadAllMilestonesAndTriggersForEvent(shipment, reloadedLog, reloadedLog.EventTimeOffset);
			AssertEquals(0, actualProcessTasks.Length);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestLoadAllMilestonesAndTriggersForEvent_ARV_DifferentCompanies()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var company = (BusinessObject)factory.New<IGlbCompany>();

			var declaration = factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

			var entry = factory.New(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.ICusEntryHeader)));
			entry[CusEntryHeaderSchema.CH_JE] = declaration.PK;

			var log_arv = declaration.GetLogs().AddNew(Events.Arrival);
			var log_dep = declaration.GetLogs().AddNew(Events.Departure);
			var log_siv = declaration.GetLogs().AddNew(Events.ServiceInvoicePosted);
			factory.Save();

			var milestone_arv = ((IWorkflowProvider)shipment).WorkflowItems.Milestones.AddNew();
			((IBaseTrigger)milestone_arv).TriggerEventCode = "ARV";
			((BusinessObject)milestone_arv)[ProcessTasksSchema.P9_GC] = company.PK;

			var milestone_dep = ((IWorkflowProvider)shipment).WorkflowItems.Milestones.AddNew();
			((IBaseTrigger)milestone_dep).TriggerEventCode = "DEP";
			((BusinessObject)milestone_dep)[ProcessTasksSchema.P9_GC] = company.PK;

			var milestone_siv = ((IWorkflowProvider)shipment).WorkflowItems.Milestones.AddNew();
			((IBaseTrigger)milestone_siv).TriggerEventCode = "SIV";
			((BusinessObject)milestone_siv)[ProcessTasksSchema.P9_GC] = company.PK;
			factory.Save();

			factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, shipment.PK);

			var tasks = TriggerProvider.LoadAllMilestonesAndTriggersForEvent(log_arv.Master, log_arv, log_arv.EventTimeOffset);
			AssertEquals("Should not take P9_GC into consideration", 1, tasks.Length);
			AssertEquals("should load the milestone", milestone_arv.PK, tasks[0].trigger.Identifier);

			tasks = TriggerProvider.LoadAllMilestonesAndTriggersForEvent(log_dep.Master, log_dep, log_dep.EventTimeOffset);
			AssertEquals("Should not take P9_GC into consideration", 1, tasks.Length);
			AssertEquals("should load the milestone", milestone_dep.PK, tasks[0].trigger.Identifier);
		}

		public void TestGetProcessTaskQueryForParent()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var eventTypeCode = "TST";
			var expectedQueryText = string.Format(@"P9_ParentID = CONVERT('{0}', 'System.Guid') and P9_SE_NKMilestoneEvent = '{1}' and (P9_Type in ('MIL', 'TRG')) and P9_LineTriggerType = ''", shipment.PK, eventTypeCode);
			new ProcessTaskTriggerQueryStrategy().TryGetQueryForAllTriggersIncludingThoseOnParentObjects((IStmALogParent)shipment, eventTypeCode, out ZQuery query);
			AssertEquals("query.LiteralTextADO", expectedQueryText, query.LiteralTextADO);

			expectedQueryText = string.Format(@"P9_ParentID = CONVERT('{0}', 'System.Guid') and (P9_Type in ('MIL', 'TRG')) and P9_LineTriggerType = ''", shipment.PK);
			new ProcessTaskTriggerQueryStrategy().TryGetQueryForAllTriggersIncludingThoseOnParentObjects((IStmALogParent)shipment, string.Empty, out query);
			AssertEquals("query.LiteralTextADO", expectedQueryText, query.LiteralTextADO);
		}

		public void TestGetProcessTriggerQueryForParentLoadFromCacheIsFalse()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var eventTypeCode = "JOP";
			new ProcessTaskTriggerQueryStrategy().TryGetQueryForAllTriggersIncludingThoseOnParentObjects((IStmALogParent)shipment, eventTypeCode, out ZQuery query);
			Assert(query.FetchOnlyFromLocalCache);

			new ProcessTemplateTriggerQueryStrategy().TryGetQueryForAllTriggersIncludingThoseOnParentObjects((IStmALogParent)shipment, eventTypeCode, out query);
			Assert(!query.FetchOnlyFromLocalCache);
		}

		public void TestProcessTemplateTriggerQueryStrategy()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var dependingDeclaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			dependingDeclaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			var standaloneDeclaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));

			var loader = new ProcessTaskTemplate.Loader(Factory);
			loader.FindMatches(shipment as IWorkflowProviderCore, includeOnlyUniversalTemplates: true).ForEach(t => t.P0_IsActive = false);
			loader.FindMatches(dependingDeclaration as IWorkflowProviderCore, includeOnlyUniversalTemplates: true).ForEach(t => t.P0_IsActive = false);
			loader.FindMatches(standaloneDeclaration as IWorkflowProviderCore, includeOnlyUniversalTemplates: true).ForEach(t => t.P0_IsActive = false);

			var templateSHP = Factory.New<ProcessTaskTemplate>();
			templateSHP.P0_ProcessType = "SHP";
			templateSHP.P0_IsUniversal = true;
			templateSHP.P0_TriggerFallbackMethod = FallbackTypeList.Codes.NeverFallback;
			templateSHP.P0_Name = "SHP Template";
			var templateBRK = Factory.New<ProcessTaskTemplate>();
			templateBRK.P0_ProcessType = "BRK";
			templateBRK.P0_IsUniversal = true;
			templateBRK.P0_TriggerFallbackMethod = FallbackTypeList.Codes.NeverFallback;
			templateBRK.P0_Name = "BRK Template";

			Factory.Save();

			var strategy = new ProcessTemplateTriggerQueryStrategy();
			var shipmentAsLogParent = shipment as IStmALogParent;
			var resultForShipment = strategy.TryGetQueryForAllTriggersIncludingThoseOnParentObjects(shipmentAsLogParent, "EDT", out _);
			Assert("Should return true for None-ISometimesWorkflowProvider.", resultForShipment);

			var dependingDeclarationAsLogParent = dependingDeclaration as IStmALogParent;
			var resultFordependingDeclaration = strategy.TryGetQueryForAllTriggersIncludingThoseOnParentObjects(dependingDeclarationAsLogParent, "EDT", out _);
			Assert("Should return false for Declaration depending on Shipment.", resultFordependingDeclaration);

			var standalongDeclarationAsLogParent = standaloneDeclaration as IStmALogParent;
			var resultForStandalongDeclaration = strategy.TryGetQueryForAllTriggersIncludingThoseOnParentObjects(standalongDeclarationAsLogParent, "EDT", out _);
			Assert("Should return true for standalong Declaration.", resultForStandalongDeclaration);
		}

		public void TestGetProcessTaskQueryForAgencyShipment()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Freight.Integration.Agency.IAgencyShipment>());
			var eventTypeCode = "TST";
			var expectedQueryText = string.Format(@"P9_ParentID = CONVERT('{0}', 'System.Guid') and P9_SE_NKMilestoneEvent = '{1}' and (P9_Type in ('MIL', 'TRG')) and P9_LineTriggerType = ''", shipment.PK, eventTypeCode);
			new ProcessTaskTriggerQueryStrategy().TryGetQueryForAllTriggersIncludingThoseOnParentObjects((IStmALogParent)shipment, eventTypeCode, out ZQuery query);
			AssertEquals("query.LiteralTextADO", expectedQueryText, query.LiteralTextADO);
		}

		public void TestAddExtraQueryIfNeeded()
		{
			var factory1 = new BusinessObjectFactory();

			var shipment = factory1.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var log = shipment.GetLogs().AddNew(Events.ServiceInvoicePosted);

			var eventTypeCode = "TST";
			AssertNoExceptionThrown(delegate
			{ new ProcessTaskTriggerQueryStrategy().TryGetQueryForAllTriggersIncludingThoseOnParentObjects((IStmALogParent)shipment, eventTypeCode, out ZQuery query); });
		}

		public void TestGetBranchForTemporaryUserContext_SystemUser()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var homeBranch = Factory.NewWithValidTestData<GlbBranch>();
			var firstActiveBranch = Factory.NewWithValidTestData<GlbBranch>();
			var user = Factory.NewWithValidTestData<GlbStaff>();

			company.GC_Code = "CP1";
			firstActiveBranch.GB_Code = "BR0";
			homeBranch.GB_Code = "BR1";
			company.Branches.Add(firstActiveBranch);
			company.Branches.Add(homeBranch);
			user.GS_GB_HomeBranch = homeBranch.PK;

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Milestones.AddNew();
			task.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			Factory.Save();
			task.P9_GC = company.PK;

			AssertEquals("Precondition", firstActiveBranch.GB_Code, company.FirstActiveBranch.GB_Code);
			AssertEquals("Should use homeBranch when user is not system defined", homeBranch.GB_Code, TriggerProvider.GetBranchForTemporaryUserContext(task, dummy, Lazy.Create(() => user)).GB_Code);

			user.GS_IsSystemAccount = true;
			AssertEquals("Should use firstActiveBranch when user is system defined", firstActiveBranch.GB_Code, TriggerProvider.GetBranchForTemporaryUserContext(task, dummy, Lazy.Create(() => user)).GB_Code);
		}

		IBaseTrigger CreateProcessTask(IStmALogParent parent, string type, string eventCode, string lineTriggerType)
		{
			var processTask = Factory.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)processTask).TriggerEventCode = eventCode;
			processTask.P9_ParentID = parent.LogsParentPK;
			processTask.P9_ParentTableCode = "JS";
			processTask.P9_Type = type;
			processTask.P9_LineTriggerType = lineTriggerType;

			return (IBaseTrigger)processTask;
		}

		static string ToString(IBaseTrigger trigger)
		{
			return string.Format("Parent={0}, Type={1}, Event={2}, LineTriggerType={3}", trigger.ParentID, trigger.WorkflowItemType, trigger.TriggerEventCode, (trigger as IProcessTask)?.P9_LineTriggerType);
		}
	}
}
