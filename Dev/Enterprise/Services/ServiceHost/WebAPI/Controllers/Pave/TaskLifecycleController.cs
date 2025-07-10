using System;
using System.Web.Http;
using Enterprise.BufferManagement.Service;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.BufferManagement.Service.Shared.Task.Dtos.Lifecycle;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class TaskLifecycleController(ITaskLifecycleService taskLifecycleService) : BasePaveController
	{
		protected readonly ITaskLifecycleService taskLifecycleService = taskLifecycleService;

		public TaskLifecycleController() : this(new TaskLifecycleService())
		{
		}

		[HttpPost]
		[Route("api/pave/tasks/{taskId}/start")]
		public IHttpActionResult Start(Guid taskId, [FromBody] StartTaskRequest request)
		{
			return RunWithUserContext(() =>
			{
				if (!taskLifecycleService.TryStart(taskId, request, out var businessResponse))
				{
					return UnprocessableEntity(businessResponse);
				}
				return Ok();
			});
		}

		[HttpPost]
		[Route("api/pave/tasks/{taskId}/suspend")]
		public IHttpActionResult Suspend(Guid taskId, [FromBody] SuspendTaskRequest request)
		{
			return RunWithUserContext(() =>
			{
				if (!taskLifecycleService.TrySuspend(taskId, request, out var businessResponse))
				{
					return UnprocessableEntity(businessResponse);
				}
				return Ok();
			});
		}

		[HttpPost]
		[Route("api/pave/tasks/{taskId}/resume")]
		public IHttpActionResult Resume(Guid taskId, [FromBody] ResumeTaskRequest request)
		{
			return RunWithUserContext(() =>
			{
				if (!taskLifecycleService.TryResume(taskId, request, out var businessResponse))
				{
					return UnprocessableEntity(businessResponse);
				}
				return Ok();
			});
		}

		[HttpPost]
		[Route("api/pave/tasks/{taskId}/cancel")]
		public IHttpActionResult Cancel(Guid taskId, [FromBody] CancelTaskRequest request)
		{
			return RunWithUserContext(() =>
			{
				if (!taskLifecycleService.TryCancel(taskId, request, out var businessResponse))
				{
					return UnprocessableEntity(businessResponse);
				}
				return Ok();
			});
		}

		[HttpPost]
		[Route("api/pave/tasks/{taskId}/claim")]
		public IHttpActionResult Claim(Guid taskId, [FromBody] ClaimTaskRequest request)
		{
			return RunWithUserContext(() =>
			{
				if (!taskLifecycleService.TryClaim(taskId, request, out var businessResponse))
				{
					return UnprocessableEntity(businessResponse);
				}
				return Ok();
			});
		}

		[HttpPost]
		[Route("api/pave/tasks/{taskId}/claim-and-start")]
		public IHttpActionResult ClaimAndStart(Guid taskId, [FromBody] ClaimAndStartTaskRequest request)
		{
			return RunWithUserContext(() =>
			{
				if (!taskLifecycleService.TryClaimAndStart(taskId, request, out var businessResponse))
				{
					return UnprocessableEntity(businessResponse);
				}
				return Ok();
			});
		}

		[HttpPost]
		[Route("api/pave/tasks/{taskId}/assign")]
		public IHttpActionResult Assign(Guid taskId, [FromBody] AssignTaskRequest request)
		{
			return RunWithUserContext(() =>
			{
				if (!taskLifecycleService.TryAssign(taskId, request, out var businessResponse))
				{
					return UnprocessableEntity(businessResponse);
				}
				return Ok();
			});
		}

		[HttpPost]
		[Route("api/pave/tasks/{taskId}/complete")]
		public IHttpActionResult Complete(Guid taskId, [FromBody] CompleteTaskRequest request)
		{
			return RunWithUserContext(() =>
			{
				if (!taskLifecycleService.TryComplete(taskId, request, out var businessResponse))
				{
					return UnprocessableEntity(businessResponse);
				}
				return Ok();
			});
		}

		[HttpGet]
		[Route("api/pave/tasks/{taskId}/complete/pre-check")]
		public IHttpActionResult CheckBeforeComplete(Guid taskId)
		{
			return RunWithUserContext(() =>
			{
				if (!taskLifecycleService.TryCheckBeforeComplete(taskId, out var taskCompletionCheckResponse, out var businessResponse))
				{
					return UnprocessableEntity(businessResponse);
				}
				return Ok(taskCompletionCheckResponse);
			});
		}

		[HttpPost]
		[Route("api/pave/tasks/{taskId}/reopen")]
		public object Reopen(Guid taskId, [FromBody] ReopenTaskRequest request)
		{
			return RunWithUserContext(() =>
			{
				if (!taskLifecycleService.TryReopen(taskId, request, out var businessResponse))
				{
					return UnprocessableEntity(businessResponse);
				}
				return Ok();
			});
		}
	}
}
