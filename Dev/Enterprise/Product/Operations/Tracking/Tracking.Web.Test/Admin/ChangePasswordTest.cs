using System;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class ChangePasswordTest : BaseTrackingPageTest
	{
		public void TestChangePassword_NotLoggedIn()
		{
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			Factory.Save();

			using (var page = ChangePasswordPage)
			{
				page.ContactsBox_Expose.Visible = true;
				page.SetPasswordBox_Expose.Visible = true;
				AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);
				page.OnLoad();
				AssertEquals("Please log in before attempting to change your password.", page.PasswordChangeMessage_Expose.Text);
				AssertEquals("Should not have been logged in", false, page.SiteUser.IsLoggedIn);
				Assert("Password controls should be hidden", !page.ContactsBox_Expose.Visible);
				Assert("Password controls should be hidden", !page.SetPasswordBox_Expose.Visible);
			}
		}

		public void TestChangePassword_LoginExpires()
		{
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			Factory.Save();

			using (var page = ChangePasswordPage)
			{
				page.SiteUser.Login(TestOrgContact.OrgCode, TestOrgContact.OC_Email, "5678");
				AssertEquals("Precondition", true, page.SiteUser.IsLoggedIn);

				page.ContactsBox_Expose.Visible = true;
				page.SetPasswordBox_Expose.Visible = true;
				page.OnLoad();
				Assert("Precondition: Password controls should be shown", page.ContactsBox_Expose.Visible);
				Assert("Precondition: Password controls should be shown", page.SetPasswordBox_Expose.Visible);
				AssertEquals(string.Empty, page.PasswordChangeMessage_Expose.Text);

				page.SiteUser.Logout();
				AssertEquals("Precondition: Simulating session expiry", false, page.SiteUser.IsLoggedIn);
				page.OnUpdate();
				AssertEquals("Your session has expired. Please login again before attempting to change your password.", page.PasswordChangeMessage_Expose.Text);
				AssertEquals("Should not have been logged in", false, page.SiteUser.IsLoggedIn);
				Assert("Password controls should be hidden", !page.ContactsBox_Expose.Visible);
				Assert("Password controls should be hidden", !page.SetPasswordBox_Expose.Visible);
			}
		}

		public void TestChangePassword()
		{
			using (var page = ChangePasswordPage)
			{
				page.SiteUser.Login(TestOrgContact.OrgCode, TestOrgContact.OC_Email, "5678");
				AssertEquals("Precondition: Should be logged in", true, page.SiteUser.IsLoggedIn);

				page.ContactsBox_Expose.Visible = true;
				page.ChangePasswordInstructionsLabel_Expose.Visible = true;
				page.NewPassword_Expose.Visible = true;
				page.NewPasswordConfirm_Expose.Visible = true;
				page.Update_Expose.Visible = true;
				page.OnLoad();
				Assert("Password controls should be visible", page.ContactsBox_Expose.Visible);
				Assert("Password controls should be visible", page.ChangePasswordInstructionsLabel_Expose.Visible);
				Assert("Password controls should be visible", page.SetPasswordBox_Expose.Visible);
				page.CurrentPassword_Expose.Text = "5678";
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";

				page.OnUpdate();
				AssertEquals("Personal password should be updated", true, TestOrgContact.Person.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				AssertEquals("Should remain logged in", true, page.SiteUser.IsLoggedIn);
				Assert("Password controls should be hidden", !page.ContactsBox_Expose.Visible);
				Assert("Password controls should be hidden", !page.ChangePasswordInstructionsLabel_Expose.Visible);
				Assert("Password controls should be hidden", !page.SetPasswordBox_Expose.Visible);
			}
		}

		public void TestChangePassword_NoPersonPassword()
		{
			TestOrgContact.Person.RemovePasswordHash();
			TestOrgContact.SetHashedPassword("1234");
			Factory.Save();

			using (var page = ChangePasswordPage)
			{
				page.SiteUser.Login(TestOrgContact.OrgCode, TestOrgContact.OC_Email, "1234");
				AssertEquals("Precondition: Should be logged in", true, page.SiteUser.IsLoggedIn);

				page.ContactsBox_Expose.Visible = true;
				page.ChangePasswordInstructionsLabel_Expose.Visible = true;
				page.SetPasswordBox_Expose.Visible = true;
				page.OnLoad();
				Assert("linked contacts table should be hidden", !page.ContactsBox_Expose.Visible);
				Assert("linked contact instructions should be hidden", !page.ChangePasswordInstructionsLabel_Expose.Visible);
				Assert("Password controls should be visible", page.SetPasswordBox_Expose.Visible);
				page.CurrentPassword_Expose.Text = "1234";
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";

				page.OnUpdate();
				AssertEquals("Personal password should be empty (unchanged)", true, TestOrgContact.Person.PER_PasswordHash.IsEmpty);
				AssertEquals("Personal password should be empty (unchanged)", false, TestOrgContact.Person.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Contact password should be updated", true, TestOrgContact.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				AssertEquals("Should remain logged in", true, page.SiteUser.IsLoggedIn);
				Assert("Password controls should be hidden", !page.ContactsBox_Expose.Visible);
				Assert("Password controls should be hidden", !page.ChangePasswordInstructionsLabel_Expose.Visible);
				Assert("Password controls should be hidden", !page.SetPasswordBox_Expose.Visible);
			}
		}

		public void TestNoCache()
		{
			using (var page = ChangePasswordPage)
			{
				Assert(!page.Cacheable_Expose);
			}
		}

		class ChangePasswordForTest : Admin.ChangePassword
		{
			protected override Uri RequestUrl => new Uri("http://www.test.com/WebTracker/Admin/ChangePassword.aspx");

			public ChangePasswordForTest()
			{
				PasswordChangeMessage = new Label();
				ChangePasswordInstructionsLabel = new ZTextLabel();
				ContactsBox = new HtmlGenericControl();
				CurrentPassword = new TextBox();
				NewPassword = new TextBox();
				NewPasswordConfirm = new TextBox();
				SetPasswordBox = new HtmlGenericControl();
				Update = new Button();
				RelatedAccounts = new ZTextLabelNoEncode();
			}

			public void OnLoad()
			{
				base.OnLoad(EventArgs.Empty);
			}

			public bool Cacheable_Expose => Cacheable;

			public Label PasswordChangeMessage_Expose => PasswordChangeMessage;

			public HtmlGenericControl ContactsBox_Expose => ContactsBox;

			public Label ChangePasswordInstructionsLabel_Expose => ChangePasswordInstructionsLabel;

			public TextBox CurrentPassword_Expose => CurrentPassword;

			public TextBox NewPassword_Expose => NewPassword;

			public TextBox NewPasswordConfirm_Expose => NewPasswordConfirm;

			public Button Update_Expose => Update;

			public HtmlGenericControl SetPasswordBox_Expose => SetPasswordBox;

			public void OnUpdate()
			{
				Update_Click(this, EventArgs.Empty);
			}

			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			class GlobalForTest : Global
			{
				public void OnCustomSessionStart()
				{
					base.OnCustomSessionStart(this, EventArgs.Empty);
				}
			}
		}

		ChangePasswordForTest ChangePasswordPage
		{
			get
			{
				var testPage = new ChangePasswordForTest();
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

		protected override void SetUp()
		{
			base.SetUp();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "WTG";
			orgHeader.OH_FullName = "WiseTech Global";

			TestOrgContact = orgHeader.Contacts.AddNew();
			TestOrgContact.OC_Email = "TestUser@wisetechglobal.com";
			TestOrgContact.OC_IsActive = true;
			TestOrgContact.OC_WebAccessEnabled = true;
			Assert("Precondition", !orgHeader.OH_Code.IsEmpty);
			Factory.Save();
			TestOrgContact.Person.PER_FullName = "Franky";
			TestOrgContact.Person.SetHashedPassword("5678");
			Factory.Save();
		}

		#region Overrides

		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.ChangePassword;
		}

		protected override Control GetNewControl()
		{
			return new ChangePasswordForTest();
		}

		#endregion
	}
}
