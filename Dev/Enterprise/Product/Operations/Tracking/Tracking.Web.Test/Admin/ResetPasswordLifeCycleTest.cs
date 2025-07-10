using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions.Authentication;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using Newtonsoft.Json;
using WTG.Foundation.Cryptography.UserSecrets;
using PasswordResetHelper = Enterprise.ZArchitecture.Web.GUI.PasswordResetHelper;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class ResetPasswordLifeCycleTest : ZPageLifeCycleTest
	{
		protected override ZPage GetNewPage()
		{
			return ResetPasswordPage;
		}

		public void TestPageLoad()
		{
			AssertPageLoad(AssertOnLoadComplete_TokenInvalid, "invalidToken");
			AssertPageLoad(AssertOnLoadComplete_TokenInvalid, "");
			AssertPageLoad(AssertOnLoadComplete_TokenValid, TestToken_ResetPassword);
			AssertPageLoad(AssertOnLoadComplete_TokenInvalid, TestToken_InvalidType);
		}

		void AssertPageLoad(EventHandler onLOadCompleteHandler, string token)
		{
			HttpContext.Current.Request.QueryString.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
			HttpContext.Current.Request.QueryString.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, token);

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

		void AssertOnLoadComplete_TokenInvalid(object sender, EventArgs e)
		{
			var testPage = (ResetPasswordForTest)this.TestPage;
			CombineAssertions(() =>
			{
				Assert("cannot see reset password controls", !testPage.ResetTable_Expose.Visible);
				AssertEquals("The reset link you have followed is invalid or expired.", testPage.PasswordChangeMessage_Expose.Text);
			});
		}

		void AssertOnLoadComplete_TokenValid(object sender, EventArgs e)
		{
			var testPage = (ResetPasswordForTest)this.TestPage;
			CombineAssertions(() =>
			{
				Assert(testPage.ResetTable_Expose.Visible);
				AssertNullOrEmpty(testPage.PasswordChangeMessage_Expose.Text);
			});
		}

		public void TestResetPassword_EmailOverride()
		{
			try
			{
				WebDataRegistry.Instance.WebLoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 60);
				CreateLockoutUserRecord(TestOrgContact.OrgCode, TestOrgContact.OC_Email);
				Factory.Save();
				DataRegistry.Instance.EmailDestinationOverride = "override@email.com";

				using (var page = ResetPasswordPage)
				{
					var hash = LoadLoginHashCookie(TestOrgContact);

					Assert("Precondition", ContactIsLockedOut(TestOrgContact.OrgCode, TestOrgContact.OC_Email, hash));
					page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
					page.RequestQueryString_Expose.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, "invalidToken");

					page.OnUpdate();
					AssertEquals("The reset link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
				}

				using (var page = ResetPasswordPage)
				{
					page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
					page.RequestQueryString_Expose.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, TestToken_ResetPassword);

					page.OnLoad();
					page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
					page.NewPasswordConfirm_Expose.Text = "2";

					var passwordResetHelper = page.DataSource as PasswordResetHelper;
					passwordResetHelper.CompanyCode = "WiseTech";

					page.OnUpdate();

					ITokenizedAccessControl accessControl = new TokenizedAccessControl();
					Assert(accessControl.TryPeek(TestToken_ResetPassword, AccessTokenTypes.ResetPassword, out var accessToken));

					page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
					page.OnUpdate();

					Assert("Password should be updated", TestOrgContact.VerifyPassword("Th1sIsMyPassword!"));
					AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
					Assert(page.Back_Expose.Visible);
					Assert(!accessControl.TryPeek(TestToken_ResetPassword, AccessTokenTypes.ResetPassword, out accessToken));

					var hash = LoadLoginHashCookie(TestOrgContact);
					Assert("Should unlock account after changing password", !ContactIsLockedOut(TestOrgContact.OrgCode, TestOrgContact.OC_Email, hash));

					var email = Env.OutgoingMailManager.EmailsCreated[0];
					AssertEquals(true, email.Recipients[0].IsForSystemCommunication);
				}
			}
			finally
			{
				DataRegistry.Instance.EmailDestinationOverride = "";
			}
		}

		public void TestResetPassword()
		{
			WebDataRegistry.Instance.WebLoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 60);
			CreateLockoutUserRecord(TestOrgContact.OrgCode, TestOrgContact.OC_Email);
			Factory.Save();

			using (var page = ResetPasswordPage)
			{
				var hash = LoadLoginHashCookie(TestOrgContact);
				Assert("Precondition", ContactIsLockedOut(TestOrgContact.OrgCode, TestOrgContact.OC_Email, hash));
				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
				page.RequestQueryString_Expose.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, "invalidToken");

				page.OnUpdate();
				AssertEquals("The reset link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
			}

			using (var page = ResetPasswordPage)
			{
				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
				page.RequestQueryString_Expose.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, TestToken_ResetPassword);

				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "2";

				var passwordResetHelper = page.DataSource as PasswordResetHelper;
				passwordResetHelper.CompanyCode = "WiseTech";

				page.OnUpdate();

				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(accessControl.TryPeek(TestToken_ResetPassword, AccessTokenTypes.ResetPassword, out var accessToken));

				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.OnUpdate();

				Assert("Password should be updated", TestOrgContact.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				Assert(page.Back_Expose.Visible);
				Assert(!accessControl.TryPeek(TestToken_ResetPassword, AccessTokenTypes.ResetPassword, out accessToken));

				var hash = LoadLoginHashCookie(TestOrgContact);
				Assert("Should unlock account after changing password", !ContactIsLockedOut(TestOrgContact.OrgCode, TestOrgContact.OC_Email, hash));
			}

			using (var page = ResetPasswordPage)
			{
				page.OnLoad();
				AssertEquals("The reset link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
			}

			using (var page = ResetPasswordPage)
			{
				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
				page.RequestQueryString_Expose.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, TestToken_LegacyResetPassword);

				page.OnLoad();

				page.NewPassword_Expose.Text = "Th1sIsMyPassword!!!!!";
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!!!!!";

				var passwordResetHelper = page.DataSource as PasswordResetHelper;
				passwordResetHelper.CompanyCode = "WiseTech";

				page.OnUpdate();
				Assert("Password should be updated", TestOrgContact.VerifyPassword("Th1sIsMyPassword!!!!!"));
				Assert("Should write login hash cookie", page.Response.Cookies.Keys.OfType<string>().Any(key => key.StartsWith("DeviceCookie_")));
			}

			using (var page = ResetPasswordPage)
			{
				ClearKey(WebDataRegistry.Instance.LoginFailureAttemptSecretKey.Name);

				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
				page.RequestQueryString_Expose.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, TestToken_LegacyResetPassword);

				page.OnLoad();

				page.NewPassword_Expose.Text = "Th1sIsMyPassword!!!!!";
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!!!!!";

				var passwordResetHelper = page.DataSource as PasswordResetHelper;
				passwordResetHelper.CompanyCode = "WiseTech";

				page.OnUpdate();
				Assert("Should not write login hash cookie when LoginFailureAttemptSecretKey is not set", !page.Response.Cookies.Keys.OfType<string>().Any(key => key.StartsWith("DeviceCookie_")));
				ErrorReporter.Clear();
			}

			void ClearKey(string name)
			{
				using (var command = Db.Connection.Command("UPDATE dbo.StmData SET SD_BinaryValue = NULL WHERE SD_Name = @Name"))
				{
					command.AddParameter("@Name", SqlDbType.VarChar, name);
					command.ExecuteNonQuery();
				}

				WebDataRegistry.Instance.RemoveItemFromCacheIfOlderThan(name, TimeSpan.FromSeconds(-1));
			}
		}

		public void TestResetPasswordShouldLoadCompanyFromToken()
		{
			var contactEmail = "testuser@cargowise.com";
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "ALBANIA";
			var contact1 = orgHeader1.Contacts.AddNew();
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_Email = contactEmail;
			contact1.OC_ContactName = "User A";
			orgHeader1.Contacts.Add(contact1);
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "MONTENE";
			var contact2 = orgHeader2.Contacts.AddNew();
			contact2.OC_IsActive = true;
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_Email = contactEmail;
			contact2.OC_ContactName = "User B";
			orgHeader2.Contacts.Add(contact2);

			WebDataRegistry.Instance.WebLoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 60);
			CreateLockoutUserRecord(TestOrgContact.OrgCode, TestOrgContact.OC_Email);

			Factory.Save();

			var passwordResetInfo = new PasswordResetInfo
			{
				ContactEmail = contactEmail,
				OrgCode = orgHeader1.OH_Code
			};
			var scope = JsonConvert.SerializeObject(passwordResetInfo);
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			TestToken_ResetPassword = accessControl.CreateLimitedToken(AccessTokenTypes.ResetPassword, new AccessTokenInfo(scope, Guid.Empty, "INV"), maxUses: 1);

			using (var page = ResetPasswordPage)
			{
				Assert("Precondition", ContactIsLockedOut(TestOrgContact.OrgCode, TestOrgContact.OC_Email));
				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
				page.RequestQueryString_Expose.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, TestToken_ResetPassword);

				page.OnLoad();
				var helper = (PasswordResetHelper)page.DataSource;
				AssertEquals("Should only contain the specified org", 1, helper.OrgHeaders.Count);
				AssertEquals("Should only contain the specified org", orgHeader1.PK, helper.OrgHeaders[0].PK);
			}

			passwordResetInfo = new PasswordResetInfo
			{
				ContactEmail = contactEmail
			};
			scope = JsonConvert.SerializeObject(passwordResetInfo);
			TestToken_ResetPassword = accessControl.CreateLimitedToken(AccessTokenTypes.ResetPassword, new AccessTokenInfo(scope, Guid.Empty, "INV"), maxUses: 1);

			using (var page = ResetPasswordPage)
			{
				Assert("Precondition", ContactIsLockedOut(TestOrgContact.OrgCode, TestOrgContact.OC_Email));
				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
				page.RequestQueryString_Expose.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, TestToken_ResetPassword);

				page.OnLoad();
				var helper = (PasswordResetHelper)page.DataSource;
				AssertEquals("Should contain both orgs", 2, helper.OrgHeaders.Count);
			}
		}

		public void TestHeadingLabelWithExpiredPassword()
		{
			WebDataRegistry.Instance.WebPasswordRotationDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 30);
			WebDataRegistry.Instance.WebPasswordRotationEffectiveDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(-35).ToDateTime());
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://yahoo.com.au/");

			using (var page = ResetPasswordPage)
			{
				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, TestToken_ResetPassword);
				page.OnLoad();
				Assert(!page.PasswordExpiredMessageLabel_Expose.Visible);
			}

			using (var page = ResetPasswordPage)
			{
				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, TestToken_ResetPassword);
				page.RequestQueryString_Expose.Remove("ref");
				HttpContext.Current.Request.QueryString.Add("ref", "exp");
				page.OnLoad();
				Assert(page.PasswordExpiredMessageLabel_Expose.Visible);
			}
		}

		public void TestResetPassword_UnlockContact()
		{
			var otherOrg = "OTHERORG";
			WebDataRegistry.Instance.WebLoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 60);
			CreateLockoutUserRecord(TestOrgContact.OrgCode, TestOrgContact.OC_Email);
			CreateLockoutUserRecord(string.Empty, TestOrgContact.OC_Email);
			CreateLockoutUserRecord(otherOrg, TestOrgContact.OC_Email);
			Factory.Save();

			using (var page = ResetPasswordPage)
			{
				var hash = LoadLoginHashCookie(TestOrgContact);

				AssertEquals("Precondition", null, hash);
				Assert("Precondition", ContactIsLockedOut(TestOrgContact.OrgCode, TestOrgContact.OC_Email, hash));
				Assert("Precondition", ContactIsLockedOut(string.Empty, TestOrgContact.OC_Email, hash));
				Assert("Precondition", ContactIsLockedOut(otherOrg, TestOrgContact.OC_Email, hash));

				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
				page.RequestQueryString_Expose.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, TestToken_ResetPassword);

				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";

				var passwordResetHelper = page.DataSource as PasswordResetHelper;
				passwordResetHelper.CompanyCode = "WiseTech";

				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.OnUpdate();

				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);

				hash = LoadLoginHashCookie(TestOrgContact);

				AssertEquals("Should load hash from cookie", 32, hash.Length);
				Assert("Should unlock account after changing password", !ContactIsLockedOut(TestOrgContact.OrgCode, TestOrgContact.OC_Email, hash));
				Assert("Should not unlock account without hash", ContactIsLockedOut(TestOrgContact.OrgCode, TestOrgContact.OC_Email, null));
				Assert("Should not unlock account with same email but no org code", ContactIsLockedOut(string.Empty, TestOrgContact.OC_Email, null));
				Assert("Should not unlock account with different org code", ContactIsLockedOut(otherOrg, TestOrgContact.OC_Email, null));
			}
		}

		public void TestNoCache()
		{
			using (var page = ResetPasswordPage)
			{
				Assert(!page.Cacheable_Expose);
			}
		}

		public void TestOnInit_LogOff()
		{
			using (var page = ResetPasswordPage)
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

		#region Unlock Contact Helper

		void CreateLockoutUserRecord(string companyCode, string loginName)
		{
			WebDataRegistry.Instance.WebLoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 60);
			var loginFailureLog = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog.SFL_IsLockOut = true;
			loginFailureLog.SFL_LoginName = string.IsNullOrEmpty(companyCode) ? loginName : (loginName + " " + companyCode);
			loginFailureLog.SFL_TableCode = OrgContactSchema.Constants.Prefix;
			loginFailureLog.SFL_SystemCreateTimeUtc = ZDateTime.UtcNow;
		}

		bool ContactIsLockedOut(string companyCode, string loginName, byte[] hash = null)
		{
			return LoginAttemptRecorder.IsLockedOut(companyCode, loginName, hash);
		}

		byte[] LoadLoginHashCookie(OrgContact contact)
		{
			return WebApplicationLoginHelper.RetrieveLoginHashFromCookie(contact.OrgCode, contact.OC_Email);
		}

		IOrgContactLoginAttemptRecorder LoginAttemptRecorder => loginAttemptRecorder ?? (loginAttemptRecorder = ObjectFactory.Get<IOrgContactLoginAttemptRecorder>());
		IOrgContactLoginAttemptRecorder loginAttemptRecorder;

		#endregion

		ResetPasswordForTest ResetPasswordPage
		{
			get
			{
				var testPage = new ResetPasswordForTest();
				var method = typeof(Page).GetMethod("SetIntrinsics",
					BindingFlags.NonPublic | BindingFlags.Instance,
					null,
					new Type[] { typeof(HttpContext) },
					null);
				HttpContext.Current.Response.Cookies.Clear();
				method.Invoke(testPage, new object[] { HttpContext.Current });

				return testPage;
			}
		}

		OrgContact TestOrgContact;

		string TestToken_ResetPassword;
		string TestToken_LegacyResetPassword;
		string TestToken_InvalidType;

		protected override void SetUp()
		{
			base.SetUp();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "WiseTech";
			orgHeader.OH_FullName = "WiseTech Global";

			var person = Factory.NewWithValidTestData<GlbPerson>();

			TestOrgContact = orgHeader.Contacts.AddNew();
			TestOrgContact.OC_Email = "TestUser@wisetechglobal.com";
			TestOrgContact.OC_IsActive = true;
			TestOrgContact.OC_WebAccessEnabled = true;
			TestOrgContact.OC_PER = person.PK;

			// we can reset contact password only when it is not the only contact in the person relationship
			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_Email = "contact2@wisetechglobal.com";
			contact2.OC_IsActive = true;
			contact2.OC_PER = person.PK;

			UserSecretsContext.DefaultContext.SaveSecret("test", UserSecretHashAlgorithm.Pbkdf2HmacSha1, 100, TestOrgContact.GetPasswordAdapter());

			Factory.Save();

			ITokenizedAccessControl accessControl = new TokenizedAccessControl();

			var passwordResetInfo = new PasswordResetInfo { ContactEmail = "TestUser@wisetechglobal.com" };
			var scope = JsonConvert.SerializeObject(passwordResetInfo);
			TestToken_ResetPassword = accessControl.CreateLimitedToken(AccessTokenTypes.ResetPassword, new AccessTokenInfo(scope, Guid.Empty, "INV"), maxUses: 1);
			TestToken_LegacyResetPassword = accessControl.CreateLimitedToken(AccessTokenTypes.ResetPassword, new AccessTokenInfo("TestUser@wisetechglobal.com", Guid.Empty, "INV"), maxUses: 1);
			TestToken_InvalidType = accessControl.CreateLimitedToken("INV", new AccessTokenInfo("", Guid.Empty, "INV"), maxUses: 1);

			WebDataRegistry.Instance.LoginFailureAttemptSecretKey.SetValue(
				Guid.Empty,
				Guid.Empty,
				Guid.Empty,
				"8F909455805C9A77DFD61579E86B54880FBBC194CE943F08B006B5A5488FBA1EF805BF4374BED6BF653FA096AF5BE06694459160390312A0C5B69AC2019E8DA5");
		}
	}
}
