using System;
using System.Net;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Service;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.BufferManagement.Service.Shared.Common;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class ProjectController : BasePaveController<IProjectService>
	{
		public ProjectController() : base(() => new ProjectService())
		{
		}

		[HttpGet]
		[Route("Pave/projects/{projectId}/details")]
		public IHttpActionResult GetDetails(Guid projectId)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = Service.GetDetails(projectId);
				return ToPaveResponseJson(result);
			}
		}

		[HttpPost]
		[Route("Pave/projects/{projectId}/details")]
		public IHttpActionResult PostChangeDetails(Guid projectId, UpdateContentWithHashRequest updateRequest)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (updateRequest.PreviousHash == null)
				{
					return Content(HttpStatusCode.BadRequest, updateRequest);
				}

				var response = Service.TryUpdateDetails(projectId, updateRequest);

				if (!response.Success)
				{
					return ToPaveResponseJson((object)null, response.Error);
				}

				return ToPaveResponseJson(response.DetailsResponse);
			}
		}

		[HttpPost]
		[Route("Pave/projects/{projectId}/summary")]
		public IHttpActionResult PostSummaryChange(Guid projectId, UpdateContentWithHashRequest updateRequest)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if(updateRequest.PreviousHash == null)
				{
					return Content(HttpStatusCode.BadRequest, updateRequest);
				}

				var succeeded = Service.TryUpdateSummary(projectId, updateRequest, out PaveError error);

				if(!succeeded)
				{
					return ToPaveResponseJson((object)null, error);
				}

				return ToPaveResponseJson();
			}
		}
	}
}
