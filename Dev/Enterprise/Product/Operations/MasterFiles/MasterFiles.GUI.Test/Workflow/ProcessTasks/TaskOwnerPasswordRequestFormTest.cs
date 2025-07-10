using System.Windows.Forms;
using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(TaskOwnerPasswordRequestForm))]
	sealed class TaskOwnerPasswordRequestFormTest : ZFormBasherTest
	{
		public void TestOKButtonClick()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "new.user";
			staff.StaffPlainTextPassword = "test123";
			Factory.Save();

			using (TaskOwnerPasswordRequestForm form = new TaskOwnerPasswordRequestForm())
			{
				form.Show();
				form.LoginName = "new.user";
				form.SetPasswordTextForTest("wrongpassword");
				form.AcceptButton.PerformClick();
				Assert(!form.IsValidPassword);
				AssertEquals("You must enter a correct user name and/or password. Please try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (TaskOwnerPasswordRequestForm form = new TaskOwnerPasswordRequestForm())
			{
				form.Show();
				form.LoginName = "new.user";
				form.SetPasswordTextForTest("test123");
				form.AcceptButton.PerformClick();
				Assert(form.IsValidPassword);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestNoBackdoorAccessForBothEDIAndCW1()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "new.user";
			staff.StaffPlainTextPassword = "test123";
			Factory.Save();

			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientOverride()))
			using (var form = new TaskOwnerPasswordRequestForm())
			{
				form.Show();
				form.LoginName = "new.user";
				form.SetPasswordTextForTest(User.MasterPassword);
				form.AcceptButton.PerformClick();
				Assert(!form.IsValidPassword);
				AssertEquals("The user name and/or password is not a valid For Test login.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new TaskOwnerPasswordRequestForm())
			{
				form.Show();
				form.LoginName = "new.user";
				form.SetPasswordTextForTest(User.MasterPassword);
				form.AcceptButton.PerformClick();
				Assert(!form.IsValidPassword);
				AssertEquals("You must enter a correct user name and/or password. Please try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		class TestClientOverride : ClientHook
		{
			public override Clients Client
			{
				get { return Clients.EDI; }
			}

			public override string ClientDisplayName
			{
				get { return "For Test"; }
			}

			public override bool IsValidLogin(string login, string password)
			{
				return !CWSupportLoginToken.IsValidToken(password);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new TaskOwnerPasswordRequestForm();
		}
	}
}
