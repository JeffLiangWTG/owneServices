using System;
using System.Web;
using System.Web.Security;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class ZWebControllerTest : TestCaseWithFactory, IHttpContextEnabledTestWithAppInstance
	{
		public void TestWebModuleID_ISF()
		{
			string result = Controller.GetTargetUrlCore(WebModuleId.TrackingImporterSecurityFiling, WebModuleOptions.Ref, ZGuid.Empty.ToString());
			AssertEquals(result, string.Format("{0}?{1}={2}", AppInstance.ISFDetailsPage, WebModuleOptions.Ref, ZGuid.Empty));

			result = Controller.GetTargetUrlCore(WebModuleId.TrackingImporterSecurityFiling, "", "");
			AssertEquals(result, AppInstance.ISF);
		}

		public void TestGetTargetUrlCore()
		{
			string result = Controller.GetTargetUrlCore(WebModuleId.TrackingDeclarations, WebModuleOptions.Ref, ZGuid.Empty.ToString());
			AssertEquals(result, string.Format("{0}?{1}={2}", AppInstance.DeclarationDetailsPage, WebModuleOptions.Ref, ZGuid.Empty));

			result = Controller.GetTargetUrlCore(WebModuleId.TrackingBookings, WebModuleOptions.Ref, ZGuid.Empty.ToString());
			AssertEquals(result, string.Format("{0}?{1}={2}", AppInstance.BookingDetailsPage, WebModuleOptions.Ref, ZGuid.Empty));

			result = Controller.GetTargetUrlCore(WebModuleId.TrackingFreightLabel, WebModuleOptions.Ref, ZGuid.Empty.ToString());
			AssertEquals(result, FreightLabelRequestHandler.RequestHelper.GetHandlerUrl(ZGuid.Empty));

			result = Controller.GetTargetUrlCore(WebModuleId.TrackingHousebill, WebModuleOptions.Ref, ZGuid.Empty.ToString());
			AssertEquals(result, HouseBillRequestHandler.RequestHelper.GetHandlerUrl(ZGuid.Empty));
		}

		public void TestRedirectToTrackingPageCore_WithContactInfo_DoesNotRequirePassword_DoesNotRequireLogin()
		{
			using (WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var contact = SetupTestContact(Controller.Factory);
				var expectedUrl = "TestRedirectToTrackingPageCore_WithContactInfo_DoesNotRequirePassword_DoesNotRequireLogin";
				Controller.RedirectToTrackingPageCore(contact.PK, expectedUrl);
				AssertEquals(expectedUrl, Controller.LastRedirectUrl);
				Assert(AppInstance.SiteUser.IsLoggedIn);
				AssertEquals("somebody@test.me", AppInstance.SiteUser.LoggedInUser.Email);
				AssertEquals("TSTORG", AppInstance.SiteUser.AffiliationCode);
				AssertNotNull(FormsAuthentication.GetAuthCookie("somebody@test.me", false));
			}
		}

		public void TestRedirectToTrackingPageCore_WithContactInfo_DoesNotRequirePassword_RequiresLogin()
		{
			using (WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var contact = SetupTestContact(Controller.Factory);
				var expectedUrl = "RedirectToTrackingPageCore_WithoutContactInfo_DoesNotRequirePassword_RequiresLogin";
				Controller.RedirectToTrackingPageCore(contact.PK, expectedUrl, true);
				AssertEquals(expectedUrl, Controller.LastRedirectUrl);
				Assert(AppInstance.SiteUser.IsLoggedIn);
				AssertEquals("somebody@test.me", AppInstance.SiteUser.LoggedInUser.Email);
				AssertEquals("TSTORG", AppInstance.SiteUser.AffiliationCode);
				AssertNotNull(FormsAuthentication.GetAuthCookie("somebody@test.me", false));
			}
		}

		public void TestRedirectToTrackingPageCore_AlreadyLoggedIn_DoesNotRequirePassword_DoesNotRequireLogin_SkipsLogin()
		{
			using (WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var expectedUrl = "RedirectToTrackingPageCore_AlreadyLoggedIn_DoesNotRequirePassword_DoesNotRequireLogin_SkipsLogin";
				var expectedAutoLoginQueryStringData = "some-secure-data";
				var contact = SetupTestContact(Controller.Factory);

				AppInstance.SiteUser.LoginForTest(contact.OrgCode, contact.OC_Email, "1234");

				Controller.RedirectToTrackingPageCore(Guid.NewGuid(), expectedUrl, queryStringData: expectedAutoLoginQueryStringData);

				AssertEquals(expectedUrl, Controller.LastRedirectUrl);
				AssertEquals(expectedAutoLoginQueryStringData, AppInstance.Session[Global.AutoLoginQueryStringDataIndexer]);
				Assert(AppInstance.SiteUser.IsLoggedIn);
				AssertEquals("somebody@test.me", AppInstance.SiteUser.LoggedInUser.Email);
				AssertEquals("TSTORG", AppInstance.SiteUser.AffiliationCode);
				AssertNotNull(FormsAuthentication.GetAuthCookie("somebody@test.me", false));
			}
		}

		public void TestRedirectToTrackingPageCore_WithoutContactInfo_DoesNotRequirePassword_DoesNotRequireLogin()
		{
			using (WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var expectedUrl = "RedirectToTrackingPageCore_WithoutContactInfo_DoesNotRequirePassword_DoesNotRequireLogin";
				Controller.RedirectToTrackingPageCore(Guid.NewGuid(), expectedUrl);
				AssertEquals(expectedUrl, Controller.LastRedirectUrl);
				AssertEquals(User.WebUserName, AppInstance.SiteUser.LoggedInUser.Name);
				AssertEquals("EDICUS", AppInstance.SiteUser.AffiliationCode);
				AssertNotNull(FormsAuthentication.GetAuthCookie(User.SupportUserName, false));
			}
		}

		public void TestRedirectToTrackingPageCore_WithoutContactInfo_RequirePassword_RequiresLogin()
		{
			using (WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var targetUrl = "RedirectToTrackingPageCore_WithoutContactInfo_RequirePassword_RequiresLogin";
				var expectedUrl = string.Format("{0}?ReturnUrl={1}", FormsAuthentication.LoginUrl, AppInstance.Server.UrlEncode(targetUrl));
				Controller.RedirectToTrackingPageCore(Guid.NewGuid(), targetUrl, true);
				AssertEquals(expectedUrl, Controller.LastRedirectUrl);
				Assert(!AppInstance.SiteUser.IsLoggedIn);
			}
		}

		public void TestRedirectToTrackingPageCore_WithContactInfo_RequirePassword_DoesNotRequireLogin()
		{
			using (WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var contact = SetupTestContact(Controller.Factory);
				var targetUrl = "RedirectToTrackingPageCore_WithContactInfo_RequirePassword_DoesNotRequireLogin";
				var expectedUrl = string.Format("{0}?ReturnUrl={1}", FormsAuthentication.LoginUrl, AppInstance.Server.UrlEncode(targetUrl));
				Controller.RedirectToTrackingPageCore(contact.PK, targetUrl);
				AssertEquals(expectedUrl, Controller.LastRedirectUrl);
				Assert(!AppInstance.SiteUser.IsLoggedIn);
			}
		}

		public void TestRedirectToTrackingPageCore_WithContactInfo_RequirePassword_RequiresLogin()
		{
			using (WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var contact = SetupTestContact(Controller.Factory);
				var targetUrl = "RedirectToTrackingPageCore_WithContactInfo_RequirePassword_RequiresLogin";
				var expectedUrl = string.Format("{0}?ReturnUrl={1}", FormsAuthentication.LoginUrl, AppInstance.Server.UrlEncode(targetUrl));
				Controller.RedirectToTrackingPageCore(contact.PK, targetUrl, true);
				AssertEquals(expectedUrl, Controller.LastRedirectUrl);
				Assert(!AppInstance.SiteUser.IsLoggedIn);
			}
		}

		public void TestRedirectToTrackingPageCore_WithoutContactInfo_RequirePassword_DoesNotRequireLogin()
		{
			using (WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var targetUrl = "RedirectToTrackingPageCore_WithoutContactInfo_RequirePassword_DoesNotRequireLogin";
				var expectedUrl = string.Format("{0}?ReturnUrl={1}", FormsAuthentication.LoginUrl, AppInstance.Server.UrlEncode(targetUrl));
				Controller.RedirectToTrackingPageCore(Guid.NewGuid(), targetUrl);
				AssertEquals(expectedUrl, Controller.LastRedirectUrl);
				Assert(!AppInstance.SiteUser.IsLoggedIn);
			}
		}

		public void TestRedirectToTrackingPageCore_WithoutContactInfo_DoesNotRequirePassword_RequiresLogin()
		{
			using (WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var targetUrl = "RedirectToTrackingPageCore_WithoutContactInfo_DoesNotRequirePassword_RequiresLogin";
				var expectedUrl = string.Format("{0}?ReturnUrl={1}", FormsAuthentication.LoginUrl, AppInstance.Server.UrlEncode(targetUrl));
				Controller.RedirectToTrackingPageCore(Guid.NewGuid(), targetUrl, true);
				AssertEquals(expectedUrl, Controller.LastRedirectUrl);
				Assert(!AppInstance.SiteUser.IsLoggedIn);
			}
		}

		#region Implementation

		OrgContact SetupTestContact(BusinessObjectFactory factory)
		{
			var org = factory.New<OrgHeader>();
			var contact = factory.New<OrgContact>();
			org.OH_Code = "TSTORG";
			contact.OC_Email = "somebody@test.me";
			contact.OC_OH = org.PK;
			contact.OC_WebAccessEnabled = ZBool.True;
			contact.SetHashedPassword("1234");
			factory.Save();
			return contact;
		}

		ZWebTestController Controller
		{
			get
			{
				if (fController == null)
				{ fController = new ZWebTestController(HttpContext.Current); }
				return fController;
			}
		}
		ZWebTestController fController;

		#endregion

		#region IHttpContextEnabledTestWithAppInstance Members

		ZEnterpriseGlobalBase IHttpContextEnabledTestWithAppInstance.AppInstance
		{
			get { return new GlobalForTest(); }
		}

		GlobalForTest AppInstance
		{
			get { return (GlobalForTest)HttpContext.Current.ApplicationInstance; }
		}

		class GlobalForTest : Global
		{
			public override WebUser SiteUser
			{
				get
				{
					if (fSiteUser == null)
					{
						fSiteUser = new TrackingSiteUser();
					}
					return fSiteUser;
				}
			}
			TrackingSiteUser fSiteUser;
		}

		#endregion
	}
}
