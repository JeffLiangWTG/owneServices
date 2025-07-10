using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module.Testing
{
	sealed class WorkflowFilterStripsHelperCustomsTest : WorkflowFilterStripsHelperTest
	{
		public new void TestMilestoneCompletedFilter()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var milestone1 = declaration1.WorkflowItems.Milestones.AddNew();
			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			var trigger = declaration1.WorkflowItems.Triggers.AddNew();
			trigger.SetMilestoneActualDateForTest(ZDateTime.Empty);

			var shipment = Factory.New<ForwardingShipment>();
			var milestone2 = shipment.WorkflowItems.Milestones.AddNew();
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			var exception = shipment.WorkflowItems.Exceptions.AddNew();
			exception.SetMilestoneActualDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_JS = shipment.PK;

			Factory.Save();
			var allFilters = new DummyFilterStripBizOWithWorkflowFilters();
			var milestoneFilter = ((ModuleTextFilter)allFilters["Milestone Completed"]);
			milestoneFilter.Property = "Completed";
			milestoneFilter.IsActive = true;
			var collection = new BaseJobDeclarationCollection(Factory, allFilters.Filter);
			collection.Load();
			Assert(collection.Contains(declaration1));
			Assert(!collection.Contains(declaration2));
			milestoneFilter.Property = "Not Completed";
			milestoneFilter.IsActive = true;
			collection = new BaseJobDeclarationCollection(Factory, allFilters.Filter);
			collection.Load();
			Assert(!collection.Contains(declaration1));
			Assert(collection.Contains(declaration2));
		}

		public new void TestMilestoneNextFilter()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ProcessTask milestone1 = declaration.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = "AID";
			milestone1.P9_Type = Core.Constants.Workflow.MilestoneType;
			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
			milestone1.SetMilestoneActualDateForTest(ZDateTime.Empty);
			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			declaration2.JE_JS = shipment2.PK;
			ProcessTask milestone2 = shipment2.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = "AID";
			milestone2.P9_Type = Core.Constants.Workflow.MilestoneType;
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 1)));
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			Factory.Save();
			DummyFilterStripBizOWithWorkflowFilters milestone1Filter = new DummyFilterStripBizOWithWorkflowFilters();
			((WorkflowModuleFilter)milestone1Filter["Next Milestone"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((WorkflowModuleFilter)milestone1Filter["Next Milestone"]).Property1 = new ZDateTime(2000, 1, 1);
			((WorkflowModuleFilter)milestone1Filter["Next Milestone"]).Property2 = new ZDateTime(2000, 1, 3);
			((WorkflowModuleFilter)milestone1Filter["Next Milestone"]).IsActive = true;
			BaseJobDeclarationCollection dummyCollection = new BaseJobDeclarationCollection(Factory, milestone1Filter.Filter);
			dummyCollection.Load();
			Assert(dummyCollection.Contains(declaration));
			Assert(dummyCollection.Contains(declaration2));
		}

		public new void TestMilestoneLastCompletedFilter()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ProcessTask milestone1 = declaration.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Type = Core.Constants.Workflow.MilestoneType;
			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			declaration2.JE_JS = shipment2.PK;
			ProcessTask milestone2 = shipment2.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Type = Core.Constants.Workflow.MilestoneType;
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 1));
			Factory.Save();
			DummyFilterStripBizOWithWorkflowFilters milestone1Filter = new DummyFilterStripBizOWithWorkflowFilters();
			((WorkflowModuleFilter)milestone1Filter["Last Completed Milestone"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((WorkflowModuleFilter)milestone1Filter["Last Completed Milestone"]).Property1 = new ZDateTime(2000, 1, 1);
			((WorkflowModuleFilter)milestone1Filter["Last Completed Milestone"]).Property2 = new ZDateTime(2000, 1, 3);
			((WorkflowModuleFilter)milestone1Filter["Last Completed Milestone"]).IsActive = true;
			BaseJobDeclarationCollection dummyCollection = new BaseJobDeclarationCollection(Factory, milestone1Filter.Filter);
			dummyCollection.Load();
			Assert(dummyCollection.Contains(declaration));
			Assert(dummyCollection.Contains(declaration2));
		}

		public void TestGetDeclarationAndShipmentQuery()
		{
			const string expectedPredicate = "JE_ComputedParent IN (SELECT P9_ParentID FROM dbo.ProcessTasks WHERE (P9_ParentTableCode in ('JE', 'JS')))";

			var query = WorkflowFilterStripsHelperCustoms.GetDeclarationAndShipmentQuery(new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID));

			AssertEquals(query.LiteralTextADO, expectedPredicate);
		}

		public new void TestAddMiscFilters()
		{
			var helper = new WorkflowFilterStripsHelperCustoms(typeof(BaseJobDeclaration), WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorBrokerageAttachedCode, Factory);
			var filters = new ModuleFilterCollection();
			helper.AddFilterStrips(filters);
			CombineAssertions(() =>
			{
				var expectedFilterColumnName = JobDeclarationSchema.JE_ComputedParent.Name;
				AssertMiscFilter(filters, "Tasks", expectedFilterColumnName);
				AssertMiscFilter(filters, "Milestones", expectedFilterColumnName);
				AssertMiscFilter(filters, "Triggers", expectedFilterColumnName);
				AssertMiscFilter(filters, "Exceptions", expectedFilterColumnName);
			});
		}

		public void TestAnyOpenTaskAssignedToFilter_ForCustomsDeclarationOnShipment()
		{
			(ProcessTask, Integration.Customs.IBaseJobDeclaration) GetPair(Func<IWorkflowProvider, ProcessTask> makeTask)
			{
				var shipment = (IWorkflowProvider)Factory.New<Integration.Forwarding.IForwardingShipment>();
				var declaration = Factory.New<Integration.Customs.IBaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				var task = makeTask(shipment);
				return (task, declaration);
			}

			var (task1, declaration1) = GetPair(w => w.WorkflowItems.Tasks.AddNew());
			var (task2, declaration2) = GetPair(w => w.WorkflowItems.Tasks.AddNew());
			var (task3, declaration3) = GetPair(w => w.WorkflowItems.Tasks.AddNew());
			var (task4, declaration4) = GetPair(w => w.WorkflowItems.Tasks.AddNew());
			var (exception1, declaration5) = GetPair(w => w.WorkflowItems.Exceptions.AddNew());
			var (exception2, declaration6) = GetPair(w => w.WorkflowItems.Exceptions.AddNew());
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
			var task1Filter = new DummyFilterStripBizOWithWorkflowFilters();
			var anyTaskAssignedToFilter = (ModuleNkFilter)task1Filter["Any Open Task Assigned To"];
			anyTaskAssignedToFilter.Property = "SM1";
			anyTaskAssignedToFilter.IsActive = true;
			var list = (IList)Factory.Load<Integration.Customs.IBaseJobDeclaration>(task1Filter.Filter);
			Assert("declaration1 should be present after applying this filter", list.Contains(declaration1));
			Assert("declaration2 should be filered out", !list.Contains(declaration2));
			Assert("declaration3 should be filered out", !list.Contains(declaration3));
			Assert("declaration4 should be filered out", !list.Contains(declaration4));
			Assert("declaration5 is an exception, not a task so it should not appear", !list.Contains(declaration5));
			Assert("declaration6 should be filered out", !list.Contains(declaration6));
		}

		sealed class DummyFilterStripBizOWithWorkflowFilters : DummyFilterStripBusinessObject
		{
			public DummyFilterStripBizOWithWorkflowFilters()
			{
				QueryObjectType = typeof(BaseJobDeclaration);
			}

			protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
			{
				var helpers = base.GetCustomFilterStripsHelpersCore();
				helpers.Add(new WorkflowFilterStripsHelperCustoms(typeof(BaseJobDeclaration), ZString.Empty, Factory));
				return helpers;
			}
		}
	}
}
