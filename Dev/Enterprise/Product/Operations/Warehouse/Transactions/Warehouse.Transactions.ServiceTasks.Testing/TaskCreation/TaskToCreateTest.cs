using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	class TaskToCreateTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var pk = ZGuid.NewZGuid();
			var releaseGroupPk = ZGuid.NewZGuid();

			var taskToCreate = new TaskToCreate(pk, "TSK", "WNAME", "TNAME", "BRS", 42, "PWH", releaseGroupPk, "TST");
			CombineAssertions(() =>
			{
				AssertEquals(nameof(TaskToCreate.ID), pk, taskToCreate.ID);
				AssertEquals(nameof(TaskToCreate.FormflowType), "TSK", taskToCreate.FormflowType);
				AssertEquals(nameof(TaskToCreate.WorkflowName), "WNAME", taskToCreate.WorkflowName);
				AssertEquals(nameof(TaskToCreate.TaskName), "TNAME", taskToCreate.TaskName);
				AssertEquals(nameof(TaskToCreate.StaffCode), "BRS", taskToCreate.StaffCode);
				AssertEquals(nameof(TaskToCreate.RawNudge), (short)42, taskToCreate.RawNudge);
				AssertEquals(nameof(TaskToCreate.ReleaseGroupPk), releaseGroupPk, taskToCreate.ReleaseGroupPk);
				AssertEquals(nameof(TaskToCreate.CapabilityCode), "PWH", taskToCreate.CapabilityCode);
				AssertEquals(nameof(TaskToCreate.TaskType), "TST", taskToCreate.TaskType);
			});
		}

		public void TestConstructor_NoTaskType()
		{
			var pk = ZGuid.NewZGuid();
			var releaseGroupPk = ZGuid.NewZGuid();

			var taskToCreate = new TaskToCreate(pk, "TSK", "WNAME", "TNAME", "BRS", 42, "PWH", releaseGroupPk);
			CombineAssertions(() =>
			{
				AssertEquals(nameof(TaskToCreate.ID), pk, taskToCreate.ID);
				AssertEquals(nameof(TaskToCreate.FormflowType), "TSK", taskToCreate.FormflowType);
				AssertEquals(nameof(TaskToCreate.WorkflowName), "WNAME", taskToCreate.WorkflowName);
				AssertEquals(nameof(TaskToCreate.TaskName), "TNAME", taskToCreate.TaskName);
				AssertEquals(nameof(TaskToCreate.StaffCode), "BRS", taskToCreate.StaffCode);
				AssertEquals(nameof(TaskToCreate.RawNudge), (short)42, taskToCreate.RawNudge);
				AssertEquals(nameof(TaskToCreate.ReleaseGroupPk), releaseGroupPk, taskToCreate.ReleaseGroupPk);
				AssertEquals(nameof(TaskToCreate.CapabilityCode), "PWH", taskToCreate.CapabilityCode);
				AssertEquals(nameof(TaskToCreate.TaskType), "UDF", taskToCreate.TaskType);
			});
		}
	}
}
