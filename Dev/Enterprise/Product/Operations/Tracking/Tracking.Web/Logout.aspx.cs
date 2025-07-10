using System;
using System.Web;
using System.Web.Security;

namespace Enterprise.Tracking.Web
{
	public partial class Logout : BasePage
	{
		protected override void OnInit(EventArgs e)
		{
			FormsAuthentication.SignOut();
			if (SiteUser != null && SiteUser.IsLoggedIn)
			{
				SiteUser.Logout();
			}

			var queryString = HttpContext.Current.Request.QueryString.Count > 0 ? $"?{HttpContext.Current.Request.QueryString}" : string.Empty; // Url query string
			HttpContext.Current.Response.Redirect($"{AppInstance.LoginPage}{queryString}", true); // Url string
		}
	}
}
