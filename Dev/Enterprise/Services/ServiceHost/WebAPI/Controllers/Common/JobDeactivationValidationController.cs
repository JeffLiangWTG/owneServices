using System;
using System.Web.Http;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class JobDeactivationValidationController : ApiController
	{
		[Route("api/Common/JobDeactivationValidation/GetCanCancelJob/")]
		[HttpGet]
		public IHttpActionResult GetCanCancelJob([FromUri] string controllerID, [FromUri] Guid[] jobPK)
		{
			var service = new JobDeactivationValidationService(this);

			return service.TryGetCanCancelJob(controllerID, jobPK, out var results, out var failedResult)
				? Json(results)
				: failedResult;
		}
	}
}
