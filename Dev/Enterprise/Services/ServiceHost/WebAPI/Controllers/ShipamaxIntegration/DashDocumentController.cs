using System;
using System.Web.Http;
using Enterprise.Dash.Business.Services;
using Enterprise.Dash.Integration;
using Enterprise.DocumentScanning.Web;

namespace Enterprise.Services.ServiceHost
{
	[RoutePrefix("api/dash/document")]
	[GlowTicketAuthentication]
	public class DashDocumentController : ApiController
	{
		readonly IDashCompletionService dashCompletionService;

		public DashDocumentController() : this(new DashCompletionService(new ShipamaxService(), new DashPostingService()))
		{
		}

		public DashDocumentController(IDashCompletionService dashCompletionService)
		{
			this.dashCompletionService = dashCompletionService;
		}

		[HttpPut]
		[Route("{dashDocumentId}/complete")]
		public IHttpActionResult Complete(Guid dashDocumentId)
		{
			if (dashDocumentId == Guid.Empty)
			{
				return BadRequest($"Parameter {nameof(dashDocumentId)} can't be empty");
			}

			try
			{
				using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
				{
					dashCompletionService.Complete(dashDocumentId);
				}
				return Ok();
			}
			catch(DashException ex)
			{
				return BadRequest(ex.Message);
			}
		}
	}
}
