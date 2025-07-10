using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Web;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class TermsAndConditionsPageTest : BaseTrackingPageTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.TermsAndConditions;
		}

		protected override bool IsInWebRootDirectory
		{
			get { return true; }
		}

		#region OnLoad Site Terms and Conditions

		public void TestOnLoadWithoutSiteTermsAndConditions()
		{
			Assert("Should not be redirected", string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));
			AssertEquals("RedirectUrlForTest should be Default page", TestPage.AppInstance.DefaultPage, TestPage.RedirectUrlForTest);
			AssertEquals("User is logged in", true, TestPage.SiteUser.IsLoggedIn);
			Assert("The OC_WebContractSignedDate should be empty", TestPage.SiteUser.LoggedInWebContact.OC_WebContractSignedDate.IsEmpty);
			Assert("Terms and Conditions Text should be empty", string.IsNullOrEmpty(WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.Value));

			TestPage.OnLoadForTest();

			AssertEquals("Should be immediately redirected to Default page", TestPage.AppInstance.DefaultPage, HttpContext.Current.Response.RedirectLocation);
		}

		public void TestOnLoadWithSiteTermsAndConditions()
		{
			Assert("Should not be redirected", string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));
			AssertEquals("RedirectUrlForTest should be Default page", TestPage.AppInstance.DefaultPage, TestPage.RedirectUrlForTest);
			AssertEquals("User is logged in", true, TestPage.SiteUser.IsLoggedIn);
			Assert("The OC_WebContractSignedDate should be empty", TestPage.SiteUser.LoggedInWebContact.OC_WebContractSignedDate.IsEmpty);
			Assert("Terms and Conditions Text should be empty", string.IsNullOrEmpty(WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.Value));
			WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Some Terms and Conditions");

			TestPage.OnLoadForTest();

			Assert("Should not be redirected", string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));

			AssertEquals("TermsAndConditionsText should be as expected", "Some Terms and Conditions", TestPage.TermsAndConditionsText.Text);
		}

		public void TestIAgreeButtonClickForSiteTermsAndConditions()
		{
			Assert("Should not be redirected", string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));
			AssertEquals("RedirectUrlForTest should be Default page", TestPage.AppInstance.DefaultPage, TestPage.RedirectUrlForTest);
			AssertEquals("User is logged in", true, TestPage.SiteUser.IsLoggedIn);
			Assert("The OC_WebContractSignedDate should be empty", TestPage.SiteUser.LoggedInWebContact.OC_WebContractSignedDate.IsEmpty);
			Assert("Terms and Conditions Text should be empty", string.IsNullOrEmpty(WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.Value));
			WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Some Terms and Conditions");

			TestPage.OnLoadForTest();

			AssertNull("Should be not redirected", HttpContext.Current.Response.RedirectLocation);

			TestPage.IAgreeButtonCLickForTest();

			AssertEquals("The OC_WebContractSignedDate should be not empty", false, TestPage.SiteUser.LoggedInWebContact.OC_WebContractSignedDate.IsEmpty);
			AssertEquals("Should be redirected to Default page", TestPage.AppInstance.DefaultPage, HttpContext.Current.Response.RedirectLocation);
		}

		public void TestIDisagreeButtonClickForSiteTermsAndConditions()
		{
			Assert("Should not be redirected", string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));
			AssertEquals("RedirectUrlForTest should be Default page", TestPage.AppInstance.DefaultPage, TestPage.RedirectUrlForTest);
			AssertEquals("User is logged in", true, TestPage.SiteUser.IsLoggedIn);
			Assert("The OC_WebContractSignedDate should be empty", TestPage.SiteUser.LoggedInWebContact.OC_WebContractSignedDate.IsEmpty);
			Assert("Terms and Conditions Text should be empty", string.IsNullOrEmpty(WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.Value));
			WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Some Terms and Conditions");

			TestPage.OnLoadForTest();

			AssertNull("Should be not redirected", HttpContext.Current.Response.RedirectLocation);

			TestPage.IDisagreeButtonClickForTest();

			Assert("The OC_WebContractSignedDate should be empty", TestPage.SiteUser.LoggedInWebContact.OC_WebContractSignedDate.IsEmpty);
			AssertEquals("Should be redirected to Logout page", TestPage.AppInstance.LogoutPage, HttpContext.Current.Response.RedirectLocation);
		}

		public void TestApplyUserAgreed()
		{
			TestPage.OnLoadForTest();

			TestPage.SiteUser.Login("", "");
			AssertEquals(false, TestPage.SiteUser.IsLoggedIn);
			AssertNoExceptionThrown(delegate
			{ TestPage.ApplyUserAgreed(); });

			TestPage.IsCreateNewAppInstanceIfNullForTest = false;
			AssertNull(TestPage.SiteUser);
			AssertNoExceptionThrown(delegate
			{ TestPage.ApplyUserAgreed(); });
		}

		#endregion

		#region OnLoad Booking Terms and Conditions

		public void TestOnLoadWithoutBookingTermsAndConditions()
		{
			Assert("Should not be redirected", string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));
			AssertEquals("RedirectUrlForTest should be Default page", TestPage.AppInstance.DefaultPage, TestPage.RedirectUrlForTest);
			AssertEquals("User is logged in", true, TestPage.SiteUser.IsLoggedIn);
			AssertNull("Flag in Session should be null", TestPage.Session[Global.BookingTermsAndConditionsIndexer]);
			Assert("Terms and Conditions Text should be empty", string.IsNullOrEmpty(WebDataRegistry.Instance.BookingTermsAndConditions.Value));
			SetRedirectUrlParam(TestPage.AppInstance.EditBookingPage);

			TestPage.OnLoadForTest();

			AssertEquals("Should be immediately redirected to EditBooking page", TestPage.AppInstance.EditBookingPage, HttpContext.Current.Response.RedirectLocation);
		}

		public void TestOnLoadWithBookingTermsAndConditions()
		{
			Assert("Should not be redirected", string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));
			AssertEquals("RedirectUrlForTest should be Default page", TestPage.AppInstance.DefaultPage, TestPage.RedirectUrlForTest);
			AssertEquals("User is logged in", true, TestPage.SiteUser.IsLoggedIn);
			AssertNull("Flag in Session should be null", TestPage.Session[Global.BookingTermsAndConditionsIndexer]);
			Assert("Terms and Conditions Text should be empty", string.IsNullOrEmpty(WebDataRegistry.Instance.BookingTermsAndConditions.Value));
			WebDataRegistry.Instance.BookingTermsAndConditions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Some Terms and Conditions\r\nFirst Term\r\nFirst Condition");
			SetRedirectUrlParam(TestPage.AppInstance.EditBookingPage);

			TestPage.OnLoadForTest();

			Assert("Should not be redirected", string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));

			AssertEquals("TermsAndConditionsText should be with replaced line breaks", "Some Terms and Conditions<br />First Term<br />First Condition", TestPage.TermsAndConditionsText.Text);
		}

		public void TestIAgreeButtonClickForBookingTermsAndConditions()
		{
			Assert("Should not be redirected", string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));
			AssertEquals("RedirectUrlForTest should be Default page", TestPage.AppInstance.DefaultPage, TestPage.RedirectUrlForTest);
			AssertEquals("User is logged in", true, TestPage.SiteUser.IsLoggedIn);
			AssertNull("Flag in Session should be null", TestPage.Session[Global.BookingTermsAndConditionsIndexer]);
			Assert("Terms and Conditions Text should be empty", string.IsNullOrEmpty(WebDataRegistry.Instance.BookingTermsAndConditions.Value));
			WebDataRegistry.Instance.BookingTermsAndConditions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Some Terms and Conditions");
			SetRedirectUrlParam(TestPage.AppInstance.EditBookingPage);

			TestPage.OnLoadForTest();

			AssertNull("Should be not redirected", HttpContext.Current.Response.RedirectLocation);

			TestPage.IAgreeButtonCLickForTest();

			AssertNotNull("Flag in Session should be not null", TestPage.Session[Global.BookingTermsAndConditionsIndexer]);
			AssertEquals("Flag in Session should be set", true, (bool)TestPage.Session[Global.BookingTermsAndConditionsIndexer]);
			AssertEquals("Should be redirected to Default page", TestPage.AppInstance.EditBookingPage, HttpContext.Current.Response.RedirectLocation);
		}

		public void TestIDisagreeButtonClickForBookingTermsAndConditions()
		{
			Assert("Should not be redirected", string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));
			AssertEquals("RedirectUrlForTest should be Default page", TestPage.AppInstance.DefaultPage, TestPage.RedirectUrlForTest);
			AssertEquals("User is logged in", true, TestPage.SiteUser.IsLoggedIn);
			AssertNull("Flag in Session should be null", TestPage.Session[Global.BookingTermsAndConditionsIndexer]);
			Assert("Terms and Conditions Text should be empty", string.IsNullOrEmpty(WebDataRegistry.Instance.BookingTermsAndConditions.Value));
			WebDataRegistry.Instance.BookingTermsAndConditions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Some Terms and Conditions");
			SetRedirectUrlParam(TestPage.AppInstance.EditBookingPage);

			TestPage.OnLoadForTest();

			AssertNull("Should be not redirected", HttpContext.Current.Response.RedirectLocation);

			TestPage.IDisagreeButtonClickForTest();

			AssertNull("Flag in Session should be null", TestPage.Session[Global.BookingTermsAndConditionsIndexer]);
			AssertEquals("Should be redirected to Default page", TestPage.AppInstance.DefaultPage, HttpContext.Current.Response.RedirectLocation);
		}

		#endregion

		public void TestSanitizeRedirectUrl()
		{
			CombineAssertions(() =>
			{
				AssertRedirect(TestPage.AppInstance.DefaultPage, "not a url");
				AssertRedirect(TestPage.AppInstance.DefaultPage, "http://www.openredirectionvulnerability.com");
				AssertRedirect(TestPage.AppInstance.DefaultPage, "//openredirectionvulnerability.com");
				AssertRedirect(TestPage.AppInstance.DefaultPage, "wss:openredirectionvulnerability.com");
				AssertRedirect(TestPage.AppInstance.DefaultPage, TestPage.AppInstance.DefaultPage);
				AssertRedirect("/Shipments/Shipments.aspx", "/Shipments/Shipments.aspx");
				AssertRedirect(TestPage.AppInstance.DefaultPage, "/../../..");
			});

			void AssertRedirect(string expectedUrl, string redirectUrl)
			{
				SetRedirectUrlParam(redirectUrl);

				AssertEquals(expectedUrl, TestPage.RedirectUrlForTest);
			}
		}

		void SetRedirectUrlParam(string redirectUrl)
		{
			var isReadOnlyProperty = typeof(NameValueCollection).GetProperty("IsReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
			isReadOnlyProperty.SetValue(HttpContext.Current.Request.Params, false, null);

			HttpContext.Current.Request.Params[Global.TermsAndConditionsRedirectUrlTag] = redirectUrl;
		}

		#region Implementation

		new TermsAndConditionsForTest TestPage
		{
			get { return base.TestPage as TermsAndConditionsForTest; }
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new TermsAndConditionsForTest();
		}

		#endregion
	}
}
