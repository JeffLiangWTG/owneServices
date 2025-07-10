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
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using Newtonsoft.Json;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class ResetMasterPasswordLifeCycleTest : ZPageLifeCycleTest
	{
		protected override ZPage GetNewPage()
		{
			return ResetMasterPasswordPage;
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
			HttpContext.Current.Request.QueryString.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
			HttpContext.Current.Request.QueryString.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, token);

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

		public void TestResetMasterPassword()
		{
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://google.com.au/");

			using (var page = ResetMasterPasswordPage)
			{
				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, TestToken);

				page.SetTable_Expose.Visible = true;
				page.PasswordChangeMessage_Expose.Visible = false;
				page.Back_Expose.Visible = false;

				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "2";

				AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);
				page.OnUpdate();

				AssertEquals("Please select a contact for login", page.PasswordChangeMessage_Expose.Text);
				Assert("Password controls should be visible", page.PasswordChangeMessage_Expose.Visible);
				Assert("Password controls should be visible", page.SetTable_Expose.Visible);
				Assert("Back link should not be visible", !page.Back_Expose.Visible);

				page.PersonForPasswordChange = TestOrgContact.Person;
				page.OnUpdate();

				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(accessControl.TryPeek(TestToken, AccessTokenTypes.ResetMasterPassword, out _));

				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.OnUpdate();
				AssertEquals("Personal password should be updated", true, TestOrgContact.Person.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				Assert("Password message should be visible", page.PasswordChangeMessage_Expose.Visible);
				Assert("Password controls should be hidden", !page.SetTable_Expose.Visible);
				Assert("Back link should have been made visible", page.Back_Expose.Visible);
				Assert(!accessControl.TryPeek(TestToken, AccessTokenTypes.ResetMasterPassword, out _));
				Assert("Should write login hash cookie", page.Response.Cookies.Keys.OfType<string>().Any(key => key.StartsWith("DeviceCookie_")));

				ClearKey(WebDataRegistry.Instance.LoginFailureAttemptSecretKey.Name);
				page.Response.Cookies.Clear();
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

		public void TestResetMasterPassword_WithoutExistingPassword()
		{
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			TestOrgContact.OC_PasswordHash = ZBlob.Empty;
			TestOrgContact.Person.RemovePasswordHash();
			Factory.Save();
			AssertEquals(false, TestOrgContact.HasPassword);
			AssertEquals(false, TestOrgContact.Person.HasPassword);

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://google.com.au/");

			using (var page = ResetMasterPasswordPage)
			{
				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, TestToken);

				page.ResetMasterPasswordHeadingLabel_Expose.Visible = true;
				page.SetTable_Expose.Visible = true;
				page.PasswordChangeMessage_Expose.Visible = false;
				page.Back_Expose.Visible = false;

				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "2";

				AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);
				page.OnUpdate();

				AssertEquals("Please select a contact for login", page.PasswordChangeMessage_Expose.Text);
				Assert("Password controls should be visible", page.PasswordChangeMessage_Expose.Visible);
				Assert("Password controls should be visible", page.SetTable_Expose.Visible);
				Assert("Back link should not be visible", !page.Back_Expose.Visible);

				page.PersonForPasswordChange = TestOrgContact.Person;
				page.OnUpdate();

				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(accessControl.TryPeek(TestToken, AccessTokenTypes.ResetMasterPassword, out _));
				AssertEquals("Should not be logged in", false, page.SiteUser.IsLoggedIn);

				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.OnUpdate();
				AssertEquals("Personal password should be updated", true, TestOrgContact.Person.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				Assert("Password message should be visible", page.PasswordChangeMessage_Expose.Visible);
				Assert("Password controls should be hidden", !page.SetTable_Expose.Visible);
				Assert("Back link should have been made visible", page.Back_Expose.Visible);
				Assert(!accessControl.TryPeek(TestToken, AccessTokenTypes.ResetMasterPassword, out _));
			}
		}

		public void TestResetMasterPassword_WithResetInfo()
		{
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			otherContact.OC_Email = TestOrgContact.OC_Email;
			otherContact.OC_IsActive = true;
			otherContact.OC_WebAccessEnabled = true;
			Factory.Save();

			AssertNotEquals("Precondition: Should be in separate organisations", otherContact.OC_OH, TestOrgContact.OC_OH);
			AssertNotEquals("Precondition: Should be on separate persons", otherContact.OC_PER, TestOrgContact.OC_PER);

			using (var page = ResetMasterPasswordPage)
			{
				WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://yahoo.com.au/");
				var companySpecificSubject = "Hello";
				var companySpecificBody = "Anyone home?";
				WebDataRegistry.Instance.MasterPasswordResetSuccessfullyEmailTemplate.SetValue(Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = companySpecificSubject, EmailBody = companySpecificBody });

				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, TestTokenWithResetInfo);

				page.SetTable_Expose.Visible = true;
				page.PasswordChangeMessage_Expose.Visible = false;
				page.Back_Expose.Visible = false;

				page.OnLoad();

				var personList = ((ResetMasterPasswordManager)page.DataSource).PersonsForBinding;
				AssertEquals("Should only show persons with contacts in the specified org from the ResetInfo", 1, personList.Count);
				AssertEquals("Should only show persons with contacts in the specified org from the ResetInfo", TestOrgContact.OC_PER, personList[0].Person.PK);

				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "2";

				page.OnUpdate();

				AssertEquals("Please select a contact for login", page.PasswordChangeMessage_Expose.Text);
				Assert("Password controls should be visible", page.PasswordChangeMessage_Expose.Visible);
				Assert("Password controls should be visible", page.SetTable_Expose.Visible);
				Assert("Back link should not be visible", !page.Back_Expose.Visible);

				page.PersonForPasswordChange = TestOrgContact.Person;
				page.OnUpdate();

				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(accessControl.TryPeek(TestTokenWithResetInfo, AccessTokenTypes.ResetMasterPassword, out _));
				AssertEquals("Should not be logged in", false, page.SiteUser.IsLoggedIn);

				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.OnUpdate();
				AssertEquals("Personal password should be updated", true, TestOrgContact.Person.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				Assert("Password message should be visible", page.PasswordChangeMessage_Expose.Visible);
				Assert("Password controls should be hidden", !page.SetTable_Expose.Visible);
				Assert("Back link should have been made visible", page.Back_Expose.Visible);
				Assert(!accessControl.TryPeek(TestTokenWithResetInfo, AccessTokenTypes.ResetMasterPassword, out _));

				var createdEmails = Env.OutgoingMailManager.EmailsCreated;
				AssertEquals("A password set confirmation should have been sent to the active web access contact", 1, createdEmails.Count);
				var email = createdEmails[0];
				AssertEquals("Contact should be the only recipient", 1, email.Recipients.Count);
				Assert("Email should match contact email", TestOrgContact.OC_Email.EqualsIgnoringCase(email.Recipients[0].Email));
				AssertContains("Email should use template of the company specified in the token scope", companySpecificSubject, email.Subject);
				AssertContains("Email should use template of the company specified in the token scope", companySpecificBody, email.Body);
			}
		}

		public void TestResetMasterPassword_UnlockContact()
		{
			var otherOrg = "OTHERORG";
			WebDataRegistry.Instance.WebLoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 60);
			CreateLockoutUserRecord(TestOrgContact.OrgCode, TestOrgContact.OC_Email);
			CreateLockoutUserRecord(string.Empty, TestOrgContact.OC_Email);
			CreateLockoutUserRecord(otherOrg, TestOrgContact.OC_Email);
			Factory.Save();

			using (var page = ResetMasterPasswordPage)
			{
				var hash = WebApplicationLoginHelper.RetrieveLoginHashFromCookie(TestOrgContact.OrgCode, TestOrgContact.OC_Email);

				AssertEquals("Precondition", null, hash);
				Assert("Precondition", ContactIsLockedOut(TestOrgContact.OrgCode, TestOrgContact.OC_Email, hash));
				Assert("Precondition", ContactIsLockedOut(string.Empty, TestOrgContact.OC_Email, hash));
				Assert("Precondition", ContactIsLockedOut(otherOrg, TestOrgContact.OC_Email, hash));

				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, TestToken);

				page.SetTable_Expose.Visible = true;
				page.PasswordChangeMessage_Expose.Visible = false;
				page.Back_Expose.Visible = false;

				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";

				AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);

				page.PersonForPasswordChange = TestOrgContact.Person;

				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(accessControl.TryPeek(TestToken, AccessTokenTypes.ResetMasterPassword, out _));

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

		void AssertOnLoadComplete_WithValidToken(object sender, EventArgs e)
		{
			var testPage = (ResetMasterPasswordForTest)TestPage;
			Assert(testPage.SetTable_Expose.Visible);
			AssertNullOrEmpty(testPage.PasswordChangeMessage_Expose.Text);
		}

		void AssertOnLoadComplete_WithInvalidToken(object sender, EventArgs e)
		{
			var testPage = (ResetMasterPasswordForTest)TestPage;
			CombineAssertions(() =>
			{
				Assert("Should not see set password controls", !testPage.SetTable_Expose.Visible);
				AssertEquals("The set link you have followed is invalid or expired.", testPage.PasswordChangeMessage_Expose.Text);
				AssertEquals(string.Empty, testPage.ResetMasterPasswordHeadingLabel_Expose.Text);
				AssertEquals(false, testPage.HeadingMessageDiv_Expose.Visible);
			});
		}

		public void TestShouldHideHeadingLabelIfOnlyOnePerson()
		{
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			Factory.Save();
			using (var page = ResetMasterPasswordPage)
			{
				WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://yahoo.com.au/");
				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, TestToken);
				page.OnLoad();
				var personList = ((ResetMasterPasswordManager)page.DataSource).PersonsForBinding;
				AssertEquals("Precondition: Should only have 1 contact/person", 1, personList.Count);
				Assert("ResetMasterPasswordHeadingLabel should not be visible if only one person", !page.ResetMasterPasswordHeadingLabel_Expose.Visible);
			}
		}
		public void TestShouldShowHeadingLabelIfMultiplePersons()
		{
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			otherContact.OC_IsActive = true;
			otherContact.OC_WebAccessEnabled = true;
			otherContact.OC_Email = TestOrgContact.OC_Email;
			Factory.Save();
			using (var page = ResetMasterPasswordPage)
			{
				WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://yahoo.com.au/");
				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, TestToken);
				page.OnLoad();
				var personList = ((ResetMasterPasswordManager)page.DataSource).PersonsForBinding;
				AssertEquals("Precondition: Should have 2 contacts/persons", 2, personList.Count);
				Assert("ResetMasterPasswordHeadingLabel should be visible if 2 persons", page.ResetMasterPasswordHeadingLabel_Expose.Visible);
			}
		}

		public void TestHeadingLabelWithExpiredPassword()
		{
			WebDataRegistry.Instance.WebPasswordRotationDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 30);
			WebDataRegistry.Instance.WebPasswordRotationEffectiveDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(-35).ToDateTime());
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://yahoo.com.au/");

			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			otherContact.OC_IsActive = true;
			otherContact.OC_WebAccessEnabled = true;
			otherContact.OC_Email = TestOrgContact.OC_Email;
			Factory.Save();

			using (var page = ResetMasterPasswordPage)
			{
				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, TestToken);
				page.OnLoad();
				var personList = ((ResetMasterPasswordManager)page.DataSource).PersonsForBinding;
				Assert(page.ResetMasterPasswordHeadingLabel_Expose.Visible);
				Assert(!page.PasswordExpiredMessageLabel_Expose.Visible);
			}

			using (var page = ResetMasterPasswordPage)
			{
				page.RequestQueryString_Expose.Remove(TrackingConstants.QueryStringKeys.ResetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(TrackingConstants.QueryStringKeys.ResetPasswordKey, TestToken);
				page.RequestQueryString_Expose.Remove("ref");
				HttpContext.Current.Request.QueryString.Add("ref", "exp");
				page.OnLoad();
				var personList = ((ResetMasterPasswordManager)page.DataSource).PersonsForBinding;
				Assert(page.ResetMasterPasswordHeadingLabel_Expose.Visible);
				Assert(page.PasswordExpiredMessageLabel_Expose.Visible);
			}
		}

		public void TestOnInit_LogOff()
		{
			using (var page = ResetMasterPasswordPage)
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

		ResetMasterPasswordForTest ResetMasterPasswordPage
		{
			get
			{
				var testPage = new ResetMasterPasswordForTest();
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
		GlbBranch Branch;

		string TestToken;
		string TestToken_Expired;
		string TestToken_InvalidType;
		string TestTokenWithResetInfo;

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
			Branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();
			TestOrgContact.Person.PER_FullName = "Franky";
			TestOrgContact.Person.SetHashedPassword("5678");
			Factory.Save();

			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			TestToken = accessControl.CreateLimitedToken(AccessTokenTypes.ResetMasterPassword, new AccessTokenInfo(TestOrgContact.OC_Email, Guid.Empty, "INV"), maxUses: 1);
			TestToken_Expired = accessControl.CreateLimitedToken(AccessTokenTypes.ResetMasterPassword, new AccessTokenInfo(TestOrgContact.OC_Email, Guid.Empty, "INV"), TimeSpan.FromHours(-1), 1);
			TestToken_InvalidType = accessControl.CreateLimitedToken("INV", new AccessTokenInfo(TestOrgContact.OC_Email, Guid.Empty, "INV"), maxUses: 1);

			var resetInfo = new PasswordResetInfo()
			{
				ContactEmail = TestOrgContact.Email,
				OrgCode = TestOrgContact.OrgCode,
				EmailTemplateCompanyPk = Branch.Company.PK.ToString()
			};
			var jsonScope = JsonConvert.SerializeObject(resetInfo);
			TestTokenWithResetInfo = accessControl.CreateLimitedToken(AccessTokenTypes.ResetMasterPassword, new AccessTokenInfo(jsonScope, Guid.Empty, "INV"), maxUses: 1);

			WebDataRegistry.Instance.LoginFailureAttemptSecretKey.SetValue(
				Guid.Empty,
				Guid.Empty,
				Guid.Empty,
				"8F909455805C9A77DFD61579E86B54880FBBC194CE943F08B006B5A5488FBA1EF805BF4374BED6BF653FA096AF5BE06694459160390312A0C5B69AC2019E8DA5");
		}
	}
}
