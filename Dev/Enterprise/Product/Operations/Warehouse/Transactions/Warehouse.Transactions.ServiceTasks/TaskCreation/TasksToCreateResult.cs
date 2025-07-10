using System.Collections.Generic;
using System.Linq;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	class TasksToCreateResult
	{
		public TasksToCreateResult(
			IEnumerable<TaskToCreate> tasks,
			IEnumerable<ITaskManagementLineFact> lines)
			: this(ResultStatus.Success, string.Empty, tasks, lines)
		{
		}

		public TasksToCreateResult(string error)
			: this(ResultStatus.Error, error, Enumerable.Empty<TaskToCreate>(), Enumerable.Empty<ITaskManagementLineFact>())
		{
		}

		TasksToCreateResult(
			ResultStatus status,
			string notifications,
			IEnumerable<TaskToCreate> tasks,
			IEnumerable<ITaskManagementLineFact> lines)
		{
			Status = status;
			Notifications = Argument.NotNull(notifications, nameof(notifications));
			Tasks = Argument.NotNull(tasks, nameof(tasks));
			Lines = Argument.NotNull(lines, nameof(lines));
		}

		public ResultStatus Status { get; }
		public string Notifications { get; }
		public IEnumerable<TaskToCreate> Tasks { get; }
		public IEnumerable<ITaskManagementLineFact> Lines { get; }

		public static TasksToCreateResult Halted => new TasksToCreateResult(ResultStatus.Halted, string.Empty, Enumerable.Empty<TaskToCreate>(), Enumerable.Empty<ITaskManagementLineFact>());
	}
}
