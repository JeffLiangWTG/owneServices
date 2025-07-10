using System.Threading;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost
{
	[RoutePrefix("api/performanceReporting")]
	[GlowTicketAuthentication]
	public sealed class PerformanceReportingController : ApiController
	{
		[Route("url")]
		[HttpGet]
		public IHttpActionResult GetUrl(string report, CancellationToken ct)
		{
			if (string.IsNullOrEmpty(report))
			{
				return BadRequest();
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = GetPerformanceReportingUrlResult(report, ct);

				return Json(result);
			}
		}

		PerformanceReportingUrlResult GetPerformanceReportingUrlResult(string report, CancellationToken ct)
		{
			var factory = GetNewBusinessObjectFactory();
			var contact = GetContact(factory);
			var generator = ObjectFactory.Get<IPerformanceReportingUrlGenerator>();
			var correlationID = ZGuid.NewZGuid().ToString();
			var token = generator.GetToken(correlationID, contact, ct);

			if (!string.IsNullOrEmpty(token.ErrorMessage))
			{
				return new PerformanceReportingUrlResult
				{
					Errors = new[] { token.ErrorMessage },
				};
			}

			var path = $"reports/{report}";
			var (url, errorMessage) = generator.Generate(path, token.Token, true);
			if (url == null)
			{
				return new PerformanceReportingUrlResult
				{
					Errors = new[] { errorMessage }
				};
			}

			return new PerformanceReportingUrlResult
			{
				Url = url.ToString()
			};
		}

		OrgContact GetContact(BusinessObjectFactory factory)
		{
			var identity = User?.Identity as IGlowAuthenticationTicketIdentity;

			return identity?.GetContact(factory);
		}

		static BusinessObjectFactory GetNewBusinessObjectFactory() => new BusinessObjectFactory() { NameForDebugging = nameof(PerformanceReportingController) };
	}
}
