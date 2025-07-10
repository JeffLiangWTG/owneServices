using System;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Definitions.Authentication;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class SetPasswordLifeCycleTest : ZPageLifeCycleTest
	{
		protected override ZPage GetNewPage()
		{
			return SetPasswordPage;
		}

		public void TestPageLoad()
		{
			AssertPageLoad(AssertOnLoadComplete_WithValidToken, TestToken);
			AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, TestToken_Expired);
			AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, TestToken_InvalidType);
			AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, "invalidToken");
			AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, "");
		}

		void AssertPageLoad(EventHandler onLOadCompleteHandler, string token)
		{
			HttpContext.Current.Request.QueryString.Remove(TrackingConstants.QueryStringKeys.SetPasswordKey);
			HttpContext.Current.Request.QueryString.Add(TrackingConstants.QueryStringKeys.SetPasswordKey, token);

			this.AssertOnLoadComplete += onLOadCompleteHandler;
			try
			{
				RunPageLifeCycle();
			}
			finally
			{
				this.AssertOnLoadComplete -= onLOadCompleteHandler;
			}
		}

		public void TestSetPassword_IgnoreOverride()
		{
			try
			{
				DataRegistry.Instance.EmailDestinationOverride = "override@email.com";
				using (var page = SetPasswordPage)
				{
					page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.SetPasswordKey);
					page.RequestQueryString_Expose.Add(TrackingConstants.QueryStringKeys.SetPasswordKey, "invalidToken");

					page.OnUpdate();
					AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);

					page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.SetPasswordKey);
					page.RequestQueryString_Expose.Add(TrackingConstants.QueryStringKeys.SetPasswordKey, TestToken);

					page.OnLoad();
					page.NewPassword_Expose.Text = "Th1sIsMyP4ssword!";
					page.NewPasswordConfirm_Expose.Text = "2";

					page.OnUpdate();

					ITokenizedAccessControl accessControl = new TokenizedAccessControl();
					Assert(accessControl.TryPeek(TestToken, AccessTokenTypes.SetPassword, out var accessToken));

					page.NewPasswordConfirm_Expose.Text = "Th1sIsMyP4ssword!";
					page.OnUpdate();
					Assert("Password should be updated", TestOrgContact.VerifyPassword("Th1sIsMyP4ssword!"));
					AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
					Assert(page.Back_Expose.Visible);
					Assert(!accessControl.TryPeek(TestToken, AccessTokenTypes.SetPassword, out accessToken));

					page.OnLoad();
					AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);

					var email = Env.OutgoingMailManager.EmailsCreated[0];
					AssertEquals(true, email.Recipients[0].IsForSystemCommunication);
				}
			}
			finally
			{
				DataRegistry.Instance.EmailDestinationOverride = "";
			}
		}

		public void TestSetPassword()
		{
			using (var page = SetPasswordPage)
			{
				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.SetPasswordKey);
				page.RequestQueryString_Expose.Add(TrackingConstants.QueryStringKeys.SetPasswordKey, "invalidToken");

				page.OnUpdate();
				AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);

				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.SetPasswordKey);
				page.RequestQueryString_Expose.Add(TrackingConstants.QueryStringKeys.SetPasswordKey, TestToken);

				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyP4ssword!";
				page.NewPasswordConfirm_Expose.Text = "2";

				page.OnUpdate();

				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(accessControl.TryPeek(TestToken, AccessTokenTypes.SetPassword, out var accessToken));

				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyP4ssword!";
				page.OnUpdate();
				Assert("Password should be updated", TestOrgContact.VerifyPassword("Th1sIsMyP4ssword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				Assert(page.Back_Expose.Visible);
				Assert(!accessControl.TryPeek(TestToken, AccessTokenTypes.SetPassword, out accessToken));

				page.OnLoad();
				AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
			}
		}

		public void TestNoCache()
		{
			using (var page = SetPasswordPage)
			{
				Assert(!page.Cacheable_Expose);
			}
		}

		public void TestOnInit_LogOff()
		{
			using (var page = SetPasswordPage)
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
			var testPage = (SetPasswordForTest)this.TestPage;
			Assert(testPage.SetTable_Expose.Visible);
			AssertNullOrEmpty(testPage.PasswordChangeMessage_Expose.Text);
			AssertEquals($"Set your password for login to <b>{TestOrgContact.Header.OH_FullName}</b>", testPage.SetPasswordHeadingLabel_Expose.Text);
			AssertEquals(TestOrgContact.Header.OH_Code, testPage.OrgCodeText_Expose.Text);
		}

		void AssertOnLoadComplete_WithInvalidToken(object sender, EventArgs e)
		{
			var testPage = (SetPasswordForTest)this.TestPage;
			CombineAssertions(() =>
			{
				Assert("cannot see set password controls", !testPage.SetTable_Expose.Visible);
				AssertEquals("The set link you have followed is invalid or expired.", testPage.PasswordChangeMessage_Expose.Text);
				AssertEquals("", testPage.SetPasswordHeadingLabel_Expose.Text);
				AssertEquals("", testPage.OrgCodeText_Expose.Text);
			});
		}

		SetPasswordForTest SetPasswordPage
		{
			get
			{
				var testPage = new SetPasswordForTest();
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

		protected override void SetUp()
		{
			base.SetUp();

			TestToken = "testToken";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "WiseTech";
			orgHeader.OH_FullName = "WiseTech Global";

			var person = Factory.NewWithValidTestData<GlbPerson>();

			TestOrgContact = orgHeader.Contacts.AddNew();
			TestOrgContact.OC_Email = "TestUser@wisetechglobal.com";
			TestOrgContact.OC_IsActive = true;
			TestOrgContact.OC_WebAccessEnabled = true;
			TestOrgContact.OC_PER = person.PK;

			// we can set contact password only when it is not the only contact in the person relationship
			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_Email = "contact2@wisetechglobal.com";
			contact2.OC_IsActive = true;
			contact2.OC_PER = person.PK;

			Factory.Save();

			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			TestToken = accessControl.CreateLimitedToken(AccessTokenTypes.SetPassword, new AccessTokenInfo("", TestOrgContact.PK.ToGuid(), "OC"), maxUses: 1);
			TestToken_Expired = accessControl.CreateLimitedToken(AccessTokenTypes.SetPassword, new AccessTokenInfo("", TestOrgContact.PK.ToGuid(), "OC"), TimeSpan.FromHours(-1), 1);
			TestToken_InvalidType = accessControl.CreateLimitedToken("INV", new AccessTokenInfo("", Guid.Empty, "INV"), maxUses: 1);
		}
	}
}
