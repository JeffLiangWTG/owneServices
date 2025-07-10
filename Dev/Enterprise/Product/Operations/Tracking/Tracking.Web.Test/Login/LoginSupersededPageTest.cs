using System;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class LoginSupersededPageTest : BaseTrackingPageTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.LoginSuperseded;
		}

		new LoginSupersededForTest TestPage
		{
			get { return base.TestPage as LoginSupersededForTest; }
		}

		protected override Control GetNewControl()
		{
			var testPage = new LoginSupersededForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics",
				BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				new Type[] { typeof(HttpContext) },
				null);
			method.Invoke(testPage, new object[] { HttpContext.Current });
			return testPage;
		}

		public void TestValidContacts()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SupersedeWebAccess();
			var contactPassword = "1234";
			contact.SetHashedPassword(contactPassword);
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			Factory.Save();

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString
			{
				[LoginRouter.IdentityTokenQueryStringKey] = token
			};

			TestPage.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));

			TestPage.OnLoadForTest();
			Assert("Precondition", TestPage.HelperForTest.HasValidContacts);
			Assert("Email Verification Instructions should be visible", TestPage.SupersededInstructionsLabelForTest.Visible);
			Assert("Button should be visible", TestPage.SetMasterPasswordButtonForTest.Visible);
			Assert("Error message should not be visible", !TestPage.ErrorMessageForTest.Visible);
		}

		public void TestInvalidTokenQueryString()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SupersedeWebAccess();
			var contactPassword = "1234";
			contact.SetHashedPassword(contactPassword);
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			Factory.Save();

			var secureQueryString = new SecureQueryString
			{
				[LoginRouter.IdentityTokenQueryStringKey] = "dud"
			};

			TestPage.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));

			TestPage.OnLoadForTest();
			Assert("Precondition", !TestPage.HelperForTest.HasValidContacts);
			Assert("Email Verification Instructions should not be visible", !TestPage.SupersededInstructionsLabelForTest.Visible);
			Assert("Button should not be visible", !TestPage.SetMasterPasswordButtonForTest.Visible);
			Assert("Error message should be visible", TestPage.ErrorMessageForTest.Visible);
			AssertEquals("Error message should be set",
				"You have been redirected here because your account was deactivated. However, the link was invalid. Please attempt login again and if this issue is recurring, contact your system administrator.",
				TestPage.ErrorMessageForTest.Text);
		}

		public void TestInvalidQueryString()
		{
			TestPage.OnLoadForTest();
			Assert("Precondition", !TestPage.HelperForTest.HasValidContacts);
			Assert("Email Verification Instructions should not be visible", !TestPage.SupersededInstructionsLabelForTest.Visible);
			Assert("Button should not be visible", !TestPage.SetMasterPasswordButtonForTest.Visible);
			Assert("Error message should be visible", TestPage.ErrorMessageForTest.Visible);
			AssertEquals("Error message should be set",
				"You have been redirected here because your account was deactivated. However, the link was invalid. Please attempt login again and if this issue is recurring, contact your system administrator.",
				TestPage.ErrorMessageForTest.Text);
		}

		public void TestSetMasterPasswordButton()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "shame@game.com";
			contact.OC_WebAccessEnabled = true;
			contact.SupersedeWebAccess();
			var contactPassword = "1234";
			contact.SetHashedPassword(contactPassword);
			Factory.Save();

			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_Email = "blame@game.com";
			contact2.OC_WebAccessEnabled = true;
			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_PER = contact.OC_PER;
			contact3.OC_Email = "tame@game.com";
			contact3.OC_WebAccessEnabled = true;
			Factory.Save();

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var returnUrl = "https://google.com/";

			var secureQueryString = new SecureQueryString
			{
				[LoginRouter.IdentityTokenQueryStringKey] = token,
				[LoginRouter.OriginalUrlQueryStringKey] = returnUrl
			};

			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);

			TestPage.OnLoadForTest();
			TestPage.SetMasterPasswordButton_ClickForTest();

			var query = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.SetMasterPassword);
			var accessToken = Factory.LoadTop1<StmAccessToken>(query);

			AssertEquals("Parent should be the superseded contact", contact.PK, accessToken.SAT_ParentId);
			AssertEquals("Should be OrgContact", OrgContactSchema.Constants.Prefix, accessToken.SAT_ParentTableCode);
			AssertEquals("Scope should contain original url from query string", returnUrl, accessToken.SAT_Scope);
			AssertStartsWith("Should be redirected to set master password", "/webapp/Admin/SetMasterPassword", TestPage.Response.RedirectLocation);
		}
	}
}
