using System;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class LoginRedirectionPageTest : BaseTrackingPageTest
	{
		public void TestValidContactsAndPassword()
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
			var webUserAdminManager = new WebUserAdminManager(contact);
			var message = webUserAdminManager.SetMasterPassword("Th1sIsMyPassword!", "Th1sIsMyPassword!", PasswordInstructionType.Set);
			AssertEquals("Precondition", webUserAdminManager.PasswordChangeSuccess, message);
			Factory.Save();
			Assert("Precondition", contact.Person.HasPassword);

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString
			{
				[LoginRouter.IdentityTokenQueryStringKey] = token
			};

			TestPage.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));

			TestPage.OnLoadForTest();
			Assert("Precondition", TestPage.HelperForTest.HasValidContacts);
			Assert("Login Instructions should be visible", TestPage.ContactsBoxForTest.Visible);
			Assert("Error message should not be visible", !TestPage.ErrorMessageForTest.Visible);
		}

		public void TestNoTokenInRequest()
		{
			TestPage.OnLoadForTest();
			Assert("Precondition", !TestPage.HelperForTest.HasValidContacts);
			Assert("Login Instructions should not be visible", !TestPage.ContactsBoxForTest.Visible);
			Assert("Error message should be visible", TestPage.ErrorMessageForTest.Visible);
			AssertEquals("Error message should be set",
				"You have been redirected here incorrectly. Please attempt login again and if this issue is recurring, contact your system administrator.",
				TestPage.ErrorMessageForTest.Text);
		}

		public void TestSignedInContactHasNoPersonPassword()
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
			Assert("Precondition", contact.Person.PER_PasswordHash.IsEmpty);

			TestPage.OnLoadForTest();
			Assert("Login Instructions should not be visible", !TestPage.ContactsBoxForTest.Visible);
			Assert("Error message should be visible", TestPage.ErrorMessageForTest.Visible);
			AssertEquals("Error message should be set",
				"You have been redirected here incorrectly. Please attempt login again and if this issue is recurring, contact your system administrator.",
				TestPage.ErrorMessageForTest.Text);
		}

		public void TestHasPasswordButNoLoginContacts()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SupersedeWebAccess();
			var contactPassword = "1234";
			contact.SetHashedPassword(contactPassword);
			Factory.Save();
			var webUserAdminManager = new WebUserAdminManager(contact);
			var message = webUserAdminManager.SetMasterPassword("Th1sIsMyPassword!", "Th1sIsMyPassword!", PasswordInstructionType.Set);
			AssertEquals("Precondition", webUserAdminManager.PasswordChangeSuccess, message);
			Factory.Save();
			Assert("Precondition", contact.Person.HasPassword);

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString
			{
				[LoginRouter.IdentityTokenQueryStringKey] = token
			};

			TestPage.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));

			TestPage.OnLoadForTest();
			Assert("Precondition", !TestPage.HelperForTest.HasValidContacts);
			Assert("Login Instructions should not be visible", !TestPage.ContactsBoxForTest.Visible);
			Assert("Error message should be visible", TestPage.ErrorMessageForTest.Visible);
			AssertEquals("Error message should be set",
				"You have been redirected here incorrectly. Please attempt login again and if this issue is recurring, contact your system administrator.",
				TestPage.ErrorMessageForTest.Text);
		}

		#region Implementation

		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.LoginRedirection;
		}

		new LoginRedirectionForTest TestPage
		{
			get { return base.TestPage as LoginRedirectionForTest; }
		}

		protected override Control GetNewControl()
		{
			var testPage = new LoginRedirectionForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics",
				BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				new Type[] { typeof(HttpContext) },
				null);
			method.Invoke(testPage, new object[] { HttpContext.Current });
			return testPage;
		}

		#endregion
	}
}
