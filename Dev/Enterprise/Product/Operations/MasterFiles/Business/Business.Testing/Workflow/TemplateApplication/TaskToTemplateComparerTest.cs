using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class TaskToTemplateComparerTest : TestCaseWithFactory
	{
		#region Setup

		public (ProcessTask, ProcessTask) MakeTasks()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			var template = Factory.NewWithValidTestData<ProcessTask>();
			template.P9_ParentTableCode = ProcessTaskTemplateSchema.Constants.Prefix;
			template.P9_Type = task.P9_Type = Core.Constants.Workflow.MilestoneType;
			template.P9_Description = task.P9_Description = "Boggo";

			return (template, task);
		}

		#endregion

		public void TestDescription()
		{
			(var template, var task) = MakeTasks();

			Assert(task.IsDuplicateItemTemplateApplication(template));

			task.P9_Description = "I went to the park";
			template.P9_Description = "And ate a squirrel";

			Assert(!task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestP9_Type()
		{
			(var template, var task) = MakeTasks();

			task.P9_Type = Core.Constants.Workflow.ExceptionType;
			Assert(!task.IsDuplicateItemTemplateApplication(template));

			task.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			Assert(!task.IsDuplicateItemTemplateApplication(template));

			task.P9_Type = Core.Constants.Workflow.MilestoneType;
			Assert(task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestTemplatePK()
		{
			(var template, var task) = MakeTasks();

			Assert(!task.IsTask);
			task.P9_ParentTemplateID = template.PK;
			Assert(task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestP9_SE_NKMilestoneEvent()
		{
			(var template, var task) = MakeTasks();

			template.TriggerConditions.TriggerEventCode = "Zzz";
			task.TriggerConditions.TriggerEventCode = "MAA";
			Assert(!task.IsDuplicateItemTemplateApplication(template));

			task.TriggerConditions.TriggerEventCode = template.TriggerConditions.TriggerEventCode;
			Assert(task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestP9_Condition1()
		{
			(var template, var task) = MakeTasks();

			template.TemplateConditions.TemplateCondition1 = "REF";
			task.TemplateConditions.TemplateCondition1 = "xyz";
			Assert(!task.IsDuplicateItemTemplateApplication(template));

			task.TemplateConditions.TemplateCondition1 = template.TemplateConditions.TemplateCondition1;
			Assert(task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestP9_Condition2()
		{
			(var template, var task) = MakeTasks();

			template.TemplateConditions.TemplateCondition2 = "'1'";
			task.TemplateConditions.TemplateCondition2 = "XXL";
			Assert(!task.IsDuplicateItemTemplateApplication(template));

			task.TemplateConditions.TemplateCondition2 = template.TemplateConditions.TemplateCondition2;
			Assert(task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestP9_Condition2Value()
		{
			(var template, var task) = MakeTasks();

			template.TemplateConditions.TemplateCondition2Value = "v2";
			task.TemplateConditions.TemplateCondition2Value = "---";

			task.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			Assert(task.P9_ParentTemplateID != template.PK && template.PK.IsValid);
			Assert(!task.IsDuplicateItemTemplateApplication(template));

			template.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task.TemplateConditions.TemplateCondition2Value = template.TemplateConditions.TemplateCondition2Value;
			Assert(task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestOriginCountryCode()
		{
			(var template, var task) = MakeTasks();

			task.TemplateConditions.OriginCountryCode = "AU";
			template.TemplateConditions.OriginCountryCode = "SE";
			Assert(!task.IsDuplicateItemTemplateApplication(template));

			task.TemplateConditions.OriginCountryCode = template.TemplateConditions.OriginCountryCode;
			Assert(task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestDestinationCountryCode()
		{
			(var template, var task) = MakeTasks();

			task.TemplateConditions.DestinationCountryCode = "US";
			template.TemplateConditions.DestinationCountryCode = "SG";
			Assert(!task.IsDuplicateItemTemplateApplication(template));

			task.TemplateConditions.DestinationCountryCode = template.TemplateConditions.DestinationCountryCode;
			Assert(task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestP9_RespondToCascadedEvents()
		{
			(var template, var task) = MakeTasks();

			task.P9_RespondToCascadedEvents = false;
			template.P9_RespondToCascadedEvents = true;
			Assert(!task.IsDuplicateItemTemplateApplication(template));

			task.P9_RespondToCascadedEvents = template.P9_RespondToCascadedEvents;
			Assert(task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestP9_CascadedEventsContext()
		{
			(var template, var task) = MakeTasks();

			task.P9_RespondToCascadedEvents = template.P9_RespondToCascadedEvents = true;

			task.P9_CascadedEventsContext = "AAA";
			template.P9_CascadedEventsContext = "BBB";
			Assert(!task.IsDuplicateItemTemplateApplication(template));

			task.P9_CascadedEventsContext = template.P9_CascadedEventsContext;
			Assert(task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestP9_TriggerField()
		{
			(var template, var task) = MakeTasks();

			task.TriggerConditions.TriggerFieldName = "DATE";
			template.TriggerConditions.TriggerFieldName = "TIME";
			Assert(!task.IsDuplicateItemTemplateApplication(template));

			task.TriggerConditions.TriggerFieldName = template.TriggerConditions.TriggerFieldName;
			Assert(task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestP9_TriggerCondition()
		{
			(var template, var task) = MakeTasks();

			task.TriggerConditions.TriggerCondition = "GT";
			template.TriggerConditions.TriggerCondition = "LT";
			Assert(!task.IsDuplicateItemTemplateApplication(template));

			task.TriggerConditions.TriggerCondition = template.TriggerConditions.TriggerCondition;
			Assert(task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestP9_TriggerConditionValue()
		{
			(var template, var task) = MakeTasks();

			task.TriggerConditions.TriggerConditionValue = "T1";
			template.TriggerConditions.TriggerConditionValue = "T2";
			template.TriggerConditions.TriggerCondition = "+++";
			Assert(!task.IsDuplicateItemTemplateApplication(template));

			Assert(!task.IsDuplicateItemTemplateApplication(template));

			task.TriggerConditions.TriggerConditionValue = template.TriggerConditions.TriggerConditionValue;
			task.TriggerConditions.TriggerCondition = template.TriggerConditions.TriggerCondition;
			Assert(task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestTriggerContext()
		{
			(var template, var task) = MakeTasks();
			task.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Event;
			template.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Default;
			AssertEquals(false, task.IsDuplicateItemTemplateApplication(template));
			template.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Event;
			AssertEquals(true, task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestTriggerContext_DifferentStaff_Merge()
		{
			(var template, var task) = MakeTasks();
			task.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			template.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			task.TriggerConditions.TriggerStaffCode = "BIB";
			template.TriggerConditions.TriggerStaffCode = "NIM";
			AssertEquals(false, task.IsDuplicateItemTemplateApplication(template));
			template.TriggerConditions.TriggerStaffCode = "BIB";
			AssertEquals(true, task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestTriggerContext_DifferentBranch_NoMerge()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			(var template, var task) = MakeTasks();
			task.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			template.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			task.TriggerConditions.TriggerBranch = branch1.PK;
			template.TriggerConditions.TriggerBranch = branch2.PK;
			AssertEquals(false, task.IsDuplicateItemTemplateApplication(template));
			template.TriggerConditions.TriggerBranch = branch1.PK;
			AssertEquals(true, task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestTriggerContext_DifferentDepartment_NoMerge()
		{
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			(var template, var task) = MakeTasks();
			task.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			template.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			task.TriggerConditions.TriggerDepartment = department1.PK;
			template.TriggerConditions.TriggerDepartment = department2.PK;
			AssertEquals(false, task.IsDuplicateItemTemplateApplication(template));
			template.TriggerConditions.TriggerDepartment = department1.PK;
			AssertEquals(true, task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestTriggerContext_DifferentCompany_NoMerge()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			(var template, var task) = MakeTasks();
			task.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			template.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			task.TriggerConditions.TriggerCompany = company1.PK;
			template.TriggerConditions.TriggerCompany = company2.PK;
			AssertEquals(false, task.IsDuplicateItemTemplateApplication(template));
			template.TriggerConditions.TriggerCompany = company1.PK;
			AssertEquals(true, task.IsDuplicateItemTemplateApplication(template));
		}

		public void TestTriggerContext_SameTemplateAlwaysWins()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.P9_Description = "Wilson";
			templateMilestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			templateMilestone.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			templateMilestone.TriggerConditions.TriggerCompany = company1.PK;
			templateMilestone.TemplateConditions.TemplateCondition2 = "UDF";
			templateMilestone.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";

			Factory.Save();
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals(1, dummy.WorkflowItems.Count);
			dummy.WorkflowItems[0].P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			dummy.WorkflowItems[0].P9_Description = "Boogy";
			dummy.WorkflowItems[0].TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;

			dummy.ApplyWorkflowTemplates();
			AssertEquals(1, dummy.WorkflowItems.Count);
		}

		#region IsDuplicateItemTemplateApplication

		public (ProcessTask, ProcessTask, ProcessTask) Make2TasksFromTemplate()
		{
			var task1 = Factory.NewWithValidTestData<ProcessTask>();
			var task2 = Factory.NewWithValidTestData<ProcessTask>();
			var template = Factory.NewWithValidTestData<ProcessTask>();
			template.P9_ParentTableCode = ProcessTaskTemplateSchema.Constants.Prefix;
			template.P9_Type = task1.P9_Type = task2.P9_Type = Core.Constants.Workflow.MilestoneType;
			template.P9_Description = task1.P9_Description = task2.P9_Description = "Boggo";

			return (template, task1, task2);
		}

		public void TestIsDuplicateItemTemplateApplication_Description()
		{
			(_, var task1, var task2) = Make2TasksFromTemplate();

			Assert(task1.IsDuplicateItemTemplateApplication(task2));

			task1.P9_Description = "I went to the park";
			task2.P9_Description = "And ate a squirrel";

			Assert(!task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_P9_Type()
		{
			(_, var task1, var task2) = Make2TasksFromTemplate();

			task1.P9_Type = Core.Constants.Workflow.ExceptionType;
			Assert(!task1.IsDuplicateItemTemplateApplication(task2));

			task1.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			Assert(!task1.IsDuplicateItemTemplateApplication(task2));

			task1.P9_Type = Core.Constants.Workflow.MilestoneType;
			Assert(task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_TemplatePK()
		{
			(var template, var task1, var task2) = Make2TasksFromTemplate();

			Assert(!task1.IsTask);
			Assert(!task2.IsTask);
			task1.P9_ParentTemplateID = task2.P9_ParentTemplateID = template.PK;
			Assert(task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_P9_SE_NKMilestoneEvent()
		{
			(var template, var task1, var task2) = Make2TasksFromTemplate();

			task2.TriggerConditions.TriggerEventCode = template.TriggerConditions.TriggerEventCode = "Zzz";
			task1.TriggerConditions.TriggerEventCode = "MAA";
			Assert(!task1.IsDuplicateItemTemplateApplication(task2));

			task1.TriggerConditions.TriggerEventCode = task2.TriggerConditions.TriggerEventCode;
			Assert(task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_P9_Condition1()
		{
			(var template, var task1, var task2) = Make2TasksFromTemplate();

			task2.TemplateConditions.TemplateCondition1 = template.TemplateConditions.TemplateCondition1 = "REF";
			task1.TemplateConditions.TemplateCondition1 = "xyz";
			Assert(!task1.IsDuplicateItemTemplateApplication(task2));

			task1.TemplateConditions.TemplateCondition1 = template.TemplateConditions.TemplateCondition1;
			Assert(task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_P9_Condition2()
		{
			(var template, var task1, var task2) = Make2TasksFromTemplate();

			task2.TemplateConditions.TemplateCondition2 = template.TemplateConditions.TemplateCondition2 = "'1'";
			task1.TemplateConditions.TemplateCondition2 = "XXL";
			Assert(!task1.IsDuplicateItemTemplateApplication(task2));

			task1.TemplateConditions.TemplateCondition2 = template.TemplateConditions.TemplateCondition2;
			Assert(task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_P9_Condition2Value()
		{
			(var template, var task1, var task2) = Make2TasksFromTemplate();

			task2.TemplateConditions.TemplateCondition2Value = template.TemplateConditions.TemplateCondition2Value = "v2";
			task1.TemplateConditions.TemplateCondition2Value = "---";

			task1.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			Assert(task1.P9_ParentTemplateID != template.PK && template.PK.IsValid);
			Assert(!task1.IsDuplicateItemTemplateApplication(task2));

			task2.TemplateConditions.TemplateCondition2 = template.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task1.TemplateConditions.TemplateCondition2Value = template.TemplateConditions.TemplateCondition2Value;
			Assert(task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_OriginCountryCode()
		{
			(var template, var task1, var task2) = Make2TasksFromTemplate();

			task1.TemplateConditions.OriginCountryCode = "AU";
			task2.TemplateConditions.OriginCountryCode = template.TemplateConditions.OriginCountryCode = "SE";
			Assert(!task1.IsDuplicateItemTemplateApplication(task2));

			task1.TemplateConditions.OriginCountryCode = template.TemplateConditions.OriginCountryCode;
			Assert(task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_DestinationCountryCode()
		{
			(var template, var task1, var task2) = Make2TasksFromTemplate();

			task1.TemplateConditions.DestinationCountryCode = "US";
			task2.TemplateConditions.DestinationCountryCode = template.TemplateConditions.DestinationCountryCode = "SG";
			Assert(!task1.IsDuplicateItemTemplateApplication(task2));

			task1.TemplateConditions.DestinationCountryCode = template.TemplateConditions.DestinationCountryCode;
			Assert(task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_P9_RespondToCascadedEvents()
		{
			(var template, var task1, var task2) = Make2TasksFromTemplate();

			task1.P9_RespondToCascadedEvents = false;
			task2.P9_RespondToCascadedEvents = template.P9_RespondToCascadedEvents = true;
			Assert(!task1.IsDuplicateItemTemplateApplication(task2));

			task1.P9_RespondToCascadedEvents = template.P9_RespondToCascadedEvents;
			Assert(task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_P9_CascadedEventsContext()
		{
			(var template, var task1, var task2) = Make2TasksFromTemplate();

			task1.P9_RespondToCascadedEvents = task2.P9_RespondToCascadedEvents = template.P9_RespondToCascadedEvents = true;

			task1.P9_CascadedEventsContext = "AAA";
			task2.P9_CascadedEventsContext = template.P9_CascadedEventsContext = "BBB";
			Assert(!task1.IsDuplicateItemTemplateApplication(task2));

			task1.P9_CascadedEventsContext = template.P9_CascadedEventsContext;
			Assert(task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_P9_TriggerField()
		{
			(var template, var task1, var task2) = Make2TasksFromTemplate();

			task1.TriggerConditions.TriggerFieldName = "DATE";
			task2.TriggerConditions.TriggerFieldName = template.TriggerConditions.TriggerFieldName = "TIME";
			Assert(!task1.IsDuplicateItemTemplateApplication(task2));

			task1.TriggerConditions.TriggerFieldName = template.TriggerConditions.TriggerFieldName;
			Assert(task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_P9_TriggerCondition()
		{
			(var template, var task1, var task2) = Make2TasksFromTemplate();

			task1.TriggerConditions.TriggerCondition = "GT";
			task2.TriggerConditions.TriggerCondition = template.TriggerConditions.TriggerCondition = "LT";
			Assert(!task1.IsDuplicateItemTemplateApplication(task2));

			task1.TriggerConditions.TriggerCondition = template.TriggerConditions.TriggerCondition;
			Assert(task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_P9_TriggerConditionValue()
		{
			(var template, var task1, var task2) = Make2TasksFromTemplate();

			task1.TriggerConditions.TriggerConditionValue = "T1";
			task2.TriggerConditions.TriggerConditionValue = template.TriggerConditions.TriggerConditionValue = "T2";
			task2.TriggerConditions.TriggerCondition = template.TriggerConditions.TriggerCondition = "+++";

			Assert(!task1.IsDuplicateItemTemplateApplication(task2));

			task1.TriggerConditions.TriggerConditionValue = template.TriggerConditions.TriggerConditionValue;
			task1.TriggerConditions.TriggerCondition = template.TriggerConditions.TriggerCondition;
			Assert(task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_TriggerContext()
		{
			(var template, var task1, var task2) = Make2TasksFromTemplate();
			task1.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Event;
			task2.TriggerConditions.TriggerContextCode = template.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Default;
			AssertEquals(false, task1.IsDuplicateItemTemplateApplication(task2));
			task2.TriggerConditions.TriggerContextCode = template.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Event;
			AssertEquals(true, task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_TriggerContext_DifferentStaff_Merge()
		{
			(var template, var task1, var task2) = Make2TasksFromTemplate();
			task1.TriggerConditions.TriggerContextCode = task2.TriggerConditions.TriggerContextCode = template.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			task1.TriggerConditions.TriggerStaffCode = "BIB";
			task2.TriggerConditions.TriggerStaffCode = template.TriggerConditions.TriggerStaffCode = "NIM";
			AssertEquals(false, task1.IsDuplicateItemTemplateApplication(task2));
			task2.TriggerConditions.TriggerStaffCode = template.TriggerConditions.TriggerStaffCode = "BIB";
			AssertEquals(true, task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_TriggerContext_DifferentBranch_NoMerge()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			(var template, var task1, var task2) = Make2TasksFromTemplate();
			task1.TriggerConditions.TriggerContextCode = task2.TriggerConditions.TriggerContextCode = template.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			task1.TriggerConditions.TriggerBranch = branch1.PK;
			task2.TriggerConditions.TriggerBranch = template.TriggerConditions.TriggerBranch = branch2.PK;
			AssertEquals(false, task1.IsDuplicateItemTemplateApplication(task2));
			task2.TriggerConditions.TriggerBranch = template.TriggerConditions.TriggerBranch = branch1.PK;
			AssertEquals(true, task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_TriggerContext_DifferentDepartment_NoMerge()
		{
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			(var template, var task1, var task2) = Make2TasksFromTemplate();
			task1.TriggerConditions.TriggerContextCode = task2.TriggerConditions.TriggerContextCode = template.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			task1.TriggerConditions.TriggerDepartment = department1.PK;
			task2.TriggerConditions.TriggerDepartment = template.TriggerConditions.TriggerDepartment = department2.PK;
			AssertEquals(false, task1.IsDuplicateItemTemplateApplication(task2));
			task2.TriggerConditions.TriggerDepartment = template.TriggerConditions.TriggerDepartment = department1.PK;
			AssertEquals(true, task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_TriggerContext_DifferentCompany_NoMerge()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			(var template, var task1, var task2) = Make2TasksFromTemplate();
			task1.TriggerConditions.TriggerContextCode = task2.TriggerConditions.TriggerContextCode = template.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			task1.TriggerConditions.TriggerCompany = company1.PK;
			task2.TriggerConditions.TriggerCompany = template.TriggerConditions.TriggerCompany = company2.PK;
			AssertEquals(false, task1.IsDuplicateItemTemplateApplication(task2));
			task2.TriggerConditions.TriggerCompany = template.TriggerConditions.TriggerCompany = company1.PK;
			AssertEquals(true, task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_IsTemplateReapplication_Sequence()
		{
			var task1 = Factory.NewWithValidTestData<ProcessTask>();
			var task2 = Factory.NewWithValidTestData<ProcessTask>();
			var template = Factory.NewWithValidTestData<ProcessTask>();
			template.P9_ParentTableCode = ProcessTaskTemplateSchema.Constants.Prefix;
			template.P9_Type = task1.P9_Type = task2.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task1.P9_ParentTemplateID = task2.P9_ParentTemplateID = template.PK;
			task1.P9_Sequence = task2.P9_Sequence = template.P9_Sequence = 100;
			task2.IsTemplateReapplication = true;

			Assert(task1.IsDuplicateItemTemplateApplication(task2));

			task2.P9_Sequence = 200;

			Assert(!task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_IsTemplateReapplication_TemplateID()
		{
			var task1 = Factory.NewWithValidTestData<ProcessTask>();
			var task2 = Factory.NewWithValidTestData<ProcessTask>();
			var template = Factory.NewWithValidTestData<ProcessTask>();
			template.P9_ParentTableCode = ProcessTaskTemplateSchema.Constants.Prefix;
			template.P9_Type = task1.P9_Type = task2.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task1.P9_ParentTemplateID = task2.P9_ParentTemplateID = template.PK;
			task1.P9_Sequence = task2.P9_Sequence = template.P9_Sequence = 100;
			task2.IsTemplateReapplication = true;

			Assert(task1.IsDuplicateItemTemplateApplication(task2));

			task1.P9_ParentTemplateID = Guid.Empty;

			Assert(!task1.IsDuplicateItemTemplateApplication(task2));
		}

		public void TestIsDuplicateItemTemplateApplication_IsTemplateReapplication_P9_Type()
		{
			var task1 = Factory.NewWithValidTestData<ProcessTask>();
			var task2 = Factory.NewWithValidTestData<ProcessTask>();
			var template = Factory.NewWithValidTestData<ProcessTask>();
			template.P9_ParentTableCode = ProcessTaskTemplateSchema.Constants.Prefix;
			template.P9_Type = task1.P9_Type = task2.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task1.P9_ParentTemplateID = task2.P9_ParentTemplateID = template.PK;
			task1.P9_Sequence = task2.P9_Sequence = template.P9_Sequence = 100;
			task2.IsTemplateReapplication = true;

			Assert(task1.IsDuplicateItemTemplateApplication(task2));

			task1.P9_Type = "CDF";

			Assert(!task1.IsDuplicateItemTemplateApplication(task2));
		}

		#endregion
	}
}
