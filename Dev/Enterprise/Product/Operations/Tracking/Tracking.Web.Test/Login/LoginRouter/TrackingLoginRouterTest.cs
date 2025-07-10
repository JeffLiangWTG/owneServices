using System;
using System.Net;
using System.Web;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.Login.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class TrackingLoginRouterTest : LoginRouterTest
	{
		public new void TestGetRoutingUrl()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Test User";
			contact.OC_Email = "tester@test.org";
			contact.OC_WebAccessEnabled = true;
			contact.SupersedeWebAccess();

			Factory.Save();

			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_PER = contact.OC_PER;
			activeContact.OC_WebAccessEnabled = true;
			Factory.Save();
			AssertEquals("Precondition", true, new OrgContactSupersededHelper(contact).HasValidContacts);

			var router = new TrackingLoginRouterForTest(new Uri("https://google.com/"), contact);
			Assert("Routing required", router.HasAnyRoutingRequired);
			var redirectUrl = router.GetRoutingUrl();
			AssertStartsWith("TrackingSupersededLoginRoutingDescriptor is triggered", "~/Login/LoginSuperseded.aspx", redirectUrl.OriginalString);

			var uriDeconstructor = new UriDeconstructor(redirectUrl);
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			var originalUrl = secureQueryString[LoginRouter.OriginalUrlQueryStringKey];
			AssertEquals("https://google.com/", originalUrl);
		}

		public override void TestGetRoutingUrlNoOriginalUrl()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Test User";
			contact.OC_Email = "tester@test.org";
			contact.OC_WebAccessEnabled = true;

			Factory.Save();

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var router = new TrackingLoginRouterForTest(null, token);
			Assert("Routing required", !router.HasAnyRoutingRequired);
			var redirectUrl = router.GetRoutingUrl();

			var uriDeconstructor = new UriDeconstructor(redirectUrl);
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			var originalUrl = secureQueryString[LoginRouter.OriginalUrlQueryStringKey];
			AssertEquals(null, originalUrl);
		}

		public void TestGetRoutingUrl_RelativePathShouldBeUsed()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Test User";
			contact.OC_Email = "tester@test.org";
			contact.OC_WebAccessEnabled = true;
			contact.SupersedeWebAccess();
			Factory.Save();

			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_PER = contact.OC_PER;
			activeContact.OC_WebAccessEnabled = true;

			Factory.Save();

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var router = new TrackingLoginRouterForTest(null, token);
			Assert("Precondition: Routing required", router.HasAnyRoutingRequired);
			var redirectUrl = router.GetRoutingUrl();
			AssertStartsWith("redirect url should use relative path", "~/Login/LoginSuperseded.aspx", redirectUrl.OriginalString);
		}

		protected override string GetErrorPage()
		{
			return new Global().ErrorPage;
		}

		protected override LoginRouter GetNewLoginRouter(Uri originalUrl, string token)
		{
			return new TrackingLoginRouter(originalUrl, token);
		}
	}
}
