using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.Tracking.Web
{
	public abstract class RoutingEnabledPage : BasePage
	{
		protected void RedirectViaLoginRouter(string originalUrl, OrgContact contact)
		{
			var originalUri = string.IsNullOrEmpty(originalUrl) ? null : new Uri(originalUrl, UriKind.RelativeOrAbsolute);
			RedirectViaLoginRouter(originalUri, contact);
		}

		protected void RedirectViaLoginRouter(Uri originalUrl, OrgContact contact)
		{
			var router = new TrackingLoginRouter(originalUrl, contact);
			var redirectUrl = router.GetRoutingUrl();
			Response.Redirect(redirectUrl.IsAbsoluteUri ? redirectUrl.AbsoluteUri : redirectUrl.OriginalString);
		}

		protected LoginRouterIdentityManager IdentityManager
		{
			get
			{
				if (identityManager == null)
				{
					var token = LoginRouter.GetIdentityTokenFromRequest(Request);
					identityManager = new LoginRouterIdentityManager(new BusinessObjectFactory());
					identityManager.PopulatePropertiesFromToken(token);
				}

				return identityManager;
			}
		}

		LoginRouterIdentityManager identityManager;
	}
}
