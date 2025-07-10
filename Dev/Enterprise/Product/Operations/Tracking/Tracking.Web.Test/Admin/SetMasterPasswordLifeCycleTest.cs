using System;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class SetMasterPasswordLifeCycleTest : ZPageLifeCycleTest
	{
		protected override ZPage GetNewPage()
		{
			return SetMasterPasswordPage;
		}

		public void TestPageLoad()
		{
			AssertPageLoad(AssertOnLoadComplete_WithValidToken, TestToken);
		}

		public void TestPageLoadTokenExpired()
		{
			AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, TestToken_Expired);
		}

		public void TestPageLoadTokenInvalidType()
		{
			AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, TestToken_InvalidType);
		}

		public void TestPageLoadTokenInvalidToken()
		{
			AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, "invalidToken");
		}

		public void TestPageLoadTokenEmptyToken()
		{
			AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, "");
		}

		void AssertPageLoad(EventHandler onLoadCompleteHandler, string token)
		{
			HttpContext.Current.Request.QueryString.Remove(SecureQueryString.QueryStringKey);
			var passwordSecureQueryString = new SecureQueryString { { WebUserAdminManager.SetMasterPasswordKey, token } };
			HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, WebUtility.UrlEncode(passwordSecureQueryString.ToString()));

			AssertOnLoadComplete += onLoadCompleteHandler;
			try
			{
				RunPageLifeCycle();
			}
			finally
			{
				AssertOnLoadComplete -= onLoadCompleteHandler;
			}
		}

		public void TestPageLoadNonSecureToken()
		{
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			Factory.Save();

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://google.com.au/");

			using (var page = SetMasterPasswordPage)
			{
				page.RequestQueryString_Expose.Remove(WebUserAdminManager.SetMasterPasswordKey);
				HttpContext.Current.Request.QueryString.Add(WebUserAdminManager.SetMasterPasswordKey, TestToken_Expired);

				page.OnLoad();
				AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
			}

			using (var page = SetMasterPasswordPage)
			{
				page.RequestQueryString_Expose.Remove(WebUserAdminManager.SetMasterPasswordKey);
				HttpContext.Current.Request.QueryString.Add(WebUserAdminManager.SetMasterPasswordKey, TestToken);

				page.OnLoad();
				Assert(page.SetMasterPasswordHeadingLabel_Expose.Visible);
				Assert(page.NewPassword_Expose.Visible);
				Assert(page.NewPasswordConfirm_Expose.Visible);
				AssertNullOrEmpty(page.PasswordChangeMessage_Expose.Text);
			}
		}

		public void TestSetMasterPasswordDoesNotThrowExceptionWhenContactIsInvalid()
		{
			using (var page = SetMasterPasswordPage)
			{
				var accessControl = new TokenizedAccessControl();
				TestToken = accessControl.CreateLimitedToken(AccessTokenTypes.SetMasterPassword, new AccessTokenInfo(RedirectUrl, Guid.NewGuid(), "OC"), maxUses: 1);

				var passwordSecureQueryString = new SecureQueryString { { WebUserAdminManager.SetMasterPasswordKey, TestToken } };
				HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, WebUtility.UrlEncode(passwordSecureQueryString.ToString()));

				AssertNoExceptionThrown(page.OnUpdate);
			}
		}

		public void TestSetMasterPassword()
		{
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://google.com.au/");

			using (var page = SetMasterPasswordPage)
			{
				page.RequestQueryString_Expose.Remove(SecureQueryString.QueryStringKey);
				var passwordSecureQueryString = new SecureQueryString { { WebUserAdminManager.SetMasterPasswordKey, "invalidToken" } };
				HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, WebUtility.UrlEncode(passwordSecureQueryString.ToString()));

				page.OnUpdate();
				AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);

				page.RequestQueryString_Expose.Remove(SecureQueryString.QueryStringKey);
				passwordSecureQueryString = new SecureQueryString { { WebUserAdminManager.SetMasterPasswordKey, TestToken } };
				HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, WebUtility.UrlEncode(passwordSecureQueryString.ToString()));

				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "2";

				AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);
				page.OnUpdate();

				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(accessControl.TryPeek(TestToken, AccessTokenTypes.SetMasterPassword, out _));

				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.OnUpdate();
				AssertEquals("Personal password should be updated", true, TestOrgContact.Person.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				Assert(page.Back_Expose.Visible);
				Assert(!accessControl.TryPeek(TestToken, AccessTokenTypes.SetMasterPassword, out _));
				var router = new TrackingLoginRouter(new Uri(RedirectUrl), TestOrgContact);
				AssertEquals("Precondition", false, router.HasAnyRoutingRequired);
				AssertEquals("Should have been routed through Login Router", "/webapp/Login/LoginComplete.aspx?",
					page.Response.RedirectLocation.Substring(0, page.Response.RedirectLocation.IndexOf(LoginRouter.QueryStringKey)));

				page.OnLoad();
				AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
			}
		}
		public void TestSetMasterPassword_NoRedirectUrl()
		{
			using (var page = SetMasterPasswordPage)
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Code = "TechDeckz";
				orgHeader.OH_FullName = "Tech Decks";

				TestOrgContact = orgHeader.Contacts.AddNew();
				TestOrgContact.OC_Email = "TestUser@tekdekz.com";
				TestOrgContact.OC_IsActive = true;
				TestOrgContact.OC_WebAccessEnabled = true;
				Factory.Save();
				TestOrgContact.Person.PER_FullName = "Pranky";
				Factory.Save();

				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				var testToken = accessControl.CreateLimitedToken(AccessTokenTypes.SetMasterPassword, new AccessTokenInfo(string.Empty, TestOrgContact.PK.ToGuid(), "OC"), maxUses: 1);

				page.RequestQueryString_Expose.Remove(SecureQueryString.QueryStringKey);
				var passwordSecureQueryString = new SecureQueryString { { WebUserAdminManager.SetMasterPasswordKey, testToken } };
				HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, WebUtility.UrlEncode(passwordSecureQueryString.ToString()));

				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				AssertEquals("Precondition: Should not be logged in", false, page.SiteUser.IsLoggedIn);

				page.OnUpdate();
				AssertEquals("Personal password should be updated", true, TestOrgContact.Person.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				Assert(!accessControl.TryPeek(testToken, AccessTokenTypes.SetMasterPassword, out _));
				AssertStartsWith("Should redirect to LoginComplete", "/webapp/Login/LoginComplete.aspx", page.Response.RedirectLocation);
				var uriDeconstructor = new UriDeconstructor(new Uri(page.Response.RedirectLocation, UriKind.Relative));
				var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
				var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
				var originalUrl = secureQueryString[LoginRouter.OriginalUrlQueryStringKey];
				AssertEquals("Should not have an original redirect url", null, originalUrl);
				AssertEquals("Should not have been logged in", false, page.SiteUser.IsLoggedIn);
				Assert("Password controls should be hidden", !page.SetTable_Expose.Visible);

				page.OnLoad();
				AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
			}
		}

		public void TestSetMasterPassword_AlreadyLoggedIn()
		{
			using (var page = SetMasterPasswordPage)
			{
				WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://google.com.au/");

				page.RequestQueryString_Expose.Remove(SecureQueryString.QueryStringKey);
				var passwordSecureQueryString = new SecureQueryString { { WebUserAdminManager.SetMasterPasswordKey, TestToken } };
				HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, WebUtility.UrlEncode(passwordSecureQueryString.ToString()));

				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.SiteUser.Login(TestOrgContact.OrgCode, TestOrgContact.OC_Email, "1234");
				AssertEquals("Precondition: Should be logged in", true, page.SiteUser.IsLoggedIn);

				page.OnUpdate();
				AssertEquals("Personal password should be updated", true, TestOrgContact.Person.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(!accessControl.TryPeek(TestToken, AccessTokenTypes.SetMasterPassword, out _));
				AssertStartsWith("Should have redirected", RedirectUrl, page.Response.RedirectLocation);
				AssertEquals("Should remain logged in", true, page.SiteUser.IsLoggedIn);
				Assert("Password controls should be hidden", !page.SetTable_Expose.Visible);

				page.OnLoad();
				AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
			}
		}

		public void TestSetMasterPassword_AlreadyLoggedIn_NoRedirectUrl()
		{
			using (var page = SetMasterPasswordPage)
			{
				WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://google.com.au/");

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Code = "TechDeckz";
				orgHeader.OH_FullName = "Tech Decks";

				TestOrgContact = orgHeader.Contacts.AddNew();
				TestOrgContact.OC_Email = "TestUser@tekdekz.com";
				TestOrgContact.OC_IsActive = true;
				TestOrgContact.OC_WebAccessEnabled = true;
				TestOrgContact.SetHashedPassword("1234");
				Factory.Save();
				TestOrgContact.Person.PER_FullName = "Pranky";
				Factory.Save();

				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				var testToken = accessControl.CreateLimitedToken(AccessTokenTypes.SetMasterPassword, new AccessTokenInfo(string.Empty, TestOrgContact.PK.ToGuid(), "OC"), maxUses: 1);

				page.RequestQueryString_Expose.Remove(SecureQueryString.QueryStringKey);
				var passwordSecureQueryString = new SecureQueryString { { WebUserAdminManager.SetMasterPasswordKey, testToken } };
				HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, WebUtility.UrlEncode(passwordSecureQueryString.ToString()));

				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.SiteUser.Login(TestOrgContact.OrgCode, TestOrgContact.OC_Email, "1234");
				AssertEquals("Precondition: Should be logged in", true, page.SiteUser.IsLoggedIn);

				page.OnUpdate();
				AssertEquals("Personal password should be updated", true, TestOrgContact.Person.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				Assert(!accessControl.TryPeek(testToken, AccessTokenTypes.SetMasterPassword, out _));
				AssertEquals("Should not have redirected", null, page.Response.RedirectLocation);
				AssertEquals("Should remain logged in", true, page.SiteUser.IsLoggedIn);
				Assert("Password controls should be hidden", !page.SetTable_Expose.Visible);

				page.OnLoad();
				AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
			}
		}

		public void TestSetMasterPassword_SiteUserAlreadyLoggedIn()
		{
			using (var page = SetMasterPasswordPage)
			{
				page.RequestQueryString_Expose.Remove(SecureQueryString.QueryStringKey);
				var passwordSecureQueryString = new SecureQueryString { { WebUserAdminManager.SetMasterPasswordKey, "invalidToken" } };
				HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, WebUtility.UrlEncode(passwordSecureQueryString.ToString()));

				page.OnUpdate();
				AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);

				page.RequestQueryString_Expose.Remove(SecureQueryString.QueryStringKey);
				passwordSecureQueryString = new SecureQueryString { { WebUserAdminManager.SetMasterPasswordKey, TestToken } };
				HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, WebUtility.UrlEncode(passwordSecureQueryString.ToString()));

				page.OnLoad();
				page.SiteUser.LoginForTest(TestOrgContact.OrgCode, TestOrgContact.OC_Email, "1234");
				Assert("Precondition", page.SiteUser.IsLoggedIn);
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.OnUpdate();
				AssertEquals("Personal password should be updated", true, TestOrgContact.Person.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				Assert(page.Back_Expose.Visible);
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(!accessControl.TryPeek(TestToken, AccessTokenTypes.SetMasterPassword, out _));
				AssertEquals("Should remain logged in", true, page.SiteUser.IsLoggedIn);
				AssertEquals("Should have redirected", RedirectUrl, page.Response.RedirectLocation);

				page.OnLoad();
				AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
			}
		}

		public void TestOnInit_LogOff()
		{
			using (var page = SetMasterPasswordPage)
			{
				page.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
				page.AppInstance.ApplicationCookie.WriteCookie("cookie");

				page.IsPostBack = true;
				page.OnInit();

				Assert(page.SiteUser.IsLoggedIn);
				Assert(page.AppInstance.ApplicationCookie.CookieExist());

				page.IsPostBack = false;
				page.OnInit();

				Assert(!page.SiteUser.IsLoggedIn);
				Assert(!page.AppInstance.ApplicationCookie.CookieExist());
			}
		}

		void AssertOnLoadComplete_WithValidToken(object sender, EventArgs e)
		{
			var testPage = (SetMasterPasswordForTest)this.TestPage;
			Assert(testPage.SetTable_Expose.Visible);
			AssertNullOrEmpty(testPage.PasswordChangeMessage_Expose.Text);
		}

		void AssertOnLoadComplete_WithInvalidToken(object sender, EventArgs e)
		{
			var testPage = (SetMasterPasswordForTest)this.TestPage;
			CombineAssertions(() =>
			{
				Assert("Should not see set password controls", !testPage.SetTable_Expose.Visible);
				AssertEquals("The set link you have followed is invalid or expired.", testPage.PasswordChangeMessage_Expose.Text);
				AssertEquals(string.Empty, testPage.SetMasterPasswordHeadingLabel_Expose.Text);
			});
		}

		SetMasterPasswordForTest SetMasterPasswordPage
		{
			get
			{
				var testPage = new SetMasterPasswordForTest();
				var method = typeof(Page).GetMethod("SetIntrinsics",
					BindingFlags.NonPublic | BindingFlags.Instance,
					null,
					new Type[] { typeof(HttpContext) },
					null);
				method.Invoke(testPage, new object[] { HttpContext.Current });

				return testPage;
			}
		}

		OrgContact TestOrgContact;

		string TestToken;
		string TestToken_Expired;
		string TestToken_InvalidType;
		const string RedirectUrl = "http://google.com.au/";

		protected override void SetUp()
		{
			base.SetUp();

			TestToken = "testToken";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "WiseTech";
			orgHeader.OH_FullName = "WiseTech Global";

			TestOrgContact = orgHeader.Contacts.AddNew();
			TestOrgContact.OC_Email = "TestUser@wisetechglobal.com";
			TestOrgContact.OC_IsActive = true;
			TestOrgContact.OC_WebAccessEnabled = true;
			TestOrgContact.SetHashedPassword("1234");
			Factory.Save();
			TestOrgContact.Person.PER_FullName = "Franky";
			Factory.Save();

			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			TestToken = accessControl.CreateLimitedToken(AccessTokenTypes.SetMasterPassword, new AccessTokenInfo(RedirectUrl, TestOrgContact.PK.ToGuid(), "OC"), maxUses: 1);
			TestToken_Expired = accessControl.CreateLimitedToken(AccessTokenTypes.SetMasterPassword, new AccessTokenInfo(RedirectUrl, TestOrgContact.PK.ToGuid(), "OC"), TimeSpan.FromHours(-1), 1);
			TestToken_InvalidType = accessControl.CreateLimitedToken("INV", new AccessTokenInfo("", Guid.Empty, "INV"), maxUses: 1);
		}
	}
}
