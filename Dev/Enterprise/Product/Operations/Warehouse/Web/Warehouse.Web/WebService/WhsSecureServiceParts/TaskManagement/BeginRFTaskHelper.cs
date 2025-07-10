using System;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public static class BeginRFTaskHelper
	{
		public static bool BeginRFTask(WebServiceResponse response, ProcessTask task, string expectedFormFlowType, GlbStaff staff)
			=> UpdateTaskStatus(
				response,
				task,
				(whsTaskManagementService) => whsTaskManagementService.SetTaskToPlayIfValid(task, expectedFormFlowType, staff.GS_Code));

		public static bool SuspendRFTask(WebServiceResponse response, ProcessTask task, GlbStaff staff)
			=> UpdateTaskStatus(
				response,
				task,
				(whsTaskManagementService) => whsTaskManagementService.SetTaskToSuspendedIfValid(task, staff.GS_Code));

		public static bool CloseRFTask(WebServiceResponse response, ProcessTask task, GlbStaff staff)
			=> UpdateTaskStatus(
				response,
				task,
				(whsTaskManagementService) => whsTaskManagementService.SetTaskToCompletedIfValid(task, staff.GS_Code));

		static bool UpdateTaskStatus(
			WebServiceResponse response,
			ProcessTask task,
			Func<IWhsTaskManagementService, UpdateTaskStatusResult> updateTaskStatusIfValid)
		{
			var isUpdated = false;
			if (task == null)
			{
				response.LogBusinessValidationError(Res.GetString("e9be384f-5ee9-4c10-a39e-feee9a793ecd", "Task could not be found."));
			}
			else
			{
				var result = updateTaskStatusIfValid(ObjectFactory.Get<IWhsTaskManagementService>());
				if (result == UpdateTaskStatusResult.Success)
				{
					isUpdated = true;
				}
				else if (result != UpdateTaskStatusResult.SuccessWithNoChanges)
				{
					var errorMessage = result switch
					{
						UpdateTaskStatusResult.TaskStatusIsOpen =>
							Res.GetString("f8374e4a-9160-4382-a22b-d284a13f3374", "The task is currently unassigned to any user and cannot be updated. Please check the task status and try again."),
						UpdateTaskStatusResult.TaskStatusIsCompleted =>
							Res.GetString("09f2af9b-097d-499c-8ed6-1b6ca124f577", "The task is already closed and cannot be updated. Please check the task status and try again."),
						UpdateTaskStatusResult.TaskStatusIsCancelled =>
							Res.GetString("b56880d1-72e3-404e-9de7-a5b702a3b03c", "The task is already canceled and cannot be updated. Please check the task status and try again."),
						UpdateTaskStatusResult.TaskIsNotValidWarehouseJob =>
							Res.GetString("7e355aaa-3068-4638-a251-085fbde11e17", "This task is not a valid warehouse job. Please perform a different task."),
						UpdateTaskStatusResult.TaskIsWrongFormFlowType => InvalidTaskType,
						UpdateTaskStatusResult.AssignedUserIsDifferent =>
							Res.GetString("5ca191f9-e1fd-4efe-8380-40c39c010554", "This task is not assigned to the current user. Please perform a different task."),
						_ =>
							Res.GetString("da80e173-4895-4d52-937f-afc5dd24345d", "Something went wrong while trying to update the task. Please try again."),
					};
					response.LogBusinessValidationError(errorMessage);
				}
			}

			return isUpdated;
		}

		public static string InvalidTaskType => Res.GetString("ae253741-c973-4f70-8479-70e450b656a0", "This task is not valid for the current operation.");

		public static string TaskConcurrencyErrorMessage => Res.GetString("db50a81c-1ea5-468e-8b77-c810b23e77a5", "Another user has modified this task and the status can not be updated. Please restart the operation and try again.");
	}
}
