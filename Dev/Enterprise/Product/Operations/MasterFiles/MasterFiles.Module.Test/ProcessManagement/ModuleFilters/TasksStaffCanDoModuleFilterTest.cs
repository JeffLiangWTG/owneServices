using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(TasksStaffCanDoModuleFilter))]
	sealed class TasksStaffCanDoModuleFilterTest : ModuleFilterTestCase<TasksStaffCanDoModuleFilter>
	{
		#region Basic stuff

		public void TestAllowedComparisonOperators()
		{
			AssertContainsExactElementsInAnyOrder(new[]
			{
				ModuleTextFilter.ComparisonConstants.Exact,
				ModuleTextFilter.ComparisonConstants.CurrentUser,
				ModuleTextFilter.ComparisonConstants.FiltersMatch,
			}, Filter.AllowedComparisonOperators);
		}

		public void TestDefaultComparisonOperator()
		{
			AssertEquals(ModuleTextFilter.ComparisonConstants.CurrentUser, Filter.ComparisonOperator);
		}

		public void TestDefaultMode()
		{
			AssertEquals(TasksStaffCanDoModuleFilterModes.Codes.EitherStaffOrCapabilityAndUnassignedStaff, Filter.Mode);
		}

		#endregion

		#region Filter results

		public void TestTasksAssignedToStaff()
		{
			const string mode = TasksStaffCanDoModuleFilterModes.Codes.OnlyStaff;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.ResourcesWithCapability.Add(staff1);
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.ResourcesWithCapability.Add(staff2);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			CreateTask(job, "Task 1", staff1, null);
			CreateTask(job, "Task 2", staff2, null);
			CreateTask(job, "Task 3", null, capability1);
			CreateTask(job, "Task 4", null, capability2);
			CreateTask(job, "Task 5", staff1, capability1);
			CreateTask(job, "Task 6", staff1, capability2);
			CreateTask(job, "Task 7", staff2, capability1);
			CreateTask(job, "Task 8", staff2, capability2);

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals(ModuleTextFilter.ComparisonConstants.CurrentUser, Filter.ComparisonOperator);
				AssertFilterResults("Only tasks assigned directly to the current user should be selected.", mode, "Task 1", "Task 5", "Task 6");

				using (Env.SetTemporaryUserContext(staff2.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					AssertFilterResults("Only tasks assigned directly to the current user (who is now staff2) should be selected.", mode, "Task 2", "Task 7", "Task 8");
				}

				Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				Filter.Property = staff1.GS_Code;
				AssertFilterResults("Only tasks assigned directly to the current user should be selected.", mode, "Task 1", "Task 5", "Task 6");

				Filter.Property = staff2.GS_Code;
				AssertFilterResults("Only tasks assigned directly to the other user should be selected.", mode, "Task 2", "Task 7", "Task 8");

				Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				Filter.SelectedFilters.AddTextFilterStrip("Active Status", FilterStripBusinessObject.StatusActive);
				AssertFilterResults("Both staff match the selected filters so any tasks assigned directly to either staff should be selected.", mode, "Task 1", "Task 2", "Task 5", "Task 6", "Task 7", "Task 8");

				staff2.GS_IsActive = false;
				Factory.Save();
				AssertFilterResults("The current user is now the only one that matches the selected filters.", mode, "Task 1", "Task 5", "Task 6");
			}
		}

		public void TestTasksAssignedToCapabilitiesRegardlessOfStaffAssignment_NoReleaseGroup()
		{
			const string mode = TasksStaffCanDoModuleFilterModes.Codes.RequiredCapabilityRegardlessOfStaffAssignment;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.ResourcesWithCapability.Add(staff1);
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.ResourcesWithCapability.Add(staff2);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			CreateTask(job, "Task 1", staff1, null);
			CreateTask(job, "Task 2", staff2, null);
			CreateTask(job, "Task 3", null, capability1);
			CreateTask(job, "Task 4", null, capability2);
			CreateTask(job, "Task 5", staff1, capability1);
			CreateTask(job, "Task 6", staff1, capability2);
			CreateTask(job, "Task 7", staff2, capability1);
			CreateTask(job, "Task 8", staff2, capability2);

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals(ModuleTextFilter.ComparisonConstants.CurrentUser, Filter.ComparisonOperator);
				AssertFilterResults("Only tasks assigned to the current user's capabilities should be selected.", mode, "Task 3", "Task 5", "Task 7");

				using (Env.SetTemporaryUserContext(staff2.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					AssertFilterResults("Only tasks assigned directly to the current user's capabilities (who is now staff2) should be selected.", mode, "Task 4", "Task 6", "Task 8");
				}

				Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				Filter.Property = staff1.GS_Code;
				AssertFilterResults("Only tasks assigned to the current user's capabilities should be selected.", mode, "Task 3", "Task 5", "Task 7");

				Filter.Property = staff2.GS_Code;
				AssertFilterResults("Only tasks assigned to the other user's capabilities should be selected.", mode, "Task 4", "Task 6", "Task 8");

				Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				Filter.SelectedFilters.AddTextFilterStrip("Active Status", FilterStripBusinessObject.StatusActive);
				AssertFilterResults("Both staff match the selected filters so any tasks assigned to either staff's capabilities should be selected.", mode, "Task 3", "Task 4", "Task 5", "Task 6", "Task 7", "Task 8");

				staff2.GS_IsActive = false;
				Factory.Save();
				AssertFilterResults("The current user is now the only one that matches the selected filters.", mode, "Task 3", "Task 5", "Task 7");
			}
		}

		public void TestTasksAssignedToCapabilitiesWithNoStaffAssignment_NoReleaseGroup()
		{
			const string mode = TasksStaffCanDoModuleFilterModes.Codes.RequiredCapabilityAndUnassignedStaff;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.ResourcesWithCapability.Add(staff1);
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.ResourcesWithCapability.Add(staff2);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			CreateTask(job, "Task 1", staff1, null);
			CreateTask(job, "Task 2", staff2, null);
			CreateTask(job, "Task 3", null, capability1);
			CreateTask(job, "Task 4", null, capability2);
			CreateTask(job, "Task 5", staff1, capability1);
			CreateTask(job, "Task 6", staff1, capability2);
			CreateTask(job, "Task 7", staff2, capability1);
			CreateTask(job, "Task 8", staff2, capability2);

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals(ModuleTextFilter.ComparisonConstants.CurrentUser, Filter.ComparisonOperator);
				AssertFilterResults("Only tasks assigned to the current user's capabilities, but not assigned directly to any staff, should be selected.", mode, "Task 3");

				using (Env.SetTemporaryUserContext(staff2.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					AssertFilterResults("Only tasks assigned directly to the current user's capabilities (who is now staff2), but not assigned directly to any staff, should be selected.", mode, "Task 4");
				}

				Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				Filter.Property = staff1.GS_Code;
				AssertFilterResults("Only tasks assigned to the specified user's capabilities, but not assigned directly to any staff, should be selected.", mode, "Task 3");

				Filter.Property = staff2.GS_Code;
				AssertFilterResults("Only tasks assigned to the other user's capabilities, but not assigned directly to any staff, should be selected.", mode, "Task 4");

				Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				Filter.SelectedFilters.AddTextFilterStrip("Active Status", FilterStripBusinessObject.StatusActive);
				AssertFilterResults("Both staff match the selected filters so any tasks assigned to either staff's capabilities, but not assigned directly to any staff, should be selected.", mode, "Task 3", "Task 4");

				staff2.GS_IsActive = false;
				Factory.Save();
				AssertFilterResults("The current user is now the only one that matches the selected filters.", mode, "Task 3");
			}
		}

		public void TestTasksAssignedToCapabilities_WorkflowReleaseGroupOnly()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.ResourcesWithCapability.Add(staff1);
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.ResourcesWithCapability.Add(staff2);
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var releaseGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup1.Staff.Add(staff1);
			var releaseGroup2 = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup2.Staff.Add(staff2);

			var jobLevelWorkflow = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = helper.CreateWorkflow(jobLevelWorkflow, "Workflow 1", releaseGroup1.PK.ToGuid());
			helper.CreateTask(workflow1, description: "Task 1", capability: capability1);
			helper.CreateTask(workflow1, description: "Task 2", capability: capability2);

			var workflow2 = helper.CreateWorkflow(jobLevelWorkflow, "Workflow 2", releaseGroup2.PK.ToGuid());
			helper.CreateTask(workflow2, description: "Task 3", capability: capability1);
			helper.CreateTask(workflow2, description: "Task 4", capability: capability2);

			Factory.Save();

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			Filter.Property = staff1.GS_Code;
			AssertFilterResults("The staff needs to be in the task's capability and it's workflow's group.", TasksStaffCanDoModuleFilterModes.Codes.RequiredCapabilityRegardlessOfStaffAssignment, "Task 1");

			Filter.Property = staff2.GS_Code;
			AssertFilterResults("The staff needs to be in the task's capability and it's workflow's group.", TasksStaffCanDoModuleFilterModes.Codes.RequiredCapabilityRegardlessOfStaffAssignment, "Task 4");
		}

		public void TestTasksAssignedToCapabilities_TaskReleaseGroupOnly()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.ResourcesWithCapability.Add(staff1);
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.ResourcesWithCapability.Add(staff2);
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var releaseGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup1.Staff.Add(staff1);
			var releaseGroup2 = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup2.Staff.Add(staff2);

			var jobLevelWorkflow = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = helper.CreateWorkflow(jobLevelWorkflow, "Workflow");
			helper.CreateTask(workflow1, description: "Task 1", capability: capability1, group: releaseGroup1);
			helper.CreateTask(workflow1, description: "Task 2", capability: capability2, group: releaseGroup1);
			helper.CreateTask(workflow1, description: "Task 3", capability: capability1, group: releaseGroup2);
			helper.CreateTask(workflow1, description: "Task 4", capability: capability2, group: releaseGroup2);

			Factory.Save();

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			Filter.Property = staff1.GS_Code;
			AssertFilterResults("The staff needs to be in the task's capability and group.", TasksStaffCanDoModuleFilterModes.Codes.RequiredCapabilityRegardlessOfStaffAssignment, "Task 1");

			Filter.Property = staff2.GS_Code;
			AssertFilterResults("The staff needs to be in the task's capability and group.", TasksStaffCanDoModuleFilterModes.Codes.RequiredCapabilityRegardlessOfStaffAssignment, "Task 4");
		}

		public void TestTasksAssignedToCapabilities_DifferentWorkflowAndTaskReleaseGroups()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.ResourcesWithCapability.Add(staff1);
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.ResourcesWithCapability.Add(staff2);
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var releaseGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup1.Staff.Add(staff1);
			var releaseGroup2 = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup2.Staff.Add(staff2);

			var jobLevelWorkflow = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = helper.CreateWorkflow(jobLevelWorkflow, "Workflow", releaseGroup1.PK.ToGuid());
			helper.CreateTask(workflow1, description: "Task 1", capability: capability1, group: releaseGroup1);
			helper.CreateTask(workflow1, description: "Task 2", capability: capability2, group: releaseGroup1);
			helper.CreateTask(workflow1, description: "Task 3", capability: capability1, group: releaseGroup2);
			helper.CreateTask(workflow1, description: "Task 4", capability: capability2, group: releaseGroup2);

			Factory.Save();

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			Filter.Property = staff1.GS_Code;
			AssertFilterResults("The staff needs to be in the task's capability and group.", TasksStaffCanDoModuleFilterModes.Codes.RequiredCapabilityRegardlessOfStaffAssignment, "Task 1");

			Filter.Property = staff2.GS_Code;
			AssertFilterResults("The task group should override the workflow's release group.", TasksStaffCanDoModuleFilterModes.Codes.RequiredCapabilityRegardlessOfStaffAssignment, "Task 4");
		}

		public void TestTasksAssignedToCapabilities_DifferentWorkflowAndTaskReleaseGroups_TaskCapabilityIsGlobal()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.ResourcesWithCapability.Add(staff1);
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var releaseGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup1.Staff.Add(staff1);
			var releaseGroup2 = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup2.Staff.Add(staff2);

			var jobLevelWorkflow = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = helper.CreateWorkflow(jobLevelWorkflow, "Workflow", releaseGroup1.PK.ToGuid());
			helper.CreateTask(workflow1, description: "Task 1", capability: capability1, group: releaseGroup1);
			helper.CreateTask(workflow1, description: "Task 2", capability: capability1, group: releaseGroup2);

			Factory.Save();

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			Filter.Property = staff1.GS_Code;
			AssertFilterResults("The capability is global so having a group specified on the task doesn't matter, even though the user isn't a member of that group.", TasksStaffCanDoModuleFilterModes.Codes.RequiredCapabilityRegardlessOfStaffAssignment, "Task 1", "Task 2");
		}

		public void TestTasksAssignedToStaffOrCapabilitiesRegardlessOfStaffAssignment()
		{
			const string mode = TasksStaffCanDoModuleFilterModes.Codes.EitherStaffOrCapabilityRegardlessOfStaffAssignment;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.ResourcesWithCapability.Add(staff1);
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.ResourcesWithCapability.Add(staff2);
			AssertEquals("GLB", capability1.G4_CapacityScope);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			CreateTask(job, "Task 1", staff1, null);
			CreateTask(job, "Task 2", staff2, null);
			CreateTask(job, "Task 3", null, capability1);
			CreateTask(job, "Task 4", null, capability2);
			CreateTask(job, "Task 5", staff1, capability1);
			CreateTask(job, "Task 6", staff1, capability2);
			CreateTask(job, "Task 7", staff2, capability1);
			CreateTask(job, "Task 8", staff2, capability2);

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals(ModuleTextFilter.ComparisonConstants.CurrentUser, Filter.ComparisonOperator);
				AssertFilterResults("Only tasks assigned to the current user or their capabilities should be selected.", mode, "Task 1", "Task 3", "Task 5", "Task 6", "Task 7");

				using (Env.SetTemporaryUserContext(staff2.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					AssertFilterResults("Only tasks assigned directly to the current user or their capabilities (who is now staff2) should be selected.", mode, "Task 2", "Task 4", "Task 6", "Task 7", "Task 8");
				}

				Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				Filter.Property = staff1.GS_Code;
				AssertFilterResults("Only tasks assigned to the current user or their capabilities should be selected.", mode, "Task 1", "Task 3", "Task 5", "Task 6", "Task 7");

				Filter.Property = staff2.GS_Code;
				AssertFilterResults("Only tasks assigned to the other user or their capabilities should be selected.", mode, "Task 2", "Task 4", "Task 6", "Task 7", "Task 8");

				Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				Filter.SelectedFilters.AddTextFilterStrip("Active Status", FilterStripBusinessObject.StatusActive);
				AssertFilterResults("Both staff match the selected filters so any tasks assigned to either staff or their capabilities should be selected.", mode, "Task 1", "Task 2", "Task 3", "Task 4", "Task 5", "Task 6", "Task 7", "Task 8");

				staff2.GS_IsActive = false;
				Factory.Save();
				AssertFilterResults("The current user is now the only one that matches the selected filters.", mode, "Task 1", "Task 3", "Task 5", "Task 6", "Task 7");
			}
		}

		public void TestTasksAssignedToStaffOrCapabilitiesWithNoStaffAssignment()
		{
			const string mode = TasksStaffCanDoModuleFilterModes.Codes.EitherStaffOrCapabilityAndUnassignedStaff;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.ResourcesWithCapability.Add(staff1);
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.ResourcesWithCapability.Add(staff2);
			AssertEquals("GLB", capability1.G4_CapacityScope);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			CreateTask(job, "Task 1", staff1, null);
			CreateTask(job, "Task 2", staff2, null);
			CreateTask(job, "Task 3", null, capability1);
			CreateTask(job, "Task 4", null, capability2);
			CreateTask(job, "Task 5", staff1, capability1);
			CreateTask(job, "Task 6", staff1, capability2);
			CreateTask(job, "Task 7", staff2, capability1);
			CreateTask(job, "Task 8", staff2, capability2);

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals(ModuleTextFilter.ComparisonConstants.CurrentUser, Filter.ComparisonOperator);
				AssertFilterResults("Only tasks assigned to the current user or (their capabilities but not assigned directly to any other user) should be selected.", mode, "Task 1", "Task 3", "Task 5", "Task 6");

				using (Env.SetTemporaryUserContext(staff2.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					AssertFilterResults("Only tasks assigned directly to the current user (their capabilities but not assigned directly to any other user) (who is now staff2) should be selected.", mode, "Task 2", "Task 4", "Task 7", "Task 8");
				}

				Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				Filter.Property = staff1.GS_Code;
				AssertFilterResults("Only tasks assigned to the current user (their capabilities but not assigned directly to any other user) should be selected.", mode, "Task 1", "Task 3", "Task 5", "Task 6");

				Filter.Property = staff2.GS_Code;
				AssertFilterResults("Only tasks assigned to the other user (their capabilities but not assigned directly to any other user) should be selected.", mode, "Task 2", "Task 4", "Task 7", "Task 8");

				Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				Filter.SelectedFilters.AddTextFilterStrip("Active Status", FilterStripBusinessObject.StatusActive);
				AssertFilterResults("Both staff match the selected filters so any tasks assigned to either staff (their capabilities but not assigned directly to any other user) should be selected.", mode, "Task 1", "Task 2", "Task 3", "Task 4", "Task 5", "Task 6", "Task 7", "Task 8");

				staff2.GS_IsActive = false;
				Factory.Save();
				AssertFilterResults("The current user is now the only one that matches the selected filters.", mode, "Task 1", "Task 3", "Task 5", "Task 6");
			}
		}

		#endregion

		#region Performance

		public void TestQuery_WhenBufferManagementDisabled_ShouldNotBotherLookingAtProcessHeader()
		{
			SetBufferManagementInRegistry(false);

			var sqlText = Filter.Query.LiteralTextSqlFormatted.Replace(ProcessTasksSchema.P9_FH_ProcessHeader.Name, "[this column is allowed, we're just checking for null and not actually joining or subquerying.]");
			AssertNotContains("If Buffer Management isn't enable then we can skip the unions that look at the workflow's release group, since there won't be any workflows.", ProcessHeaderSchema.Constants.TableName, sqlText);
		}

		public void TestQuery_ShouldIncludeTaskTypeClause_ForPerformance()
		{
			var sqlText = Filter.Query.LiteralTextSqlFormatted;
			var regex = new Regex("P9_Type \\<\\> 'MIL'");
			var matches = regex.Matches(sqlText).Count;

			AssertEquals("We need to include the task type clause for performance reasons. Query: " + sqlText, 5, matches);
		}

		#endregion

		#region XML Serialization

		public void TestSerializeAndDeserialize_ShouldRetainMode()
		{
			var filterBizo = new DummyFilterBizoWithTasksStaffCanDoFilter();
			var filter = filterBizo.AddFilterStrip<TasksStaffCanDoModuleFilter>("TasksStaffCanDo");
			AssertEquals("Default mode", TasksStaffCanDoModuleFilterModes.Codes.EitherStaffOrCapabilityAndUnassignedStaff, filter.Mode);

			filter.Mode = TasksStaffCanDoModuleFilterModes.Codes.RequiredCapabilityRegardlessOfStaffAssignment;

			var layout = filterBizo.SaveLayout("layout");

			filterBizo.ResetModuleFilters();
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), filterBizo.ActiveModuleFilters.Select(x => x.Description));

			filterBizo.LoadLayout(layout);
			AssertContainsExactElementsInAnyOrder("The filter strip we want to test should have been reloaded.", new[] { "TasksStaffCanDo" }, filterBizo.ActiveModuleFilters.Select(x => x.Description));
			var loadedFilter = filterBizo.ActiveModuleFilters.OfType<TasksStaffCanDoModuleFilter>().Single();
			AssertEquals("The selected mode should be loaded with the layout, not the default.", TasksStaffCanDoModuleFilterModes.Codes.RequiredCapabilityRegardlessOfStaffAssignment, loadedFilter.Mode);
		}

		class DummyFilterBizoWithTasksStaffCanDoFilter : DummyFilterBusinessObject
		{
			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				var filters = base.GetModuleFiltersCore();
				filters.AddCustomFilter(new TasksStaffCanDoModuleFilter("TasksStaffCanDo", () => new GlbStaffCollection(Factory)));

				return filters;
			}
		}

		#endregion

		#region Validation

		public void TestModeValidation()
		{
			AssertEquals(TasksStaffCanDoModuleFilterModes.Codes.EitherStaffOrCapabilityAndUnassignedStaff, Filter.Mode);
			AssertNoErrors(Filter.ModeInfo);

			Filter.Mode = "ABC";
			AssertHasError(Filter.ModeInfo, "Enter a valid selection.");

			Filter.Mode = TasksStaffCanDoModuleFilterModes.Codes.OnlyStaff;
			AssertNoErrors(Filter.ModeInfo);

			Filter.Mode = ZString.Empty;
			AssertHasError(Filter.ModeInfo, "Please enter a value.");
		}

		#endregion

		#region Implementation

		void AssertFilterResults(string message, string mode, params string[] expectedTaskDescriptions)
		{
			Filter.InvalidateCachedQuery();
			Filter.Mode = mode;
			var query = Filter.Query;
			var sqlText = query.LiteralTextSqlFormatted;

			var results = Factory.Load<ProcessTask>(query);

			AssertContainsExactElementsInAnyOrder(message + System.Environment.NewLine + $"Filter Mode: {mode}. Query: {sqlText}", expectedTaskDescriptions, results.Select(x => x.P9_Description));
		}

		static ProcessTask CreateTask(IWorkflowProvider job, string description, GlbStaff staff, GlbCapability capability)
		{
			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_Description = description;
			task.P9_GS_NKAssignedStaffMember = staff?.GS_Code ?? string.Empty;
			task.P9_G4_RequiredCapability = capability?.PK ?? ZGuid.Empty;

			return task;
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		public override void TestQueryIsEmptyByDefault()
		{
			AssertEquals("The 'current user' comparison operator is selected by default so it will not be empty unless 'exact' is chosen.", false, Filter.Query.IsEmpty);
		}

		protected override TasksStaffCanDoModuleFilter GetNewModuleFilter()
		{
			return new TasksStaffCanDoModuleFilter("moo", () => new GlbStaffCollection(Factory));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		protected override Dictionary<string, IZType> GetDummyValuesForCacheInvalidationTest(TasksStaffCanDoModuleFilter filter)
		{
			// Because the default isn't the first in the list (exact), and for this test we need to change it from whatever the default is.

			var values = base.GetDummyValuesForCacheInvalidationTest(filter);
			values.Add(nameof(filter.ComparisonOperator), new ZString(ModuleTextFilter.ComparisonConstants.Exact));

			return values;
		}

		protected override void SetUp()
		{
			base.SetUp();

			SetBufferManagementInRegistry(true);
		}

		static void SetBufferManagementInRegistry(bool enable)
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = enable;
		}

		#endregion
	}
}
