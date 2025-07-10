using System.Net;
using System.Web.Http;

namespace Enterprise.Services.Scim.Api.Controllers
{
	public class WtgController : ApiController
	{
		[AllowAnonymous]
		[AcceptVerbs(new[] { "GET", "HEAD" })]
		[Route("wtg/status")]
		public IHttpActionResult Status()
		{
			return StatusCode(HttpStatusCode.OK);
		}
	}
}
