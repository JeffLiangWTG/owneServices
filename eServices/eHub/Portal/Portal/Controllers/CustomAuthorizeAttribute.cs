using System.Web;
using System.Web.Mvc;

namespace CargoWise.eHub.Portal.Controllers
{
	public class CustomAuthorizeAttribute : AuthorizeAttribute
	{
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			if (httpContext.Request.Url.IsLoopback)
			{
				return true;
			}

			return base.AuthorizeCore(httpContext);
		}
	}
}
