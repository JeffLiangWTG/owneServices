using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingPasswordResetHelper))]
	[HttpContextEnabledTest]
	sealed class TrackingPasswordResetHelperTest : PasswordResetHelperTest
	{
		public void TestRequestPasswordReset_HasPersonPassword()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_Email = "testuserAAA@cargowise.com";
			contact1.OC_ContactName = "User A";
			contact1.OC_PER = person1.PK;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_IsActive = true;
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_Email = "testuserBBB@cargowise.com";
			contact2.OC_ContactName = "User B";
			contact2.OC_PER = person2.PK;
			contact2.Person.SetHashedPassword("password");
			Factory.Save();
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			otherContact.OC_IsActive = true;
			otherContact.OC_WebAccessEnabled = true;
			otherContact.OC_Email = "other@email.com";
			otherContact.OC_PER = person1.PK;
			Factory.Save();

			AssertEquals("Precondition", false, ((IPasswordInstructionEmailSource)contact1).ShouldSendMasterPassword);
			AssertEquals("Precondition", true, ((IPasswordInstructionEmailSource)contact2).ShouldSendMasterPassword);

			TestPasswordReset.EmailAddress = contact1.OC_Email;

			using (var page = TestPage)
			{
				AssertEquals("Precondition", ExpectedEmailSentMessage, TestPasswordReset.RequestPasswordReset(page));
				TestPasswordReset.EmailAddress = contact2.OC_Email;
				AssertEquals("Precondition", ExpectedEmailSentMessage, TestPasswordReset.RequestPasswordReset(page));

				AssertEquals("Precondition: Should have sent an email for each contact", 2,
					Env.OutgoingMailManager.EmailsCreated.Count);
				var contact1Email = Env.OutgoingMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(contact1.OC_Email));
				var contact2Email = Env.OutgoingMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(contact2.OC_Email));

				AssertContains("Should send to normal password reset since master password has never been set", TrackingConstants.RelativePath.ResetPasswordPage, contact1Email.Body);
				AssertContains("Should send to master password page since it has been set before", TrackingConstants.RelativePath.ResetMasterPasswordPage, contact2Email.Body);
				Assert("Should not have log since it doesn't necessarily send to a single contact", !contact1.GetLogs().HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.WebAccessPasswordEmailSentCode));
				Assert("Should not have log since it doesn't necessarily send to a single contact", !contact2.GetLogs().HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.WebAccessPasswordEmailSentCode));
				Assert("Should not have log since it doesn't necessarily send to a single contact", !contact2.Person.GetLogs().HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.WebAccessPasswordEmailSentCode));
			}
		}

		public void TestRequestPasswordResetForValidWebUser()
		{
			TestOrgHeader.CompanyData.OB_GB_ControllingBranch = Env.CurrentBranchPK;
			TestPasswordReset.EmailAddress = TestOrgContactWithWebAccess.OC_Email;
			Factory.Save();

			using (var page = TestPage)
			{
				AssertEquals("Message", ExpectedEmailSentMessage, TestPasswordReset.RequestPasswordReset(page));
				var expectedRecipients = new StringCollection();
				expectedRecipients.Add(TestOrgContactWithWebAccess.OC_Email);

				var stmToken = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Scope,
					TestOrgContactWithWebAccess.OC_Email));
				AssertNotNull(stmToken);

				AssertEmailDetails(expectedRecipients,
					Parser.Parse(TestPasswordReset.DefaultOrgContactExposed,
						WebDataRegistry.Instance.PasswordResetEmailTemplate.Value.EmailSubject),
					TestOrgContactWithWebAccess.OC_ContactName, stmToken.SAT_Token);
			}
		}

		public void TestRequestPasswordResetForNoWebAccount()
		{
			CreateStaff("staff1@cargowise.com");
			CreateStaff("staff2@cargowise.com");
			Factory.Save();

			TestPasswordReset.EmailAddress = TestOrgContactWithoutWebAccess.OC_Email;

			using (var page = TestPage)
			{
				AssertEquals("Message", ExpectedEmailSentMessage, TestPasswordReset.RequestPasswordReset(page));
				var expectedRecipients = new StringCollection();
				expectedRecipients.Add("staff1@cargowise.com");
				expectedRecipients.Add("staff2@cargowise.com");

				AssertEmailDetails(expectedRecipients,
					FormattableString.Invariant($"{CompanyName} Website Password Reset Request: {TestOrgContactWithoutWebAccess.OC_Email}"),
					FormattableString.Invariant($"{CompanyName} Website Password Reset Request: {TestOrgContactWithoutWebAccess.OC_Email}"),
					TestOrgContactWithoutWebAccess.OC_ContactName, TestOrgContactWithoutWebAccess.OrganisationCode);
			}
		}

		public void TestUsesSameGenericMessageForUnknownEmail()
		{
			CreateStaff("staff1@cargowise.com");
			CreateStaff("staff2@cargowise.com");
			Factory.Save();

			TestPasswordReset.EmailAddress = "abracadabra@b.com";

			using (var page = TestPage)
			{
				AssertEquals("Message", ExpectedEmailSentMessage, TestPasswordReset.RequestPasswordReset(page));
			}
		}

		public void TestAdminEmailsAreInAdminLanguage()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockChs = Res.UseMockData())
			{
				mockChs.EnableRot13();

				CreateStaff("staff1@cargowise.com");
				CreateStaff("staff2@cargowise.com");
				Factory.Save();
				var expectedRecipients = new StringCollection();
				expectedRecipients.Add("staff1@cargowise.com");
				expectedRecipients.Add("staff2@cargowise.com");

				TestPasswordReset.EmailAddress = TestOrgContactWithoutWebAccess.OC_Email;

				using (var page = TestPage)
				{
					TestPasswordReset.RequestPasswordReset(page);
					AssertEmailDetails(expectedRecipients,
						FormattableString.Invariant($"{CompanyName} Website Password Reset Request: {TestOrgContactWithoutWebAccess.OC_Email}"),
						FormattableString.Invariant($"{CompanyName} Website Password Reset Request: {TestOrgContactWithoutWebAccess.OC_Email}"),
						"A user without an active web account has requested a password reset, however this was not delivered because their account is not enabled for web access.");
				}
			}
		}

		public void TestRequestPasswordReset_ShouldSendPersonPassword()
		{
			TestPasswordReset.EmailAddress = TestOrgContactWithWebAccess.OC_Email;

			AssertEquals("Precondition", true, ((IPasswordInstructionEmailSource)TestOrgContactWithWebAccess).ShouldSendMasterPassword);

			using (var page = TestPage)
			{
				AssertEquals("Precondition: Should be no emails", 0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
				var message = TestPasswordReset.RequestPasswordReset(page);
				AssertEquals("Precondition: Should show password send email", ExpectedEmailSentMessage, message);

				AssertEquals("Precondition: Should have sent an email for the contact", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
				var contact1Email = EnvProxy.Instance.OutgoingMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(TestOrgContactWithWebAccess.OC_Email));

				AssertContains("Should send to master password reset as per ShouldSendMasterPassword", page.AppInstance.ResetMasterPasswordPage, contact1Email.Body);
				AssertContains("Should send to master password reset as per ShouldSendMasterPassword", TrackingConstants.QueryStringKeys.ResetPasswordKey, contact1Email.Body);
				Assert("Should not have log since it doesn't necessarily send to a single contact", !TestOrgContactWithWebAccess.GetLogs().HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.WebAccessPasswordEmailSentCode));
			}
		}

		public void TestRequestPasswordReset_ShouldConsiderHttpsBehindLoadBalancer()
		{
			TestPasswordReset.EmailAddress = TestOrgContactWithWebAccess.OC_Email;

			using var page = TestPage;
			HttpContext.Current.Request.SetRequestHeader("X-Forwarded-Proto", "https");

			AssertEquals("Precondition: Should be no emails", 0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);

			var message = TestPasswordReset.RequestPasswordReset(page);
			AssertEquals("Should show password send email", ExpectedEmailSentMessage, message);
			AssertEquals("Should have sent an email for the contact", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);

			var contactEmail = EnvProxy.Instance.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(TestOrgContactWithWebAccess.OC_Email));
			AssertContains("Should contain reset url with correct protocol", "https://127.0.0.1/webapp/Admin/ResetMasterPassword.aspx", contactEmail.Body);
		}

		readonly string ExpectedEmailSentMessage = "If the email address is linked to a valid account, a reset password link has been sent.";

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TrackingPasswordResetHelperForTest(Factory);
		}

		new TrackingPasswordResetHelperForTest TestPasswordReset =>
			(TrackingPasswordResetHelperForTest)base.TestPasswordReset;

		PageForPasswordResetHelperTest TestPage
		{
			get
			{
				if (fTestPage == null)
				{
					fTestPage = new PageForPasswordResetHelperTest();
					var method = typeof(Page).GetMethod("SetIntrinsics",
						BindingFlags.NonPublic | BindingFlags.Instance,
						null,
						new[] { typeof(HttpContext) },
						null);
					method.Invoke(fTestPage, new object[] { HttpContext.Current });
				}

				return fTestPage;
			}
		}

		PageForPasswordResetHelperTest fTestPage;

		#endregion
	}
}
