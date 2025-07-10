using System;
using System.Net;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Service;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.BufferManagement.Service.Shared.Common;
using Enterprise.BufferManagement.Service.Shared.Templates.Dtos;
using Enterprise.BufferManagement.Service.Templates;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class WorkItemController : BasePaveController<IWorkItemService>
	{
		public WorkItemController() : base(() => new WorkItemService())
		{
		}

		[HttpGet]
		[Route("Pave/workitems/{workItemId}/details")]
		public IHttpActionResult GetDetails(Guid workItemId)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = Service.GetDetails(workItemId);
				return ToPaveResponseJson(result);
			}
		}

		[HttpPost]
		[Route("Pave/workitems/{workItemId}/details")]
		public IHttpActionResult PostChangeDetails(Guid workItemId, UpdateContentWithHashRequest updateRequest)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (updateRequest.PreviousHash == null)
				{
					return Content(HttpStatusCode.BadRequest, updateRequest);
				}

				var response = Service.TryUpdateDetails(workItemId, updateRequest);

				if (!response.Success)
				{
					return ToPaveResponseJson((object)null, response.Error);
				}

				return ToPaveResponseJson(response.DetailsResponse);
			}
		}

		[HttpPost]
		[Route("Pave/workitems/{workItemId}/summary")]
		public IHttpActionResult PostChangeSummary(Guid workItemId, UpdateContentWithHashRequest updateRequest)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (updateRequest.PreviousHash == null)
				{
					return Content(HttpStatusCode.BadRequest, updateRequest);
				}

				var updated = Service.TryUpdateSummary(workItemId, updateRequest, out PaveError error);

				if (!updated)
				{
					return ToPaveResponseJson((object)null, error);
				}

				return ToPaveResponseJson();
			}
		}

		[HttpPost]
		[Route("Pave/workitems/{workItemId}/apply-note-template")]
		public IHttpActionResult PostApplyWorkItemNoteTemplate([FromUri] Guid workItemId, ApplyTemplateRequest applyNoteRequest)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var service = new TemplateService();
				var response = service.ApplyWorkItemNoteTemplate(workItemId, applyNoteRequest);
				return ToPaveResponseJson(response);
			}
		}

		#region WorkItem Selection Criteria

		[Route("Pave/workitems/types/select-list")]
		[HttpGet]
		public IHttpActionResult GetWorkItemTypes()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var list = Service.GetWorkItemTypes();
				return ToPaveResponseJson(list);
			}
		}

		[Route("Pave/workitems/areas/select-list")]
		[HttpGet]
		public IHttpActionResult GetWorkItemAreas([FromUri]string workItemType)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var list = Service.GetWorkItemAreas(workItemType);
				return ToPaveResponseJson(list);
			}
		}

		[Route("Pave/workitems/activity-types/select-list")]
		[HttpGet]
		public IHttpActionResult GetActivityTypes([FromUri] string workItemType, [FromUri] string workItemArea)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var list = Service.GetActivityTypes(workItemType, workItemArea);
				return ToPaveResponseJson(list);
			}
		}

		[Route("Pave/workitems/activity-subtypes/select-list")]
		[HttpGet]
		public IHttpActionResult GetActivitySubtypes([FromUri] string workItemType, [FromUri] string area, [FromUri] string activityType)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var list = Service.GetActivitySubtypes(workItemType, area, activityType);
				return ToPaveResponseJson(list);
			}
		}

		[Route("Pave/workitems/priorities/select-list")]
		[HttpGet]
		public IHttpActionResult GetPriorities([FromUri] string workItemType, [FromUri] string area, [FromUri] string activityType, [FromUri] string activitySubType)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var list = Service.GetPriorities(workItemType, area, activityType, activitySubType);
				return ToPaveResponseJson(list);
			}
		}

		#endregion
	}
}
