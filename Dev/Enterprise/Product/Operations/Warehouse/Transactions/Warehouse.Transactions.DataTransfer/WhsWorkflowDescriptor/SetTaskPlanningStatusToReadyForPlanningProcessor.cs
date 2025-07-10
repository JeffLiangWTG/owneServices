using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	class SetTaskPlanningStatusToReadyForPlanningProcessor : IProcessor
	{
		public SetTaskPlanningStatusToReadyForPlanningProcessor(ITaskPlanningJob job)
		{
			Job = Argument.NotNull(job, nameof(job));
		}

		ITaskPlanningJob Job { get; }

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			var reason = Job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true);
			if (!reason.IsNullOrEmpty())
			{
				notifications.AddError(reason);
			}
			else if (Job.TaskPlanningStatus != TaskPlanningStatus.Codes.NotReady)
			{
				notifications.AddError(Res.GetString("e65f6323-6406-4d92-8314-678ffc4c70f0", "Current Task Planning Status is not 'Not Ready For Planning'."));
			}
			else
			{
				Job.TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			}
		}
	}
}
