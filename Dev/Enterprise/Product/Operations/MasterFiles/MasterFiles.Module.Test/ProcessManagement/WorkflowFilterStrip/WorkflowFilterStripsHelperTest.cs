using System;
using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	public class WorkflowFilterStripsHelperTest : TestCaseWithFactory
	{
		#region Milestones

		public void TestMilestoneDateFilter()
		{
			var dummyBO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyBO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var milestone1 = dummyBO1.WorkflowItems.Milestones.AddNew();
			var milestone2 = dummyBO2.WorkflowItems.Milestones.AddNew();

			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 7, 1)));

			Factory.Save();

			var allFilters = new DummyFilterStripBizOWithWorkflowFilters();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["Milestone Date"];
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneFilter.Property1 = new ZDateTime(2000, 1, 1);
			milestoneFilter.Property2 = new ZDateTime(2000, 1, 3);
			milestoneFilter.IsActive = true;

			var dummyCollection = new DummyBusinessObjectCollection(Factory, allFilters.Filter);
			dummyCollection.Load();

			Assert(dummyCollection.Contains(dummyBO1));
			Assert(!dummyCollection.Contains(dummyBO2));
		}

		public void TestMilestoneCompletedFilter()
		{
			var dummyBO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyBO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var milestone1 = dummyBO1.WorkflowItems.Milestones.AddNew();
			var milestone2 = dummyBO2.WorkflowItems.Milestones.AddNew();

			var trigger = dummyBO1.WorkflowItems.Triggers.AddNew();
			var exception = dummyBO2.WorkflowItems.Exceptions.AddNew();

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			exception.SetMilestoneActualDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
			trigger.SetMilestoneActualDateForTest(ZDateTime.Empty);

			Factory.Save();

			var allFilters = new DummyFilterStripBizOWithWorkflowFilters();
			var milestoneFilter = (ModuleTextFilter)allFilters["Milestone Completed"];
			milestoneFilter.Property = "Completed";
			milestoneFilter.IsActive = true;

			var dummyCollection = new DummyBusinessObjectCollection(Factory, allFilters.Filter);
			dummyCollection.Load();

			Assert(dummyCollection.Contains(dummyBO1));
			Assert(!dummyCollection.Contains(dummyBO2));

			milestoneFilter.Property = "Not Completed";
			milestoneFilter.IsActive = true;

			dummyCollection = new DummyBusinessObjectCollection(Factory, allFilters.Filter);
			dummyCollection.Load();

			Assert(!dummyCollection.Contains(dummyBO1));
			Assert(dummyCollection.Contains(dummyBO2));
		}

		public void TestMilestoneCompletedFilter_PublishedMilestones()
		{
			bool originalGlobalsIsWeb = Globals.IsWeb;

			try
			{
				Globals.IsWeb = true;
				var dummyBO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
				var dummyBO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();

				var milestone1 = dummyBO1.WorkflowItems.Milestones.AddNew();
				var milestone2 = dummyBO2.WorkflowItems.Milestones.AddNew();

				var trigger = dummyBO1.WorkflowItems.Triggers.AddNew();
				var exception = dummyBO2.WorkflowItems.Exceptions.AddNew();

				milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
				milestone1.P9_IsPublished = true;
				milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
				milestone2.P9_IsPublished = false;
				exception.SetMilestoneActualDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
				trigger.SetMilestoneActualDateForTest(ZDateTime.Empty);

				Factory.Save();

				var allFilters = new DummyFilterStripBizOWithWorkflowFilters();
				var milestoneFilter = (ModuleTextFilter)allFilters["Milestone Completed"];
				milestoneFilter.Property = "Completed";
				milestoneFilter.IsActive = true;

				var dummyCollection = new DummyBusinessObjectCollection(Factory, allFilters.Filter);
				dummyCollection.Load();

				Assert(dummyCollection.Contains(dummyBO1));
				Assert(!dummyCollection.Contains(dummyBO2));

				milestone1.P9_IsPublished = false;
				milestone2.P9_IsPublished = true;
				Factory.Save();

				dummyCollection = new DummyBusinessObjectCollection(Factory, allFilters.Filter);
				dummyCollection.Load();

				Assert(!dummyCollection.Contains(dummyBO1));
				Assert(dummyCollection.Contains(dummyBO2));

				milestone1.P9_IsPublished = true;
				Factory.Save();

				dummyCollection = new DummyBusinessObjectCollection(Factory, allFilters.Filter);
				dummyCollection.Load();

				Assert(dummyCollection.Contains(dummyBO1));
				Assert(dummyCollection.Contains(dummyBO2));
			}
			finally
			{
				Globals.IsWeb = originalGlobalsIsWeb;
			}
		}

		public void TestMilestoneNextFilter()
		{
			DummyWithWorkflow dummyBO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow dummyBO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			ProcessTask milestone1 = dummyBO1.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = "AID";
			ProcessTask milestone2 = dummyBO2.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = "AID";

			milestone1.P9_Type = Core.Constants.Workflow.MilestoneType;
			milestone2.P9_Type = Core.Constants.Workflow.MilestoneType;

			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 7, 1)));
			milestone1.SetMilestoneActualDateForTest(ZDateTime.Empty);
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);

			Factory.Save();

			DummyFilterStripBizOWithWorkflowFilters milestone1Filter = new DummyFilterStripBizOWithWorkflowFilters();
			((WorkflowModuleFilter)milestone1Filter["Next Milestone"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((WorkflowModuleFilter)milestone1Filter["Next Milestone"]).Property1 = new ZDateTime(2000, 1, 1);
			((WorkflowModuleFilter)milestone1Filter["Next Milestone"]).Property2 = new ZDateTime(2000, 1, 3);
			((WorkflowModuleFilter)milestone1Filter["Next Milestone"]).IsActive = true;

			DummyBusinessObjectCollection dummyCollection = new DummyBusinessObjectCollection(Factory, milestone1Filter.Filter);
			dummyCollection.Load();

			Assert(dummyCollection.Contains(dummyBO1));
			Assert(!dummyCollection.Contains(dummyBO2));
		}

		public void TestMilestoneLastCompletedFilter()
		{
			DummyWithWorkflow dummyBO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow dummyBO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			ProcessTask milestone1 = dummyBO1.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone2 = dummyBO2.WorkflowItems.Milestones.AddNew();

			milestone1.P9_Type = Core.Constants.Workflow.MilestoneType;
			milestone2.P9_Type = Core.Constants.Workflow.MilestoneType;

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 7, 1));

			Factory.Save();

			DummyFilterStripBizOWithWorkflowFilters milestone1Filter = new DummyFilterStripBizOWithWorkflowFilters();
			((WorkflowModuleFilter)milestone1Filter["Last Completed Milestone"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((WorkflowModuleFilter)milestone1Filter["Last Completed Milestone"]).Property1 = new ZDateTime(2000, 1, 1);
			((WorkflowModuleFilter)milestone1Filter["Last Completed Milestone"]).Property2 = new ZDateTime(2000, 1, 3);
			((WorkflowModuleFilter)milestone1Filter["Last Completed Milestone"]).IsActive = true;

			DummyBusinessObjectCollection dummyCollection = new DummyBusinessObjectCollection(Factory, milestone1Filter.Filter);
			dummyCollection.Load();

			Assert(dummyCollection.Contains(dummyBO1));
			Assert(!dummyCollection.Contains(dummyBO2));
		}

		public void TestMilestoneLastCompletedFilter_NoDateMeansNoResults()
		{
			var dummyBO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyBO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var milestone1 = dummyBO1.WorkflowItems.Milestones.AddNew();
			var milestone2 = dummyBO2.WorkflowItems.Milestones.AddNew();

			milestone1.P9_Type = Core.Constants.Workflow.MilestoneType;
			milestone2.P9_Type = Core.Constants.Workflow.MilestoneType;

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 7, 1));

			Factory.Save();

			var milestone1Filter = new DummyFilterStripBizOWithWorkflowFilters();
			((WorkflowModuleFilter)milestone1Filter["Last Completed Milestone"]).PropertySearch = ModuleDateFilter.HasNoDateEntered;
			((WorkflowModuleFilter)milestone1Filter["Last Completed Milestone"]).IsActive = true;

			AssertEquals(0, Factory.Load<DummyWithWorkflow>(milestone1Filter.Filter).Length);
		}

		public void TestGetParentTableCodeQuery()
		{
			var query = WorkflowFilterStripsHelper.GetParentTableCodeQuery(typeof(DummyBusinessObject));
			Assert(query.LiteralTextSqlFormatted.Contains("P9_ParentTableCode = 'Z0'"));

			query = WorkflowFilterStripsHelper.GetParentTableCodeQuery(typeof(OrgHeader));
			Assert(query.LiteralTextSqlFormatted.Contains("P9_ParentTableCode = 'OH'"));
		}

		#endregion

		#region Related Job Milestones

		public void TestRelatedMilestones()
		{
			OrgHeader parent = Factory.NewWithValidTestData<OrgHeader>();
			var relatedItem = Factory.NewWithValidTestData<DummyWithWorkflow>();
			relatedItem.Z0_Guid = parent.PK;
			var milestone = relatedItem.WorkflowItems.Milestones.AddNew();
			milestone.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));

			Factory.Save();

			var allFilters = new DummyFilterStripBizOWithWorkflowFiltersAndRelatedJobFilters();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["Milestone Date"];
			milestoneFilter.IsActive = true;
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneFilter.Property1 = new ZDateTime(2000, 1, 1);
			milestoneFilter.Property2 = new ZDateTime(2000, 1, 3);
			OrgHeaderCollection results = new OrgHeaderCollection(Factory, allFilters.Filter);
			results.Load();
			AssertCollectionNotContains("Milestone is on RELATED job, not job itself", parent, results);
			milestoneFilter.IsActive = false;

			var milestoneRelatedFilter = (WorkflowModuleFilter)allFilters["Milestone Date (Related)"];

			milestoneRelatedFilter.IsActive = true;
			milestoneRelatedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneRelatedFilter.Property1 = new ZDateTime(2000, 1, 1);
			milestoneRelatedFilter.Property2 = new ZDateTime(2000, 1, 3);
			results = new OrgHeaderCollection(Factory, allFilters.Filter);
			results.Load();
			AssertCollectionContains(parent, results);

			milestoneRelatedFilter.Property1 = new ZDateTime(2001, 1, 1);
			milestoneRelatedFilter.Property2 = new ZDateTime(2001, 1, 3);
			results = new OrgHeaderCollection(Factory, allFilters.Filter);
			results.Load();
			AssertCollectionNotContains(parent, results);
		}

		#endregion

		#region Tasks

		public void TestAnyOpenTaskAssignedToFilter_ForShipments()
		{
			var dummyBO1 = (IWorkflowProvider)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var dummyBO2 = (IWorkflowProvider)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var dummyBO3 = (IWorkflowProvider)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var dummyBO4 = (IWorkflowProvider)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var dummyBO5 = (IWorkflowProvider)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var dummyBO6 = (IWorkflowProvider)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var task1 = dummyBO1.WorkflowItems.Tasks.AddNew();
			var task2 = dummyBO2.WorkflowItems.Tasks.AddNew();
			var task3 = dummyBO3.WorkflowItems.Tasks.AddNew();
			var task4 = dummyBO4.WorkflowItems.Tasks.AddNew();
			var exception1 = dummyBO5.WorkflowItems.Exceptions.AddNew();
			var exception2 = dummyBO6.WorkflowItems.Exceptions.AddNew();

			task1.P9_GS_NKAssignedStaffMember = "SM1";
			task2.P9_GS_NKAssignedStaffMember = "SM1";
			task3.P9_GS_NKAssignedStaffMember = "SM1";
			task4.P9_GS_NKAssignedStaffMember = "SM2";
			exception1.P9_GS_NKAssignedStaffMember = "SM1";
			exception2.P9_GS_NKAssignedStaffMember = "SM1";

			task1.P9_Status = "WRK";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = "WRK";
			exception1.IsExceptionActioned = false;
			exception2.IsExceptionActioned = true;

			Factory.Save();

			DummyFilterStripBizOWithWorkflowFiltersForTaskFilters task1Filter = new DummyFilterStripBizOWithWorkflowFiltersForTaskFilters();
			((ModuleNkFilter)task1Filter["Any Open Task Assigned To"]).Property = "SM1";
			((ModuleNkFilter)task1Filter["Any Open Task Assigned To"]).IsActive = true;

			IList list = Factory.Load<Enterprise.Integration.Forwarding.IForwardingShipment>(task1Filter.Filter);

			Assert("dummyBO1 should be present after applying this filter", list.Contains(dummyBO1));
			Assert("dummyBO2 should be filtered out", !list.Contains(dummyBO2));
			Assert("dummyBO3 should be filtered out", !list.Contains(dummyBO3));
			Assert("dummyBO4 should be filtered out", !list.Contains(dummyBO4));
			Assert("Exceptiosn aren't tasks, so not visible", !list.Contains(dummyBO5));
			Assert("dummyBO6 should be filtered out", !list.Contains(dummyBO6));
		}

		public void TestAnyOpenTaskAssignedToFilter()
		{
			var dummyBOForTask1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyBOForTask2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyBOForTask3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyBOForTask4 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyBOForTask5 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyBOForException1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyBOForException2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1 = dummyBOForTask1.WorkflowItems.Tasks.AddNew();
			var task2 = dummyBOForTask2.WorkflowItems.Tasks.AddNew();
			var task3 = dummyBOForTask3.WorkflowItems.Tasks.AddNew();
			var task4 = dummyBOForTask4.WorkflowItems.Tasks.AddNew();
			var task5 = dummyBOForTask5.WorkflowItems.Tasks.AddNew();
			var exception1 = dummyBOForException1.WorkflowItems.Exceptions.AddNew();
			var exception2 = dummyBOForException2.WorkflowItems.Exceptions.AddNew();

			task1.P9_GS_NKAssignedStaffMember = "SM1";
			task2.P9_GS_NKAssignedStaffMember = "SM1";
			task3.P9_GS_NKAssignedStaffMember = "SM1";
			task4.P9_GS_NKAssignedStaffMember = "SM2";
			task5.P9_GS_NKAssignedStaffMember = "SM1";
			exception1.P9_GS_NKAssignedStaffMember = "SM1";
			exception2.P9_GS_NKAssignedStaffMember = "SM1";

			task1.P9_Status = "WRK";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = "WRK";
			task5.P9_Status = ProcessTask.LastCompletedStatusCode;

			exception1.IsExceptionActioned = false;
			exception2.IsExceptionActioned = true;

			Factory.Save();

			DummyFilterStripBizOWithWorkflowFiltersForOrdersJobs task1Filter = new DummyFilterStripBizOWithWorkflowFiltersForOrdersJobs();
			((ModuleNkFilter)task1Filter["Any Open Task Assigned To"]).Property = "SM1";
			((ModuleNkFilter)task1Filter["Any Open Task Assigned To"]).IsActive = true;
			AssertEquals("Should not be published on web", false, ((ModuleNkFilter)task1Filter["Any Open Task Assigned To"]).IsPublishedOnWeb);

			var dummyCollection = Factory.Load<DummyWithWorkflow>(task1Filter.Filter);
			AssertContains("P9_ParentTableCode = 'Z0'", task1Filter.Filter.LiteralTextSqlFormatted);

			AssertCollectionContains(dummyBOForTask1, dummyCollection);
			AssertCollectionNotContains(dummyBOForTask2, dummyCollection);
			AssertCollectionNotContains(dummyBOForTask3, dummyCollection);
			AssertCollectionNotContains(dummyBOForTask4, dummyCollection);
			AssertCollectionNotContains(dummyBOForTask5, dummyCollection);
			AssertCollectionNotContains(dummyBOForException1, dummyCollection);
			AssertCollectionNotContains(dummyBOForException2, dummyCollection);
		}

		public void TestAnyOpenTaskAssignedToFilter_IgnoreTriggersAndMilestones()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = dummy1.WorkflowItems.Milestones.AddNew();
			milestone.P9_GS_NKAssignedStaffMember = "SM1";

			var trigger = dummy1.WorkflowItems.Milestones.AddNew();
			trigger.P9_GS_NKAssignedStaffMember = "SM1";

			var task1Filter = new DummyFilterStripBizOWithWorkflowFiltersForOrdersJobs();
			((ModuleNkFilter)task1Filter["Any Open Task Assigned To"]).Property = "SM1";
			((ModuleNkFilter)task1Filter["Any Open Task Assigned To"]).IsActive = true;
			AssertEquals("Should not be published on web", false, ((ModuleNkFilter)task1Filter["Any Open Task Assigned To"]).IsPublishedOnWeb);

			AssertCollectionNotContains(dummy1, Factory.Load<DummyWithWorkflow>(task1Filter.Filter));

			var task = dummy1.WorkflowItems.Tasks.AddNew();
			task.P9_GS_NKAssignedStaffMember = "SM1";

			AssertCollectionNotContains(dummy1, Factory.Load<DummyWithWorkflow>(task1Filter.Filter));
		}

		public void TestAnyOpenTaskAssignedToFilter_ForCustomDeclarationJobs()
		{
			(BusinessObject, ProcessTask) MakeTask(Func<IWorkflowProvider, ProcessTask> makeItem)
			{
				var biz = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				var ship = (IWorkflowProvider)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
				var t = makeItem(ship);
				biz[JobDeclarationSchema.JE_JS] = ship.PK;
				return (biz, t);
			}

			var (dummyBO1, task1) = MakeTask(t => t.WorkflowItems.Tasks.AddNew());
			var (dummyBO2, task2) = MakeTask(t => t.WorkflowItems.Tasks.AddNew());
			var (dummyBO3, task3) = MakeTask(t => t.WorkflowItems.Tasks.AddNew());
			var (dummyBO4, task4) = MakeTask(t => t.WorkflowItems.Tasks.AddNew());
			var (dummyBO5, exception1) = MakeTask(t => t.WorkflowItems.Exceptions.AddNew());
			var (dummyBO6, exception2) = MakeTask(t => t.WorkflowItems.Exceptions.AddNew());

			task1.P9_GS_NKAssignedStaffMember = "SM1";
			task2.P9_GS_NKAssignedStaffMember = "SM1";
			task3.P9_GS_NKAssignedStaffMember = "SM1";
			task4.P9_GS_NKAssignedStaffMember = "SM2";
			exception1.P9_GS_NKAssignedStaffMember = "SM1";
			exception2.P9_GS_NKAssignedStaffMember = "SM1";

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			exception1.IsExceptionActioned = false;
			exception2.IsExceptionActioned = true;

			Factory.Save();

			var task1Filter = new DummyFilterStripBizOWithWorkflowFiltersForCustomsDeclarationJobs();
			((ModuleNkFilter)task1Filter["Any Open Task Assigned To"]).Property = "SM1";
			((ModuleNkFilter)task1Filter["Any Open Task Assigned To"]).IsActive = true;

			IList list = Factory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(task1Filter.Filter);
			AssertContains("P9_ParentTableCode = 'JE'", task1Filter.Filter.LiteralTextSqlFormatted);
			AssertContains("P9_ParentTableCode = 'JS'", task1Filter.Filter.LiteralTextSqlFormatted);
			Assert("dummyBO1 should be present after applying this filter", list.Contains(dummyBO1));
			Assert("dummyBO2 should be filtered out", !list.Contains(dummyBO2));
			Assert("dummyBO3 should be filtered out", !list.Contains(dummyBO3));
			Assert("dummyBO4 should be filtered out", !list.Contains(dummyBO4));
			Assert("Exceptions aren't tasks so no.", !list.Contains(dummyBO5));
			Assert("dummyBO6 should be filtered out", !list.Contains(dummyBO6));
		}

		public void TestNextTaskAssignedToFilter_ForShipments()
		{
			var dummyBO1 = (IWorkflowProvider)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var dummyBO2 = (IWorkflowProvider)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var dummyBO3 = (IWorkflowProvider)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var dummyBO4 = (IWorkflowProvider)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var dummyBO5 = (IWorkflowProvider)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();

			var task1 = dummyBO1.WorkflowItems.Tasks.AddNew();
			var task2 = dummyBO2.WorkflowItems.Tasks.AddNew();
			var task3 = dummyBO3.WorkflowItems.Tasks.AddNew();
			var task4 = dummyBO4.WorkflowItems.Tasks.AddNew();
			var task5 = dummyBO5.WorkflowItems.Tasks.AddNew();
			var task6 = dummyBO4.WorkflowItems.Tasks.AddNew();
			task1.P9_GS_NKAssignedStaffMember = "SM1";
			task2.P9_GS_NKAssignedStaffMember = "SM1";
			task3.P9_GS_NKAssignedStaffMember = "SM1";
			task4.P9_GS_NKAssignedStaffMember = "SM2";
			task5.P9_GS_NKAssignedStaffMember = "SM1";
			task6.P9_GS_NKAssignedStaffMember = "SM1";
			task1.P9_Status = "WRK";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = "WRK";
			task5.P9_Status = "WRK";
			task6.P9_Status = "WRK";
			task1.P9_Sequence = 5;
			task2.P9_Sequence = 1;
			task3.P9_Sequence = 2;
			task4.P9_Sequence = 3;
			task5.P9_Sequence = 4;
			task6.P9_Sequence = 6;

			Factory.Save();

			DummyFilterStripBizOWithWorkflowFiltersForTaskFilters task1Filter = new DummyFilterStripBizOWithWorkflowFiltersForTaskFilters();
			((ModuleNkFilter)task1Filter["Next Task Assigned To"]).Property = "SM1";
			((ModuleNkFilter)task1Filter["Next Task Assigned To"]).IsActive = true;

			IList list = Factory.Load<Enterprise.Integration.Forwarding.IForwardingShipment>(task1Filter.Filter);

			Assert("dummyBO1 should be present after applying this filter", list.Contains(dummyBO1));
			Assert("dummyBO2 should be filtered out", !list.Contains(dummyBO2));
			Assert("dummyBO3 should be filtered out", !list.Contains(dummyBO3));
			Assert("dummyBO4 should be filtered out", !list.Contains(dummyBO4));
			Assert("dummyBO5 should be present after applying this filter", list.Contains(dummyBO5));
		}

		public void TestNextTaskAssignedToFilter()
		{
			var dummyBO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyBO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyBO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyBO4 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyBO5 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1 = dummyBO1.WorkflowItems.Tasks.AddNew();
			var task2 = dummyBO2.WorkflowItems.Tasks.AddNew();
			var task3 = dummyBO3.WorkflowItems.Tasks.AddNew();
			var task4 = dummyBO4.WorkflowItems.Tasks.AddNew();
			var task5 = dummyBO5.WorkflowItems.Tasks.AddNew();
			var task6 = dummyBO4.WorkflowItems.Tasks.AddNew();
			task1.P9_GS_NKAssignedStaffMember = "SM1";
			task2.P9_GS_NKAssignedStaffMember = "SM1";
			task3.P9_GS_NKAssignedStaffMember = "SM1";
			task4.P9_GS_NKAssignedStaffMember = "SM2";
			task5.P9_GS_NKAssignedStaffMember = "SM1";
			task6.P9_GS_NKAssignedStaffMember = "SM1";
			task1.P9_Status = "WRK";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = "WRK";
			task5.P9_Status = "WRK";
			task6.P9_Status = "WRK";
			task1.P9_Sequence = 5;
			task2.P9_Sequence = 1;
			task3.P9_Sequence = 2;
			task4.P9_Sequence = 3;
			task5.P9_Sequence = 4;
			task6.P9_Sequence = 6;
			Factory.Save();

			DummyFilterStripBizOWithWorkflowFiltersForOrdersJobs task1Filter = new DummyFilterStripBizOWithWorkflowFiltersForOrdersJobs();
			((ModuleNkFilter)task1Filter["Next Task Assigned To"]).Property = "SM1";
			((ModuleNkFilter)task1Filter["Next Task Assigned To"]).IsActive = true;

			var dummyCollection = Factory.Load<DummyWithWorkflow>(task1Filter.Filter);
			AssertContains("P9_ParentTableCode = 'Z0'", task1Filter.Filter.LiteralTextSqlFormatted);

			AssertCollectionContains(dummyBO1, dummyCollection);
			AssertCollectionNotContains(dummyBO2, dummyCollection);
			AssertCollectionNotContains(dummyBO3, dummyCollection);
			AssertCollectionNotContains(dummyBO4, dummyCollection);
			AssertCollectionContains(dummyBO5, dummyCollection);
		}

		public void TestNextTaskAssignedToFilterDifferentTypes()
		{
			var dummyBO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1 = dummyBO1.WorkflowItems.Tasks.AddNew();
			var task2 = dummyBO1.WorkflowItems.Tasks.AddNew();
			var task3 = dummyBO1.WorkflowItems.Tasks.AddNew();
			var task4 = dummyBO1.WorkflowItems.Tasks.AddNew();
			var task5 = dummyBO1.WorkflowItems.Tasks.AddNew();

			task1.P9_GS_NKAssignedStaffMember = "A.G";
			task2.P9_GS_NKAssignedStaffMember = "A.G";
			task3.P9_GS_NKAssignedStaffMember = "A.G";
			task4.P9_GS_NKAssignedStaffMember = "";
			task5.P9_GS_NKAssignedStaffMember = "A.K";

			task5.P9_Sequence = 1;
			task1.P9_Sequence = 1;
			task2.P9_Sequence = 2;
			task3.P9_Sequence = 3;
			task4.P9_Sequence = 2;
			task4.P9_Sequence = 3;

			task1.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task2.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task3.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task4.P9_Type = Core.Constants.Workflow.MilestoneType;
			task5.P9_Type = Core.Constants.Workflow.MilestoneType;

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task3.P9_Status = "ASN";
			task4.P9_Status = "ASN";
			task5.P9_Status = "ASN";

			Factory.Save();

			DummyFilterStripBizOWithWorkflowFiltersForOrdersJobs task1Filter = new DummyFilterStripBizOWithWorkflowFiltersForOrdersJobs();
			((ModuleNkFilter)task1Filter["Next Task Assigned To"]).Property = "A.G";
			((ModuleNkFilter)task1Filter["Next Task Assigned To"]).IsActive = true;

			var dummyCollection = Factory.Load<DummyWithWorkflow>(task1Filter.Filter);

			AssertCollectionContains(dummyBO1, dummyCollection);

			task1Filter = new DummyFilterStripBizOWithWorkflowFiltersForOrdersJobs();
			((ModuleNkFilter)task1Filter["Next Task Assigned To"]).Property = "A.K";
			((ModuleNkFilter)task1Filter["Next Task Assigned To"]).IsActive = true;

			dummyCollection = Factory.Load<DummyWithWorkflow>(task1Filter.Filter);

			AssertCollectionContains(dummyBO1, dummyCollection);
		}

		public void TestAnyOpenTaskAssignedToFilter_GetsTaskForCurrentCompanyOnly()
		{
			GlbCompany anotherCompany = Factory.NewWithValidTestData<GlbCompany>();

			var bizObj1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizObj2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizObj3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1 = bizObj1.WorkflowItems.Tasks.AddNew();
			var task2 = bizObj2.WorkflowItems.Tasks.AddNew();
			var task3 = bizObj3.WorkflowItems.Tasks.AddNew();

			task1.P9_GS_NKAssignedStaffMember = "A.K";
			task2.P9_GS_NKAssignedStaffMember = "A.K";
			task3.P9_GS_NKAssignedStaffMember = "A.K";

			task1.P9_GC = GlbCompany.CurrentCompany.PK;
			task3.P9_GC = anotherCompany.PK;

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			Factory.Save();

			DummyFilterStripBizOWithWorkflowFilters taskFilter = new DummyFilterStripBizOWithWorkflowFilters();
			((ModuleNkFilter)taskFilter["Any Open Task Assigned To"]).Property = "A.K";
			((ModuleNkFilter)taskFilter["Any Open Task Assigned To"]).IsActive = true;

			var collection = Factory.Load<DummyWithWorkflow>(taskFilter.Filter);

			AssertCollectionContains(bizObj1, collection);
			AssertCollectionContains(bizObj2, collection);
			AssertCollectionNotContains(bizObj3, collection);

			taskFilter = new DummyFilterStripBizOWithWorkflowFilters(shouldFilterByCompanyForAnyOpenTask: false);
			((ModuleNkFilter)taskFilter["Any Open Task Assigned To"]).Property = "A.K";
			((ModuleNkFilter)taskFilter["Any Open Task Assigned To"]).IsActive = true;

			collection = Factory.Load<DummyWithWorkflow>(taskFilter.Filter);

			AssertCollectionContains(bizObj1, collection);
			AssertCollectionContains(bizObj2, collection);
			AssertCollectionContains(bizObj3, collection);
		}

		public void TestNextTaskAssignedToFilter_ForCustomDeclarationJobs()
		{
			(BusinessObject, ProcessTask, IWorkflowProvider) MakeItems()
			{
				var biz = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				var shipment1 = (IWorkflowProvider)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
				var task = shipment1.WorkflowItems.Tasks.AddNew();
				biz[JobDeclarationSchema.JE_JS] = shipment1.PK;
				return (biz, task, shipment1);
			}
			var (dummyBO1, task1, s1) = MakeItems();
			var (dummyBO2, task2, s2) = MakeItems();
			var (dummyBO3, task3, s3) = MakeItems();
			var (dummyBO4, task4, s4) = MakeItems();
			var task6 = s4.WorkflowItems.Tasks.AddNew();
			var (dummyBO5, task5, s5) = MakeItems();

			task1.P9_GS_NKAssignedStaffMember = "SM1";
			task2.P9_GS_NKAssignedStaffMember = "SM1";
			task3.P9_GS_NKAssignedStaffMember = "SM1";
			task4.P9_GS_NKAssignedStaffMember = "SM2";
			task5.P9_GS_NKAssignedStaffMember = "SM1";
			task6.P9_GS_NKAssignedStaffMember = "SM1";
			task1.P9_Status = "WRK";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = "WRK";
			task5.P9_Status = "WRK";
			task6.P9_Status = "WRK";
			task1.P9_Sequence = 5;
			task2.P9_Sequence = 1;
			task3.P9_Sequence = 2;
			task4.P9_Sequence = 3;
			task5.P9_Sequence = 4;
			task6.P9_Sequence = 6;

			Factory.Save();

			DummyFilterStripBizOWithWorkflowFiltersForCustomsDeclarationJobs task1Filter = new DummyFilterStripBizOWithWorkflowFiltersForCustomsDeclarationJobs();
			((ModuleNkFilter)task1Filter["Next Task Assigned To"]).Property = "SM1";
			((ModuleNkFilter)task1Filter["Next Task Assigned To"]).IsActive = true;

			IList list = Factory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(task1Filter.Filter);

			Assert("dummyBO1 should be present after applying this filter", list.Contains(dummyBO1));
			Assert("dummyBO2 should be filered out", !list.Contains(dummyBO2));
			Assert("dummyBO3 should be filered out", !list.Contains(dummyBO3));
			Assert("dummyBO4 should be filered out", !list.Contains(dummyBO4));
			Assert("dummyBO5 should be present after applying this filter", list.Contains(dummyBO5));
		}

		#endregion

		#region Not published on Web Filters

		public void TestNotPublishedOnWebFilters()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;
			var taskFilter = new DummyFilterStripBizOWithWorkflowFiltersForCustomsDeclarationJobs();

			AssertNotNull("Any Open Task Assigned To should not be null", ((ModuleNkFilter)taskFilter["Any Open Task Assigned To"]).IsPublishedOnWeb);
			AssertEquals("Any Open Task Assigned To should not be published on web", false, ((ModuleNkFilter)taskFilter["Any Open Task Assigned To"]).IsPublishedOnWeb);
			AssertNotNull("Next Task Assigned To should not be null", ((ModuleNkFilter)taskFilter["Next Task Assigned To"]).IsPublishedOnWeb);
			AssertEquals("Next Task Assigned To should not be published on web", false, ((ModuleNkFilter)taskFilter["Next Task Assigned To"]).IsPublishedOnWeb);
			AssertNull("Registry off so BM filter should not be there", taskFilter["Buffer Management Component"]);

			var type = ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var bmHelper = ObjectFactory.Get<IFilterStripsHelper>("IBMFilterStripsHelper", type, ZString.Empty, Factory);
			var bmTestCase = ObjectFactory.Get<AutomaticFilterTest>(bmHelper.GetAutomaticFilterTestCaseName_ForObjectFactory());
			bmTestCase.SetUpForHelperFiltersWorkTests(type);

			taskFilter = new DummyFilterStripBizOWithWorkflowFiltersForCustomsDeclarationJobs();
			AssertEquals("Buffer Management should not be published on web", false, ((ModuleGuidFilter)taskFilter["Buffer Management Component"]).IsPublishedOnWeb);
		}

		#endregion

		#region Custom Fields

		public void TestSetShouldAddWorkflowCustomFieldsFilters()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			template.P0_IsActive = true;

			var def = Factory.New<GenCustomColumnDefinition>();
			def.XC_ParentTableCode = DummyBusinessObjectWithWorkflow.Schema.TablePrefix;
			def.XC_Name = "Pinot Noir";
			def.XC_Type = "STR";
			def.XC_ParentID = template.PK;

			Factory.Save();

			var helper = new WorkflowFilterStripsHelper(typeof(DummyBusinessObjectWithWorkflow), "DUM", Factory);
			var filters = new ModuleFilterCollection();
			helper.AddFilterStrips(filters);

			AssertNotNull("SetShouldAddWorkflowCustomFieldsFilters was not called on the helper so the custom column filter should have been added, and yet...", filters["Pinot Noir"]);

			filters = new ModuleFilterCollection();
			helper.SetShouldAddWorkflowCustomFieldsFilters(false);
			helper.AddFilterStrips(filters);

			AssertNull("SetShouldAddWorkflowCustomFieldsFilters was called on the helper so the custom column filter should not have been added, and yet...", filters["Pinot Noir"]);
		}
		public void TestDuplicateCustomFields()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			template.P0_IsActive = true;

			var def1 = Factory.New<GenCustomColumnDefinition>();
			def1.XC_ParentTableCode = DummyBusinessObjectWithWorkflow.Schema.TablePrefix;
			def1.XC_Name = "Double Trouble";
			def1.XC_Type = "STR";
			def1.XC_ParentID = template.PK;

			var def2 = Factory.New<GenCustomColumnDefinition>();
			def2.XC_ParentTableCode = DummyBusinessObjectWithWorkflow.Schema.TablePrefix;
			def2.XC_Name = "Double Trouble";
			def2.XC_Type = "BOO";
			def2.XC_ParentID = template.PK;
			Factory.Save();

			var helper = new WorkflowFilterStripsHelper(typeof(DummyBusinessObjectWithWorkflow), "DUM", Factory);
			var filters = new ModuleFilterCollection();
			helper.AddFilterStrips(filters);

			AssertNotNull(filters.GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive("Double Trouble"));
		}

		#endregion

		#region Misc Filters

		public void TestAddMiscFilters()
		{
			var helper = new WorkflowFilterStripsHelper(typeof(DummyBusinessObjectWithWorkflow), "DUM", Factory);
			var filters = new ModuleFilterCollection();
			helper.AddFilterStrips(filters);
			CombineAssertions(() =>
			{
				var expectedFilterColumnName = DummyBusinessObjectWithWorkflow.Schema.PK;
				AssertMiscFilter(filters, "Tasks", expectedFilterColumnName);
				AssertMiscFilter(filters, "Milestones", expectedFilterColumnName);
				AssertMiscFilter(filters, "Triggers", expectedFilterColumnName);
				AssertMiscFilter(filters, "Exceptions", expectedFilterColumnName);
			});
		}

		protected void AssertMiscFilter(ModuleFilterCollection filters, string filterName, string assertFilterColumnName)
		{
			var filter = filters[filterName];
			AssertNotNull($"{filterName} filter", filter);
			AssertEquals($"{filterName} filter column name should be Primary Key", assertFilterColumnName, filter?.FilterColumn?.Name);
		}

		#endregion

		#region IndexSearch

		public void TestAddFilterStripsForIndexSearchWhenEmpty()
		{
			var helper = new WorkflowFilterStripsHelper(typeof(DummyBusinessObjectWithWorkflow), "DUM", Factory);
			var filters = new ModuleFilterCollection();
			helper.AddFilterStripsForIndexSearch(filters, []);
			AssertEquals(0, filters.Count());
		}

		public void TestAddFilterStripsForIndexSearch()
		{
			var helper = new WorkflowFilterStripsHelper(typeof(DummyBusinessObjectWithWorkflow), "DUM", Factory);
			var filters = new ModuleFilterCollection();
			helper.AddFilterStripsForIndexSearch(filters, SearchFields);
			AssertEquals(3, filters.Count());
			AssertNotNull(filters["Milestone Completed"]);
			AssertNotNull(filters["Next Milestone"]);
			AssertNotNull(filters["Last Completed Milestone"]);
		}

		public void TestAddFilterStripsForIndexSearchWhenFilterNotComplete()
		{
			var helper = new WorkflowFilterStripsHelper(typeof(DummyBusinessObjectWithWorkflow), "DUM", Factory);
			var filters = new ModuleFilterCollection();
			helper.AddFilterStripsForIndexSearch(filters, [SearchFields[0]]);
			AssertEquals(1, filters.Count());
		}

		public void TestIndexSearchMilestoneCompletedFilter()
		{
			var helper = new WorkflowFilterStripsHelper(typeof(DummyBusinessObjectWithWorkflow), "DUM", Factory);
			var filters = new ModuleFilterCollection();
			helper.AddFilterStripsForIndexSearch(filters, SearchFields);

			var filter = (IndexSearchModuleTextFilter)filters["Milestone Completed"];
			AssertEquals("Completed", filter.Property);
			AssertEquals("(MilestoneCompleted eq true)", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.Property = "Not Completed";
			AssertEquals("(MilestoneCompleted eq false)", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.Property = "All";
			AssertEquals("", filter.GetGlowIndexQuery().ToUrlComponent());
		}

		internal static SearchField[] SearchFields
		{
			get => new[]
			{
				new SearchField("MilestoneCompleted","MilestoneCompleted",typeof(bool)),
				new SearchField("LastCompletedMilestoneActualTime", "LastCompletedMilestoneActualTime", typeof(DateTime)),
				new SearchField("NextMilestoneEstimatedTime", "NextMilestoneEstimatedTime", typeof(DateTime))
			};
		}

		#endregion
	}
}
