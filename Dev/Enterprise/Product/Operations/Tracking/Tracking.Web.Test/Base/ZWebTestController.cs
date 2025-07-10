using System.Web;
using Enterprise.Tracking.Web.Base;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ZWebTestController : ZWebController
	{
		public ZWebTestController(HttpContext context)
			: base(context) { }

		public string LastRedirectUrl { get; set; }

		protected override void RedirectToTrackingPageCore(string targetUrl)
		{
			LastRedirectUrl = targetUrl;
		}
	}
}
