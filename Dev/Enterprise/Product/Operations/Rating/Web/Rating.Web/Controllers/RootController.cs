using System;
using System.Web.Http;
using System.Web.Http.Description;

namespace Enterprise.Rating.Web.Controllers
{
	[RoutePrefix("")]
	public class RootController : ApiController
	{
		[HttpGet]
		[Route("")]
		[ApiExplorerSettings(IgnoreApi = true)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "URI path")]
		public IHttpActionResult Get()
		{
			var swaggerDocsRoot = new Uri(Url.Content("~/swagger/ui/index"), UriKind.Absolute);
			return Redirect(swaggerDocsRoot);
		}
	}
}
