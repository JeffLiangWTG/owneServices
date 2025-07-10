using System;
using System.Net;
using System.Web.Http;
using CargoWise.Data;
using Enterprise.Tracking.Business;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	[RoutePrefix("api/quoteDocuments")]
	public class QuoteDocumentsController : ApiController
	{
		public QuoteDocumentsController() : this(new QuoteDocumentsService())
		{
		}

		public QuoteDocumentsController(IQuoteDocumentsService quoteDocumentsService)
		{
			this.quoteDocumentsService = quoteDocumentsService ?? throw new ArgumentNullException(nameof(quoteDocumentsService));
		}
		readonly IQuoteDocumentsService quoteDocumentsService;

		[Route("print/{quotePK}")]
		[HttpGet]
		public IHttpActionResult Print(Guid? quotePK)
		{
			if (quotePK == null)
			{
				return NotFound();
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var identity = User?.Identity as IGlowAuthenticationTicketIdentity;
				var contactPK = identity?.GetContactPK();
				if (contactPK == null)
				{
					return StatusCode(HttpStatusCode.Forbidden);
				}

				var printResult = quoteDocumentsService.Print(contactPK.Value, quotePK.Value);

				return new WebTrackerPrintActionResult(printResult);
			}
		}
	}
}
