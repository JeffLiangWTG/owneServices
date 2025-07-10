using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(Project))]
	public class ProjectTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var project = Factory.New<Project>();
			AssertContainsOnlyOneMatch("Project Created", project.LogText);
		}

		#region TestType

		public void TestType()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.ProjectTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var project = Factory.New<Project>();
			project.WKP_Type = "1AA";
			project.WKP_SubType = "2AA";
			project.WKP_Module = "3AA";
			project.WKP_Priority = "ANY";
			AssertNoNotifications(project);

			project.WKP_Type = "1BB";
			project.Validation.ValidateWKP_SubType();
			project.Validation.ValidateWKP_Module();
			project.Validation.ValidateWKP_Priority();
			CombineAssertions(() =>
			{
				AssertNoNotifications(project);
				AssertEquals("1BB", project.WKP_Type);
				AssertEquals("", project.WKP_SubType);
				AssertEquals("", project.WKP_Module);
				AssertEquals("ANY", project.WKP_Priority);
			});
		}

		#endregion

		#region TestSubType

		public void TestSubType()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.ProjectTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var project = Factory.New<Project>();
			project.WKP_Type = "1AA";
			project.WKP_SubType = "2AA";
			project.WKP_Module = "3AA";
			project.WKP_Priority = "ANY";
			AssertNoNotifications(project);

			project.WKP_SubType = "2CC";
			project.Validation.ValidateWKP_Module();
			project.Validation.ValidateWKP_Priority();
			CombineAssertions(() =>
			{
				AssertNoNotifications(project);
				AssertEquals("1AA", project.WKP_Type);
				AssertEquals("2CC", project.WKP_SubType);
				AssertEquals("", project.WKP_Module);
				AssertEquals("ANY", project.WKP_Priority);
			});
		}

		#endregion

		#region TestModule

		public void TestModule()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.ProjectTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var project = Factory.New<Project>();
			project.WKP_Type = "1AA";
			project.WKP_SubType = "2AA";
			project.WKP_Module = "3AA";
			project.WKP_Priority = "4AA";
			AssertNoNotifications(project);

			project.WKP_Module = "3S2";
			project.Validation.ValidateWKP_Priority();
			CombineAssertions(() =>
			{
				AssertNoNotifications(project);
				AssertEquals("1AA", project.WKP_Type);
				AssertEquals("2AA", project.WKP_SubType);
				AssertEquals("3S2", project.WKP_Module);
				AssertEquals("", project.WKP_Priority);
			});
		}

		#endregion

		public void TestLogTextMaxLength()
		{
			var project = Factory.New<Project>();
			string normalText = "".PadLeft(10000, 'a');
			string largeText = "".PadLeft(100000, 'a');

			project.LogText = normalText;
			AssertEquals(normalText, project.LogText);

			project.LogText = largeText;
			AssertNotEquals(largeText, project.LogText);
			AssertEquals(largeText.Substring(0, project.LogTextInfo.MaxLength), project.LogText);
		}

		public void TestReOpenedStatusLog()
		{
			var project = Factory.New<Project>();
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNotContains("Re-Opened", project.LogText);
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertNotContains("Should not say it's re-opened when switching from closed to cancelled", "Re-Opened", project.LogText);
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertContainsOnlyOneMatch("Re-Opened", project.LogText);

			project = Factory.New<Project>();
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertNotContains("Re-Opened", project.LogText);
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNotContains("Should not say it's re-opened when switching from cancelled to closed", "Re-Opened", project.LogText);
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertContainsOnlyOneMatch("Re-Opened", project.LogText);
		}

		public void TestReOpenedStatusLog_ReOpenAction()
		{
			var project = Factory.New<Project>();
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNotContains("Re-Opened", project.LogText);
			project.ReOpen("");
			AssertContainsOnlyOneMatch("Re-Opened", project.LogText);

			project = Factory.New<Project>();
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertNotContains("Re-Opened", project.LogText);
			project.ReOpen("");
			AssertContainsOnlyOneMatch("Re-Opened", project.LogText);
		}

		public void TestClosedStatusLog()
		{
			var project = Factory.New<Project>();
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertContainsOnlyOneMatch(ProcessTaskStatusCodeList.Descriptions.Closed, project.LogText);

			project = Factory.New<Project>();
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertContainsOnlyOneMatch(ProcessTaskStatusCodeList.Descriptions.Cancelled, project.LogText);
		}

		public void TestClosedStatusLog_ClosedAction()
		{
			var project = Factory.New<Project>();
			project.Close(ProcessTaskStatusCodeList.Codes.Closed, "");
			AssertContainsOnlyOneMatch(ProcessTaskStatusCodeList.Descriptions.Closed, project.LogText);

			project = Factory.New<Project>();
			project.Close(ProcessTaskStatusCodeList.Codes.Cancelled, "");
			AssertContainsOnlyOneMatch(ProcessTaskStatusCodeList.Descriptions.Cancelled, project.LogText);
		}

		public void TestClose_ShouldIgnoreTaskCancellationValidationAndCancelAllTasks()
		{
			var project = Factory.New<Project>();
			var task = project.WorkflowItems.Tasks.AddNew();
			task.P9_GS_NKAssignedStaffMember = string.Empty;
			task.P9_Type = "UDF";

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypes = categorisedTaskTypes.AddNew();
			taskTypes.Code = "WKP";
			var taskType = taskTypes.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.CanCancelTask = false;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
			Env.Security.WorkflowTasksCanCancelAllTasks.IsAllowed = false;

			Factory.Save();

			project.Close(ProcessTaskStatusCodeList.Codes.Cancelled, "");
			task.Validation.ValidateP9_Status();

			AssertNoErrors("Validation on task status should be disabled when cancelling a project", task.P9_StatusInfo);
		}

		public void TestNoteTypes()
		{
			var project = Factory.New<Project>();
			AssertEquals(1, project.NoteTypes.Count);
			AssertCollectionContains(PredefinedNoteTypes.Instance.ProjectLog, project.NoteTypes);
		}

		public void TestLogNote()
		{
			var project = Factory.NewWithValidTestData<Project>();
			AssertEquals("Log is read-only", true, project.LogTextInfo.ReadOnly);

			project.LogText = "Haha";
			AssertEquals(true, project.HasChanges);

			Factory.Save();
			AssertEquals("Haha", new BusinessObjectFactory().Load<Project>(project.PK).Notes.FindByDescription(PredefinedNoteTypes.Instance.ProjectLog.Description)[0].ST_NoteText);
		}

		[ExpectNoExceptions]
		public void TestPreSaveValidationWhenTickCustomNoteType()
		{
			var project = Factory.New<Project>();
			var notes = project.Notes.FindByDescription(PredefinedNoteTypes.Instance.ProjectLog.Description);
			notes[0].ST_IsCustomDescription = true;
			project.RunPreSaveValidation();
		}

		[TestDate(2014, 7, 4, 1, 0, 0)]
		public void TestCommentMaxLength()
		{
			int metaLength = 142;
			var project = Factory.New<Project>();
			AssertEquals("Precondition", metaLength, project.LogWithTimeStampAndCommentor(string.Empty).Length);
			int initialLogLength = project.LogText.Length;

			project.AddToLog(new string('A', 10000));
			int logWithMetaLength = 10000 + metaLength + initialLogLength;
			AssertEquals(logWithMetaLength, project.LogText.Length);
			// The added 20 represents appended comments such as the following:
			// - 'Closed - '
			// - 'Cancelled - '
			// - 'Re-Opened - '
			// See Enterprise.ProcessManagement.Business.Project.CommentMaxLength
			AssertEquals("The max length of the next comment", 50000 - (logWithMetaLength + metaLength + 20), project.CommentMaxLength);

			project.LogText = string.Empty;
			project.AddToLog(new string('B', 50000 - metaLength));
			AssertEquals(50000, project.LogText.Length);
			AssertEquals("The max length of the next comment", 0, project.CommentMaxLength);
		}

		public void TestIsAutoLogged()
		{
			AssertEquals(true, CachedProject.IsAutoAdminBusinessObjectLoggerEnabled);
		}

		public void TestOnSaving()
		{
			var project = Factory.NewWithValidTestData<Project>();
			Factory.Save();
			AssertEquals("PRJ00000001", project.WKP_ProjectNumber);
			var project2 = Factory.NewWithValidTestData<Project>();
			Factory.Save();
			AssertEquals("PRJ00000002", project2.WKP_ProjectNumber);
		}

		public void TestHumanReadableName()
		{
			var project = Factory.NewWithValidTestData<Project>();
			AssertEquals("Project", project.HumanReadableName);
			Factory.Save();
			AssertEquals("Project PRJ00000001", project.HumanReadableName);
		}

		public void TestHumanReadableShortcutNameCore()
		{
			var project = Factory.NewWithValidTestData<Project>();
			project.WKP_Summary = "Some Summary";
			Factory.Save();
			AssertEquals(string.Format("PRJ00000001 - {0} - {1}", project.ClientOrganisation.OH_Code, "Some Summary"), project.HumanReadableShortcutName);
		}

		public void TestInvoicingSupporter()
		{
			var project = Factory.New<Project>();
			IJobInvoicingPlugIn job = project;
			AssertType(typeof(ProjectInvoicingSupporter), job.InvoicingSupporter);
		}

		public void TestDocManagerInfo()
		{
			Project project = Factory.New<Project>();
			DocManagerInfo support = project.DocManagerInfo;
			AssertEquals(project, support.BusinessEntity);
			AssertEquals(Core.Constants.DocManagerCodes.Project, support.DocManagerCode);
		}

		public void TestAllowInvoiceDeletion()
		{
			IJobHeaderParent bizo = CachedProject;
			Assert(bizo.AllowInvoiceDeletion);
		}

		#region Tasks / Status

		public void TestAssignedTo()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "XYZ";

			Project project = Factory.NewWithValidTestData<Project>();
			ProcessTask task1 = project.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			ProcessTask task2 = project.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			AssertEquals(GlbStaff.CurrentUser.PK, project.AssignedToStaff.PK);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(staff.PK, project.AssignedToStaff.PK);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(staff.PK, project.AssignedToStaff.PK);

			project.WorkflowItems.AddNew(); // Assigned staff is automatically set to staff2 on creation
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			AssertEquals(staff.PK, project.AssignedToStaff.PK);
		}

		public void TestCurrentStartableTask()
		{
			var project = Factory.NewWithValidTestData<Project>();
			CurrentTaskTestHelper.AssertCurrentStartableTask(project, () => project.CurrentTask);
		}

		public void TestCurrentOrNextStartableTask()
		{
			var project = Factory.NewWithValidTestData<Project>();
			CurrentTaskTestHelper.AssertCurrentOrNextStartableTask(project, () => project.CurrentOrNextTask);
		}

		public void TestCurrentTaskProperties()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "XYZ";

			Project project = Factory.NewWithValidTestData<Project>();
			AssertEquals("", project.CurrentTaskAssignedToCode);
			AssertEquals(ZGuid.Empty, project.CurrentTaskAssignedToGroupPK);
			AssertNull(project.CurrentTaskAssignedTo);
			AssertNull(project.CurrentTaskAssignedToGroup);

			ProcessTask task1 = project.WorkflowItems.AddNew();
			task1.P9_Description = "Task 1";

			ProcessTask task2 = project.WorkflowItems.AddNew();
			task2.P9_TaskID = "T0000100";
			task2.P9_Description = "Task 2";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GG_AssignedGroup = group.PK;

			ProcessTask task3 = project.WorkflowItems.AddNew();
			task3.P9_Description = "Task 3";
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			AssertEquals(GlbStaff.CurrentUser.PK, project.CurrentTaskAssignedTo.PK);
			AssertEquals(group.PK, project.CurrentTaskAssignedToGroupPK);
			AssertEquals(GlbStaff.CurrentUser.PK, project.CurrentTaskAssignedTo.PK);
			AssertEquals(group, project.CurrentTaskAssignedToGroup);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
		}

		public void TestCurrentTaskStatus()
		{
			Project project = Factory.NewWithValidTestData<Project>();
			AssertEquals(ZString.Empty, project.CurrentTaskStatus);

			ProcessTask task1 = project.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			ProcessTask task2 = project.WorkflowItems.AddNew();
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			ProcessTask task3 = project.WorkflowItems.AddNew();
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, project.CurrentTaskStatus);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, project.CurrentTaskStatus);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, project.CurrentTaskStatus);
		}

		public void TestCurrentOrNextTask()
		{
			var project = Factory.NewWithValidTestData<Project>();

			var task1 = Factory.NewWithPrimaryKey<ProcessTask>(new Guid("579520de-ba34-4339-b9ae-03ed90c5c864"));
			var task2 = Factory.NewWithPrimaryKey<ProcessTask>(new Guid("179520de-ba34-4339-b9ae-03ed90c5c864"));
			var task3 = Factory.NewWithPrimaryKey<ProcessTask>(new Guid("379520de-ba34-4339-b9ae-03ed90c5c864"));
			var task4 = Factory.NewWithPrimaryKey<ProcessTask>(new Guid("279520de-ba34-4339-b9ae-03ed90c5c864"));
			var task5 = Factory.NewWithPrimaryKey<ProcessTask>(new Guid("479520de-ba34-4339-b9ae-03ed90c5c864"));

			task1.P9_TaskID = "T00004005";
			task2.P9_TaskID = "T00004001";
			task3.P9_TaskID = "T00004003";
			task4.P9_TaskID = "T00004002";
			task5.P9_TaskID = "T00004004";

			project.WorkflowItems.AddRange(task1, task2, task3, task4, task5);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertNull(project.CurrentOrNextTask);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(task2, project.CurrentOrNextTask);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(task4, project.CurrentOrNextTask);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertEquals(task1, project.CurrentOrNextTask);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(task2, project.CurrentOrNextTask);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task1.P9_Sequence = 5;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task2.P9_Sequence = 3;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task3.P9_Sequence = 1;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task4.P9_Sequence = 2;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task5.P9_Sequence = 4;
			AssertEquals(task3.PK, project.CurrentOrNextTask.PK);

			task1.P9_Sequence = 1;
			task2.P9_Sequence = 1;
			task3.P9_Sequence = 1;
			task4.P9_Sequence = 1;
			task5.P9_Sequence = 1;
			AssertEquals(task2, project.CurrentOrNextTask);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task3.P9_Sequence = 2;
			task4.P9_Sequence = 2;
			task5.P9_Sequence = 2;
			AssertEquals(task4, project.CurrentOrNextTask);
		}

		public void TestCurrentOrNextTaskAssignedToCodeAndName()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ZZZ";
			staff.GS_FullName = "Zaphod B";

			var project = Factory.New<Project>();
			var task = project.WorkflowItems.Tasks.AddNew();
			task.P9_GS_NKAssignedStaffMember = "ZZZ";

			AssertEquals("ZZZ  Zaphod B", project.CurrentOrNextTaskAssignedToCodeAndName);
		}

		public void TestLogChanges()
		{
			Project proj = Factory.NewWithValidTestData<Project>();
			proj.WKP_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			proj.WKP_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			AssertEquals(true, proj.Logs.HasLogWith(StmALogSchema.SL_Reference, "WRK"));

			proj.WKP_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();
			AssertEquals(true, proj.Logs.HasLogWith(StmALogSchema.SL_Reference, "OPN"));
		}

		public void TestOverallTaskStatusCode()
		{
			var project = Factory.New<Project>();
			AssertEquals("", project.OverallTaskStatusCode);

			var task1 = project.WorkflowItems.Tasks.AddNew();
			var task2 = project.WorkflowItems.Tasks.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, project.OverallTaskStatusCode);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, project.OverallTaskStatusCode);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, project.OverallTaskStatusCode);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, project.OverallTaskStatusCode);
		}

		public void TestOverallTaskStatusCodeAndDescription()
		{
			var project = Factory.New<Project>();
			AssertEquals("", project.OverallTaskStatusCodeAndDescription);

			var task1 = project.WorkflowItems.Tasks.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed + "  " + ProcessTaskStatusCodeList.Descriptions.Closed, project.OverallTaskStatusCodeAndDescription);
		}

		public void TestOperationalActionFields()
		{
			var properties = typeof(Project).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var generator = new OperationalActionFieldGenerator();

			List<string> editableNames = new List<string>();
			List<string> readonlyNames = new List<string>();

			string lastPropertyName = "";
			foreach (PropertyInfo info in properties)
			{
				if (lastPropertyName == info.Name)
				{
					continue;
				}

				lastPropertyName = info.Name;

				switch (ReflectionHelper.Classify(info))
				{
					case PropertyClassification.Updatable:
						OperationalActionFieldSupporter fieldSupporter = generator.CreateField(new PropertyInfo[] { info });

						if (fieldSupporter != null)
						{
							if (fieldSupporter.ReadOnly)
							{
								readonlyNames.Add(info.Name);
							}
							else
							{
								editableNames.Add(info.Name);
							}
						}
						break;

					case PropertyClassification.FollowSingle:
					case PropertyClassification.FollowCollection:
						editableNames.Add(info.Name);
						break;
				}
			}
			editableNames.Sort();
			readonlyNames.Sort();

			string[] expectedEditableNames =
			{
				AutoWorkProject.Schema.WKP_Type,
				AutoWorkProject.Schema.WKP_SubType,
				AutoWorkProject.Schema.WKP_Module,
				AutoWorkProject.Schema.WKP_Priority,
				AutoWorkProject.Schema.WKP_P8_Opportunity,
				AutoWorkProject.Schema.WKP_GS_NKProjectManager,
				AutoWorkProject.Schema.WKP_Summary,
				AutoWorkProject.Schema.WKP_SystemCreateTimeUtc,
				AutoWorkProject.Schema.WKP_ClosedDate,
				AutoWorkProject.Schema.WKP_SystemCreateUser,
				AutoWorkProject.Schema.WKP_SystemCreateBranch,
				AutoWorkProject.Schema.WKP_SystemCreateDepartment,
				AutoWorkProject.Schema.WKP_SystemLastEditTimeUtc,
				AutoWorkProject.Schema.WKP_SystemLastEditUser,
				AutoWorkProject.Schema.WKP_Status,
				AutoWorkProject.Schema.WKP_OA_ClientAddress,
				AutoWorkProject.Schema.WKP_OC_Contact,
				AutoWorkProject.Schema.WKP_OC_TechnicalContact,
				"Workflows",
				"WorkflowItems",
				"Conversation"
			};

			string[] expectedReadonlyNames =
			{
				AutoWorkProject.Schema.WKP_ProjectNumber,
				"InstantiationTime",
				"DeferredDateMet",
				"HasDeferred",
				"DeferredDateLocal",
				"IsClosedOrCancelled",
				"ReleaseSequenceDate",
				"ReleaseSequencePosition",
				"ReleaseSequenceValue",
				"ReleaseSequenceInvestment",
			};
			Array.Sort(expectedEditableNames);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("editable", expectedEditableNames, editableNames);
				AssertContainsExactElementsInAnyOrder("readonly", expectedReadonlyNames, readonlyNames);
			});
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestReleaseSequence_ShouldReturnSequenceValues_WhenReleaseSequenceModuleEnabled()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			helper.CreateSystem(Factory, WorkflowDescriptors.ProjectWorkflowDescriptorCode);

			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = true;

			var project = Factory.NewWithValidTestData<Project>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(project, Factory, true);
			jobHeader.FH_AgreedDeliveryDate = new ZDateTime(2021, 11, 23);

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var sequence = helper.CreateReleaseSequence(Factory, group.PK.ToGuid(), "Name of sequence");
			helper.CreateReleaseSequenceItem(sequence, jobHeader, position: 1, value: 2, investment: 3);

			Factory.Save();

			AssertEquals("Name of sequence", project.ReleaseSequenceName);
			AssertEquals(1, project.ReleaseSequencePosition);
			AssertEquals(2, project.ReleaseSequenceValue);
			AssertEquals(3, project.ReleaseSequenceInvestment);
			AssertEquals(new ZDateTime(2021, 11, 23, 0, 0, 0), project.ReleaseSequenceDate);
			AssertEquals("23-Nov-21 00:00", project.ReleaseSequenceDateAsText);
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestReleaseSequence_ShouldReturnEmptyValues_WhenReleaseSequenceModuleDisabled()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			helper.CreateSystem(Factory, WorkflowDescriptors.ProjectWorkflowDescriptorCode);

			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = false;

			var project = Factory.NewWithValidTestData<Project>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(project, Factory, true);
			jobHeader.FH_AgreedDeliveryDate = new ZDateTime(2021, 11, 23, 0, 0, 0);

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var sequence = helper.CreateReleaseSequence(Factory, group.PK.ToGuid(), "Name of sequence");
			helper.CreateReleaseSequenceItem(sequence, jobHeader, position: 1, value: 2, investment: 3);

			Factory.Save();

			AssertEquals(ZString.Empty, project.ReleaseSequenceName);
			AssertEquals(0, project.ReleaseSequencePosition);
			AssertEquals(0, project.ReleaseSequenceValue);
			AssertEquals(0, project.ReleaseSequenceInvestment);
			AssertEquals(ZDateTime.Empty, project.ReleaseSequenceDate);
			AssertEquals(ZString.Empty, project.ReleaseSequenceDateAsText);
		}

		#endregion

		#region Property Descriptions

		public void TestProjectTypeDescription()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.ProjectTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var project = Factory.New<Project>();
			AssertEquals("", project.TypeDescription);

			project.WKP_Type = "1AA";
			AssertEquals("1AA depth 1", project.TypeDescription);

			project.WKP_Type = "1ZZ";
			AssertEquals("1ZZ depth 1", project.TypeDescription);
		}

		public void TestProjectSubtypeDescription()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.ProjectTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var project = Factory.New<Project>();
			AssertEquals("", project.SubtypeDescription);

			project.WKP_SubType = "2AA";
			AssertEquals("2AA depth 2", project.SubtypeDescription);

			project.WKP_SubType = "2ZZ";
			AssertEquals("2ZZ depth 2", project.SubtypeDescription);
		}

		public void TestProjectModuleDescription()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.ProjectTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var project = Factory.New<Project>();
			AssertEquals("", project.ModuleDescription);

			project.WKP_Module = "3AA";
			AssertEquals("3AA depth 3", project.ModuleDescription);

			project.WKP_Module = "3ZZ";
			AssertEquals("3ZZ depth 3", project.ModuleDescription);
		}

		public void TestPriorityDescription()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.ProjectTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var project = Factory.New<Project>();
			AssertEquals("", project.PriorityDescription);

			project.WKP_Priority = "4AA";
			AssertEquals("4AA depth 4", project.PriorityDescription);

			project.WKP_Priority = "4ZZ";
			AssertEquals("4ZZ depth 4", project.PriorityDescription);
		}

		#endregion

		#region Action Menu Operations

		public void TestClose()
		{
			var project = Factory.New<Project>();
			var task = project.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = "E";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var task2 = project.WorkflowItems.AddNew();
			var task3 = project.WorkflowItems.AddNew();
			var task4 = project.WorkflowItems.AddNew();
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals("Precondition", string.Empty, project.WKP_Status);

			project.Close(ProcessTaskStatusCodeList.Codes.Closed, "");
			AssertEquals(AutoWorkProject.Schema.WKP_Status, ProcessTaskStatusCodeList.Codes.Closed, project.WKP_Status);
			AssertEquals("task", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			AssertEquals("task2", ProcessTaskStatusCodeList.Codes.Closed, task2.P9_Status);
			AssertEquals("task3", ProcessTaskStatusCodeList.Codes.Closed, task3.P9_Status);
			AssertEquals("task4", ProcessTaskStatusCodeList.Codes.Cancelled, task4.P9_Status);
		}

		public void TestCancel()
		{
			var project = Factory.New<Project>();
			var task = project.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = "E";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var task2 = project.WorkflowItems.AddNew();
			var task3 = project.WorkflowItems.AddNew();
			var task4 = project.WorkflowItems.AddNew();
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Precondition", string.Empty, project.WKP_Status);

			project.Close(ProcessTaskStatusCodeList.Codes.Cancelled, "");
			AssertEquals(AutoWorkProject.Schema.WKP_Status, ProcessTaskStatusCodeList.Codes.Cancelled, project.WKP_Status);
			AssertEquals("task", ProcessTaskStatusCodeList.Codes.Cancelled, task.P9_Status);
			AssertEquals("task2", ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);
			AssertEquals("task3", ProcessTaskStatusCodeList.Codes.Cancelled, task3.P9_Status);
			AssertEquals("task4", ProcessTaskStatusCodeList.Codes.Closed, task4.P9_Status);
		}

		public void TestReOpen()
		{
			var project = Factory.New<Project>();
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Closed;
			project.WKP_ClosedDate = ZDateTime.UtcNow;
			var task = project.WorkflowItems.AddNew();
			var task2 = project.WorkflowItems.AddNew();
			var task3 = project.WorkflowItems.AddNew();
			var task4 = project.WorkflowItems.AddNew();
			var task5 = project.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = "E";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task3.P9_GS_NKAssignedStaffMember = "E";
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			project.ReOpen("");
			AssertEquals(AutoWorkProject.Schema.WKP_Status, ProcessTaskStatusCodeList.Codes.Assigned, project.WKP_Status);
			AssertEquals("task", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("task2", ProcessTaskStatusCodeList.Codes.Open, task2.P9_Status);
			AssertEquals("task3", ProcessTaskStatusCodeList.Codes.Working, task3.P9_Status);
			AssertEquals("task4", ProcessTaskStatusCodeList.Codes.Assigned, task4.P9_Status);
			AssertEquals("task5", ProcessTaskStatusCodeList.Codes.Assigned, task5.P9_Status);
		}

		#endregion

		[TestUtcOffset(10, 0, 0)]
		public void TestCreatedTimeAsText()
		{
			var project = Factory.New<Project>();
			project.WKP_SystemCreateTimeUtc = ZDateTime.Now;
			AssertDate("Local Created Time", project.WKP_SystemCreateTimeUtc.AddHours(10), project.CreatedDateAsText);
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestClosedDateAsText()
		{
			var project = Factory.New<Project>();
			project.WKP_ClosedDate = ZDateTime.Now;
			AssertDate("Local Closed Time", project.WKP_ClosedDate.AddHours(10), project.ClosedDateAsText);
		}

		[TestDate(2014, 3, 21)]
		public void TestStatus_ChangesClosedDate()
		{
			var project = Factory.New<Project>();
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Closed Status", ZDateTime.UtcNow, project.WKP_ClosedDate);

			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals("Assigned Status", ZDateTime.Empty, project.WKP_ClosedDate);

			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals("Cancelled Status", ZDateTime.UtcNow, project.WKP_ClosedDate);

			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Working Status", ZDateTime.Empty, project.WKP_ClosedDate);
		}

		[TestDate(2014, 03, 27)]
		public void TestClosedOrDeferredDate()
		{
			ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "WKP");

			ZDateTime deferredDate = ZDateTime.Now.AddDays(1);

			var project = Factory.New<Project>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(project, Factory);
			jobHeader.DoNotStartBeforeDateLocal = deferredDate;
			AssertDate("Precondition: deferred date is set", deferredDate, project.ClosedOrDeferredDateAsText);
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertDate("Deferred Date should be blank when setting status to close", project.WKP_ClosedDate, project.ClosedOrDeferredDateAsText);

			project.WKP_ClosedDate = ZDateTime.Empty;

			jobHeader.DoNotStartBeforeDateLocal = deferredDate;
			AssertDate("Precondition: deferred date is set", deferredDate, project.ClosedOrDeferredDateAsText);
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertDate("Deferred Date should be blank when setting status to cancelled", project.WKP_ClosedDate, project.ClosedOrDeferredDateAsText);
		}

		void AssertDate(string message, ZDateTime expectedDate, string actualDateAsText)
		{
			AssertEquals(message, expectedDate.ToString(DateTimeFormatStrings.ShortDateFormat), actualDateAsText);
		}

		[TestDate(2014, 3, 19, 23, 59, 59)]
		public void TestClosedOrDeferredLabel()
		{
			ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "WKP");

			var project = Factory.NewWithValidTestData<Project>();
			AssertEquals("ClosedOrDeferredLabel", "Project Closed", project.ClosedOrDeferredLabel);

			var jobHeader = ProcessJobHeaderProvider.GetForParent(project, Factory);
			jobHeader.DoNotStartBeforeDateLocal = ZDateTime.Now;
			AssertEquals("ClosedOrDeferredLabel", "Def. Date Met", project.ClosedOrDeferredLabel);

			jobHeader.DoNotStartBeforeDateLocal = ZDateTime.Now.AddDays(1);
			AssertEquals("ClosedOrDeferredLabel", "Deferred Until", project.ClosedOrDeferredLabel);
		}

		[TestDate(2014, 04, 01)]
		public void TestHasDeferred()
		{
			// See ProjectStatusControlTest.TestProjectClosedOrDeferredBox
			ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "WKP");

			var project = Factory.New<Project>();
			AssertEquals("No deferred date", false, project.HasDeferred);

			var jobHeader = ProcessJobHeaderProvider.GetForParent(project, Factory);
			jobHeader.DoNotStartBeforeDateLocal = new ZDateTime(2014, 3, 1);
			AssertEquals("Deferred date was last month", true, project.HasDeferred);
			jobHeader.DoNotStartBeforeDateLocal = ZDateTime.Now;
			AssertEquals("Deferred date is today", true, project.HasDeferred);
			jobHeader.DoNotStartBeforeDateLocal = ZDateTime.Now.AddDays(1);
			AssertEquals("Deferred date is tomorrow", true, project.HasDeferred);
		}

		[TestDate(2014, 04, 01)]
		public void TestDeferredDateMet()
		{
			// See ProjectStatusControlTest.TestProjectClosedOrDeferredBox
			ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "WKP");

			var project = Factory.New<Project>();
			AssertEquals("Precondition: no deferred date", false, project.DeferredDateMet);
			var jobHeader = ProcessJobHeaderProvider.GetForParent(project, Factory);
			jobHeader.DoNotStartBeforeDateLocal = new ZDateTime(2014, 3, 1);
			AssertEquals("Deferred date was last month", true, project.DeferredDateMet);
			jobHeader.DoNotStartBeforeDateLocal = ZDateTime.Now;
			AssertEquals("Deferred date is today", true, project.DeferredDateMet);
			jobHeader.DoNotStartBeforeDateLocal = ZDateTime.Now.AddDays(1);
			AssertEquals("Deferred date is tomorrow", false, project.DeferredDateMet);
		}

		public void TestNewProjectDoesNotCreateDefaultWorkflow()
		{
			ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "WKP");

			Project proj = Factory.New<Project>();
			AssertEquals("Newly created project job should not contain any workflows", 0, proj.JobWorkflow.ProcessHeaders.Count);
		}

		#region Client

		public void TestClientOrganisationPK()
		{
			Project project = Factory.New<Project>();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress orgBranchAddress = org.Addresses.AddNew();
			orgBranchAddress.OA_Address1 = "2nd St";
			orgBranchAddress.OA_City = "Brisbane";
			orgBranchAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgBranchAddress.OA_State = "Queensland";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Jenny";
			project.WKP_OA_ClientAddress = org.MainAddress.PK;
			project.WKP_OC_Contact = contact.PK;
			Factory.Save();
			AssertEquals(org.PK, project.ClientOrganisationPK);

			Project reloadedProject = new BusinessObjectFactory().Load<Project>(project.PK);
			AssertEquals(org.PK, reloadedProject.ClientOrganisationPK);

			reloadedProject.WKP_OA_ClientAddress = orgBranchAddress.PK;
			AssertEquals("Address is cleared but client is still the same so keep the contact", contact.PK, reloadedProject.WKP_OC_Contact);

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			reloadedProject.WKP_OA_ClientAddress = org2.MainAddress.PK;
			AssertEquals("Contact gets cleared when client is different", Guid.Empty, reloadedProject.WKP_OC_Contact);
		}

		public void TestClientDoesNotSetTechnicianOrg()
		{
			Project project = Factory.New<Project>();
			OrgHeader org = Factory.New<OrgHeader>();
			project.ClientOrganisationPK = org.PK;
			AssertEquals(Guid.Empty, project.TechnicianOrganisationPK);
		}

		public void TestClient_ShouldNotBeMandatory()
		{
			var project = Factory.New<Project>();
			project.WKP_Summary = "Two weeks is not enough weeks";

			project.RunPreSaveValidation();

			AssertNoErrors(project);
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		public void TestSettingBranchAddressSetsCorrectARSettlementGroupAddressToJob()
		{
			Project project = Factory.New<Project>();
			OrgHeader mainOrg = Factory.NewWithValidTestData<OrgHeader>();
			AssertNotNull("mainOrg.MainAddress", mainOrg.MainAddress);
			OrgAddress arAddress = mainOrg.Addresses.AddNew(OrgAddressType.Receivables, true);
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AssertNotNull("org.MainAddress", org.MainAddress);

			org.ARSettlementGroupPK = mainOrg.PK;
			Job.Loader loader = new Job.Loader(project);
			Job job = loader.Load();
			AssertNull("Precondition: Job", job);
			project.WKP_OA_ClientAddress = org.MainAddress.PK;
			job = loader.Load();
			AssertNull("Setting WKP_OA_ClientAddress does not create Job", job);

			loader.TryCreate();
			project.WKP_OA_ClientAddress = org.MainAddress.PK;
			job = loader.Load();
			AssertNotNull("Job", job);
			AssertEquals("Should be AR adress of the Settlement Group", arAddress.PK, job.JH_OA_LocalChargesAddr);
		}

		#region Custom Fields

		[TestedType(typeof(Project))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
		}

		#endregion

		#region Client / Technician Information

		public void TestContactPhone()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "blah blah lola lola";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "100 Fake St";
			org.MainAddress.OA_Phone = "33445566";
			OrgAddress branchAddress = org.Addresses.AddNew();
			branchAddress.OA_Phone = "919919";

			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Zubin Appoo";
			contact1.OC_Phone = "99112233";
			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "John K";
			contact2.OC_OA_OrgAddress = branchAddress.PK;

			var project = Factory.New<Project>();

			project.WKP_OA_ClientAddress = org.MainAddress.PK;
			project.WKP_OC_Contact = contact1.PK;
			AssertEquals("99112233", project.ContactPhone);

			contact1.OC_Phone = "";
			AssertEquals("33445566", project.ContactPhone);

			project.WKP_OC_Contact = contact2.PK;
			AssertEquals("919919", project.ContactPhone);
		}

		public void TestGetPhoneWithFallbackForDisplay()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Demo_Company";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Phone = "111111111";
			org.MainAddress.OA_Address1 = "100 Fake St";
			OrgAddress branchAddress = org.Addresses.AddNew();
			branchAddress.OA_Address1 = "123 Fake St";
			branchAddress.OA_Phone = "333333333";

			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "John Smith";
			contact1.OC_Phone = "222222222";

			var project = Factory.New<Project>();

			project.WKP_OA_ClientAddress = org.MainAddress.PK;
			project.WKP_OC_Contact = contact1.PK;
			AssertEquals("Dir: 222222222", project.ContactPhoneForDisplay);

			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Jane Doe";
			contact2.OC_OA_OrgAddress = branchAddress.PK;

			project.WKP_OC_Contact = contact2.PK;
			AssertEquals("Off: 333333333", project.ContactPhoneForDisplay);

			OrgContact contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "John K";

			project.WKP_OC_Contact = contact3.PK;
			AssertEquals("Off: 111111111", project.ContactPhoneForDisplay);
		}

		public void TestContactEmail()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "blah blah lola lola";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Email = "company@company.com";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Samuel";
			contact.OC_Email = "Samuel.Wang@cargowise.com";

			var project = Factory.New<Project>();
			AssertEquals("", project.ContactEmail);

			project.WKP_OC_Contact = contact.PK;
			AssertEquals("Samuel.Wang@cargowise.com", project.ContactEmail);

			contact.OC_Email = "";

			AssertEquals("company@company.com", project.ContactEmail);
		}

		public void TestTechnicianContactPhone()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Demo_Company";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Phone = "12234";
			org.MainAddress.OA_Address1 = "100 Fake St";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_Phone = "9876";

			var project = Factory.New<Project>();

			project.WKP_OC_TechnicalContact = contact.PK;
			AssertEquals("Dir: 9876", project.TechnicalContactPhoneForDisplay);

			contact.OC_Phone = "";
			AssertEquals("Off: 12234", project.TechnicalContactPhoneForDisplay);
		}

		#endregion

		#region Delete

		public void TestCanDeleteAndReasonMessage()
		{
			var project = ProcessMgmtTestHelper.CreateProject(Factory);
			Factory.Save();

			AssertEquals("Precondition: deleting is normally possible", true, project.CanDelete);

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = project.PK;
			job.JH_ParentTableCode = WorkProjectSchema.Constants.Prefix;
			job.JH_JobNum = "01189998819991197253";
			Factory.Save();

			AssertEquals(false, project.CanDelete);
			AssertEquals("Projects with Job Headers may not be deleted.", project.ReasonForNotAbleToDelete);
		}

		public void TestDelete_WithExternalEntityLinks_ShouldAlsoDeleteLinks()
		{
			var project1 = ProcessMgmtTestHelper.CreateProject(Factory);
			var linkable1 = new DummyExternalEntityLinkable("Squanch", "WKP");
			ExternalEntityLinkHelper.CreateLink(linkable1, project1, "SYS");

			var project2 = ProcessMgmtTestHelper.CreateProject(Factory);
			var linkable2 = new DummyExternalEntityLinkable("Shmloss", "WKP");
			ExternalEntityLinkHelper.CreateLink(linkable2, project2, "ABC");

			Factory.Save();
			var links = new BusinessObjectFactory().Load<ExternalEntityLink>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(new[] { "Squanch", "Shmloss" }, links.Select(x => x.EEL_ExternalCode));

			project1.Delete();
			Factory.Save();

			links = new BusinessObjectFactory().Load<ExternalEntityLink>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("The project's links should have been deleted along with the project. SAD!", new[] { "Shmloss" }, links.Select(x => x.EEL_ExternalCode));
		}

		#endregion

		public void TestWKP_GS_NKProjectManager_ReadOnly()
		{
			Env.Security.ProjectEditModifyStaffAssignment.IsAllowed = true;
			var project = Factory.NewWithValidTestData<Project>();
			AssertEquals(false, project.WKP_GS_NKProjectManager_ReadOnly);

			Env.Security.ProjectEditModifyStaffAssignment.IsAllowed = false;
			AssertEquals(false, project.WKP_GS_NKProjectManager_ReadOnly);

			Factory.Save();

			Env.Security.ProjectEditModifyStaffAssignment.IsAllowed = true;
			AssertEquals(false, project.WKP_GS_NKProjectManager_ReadOnly);

			Env.Security.ProjectEditModifyStaffAssignment.IsAllowed = false;
			AssertEquals(true, project.WKP_GS_NKProjectManager_ReadOnly);
		}

		public void TestNoCollectionModificationExceptionOnReOpen()
		{
			var project = Factory.NewWithValidTestData<Project>();
			var task = project.WorkflowItems.AddNew();
			project.Close(ProcessTaskStatusCodeList.Codes.Cancelled, ZString.Empty);
			task.P9_StatusInfo.ValueChanged += (s, e) =>
			{
				project.WorkflowItems.AddNew();
			};
			project.ReOpen(ZString.Empty);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
		}

		public void TestNoCollectionModificationExceptionOnClose()
		{
			var project = Factory.NewWithValidTestData<Project>();
			var task = project.WorkflowItems.AddNew();
			task.P9_StatusInfo.ValueChanged += (s, e) =>
			{
				project.WorkflowItems.AddNew();
			};
			project.Close(ProcessTaskStatusCodeList.Codes.Cancelled, ZString.Empty);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task.P9_Status);
		}

		public void TestHtmlProperty()
		{
			var project = Factory.New<Project>();
			AssertEquals(ZBlob.Empty, project.WKP_Details);
			AssertEquals(ZBlob.Empty, project.WKP_Details_HTML);

			project.WKP_Details_HTML = ZBlob.FromUTF8("<p>123</p>");

			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(project.WKP_Details.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", project.WKP_Details_HTML.ToUTF8());
		}

		public void TestHtmlFromTextProperty()
		{
			var project = Factory.New<Project>();
			AssertEquals(ZBlob.Empty, project.WKP_Details);
			AssertEquals(ZBlob.Empty, project.WKP_Details_HTML);

			project.WKP_Details = ZBlob.FromUTF8("1234\r\n5678");

			AssertEquals("<p>1234</p><p>5678</p>", project.WKP_Details_HTML.ToUTF8());

			project.WKP_Details = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");

			AssertEquals("<p>rtf</p>", project.WKP_Details_HTML.ToUTF8());
		}

		protected Project CachedProject
		{
			get { return (Project)CachedBusinessObject; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			// Do not create a BMSystem here to allow TestOnLoadedDoesNotChangePersistentValues to correctly pass
		}

		protected void AssertContainsOnlyOneMatch(string expected, string stringToSearchIn)
		{
			int firstMatch = stringToSearchIn.IndexOf(expected);
			int lastMatch = stringToSearchIn.LastIndexOf(expected);
			Assert("No expected part in message was found", firstMatch != -1);
			Assert("The expected part of the message should appear only once", firstMatch == lastMatch);
		}

		protected override Type ExpectedMetadataType => typeof(Metadata.Business.Project);
	}

	[TestedType(typeof(Project))]
	public class ProjectWorkflowProviderTest : WorkflowProviderTest<Project, ProjectProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return JobInvoicingConsumerTypes.Project.Code; }
		}
	}

	public abstract class ProjectRelatedItemTestCase : WorkTaskRelatedItemTestCase
	{
		protected override string ExpectedSelectionCriterion1 => "1AA - 1AA depth 1";
		protected override string ExpectedSelectionCriterion2 => "2AA - 2AA depth 2";
		protected override string ExpectedSelectionCriterion3 => "3AA - 3AA depth 3";
		protected override string ExpectedSelectionCriterion4 => string.Empty;
		protected override string ExpectedSelectionCriterion5 => string.Empty;

		protected override IWorkTaskRelatedItem GetItemForSelectionCriteriaTest()
		{
			var project = (Project)Factory.New(TestedTypeHelper.GetTestedType(GetType()));

			project.WKP_Type = "1AA";
			project.WKP_SubType = "2AA";
			project.WKP_Module = "3AA";

			return project;
		}

		protected override Type ExpectedPivotCollectionType => typeof(GenPivotCollection);

		protected override void SetUp()
		{
			base.SetUp();

			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.ProjectTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);
		}
	}

	[TestedType(typeof(Project))]
	sealed class ProjectRelatedItemTest : ProjectRelatedItemTestCase
	{
	}

	public abstract class ProjectRelatedItemSourceTestCase : WorkTaskRelatedItemSourceTestCase
	{
	}

	[TestedType(typeof(Project))]
	sealed class ProjectRelatedItemSourceTest : ProjectRelatedItemSourceTestCase
	{
	}
}
