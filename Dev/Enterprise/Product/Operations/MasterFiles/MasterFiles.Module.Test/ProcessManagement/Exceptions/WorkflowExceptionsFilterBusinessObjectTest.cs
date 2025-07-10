using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(WorkflowExceptionsFilterBusinessObject))]
	sealed class WorkflowExceptionsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Status and Flags

		public void TestActionedFilter()
		{
			ProcessTask actionedException = Dummy.WorkflowItems.Exceptions.AddNew();
			ProcessTask unactionedException = Dummy.WorkflowItems.Exceptions.AddNew();
			actionedException.P9_Type = Core.Constants.Workflow.ExceptionType;
			unactionedException.P9_Type = Core.Constants.Workflow.ExceptionType;
			actionedException.IsExceptionActioned = true;
			unactionedException.IsExceptionActioned = false;

			Factory.Save();

			WorkflowExceptionsFilterBusinessObject filter = new WorkflowExceptionsFilterBusinessObject();
			((ModuleTextFilter)filter["Actioned"]).IsActive = true;
			AssertEquals("Default value", "OPN", ((ModuleTextFilter)filter["Actioned"]).Property);

			ProcessTaskCollection tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);
			AssertCollectionNotContains(actionedException, tasks);
			AssertCollectionContains(unactionedException, tasks);

			((ModuleTextFilter)filter["Actioned"]).Property = "RSL";
			((ModuleTextFilter)filter["Actioned"]).IsActive = true;

			tasks.Load(filter.Filter);
			AssertCollectionContains(actionedException, tasks);
			AssertCollectionNotContains(unactionedException, tasks);

			((ModuleTextFilter)filter["Actioned"]).Property = "All";
			((ModuleTextFilter)filter["Actioned"]).IsActive = true;

			tasks.Load(filter.Filter);
			AssertCollectionContains(actionedException, tasks);
			AssertCollectionContains(unactionedException, tasks);
		}

		#endregion

		#region Text

		public void TestDescriptionFilter()
		{
			ProcessTask exception1 = Dummy.WorkflowItems.Exceptions.AddNew();
			ProcessTask exception2 = Dummy.WorkflowItems.Exceptions.AddNew();
			exception1.P9_Type = Core.Constants.Workflow.ExceptionType;
			exception2.P9_Type = Core.Constants.Workflow.ExceptionType;
			exception1.IsExceptionActioned = false;
			exception2.IsExceptionActioned = false;
			exception1.P9_Description = "desc";
			exception2.P9_Description = "anotherdesc";

			Factory.Save();

			WorkflowExceptionsFilterBusinessObject filter = new WorkflowExceptionsFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "desc";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			ProcessTaskCollection tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);

			AssertCollectionContains(exception1, tasks);
			AssertCollectionNotContains(exception2, tasks);
		}

		public void TestExceptionTypeFilter_IsAndIsNotBlank()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var dummy = Factory.New<DummyWithWorkflow>();

			var blankException = dummy.WorkflowItems.Exceptions.AddNew();
			blankException.P9_Description = "Blank";
			blankException.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var nonBlankException = dummy.WorkflowItems.Exceptions.AddNew();
			nonBlankException.P9_Description = "NonBlank";
			nonBlankException.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			nonBlankException.ExceptionTypeCode = "ABC";

			Factory.Save();

			var filterBizo = new WorkflowExceptionsFilterBusinessObject();
			filterBizo.AddNkFilterStrip("Assigned Staff", staff.GS_Code);
			var filter = filterBizo.AddTextFilterStrip("Exception Type Code");
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			ProcessTask[] results = null;

			AssertNoExceptionThrown("Using the 'is blank' comparison operator for this filter should work just fine. SAD!", () => results = Factory.Load<ProcessTask>(filterBizo.Filter));
			AssertContainsExactElementsInAnyOrder(new[] { "Blank" }, results.Select(x => x.P9_Description));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;

			AssertNoExceptionThrown("Using the 'is not blank' comparison operator for this filter should work just fine. SAD!", () => results = Factory.Load<ProcessTask>(filterBizo.Filter));
			AssertContainsExactElementsInAnyOrder(new[] { "NonBlank" }, results.Select(x => x.P9_Description));
		}

		void TestExceptionTypeFilter(string filterName, string filterValue)
		{
			var exception = Dummy.WorkflowItems.Exceptions.AddNew();
			exception.P9_Type = Core.Constants.Workflow.ExceptionType;
			exception.IsExceptionActioned = false;
			exception.ExceptionTypeCode = "ETC";

			var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType.WET_Category = "CT1";
			exceptionType.WET_JobType = "WKI";
			exceptionType.WET_Code = "ETC";
			exceptionType.WET_Description = "Some description";

			var cause = Factory.New<ProcessWorkflowExceptionCause>();
			cause.WEC_WET_Type = exceptionType.PK;
			cause.WEC_Code = "CAU";
			cause.WEC_Description = "Some cause";
			cause.WEC_IsActive = true;
			exceptionType.Causes.Add(cause);

			var resolution = Factory.New<ProcessWorkflowExceptionResolution>();
			resolution.WER_WET_Type = exceptionType.PK;
			resolution.WER_Code = "RES";
			resolution.WER_Description = "Some resolution";
			resolution.WER_IsActive = true;
			exceptionType.Resolutions.Add(resolution);

			var processWorkflowException = exception.ProcessWorkflowException;
			processWorkflowException.WEX_WEC_Cause = cause.PK;
			processWorkflowException.WEX_WER_Resolution = resolution.PK;

			Factory.Save();

			var filter = new WorkflowExceptionsFilterBusinessObject();
			((ModuleTextFilter)filter[filterName]).Property = filterValue;
			((ModuleTextFilter)filter[filterName]).IsActive = true;
			var tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);

			AssertCollectionContains(exception, tasks);
		}

		public void TestCategoryFilter() => TestExceptionTypeFilter("Category", "CT1");
		public void TestExceptionTypeCodeFilter() => TestExceptionTypeFilter("Exception Type Code", "ETC");
		public void TestWorkflowTypeFilter() => TestExceptionTypeFilter("Workflow Type", "DUM");
		public void TestCauseCodeFilter() => TestExceptionTypeFilter("Cause Code", "CAU");
		public void TestCauseDescriptionFilter() => TestExceptionTypeFilter("Cause Description", "Some cause");
		public void TestResolutionCodeFilter() => TestExceptionTypeFilter("Resolution Code", "RES");
		public void TestResolutionDescriptionFilter() => TestExceptionTypeFilter("Resolution Description", "Some resolution");

		#endregion

		#region Date

		public void TestScheduledDateFilter()
		{
			ProcessTask exception1 = Dummy.WorkflowItems.Exceptions.AddNew();
			ProcessTask exception2 = Dummy.WorkflowItems.Exceptions.AddNew();
			exception1.P9_Type = Core.Constants.Workflow.ExceptionType;
			exception2.P9_Type = Core.Constants.Workflow.ExceptionType;
			exception1.IsExceptionActioned = false;
			exception2.IsExceptionActioned = false;
			exception1.SetMilestoneActualDateForTest(new ZDateTimeOffset(new ZDateTime(2005, 1, 1)));
			exception2.SetMilestoneActualDateForTest(new ZDateTimeOffset(new ZDateTime(2005, 2, 1)));

			Factory.Save();

			WorkflowExceptionsFilterBusinessObject filter = new WorkflowExceptionsFilterBusinessObject();
			((ModuleDateFilter)filter["Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["Date"]).Property1 = new ZDateTime(2005, 1, 1);
			((ModuleDateFilter)filter["Date"]).Property2 = new ZDateTime(2005, 1, 3);
			((ModuleDateFilter)filter["Date"]).IsActive = true;

			ProcessTaskCollection tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);

			AssertCollectionContains(exception1, tasks);
			AssertCollectionNotContains(exception2, tasks);
		}

		[TestDate(2023, 1, 2)]
		public void TestScheduledActionedDateFilter()
		{
			ProcessTask exception1 = Dummy.WorkflowItems.Exceptions.AddNew();
			ProcessTask exception2 = Dummy.WorkflowItems.Exceptions.AddNew();
			exception1.P9_Type = Core.Constants.Workflow.ExceptionType;
			exception2.P9_Type = Core.Constants.Workflow.ExceptionType;
			exception1.IsExceptionActioned = false;
			exception2.IsExceptionActioned = false;

			Factory.Save();

			WorkflowExceptionsFilterBusinessObject filter = new WorkflowExceptionsFilterBusinessObject();
			((ModuleDateFilter)filter["Actioned Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["Actioned Date"]).Property1 = new ZDateTime(2023, 1, 1);
			((ModuleDateFilter)filter["Actioned Date"]).Property2 = new ZDateTime(2023, 1, 3);
			((ModuleDateFilter)filter["Actioned Date"]).IsActive = true;

			ProcessTaskCollection tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);

			AssertCollectionNotContains(exception1, tasks);
			AssertCollectionNotContains(exception2, tasks);

			exception1.IsExceptionActioned = true;

			Factory.Save();

			tasks.Load(filter.Filter);

			AssertCollectionContains(exception1, tasks);
			AssertCollectionNotContains(exception2, tasks);

			exception1.IsExceptionActioned = false;
			exception2.IsExceptionActioned = true;

			tasks.Load(filter.Filter);

			AssertCollectionNotContains(exception1, tasks);
			AssertCollectionContains(exception2, tasks);
		}

		#endregion

		#region Findbox Filters

		public void TestAssignedGroupFilter()
		{
			GlbGroup group1 = Factory.NewWithValidTestData<GlbGroup>();
			GlbGroup group2 = Factory.NewWithValidTestData<GlbGroup>();

			ProcessTask exception1 = Dummy.WorkflowItems.Exceptions.AddNew();
			ProcessTask exception2 = Dummy.WorkflowItems.Exceptions.AddNew();
			ProcessTask exception3 = Dummy.WorkflowItems.Exceptions.AddNew();

			exception1.IsExceptionActioned = false;
			exception2.IsExceptionActioned = false;
			exception3.IsExceptionActioned = false;

			exception1.P9_GG_AssignedGroup = group1.PK;
			exception2.P9_GG_AssignedGroup = group2.PK;
			exception3.P9_GG_AssignedGroup = group2.PK;

			Factory.Save();

			WorkflowExceptionsFilterBusinessObject group1Filter = new WorkflowExceptionsFilterBusinessObject();
			((ModuleGuidFilter)group1Filter["Assigned Group"]).Property = group1.PK;
			((ModuleGuidFilter)group1Filter["Assigned Group"]).IsActive = true;

			ProcessTaskCollection tasks = new ProcessTaskCollection(Factory);
			tasks.Load(group1Filter.Filter);

			AssertCollectionContains(exception1, tasks);
			AssertCollectionNotContains(exception2, tasks);
			AssertCollectionNotContains(exception3, tasks);

			WorkflowExceptionsFilterBusinessObject group2Filter = new WorkflowExceptionsFilterBusinessObject();
			((ModuleGuidFilter)group2Filter["Assigned Group"]).Property = group2.PK;
			((ModuleGuidFilter)group2Filter["Assigned Group"]).IsActive = true;

			tasks = new ProcessTaskCollection(Factory);
			tasks.Load(group2Filter.Filter);

			AssertCollectionNotContains(exception1, tasks);
			AssertCollectionContains(exception2, tasks);
			AssertCollectionContains(exception3, tasks);

			WorkflowExceptionsFilterBusinessObject emptyGlbGroupFilter = new WorkflowExceptionsFilterBusinessObject();
			((ModuleGuidFilter)emptyGlbGroupFilter["Assigned Group"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)emptyGlbGroupFilter["Assigned Group"]).IsActive = false;

			tasks = new ProcessTaskCollection(Factory);
			tasks.Load(emptyGlbGroupFilter.Filter);

			AssertCollectionContains(exception1, tasks);
			AssertCollectionContains(exception2, tasks);
			AssertCollectionContains(exception3, tasks);
		}

		public void TestAssignedStaffFilter()
		{
			ProcessTask exception1 = Dummy.WorkflowItems.Exceptions.AddNew();
			ProcessTask exception2 = Dummy.WorkflowItems.Exceptions.AddNew();
			exception1.P9_Type = Core.Constants.Workflow.ExceptionType;
			exception2.P9_Type = Core.Constants.Workflow.ExceptionType;
			exception1.IsExceptionActioned = false;
			exception2.IsExceptionActioned = false;
			exception1.P9_GS_NKAssignedStaffMember = "SM1";
			exception2.P9_GS_NKAssignedStaffMember = "SM2";

			Factory.Save();

			WorkflowExceptionsFilterBusinessObject filter = new WorkflowExceptionsFilterBusinessObject();
			((ModuleNkFilter)filter["Assigned Staff"]).Property = "SM1";
			((ModuleNkFilter)filter["Assigned Staff"]).IsActive = true;

			ProcessTaskCollection tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);

			AssertCollectionContains(exception1, tasks);
			AssertCollectionNotContains(exception2, tasks);
		}

		#endregion

		#region Parent Job

		public void TestParentJobFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "MYORGSYD";
			org1.OH_FullName = "Hitech Software";
			var task1 = org1.WorkflowItems.Tasks.AddNew();
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			var exception1 = org1.WorkflowItems.Exceptions.AddNew();
			exception1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "USORGSYD";
			org2.OH_FullName = "Lowtech Software";
			var task2 = org2.WorkflowItems.Tasks.AddNew();
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			var exception2 = org2.WorkflowItems.Exceptions.AddNew();
			exception2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var task3 = enquiry.WorkflowItems.Tasks.AddNew();
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			var exception3 = enquiry.WorkflowItems.Exceptions.AddNew();
			exception3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			var filterBizo = new WorkflowExceptionsFilterBusinessObject();
			var filter = (ModuleGuidModuleSpecifiedFilter)filterBizo["Parent Job"];
			filter.IsActive = true;
			filter.SelectedModule = "OrgHeader";
			filter.Property = org1.PK;

			var result = Factory.Load<ProcessTask>(filterBizo.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { exception1 }, result);

			filter.SelectedModule = ModuleIDs.SalesEnquiry.Name;
			filter.Property = enquiry.PK;

			result = Factory.Load<ProcessTask>(filterBizo.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { exception3 }, result);
		}

		#endregion

		#region Overall

		public void TestIncludeExceptionsOnly()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			ProcessTask milestone = Factory.New<ProcessTask>();
			ProcessTask exception = Factory.New<ProcessTask>();
			ProcessTask trigger = Factory.New<ProcessTask>();

			milestone.IsMilestone = true;
			exception.IsException = true;
			trigger.IsWorkflowTrigger = true;

			Factory.Save();

			ProcessTaskCollection tasks = new ProcessTaskCollection(Factory);
			tasks.Load((new WorkflowExceptionsFilterBusinessObject()).Filter);
			AssertCollectionNotContains("Tasks should NOT be loaded", task, tasks);
			AssertCollectionNotContains("Milestones should NOT be loaded", milestone, tasks);
			AssertCollectionContains("Workflow exceptions should be loaded", exception, tasks);
			AssertCollectionNotContains("Triggers should NOT be loaded", trigger, tasks);
		}

		#endregion

		#region Implementation

		DummyWithWorkflow Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithWorkflow>()); }
		}

		DummyWithWorkflow dummy;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WorkflowExceptionsFilterBusinessObject();
		}

		#endregion
	}
}
