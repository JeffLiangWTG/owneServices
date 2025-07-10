using System.Net;
using System.Web.Http;
using Enterprise.Environment;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	[RoutePrefix("api/SecurityCheckpoint")]
	public class SecurityCheckpointController : ApiController
	{
		public SecurityCheckpointController()
		{
		}

		[Route("isallowed/{checkpointName}")]
		[HttpGet]
		[HttpPost]
		public IHttpActionResult GetSecurityCheckpointIsAllowed(string checkpointName)
		{
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var checkpoint = Env.Security.FindCheckPoint(checkpointName);
				if (checkpoint == null) // Checkpoint not found
				{
					return NotFound();
				}
				if (checkpoint.IsAllowed)
				{
					return Ok();
				}
				return StatusCode(HttpStatusCode.Forbidden);
			}
		}
	}
}
