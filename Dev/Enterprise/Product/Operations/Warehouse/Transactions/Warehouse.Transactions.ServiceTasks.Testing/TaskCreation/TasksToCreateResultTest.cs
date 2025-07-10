using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	class TasksToCreateResultTest : TestCaseWithFactory
	{
		public void TestConstructor_NullThrows()
		{
			var taskToCreate = new TaskToCreate(ZGuid.BrettsGuid, "TSK", "WNAME", "TNAME", "BRS", 1, "PWH", ZGuid.BrettsGuid, "TST");
			var line = Mock.Of<ITaskManagementLineFact>();
			var tasksToCreate = new[] { taskToCreate };
			var lines = new[] { line };

			AssertExceptionThrown<ArgumentNullException>(() => new TasksToCreateResult(null, lines));
			AssertExceptionThrown<ArgumentNullException>(() => new TasksToCreateResult(tasksToCreate, null));
		}

		public void TestConstructor_Success()
		{
			var taskToCreate = new TaskToCreate(ZGuid.BrettsGuid, "TSK", "WNAME", "TNAME", "BRS", 1, "PWH", ZGuid.BrettsGuid, "TST");
			var line = Mock.Of<ITaskManagementLineFact>();
			var tasksToCreate = new[] { taskToCreate };
			var lines = new[] { line };

			var result = new TasksToCreateResult(tasksToCreate, lines);
			AssertEquals(nameof(TasksToCreateResult.Status), ResultStatus.Success, result.Status);
			AssertEquals(nameof(TasksToCreateResult.Notifications), string.Empty, result.Notifications);
			AssertEquals(nameof(TasksToCreateResult.Tasks), tasksToCreate, result.Tasks);
			AssertEquals(nameof(TasksToCreateResult.Lines), lines, result.Lines);
		}

		public void TestConstructor_Error()
		{
			const string notification = "Failed to run";
			var result = new TasksToCreateResult(notification);
			AssertEquals(nameof(TasksToCreateResult.Status), ResultStatus.Error, result.Status);
			AssertEquals(nameof(TasksToCreateResult.Notifications), notification, result.Notifications);
			AssertEquals(nameof(TasksToCreateResult.Tasks), 0, result.Tasks.Count());
			AssertEquals(nameof(TasksToCreateResult.Lines), 0, result.Lines.Count());
		}

		public void TestConstructor_Halted()
		{
			var result = TasksToCreateResult.Halted;
			AssertEquals(nameof(TasksToCreateResult.Status), ResultStatus.Halted, result.Status);
			AssertEquals(nameof(TasksToCreateResult.Notifications), string.Empty, result.Notifications);
			AssertEquals(nameof(TasksToCreateResult.Tasks), 0, result.Tasks.Count());
			AssertEquals(nameof(TasksToCreateResult.Lines), 0, result.Lines.Count());
		}
	}
}
