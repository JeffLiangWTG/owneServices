using System;
using System.Net;
using System.Web.Http;
using CargoWise.Data;
using Enterprise.Tracking.Business;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	[RoutePrefix("api/statements")]
	public class StatementsController : ApiController
	{
		public StatementsController() : this(new StatementService())
		{
		}

		public StatementsController(IStatementService statementService)
		{
			this.statementService = statementService ?? throw new ArgumentNullException(nameof(statementService));
		}

		readonly IStatementService statementService;

		[Route("print/{companyPK}")]
		[HttpGet]
		public IHttpActionResult Print(Guid? companyPK)
		{
			if (companyPK == null)
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

				var printResult = statementService.Print(contactPK.Value, companyPK.Value);

				return new WebTrackerPrintActionResult(printResult);
			}
		}
	}
}
