using System;
using System.Net;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Service;
using Enterprise.BufferManagement.Service.Shared.Common;
using Enterprise.BufferManagement.Service.Shared.Incident;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class IncidentController : BasePaveController<IIncidentService>
	{
		public IncidentController() : base(() => new IncidentService())
		{
		}

		[HttpPost]
		[Route("PAVE/incidents/{incidentId}/summary")]
		public IHttpActionResult PostChangeSummary(Guid incidentId, UpdateContentWithHashRequest updateRequest)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (updateRequest.PreviousHash == null)
				{
					return Content(HttpStatusCode.BadRequest, updateRequest);
				}

				var updated = Service.TryUpdateSummary(incidentId, updateRequest, out PaveError error);

				if (!updated)
				{
					return ToPaveResponseJson((object)null, error);
				}

				return ToPaveResponseJson();
			}
		}
	}
}
