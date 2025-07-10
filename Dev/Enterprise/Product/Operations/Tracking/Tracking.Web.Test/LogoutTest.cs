using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class LogoutTest : TestCaseWithFactory
	{
		public void TestUserIsLoggedOut()
		{
			var testOrg = OrgHeader.LoadFromCode(Factory, "DEMORG");
			AssertNotNull("testOrg", testOrg);
			testPage.SiteUser.LoginSupportForTest("DEMORG");
			Assert(testPage.SiteUser.IsLoggedIn);

			testPage.OnInitForTest();
			Assert(!testPage.SiteUser.IsLoggedIn);
		}

		public void TestFormsAuthenticationCookieExpired()
		{
			AssertEquals(DateTime.MinValue, testPage.Response.Cookies.Get(FormsAuthentication.FormsCookieName).Expires);

			testPage.OnInitForTest();
			var authCookie = testPage.Response.Cookies.Get(FormsAuthentication.FormsCookieName);
			var authCookieExpired = DateTime.MinValue != authCookie.Expires && authCookie.Expires < ZDateTime.Now;
			Assert(authCookieExpired);
		}

		public void TestRedirectToLoginPage()
		{
			testPage.OnInitForTest();
			AssertEquals(testPage.AppInstance.LoginPage, HttpContext.Current.Response.RedirectLocation);
		}

		public void TestCookieRemovedWhenClearSaved()
		{
			testPage.RequestQueryString_Expose.Add("ClearSaved", "1");

			testPage.OnInitForTest();
			AssertEquals($"{testPage.AppInstance.LoginPage}?ClearSaved=1", HttpContext.Current.Response.RedirectLocation);
		}

		public void TestCookieNotRemovedWhenNoClearSaved()
		{
			Assert(testPage.AppInstance.ApplicationCookie.CookieExist());

			testPage.OnInitForTest();
			Assert(testPage.AppInstance.ApplicationCookie.CookieExist());
		}

		protected override void SetUp()
		{
			testPage = new LogoutForTest();
			testPage.SetupPageForTesting();
		}

		LogoutForTest testPage;

		class LogoutForTest : Logout
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}

			public void OnInitForTest()
			{
				base.OnInit(EventArgs.Empty);
			}

			public void SetupPageForTesting()
			{
				var method = typeof(Page).GetMethod("SetIntrinsics",
					BindingFlags.NonPublic | BindingFlags.Instance,
					null,
					new Type[] { typeof(HttpContext) },
					null);
				method.Invoke(this, new object[] { HttpContext.Current });

				FormsAuthentication.SetAuthCookie("homer", false);
				this.AppInstance.ApplicationCookie.WriteCookie("meh");
				this.SiteUser.LoginSupportForTest();
			}

			public NameValueCollection RequestQueryString_Expose => base.RequestQueryString;
		}
	}
}
