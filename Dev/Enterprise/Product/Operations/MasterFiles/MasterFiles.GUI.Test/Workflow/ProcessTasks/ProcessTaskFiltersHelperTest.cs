using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ProcessTaskFiltersHelperTest : TestCaseWithFactory
	{
		#region Current Task Filters

		public void TestAddCurrentTaskAssignedToFilter()
		{
			var dummy1 = Factory.New<DummyWithWorkflow>();
			var dummy2 = Factory.New<DummyWithWorkflow>();
			var dummy3 = Factory.New<DummyWithWorkflow>();
			var dummy4 = Factory.New<DummyWithWorkflow>();
			AddTask(dummy1, 1, ProcessTaskStatusCodeList.Codes.Assigned, "ADL");
			AddTask(dummy2, 1, ProcessTaskStatusCodeList.Codes.Closed, "ADL");
			AddTask(dummy2, 2, ProcessTaskStatusCodeList.Codes.Assigned, "SCW");
			AddTask(dummy3, 1, ProcessTaskStatusCodeList.Codes.Open, "ADL");

			Factory.Save();

			var helper = new ProcessTaskFiltersHelper<DummyWithWorkflow>(Factory);
			var filterCollection = new ModuleFilterCollection();
			var currentTaskAssignedToFilter = helper.AddCurrentTaskAssignedToFilter(filterCollection);
			AssertFilterCollection(new[] { dummy1, dummy2, dummy3, dummy4 }, currentTaskAssignedToFilter);

			currentTaskAssignedToFilter.IsActive = true;
			currentTaskAssignedToFilter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.Exact;
			currentTaskAssignedToFilter.Property = "ADL";
			AssertFilterCollection(new[] { dummy1 }, currentTaskAssignedToFilter);

			currentTaskAssignedToFilter.Property = "SCW";
			AssertFilterCollection(new[] { dummy2 }, currentTaskAssignedToFilter);

			currentTaskAssignedToFilter.Property = "RIS";
			AssertFilterCollection(Enumerable.Empty<DummyWithWorkflow>(), currentTaskAssignedToFilter);

			currentTaskAssignedToFilter.Property = "";
			currentTaskAssignedToFilter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.IsBlank;
			AssertFilterCollection(Enumerable.Empty<DummyWithWorkflow>(), currentTaskAssignedToFilter);

			currentTaskAssignedToFilter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.IsNotBlank;
			AssertFilterCollection(new[] { dummy1, dummy2 }, currentTaskAssignedToFilter);
		}

		public void TestAddCurrentTaskAssignedGroupFilter()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var group3 = Factory.NewWithValidTestData<GlbGroup>();

			var dummy1 = Factory.New<DummyWithWorkflow>();
			var dummy2 = Factory.New<DummyWithWorkflow>();
			var dummy3 = Factory.New<DummyWithWorkflow>();
			var dummy4 = Factory.New<DummyWithWorkflow>();
			AddTask(dummy1, 1, ProcessTaskStatusCodeList.Codes.Assigned).P9_GG_AssignedGroup = group1.PK;
			AddTask(dummy2, 1, ProcessTaskStatusCodeList.Codes.Closed).P9_GG_AssignedGroup = group1.PK;
			AddTask(dummy2, 2, ProcessTaskStatusCodeList.Codes.Assigned).P9_GG_AssignedGroup = group2.PK;
			AddTask(dummy3, 1, ProcessTaskStatusCodeList.Codes.Closed).P9_GG_AssignedGroup = group1.PK;

			Factory.Save();

			var helper = new ProcessTaskFiltersHelper<DummyWithWorkflow>(Factory);
			var filterCollection = new ModuleFilterCollection();
			var currentTaskAssignedToFilter = helper.AddCurrentTaskAssignedGroupFilter(filterCollection);
			AssertFilterCollection(new[] { dummy1, dummy2, dummy3, dummy4 }, currentTaskAssignedToFilter);

			currentTaskAssignedToFilter.IsActive = true;
			currentTaskAssignedToFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			currentTaskAssignedToFilter.Property = group1.PK;
			AssertFilterCollection(new[] { dummy1 }, currentTaskAssignedToFilter);

			currentTaskAssignedToFilter.Property = group2.PK;
			AssertFilterCollection(new[] { dummy2 }, currentTaskAssignedToFilter);

			currentTaskAssignedToFilter.Property = group3.PK;
			AssertFilterCollection(Enumerable.Empty<DummyWithWorkflow>(), currentTaskAssignedToFilter);

			currentTaskAssignedToFilter.Property = group1.PK;
			currentTaskAssignedToFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			AssertFilterCollection(new[] { dummy2 }, currentTaskAssignedToFilter);

			currentTaskAssignedToFilter.Property = ZGuid.Empty;
			currentTaskAssignedToFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsBlank;
			AssertFilterCollection(Enumerable.Empty<DummyWithWorkflow>(), currentTaskAssignedToFilter);

			currentTaskAssignedToFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsNotBlank;
			AssertFilterCollection(new[] { dummy1, dummy2 }, currentTaskAssignedToFilter);
		}

		public void TestAddCurrentTaskStatusFilter()
		{
			var dummy1 = Factory.New<DummyWithWorkflow>();
			AddTask(dummy1, 1, ProcessTaskStatusCodeList.Codes.Assigned);

			var dummy2 = Factory.New<DummyWithWorkflow>();
			AddTask(dummy2, 1, ProcessTaskStatusCodeList.Codes.Closed);
			AddTask(dummy2, 2, ProcessTaskStatusCodeList.Codes.Working);

			var dummy3 = Factory.New<DummyWithWorkflow>();
			AddTask(dummy3, 1, ProcessTaskStatusCodeList.Codes.Open);

			var dummy4 = Factory.New<DummyWithWorkflow>();
			AddTask(dummy4, 1, ProcessTaskStatusCodeList.Codes.Cancelled);

			var dummy5 = Factory.New<DummyWithWorkflow>();
			AddTask(dummy5, 1, ProcessTaskStatusCodeList.Codes.Open);
			AddTask(dummy5, 2, ProcessTaskStatusCodeList.Codes.Working);

			var dummy6 = Factory.New<DummyWithWorkflow>();
			AddTask(dummy6, 1, ProcessTaskStatusCodeList.Codes.Cancelled);
			AddTask(dummy6, 2, ProcessTaskStatusCodeList.Codes.Closed);

			var dummy7 = Factory.New<DummyWithWorkflow>();

			Factory.Save();

			var helper = new ProcessTaskFiltersHelper<DummyWithWorkflow>(Factory);
			var filterCollection = new ModuleFilterCollection();
			var currentTaskStatusFilter = helper.AddCurrentTaskStatusFilter(filterCollection);
			AssertFilterCollection(new[] { dummy1, dummy2, dummy3, dummy4, dummy5, dummy6, dummy7 }, currentTaskStatusFilter);

			currentTaskStatusFilter.IsActive = true;
			currentTaskStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			currentTaskStatusFilter.Property = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertFilterCollection(new[] { dummy1 }, currentTaskStatusFilter);

			currentTaskStatusFilter.Property = ProcessTaskStatusCodeList.Codes.Working;
			AssertFilterCollection(new[] { dummy2, dummy5 }, currentTaskStatusFilter);

			currentTaskStatusFilter.Property = ProcessTaskStatusCodeList.Codes.Open;
			AssertFilterCollection(new[] { dummy3 }, currentTaskStatusFilter);

			currentTaskStatusFilter.Property = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertFilterCollection(new[] { dummy4 }, currentTaskStatusFilter);

			currentTaskStatusFilter.Property = ProcessTaskStatusCodeList.Codes.Closed;
			AssertFilterCollection(new[] { dummy6 }, currentTaskStatusFilter);

			currentTaskStatusFilter.Property = ProcessTaskStatusCodeList.Codes.Assigned;
			currentTaskStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			AssertFilterCollection(new[] { dummy2, dummy3, dummy4, dummy5, dummy6, dummy7 }, currentTaskStatusFilter);

			currentTaskStatusFilter.Property = "";
			currentTaskStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertFilterCollection(new[] { dummy7 }, currentTaskStatusFilter);

			currentTaskStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			AssertFilterCollection(new[] { dummy1, dummy2, dummy3, dummy4, dummy5, dummy6 }, currentTaskStatusFilter);
		}

		public void TestAddCurrentTaskScheduledStartFilter()
		{
			var dummy1 = Factory.New<DummyWithWorkflow>();
			var dummy2 = Factory.New<DummyWithWorkflow>();
			var dummy3 = Factory.New<DummyWithWorkflow>();
			var dummy4 = Factory.New<DummyWithWorkflow>();
			AddTask(dummy1, 1, ProcessTaskStatusCodeList.Codes.Assigned).P9_ScheduledDate = new ZDateTime(2013, 8, 1);
			AddTask(dummy2, 1, ProcessTaskStatusCodeList.Codes.Closed).P9_ScheduledDate = new ZDateTime(2013, 8, 1);
			AddTask(dummy2, 2, ProcessTaskStatusCodeList.Codes.Working).P9_ScheduledDate = ZDateTime.Empty;
			AddTask(dummy3, 1, ProcessTaskStatusCodeList.Codes.Open).P9_ScheduledDate = new ZDateTime(2013, 8, 1);

			Factory.Save();

			var helper = new ProcessTaskFiltersHelper<DummyWithWorkflow>(Factory);
			var filterCollection = new ModuleFilterCollection();
			var currentTaskScheduledStartFilter = helper.AddCurrentTaskScheduledStartFilter(filterCollection);
			AssertFilterCollection(new[] { dummy1, dummy2, dummy3, dummy4 }, currentTaskScheduledStartFilter);

			currentTaskScheduledStartFilter.IsActive = true;
			currentTaskScheduledStartFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			currentTaskScheduledStartFilter.Property1 = new ZDateTime(2013, 7, 1);
			currentTaskScheduledStartFilter.Property2 = new ZDateTime(2013, 9, 1);
			AssertFilterCollection(new[] { dummy1 }, currentTaskScheduledStartFilter);

			currentTaskScheduledStartFilter.Property1 = new ZDateTime(2013, 9, 1);
			currentTaskScheduledStartFilter.Property2 = new ZDateTime(2013, 9, 1);
			AssertFilterCollection(Enumerable.Empty<DummyWithWorkflow>(), currentTaskScheduledStartFilter);

			currentTaskScheduledStartFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertFilterCollection(new[] { dummy1 }, currentTaskScheduledStartFilter);

			currentTaskScheduledStartFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertFilterCollection(new[] { dummy2 }, currentTaskScheduledStartFilter);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestFiltersQueryUtcScheduledDate()
		{
			var dummy1 = Factory.New<DummyWithWorkflow>();
			var baseDate = new ZDateTime(2013, 8, 1, 12, 0, 0);
			AddTask(dummy1, 1, ProcessTaskStatusCodeList.Codes.Assigned).P9_ScheduledDate = baseDate;

			Factory.Save();

			var helper = new ProcessTaskFiltersHelper<DummyWithWorkflow>(Factory);
			var filterCollection = new ModuleFilterCollection();
			var currentTaskScheduledStartFilter = helper.AddCurrentTaskScheduledStartFilter(filterCollection);
			AssertFilterCollection(new[] { dummy1 }, currentTaskScheduledStartFilter);

			currentTaskScheduledStartFilter.IsActive = true;
			currentTaskScheduledStartFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			currentTaskScheduledStartFilter.Property1 = baseDate.AddMinutes(-5);
			currentTaskScheduledStartFilter.Property2 = baseDate.AddMinutes(5);
			AssertFilterCollection(new[] { dummy1 }, currentTaskScheduledStartFilter);
		}

		public void TestAddCurrentTaskActualStartFilter()
		{
			var dummy1 = Factory.New<DummyWithWorkflow>();
			var dummy2 = Factory.New<DummyWithWorkflow>();
			var dummy3 = Factory.New<DummyWithWorkflow>();
			var dummy4 = Factory.New<DummyWithWorkflow>();
			AddTask(dummy1, 1, ProcessTaskStatusCodeList.Codes.Assigned).TaskProperties.ActualDate = new ZDateTimeOffset(new ZDateTime(2013, 8, 1));
			AddTask(dummy2, 1, ProcessTaskStatusCodeList.Codes.Closed).TaskProperties.ActualDate = new ZDateTimeOffset(new ZDateTime(2013, 8, 1));
			AddTask(dummy2, 2, ProcessTaskStatusCodeList.Codes.Working).TaskProperties.ActualDate = ZDateTimeOffset.Empty;
			AddTask(dummy3, 1, ProcessTaskStatusCodeList.Codes.Open).TaskProperties.ActualDate = new ZDateTimeOffset(new ZDateTime(2013, 8, 1));

			Factory.Save();

			var helper = new ProcessTaskFiltersHelper<DummyWithWorkflow>(Factory);
			var filterCollection = new ModuleFilterCollection();
			var currentTaskScheduledStartFilter = helper.AddCurrentTaskActualStartFilter(filterCollection);
			AssertFilterCollection(new[] { dummy1, dummy2, dummy3, dummy4 }, currentTaskScheduledStartFilter);

			currentTaskScheduledStartFilter.IsActive = true;
			currentTaskScheduledStartFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			currentTaskScheduledStartFilter.Property1 = new ZDateTime(2013, 7, 1);
			currentTaskScheduledStartFilter.Property2 = new ZDateTime(2013, 9, 1);
			AssertFilterCollection(new[] { dummy1 }, currentTaskScheduledStartFilter);

			currentTaskScheduledStartFilter.Property1 = new ZDateTime(2013, 9, 1);
			currentTaskScheduledStartFilter.Property2 = new ZDateTime(2013, 9, 1);
			AssertFilterCollection(Enumerable.Empty<DummyWithWorkflow>(), currentTaskScheduledStartFilter);

			currentTaskScheduledStartFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertFilterCollection(new[] { dummy1 }, currentTaskScheduledStartFilter);

			currentTaskScheduledStartFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertFilterCollection(new[] { dummy2 }, currentTaskScheduledStartFilter);
		}

		#endregion

		#region Implementation

		ProcessTask AddTask(IWorkflowProvider workflowProvider, int sequence, string status, string staffCode = "")
		{
			var task = workflowProvider.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = staffCode;
			task.P9_Sequence = sequence;
			task.P9_Status = status;
			return task;
		}

		void AssertFilterCollection(IEnumerable<DummyWithWorkflow> expectedDummies, ModuleFilter filter)
		{
			var query = new ModuleFilterCombiner().GetCombinedFilter(new[] { filter });
			AssertContainsExactElementsInAnyOrder(expectedDummies, Factory.Load<DummyWithWorkflow>(query));
		}

		#endregion
	}
}
