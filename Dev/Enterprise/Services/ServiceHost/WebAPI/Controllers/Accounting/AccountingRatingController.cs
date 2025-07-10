#if NETFRAMEWORK
using System.Web.Http;
using IActionResult = System.Web.Http.IHttpActionResult;
using RouteAttribute = System.Web.Http.RouteAttribute;
#elif NET
using Enterprise.Services.ServiceHost.NetCore;
#endif
using System;
using CargoWise.Data;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
#if NETFRAMEWORK
	[RoutePrefix("api/autorating")]
#elif NET
	[Route("api/autorating")]
#endif
	public class AccountingRatingController : ControllerBase
	{
		readonly IAccountingRatingService ratingService;

#if NETFRAMEWORK
		public AccountingRatingController() : this(new AccountingRatingService())
		{
		}
#endif

		public AccountingRatingController(IAccountingRatingService ratingService)
		{
			this.ratingService = ratingService ?? throw new ArgumentNullException(nameof(ratingService));
		}

		[Route("autorateandcreatejobheader/{operationsJobPk}/{operationsJobTableCode}/{localClientPk}")]
		[HttpPost]
		[HttpGet]
		public IActionResult AutoRateAndCreateJobHeader(Guid operationsJobPk, string operationsJobTableCode, Guid localClientPk, [FromQuery] bool autoRateRevenue = true, [FromQuery] bool autoRateCosts = true, [FromQuery] LogType maxLogLevel = LogType.Information, [FromQuery] Guid? branchPK = null)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var results = ratingService.AutoRateAndCreateJobHeader(operationsJobPk, operationsJobTableCode, Env.CurrentUserPK, branchPK ?? Env.CurrentBranchPK, Env.CurrentDepartmentPK, localClientPk, autoRateRevenue, autoRateCosts, maxLogLevel: maxLogLevel);
				var restrictedResults = Restrict(results);
				return Ok(restrictedResults);
			}
		}

		[Route("searchforrates/{operationsJobPk}/{operationsJobTableCode}/{localClientPk}")]
		[HttpPost]
		[HttpGet]
		public IActionResult SearchForRates(Guid operationsJobPk, string operationsJobTableCode, Guid localClientPk, [FromQuery] bool autoRateRevenue = true, [FromQuery] bool autoRateCosts = true, [FromQuery] LogType maxLogLevel = LogType.Information, [FromQuery] Guid? branchPK = null)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var results = ratingService.SearchForRates(operationsJobPk, operationsJobTableCode, Env.CurrentUserPK, branchPK ?? Env.CurrentBranchPK, Env.CurrentDepartmentPK, localClientPk, autoRateRevenue, autoRateCosts, maxLogLevel: maxLogLevel);
				var restrictedResults = Restrict(results);
				return Ok(restrictedResults);
			}
		}

		bool isSuccessful(RatingResults results)
		{
			if (results.Logs != null)
			{
				foreach (string log in results.Logs)
				{
					if (log.StartsWith((NoResString)"Error: ", StringComparison.Ordinal))
					{
						return false;
					}
				}
			}
			return true;
		}
		RestrictedRatingResults Restrict(RatingResults results)
		{
			var restrictedResults = new RestrictedRatingResults();
			restrictedResults.Successful = isSuccessful(results);
			if (User?.Identity is IGlowAuthenticationTicketIdentity identity && identity.IsStaff())
			{
				restrictedResults.Results = results;
			}
			return restrictedResults;
		}
	}

	public class RestrictedRatingResults
	{
		[JsonProperty(nameof(Successful))]
		public bool Successful { get; set; }

		[JsonProperty(nameof(Results))]
		public RatingResults Results { get; set; }
	}
}
