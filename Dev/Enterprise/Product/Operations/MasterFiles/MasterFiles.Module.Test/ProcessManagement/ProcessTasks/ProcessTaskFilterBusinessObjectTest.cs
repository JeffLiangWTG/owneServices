using System;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ProcessTaskFilterBusinessObject))]
	public class ProcessTaskFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Current Task Only filter

		public void TestTaskListModule_WhenHasCurrentTaskOnlyFilter_ShouldBeValid()
		{
			AssertTaskListModule_WhenHasCurrentTaskOnlyFilter_ShouldBeValid();
		}

		public void TestTaskListModule_WhenHasCurrentTaskOnlyFilter_QIDisabled_ShouldBeValid()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("BUF");
			AssertTaskListModule_WhenHasCurrentTaskOnlyFilter_ShouldBeValid();
		}

		void AssertTaskListModule_WhenHasCurrentTaskOnlyFilter_ShouldBeValid()
		{
			AssertWhenHasCurrentTaskOnlyFilter_ShouldBeValid(
				new ProcessTaskFilterBusinessObject(),
				addCurrentTaskFilter: (filterBizO) =>
				{
					var currentTaskOnlyFilter = ((ProcessTaskFilterBusinessObject)filterBizO).AddFilterStrip<ModuleFlagsFilter>("Current Task Only");
					currentTaskOnlyFilter.Property0 = true;
				}
			);
		}

		public void TestTaskListModule_WhenHasNestedCurrentTaskOnlyFilter_ShouldBeValid()
		{
			AssertTaskListModule_WhenHasNestedCurrentTaskOnlyFilter_ShouldBeValid();
		}

		public void TestTaskListModule_WhenHasNestedCurrentTaskOnlyFilter_QIDisabled_ShouldBeValid()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("BUF");
			AssertTaskListModule_WhenHasNestedCurrentTaskOnlyFilter_ShouldBeValid();
		}

		void AssertTaskListModule_WhenHasNestedCurrentTaskOnlyFilter_ShouldBeValid()
		{
			AssertWhenHasCurrentTaskOnlyFilter_ShouldBeValid(
				new ProcessTaskFilterBusinessObject(),
				addCurrentTaskFilter: (filterBizO) =>
				{
					var parentJobFilter = ((ProcessTaskFilterBusinessObject)filterBizO).AddFilterStrip<ParentJobModuleFilter>("Parent Job");
					parentJobFilter.SelectedModule = ModuleIDs.Organisation.Name;
					parentJobFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;

					var tasksModuleFilter = parentJobFilter.SelectedFilters.AddFilterStrip<TasksModuleFilter>("Tasks");
					tasksModuleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
					var currentTaskOnlyFilter = tasksModuleFilter.SelectedFilters.AddFilterStrip<ModuleFlagsFilter>("Current Task Only");
					currentTaskOnlyFilter.Property0 = true;
				}
			);
		}

		public void TestTaskListModule_WhenHasUserDefinedCurrentTaskOnlyFilter_ShouldBeValid()
		{
			AssertTaskListModule_WhenHasUserDefinedCurrentTaskOnlyFilter_ShouldBeValid();
		}

		public void TestTaskListModule_WhenHasUserDefinedCurrentTaskOnlyFilter_QIDisabled_ShouldBeValid()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("BUF");
			AssertTaskListModule_WhenHasUserDefinedCurrentTaskOnlyFilter_ShouldBeValid();
		}

		void AssertTaskListModule_WhenHasUserDefinedCurrentTaskOnlyFilter_ShouldBeValid()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			{
				var currentFilterBizO = module.FilterBusinessObject;
				var currentTaskOnlyFilter = currentFilterBizO.AddFilterStrip<ModuleFlagsFilter>("Current Task Only");
				currentTaskOnlyFilter.Property0 = true;
				currentTaskOnlyFilter.IsActive = true;

				FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "User Defined Filter X", isPublished: true, isPublishedGlobal: true, isUserDefinedFilter: true);
			}

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			{
				var currentFilterBizO = module.FilterBusinessObject;

				AssertWhenHasCurrentTaskOnlyFilter_ShouldBeValid(
					currentFilterBizO,
					addCurrentTaskFilter: (filterBizO) =>
					{
						((ProcessTaskFilterBusinessObject)filterBizO).AddFilterStrip<ModuleUserDefinedFilter>("[USR]User Defined Filter X");
					}
				);
			}
		}

		void AssertWhenHasCurrentTaskOnlyFilter_ShouldBeValid(FilterBusinessObject filterBizO, Action<FilterBusinessObject> addCurrentTaskFilter)
		{
			AssertNoErrors("Precondition: filterBizO.HasErrors", filterBizO);

			addCurrentTaskFilter(filterBizO);

			var getCurrentTasks = ObjectFactory.Get<IBMSQLFunctionHelper>().GetCurrentTasks;
			Assert($"FilterBusinessObject should contain {getCurrentTasks}()", filterBizO.Filter.LiteralTextADO.Contains($"{getCurrentTasks}()"));

			filterBizO.RunPreSaveValidation();

			AssertNoErrors("WHEN Module has CurrentTaskOnly filter THEN should valid", filterBizO);
		}

		#endregion

		#region User Defined Filter

		public void TestUserDefinedFilter()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			{
				var filterBizO = module.FilterBusinessObject;
				var descriptionFilter = filterBizO.AddFilterStrip<ModuleTextFilter>("Description");
				descriptionFilter.Property = "user defined description";

				FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "User Defined Filter X", isPublished: true, isPublishedGlobal: true, isUserDefinedFilter: true);
			}

			var dummy = Factory.New<DummyWithRelatedProcessTaskFilters>();
			var filter = dummy.Filter;

			FilterStripsTestHelper.AddFilterStrip<ModuleUserDefinedFilter>(filter, "[USR]User Defined Filter X");

			var query = RelatedModuleFiltersHelper.GetFilterQuery(filter);
			AssertContains("query should contain UserDefinedFilter", "user defined description", query.LiteralTextADO);
		}

		#endregion

		#region Related Items

		public void TestTagDefinition()
		{
			var def = Factory.New<ITagDefinition>();
			def.TGD_Code = "DAN";
			def.TGD_Description = "The Daniel Keogh Tag";

			var mag = Factory.New<ITagMagnitude>();
			mag.TGM_TGD_Tag = def.PK;
			mag.TGM_Code = "D";
			mag.TGM_Description = "D is for Daniel";

			var taggedTask = Factory.NewWithValidTestData<ProcessTask>();
			var notTaggedTask = Factory.NewWithValidTestData<ProcessTask>();

			taggedTask.AddTag(mag);

			Factory.Save();

			ProcessTaskFilterBusinessObject filter = new ProcessTaskFilterBusinessObjectForTest();
			((ModuleGuidFilter)filter["Tag Definition Code"]).Property = def.PK;
			((ModuleGuidFilter)filter["Tag Definition Code"]).IsActive = true;

			ProcessTaskCollection pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);

			AssertCollectionContains(taggedTask, pTasks);
			AssertCollectionNotContains(notTaggedTask, pTasks);
		}

		public void TestTagMagnitude()
		{
			var def = Factory.New<ITagDefinition>();
			def.TGD_Code = "DAN";
			def.TGD_Description = "The Daniel Keogh Tag";

			var mag = Factory.New<ITagMagnitude>();
			mag.TGM_TGD_Tag = def.PK;
			mag.TGM_Code = "D";
			mag.TGM_Description = "D is for Daniel";

			var taggedTask = Factory.NewWithValidTestData<ProcessTask>();
			var notTaggedTask = Factory.NewWithValidTestData<ProcessTask>();

			taggedTask.AddTag(mag);

			Factory.Save();

			ProcessTaskFilterBusinessObject filter = new ProcessTaskFilterBusinessObjectForTest();
			((ModuleGuidFilter)filter["Tag Magnitude"]).Property = mag.PK;
			((ModuleGuidFilter)filter["Tag Magnitude"]).IsActive = true;

			ProcessTaskCollection pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);

			AssertCollectionContains(taggedTask, pTasks);
			AssertCollectionNotContains(notTaggedTask, pTasks);
		}

		public void TestCurrentComponent()
		{
			var system = Factory.New<IBMSystem>();
			system.FS_Name = "Hey";

			var component = Factory.New<IBMComponent>();
			component.FC_FS_System = system.PK;
			component.FC_Name = "Hey";
			var currentTask = Factory.NewWithValidTestData<ProcessTask>();
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var header = BMSTestHelper.CreateWorkflow(jobHeader, "Nope");
			header.FH_FC_CurrentComponent = component.PK;
			currentTask.P9_FH_ProcessHeader = header.PK;
			currentTask.P9_ParentID = header.FH_ParentId;
			currentTask.P9_ParentTableCode = header.FH_ParentTableCode;

			var nonCurrentTask = Factory.NewWithValidTestData<ProcessTask>();
			Factory.Save();

			ProcessTaskFilterBusinessObject filter = new ProcessTaskFilterBusinessObject();
			((ModuleGuidFilter)filter["Current Component"]).Property = component.PK;
			((ModuleGuidFilter)filter["Current Component"]).IsActive = true;

			ProcessTaskCollection pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);

			AssertCollectionContains(currentTask, pTasks);
			AssertCollectionNotContains(nonCurrentTask, pTasks);
		}

		public void TestCompletionStatement()
		{
			var currentTask = Factory.NewWithValidTestData<ProcessTask>();
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var header = BMSTestHelper.CreateWorkflow(jobHeader, "Nope");
			header.FH_CompletionStatement = "Hello";
			currentTask.P9_FH_ProcessHeader = header.PK;
			currentTask.P9_ParentID = header.FH_ParentId;
			currentTask.P9_ParentTableCode = header.FH_ParentTableCode;

			var nonCurrentTask = Factory.NewWithValidTestData<ProcessTask>();
			Factory.Save();

			ProcessTaskFilterBusinessObject filter = new ProcessTaskFilterBusinessObject();
			((ModuleTextFilter)filter["Completion Statement"]).Property = "Hello";
			((ModuleTextFilter)filter["Completion Statement"]).IsActive = true;

			ProcessTaskCollection pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);

			AssertCollectionContains(currentTask, pTasks);
			AssertCollectionNotContains(nonCurrentTask, pTasks);
		}

		public void TestCurrentTaskOnly()
		{
			AssertCurrentTaskOnly();
		}

		public void TestCurrentTaskOnly_WhenQIDisabled()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("BUF");
			AssertCurrentTaskOnly();
		}

		void AssertCurrentTaskOnly()
		{
			var currentTask = Factory.NewWithValidTestData<ProcessTask>();
			currentTask.P9_Sequence = 1;
			currentTask.P9_Status = "ASN";
			currentTask.P9_ParentID = ZGuid.NewZGuid();
			currentTask.P9_ParentTableCode = "JS";
			var nonCurrentTask = Factory.NewWithValidTestData<ProcessTask>();
			nonCurrentTask.P9_Sequence = 2;
			nonCurrentTask.P9_Status = "ASN";
			nonCurrentTask.P9_ParentID = currentTask.P9_ParentID;
			nonCurrentTask.P9_ParentTableCode = "JS";
			Factory.Save();

			var filter = new ProcessTaskFilterBusinessObject();
			((ModuleFlagsFilter)filter["Current Task Only"]).Property0 = true;
			((ModuleFlagsFilter)filter["Current Task Only"]).IsActive = true;

			var pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);

			AssertCollectionContains(currentTask, pTasks);
			AssertCollectionNotContains(nonCurrentTask, pTasks);
		}

		public void TestCurrentTaskOnly_ShouldIncludeStandaloneTasks()
		{
			var standaloneTasks = new[] { "ASN", "OPN", "SUS", "WRK" }.Select(status =>
			{
				var standaloneTask = Factory.NewWithValidTestData<ProcessTask>();
				standaloneTask.P9_Sequence = 1;
				standaloneTask.P9_Status = status;
				standaloneTask.P9_ParentID = ZGuid.Empty;
				standaloneTask.P9_ParentTableCode = "JS";
				standaloneTask.P9_Type = "UDF";
				standaloneTask.P9_FH_ProcessHeader = ZGuid.Empty;
				return standaloneTask;
			}).ToList();

			Factory.Save();

			var processTaskFilter = new ProcessTaskFilterBusinessObject();
			var moduleFlagsFilter = (ModuleFlagsFilter)processTaskFilter["Current Task Only"];
			moduleFlagsFilter.Property0 = true;
			moduleFlagsFilter.IsActive = true;

			var tasks = new ProcessTaskCollection(Factory);
			tasks.Load(processTaskFilter.Filter);

			foreach (var standaloneTask in standaloneTasks)
			{
				AssertEquals("A standalone task should have NO parent.", ZGuid.Empty, standaloneTask.P9_ParentID);
				AssertCollectionContains($"A standalone task with Status {standaloneTask.P9_Status} should be in the task list after 'Current Task Only' filter is applied because it is not dependant on anything else.", standaloneTask, tasks);
			}
		}

		public void TestCurrentTaskOnly_ShouldNotIncludeStandaloneTasks_WithStatusOtherThan_ASN_OPN_SUS_WRK()
		{
			var standaloneTask = Factory.NewWithValidTestData<ProcessTask>();
			standaloneTask.P9_Sequence = 1;
			standaloneTask.P9_Status = "CAN";
			standaloneTask.P9_ParentID = ZGuid.Empty;
			standaloneTask.P9_ParentTableCode = "JS";
			standaloneTask.P9_Type = "UDF";
			standaloneTask.P9_FH_ProcessHeader = ZGuid.Empty;

			Factory.Save();

			var processTaskFilter = new ProcessTaskFilterBusinessObject();
			var moduleFlagsFilter = (ModuleFlagsFilter)processTaskFilter["Current Task Only"];
			moduleFlagsFilter.Property0 = true;
			moduleFlagsFilter.IsActive = true;

			var tasks = new ProcessTaskCollection(Factory);
			tasks.Load(processTaskFilter.Filter);

			AssertEquals("A standalone task should have NO parent.", ZGuid.Empty, standaloneTask.P9_ParentID);
			AssertCollectionNotContains($"A standalone task with Status {standaloneTask.P9_Status} should not be in the task list after 'Current Task Only' filter is applied.", standaloneTask, tasks);
		}

		public void TestCurrentTaskOnly_ShouldNotIncludeStandaloneTasks_WithType_EXC_MIL_TRG()
		{
			var standaloneTasks = new[] { "EXC", "MIL", "TRG" }.Select(type =>
			{
				var standaloneTask = Factory.NewWithValidTestData<ProcessTask>();
				standaloneTask.P9_Sequence = 1;
				standaloneTask.P9_Status = "ASN";
				standaloneTask.P9_ParentID = ZGuid.Empty;
				standaloneTask.P9_ParentTableCode = "JS";
				standaloneTask.P9_Type = type;
				standaloneTask.P9_FH_ProcessHeader = ZGuid.Empty;
				return standaloneTask;
			}).ToList();

			Factory.Save();

			var processTaskFilter = new ProcessTaskFilterBusinessObject();
			var moduleFlagsFilter = (ModuleFlagsFilter)processTaskFilter["Current Task Only"];
			moduleFlagsFilter.Property0 = true;
			moduleFlagsFilter.IsActive = true;

			var tasks = new ProcessTaskCollection(Factory);
			tasks.Load(processTaskFilter.Filter);

			foreach (var standaloneTask in standaloneTasks)
			{
				AssertEquals("A standalone task should have NO parent.", ZGuid.Empty, standaloneTask.P9_ParentID);
				AssertCollectionNotContains($"A standalone task with Type {standaloneTask.P9_Type} should not be in the task list after 'Current Task Only' filter is applied.", standaloneTask, tasks);
			}
		}

		public void TestCurrentTaskOnly_ShouldNotIncludeStandaloneTasks_WithParentTableCode_P0()
		{
			var standaloneTask = Factory.NewWithValidTestData<ProcessTask>();
			standaloneTask.P9_Sequence = 1;
			standaloneTask.P9_Status = "ASN";
			standaloneTask.P9_ParentID = ZGuid.Empty;
			standaloneTask.P9_ParentTableCode = "P0";
			standaloneTask.P9_Type = "UDF";
			standaloneTask.P9_FH_ProcessHeader = ZGuid.Empty;

			Factory.Save();

			var processTaskFilter = new ProcessTaskFilterBusinessObject();
			var moduleFlagsFilter = (ModuleFlagsFilter)processTaskFilter["Current Task Only"];
			moduleFlagsFilter.Property0 = true;
			moduleFlagsFilter.IsActive = true;

			var tasks = new ProcessTaskCollection(Factory);
			tasks.Load(processTaskFilter.Filter);

			AssertEquals("A standalone task should have NO parent.", ZGuid.Empty, standaloneTask.P9_ParentID);
			AssertCollectionNotContains($"A standalone task with ParentTableCode {standaloneTask.P9_ParentTableCode} should not be in the task list after 'Current Task Only' filter is applied.", standaloneTask, tasks);
		}

		public void TestCurrentTaskOnly_CheckQueryTablesForBMDisabled()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.DisableBMSInRegistry();
			AssertCurrentTaskOnlyQueryPlan("The query should NOT contain a reference to the ProcessTaskIterationLink table when Buffer Management is disabled.", expectProcessHeaderLink: false);
		}

		public void TestCurrentTaskOnly_CheckQueryTablesForBMEnabled()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			AssertCurrentTaskOnlyQueryPlan("The query should contain a reference to the ProcessTaskIterationLink table when Buffer Management is enabled.", expectProcessHeaderLink: true);
		}

		public void TestCurrentTaskOnly_CheckQueryTablesForQIDisabled()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("BUF");
			AssertCurrentTaskOnlyQueryPlan("The query should NOT contain a reference to the ProcessTaskIterationLink table when Quality Iteration is disabled.", expectProcessHeaderLink: false);
		}

		public void TestCurrentTaskOnly_CheckQueryTablesForQIEnabled()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			AssertCurrentTaskOnlyQueryPlan("The query should contain a reference to the ProcessTaskIterationLink table when Quality Iterations is enabled.", expectProcessHeaderLink: true);
		}

		static void AssertCurrentTaskOnlyQueryPlan(string failureMessage, bool expectProcessHeaderLink)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			var filterBizo = new ProcessTaskFilterBusinessObject();
			var strip = filterBizo.FilterStrips.AddNew("Current Task Only");
			var filter = (ModuleFlagsFilter)strip.CurrentModuleFilter;
			filter.Property0 = true;

			var sql = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM dbo.ProcessTasks WHERE ({0})", filterBizo.Filter.LiteralTextSqlFormatted);
			query.AddFilterAndZSQLParameterCollection(sql, null);
			var sqlQueryPlan = Db.Connection.GetQueryPlanXml_ForTest(query.LiteralTextSqlFormatted);

			if (expectProcessHeaderLink)
			{
				AssertContains(failureMessage, "ProcessTaskIterationLink", sqlQueryPlan);
			}
			else
			{
				AssertNotContains(failureMessage, "ProcessTaskIterationLink", sqlQueryPlan);
			}
		}

		public void TestStaffFilter()
		{
			ProcessTask staff1 = Factory.NewWithValidTestData<ProcessTask>();
			ProcessTask staff2 = Factory.NewWithValidTestData<ProcessTask>();

			staff1.P9_GS_NKAssignedStaffMember = "AUS";
			staff2.P9_GS_NKAssignedStaffMember = "UAI";

			Factory.Save();

			ProcessTaskFilterBusinessObject filter = new ProcessTaskFilterBusinessObject();
			((ModuleNkFilter)filter["Staff"]).Property = "AUS";
			((ModuleNkFilter)filter["Staff"]).IsActive = true;

			ProcessTaskCollection pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);

			AssertCollectionContains(staff1, pTasks);
			AssertCollectionNotContains(staff2, pTasks);
		}

		public void TestTasksStaffCanDoFilter_Category()
		{
			var filterBizo = new ProcessTaskFilterBusinessObject();
			var filter = filterBizo.AddFilterStrip<TasksStaffCanDoModuleFilter>("TasksStaffCanDo");
			AssertEquals(FilterCategories.Organisations, filter.Category);
		}

		public void TestGroupFilter()
		{
			GlbGroup glbGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			GlbGroup glbGroup2 = Factory.NewWithValidTestData<GlbGroup>();

			ProcessTask group0 = Factory.NewWithValidTestData<ProcessTask>();
			group0.P9_GG_AssignedGroup = glbGroup1.PK;

			ProcessTask group1 = Factory.NewWithValidTestData<ProcessTask>();
			group1.P9_GG_AssignedGroup = glbGroup2.PK;

			ProcessTask group2 = Factory.NewWithValidTestData<ProcessTask>();
			group2.P9_GG_AssignedGroup = glbGroup2.PK;

			Factory.Save();

			ProcessTaskFilterBusinessObject glbGroup1Filter = new ProcessTaskFilterBusinessObject();
			((ModuleGuidFilter)glbGroup1Filter["Group"]).Property = glbGroup1.PK;
			((ModuleGuidFilter)glbGroup1Filter["Group"]).IsActive = true;

			ProcessTaskCollection pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(glbGroup1Filter.Filter);

			AssertCollectionContains(group0, pTasks);
			AssertCollectionNotContains(group1, pTasks);
			AssertCollectionNotContains(group2, pTasks);

			ProcessTaskFilterBusinessObject glbGroup2Filter = new ProcessTaskFilterBusinessObject();
			((ModuleGuidFilter)glbGroup2Filter["Group"]).Property = glbGroup2.PK;
			((ModuleGuidFilter)glbGroup2Filter["Group"]).IsActive = true;

			pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(glbGroup2Filter.Filter);

			AssertCollectionContains(group1, pTasks);
			AssertCollectionContains(group2, pTasks);
			AssertCollectionNotContains(group0, pTasks);

			ProcessTaskFilterBusinessObject emptyGlbGroupFilter = new ProcessTaskFilterBusinessObject();
			((ModuleGuidFilter)emptyGlbGroupFilter["Group"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)emptyGlbGroupFilter["Group"]).IsActive = false;

			pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(emptyGlbGroupFilter.Filter);

			AssertCollectionContains(group0, pTasks);
			AssertCollectionContains(group1, pTasks);
			AssertCollectionContains(group2, pTasks);
		}

		public void TestParentJobFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "MYORGSYD";
			org1.OH_FullName = "Hitech Software";
			var task1 = org1.WorkflowItems.Tasks.AddNew();
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "USORGSYD";
			org2.OH_FullName = "Lowtech Software";
			var task2 = org2.WorkflowItems.Tasks.AddNew();
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var task3 = enquiry.WorkflowItems.Tasks.AddNew();
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			var filterBizo = new ProcessTaskFilterBusinessObject();
			var filter = (ModuleGuidModuleSpecifiedFilter)filterBizo["Parent Job"];
			filter.IsActive = true;
			filter.SelectedModule = "OrgHeader";
			filter.Property = org1.PK;

			var result = Factory.Load<ProcessTask>(filterBizo.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { task1 }, result);

			filter.SelectedModule = ModuleIDs.SalesEnquiry.Name;
			filter.Property = enquiry.PK;

			result = Factory.Load<ProcessTask>(filterBizo.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { task3 }, result);
		}

		public void TestWorkflowFilter()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;
			Factory.Save();

			var filter = new ProcessTaskFilterBusinessObject();
			AssertNull(filter["Workflow"]);

			BMSTestHelper.EnableBMSInRegistry();

			filter = new ProcessTaskFilterBusinessObject();
			var workflowFilter = (ModuleGuidFilter)filter["Workflow"];
			AssertNotNull(workflowFilter);

			// job 1 - one WF and 2 tasks
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "WF 1");

			var task1 = BMSTestHelper.CreateTask(workflow);
			var task2 = BMSTestHelper.CreateTask(workflow);

			//job 2 - two wfs and one task for each wf
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1_jobHeader2 = BMSTestHelper.CreateWorkflow(jobHeader2, "WF jobHeader2");
			var task_workflow1_jobHeader2 = BMSTestHelper.CreateTask(workflow1_jobHeader2);

			var workflow2_jobHeader2 = BMSTestHelper.CreateWorkflow(jobHeader2, "WF 2 jobHeader2");
			var task_workflow2_jobHeader2 = BMSTestHelper.CreateTask(workflow2_jobHeader2);

			Factory.Save();

			workflowFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			workflowFilter.Property = workflow.PK;
			workflowFilter.IsActive = true;

			var tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { task1, task2 }, tasks);

			workflowFilter.Property = workflow1_jobHeader2.PK;
			tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { task_workflow1_jobHeader2 }, tasks);

			workflowFilter.Property = workflow2_jobHeader2.PK;
			tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { task_workflow2_jobHeader2 }, tasks);
		}

		#endregion

		#region Text

		public void TestTaskTypeFilter()
		{
			CodeDescriptionPair code1 = new CodeDescriptionPair("XXX", "lololol");
			CodeDescriptionPair code2 = new CodeDescriptionPair("YYY", "sighwork");

			CategorisedWorkflowTaskTypesCollection taskTypesCollection = new CategorisedWorkflowTaskTypesCollection();
			CategorisedWorkflowTaskTypes workflowType1 = taskTypesCollection.AddNew();
			workflowType1.Code = "PRJ";
			WorkflowTaskType taskType1 = workflowType1.TaskTypes.AddNew();
			taskType1.Code = code1.Code;
			taskType1.Description = (NoResString)code1.Description;

			CategorisedWorkflowTaskTypes workflowType2 = taskTypesCollection.AddNew();
			workflowType2.Code = JobInvoicingConsumerTypes.WorkItem.Code;
			WorkflowTaskType taskType2 = workflowType2.TaskTypes.AddNew();
			taskType2.Code = code2.Code;
			taskType2.Description = (NoResString)code2.Description;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taskTypesCollection);

			var processTask1 = Factory.NewWithValidTestData<ProcessTask>();
			processTask1.P9_Type = code2.Code;

			var processTask2 = Factory.NewWithValidTestData<ProcessTask>();
			processTask2.P9_Type = "INV";

			Factory.Save();

			ProcessTaskFilterBusinessObject filter = new ProcessTaskFilterBusinessObject();
			ModuleTextFilter taskTypeFilter = (ModuleTextFilter)filter["Task Type"];
			ModuleTextFilter workflowTypeFilter = (ModuleTextFilter)filter["Workflow Type"];
			ProcessTaskCollection taskCollection = new ProcessTaskCollection(Factory);
			taskTypeFilter.IsActive = true;
			workflowTypeFilter.IsActive = false;

			AssertEquals("should contain task types for all workflow types", 2, taskTypeFilter.List.Count);

			taskTypeFilter.Property = code2.Code;
			taskCollection.Load(filter.Filter);
			AssertEquals(1, taskCollection.Count);
			AssertCollectionContains("should contain task of type \"YYY\"", processTask1, taskCollection);

			workflowTypeFilter.IsActive = true;
			workflowTypeFilter.Property = "PRJ";
			taskCollection.Load(filter.Filter);
			AssertEquals(0, taskCollection.Count);

			AssertEquals("should only contain task types for PRJ workflow type", 1, taskTypeFilter.List.Count);
		}

		public void TestWorkflowTypeFilter_ShouldProduceDistinctQueries()
		{
			WorkflowTypeFilterTest.EnsureAllWorkflowTypesProduceDistinctQueries(new ProcessTaskFilterBusinessObject());
		}

		public void TestWorkflowTypeFilter_ProductivityWiseModeEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			var filterBizo = new ProcessTaskFilterBusinessObject();
			var workflowTypeFilter = (ModuleTextFilter)filterBizo["Workflow Type"];
			workflowTypeFilter.IsActive = true;

			AssertNotNull(workflowTypeFilter.List.GetType().GetInterface(nameof(IWorkflowDescriptorListWithStandaloneTaskType)));
			AssertContainsExactElementsInAnyOrder(ProductivityWiseWorkflowTypesList, ((CodeDescriptionPairList)(workflowTypeFilter.List)).GetAllCodes());
		}

		protected virtual string[] ProductivityWiseWorkflowTypesList => new[]
		{
			WorkflowDescriptors.AccPayableOrderHeaderCode,
			WorkflowDescriptors.APInvoiceCode,
			WorkflowDescriptors.ARInvoiceCode,
			WorkflowDescriptors.CampaignWorkflowDescriptorCode,
			WorkflowDescriptors.CollectionBatchCode,
			WorkflowDescriptors.CollectionOrderCode,
			WorkflowDescriptors.CommunicationWorkflowDescriptorCode,
			WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode,
			WorkflowDescriptors.GlbAccreditationAttemptWorkflowDescriptorCode,
			WorkflowDescriptors.GlbGroupWorkflowDescriptorCode,
			WorkflowDescriptors.GlbStaffChangeRequestWorkflowDescriptorCode,
			WorkflowDescriptors.GlbStaffDescriptorCode,
			WorkflowDescriptors.GlbStaffHolidayDescriptorCode,
			WorkflowDescriptors.HRCampaignWorkflowDescriptorCode,
			WorkflowDescriptors.HRHiringRequestDescriptorCode,
			WorkflowDescriptors.HRJobApplicationWorkflowDescriptorCode,
			WorkflowDescriptors.HROnBoardingWorkflowDescriptorCode,
			WorkflowDescriptors.HRRecruitmentJobCampaignWorkflowDescriptorCode,
			WorkflowDescriptors.OpportunityWorkflowDescriptorCode,
			WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode,
			WorkflowDescriptors.ProjectWorkflowDescriptorCode,
			WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode,
			WorkflowDescriptors.WorkItemWorkflowDescriptorCode,
			WorkflowDescriptors.StandAloneTaskWorkflowDescriptor,
		};

		public void TestTaskIDFilter()
		{
			ProcessTask description1 = Factory.NewWithValidTestData<ProcessTask>();
			ProcessTask description2 = Factory.NewWithValidTestData<ProcessTask>();
			description2.P9_Description = "SomeDescription";

			Factory.Save();
			Assert("unique values are expected from dbo.ProcessTasks workflow (right after 'Factory.Save()')", description1 != description2);

			ProcessTaskFilterBusinessObject filter = new ProcessTaskFilterBusinessObject();
			((ModuleTextFilter)filter["Task ID"]).Property = description1.P9_TaskID;
			((ModuleTextFilter)filter["Task ID"]).IsActive = true;

			((ModuleTextFilter)filter["Description"]).Property = "SomeDescription";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			ProcessTaskCollection pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);

			AssertCollectionContains(description1, pTasks);
			AssertCollectionNotContains(description2, pTasks);
		}

		public void TestDescriptionFilter()
		{
			ProcessTask description1 = Factory.NewWithValidTestData<ProcessTask>();
			ProcessTask description2 = Factory.NewWithValidTestData<ProcessTask>();
			description1.P9_Description = "Indescriptive";
			description2.P9_Description = "SomeDescription";

			Factory.Save();

			ProcessTaskFilterBusinessObject filter = new ProcessTaskFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "Indescriptive";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			ProcessTaskCollection pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);

			AssertCollectionContains(description1, pTasks);
			AssertCollectionNotContains(description2, pTasks);
		}

		public void TestTypeFilter()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			ProcessTask dummyTask = dummy.WorkflowItems.Tasks.AddNew();
			dummy.WorkflowItems.Milestones.AddNew();
			DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(true);
			DummyWithWorkflow anotherDummy = Factory.New<DummyWithWorkflow>();
			ProcessTask anotherDummyTask = anotherDummy.WorkflowItems.Tasks.AddNew();
			anotherDummy.WorkflowItems.Triggers.AddNew();
			OrgOpportunity opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			ProcessTask opportunityTask = opportunity.WorkflowItems.Tasks.AddNew();
			ProcessTask standaloneTask = Factory.New<ProcessTask>();
			Factory.Save();

			ProcessTaskFilterBusinessObject filter = new ProcessTaskFilterBusinessObject();
			ModuleTextFilter typeFilter = (ModuleTextFilter)filter["Workflow Type"];

			typeFilter.IsActive = true;
			typeFilter.Property = "DUM";
			ProcessTaskCollection collection = new ProcessTaskCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(dummyTask, collection);
			AssertCollectionContains(anotherDummyTask, collection);

			typeFilter.Property = OpportunityWorkflowDescriptor.WorkflowTypeCode;
			collection.Load(filter.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opportunityTask, collection);

			typeFilter.Property = WorkflowDescriptors.StandAloneTaskWorkflowDescriptor;
			collection.Load(filter.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(standaloneTask, collection);

			typeFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			collection.Load(filter.Filter);

			AssertContainsExactElementsInAnyOrder("Workflow Type not equal STA means everything except standalone tasks should be returned",
				new[] { dummyTask, anotherDummyTask, opportunityTask }, collection);
		}

		public void TestTemplateFilter()
		{
			TestCaseHelper.ClearTable(ProcessTasksSchema.Constants.TableName);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			var nonTemplateTask = Factory.New<ProcessTask>();

			Factory.Save();

			var bizo = new ProcessTaskFilterBusinessObject();
			var templateFilter = (ModuleTextFilter)bizo["Template"];
			var results = Factory.Load<ProcessTask>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("By default template tasks should be excluded", new[] { nonTemplateTask }, results);

			templateFilter.IsActive = true;
			AssertContainsExactElementsInAnyOrder("Just enabling the filter should not change the results", new[] { nonTemplateTask }, results);

			templateFilter.Property = TemplateFilterOptions.Codes.All;
			results = Factory.Load<ProcessTask>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("All tasks should now be included", new[] { templateTask, nonTemplateTask }, results);

			templateFilter.Property = TemplateFilterOptions.Codes.Template;
			results = Factory.Load<ProcessTask>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("Only tasks on a template should now be included", new[] { templateTask }, results);

			templateFilter.Property = TemplateFilterOptions.Codes.NonTemplate;
			results = Factory.Load<ProcessTask>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("Only tasks not on a template should now be included", new[] { nonTemplateTask }, results);
		}

		public void TestTemplateFilter_WhenFilterWouldBeInertByDefault_ButUserEnablesTheFilterStrip_QueryShouldBeActive()
		{
			TestCaseHelper.ClearTable(ProcessTasksSchema.Constants.TableName);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			var nonTemplateTask = Factory.New<ProcessTask>();

			Factory.Save();

			var bizo = new ProcessTaskFilterBusinessObject();
			var templateFilter = (ModuleTextFilter)bizo["Template"];

			AssertTasksFound("By default template tasks should be excluded", nonTemplateTask);

			bizo.IsInFilterRuleMode = true;
			AssertTasksFound("In filter rule mode, we don't care about excluding template tasks because they would get excluded by parent query predicates", templateTask, nonTemplateTask);

			bizo.IsInFilterRuleMode = false;
			AssertTasksFound("Making sure the template task is excluded at this point in the test", nonTemplateTask);

			bizo.ShouldAddNonTemplateFilter = false;
			AssertTasksFound("Un-setting this other flag turns off the template task exclusion", templateTask, nonTemplateTask);

			templateFilter.IsActive = true;
			templateFilter.Property = TemplateFilterOptions.Codes.Template;
			AssertTasksFound("The user enabling the filter strip causes the filter strip to swing into action", templateTask);

			templateFilter.Property = TemplateFilterOptions.Codes.NonTemplate;
			AssertTasksFound("The user enabling the filter strip causes the filter strip to swing into action", nonTemplateTask);

			templateFilter.Property = TemplateFilterOptions.Codes.All;
			AssertTasksFound("The user enabling the filter strip causes the filter strip to swing into action", templateTask, nonTemplateTask);

			bizo.QueryObjectType = typeof(TemplateProcessTask);
			AssertEquals("Don't exclude templates when filtering in a template", false, bizo.ShouldAddNonTemplateFilter);

			void AssertTasksFound(string message, params ProcessTask[] expectedTasks)
			{
				var results = Factory.Load<ProcessTask>(bizo.Filter);
				AssertContainsExactElementsInAnyOrder(message, expectedTasks, results);
			}
		}

		#endregion

		#region Status and Flags

		public void TestOverdueOnlyFilter_OnlyChecks24HoursIntoTheFuture()
		{
			ProcessTask task1 = Factory.NewWithValidTestData<ProcessTask>();
			task1.TaskProperties.ScheduledDate = new ZDateTimeOffset(ZDateTime.Now.AddHours(-26));
			task1.P9_EstDuration = TimeSpan.FromHours(96);
			Factory.Save();

			ProcessTaskFilterBusinessObject filter = new ProcessTaskFilterBusinessObject();
			ModuleFlagsFilter flagsFilter = ((ModuleFlagsFilter)filter["Overdue"]);
			flagsFilter.IsActive = true;
			AssertEquals("Default value", false, flagsFilter.Property0);

			ProcessTaskCollection pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);
			AssertCollectionContains(task1, pTasks);

			flagsFilter.Property0 = true;
			flagsFilter.IsActive = true;

			pTasks.Load(filter.Filter);
			// this is advantageous since we can put a pre-filter to limit the number of rows such that the query goes faster.
			AssertCollectionContains("Even though the estimated duration is 96 hours, this filter only counts 24 hours of estimate for some reason", task1, pTasks);
		}

		public void TestOverdueOnlyFilter()
		{
			ProcessTask task1 = Factory.NewWithValidTestData<ProcessTask>();
			task1.TaskProperties.ScheduledDate = new ZDateTimeOffset(ZDateTime.Now.AddHours(-4));
			task1.P9_EstDuration = new ZDateTime(2006, 1, 1, 5, 0, 0);

			ProcessTask task2 = Factory.NewWithValidTestData<ProcessTask>();
			task2.TaskProperties.ScheduledDate = new ZDateTimeOffset(ZDateTime.Now.AddHours(-4));
			task2.P9_EstDuration = new ZDateTime(2006, 1, 1, 3, 0, 0);

			ProcessTask task3 = Factory.NewWithValidTestData<ProcessTask>();
			task3.TaskProperties.ScheduledDate = new ZDateTimeOffset(ZDateTime.Today.AddMonths(1));

			ProcessTask task4 = Factory.NewWithValidTestData<ProcessTask>();
			task4.TaskProperties.ScheduledDate = new ZDateTimeOffset(ZDateTime.Today.AddDays(-1));
			task4.TaskProperties.ActualDate = ZDateTimeOffset.Today;

			ProcessTask task5 = Factory.New<ProcessTask>();
			task5.TaskProperties.ScheduledDate = new ZDateTimeOffset(ZDateTime.Now.AddMonths(-1));

			Factory.Save();

			ProcessTaskFilterBusinessObject filter = new ProcessTaskFilterBusinessObject();
			ModuleFlagsFilter flagsFilter = ((ModuleFlagsFilter)filter["Overdue"]);
			flagsFilter.IsActive = true;
			AssertEquals("Default value", false, flagsFilter.Property0);

			ProcessTaskCollection pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);
			AssertCollectionContains(task1, pTasks);
			AssertCollectionContains(task2, pTasks);
			AssertCollectionContains(task3, pTasks);
			AssertCollectionContains(task4, pTasks);
			AssertCollectionContains(task5, pTasks);

			flagsFilter.Property0 = true;
			flagsFilter.IsActive = true;

			pTasks.Load(filter.Filter);
			AssertCollectionNotContains(task1, pTasks);
			AssertCollectionContains(task2, pTasks);
			AssertCollectionNotContains(task3, pTasks);
			AssertCollectionNotContains(task4, pTasks);
			AssertCollectionContains(task5, pTasks);
		}

		public void TestStatusFilter()
		{
			ProcessTask status1 = Factory.NewWithValidTestData<ProcessTask>();
			ProcessTask status2 = Factory.NewWithValidTestData<ProcessTask>();
			status1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			status2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			Factory.Save();

			ProcessTaskFilterBusinessObject filter = new ProcessTaskFilterBusinessObject();
			((ModuleTextFilter)filter["Status"]).Property = ProcessTaskStatusCodeList.Codes.Cancelled;
			((ModuleTextFilter)filter["Status"]).IsActive = true;

			ProcessTaskCollection pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);

			AssertCollectionContains(status1, pTasks);
			AssertCollectionNotContains(status2, pTasks);
		}

		public void TestStatusNotCompleteFilter()
		{
			ProcessTask closedTask = Factory.NewWithValidTestData<ProcessTask>();
			ProcessTask cancelledTask = Factory.NewWithValidTestData<ProcessTask>();
			ProcessTask openTask1 = Factory.NewWithValidTestData<ProcessTask>();
			ProcessTask openTask2 = Factory.NewWithValidTestData<ProcessTask>();
			closedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			cancelledTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			openTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			openTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			Factory.Save();

			ProcessTaskFilterBusinessObject filter = new ProcessTaskFilterBusinessObject();
			((ModuleTextFilter)filter["Status"]).Property = "NCM";
			((ModuleTextFilter)filter["Status"]).IsActive = true;

			ProcessTaskCollection pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);

			AssertCollectionNotContains("Do not include closed tasks", closedTask, pTasks);
			AssertCollectionNotContains("Do not include cancelled tasks", cancelledTask, pTasks);
			AssertCollectionContains("Include incomplete tasks only", openTask1, pTasks);
			AssertCollectionContains("Include incomplete tasks only", openTask2, pTasks);
		}

		public void TestStatusAssignedAndSuspendedFilter()
		{
			ProcessTask assignedTask = Factory.New<ProcessTask>();
			ProcessTask suspendedTask = Factory.New<ProcessTask>();
			ProcessTask openTask1 = Factory.New<ProcessTask>();
			ProcessTask openTask2 = Factory.New<ProcessTask>();
			assignedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			suspendedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			openTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			openTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			Factory.Save();

			ProcessTaskFilterBusinessObject filter = new ProcessTaskFilterBusinessObject();
			((ModuleTextFilter)filter["Status"]).Property = "A+S";
			((ModuleTextFilter)filter["Status"]).IsActive = true;

			ProcessTaskCollection pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);

			AssertCollectionContains("Include assigned tasks", assignedTask, pTasks);
			AssertCollectionContains("Include suspended tasks", suspendedTask, pTasks);
			AssertCollectionNotContains("Include Assigned + Suspended tasks only", openTask1, pTasks);
			AssertCollectionNotContains("Include Assigned + Suspended tasks only", openTask2, pTasks);
		}

		public void TestStatusWorkingAndSuspendedFilter()
		{
			ProcessTask workingTask = Factory.New<ProcessTask>();
			ProcessTask suspendedTask = Factory.New<ProcessTask>();
			ProcessTask openTask1 = Factory.New<ProcessTask>();
			ProcessTask openTask2 = Factory.New<ProcessTask>();
			workingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			suspendedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			openTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			openTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			Factory.Save();

			ProcessTaskFilterBusinessObject filter = new ProcessTaskFilterBusinessObject();
			((ModuleTextFilter)filter["Status"]).Property = "W+S";
			((ModuleTextFilter)filter["Status"]).IsActive = true;

			ProcessTaskCollection pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);

			AssertCollectionContains("Include assigned tasks", workingTask, pTasks);
			AssertCollectionContains("Include suspended tasks", suspendedTask, pTasks);
			AssertCollectionNotContains("Include Assigned + Suspended tasks only", openTask1, pTasks);
			AssertCollectionNotContains("Include Assigned + Suspended tasks only", openTask2, pTasks);
		}

		#endregion

		#region Dates
		public void setDateFilterProperties(string filterName, ZDateTime expectedInRangeDate,
			ProcessTaskFilterBusinessObject filter)
		{
			((ModuleDateFilter)filter[filterName]).PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			((ModuleDateFilter)filter[filterName]).Property1 = expectedInRangeDate.AddMinutes(-30);
			((ModuleDateFilter)filter[filterName]).Property2 = expectedInRangeDate.AddMinutes(30);
			((ModuleDateFilter)filter[filterName]).IsActive = true;
		}

		void TestTaskDateColumnIsWithinFilterRangeAndAssert(Action<ProcessTask, ZDateTime> setDateTimeValue,
			string filterName, TimeSpan offsetLocalTimeIfColumnIsLocal = default)
		{
			ProcessTask taskWithInRangeDate = Factory.NewWithValidTestData<ProcessTask>();
			ProcessTask taskWithEarlierOutOfRangeDate = Factory.NewWithValidTestData<ProcessTask>();
			ProcessTask taskWithLaterOutOfRangeDate = Factory.NewWithValidTestData<ProcessTask>();
			var inRangeDate = new ZDateTime(2023, 1, 1);
			setDateTimeValue(taskWithInRangeDate, inRangeDate);
			setDateTimeValue(taskWithEarlierOutOfRangeDate, inRangeDate.AddHours(-1));
			setDateTimeValue(taskWithLaterOutOfRangeDate, inRangeDate.AddHours(1));
			Factory.Save();

			var expectedInRangeDate = inRangeDate.AddHours(offsetLocalTimeIfColumnIsLocal.TotalHours);
			ProcessTaskFilterBusinessObject filter = new ProcessTaskFilterBusinessObject();
			setDateFilterProperties(filterName, expectedInRangeDate, filter);
			ProcessTaskCollection pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);

			AssertCollectionContains(taskWithInRangeDate, pTasks);
			AssertCollectionNotContains(taskWithEarlierOutOfRangeDate, pTasks);
			AssertCollectionNotContains(taskWithLaterOutOfRangeDate, pTasks);
		}

		public void TestScheduledDateFilter()
		{
			TestTaskDateColumnIsWithinFilterRangeAndAssert((processTask, date) =>
				processTask.P9_ScheduledDateForBinding = new ZDateTimeOffset(date), "Scheduled Start"
			);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestScheduledDateLocalFilter()
		{
			TestTaskDateColumnIsWithinFilterRangeAndAssert((processTask, date) =>
				processTask.P9_ScheduledDateUtc = date, "Scheduled Start Time (Local)", new TimeSpan(11, 0, 0)
			);
		}
		public void TestScheduledDateUtcFilter()
		{
			TestTaskDateColumnIsWithinFilterRangeAndAssert((processTask, date) =>
				processTask.P9_ScheduledDateUtc = date, "Scheduled Start Time (UTC)"
			);
		}

		public void TestActualDateFilter()
		{
			TestTaskDateColumnIsWithinFilterRangeAndAssert((processTask, date) =>
				processTask.P9_ActualDateForBinding = new ZDateTimeOffset(date), "Actual Start"
			);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestActualDateLocalFilter()
		{
			TestTaskDateColumnIsWithinFilterRangeAndAssert((processTask, date) =>
				processTask.P9_ActualDateUtc = date, "Actual Start Time (Local)", new TimeSpan(11, 0, 0)
			);
		}

		public void TestActualDateUtcFilter()
		{
			TestTaskDateColumnIsWithinFilterRangeAndAssert((processTask, date) =>
				processTask.P9_ActualDateUtc = date, "Actual Start Time (UTC)"
			);
		}

		[TestDate(2021, 7, 3, 15, 46, 0)]
		[TestUtcOffset(11, 0, 0)]
		public void TestActualDateFilter_TimeZone()
		{
			var baseDate = ZDateTime.Now;
			var job = Factory.New<DummyWithWorkflow>();
			var task = job.WorkflowItems.Tasks.AddNew();
			AssertEquals(ZDateTimeOffset.Empty, task.P9_ActualDateForBinding);
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			AssertNotEquals(ZDateTimeOffset.Empty, task.P9_ActualDateForBinding);

			ProcessTaskFilterBusinessObject filter = new ProcessTaskFilterBusinessObject();
			setDateFilterProperties("Actual Start", baseDate, filter);
			AssertCollectionContains(task.PK, Factory.Load<ProcessTask>(filter.Filter).Select(t => t.PK));

			TestUtcOffsetAttribute.Time = TimeSpan.FromHours(5);
			filter = new ProcessTaskFilterBusinessObject();
			setDateFilterProperties("Actual Start", baseDate, filter);
			AssertCollectionContains(task.PK, Factory.Load<ProcessTask>(filter.Filter).Select(t => t.PK));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestCompletedTimeLocalFilter()
		{
			TestTaskDateColumnIsWithinFilterRangeAndAssert((processTask, date) =>
				processTask.P9_CompletedTimeUtc = date, "Completed (Local)", new TimeSpan(11, 0, 0)
			);
		}

		public void TestCompletedTimeUtcFilter()
		{
			TestTaskDateColumnIsWithinFilterRangeAndAssert((processTask, date) =>
				processTask.P9_CompletedTimeUtc = date, "Completed (UTC)"
			);
		}

		public void TestEarliestStartDateFilter()
		{
			var outOfRange = new ZDateTime(2005, 1, 1);
			var rangeStart = outOfRange.AddDays(1);
			var rangeEnd = outOfRange.AddDays(2);

			var outOfRangeTask = GetTaskWithDoNotStartSetTo(outOfRange);
			var inRangeTask = GetTaskWithDoNotStartSetTo(rangeStart);

			Factory.Save();

			var filter = new ProcessTaskFilterBusinessObject();
			var dateFilter = (ModuleDateFilter)filter["Earliest Start Date"];

			/* Look between a given date range */
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = rangeStart;
			dateFilter.Property2 = rangeEnd;
			dateFilter.IsActive = true;

			var pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);

			AssertCollectionContains("Task within date range should be in collection", inRangeTask, pTasks);
			AssertCollectionNotContains("Task outside date range shouldn't be in collection", outOfRangeTask, pTasks);
		}

		ProcessTask GetTaskWithDoNotStartSetTo(ZDateTime date)
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var header = BMSTestHelper.CreateWorkflow(jobHeader, "Nope");
			header.DoNotStartBeforeDateLocal = date;

			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_ParentID = header.FH_ParentId;
			task.P9_ParentTableCode = header.FH_ParentTableCode;

			return task;
		}

		public void TestAgreedDeliveryDateFilter()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			jobHeader.FH_CompletionStatement = "Yolo?";

			var processHeader1 = BMSTestHelper.CreateWorkflow(jobHeader, "Nope");
			processHeader1.FH_CompletionStatement = "Swag1.";
			var processHeader2 = BMSTestHelper.CreateWorkflow(jobHeader, "Nope");
			processHeader2.FH_CompletionStatement = "Swag2.";

			processHeader1.AgreedDeliveryDateLocal = new ZDateTime(2005, 1, 1);
			processHeader2.AgreedDeliveryDateLocal = new ZDateTime(2005, 1, 2);

			var task1 = BMSTestHelper.CreateTask(processHeader1);
			var task2 = BMSTestHelper.CreateTask(processHeader2);

			Factory.Save();

			var filter = new ProcessTaskFilterBusinessObject();
			((ModuleDateFilter)filter["Agreed Delivery Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["Agreed Delivery Date"]).Property1 = new ZDateTime(2005, 1, 2);
			((ModuleDateFilter)filter["Agreed Delivery Date"]).Property2 = new ZDateTime(2005, 1, 3);
			((ModuleDateFilter)filter["Agreed Delivery Date"]).IsActive = true;

			var pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);

			AssertCollectionContains(task2, pTasks);
			AssertCollectionNotContains(task1, pTasks);
		}

		public void TestLastTransferDateFilter()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			jobHeader.FH_CompletionStatement = "Yolo?";

			var processHeader1 = BMSTestHelper.CreateWorkflow(jobHeader, "Nope");
			processHeader1.FH_CompletionStatement = "Swag1.";
			var processHeader2 = BMSTestHelper.CreateWorkflow(jobHeader, "Nope");
			processHeader2.FH_CompletionStatement = "Swag2.";

			processHeader1.FH_ReleaseDateTime = new ZDateTime(2013, 1, 4);
			processHeader2.FH_ReleaseDateTime = new ZDateTime(2013, 1, 2);

			var task1 = BMSTestHelper.CreateTask(processHeader1);
			var task2 = BMSTestHelper.CreateTask(processHeader2);

			Factory.Save();

			var filter = new ProcessTaskFilterBusinessObject();
			((ModuleDateFilter)filter["Last Transfer Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["Last Transfer Date"]).Property1 = new ZDateTime(2013, 1, 1);
			((ModuleDateFilter)filter["Last Transfer Date"]).Property2 = new ZDateTime(2013, 1, 3);
			((ModuleDateFilter)filter["Last Transfer Date"]).IsActive = true;

			var pTasks = new ProcessTaskCollection(Factory);
			pTasks.Load(filter.Filter);

			AssertCollectionContains(task2, pTasks);
			AssertCollectionNotContains(task1, pTasks);
		}

		public void TestRestrictBMSRelatedFilters()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;
			Factory.Save();

			var filter = new ProcessTaskFilterBusinessObject();
			AssertNull(filter["Last Transfer Date"]);
			AssertNull(filter["Agreed Delivery Date"]);
			AssertNull(filter["Earliest Start Date"]);

			Factory.Save();

			BMSTestHelper.EnableBMSInRegistry();

			var filter2 = new ProcessTaskFilterBusinessObject();
			AssertNotNull(filter2["Last Transfer Date"]);
			AssertNotNull(filter2["Agreed Delivery Date"]);
			AssertNotNull(filter2["Earliest Start Date"]);
		}

		#endregion

		#region Overall

		public void TestIncludeTasksOnly()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			ProcessTask milestone = Factory.New<ProcessTask>();
			ProcessTask exception = Factory.New<ProcessTask>();
			ProcessTask trigger = Factory.New<ProcessTask>();

			milestone.IsMilestone = true;
			exception.IsException = true;
			trigger.IsWorkflowTrigger = true;
			Factory.Save();

			ProcessTask[] tasks = Factory.Load<ProcessTask>((new ProcessTaskFilterBusinessObject()).Filter);
			AssertEquals("Only tasks should be loaded", 1, tasks.Length);
			AssertEquals("Only tasks should be loaded", true, tasks[0].IsTask);
		}

		public void TestDontIncludeTaskTemplates()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			ProcessTask taskTemplate = template.WorkflowItems.AddNew();
			ProcessTask task = Factory.New<ProcessTask>();
			Factory.Save();

			ProcessTask[] tasks = Factory.Load<ProcessTask>((new ProcessTaskFilterBusinessObject()).Filter);
			AssertEquals("Only non-template tasks should be loaded", 1, tasks.Length);
		}

		#endregion

		#region Lookups

		public void TestStatuses()
		{
			ProcessTaskFilterBusinessObject filterBizo = new ProcessTaskFilterBusinessObject();
			AssertNotNull(filterBizo.Statuses);
			AssertEquals("Not Complete", filterBizo.Statuses.GetDescriptionFromCode("NCM"));
			AssertEquals("Assigned + Suspended", filterBizo.Statuses.GetDescriptionFromCode("A+S"));
			AssertEquals("Working + Suspended", filterBizo.Statuses.GetDescriptionFromCode("W+S"));
		}

		#endregion

		#region Implementation

		IBMTestHelper BMSTestHelper { get; set; }

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper = ObjectFactory.Get<IBMTestHelper>();
			BMSTestHelper.EnableBMSInRegistry();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ProcessTaskFilterBusinessObject();
		}

		#endregion
	}
}
