using System;
using System.Net;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Service;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.BufferManagement.Service.Shared.Workflows.Dtos;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class WorkflowController : BasePaveController<IWorkflowService>
	{
		public WorkflowController() : base(() => new WorkflowService())
		{
		}

		[HttpPost]
		[Route("pave/workflows/{workflowId}/tasks/{taskId}/reorder")]
		public IHttpActionResult PostReorderTask(Guid workflowId, Guid taskId, WorkflowTaskReorderRequest workflowTaskReorderRequest)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (!Service.TryReorderTask(workflowId, taskId, workflowTaskReorderRequest, out PaveError error))
				{
					if (error != null)
					{
						return ToPaveResponseJson((object)null, error);
					}

					return Content(HttpStatusCode.BadRequest, workflowTaskReorderRequest);
				}

				return ToPaveResponseJson();
			}
		}

		[HttpPost]
		[Route("pave/workflows/{workflowId}/groups/{groupId}/reorder")]
		public IHttpActionResult PostReorderGroup(Guid workflowId, int groupId, WorkflowGroupReorderRequest workflowGroupReorderRequest)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (!Service.TryReorderGroup(workflowId, groupId, workflowGroupReorderRequest, out PaveError error))
				{
					if (error != null)
					{
						return ToPaveResponseJson((object)null, error);
					}

					return Content(HttpStatusCode.BadRequest, workflowGroupReorderRequest);
				}

				return ToPaveResponseJson();
			}
		}

		[HttpPost]
		[Route("pave/workflows/{workflowId}/groups/{groupId}/merge/{sourceGroupId}")]
		public IHttpActionResult PostMergeGroups(Guid workflowId, int groupId, int sourceGroupId, WorkflowGroupMergeRequest workflowGroupMergeRequest)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (!Service.TryMergeGroups(workflowId, sourceGroupId, groupId, workflowGroupMergeRequest, out PaveError error))
				{
					if (error != null)
					{
						return ToPaveResponseJson((object)null, error);
					}

					return Content(HttpStatusCode.BadRequest, workflowGroupMergeRequest);
				}

				return ToPaveResponseJson();
			}
		}

		[HttpPost]
		[Route("pave/workflows/{workflowId}/groups/{groupId}/tasks/{taskId}")]
		public IHttpActionResult PostGroupTask(Guid workflowId, int groupId, Guid taskId, WorkflowGroupTaskRequest workflowGroupTaskRequest)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (!Service.TryGroupTask(workflowId, groupId, taskId, workflowGroupTaskRequest, out PaveError error))
				{
					if (error != null)
					{
						return ToPaveResponseJson((object)null, error);
					}

					return Content(HttpStatusCode.BadRequest, workflowGroupTaskRequest);
				}

				return ToPaveResponseJson();
			}
		}

		[HttpPost]
		[Route("pave/workflows/{workflowId}/tasks/{taskId}/merge-group/{groupId}")]
		public IHttpActionResult PostMergeGroupOnTask(Guid workflowId, Guid taskId, int groupId, WorkflowMergeGroupOnTaskRequest workflowMergeGroupOnTaskRequest)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (!Service.TryMergeGroupOnTask(workflowId, taskId, groupId, workflowMergeGroupOnTaskRequest, out PaveError error))
				{
					if (error != null)
					{
						return ToPaveResponseJson((object)null, error);
					}

					return Content(HttpStatusCode.BadRequest, workflowMergeGroupOnTaskRequest);
				}

				return ToPaveResponseJson();
			}
		}
	}
}
