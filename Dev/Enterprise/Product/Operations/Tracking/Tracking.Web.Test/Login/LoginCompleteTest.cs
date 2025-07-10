using System;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class LoginCompleteTest : TestCaseWithFactory
	{
		public void TestPageLoad()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "DSAJNBF";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");

			Factory.Save();

			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString
			{
				[LoginRouter.IdentityTokenQueryStringKey] = token,
				[LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/"
			};

			var page = GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));

			page.DoPageLoad();

			AssertEquals("http://test.org/", page.Response.RedirectLocation);
			AssertEquals(true, page.SiteUser.IsLoggedIn);
			AssertEquals(contact.PK, page.SiteUser.LoggedInOrgContact.PK);
			AssertNotEquals(string.Empty, page.Response.Cookies[".ASPXAUTH"]?.Value ?? string.Empty);
			AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
		}

		public void TestPageLoad_TermsAndConditions()
		{
			using (WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Some Terms and Conditions"))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "DSAJNBF";
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "User One";
				contact.OC_Email = "user.one@test.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");

				Factory.Save();

				contact.Person.PER_EmailAddress = contact.OC_Email;
				Factory.Save();

				var token = LoginRouterIdentityManager.GenerateToken(contact);
				var secureQueryString = new SecureQueryString
				{
					[LoginRouter.IdentityTokenQueryStringKey] = token,
					[LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/"
				};

				var page = GetPageForTest();
				page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));

				page.DoPageLoad();

				var expectedUrl = string.Format("{0}?{1}={2}", page.AppInstance.TermsAndConditionsPage, Global.TermsAndConditionsRedirectUrlTag, WebUtility.HtmlEncode("http://test.org/"));
				AssertEquals(expectedUrl, page.Response.RedirectLocation);
				AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
			}
		}

		public void TestPageLoad_TermsAndConditions_AlreadySigned()
		{
			using (WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Some Terms and Conditions"))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "DSAJNBF";
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "User One";
				contact.OC_Email = "user.one@test.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");
				contact.OC_WebContractSignedDate = ZDateTime.Now;

				Factory.Save();

				contact.Person.PER_EmailAddress = contact.OC_Email;
				Factory.Save();

				var token = LoginRouterIdentityManager.GenerateToken(contact);
				var secureQueryString = new SecureQueryString
				{
					[LoginRouter.IdentityTokenQueryStringKey] = token,
					[LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/"
				};

				var page = GetPageForTest();
				page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));

				page.DoPageLoad();

				AssertEquals("http://test.org/", page.Response.RedirectLocation);
				AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
			}
		}

		public void TestPageLoad_TermsAndConditions_Updated()
		{
			using (WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Some Terms and Conditions"))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "DSAJNBF";
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "User One";
				contact.OC_Email = "user.one@test.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");
				contact.OC_WebContractSignedDate = ZDateTime.Now.AddDays(-1);

				var logQuery = new ZQuery(StmDataSchema.SD_Name, WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.Name);
				var stmData = Factory.LoadTop1<StmData>(logQuery);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				stmData.Logs.AddNew(AutoEvents.EditedARecord, ZDateTimeOffset.Now);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Factory.Save();

				contact.Person.PER_EmailAddress = contact.OC_Email;
				Factory.Save();

				var token = LoginRouterIdentityManager.GenerateToken(contact);
				var secureQueryString = new SecureQueryString
				{
					[LoginRouter.IdentityTokenQueryStringKey] = token,
					[LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/"
				};

				var page = GetPageForTest();
				page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));

				page.DoPageLoad();

				var expectedUrl = string.Format("{0}?{1}={2}", page.AppInstance.TermsAndConditionsPage, Global.TermsAndConditionsRedirectUrlTag, WebUtility.HtmlEncode("http://test.org/"));
				AssertEquals(expectedUrl, page.Response.RedirectLocation);
				AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
			}
		}

		public void TestPageLoad_ClientPortalUrl()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "DSAJNBF";
			org.MiscServ.OM_CMClientPortalHomePage = "http://clienthomepage.com";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");

			Factory.Save();

			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString
			{
				[LoginRouter.IdentityTokenQueryStringKey] = token,
				[LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/"
			};

			var page = GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));

			page.DoPageLoad();

			var expectedUrl = page.SiteUser.GetClientPortalUrl();
			AssertEquals(expectedUrl, page.Response.RedirectLocation);
			AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
		}

		public void TestPageLoad_ClientPortalUrl_TermsAndConditions()
		{
			using (WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Some Terms and Conditions"))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "DSAJNBF";
				org.MiscServ.OM_CMClientPortalHomePage = "http://clienthomepage.com";
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "User One";
				contact.OC_Email = "user.one@test.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");
				contact.OC_WebContractSignedDate = ZDateTime.Now.AddDays(-1);

				var logQuery = new ZQuery(StmDataSchema.SD_Name, WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.Name);
				var stmData = Factory.LoadTop1<StmData>(logQuery);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				stmData.Logs.AddNew(AutoEvents.EditedARecord, ZDateTimeOffset.Now);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Factory.Save();

				contact.Person.PER_EmailAddress = contact.OC_Email;
				Factory.Save();

				var token = LoginRouterIdentityManager.GenerateToken(contact);
				var secureQueryString = new SecureQueryString
				{
					[LoginRouter.IdentityTokenQueryStringKey] = token,
					[LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/"
				};

				var page = GetPageForTest();
				page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));

				page.DoPageLoad();

				var expectedUrl = string.Format("{0}?{1}={2}", page.AppInstance.TermsAndConditionsPage, Global.TermsAndConditionsRedirectUrlTag, WebUtility.HtmlEncode(page.SiteUser.GetClientPortalUrl()));
				AssertEquals(expectedUrl, page.Response.RedirectLocation);
				AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
			}
		}

		public void TestPageLoad_ContactIsValidForWebLogin()
		{
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://google.com.au");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "DSAJNBF";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			contact.OC_IsActive = false;

			Factory.Save();

			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString
			{
				[LoginRouter.IdentityTokenQueryStringKey] = token,
				[LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/"
			};

			var page = GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
			page.DoPageLoad();

			AssertEquals("Precondition", false, contact.IsValidForWebLogin);
			AssertEquals(WebDataRegistry.Instance.WebTrackerUrl.Value, page.Response.RedirectLocation);
			AssertEquals(false, page.SiteUser.IsLoggedIn);
			AssertEquals(string.Empty, page.Response.Cookies[".ASPXAUTH"]?.Value ?? string.Empty);
			AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
		}

		public void TestPageLoad_AlreadyLoggedIn()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "DSAJNBF";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");

			Factory.Save();

			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString
			{
				[LoginRouter.IdentityTokenQueryStringKey] = token,
				[LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/"
			};

			var page = GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
			page.SiteUser.LoginForTest(contact.OrgCode, contact.OC_Email, "1234");
			AssertEquals("Precondition", true, page.SiteUser.IsLoggedIn);
			AssertEquals("Precondition", contact.PK, page.SiteUser.LoggedInUserPK);
			page.DoPageLoad();

			AssertEquals("http://test.org/", page.Response.RedirectLocation);
			AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
		}

		public void TestPageLoad_NoOriginalUrl()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "DSAJNBF";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			AssertNotEquals(Guid.Empty, contact.CompanyPKForLogin);
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(contact.CompanyPKForLogin, Guid.Empty, Guid.Empty, "https://google.com.au");

			Factory.Save();

			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString
			{
				[LoginRouter.IdentityTokenQueryStringKey] = token
			};

			var page = GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));

			page.DoPageLoad();

			AssertEquals("https://google.com.au", page.Response.RedirectLocation);
			AssertEquals(true, page.SiteUser.IsLoggedIn);
			AssertEquals(contact.PK, page.SiteUser.LoggedInOrgContact.PK);
			AssertNotEquals(string.Empty, page.Response.Cookies[".ASPXAUTH"]?.Value ?? string.Empty);
			AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
		}

		public void TestPageLoad_InvalidIDShouldRedirectToErrorPage()
		{
			var page = GetPageForTest();
			page.DoPageLoad();

			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);
			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);
			AssertContains(page.AppInstance.ErrorPage, page.Response.RedirectLocation);
		}

		#region Implementation

		LoginCompleteForTest GetPageForTest()
		{
			var page = new LoginCompleteForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics",
						BindingFlags.NonPublic | BindingFlags.Instance,
						null,
						new Type[] { typeof(HttpContext) },
						null);
			method.Invoke(page, new object[] { HttpContext.Current });

			return page;
		}

		#endregion
	}
}
