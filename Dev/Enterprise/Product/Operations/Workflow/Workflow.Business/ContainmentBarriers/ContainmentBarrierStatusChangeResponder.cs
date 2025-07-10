using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Workflow.Business
{
	public class ContainmentBarrierStatusChangeResponder : IStatusChangeResponder
	{
		public StatusChangeResponder Responder => StatusChangeResponder.ContainmentBarrier;

		#region IStatusChangeResponder Members

		StatusChangeResult IStatusChangeResponder.RespondToChange(IProcessTask task, string newStatus)
		{
			var processTask = task as ProcessTask;

			if (processTask == null)
			{
				return StatusChangeResult.ChangeHandled;
			}

			if (processTask.ContainmentBarrierStatusChangeResponderSupressed || processTask.Factory.IsRunningClientSideTriggerAction())
			{
				return StatusChangeResult.ChangeHandled;
			}

			using (var viewModel = new ContainmentBarrierViewModel(processTask, newStatus, deselectCancelledTasksFromIteration: true))
			{
				if (newStatus == ProcessTaskStatusCodeList.Codes.Cancelled)
				{
					viewModel.Response = ContainmentBarrierResponses.Canceled;
					viewModel.CommitResponse();

					return StatusChangeResult.ChangeHandled;
				}
				else
				{
					if (!Globals.CanShowDialogs)
					{
						return StatusChangeResult.ChangeHandled;
					}

					var view = ObjectFactory.Get<IContainmentBarrierResponseView>(nameof(IContainmentBarrierResponseView), viewModel);
					var userResponse = view.GetResponseFromUser();

					switch (userResponse)
					{
						case ContainmentBarrierResponses.Canceled:
							return StatusChangeResult.ChangeNotHandled;

						case ContainmentBarrierResponses.None:
							return StatusChangeResult.Unknown;

						default:
							return StatusChangeResult.ChangeHandled;
					}
				}
			}
		}

		bool IStatusChangeResponder.RespondsToStatusChange(IProcessTask task, string newStatus)
		{
			switch (newStatus)
			{
				case ProcessTaskStatusCodeList.Codes.Closed:
				case ProcessTaskStatusCodeList.Codes.Cancelled:
					return task.IsQualityContainmentBarrierTask();

				default:
					return false;
			}
		}

		public (bool result, string reason) CanChangeStatus(IProcessTask task, string newStatus)
		{
			return task.IsQualityContainmentBarrierTask()
				? (false, Res.GetString("307164A3-D015-4339-8C59-0D03AB8FB720", "Task is a quality containment barrier."))
				: (true, null);
		}

		#endregion
	}
}
