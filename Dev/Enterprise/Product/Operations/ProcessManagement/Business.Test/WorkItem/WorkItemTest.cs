using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkItem))]
	public class WorkItemTest : WorkItemCommonTest<WorkItem>
	{
		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var workItem1 = Factory.New<WorkItem>();
			AssertEquals("Work Item", workItem1.HumanReadableName);
			Factory.Save();
			AssertEquals("WI00000001", workItem1.HumanReadableName);
			var workItem2 = Factory.NewWithValidTestData<WorkItem>();
			Factory.Save();
			AssertEquals("WI00000002 - Summary for test", workItem2.HumanReadableName);
		}

		#endregion

		#region TestAssignedTo

		public void TestAssignedTo()
		{
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "XYZ";

			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var task1 = workItem.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			var task2 = workItem.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = staff2.GS_Code;

			AssertEquals("AssignedToStaff", GlbStaff.CurrentUser.PK, workItem.AssignedToStaff.PK);
			AssertEquals("AssignedToCode", GlbStaff.CurrentUser.GS_Code, workItem.AssignedToCode);
			AssertEquals("AssignedStaffCode", GlbStaff.CurrentUser.GS_Code, workItem.AssignedStaffCode);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("AssignedToStaff", staff2.PK, workItem.AssignedToStaff.PK);
			AssertEquals("AssignedToCode", staff2.GS_Code, workItem.AssignedToCode);
			AssertEquals("AssignedStaffCode", staff2.GS_Code, workItem.AssignedStaffCode);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("AssignedToStaff", staff2.PK, workItem.AssignedToStaff.PK);
			AssertEquals("AssignedToCode", staff2.GS_Code, workItem.AssignedToCode);
			AssertEquals("AssignedStaffCode", staff2.GS_Code, workItem.AssignedStaffCode);

			var task3 = workItem.WorkflowItems.AddNew();
			task3.P9_GS_NKAssignedStaffMember = "";
			var task4 = workItem.WorkflowItems.AddNew();
			task4.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			AssertNull("AssignedToStaff", workItem.AssignedToStaff);
			AssertNullOrEmpty("AssignedToCode", workItem.AssignedToCode);
			AssertNullOrEmpty("AssignedStaffCode", workItem.AssignedStaffCode);

			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var task5 = workItem.WorkflowItems.AddNew(); // Assigned staff is automatically set to task4's assigned staff on creation
			AssertEquals("AssignedToStaff", GlbStaff.CurrentUser.PK, workItem.AssignedToStaff.PK);
			AssertEquals("AssignedToCode", GlbStaff.CurrentUser.GS_Code, workItem.AssignedToCode);
			AssertEquals("AssignedStaffCode", GlbStaff.CurrentUser.GS_Code, workItem.AssignedStaffCode);
		}

		#endregion

		#region TestCurrentTaskProperties

		public void TestCurrentTaskProperties()
		{
			GlbGroup testGroup = Factory.New<GlbGroup>();
			testGroup.GG_Code = "XYZ";

			var workItem = Factory.NewWithValidTestData<WorkItem>();
			AssertEquals("", workItem.CurrentTaskAssignedToCode);
			AssertEquals(ZGuid.Empty, workItem.CurrentTaskAssignedToGroupPK);
			AssertNull(workItem.CurrentTaskAssignedTo);
			AssertNull(workItem.CurrentTaskAssignedToGroup);

			var task1 = workItem.WorkflowItems.AddNew();
			task1.P9_Description = "Task 1";

			var task2 = workItem.WorkflowItems.AddNew();
			task2.P9_TaskID = "T0000100";
			task2.P9_Description = "Task 2";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GG_AssignedGroup = testGroup.PK;

			var task3 = workItem.WorkflowItems.AddNew();
			task3.P9_Description = "Task 3";
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			AssertEquals(GlbStaff.CurrentUser.PK, workItem.CurrentTaskAssignedTo.PK);
			AssertEquals(testGroup.PK, workItem.CurrentTaskAssignedToGroupPK);
			AssertEquals(GlbStaff.CurrentUser.PK, workItem.CurrentTaskAssignedTo.PK);
			AssertEquals(testGroup, workItem.CurrentTaskAssignedToGroup);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
		}

		public void TestCurrentStartableTask()
		{
			var workitem = Factory.NewWithValidTestData<WorkItem>();
			CurrentTaskTestHelper.AssertCurrentStartableTask(workitem, () => workitem.CurrentTask);
		}

		public void TestCurrentOrNextStartableTask()
		{
			var workitem = Factory.NewWithValidTestData<WorkItem>();
			CurrentTaskTestHelper.AssertCurrentOrNextStartableTask(workitem, () => workitem.CurrentOrNextTask);
		}

		#endregion

		#region TestCurrentTaskStatus

		public void TestCurrentTaskStatus()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			AssertEquals(ZString.Empty, workItem.CurrentTaskStatus);

			var task1 = workItem.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			var task2 = workItem.WorkflowItems.AddNew();
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var task3 = workItem.WorkflowItems.AddNew();
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, workItem.CurrentTaskStatus);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem.CurrentTaskStatus);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, workItem.CurrentTaskStatus);
		}

		#endregion

		#region TestOperationalActionFields

		public void TestOperationalActionFields()
		{
			var properties = typeof(WorkItem).GetProperties(BindingFlags.Public | BindingFlags.Instance);
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
				AutoWorkItem.Schema.WKI_Summary,
				AutoWorkItem.Schema.WKI_WorkItemType,
				AutoWorkItem.Schema.WKI_WorkItemArea,
				AutoWorkItem.Schema.WKI_ActivityType,
				AutoWorkItem.Schema.WKI_ActivitySubtype,
				AutoWorkItem.Schema.WKI_SystemCreateTimeUtc,
				AutoWorkItem.Schema.WKI_SystemCreateUser,
				AutoWorkItem.Schema.WKI_SystemCreateBranch,
				AutoWorkItem.Schema.WKI_SystemCreateDepartment,
				AutoWorkItem.Schema.WKI_SystemLastEditTimeUtc,
				AutoWorkItem.Schema.WKI_SystemLastEditUser,
				AutoWorkItem.Schema.WKI_Priority,
				AutoWorkItem.Schema.WKI_PortOrCountry,
				AutoWorkItem.Schema.WKI_Status,
				AutoWorkItem.Schema.WKI_Risk,
				AutoWorkItem.Schema.WKI_DateOfChange,
				AutoWorkItem.Schema.WKI_GB_AssignedBranch,
				AutoWorkItem.Schema.WKI_GC_AssignedCompany,
				AutoWorkItem.Schema.WKI_GE_AssignedDepartment,
				AutoWorkItem.Schema.WKI_P9_DefectCausedByTask,
				AutoWorkItem.Schema.WKI_P9_DefectFirstMissedInTask,
				"Workflows",
				"WorkflowItems",
				"Conversation",
			};

			string[] expectedReadonlyNames =
			{
				AutoWorkItem.Schema.WKI_WorkItemNumber,
				"InstantiationTime",
				"IsClosed",
				"IsCancelled",
				"IsClosedOrCancelled",
				"JobCreatedDate",
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

		#endregion

		#region TestWorkItemType

		public void TestWorkItemType()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree5();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.New<WorkItem>();
			workItem.WKI_WorkItemType = "1AA";
			workItem.WKI_WorkItemArea = "2AA";
			workItem.WKI_ActivityType = "3AA";
			workItem.WKI_ActivitySubtype = "4AA";
			workItem.WKI_Priority = "LOW";
			AssertNoNotifications(workItem);

			workItem.WKI_WorkItemType = "1BB";
			workItem.Validation.ValidateWKI_WorkItemArea();
			workItem.Validation.ValidateWKI_ActivityType();
			workItem.Validation.ValidateWKI_ActivitySubtype();
			workItem.Validation.ValidateWKI_Priority();
			CombineAssertions(() =>
			{
				AssertNoNotifications(workItem);
				AssertEquals("", workItem.WKI_WorkItemArea);
				AssertEquals("", workItem.WKI_ActivityType);
				AssertEquals("", workItem.WKI_ActivitySubtype);
				AssertEquals("LOW", workItem.WKI_Priority);
			});
		}

		#endregion

		#region TestWorkItemArea

		public void TestWorkItemArea()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree5();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.New<WorkItem>();
			workItem.WKI_WorkItemType = "1AA";
			workItem.WKI_WorkItemArea = "2AA";
			workItem.WKI_ActivityType = "3AA";
			workItem.WKI_ActivitySubtype = "4XX";
			workItem.WKI_Priority = "LOW";
			AssertNoNotifications(workItem);

			workItem.WKI_WorkItemArea = "2CC";
			workItem.Validation.ValidateWKI_ActivityType();
			workItem.Validation.ValidateWKI_ActivitySubtype();
			workItem.Validation.ValidateWKI_Priority();
			CombineAssertions(() =>
			{
				AssertNoNotifications(workItem);
				AssertEquals("1AA", workItem.WKI_WorkItemType);
				AssertEquals("2CC", workItem.WKI_WorkItemArea);
				AssertEquals("", workItem.WKI_ActivityType);
				AssertEquals("4XX", workItem.WKI_ActivitySubtype);
				AssertEquals("LOW", workItem.WKI_Priority);
			});
		}

		#endregion

		#region TestActivityType

		public void TestActivityType()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree5();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.New<WorkItem>();
			workItem.WKI_WorkItemType = "1AA";
			workItem.WKI_WorkItemArea = "2AA";
			workItem.WKI_ActivityType = "3AA";
			workItem.WKI_ActivitySubtype = "4AA";
			workItem.WKI_Priority = "LOW";
			AssertNoNotifications(workItem);

			workItem.WKI_ActivityType = "3S2";
			workItem.Validation.ValidateWKI_ActivitySubtype();
			workItem.Validation.ValidateWKI_Priority();
			CombineAssertions(() =>
			{
				AssertNoNotifications(workItem);
				AssertEquals("1AA", workItem.WKI_WorkItemType);
				AssertEquals("2AA", workItem.WKI_WorkItemArea);
				AssertEquals("3S2", workItem.WKI_ActivityType);
				AssertEquals("", workItem.WKI_ActivitySubtype);
				AssertEquals("LOW", workItem.WKI_Priority);
			});
		}

		#endregion

		#region TestActivitySubtype

		public void TestActivitySubtype()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree5();
			CodeDescriptionBoolTreeTestHelper.Add(tree, "1AA", "2AA", "3AA", "4AA", "TOP");
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.New<WorkItem>();
			workItem.WKI_WorkItemType = "1AA";
			workItem.WKI_WorkItemArea = "2AA";
			workItem.WKI_ActivityType = "3AA";
			workItem.WKI_ActivitySubtype = "4AA";
			workItem.WKI_Priority = "TOP";
			AssertNoNotifications(workItem);

			workItem.WKI_ActivitySubtype = "4XX";
			workItem.Validation.ValidateWKI_Priority();
			CombineAssertions(() =>
			{
				AssertNoNotifications(workItem);
				AssertEquals("1AA", workItem.WKI_WorkItemType);
				AssertEquals("2AA", workItem.WKI_WorkItemArea);
				AssertEquals("3AA", workItem.WKI_ActivityType);
				AssertEquals("4XX", workItem.WKI_ActivitySubtype);
				AssertEquals("", workItem.WKI_Priority);
			});
		}

		#endregion

		#region TestWorkItemTypeDescription

		public void TestWorkItemTypeDescription()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.New<WorkItem>();
			AssertEquals("", workItem.WorkItemTypeDescription);

			workItem.WKI_WorkItemType = "1AA";
			AssertEquals("1AA depth 1", workItem.WorkItemTypeDescription);

			workItem.WKI_WorkItemType = "1ZZ";
			AssertEquals("1ZZ depth 1", workItem.WorkItemTypeDescription);
		}

		#endregion

		#region TestAreaDescription

		public void TestAreaDescription()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.New<WorkItem>();
			AssertEquals("", workItem.AreaDescription);

			workItem.WKI_WorkItemArea = "2AA";
			AssertEquals("2AA depth 2", workItem.AreaDescription);

			workItem.WKI_WorkItemArea = "2ZZ";
			AssertEquals("2ZZ depth 2", workItem.AreaDescription);
		}

		#endregion

		#region TestActivityTypeDescription

		public void TestActivityTypeDescription()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.New<WorkItem>();
			AssertEquals("", workItem.ActivityTypeDescription);

			workItem.WKI_ActivityType = "3AA";
			AssertEquals("3AA depth 3", workItem.ActivityTypeDescription);

			workItem.WKI_ActivityType = "3ZZ";
			AssertEquals("3ZZ depth 3", workItem.ActivityTypeDescription);
		}

		#endregion

		#region TestActivitySubtypeDescription

		public void TestActivitySubtypeDescription()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.New<WorkItem>();
			AssertEquals("", workItem.ActivitySubtypeDescription);

			workItem.WKI_ActivitySubtype = "4AA";
			AssertEquals("4AA depth 4", workItem.ActivitySubtypeDescription);

			workItem.WKI_ActivitySubtype = "4ZZ";
			AssertEquals("4ZZ depth 4", workItem.ActivitySubtypeDescription);
		}

		#endregion

		#region TestCurrentOrNextTask

		public void TestCurrentOrNextTask()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();

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

			workItem.WorkflowItems.AddRange(task1, task2, task3, task4, task5);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNull(workItem.CurrentOrNextTask);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(task2, workItem.CurrentOrNextTask);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(task4, workItem.CurrentOrNextTask);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertEquals(task1, workItem.CurrentOrNextTask);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(task2, workItem.CurrentOrNextTask);

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
			AssertEquals(task3, workItem.CurrentOrNextTask);

			task1.P9_Sequence = 1;
			task2.P9_Sequence = 1;
			task3.P9_Sequence = 1;
			task4.P9_Sequence = 1;
			task5.P9_Sequence = 1;
			AssertEquals(task2, workItem.CurrentOrNextTask);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task3.P9_Sequence = 2;
			task4.P9_Sequence = 2;
			task5.P9_Sequence = 2;
			AssertEquals(task4, workItem.CurrentOrNextTask);
		}

		#endregion

		#region Universal Triggers

		public void TestUniversalTriggersEnumerateCorrectly()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = new WorkItemWorkflowDescriptor().Code;
			template.P0_IsUniversal = true;
			template.P0_TriggerFallbackMethod = "AFB";
			var item = (ITemplateTrigger)template.TemplateTriggers.AddNew();
			item.Description = "Where will we go";
			item.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			var triggerAction = (ProcessTaskNotification)item.TriggerActions.AddNew();
			triggerAction.PQ_TriggerType = "NTF";
			triggerAction.PQ_TriggerParty = "EML";
			triggerAction.PQ_EmailAddr = "japan@Japan.JAPAAAAAAAN";

			Factory.Save();

			var workItem = Factory.NewWithValidTestData<WorkItem>();
			AssertEquals(1, workItem.WorkflowItems.TriggersIncludingRelated.Count);
			var trigger = workItem.WorkflowItems.TriggersIncludingRelated.AddNew();
			trigger.P9_Description = "Nipon";

			Factory.Save();
			AssertEquals(2, workItem.WorkflowItems.TriggersIncludingRelated.Count);
		}

		#endregion

		#region TestCurrentOrNextTaskAssignedToCodeAndName

		public void TestCurrentOrNextTaskAssignedToCodeAndName()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ZZZ";
			staff.GS_FullName = "Zaphod B";

			var workItem = Factory.New<WorkItem>();
			var task = workItem.WorkflowItems.Tasks.AddNew();
			task.P9_GS_NKAssignedStaffMember = "ZZZ";

			AssertEquals("ZZZ  Zaphod B", workItem.CurrentOrNextTaskAssignedToCodeAndName);
		}

		#endregion

		#region TestOverallTaskStatusCode

		public void TestOverallTaskStatusCode()
		{
			var workItem = Factory.New<WorkItem>();
			AssertEquals("", workItem.OverallTaskStatusCode);

			var task1 = workItem.WorkflowItems.Tasks.AddNew();
			var task2 = workItem.WorkflowItems.Tasks.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItem.OverallTaskStatusCode);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItem.OverallTaskStatusCode);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, workItem.OverallTaskStatusCode);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, workItem.OverallTaskStatusCode);
		}

		#endregion

		#region TestOverallTaskStatusCodeAndDescription

		public void TestOverallTaskStatusCodeAndDescription()
		{
			var workItem = Factory.New<WorkItem>();
			AssertEquals("", workItem.OverallTaskStatusCodeAndDescription);

			var task1 = workItem.WorkflowItems.Tasks.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed + "  " + ProcessTaskStatusCodeList.Descriptions.Closed, workItem.OverallTaskStatusCodeAndDescription);
		}

		#endregion

		#region TestCancel

		public void TestCancel()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var task1 = workItem.WorkflowItems.AddNew();
			var task2 = workItem.WorkflowItems.AddNew();
			var milestone = workItem.WorkflowItems.Milestones.AddNew();

			workItem.Cancel();
			AssertEquals("cancelled status", ProcessTaskStatusCodeList.Codes.Cancelled, workItem.WKI_Status);
			AssertNotEquals("Non tasks should not be affected", ProcessTaskStatusCodeList.Codes.Cancelled, milestone.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);

			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
			workItem.Cancel();
			AssertEquals("cancelled status", ProcessTaskStatusCodeList.Codes.Cancelled, workItem.WKI_Status);

			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			workItem.Cancel();
			AssertEquals("cancelled status", ProcessTaskStatusCodeList.Codes.Cancelled, workItem.WKI_Status);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Working;
			task1.P9_StatusInfo.ValueChanged += (sender, e) => { workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Suspended; };
			workItem.Cancel();
			AssertEquals("WI status is cancelled even if changing the task status has an event handler that sets WI status to Working", ProcessTaskStatusCodeList.Codes.Cancelled, workItem.WKI_Status);
			AssertEquals("Working task should be set to Closed when the WI is cancelled. This way any time recording is captured.", ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
		}

		public void TestCancel_ShouldIgnoreTaskCancellationValidationAndCancelAllTasks()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var task = workItem.WorkflowItems.Tasks.AddNew();
			task.P9_GS_NKAssignedStaffMember = string.Empty;
			task.P9_Type = "UDF";

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypes = categorisedTaskTypes.AddNew();
			taskTypes.Code = "WKI";
			var taskType = taskTypes.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.CanCancelTask = false;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
			Env.Security.WorkflowTasksCanCancelAllTasks.IsAllowed = false;

			Factory.Save();

			workItem.Cancel();
			task.Validation.ValidateP9_Status();

			AssertNoErrors("Validation on task status should be disabled when cancelling a work item", task.P9_StatusInfo);
		}

		#endregion

		#region Delete

		public void TestCanDeleteAndReasonMessage()
		{
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			Factory.Save();

			AssertEquals("Precondition: deleting is normally possible", true, workItem.CanDelete);

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = workItem.PK;
			job.JH_ParentTableCode = WorkItemSchema.Constants.Prefix;
			job.JH_JobNum = "01189998819991197253";
			Factory.Save();

			AssertEquals(false, workItem.CanDelete);
			AssertEquals("Work Items with Job Headers may not be deleted.", workItem.ReasonForNotAbleToDelete);
		}

		public void TestDelete_WithTasks_ShouldNotReportErrors()
		{
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var task = workItem.WorkflowItems.Tasks.AddNew();
			Factory.Save();
			workItem.Delete();
			Factory.Save();

			AssertEquals(true, workItem.IsDeleted);
			AssertEquals(true, task.IsDeleted);
			AssertEquals("Deleting a work item with tasks should not report errors related to accessing data on a deleted row. SAD!", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestDelete_WithTasks_WithWorkItemInMultipleFactories_ShouldNotReportErrors()
		{
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var task = workItem.WorkflowItems.Tasks.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedWorkItem = newFactory.Load<WorkItem>(workItem.PK);
			loadedWorkItem.Delete();
			newFactory.Save();

			AssertEquals(true, workItem.IsDeleted);
			AssertEquals(true, task.IsDeleted);
			AssertEquals("Deleting a work item with tasks should not report errors related to accessing data on a deleted row. SAD!", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestDelete_WithExternalEntityLinks_ShouldAlsoDeleteLinks()
		{
			var workItem1 = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var linkable1 = new DummyExternalEntityLinkable("Squanch", "WKI");
			ExternalEntityLinkHelper.CreateLink(linkable1, workItem1, "SYS");

			var workItem2 = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var linkable2 = new DummyExternalEntityLinkable("Shmloss", "WKI");
			ExternalEntityLinkHelper.CreateLink(linkable2, workItem2, "ABC");

			Factory.Save();
			var links = new BusinessObjectFactory().Load<ExternalEntityLink>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(new[] { "Squanch", "Shmloss" }, links.Select(x => x.EEL_ExternalCode));

			workItem1.Delete();
			Factory.Save();

			links = new BusinessObjectFactory().Load<ExternalEntityLink>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("The work item's links should have been deleted along with the work item. SAD!", new[] { "Shmloss" }, links.Select(x => x.EEL_ExternalCode));
		}

		#endregion

		[TestUtcOffset(0, 0, 0)]
		public void TestReleaseSequence_ShouldReturnSequenceValues_WhenReleaseSequenceModuleEnabled()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			helper.CreateSystem(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = true;

			var workitem = Factory.NewWithValidTestData<WorkItem>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(workitem, Factory, true);
			jobHeader.FH_AgreedDeliveryDate = new ZDateTime(2021, 11, 23);

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var sequence = helper.CreateReleaseSequence(Factory, group.PK.ToGuid(), "Name of sequence");
			helper.CreateReleaseSequenceItem(sequence, jobHeader, position: 1, value: 2, investment: 3);

			Factory.Save();

			AssertEquals("Name of sequence", workitem.ReleaseSequenceName);
			AssertEquals(1, workitem.ReleaseSequencePosition);
			AssertEquals(2, workitem.ReleaseSequenceValue);
			AssertEquals(3, workitem.ReleaseSequenceInvestment);
			AssertEquals(new ZDateTime(2021, 11, 23, 0, 0, 0), workitem.ReleaseSequenceDate);
			AssertEquals("23-Nov-21 00:00", workitem.ReleaseSequenceDateAsText);
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestReleaseSequence_ShouldReturnEmptyValues_WhenReleaseSequenceModuleDisabled()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			helper.CreateSystem(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = false;

			var workitem = Factory.NewWithValidTestData<WorkItem>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(workitem, Factory, true);
			jobHeader.FH_AgreedDeliveryDate = new ZDateTime(2021, 11, 23, 0, 0, 0);

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var sequence = helper.CreateReleaseSequence(Factory, group.PK.ToGuid(), "Name of sequence");
			helper.CreateReleaseSequenceItem(sequence, jobHeader, position: 1, value: 2, investment: 3);

			Factory.Save();

			AssertEquals(ZString.Empty, workitem.ReleaseSequenceName);
			AssertEquals(0, workitem.ReleaseSequencePosition);
			AssertEquals(0, workitem.ReleaseSequenceValue);
			AssertEquals(0, workitem.ReleaseSequenceInvestment);
			AssertEquals(ZDateTime.Empty, workitem.ReleaseSequenceDate);
			AssertEquals(ZString.Empty, workitem.ReleaseSequenceDateAsText);
		}

		public void TestWKI_SystemCreateUser_ReadOnly()
		{
			Env.Security.WorkItemEditModifyStaffAssignment.IsAllowed = true;
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			AssertEquals(false, workItem.WKI_SystemCreateUser_ReadOnly);

			Env.Security.WorkItemEditModifyStaffAssignment.IsAllowed = false;
			AssertEquals(false, workItem.WKI_SystemCreateUser_ReadOnly);

			Factory.Save();

			Env.Security.WorkItemEditModifyStaffAssignment.IsAllowed = true;
			AssertEquals(false, workItem.WKI_SystemCreateUser_ReadOnly);

			Env.Security.WorkItemEditModifyStaffAssignment.IsAllowed = false;
			AssertEquals(true, workItem.WKI_SystemCreateUser_ReadOnly);
		}

		#region Custom Fields

		[TestedType(typeof(WorkItem))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
		}

		#endregion

		#region TestDocManagerInfo

		public void TestDocManagerInfo()
		{
			var workItem = Factory.New<WorkItem>();

			Assert(workItem.DocManagerInfo is WorkItemDocManagerInfo);
		}

		#endregion

		#region Defect Cause Identification

		public void TestDefectCausedByTask_Detach()
		{
			WorkItem fixItem = Factory.NewWithValidTestData<WorkItem>();
			WorkItem causedItem = Factory.NewWithValidTestData<WorkItem>();
			WorkItemProcessTask causedTask = causedItem.WorkflowItems.AddNew();
			fixItem.DefectCausedByWorkItemPK = causedItem.PK;
			fixItem.WKI_P9_DefectCausedByTask = causedTask.PK;
			Factory.Save();

			BusinessObjectFactory newfactory = new BusinessObjectFactory();
			WorkItem loadFixItem = newfactory.Load<WorkItem>(fixItem.PK);
			AssertEquals("WI of task that caused the defect", causedItem.PK, loadFixItem.DefectCausedByWorkItemPK);
			AssertEquals("Task that caused the defect", causedTask.PK, loadFixItem.WKI_P9_DefectCausedByTask);

			fixItem.WKI_P9_DefectCausedByTask = Guid.Empty;
			Factory.Save();

			newfactory = new BusinessObjectFactory();
			loadFixItem = newfactory.Load<WorkItem>(fixItem.PK);
			AssertEquals("No WI due to no task that caused the defect", Guid.Empty, loadFixItem.DefectCausedByWorkItemPK);
			AssertEquals("No task caused the defect", Guid.Empty, loadFixItem.WKI_P9_DefectCausedByTask);
		}

		public void TestDefectCausedByTask_MultipleFixWorkItems()
		{
			WorkItem causedItem = Factory.NewWithValidTestData<WorkItem>();
			WorkItemProcessTask causedTask = causedItem.WorkflowItems.AddNew();
			WorkItem fixItem1 = Factory.NewWithValidTestData<WorkItem>();
			fixItem1.DefectCausedByWorkItemPK = causedItem.PK;
			fixItem1.WKI_P9_DefectCausedByTask = causedTask.PK;
			WorkItem fixItem2 = Factory.NewWithValidTestData<WorkItem>();
			fixItem2.DefectCausedByWorkItemPK = causedItem.PK;
			fixItem2.WKI_P9_DefectCausedByTask = causedTask.PK;
			Factory.Save();

			BusinessObjectFactory newfactory = new BusinessObjectFactory();
			WorkItem loadFixItem1 = newfactory.Load<WorkItem>(fixItem1.PK);
			WorkItem loadFixItem2 = newfactory.Load<WorkItem>(fixItem2.PK);
			newfactory.Save();

			newfactory = new BusinessObjectFactory();
			loadFixItem1 = newfactory.Load<WorkItem>(fixItem1.PK);
			loadFixItem2 = newfactory.Load<WorkItem>(fixItem2.PK);
			AssertEquals("WI of task of first fix WI", causedItem.PK, loadFixItem1.DefectCausedByWorkItemPK);
			AssertEquals("Task of first fix WI", causedTask.PK, loadFixItem1.WKI_P9_DefectCausedByTask);
			AssertEquals("WI of task of first fix WI", causedItem.PK, loadFixItem2.DefectCausedByWorkItemPK);
			AssertEquals("Task of second fix WI", causedTask.PK, loadFixItem2.WKI_P9_DefectCausedByTask);
		}

		public void TestHasIdentifyDefectCauseTaskCancelled()
		{
			SetupRegistryForDefectTesting();

			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var task1 = workItem.WorkflowItems.AddNew();
			task1.P9_Type = "COD";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Assert(!workItem.HasIdentifyDefectCauseTaskCancelled);

			var task2 = workItem.WorkflowItems.AddNew();
			task2.P9_Type = "DDD";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Assert(!workItem.HasIdentifyDefectCauseTaskCancelled);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Assert(workItem.HasIdentifyDefectCauseTaskCancelled);
		}

		public static void SetupRegistryForDefectTesting(params string[] additionalTaskTypes)
		{
			const string identifyDefectCauseTaskType = "DDD";

			ProcessManagementRegistry.Instance.DefectWorkItemTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "ZZZ", "AMN" });

			var categorisedTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.Value;
			var taskTypeCategory = categorisedTaskTypes.Cast<CategorisedWorkflowTaskTypes>().SingleOrDefault(c => c.Code == WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			if (taskTypeCategory == null)
			{
				taskTypeCategory = categorisedTaskTypes.AddNew();
				taskTypeCategory.Code = WorkflowDescriptors.WorkItemWorkflowDescriptorCode;
			}

			foreach (var type in additionalTaskTypes.Append(new[] { identifyDefectCauseTaskType, "CHK", "CH0" }))
			{
				var taskType = taskTypeCategory.TaskTypes.Cast<WorkflowTaskType>().SingleOrDefault(x => x.Code == type);

				if (taskType == null)
				{
					taskType = taskTypeCategory.TaskTypes.AddNew();
					taskType.Code = type;
				}
			}

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			ProcessManagementRegistry.Instance.IdentifyDefectCauseTaskType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, identifyDefectCauseTaskType);
			ProcessManagementRegistry.Instance.DefectIntroducedTaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "CH0" });
		}

		public void TestWKI_P9_DefectFirstMissedInTask()
		{
			ProcessMgmtTestHelper.EnableBufferManagement();
			ProcessManagementRegistry.Instance.DefectWorkItemTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "FIX", "ISS", "AMN" });

			var collection = new CategorisedWorkflowTaskTypesCollection();
			var category = collection.AddNew();
			category.Code = "WKI";
			category.Description = (NoResString)"Work Item";

			var taskType1 = category.TaskTypes.AddNew();
			taskType1.Code = "RRR";
			taskType1.Description = (NoResString)"Review";
			taskType1.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.GLB;
			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var causedItem = Factory.NewWithValidTestData<WorkItem>();
			var causedTask = causedItem.WorkflowItems.AddNew();
			causedTask.P9_Type = "RRR";

			var fixItem = Factory.NewWithValidTestData<WorkItem>();
			fixItem.WKI_ActivitySubtype = "ISS";
			fixItem.DefectCausedByWorkItemPK = causedItem.PK;

			fixItem.WKI_P9_DefectFirstMissedInTask = causedTask.PK;
			Assert(!fixItem.WKI_P9_DefectFirstMissedInTask.IsEmpty);
			Factory.Save();

			var newfactory = new BusinessObjectFactory();
			var fixItemReloaded = newfactory.Load<WorkItem>(fixItem.PK);
			AssertEquals("WKI_P9_DefectFirstMissedInTask should be saved", causedTask.PK, fixItemReloaded.WKI_P9_DefectFirstMissedInTask);

			fixItem.WKI_ActivitySubtype = "CLI";
			Factory.Save();

			Assert(fixItem.WKI_P9_DefectFirstMissedInTask.IsEmpty);
		}

		public void TestWKI_P9_DefectFirstMissedInTaskReadonly()
		{
			var causedItem = Factory.NewWithValidTestData<WorkItem>();
			var fixItem = Factory.NewWithValidTestData<WorkItem>();
			Assert("WKI_P9_DefectFirstMissedInTask should be readonly if DefectCausedByWorkItemPK is not entered", fixItem.WKI_P9_DefectFirstMissedInTask_ReadOnly);

			fixItem.DefectCausedByWorkItemPK = causedItem.PK;
			Assert(!fixItem.WKI_P9_DefectFirstMissedInTask_ReadOnly);
		}

		public void TestDefectCausedByTask_ReAttach()
		{
			var fixItem = Factory.NewWithValidTestData<WorkItem>();
			var causedItem1 = Factory.NewWithValidTestData<WorkItem>();
			WorkItemProcessTask causedTask1 = causedItem1.WorkflowItems.AddNew();
			fixItem.DefectCausedByWorkItemPK = causedItem1.PK;
			fixItem.WKI_P9_DefectCausedByTask = causedTask1.PK;
			Factory.Save();

			var newfactory = new BusinessObjectFactory();
			var loadFixItem = newfactory.Load<WorkItem>(fixItem.PK);
			AssertEquals("WI of task that caused the defect", causedItem1.PK, loadFixItem.DefectCausedByWorkItemPK);
			AssertEquals("Task that caused the defect", causedTask1.PK, loadFixItem.WKI_P9_DefectCausedByTask);

			var causedItem2 = Factory.NewWithValidTestData<WorkItem>();
			var causedTask2 = causedItem2.WorkflowItems.AddNew();
			fixItem.WKI_P9_DefectCausedByTask = causedTask2.PK;
			Factory.Save();

			newfactory = new BusinessObjectFactory();
			loadFixItem = newfactory.Load<WorkItem>(fixItem.PK);
			AssertEquals("WI of different task that caused the defect", causedItem2.PK, loadFixItem.DefectCausedByWorkItemPK);
			AssertEquals("Different task that caused the defect", causedTask2.PK, loadFixItem.WKI_P9_DefectCausedByTask);
		}

		public void TestDefectCausedByTaskIsReadOnly()
		{
			ProcessManagementRegistry.Instance.DefectWorkItemTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "FIX", "ISS", "AMN" });

			var causedItem = Factory.NewWithValidTestData<WorkItem>();
			var causedTask = causedItem.WorkflowItems.AddNew();
			causedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var item = Factory.NewWithValidTestData<WorkItem>();

			AssertEquals("WI is not an issue/defect fix so the defect properties are read-only", true, item.DefectCausedByWorkItemPK_ReadOnly);
			AssertEquals("WI is not an issue/defect fix so the defect properties are read-only", true, item.WKI_P9_DefectCausedByTask_ReadOnly);

			item.WKI_ActivitySubtype = "ISS";
			AssertEquals("WI is an issue fix so some defect properties can be entered", false, item.DefectCausedByWorkItemPK_ReadOnly);
			AssertEquals("WI is an issue fix - only some defect properties can be entered", true, item.WKI_P9_DefectCausedByTask_ReadOnly);

			item.DefectCausedByWorkItemPK = causedItem.PK;
			item.ActualValidation.ValidateDefectCausedByWorkItemPK();
			AssertEquals("WI is an issue fix - all defect properties can be entered", false, item.WKI_P9_DefectCausedByTask_ReadOnly);

			item.DefectCausedByWorkItemPK = ZGuid.Invalid;
			item.ActualValidation.ValidateDefectCausedByWorkItemPK();
			AssertEquals("WI is an issue fix - only some defect properties can be entered since the entered WI is invalid", true, item.WKI_P9_DefectCausedByTask_ReadOnly);

			WorkItem defectCausedItem = Factory.NewWithValidTestData<WorkItem>();
			item.WKI_P9_DefectCausedByTask = defectCausedItem.WorkflowItems.AddNew().PK;
			item.WKI_P9_DefectFirstMissedInTask = defectCausedItem.WorkflowItems.AddNew().PK;
			item.WKI_ActivitySubtype = "CLI";
			AssertEquals("Should be cleared", ZGuid.Empty, item.WKI_P9_DefectCausedByTask);
			AssertEquals("Should be cleared", ZGuid.Empty, item.WKI_P9_DefectFirstMissedInTask);

			item.WKI_ActivitySubtype = "FIX";
			AssertEquals("WI is an defect fix so some defect properties can be entered", false, item.DefectCausedByWorkItemPK_ReadOnly);
			AssertEquals("WI is an defect fix - only some defect properties can be entered", true, item.WKI_P9_DefectCausedByTask_ReadOnly);

			item.DefectCausedByWorkItemPK = causedItem.PK;
			item.ActualValidation.ValidateDefectCausedByWorkItemPK();
			AssertEquals("WI is an defect fix - all defect properties can be entered", false, item.WKI_P9_DefectCausedByTask_ReadOnly);

			item.DefectCausedByWorkItemPK = ZGuid.Invalid;
			item.ActualValidation.ValidateDefectCausedByWorkItemPK();
			AssertEquals("WI is an defect fix - only some defect properties can be entered since the entered WI is invalid", true, item.WKI_P9_DefectCausedByTask_ReadOnly);

			ProcessManagementRegistry.Instance.DefectWorkItemTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "ZZZ" });
			item.WKI_ActivitySubtype = "ZZZ";
			AssertEquals("defect field is enabled", false, item.DefectCausedByWorkItemPK_ReadOnly);
		}

		#endregion

		#region Work Item Status

		public void TestStatusChangingBetweenOpenAndClose_ShouldRaiseJobOpenAndCloseEvents()
		{
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			Factory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItem.WKI_Status);
			MasterFilesTestHelper.AssertNoEventRaised("Should not raise the JOP event until an incomplete task is added", workItem, AutoEvents.JobOpenCode);
			MasterFilesTestHelper.AssertNoEventRaised("Should not raise the JCL event until the status actually changes. The WI was never legitimately open so there's no reason to close it.", workItem, AutoEvents.JobCloseCode);

			var task = MasterFilesTestHelper.CreateTask(workItem);

			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem.WKI_Status);
			MasterFilesTestHelper.AssertNoEventRaised("Should not raise JOP event until the WI is saved", workItem, AutoEvents.JobOpenCode);
			MasterFilesTestHelper.AssertNoEventRaised(workItem, AutoEvents.JobCloseCode);

			Factory.Save();

			MasterFilesTestHelper.AssertEventRaised("Creating an incomplete task and then saving the WI should raise the JOP event", workItem, AutoEvents.JobOpenCode);
			MasterFilesTestHelper.AssertNoEventRaised(workItem, AutoEvents.JobCloseCode);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItem.WKI_Status);
			MasterFilesTestHelper.AssertNoEventRaised("Should not raise JCL event until the WI is saved", workItem, AutoEvents.JobCloseCode);

			Factory.Save();
			MasterFilesTestHelper.AssertEventRaised(workItem, AutoEvents.JobCloseCode);
		}

		public void TestStatusChangingBetweenOpenAndClose_WhenWorkItemCreatedWithIncompleteTask_ShouldRaiseJobOpenEvent()
		{
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var task = MasterFilesTestHelper.CreateTask(workItem);

			Factory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem.WKI_Status);
			MasterFilesTestHelper.AssertEventRaised(workItem, AutoEvents.JobOpenCode);
		}

		public void TestStatusChangingBetweenOpenAndClose_WhenIncompleteTaskAppliedFromWorkflowTemplate_ShouldRaiseJobOpenEvent()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);
			var templateTask = ProcessMgmtTestHelper.CreateTemplateTask(template, "Stahp");

			Factory.Save();

			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			Factory.Save();

			var task = workItem.WorkflowItems.Tasks.SingleOrDefault();

			AssertNotNull("Should have applied the task from the workflow template", task);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem.WKI_Status);
			MasterFilesTestHelper.AssertEventRaised(workItem, AutoEvents.JobOpenCode);
		}

		public void TestWorkItemStatus_DefaultValue()
		{
			var workItem = Factory.New<WorkItem>();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, workItem.WKI_Status);
		}

		public void TestWorkItemStatus_ShouldNotBeHumanEditable()
		{
			var workItem = Factory.New<WorkItem>();

			AssertEquals(true, workItem.WKI_StatusInfo.ReadOnly);
		}

		public void TestStatus_ConcurrentTaskDeletes()
		{
			Factory.RefreshEnabled = false;
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var task1 = workItem.WorkflowItems.AddNew();
			var task2 = workItem.WorkflowItems.AddNew();
			var task3 = workItem.WorkflowItems.AddNew();
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNotEquals("CLS", workItem.WKI_Status);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var workItemInOtherFactory = otherFactory.Load<WorkItem>(workItem.PK);
			var task2InOtherFactory = workItemInOtherFactory.WorkflowItems.FindByPK(task2.PK) as WorkItemProcessTask;

			task1.Delete();
			task2InOtherFactory.Delete();

			AssertNotEquals("CLS", workItem.WKI_Status);
			AssertNotEquals("CLS", workItemInOtherFactory.WKI_Status);

			Factory.Save();
			otherFactory.Save();
			var workItemInDb = new BusinessObjectFactory().Load<WorkItem>(workItem.PK);

			AssertEquals("CLS", workItemInOtherFactory.WKI_Status);
			AssertEquals("CLS", workItemInDb.WKI_Status);
		}

		public void TestStatus_ConcurrentTaskStatusChanges()
		{
			Factory.RefreshEnabled = false;
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var task1 = workItem.WorkflowItems.AddNew();
			var task2 = workItem.WorkflowItems.AddNew();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var workItemOther = otherFactory.Load<WorkItem>(workItem.PK);
			var task1Other = workItemOther.WorkflowItems.FindByPK(task1.PK) as WorkItemProcessTask;
			var task2Other = workItemOther.WorkflowItems.FindByPK(task2.PK) as WorkItemProcessTask;

			AssertNotNull(task1Other);
			AssertNotNull(task2Other);
			task1.P9_Status = "CLS";
			task2Other.P9_Status = "CLS";

			AssertNotEquals("CLS", task1Other.P9_Status);

			Factory.Save();
			otherFactory.Save();
			var workItemInDb = new BusinessObjectFactory().Load<WorkItem>(workItem.PK);

			AssertEquals("CLS", task1Other.P9_Status);
			AssertEquals("CLS", workItemOther.WKI_Status);
			AssertEquals("CLS", workItemInDb.WKI_Status);
		}

		public void TestStatus_ConcurrentTaskStatusChangeAndDelete()
		{
			Factory.RefreshEnabled = false;
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var task1 = workItem.WorkflowItems.AddNew();
			var task2 = workItem.WorkflowItems.AddNew();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var workItemOther = otherFactory.Load<WorkItem>(workItem.PK);
			var task1Other = workItemOther.WorkflowItems.FindByPK(task1.PK) as WorkItemProcessTask;
			var task2Other = workItemOther.WorkflowItems.FindByPK(task2.PK) as WorkItemProcessTask;

			AssertNotNull(task1Other);
			AssertNotNull(task2Other);
			task1.P9_Status = "CLS";
			task2Other.Delete();

			AssertNotEquals("CLS", task1Other.P9_Status);

			Factory.Save();
			otherFactory.Save();
			var workItemInDb = new BusinessObjectFactory().Load<WorkItem>(workItem.PK);

			AssertEquals("CLS", task1Other.P9_Status);
			AssertEquals("CLS", workItemOther.WKI_Status);
			AssertEquals("CLS", workItemInDb.WKI_Status);
		}

		public void TestCalculateStatus_DoesNotSetHasChanges()
		{
			var workitem = Factory.NewWithValidTestData<WorkItem>();
			Factory.Save();

			using (workitem.SuspendSettingHasChanges())
			{
				workitem.WKI_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			}

			AssertEquals("Workitem status should be cancelled after setting it to cancelled.", workitem.WKI_Status, ProcessTaskStatusCodeList.Codes.Cancelled);
			workitem.CalculateAndSetStatusWithoutSettingHasChanges_ForTest();
			Assert("CalculateAndSetStatus should not set has changes", !workitem.HasChanges);
		}

		public void TestCalculateStatus_CloseMethod()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var workItem2 = Factory.NewWithValidTestData<DummyWorkItemRelateItem>();

			workItem.WKI_WorkItemNumber = "WIDM00001";
			workItem.WKI_WorkItemNumber = "WIDM00002";

			var milestone = workItem.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Test";

			Factory.Save();

			workItem.RelatedItems.Add(workItem2);
			Assert(workItem2 is IWorkItemRelatedItem);

			AssertEquals(1, workItem.RelatedItems.Count);
			AssertEquals(1, workItem.WorkflowItems.Count);
			AssertEquals(0, workItem.WorkflowItems.Tasks.Count);

			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Working;
			workItem2.IsOnRelatedWorkItemClosedFired = false;
			workItem.CalculateAndSetStatusWithoutSettingHasChanges_ForTest();
			Assert("OnRelatedWorkItemClosed should not be fired because of no tasks", !workItem2.IsOnRelatedWorkItemClosedFired);

			var task = workItem.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Test";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task.P9_GS_NKAssignedStaffMember = "E";
			task.P9_ActualDuration = ZDateTime.Now.AddHours(1);

			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertEquals(2, workItem.WorkflowItems.Count);
			AssertEquals(1, workItem.WorkflowItems.Tasks.Count);

			workItem2.IsOnRelatedWorkItemClosedFired = false;
			workItem.CalculateAndSetStatusWithoutSettingHasChanges_ForTest();
			Assert("OnRelatedWorkItemClosed should be fired", workItem2.IsOnRelatedWorkItemClosedFired);
		}

		class DummyWorkItemRelateItem : WorkItem, IWorkItemRelatedItem
		{
			public DummyWorkItemRelateItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool IsOnRelatedWorkItemClosedFired { get; set; }
			public bool OnRelatedWorkItemClosed(WorkItem workItem)
			{
				IsOnRelatedWorkItemClosedFired = true;
				return true;
			}

			public void OnRelatedWorkItemReOpened(WorkItem workItem) { }
			public void OnWorkItemAdded(WorkItem workItem) { }

			public void OnWorkItemRemoved(WorkItem workItem) { }
		}

		public void TestItemWithMilestonesStatus_AllTaskDeleteClosesWorkItem()
		{
			var item = Factory.NewWithValidTestData<WorkItem>();
			ProcessTask task1 = item.WorkflowItems.AddNew();
			task1.P9_Type = "COD";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			ProcessTask task2 = item.WorkflowItems.AddNew();
			task2.P9_Type = "COD";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			ProcessTask milestone = item.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Some milestone";
			ProcessTask trigger = item.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Some trigger";

			Factory.Save();

			task1.Delete();
			task2.Delete();

			Factory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, item.WKI_Status);
		}

		public void TestStatus()
		{
			var item = Factory.NewWithValidTestData<WorkItem>();

			ProcessTask designReviewTask = item.WorkflowItems.AddNew();
			designReviewTask.P9_Type = "RVW";

			ProcessTask codeTask = item.WorkflowItems.AddNew();
			codeTask.P9_Description = "Some work to do";
			codeTask.P9_Type = "COD";

			ProcessTask codeReviewTask = item.WorkflowItems.AddNew();
			codeReviewTask.P9_Type = "RVW";

			ProcessTask checkinTask = item.WorkflowItems.AddNew();
			checkinTask.P9_Type = "CHK";

			ProcessTask checkinTask2 = item.WorkflowItems.AddNew();
			checkinTask2.P9_Type = "CH2";

			// non tasks should not affect status & disposition
			ProcessTask milestone = item.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Some milestone";
			ProcessTask trigger = item.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Some trigger";
			ProcessTask exception = item.WorkflowItems.Exceptions.AddNew();
			exception.P9_Description = "Some exception";

			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, item.WKI_Status);

			designReviewTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, item.WKI_Status);

			designReviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, item.WKI_Status);

			codeTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, item.WKI_Status);

			codeTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, item.WKI_Status);

			codeTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, item.WKI_Status);

			codeReviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, item.WKI_Status);

			checkinTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, item.WKI_Status);

			checkinTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkinTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, item.WKI_Status);

			checkinTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var updateNoteTask = item.WorkflowItems.AddNew();
			updateNoteTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, item.WKI_Status);

			updateNoteTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, item.WKI_Status);

			updateNoteTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, item.WKI_Status);

			var item2 = Factory.NewWithValidTestData<WorkItem>();
			var task1 = item2.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			var task2 = item2.WorkflowItems.AddNew();
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, item2.WKI_Status);
		}

		public void TestHasStartedTask()
		{
			var item = Factory.NewWithValidTestData<WorkItem>();
			ProcessTask task1 = item.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertEquals(item.WKI_Status, ProcessTaskStatusCodeList.Codes.Open);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, item.WKI_Status);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, item.WKI_Status);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, item.WKI_Status);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, item.WKI_Status);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, item.WKI_Status);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Still cancelled, since just closing tasks does not uncancel", ProcessTaskStatusCodeList.Codes.Cancelled, item.WKI_Status);
		}

		public void TestClosedAndCancelledTasks()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var task1 = workItem.WorkflowItems.AddNew();
			var task2 = workItem.WorkflowItems.AddNew();
			var task3 = workItem.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, workItem.WKI_Status);

			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItem.WKI_Status);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, workItem.WKI_Status);
		}

		public void TestCreateCloseThenFinallySave_CheckJCL()
		{
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var task1 = MasterFilesTestHelper.CreateTask(workItem);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			MasterFilesTestHelper.AssertEventRaised("JCL raised for WI", workItem, AutoEvents.JobCloseCode);

			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItem.WKI_Status);
		}

		#endregion

		#region Related Items

		public void TestSupportedRelatedItemModules()
		{
			var workItem = Factory.New<WorkItem>();
			var supportedModules = workItem.SupportedRelatedItemModules.Select(x => x.ModuleID.Name);
			AssertContainsExactElementsInAnyOrder(ExpectedSupportedRelatedItemModules, supportedModules);
		}

		protected virtual IEnumerable<string> ExpectedSupportedRelatedItemModules => new[] { ModuleIDs.CustomerServiceTicket.Name, ModuleIDs.Project.Name };

		public void TestAddRelatedCustomerServiceTicket_WhenProductivityWiseModeDisabled_ShouldCreateCorrectPivotType()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			workItem.RelatedItems.Add(ticket);
			Factory.Save();

			var genPivots = Factory.Load<GenPivot>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("Linking a CST to a Work Item should not create GenPivots. SAD!", Array.Empty<GenPivot>(), genPivots);

			var links = Factory.Load<WorkItemRequestLink>(new ZQuery());
			AssertEquals("Linking a CST to a Work Item should create a WorkItemRequestLink. SAD!", 1, links.Length);

			var link = links.Single();
			AssertEquals(workItem, link.WorkItem);
			AssertEquals(ticket, link.WorkRequest);
		}

		#endregion

		#region eConversation

		public void TestConversation_WhenCreateNewWorkItemAndNotSaved_ShouldNotAllowConversationToBeMade()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			AssertNull("You have to save the parent before you can create a conversation for it", workItem.Conversation);
		}

		public void TestConversation_WhenWorkItemExistsInDB_ShouldAllowConversationToBeMade()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			Factory.Save();

			var conversation = workItem.Conversation;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedWorkItem = newFactory.Load<WorkItem>(workItem.PK);
			var loadedConversation = loadedWorkItem.Conversation;
			AssertNotNull("When the Work item exists in the DB, the conversation object should not return null", loadedConversation);
			AssertEquals("A new conversation should not be created for the same work item. SAD!", conversation.PK, loadedConversation.PK);
		}

		public void TestConversation_WhenSavingNewWorkItem_ConversationShouldHaveNoParticipants()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			Factory.Save();

			var conversation = workItem.Conversation;
			AssertEquals("Conversation should have no participants!", 0, conversation.Participants.Count);
			AssertEquals("Conversation should have no staff!", 0, conversation.Staff.Count);
			AssertEquals("Conversation should have no groups!", 0, conversation.Groups.Count);
			AssertEquals("Conversation should have no related parties!", 0, conversation.RelatedParties.Count);
		}

		public void TestConversation_WhenAddConversationMessageAndSave_ShouldSendNotificationEmailsToParticipants()
		{
			AssertTestConversation_WhenAddMessageAndSave_ShouldSendNotificationEmails(isInternal: false);
		}

		public void TestConversation_WhenAddInternalMessageAndSave_ShouldSendNotificationEmailsToStaff()
		{
			AssertTestConversation_WhenAddMessageAndSave_ShouldSendNotificationEmails(isInternal: true);
		}

		void AssertTestConversation_WhenAddMessageAndSave_ShouldSendNotificationEmails(bool isInternal)
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var resource1 = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory, contactEmail: "superman@krypton.com");
			var resource2 = ProcessMgmtTestHelper.CreateStaff(Factory, emailAddress: "theflash@speedforce.com");
			var resource3 = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory, contactEmail: "livewire@electricity.com");
			var resource4 = ProcessMgmtTestHelper.CreateStaff(Factory, emailAddress: "missmarvel@kree.com");

			Factory.Save();

			var participant1 = workItem.Conversation.Participants.AddNewParticipant(resource1);
			var participant2 = workItem.Conversation.Staff.AddNewParticipant(resource2);
			var participant3 = workItem.Conversation.RelatedParties.AddNewParticipant(resource3);
			var participant4 = workItem.Conversation.Staff.AddNewParticipant(resource4);
			participant4.JCP_IsSubscribed = false;

			Factory.Save();

			const string brickMsg = "A brick weighs one kilogram plus half a brick. How much does the brick weigh?";
			workItem.Conversation.AddMessageFromCurrentUser(brickMsg, isInternal);

			AssertEquals(0, Env.AllEmailsCreated.Count());

			Factory.Save();

			var emails = Env.AllEmailsCreated.ToArray();

			var expectedEmailAddresses = new[] {
				new { Include = !isInternal, Email = "superman@krypton.com" },
				new { Include = true, Email = "theflash@speedforce.com" },
				new { Include = !isInternal, Email = "livewire@electricity.com" },
				new { Include = false, Email = "missmarvel@kree.com" }
			}
			.Where(l => l.Include)
			.Select(m => m.Email);

			AssertContainsExactElementsInAnyOrder("Should have sent the email to the correct people.", expectedEmailAddresses, emails.SelectMany(email => email.Recipients.Cast<RecipientDef>()).Select(r => r.Email));
			AssertEquals("Should've sent the correct number of emails.", expectedEmailAddresses.Count(), Env.AllEmailsCreated.Count());

			emails.ForEach(email => AssertContains("The email should contain the stunning message about bricks.", brickMsg, email.Body));
		}

		#endregion

		#region IWorkQueueMembersProvider Members
		public void TestJobCreateBy()
		{
			var workItem = Factory.New<WorkItem>();
			AssertEquals("", workItem.JobCreatedBy);

			workItem.WKI_SystemCreateUser = "AAA";
			AssertEquals("AAA", workItem.JobCreatedBy);
		}

		[TestDate(2019, 10, 22)]
		public void TestJobCreatedDate()
		{
			var workItem = Factory.New<WorkItem>();
			AssertEquals(ZDateTime.Empty, workItem.JobCreatedDate);

			workItem.WKI_SystemCreateTimeUtc = ZDateTime.Now;
			AssertEquals(new ZDateTime(2019, 10, 22, 0, 0, 0), workItem.JobCreatedDate);
		}

		public void TestJobCriteria1()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.New<WorkItem>();
			AssertEquals("", workItem.JobCriteria1);

			workItem.WKI_WorkItemType = "1AA";
			AssertEquals("1AA depth 1", workItem.JobCriteria1);

			workItem.WKI_WorkItemType = "1ZZ";
			AssertEquals("1ZZ depth 1", workItem.JobCriteria1);
		}

		public void TestJobCriteria2()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.New<WorkItem>();
			AssertEquals("", workItem.JobCriteria2);

			workItem.WKI_WorkItemArea = "2AA";
			AssertEquals("2AA depth 2", workItem.JobCriteria2);

			workItem.WKI_WorkItemArea = "2ZZ";
			AssertEquals("2ZZ depth 2", workItem.JobCriteria2);
		}

		public void TestJobCriteria3()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.New<WorkItem>();
			AssertEquals("", workItem.JobCriteria3);

			workItem.WKI_ActivityType = "3AA";
			AssertEquals("3AA depth 3", workItem.JobCriteria3);

			workItem.WKI_ActivityType = "3ZZ";
			AssertEquals("3ZZ depth 3", workItem.JobCriteria3);
		}

		public void TestJobCriteria4()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.New<WorkItem>();
			AssertEquals("", workItem.JobCriteria4);

			workItem.WKI_ActivitySubtype = "4AA";
			AssertEquals("4AA depth 4", workItem.JobCriteria4);

			workItem.WKI_ActivitySubtype = "4ZZ";
			AssertEquals("4ZZ depth 4", workItem.JobCriteria4);
		}

		public void TestJobCriteria5()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree5();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.New<WorkItem>();
			AssertEquals("", workItem.JobCriteria5);

			workItem.WKI_Priority = "5ZZ";
			AssertEquals("5ZZ depth 5", workItem.JobCriteria5);
		}

		#endregion

	}

	#region WorkItemRelatableActivityTest

	[TestedType(typeof(WorkItem))]
	public class WorkItemRelatableActivityTest : RelatableActivityTestCase<WorkItem>
	{
		protected override WorkItem GetNewActivity()
		{
			return Factory.NewWithValidTestData<WorkItem>();
		}
	}

	#endregion

	#region Interface Implementation Test Cases

	[TestedType(typeof(WorkItem))]
	class WorkItemAuditParentTest : AuditParentTest<WorkItem>
	{
		protected override WorkItem NewTestAuditParent()
		{
			return Factory.New<WorkItem>();
		}
	}

	public abstract class WorkItemRelatedItemTestCase : WorkTaskRelatedItemTestCase
	{
		protected override string ExpectedSelectionCriterion1 => "1AA - 1AA depth 1";
		protected override string ExpectedSelectionCriterion2 => "2AA - 2AA depth 2";
		protected override string ExpectedSelectionCriterion3 => "3AA - 3AA depth 3";
		protected override string ExpectedSelectionCriterion4 => "4AA - 4AA depth 4";
		protected override string ExpectedSelectionCriterion5 => "ABC";

		protected override IWorkTaskRelatedItem GetItemForSelectionCriteriaTest()
		{
			var workItem = (WorkItem)Factory.New(TestedTypeHelper.GetTestedType(GetType()));

			workItem.WKI_WorkItemType = "1AA";
			workItem.WKI_WorkItemArea = "2AA";
			workItem.WKI_ActivityType = "3AA";
			workItem.WKI_ActivitySubtype = "4AA";
			workItem.WKI_Priority = "ABC";

			return workItem;
		}

		protected override Type ExpectedPivotCollectionType => typeof(GenPivotCollection);

		protected override void SetUp()
		{
			base.SetUp();

			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);
		}
	}

	[TestedType(typeof(WorkItem))]
	sealed class WorkItemRelatedItemTest : WorkItemRelatedItemTestCase
	{
	}

	public abstract class WorkItemRelatedItemSourceTestCase : WorkTaskRelatedItemSourceTestCase
	{
	}

	[TestedType(typeof(WorkItem))]
	sealed class WorkItemRelatedItemSourceTest : WorkItemRelatedItemSourceTestCase
	{
	}

	#endregion
}
