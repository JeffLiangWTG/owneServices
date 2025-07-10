using System;
using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	public class WorkItemActualLookupsTest : WorkItemLookupsTest<WorkItem>
	{
		public void TestTypes()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.New<WorkItem>();
			AssertCodeDescriptionListEqual(tree.GetParents(true), workItem.Lookups.ActiveTypes);
			AssertCodeDescriptionListEqual(tree.GetParents(false), workItem.Lookups.AllTypes);
		}

		public void TestActiveAreas()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.New<WorkItem>();
			workItem.WKI_WorkItemType = "";
			var actual1 = workItem.Lookups.ActiveAreas;
			workItem.WKI_WorkItemType = "1AA";
			var actual2 = workItem.Lookups.ActiveAreas;

			AssertCodeDescriptionListEqual(tree.GetChildren("", true), actual1);
			AssertCodeDescriptionListEqual(tree.GetChildren("1AA", true), actual2);
		}

		public void TestActiveActivityTypes()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.New<WorkItem>();
			var actual1 = workItem.Lookups.ActiveActivityTypes;

			workItem.WKI_WorkItemType = "1AA";
			workItem.WKI_WorkItemArea = "2AA";
			var actual2 = workItem.Lookups.ActiveActivityTypes;

			workItem.WKI_WorkItemType = "1BB";
			workItem.WKI_WorkItemArea = "2B1";
			var actual3 = workItem.Lookups.ActiveActivityTypes;

			AssertCodeDescriptionListEqual(tree.GetChildren("", "", true), actual1);
			AssertCodeDescriptionListEqual(tree.GetChildren("1AA", "2AA", true), actual2);
			AssertCodeDescriptionListEqual(tree.GetChildren("1BB", "2B1", true), actual3);
		}

		public void TestActiveActivitySubtypes()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.New<WorkItem>();
			var actual1 = workItem.Lookups.ActiveActivitySubtypes;

			workItem.WKI_WorkItemType = "1AA";
			workItem.WKI_ActivityType = "3AA";
			var actual2 = workItem.Lookups.ActiveActivitySubtypes;

			workItem.WKI_WorkItemArea = "2AA";
			var actual3 = workItem.Lookups.ActiveActivitySubtypes;

			workItem.WKI_WorkItemType = "1BB";
			workItem.WKI_WorkItemArea = "2B1";
			workItem.WKI_ActivityType = "";
			var actual4 = workItem.Lookups.ActiveActivitySubtypes;

			workItem.WKI_ActivityType = "3S1";
			var actual5 = workItem.Lookups.ActiveActivitySubtypes;

			AssertCodeDescriptionListEqual(tree.GetChildren(true, "", "", ""), actual1);
			AssertCodeDescriptionListEqual(tree.GetChildren(true, "1AA", "", "3AA"), actual2);
			AssertCodeDescriptionListEqual(tree.GetChildren(true, "1AA", "2AA", "3AA"), actual3);
			AssertCodeDescriptionListEqual(tree.GetChildren(true, "1BB", "2B1", ""), actual4);
			AssertCodeDescriptionListEqual(tree.GetChildren(true, "1BB", "2B1", "3S1"), actual5);
		}

		public void TestStatusList()
		{
			CodeDescriptionPairList expected = new ProcessTaskStatusCodeList();

			var workItem = Factory.New<WorkItem>();
			var actual = workItem.Lookups.StatusList;
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionComparer(), expected.ToArray(), actual.ToArray());
		}

		public void TestOtherWorkItems()
		{
			var workItem = Factory.New<WorkItem>();
			var testWorkItem1 = Factory.New<WorkItem>();
			var testWorkItem2 = Factory.New<WorkItem>();

			AssertEquals("Should exclude parent workitem itself", 2, workItem.Lookups.OtherWorkItems.Count);
		}

		public void TestDefectCausedByTasks()
		{
			WorkItemTest.SetupRegistryForDefectTesting();

			var workItemIntroducedDefect = Factory.NewWithValidTestData<WorkItem>();
			var codingTask = workItemIntroducedDefect.WorkflowItems.AddNew();
			codingTask.P9_Sequence = 1;
			codingTask.P9_Type = "CDF";
			codingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var defectTaskAssigned = workItemIntroducedDefect.WorkflowItems.AddNew();
			defectTaskAssigned.P9_Sequence = 2;

			var defectIntroducedTaskType = "CH0";
			defectTaskAssigned.P9_Type = defectIntroducedTaskType;
			defectTaskAssigned.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var defectTaskClosed = workItemIntroducedDefect.WorkflowItems.AddNew();
			defectTaskClosed.P9_Sequence = 3;
			defectTaskClosed.P9_Type = defectIntroducedTaskType;
			defectTaskClosed.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var defectTask2 = workItemIntroducedDefect.WorkflowItems.AddNew();
			defectTask2.P9_Sequence = 4;
			defectTask2.P9_Type = "CH1";
			defectTask2.P9_GS_NKAssignedStaffMember = "DAT";
			defectTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var workItemToFixDefect = Factory.NewWithValidTestData<WorkItem>();

			Factory.Save();

			var collection = workItemToFixDefect.Lookups.DefectCausedByTasks;
			AssertEquals("Should be an empty collection if the 'Defect Introduced in Work Item' is not set", 0, collection.Count);

			workItemToFixDefect.DefectCausedByWorkItemPK = workItemIntroducedDefect.PK;

			collection = workItemToFixDefect.Lookups.DefectCausedByTasks;
			AssertContainsExactElementsInAnyOrder("Collection should contains only closed defect tasks.", new[] { defectTaskClosed }, collection);
		}

		public void TestDefectCausedByTasks_IncludeSavedTask()
		{
			var fixWorkItem = Factory.NewWithValidTestData<WorkItem>();
			var causedWorkItem = Factory.NewWithValidTestData<WorkItem>();
			var includedTask1 = causedWorkItem.WorkflowItems.AddNew();
			includedTask1.P9_Sequence = 1;
			includedTask1.P9_Status = "CAN";
			var includedTask2 = causedWorkItem.WorkflowItems.AddNew();
			includedTask2.P9_Sequence = 2;
			includedTask2.P9_Status = "CLS";
			Factory.Save();

			fixWorkItem.DefectCausedByWorkItemPK = causedWorkItem.PK;
			fixWorkItem.WKI_P9_DefectCausedByTask = includedTask2.PK;
			Factory.Save();

			includedTask2.P9_Status = "CAN";
			Factory.Save();

			var collection = fixWorkItem.Lookups.DefectCausedByTasks;
			AssertEquals("Should include task, which is already saved", 1, collection.Count);
			AssertEquals("Should include includedTask2 as first item", includedTask2.PK, collection[0].PK);
		}

		[GuiTest]
		public void TestDefectFirstMissedInTasks()
		{
			ProcessMgmtTestHelper.EnableBufferManagement();

			var collection = new CategorisedWorkflowTaskTypesCollection();
			var category = collection.AddNew();
			category.Code = "WKI";
			category.Description = (NoResString)"Work Item";

			var taskType1 = category.TaskTypes.AddNew();
			taskType1.Code = "RRR";
			taskType1.Description = (NoResString)"Review";
			taskType1.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.GLB;

			var taskType2 = category.TaskTypes.AddNew();
			taskType2.Code = "FRU";
			taskType2.Description = (NoResString)"Functional Review";
			taskType2.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.GLB;

			var taskType3 = category.TaskTypes.AddNew();
			taskType3.Code = "COD";
			taskType3.Description = (NoResString)"Coding";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var workItemIntroducedDefect = Factory.NewWithValidTestData<WorkItem>();
			var codingTask = workItemIntroducedDefect.WorkflowItems.AddNew();
			codingTask.P9_Sequence = 1;
			codingTask.P9_Type = "CDF";
			codingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var reviewTaskAssigned = workItemIntroducedDefect.WorkflowItems.AddNew();
			reviewTaskAssigned.P9_Sequence = 2;
			reviewTaskAssigned.P9_Type = "RRR";
			reviewTaskAssigned.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var reviewTaskClosed = workItemIntroducedDefect.WorkflowItems.AddNew();
			reviewTaskClosed.P9_Sequence = 3;
			reviewTaskClosed.P9_Type = "RRR";
			reviewTaskClosed.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var funcReviewTaskClosed = workItemIntroducedDefect.WorkflowItems.AddNew();
			funcReviewTaskClosed.P9_Sequence = 4;
			funcReviewTaskClosed.P9_Type = "FRU";
			funcReviewTaskClosed.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var workItemToFixDefect = Factory.NewWithValidTestData<WorkItem>();

			Factory.Save();

			var defectContainmentBarrierTasks = workItemToFixDefect.Lookups.DefectFirstMissedInTasks;
			AssertEquals("Should be an empty collection if the 'Defect Introduced in Work Item' is not set", 0, defectContainmentBarrierTasks.Count);

			workItemToFixDefect.DefectCausedByWorkItemPK = workItemIntroducedDefect.PK;

			defectContainmentBarrierTasks = workItemToFixDefect.Lookups.DefectFirstMissedInTasks;
			AssertContainsExactElementsInAnyOrder("Collection should contains only closed Containment Barrier Tasks.",
				new[] { reviewTaskClosed, funcReviewTaskClosed }, defectContainmentBarrierTasks);
		}

		void AssertCodeDescriptionListEqual(CodeDescriptionPairList expected, CodeDescriptionPairList actual)
		{
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionComparer(), expected.ToArray(), actual.ToArray());
		}
	}

	public class CodeDescriptionComparer : IEqualityComparer<ICodeDescription>
	{
		public int GetHashCode(ICodeDescription type)
		{
			return type.GetHashCode();
		}

		public bool Equals(ICodeDescription first, ICodeDescription second)
		{
			return first.Code == second.Code && first.Description == second.Description;
		}
	}
}
