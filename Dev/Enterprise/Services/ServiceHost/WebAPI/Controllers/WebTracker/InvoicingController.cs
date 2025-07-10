using System;
using System.Net;
using System.Web.Http;
using CargoWise.Data;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	[RoutePrefix("api/invoicing")]
	public class InvoicingController : ApiController
	{
		public InvoicingController() : this(new InvoicingService(), new GlowContactSecurityService())
		{
		}

		public InvoicingController(IInvoicingService invoicingService, IGlowContactSecurityService glowContactSecurityService)
		{
			this.invoicingService = invoicingService ?? throw new ArgumentNullException(nameof(invoicingService));
			this.glowContactSecurityService = glowContactSecurityService ?? throw new ArgumentNullException(nameof(glowContactSecurityService));
		}

		readonly IInvoicingService invoicingService;
		readonly IGlowContactSecurityService glowContactSecurityService;

		[Route("print/{invoicePK}")]
		[HttpGet]
		public IHttpActionResult Print(Guid? invoicePK)
		{
			if (invoicePK == null)
			{
				return NotFound();
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var identity = User?.Identity as IGlowAuthenticationTicketIdentity;
				var contactPK = identity?.GetContactPK();
				if (contactPK == null || !glowContactSecurityService.HasGroupRole(identity, (NoResString)"webinvoiceviewer"))
				{
					return StatusCode(HttpStatusCode.Forbidden);
				}

				var printResult = invoicingService.Print(contactPK.Value, invoicePK.Value);

				return new WebTrackerPrintActionResult(printResult);
			}
		}
	}
}
