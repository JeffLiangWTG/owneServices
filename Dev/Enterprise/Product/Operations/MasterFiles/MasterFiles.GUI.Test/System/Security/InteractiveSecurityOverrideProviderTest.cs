using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class InteractiveSecurityOverrideProviderTest : SecurityOverrideProviderTestCase
	{
		public void TestRequestGrantedConfirmation()
		{
			InteractiveSecurityOverrideProviderTestClass testProvider = new InteractiveSecurityOverrideProviderTestClass();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("Has SecurityGrantedMessage", false, string.IsNullOrEmpty(testProvider.GetSecurityGrantedMessageTest(Env.Security.ReopenJob)));
			AssertEquals(SecurityCertificate.Granted, testProvider.RequestGrantedConfirmationTest(Env.Security.ReopenJob));
			Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("Has no SecurityGrantedMessage", true, string.IsNullOrEmpty(testProvider.GetSecurityGrantedMessageTest(Env.Security.ReceivablesAllowUnmatchingPaymentMatching)));
			AssertEquals(SecurityCertificate.Granted, testProvider.RequestGrantedConfirmationTest(Env.Security.ReceivablesAllowUnmatchingPaymentMatching));
			Assert("No messages shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		[GuiTest]
		[RequiresSTA]
		public void TestRequestLoginCredentials()
		{
			InteractiveSecurityOverrideProviderTestClass testProvider = new InteractiveSecurityOverrideProviderTestClass();
			AssertNull(testProvider.RequestLoginCredentialsTest(Env.Security.ReopenJob));
		}

		[RequiresSTA]
		public void TestLoginFormMessage()
		{
			bool isAllowed = Env.Security.ReopenJob.IsAllowed;
			Env.Security.ReopenJob.IsAllowed = false;

			InteractiveSecurityOverrideProviderTestClass testProvider = new InteractiveSecurityOverrideProviderTestClass();
			AssertEquals(false, ((ISecurityOverrideProvider)testProvider).SecurityCertificates[Env.Security.ReopenJob].IsAllowed);
			AssertEquals(Env.Security.ReopenJob.DefaultSecurityOverrideMessage, testProvider.LoginForm.Message);

			Env.Security.ReopenJob.IsAllowed = isAllowed;
		}

		[GuiTest]
		[RequiresSTA]
		public void TestInvalidLoginMessage()
		{
			bool isAllowed = Env.Security.ReopenJob.IsAllowed;
			Env.Security.ReopenJob.IsAllowed = false;
			InteractiveSecurityOverrideProviderTestClass testProvider = new InteractiveSecurityOverrideProviderTestClass();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AssertEquals(false, ((ISecurityOverrideProvider)testProvider).SecurityCertificates[Env.Security.ReopenJob].IsAllowed);
			AssertEquals(SecurityOverrideProvider.InvalidLoginErrorMsg, UnitTestUserNotification.Instance.LastMessage.Text);

			Env.Security.ReopenJob.IsAllowed = isAllowed;
		}

		[RequiresSTA]
		public void TestInvalidLoginMessage_WithDeactiveUser()
		{
			var newFactory = new BusinessObjectFactory();
			newFactory.Save();
			var staff = newFactory.New<GlbStaff>();
			staff.GS_LoginName = "testUser";
			staff.GS_Code = "XXX";
			staff.StaffPlainTextPassword = "testPassword";
			staff.GS_ChangePasswordAtNextLogin = false;
			staff.GS_IsSystemAccount = true;
			staff.GS_IsActive = false;
			newFactory.Save();

			var isAllowed = Env.Security.ReopenJob.IsAllowed;
			Env.Security.ReopenJob.IsAllowed = false;

			var testProvider = new InteractiveSecurityOverrideProviderTestClass();
			testProvider.Username = staff.GS_LoginName;
			testProvider.UserPassword = staff.StaffPlainTextPassword;

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AssertEquals(false, ((ISecurityOverrideProvider)testProvider).SecurityCertificates[Env.Security.ReopenJob].IsAllowed);
			AssertEquals("Your login has been disabled. Please see your system administrator to re-activate your login.", UnitTestUserNotification.Instance.LastMessage.Text);

			Env.Security.ReopenJob.IsAllowed = isAllowed;
		}

		public void TestDefaultSecurityMessage()
		{
			DefaultSecurityOverrideMessageProviderTestClass testProvider = new DefaultSecurityOverrideMessageProviderTestClass();
			AssertEquals(Env.Security.ReopenJob.DefaultSecurityOverrideMessage, testProvider.GetSecurityOverrideMessageTest(Env.Security.ReopenJob));
		}

		public void TestCustomSecurityMessage()
		{
			CustomSecurityOverrideMessageProviderTestClass testProvider = new CustomSecurityOverrideMessageProviderTestClass();
			AssertEquals("Test Message", testProvider.GetSecurityOverrideMessageTest(Env.Security.ReopenJob));
		}

		[GuiTest]
		[RequiresSTA]
		public void TestLastLoginFormResult()
		{
			Env.Security.ReopenJob.IsAllowed = false;
			DefaultSecurityOverrideMessageProviderTestClass testProvider = new DefaultSecurityOverrideMessageProviderTestClass();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AssertEquals(false, ((ISecurityOverrideProvider)testProvider).SecurityCertificates[Env.Security.ReopenJob].IsAllowed);
			AssertEquals(DialogResult.OK, testProvider.LastLoginFormResultExposed);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			AssertEquals(false, ((ISecurityOverrideProvider)testProvider).SecurityCertificates[Env.Security.ReopenJob].IsAllowed);
			AssertEquals(DialogResult.Cancel, testProvider.LastLoginFormResultExposed);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore;
			AssertEquals(false, ((ISecurityOverrideProvider)testProvider).SecurityCertificates[Env.Security.ReopenJob].IsAllowed);
			AssertEquals(DialogResult.Ignore, testProvider.LastLoginFormResultExposed);
		}

		#region Implementation

		protected override SecurityOverrideProvider GetSecurityOverrideProvider()
		{
			return new InteractiveSecurityOverrideProvider();
		}

		class InteractiveSecurityOverrideProviderTestClass : InteractiveSecurityOverrideProvider
		{
			public string Username { get; set; }
			public string UserPassword { get; set; }

			public SecurityCore RequestLoginCredentialsTest(SecurityCheckpoint checkPoint)
			{
				return base.RequestLoginCredentials(checkPoint);
			}

			public SecurityCertificate RequestGrantedConfirmationTest(SecurityCheckpoint checkPoint)
			{
				return base.RequestGrantedConfirmation(checkPoint);
			}

			public string GetSecurityGrantedMessageTest(SecurityCheckpoint checkPoint)
			{
				return GetSecurityGrantedMessage(checkPoint);
			}

			protected override LoginForm CreateNewLoginForm()
			{
				LoginForm = new LoginForm();
				LoginForm.Message = "test message";

				if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(UserPassword))
				{
					LoginForm.DoLoginForTest(Username, UserPassword);
				}

				return LoginForm;
			}

			public LoginForm LoginForm;

			protected override string GetSecurityGrantedMessage(SecurityCheckpoint checkPoint)
			{
				return checkPoint == Env.Security.ReopenJob ? "Confirm you want to reopen Jobs" : "";
			}
		}

		class DefaultSecurityOverrideMessageProviderTestClass : InteractiveSecurityOverrideProvider
		{
			public string GetSecurityOverrideMessageTest(SecurityCheckpoint checkPoint)
			{
				return base.GetSecurityOverrideMessage(checkPoint);
			}

			public DialogResult LastLoginFormResultExposed
			{
				get { return LastLoginFormResult; }
			}
		}

		class CustomSecurityOverrideMessageProviderTestClass : InteractiveSecurityOverrideProvider
		{
			public string GetSecurityOverrideMessageTest(SecurityCheckpoint checkPoint)
			{
				return this.GetSecurityOverrideMessage(checkPoint);
			}

			protected override string GetSecurityOverrideMessage(SecurityCheckpoint checkPoint)
			{
				return "Test Message";
			}
		}

		#endregion
	}
}
