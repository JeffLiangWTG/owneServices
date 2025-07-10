using System;
using System.Web;
using System.Web.Security;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class ZWebControllerNoUserTest : TestCase, IHttpContextEnabledTestWithAppInstance
	{
		public void TestRedirectToTrackingPageCore_NoUser()
		{
			var controller = new ZWebTestController(HttpContext.Current);
			var targetUrl = "RedirectToTrackingPageCore_WithoutSiteUser_RequiresLogin";

			controller.RedirectToTrackingPageCore(Guid.NewGuid(), targetUrl);

			var expectedUrl = string.Format("{0}?ReturnUrl={1}", FormsAuthentication.LoginUrl, AppInstance.Server.UrlEncode(targetUrl));
			AssertEquals(expectedUrl, controller.LastRedirectUrl);
		}

		#region IHttpContextEnabledTestWithAppInstance Members

		ZEnterpriseGlobalBase IHttpContextEnabledTestWithAppInstance.AppInstance => new GlobalForTestNoUser();

		GlobalForTestNoUser AppInstance
		{
			get { return (GlobalForTestNoUser)HttpContext.Current.ApplicationInstance; }
		}

		class GlobalForTestNoUser : Global
		{
			public override WebUser SiteUser => null;
		}

		#endregion
	}
}
