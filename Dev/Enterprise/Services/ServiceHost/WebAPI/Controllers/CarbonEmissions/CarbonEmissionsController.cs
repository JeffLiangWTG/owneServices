using System.Web.Http;
using CargoWise.Bi.Common;
using CargoWise.Data;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	[RoutePrefix("api/emissions")]
	public class CarbonEmissionsController : ApiController
	{
		[Route("edwreadiness")]
		[HttpGet]
		public IHttpActionResult GetEDWReadiness()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(string.IsNullOrEmpty(BiServiceTaskHelpers.IsEdwEnabled()));
			}
		}
	}
}
