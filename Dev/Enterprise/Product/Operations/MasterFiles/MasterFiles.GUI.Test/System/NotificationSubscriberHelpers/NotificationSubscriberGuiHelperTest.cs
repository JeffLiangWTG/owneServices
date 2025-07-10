using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class NotificationSubscriberGuiHelperTest : TestCase
	{
		public void TestFakeHelperForYesNoAll_ResponseYes()
		{
			FakeHelperForYesNoAll.FakeResultForShowYesNoAllMessageBox = YesNoYesAllNoAllMessageBoxResult.Yes;
			FakeHelperForYesNoAll.QueryUser(YesNoAllArgs);
			Assert(YesNoAllArgs.Response);

			FakeHelperForYesNoAll.YesNoAllMessageBoxShown = false;

			FakeHelperForYesNoAll.QueryUser(YesNoAllArgs);
			Assert(YesNoAllArgs.Response);

			Assert(FakeHelperForYesNoAll.YesNoAllMessageBoxShown);
		}

		public void TestFakeHelperForYesNoAll_ResponseNo()
		{
			FakeHelperForYesNoAll.FakeResultForShowYesNoAllMessageBox = YesNoYesAllNoAllMessageBoxResult.No;
			FakeHelperForYesNoAll.QueryUser(YesNoAllArgs);
			Assert(!YesNoAllArgs.Response);

			FakeHelperForYesNoAll.YesNoAllMessageBoxShown = false;

			FakeHelperForYesNoAll.QueryUser(YesNoAllArgs);
			Assert(!YesNoAllArgs.Response);

			Assert(FakeHelperForYesNoAll.YesNoAllMessageBoxShown);
		}

		public void TestFakeHelperForYesNoAll_ReponseYesToAll()
		{
			FakeHelperForYesNoAll.FakeResultForShowYesNoAllMessageBox = YesNoYesAllNoAllMessageBoxResult.YesToAll;
			FakeHelperForYesNoAll.QueryUser(YesNoAllArgs);
			Assert(YesNoAllArgs.Response);

			FakeHelperForYesNoAll.YesNoAllMessageBoxShown = false;

			FakeHelperForYesNoAll.QueryUser(YesNoAllArgs);
			Assert(YesNoAllArgs.Response);

			Assert(!FakeHelperForYesNoAll.YesNoAllMessageBoxShown);
		}

		public void TestFakeHelperForYesNoAll_ResponseNoToAll()
		{
			FakeHelperForYesNoAll.FakeResultForShowYesNoAllMessageBox = YesNoYesAllNoAllMessageBoxResult.NoToAll;
			FakeHelperForYesNoAll.QueryUser(YesNoAllArgs);
			Assert(!YesNoAllArgs.Response);

			FakeHelperForYesNoAll.YesNoAllMessageBoxShown = false;

			FakeHelperForYesNoAll.QueryUser(YesNoAllArgs);
			Assert(!YesNoAllArgs.Response);

			Assert(!FakeHelperForYesNoAll.YesNoAllMessageBoxShown);
		}

		public void TestFakeHelperForYesNoCancelResponse()
		{
			QueryUserYesNoCancelEventArgs args = new QueryUserYesNoCancelEventArgs("Caption", "Message", true);
			FakeHelperForYesNoAll.MessageBoxShown = false;
			FakeHelperForYesNoAll.FakeResultForShowMessageBox = DialogResult.Yes;
			FakeHelperForYesNoAll.QueryUser(args);
			Assert(args.Response);
			Assert(!args.Cancel);
			Assert(FakeHelperForYesNoAll.MessageBoxShown);

			FakeHelperForYesNoAll.MessageBoxShown = false;
			FakeHelperForYesNoAll.FakeResultForShowMessageBox = DialogResult.No;
			FakeHelperForYesNoAll.QueryUser(args);
			Assert(!args.Response);
			Assert(!args.Cancel);
			Assert(FakeHelperForYesNoAll.MessageBoxShown);

			FakeHelperForYesNoAll.MessageBoxShown = false;
			FakeHelperForYesNoAll.FakeResultForShowMessageBox = DialogResult.Cancel;
			FakeHelperForYesNoAll.QueryUser(args);
			Assert(!args.Response);
			Assert(args.Cancel);
			Assert(FakeHelperForYesNoAll.MessageBoxShown);
		}

		public void TestDefaultableYesNoQuery()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var subscriber = new NotificationSubscriberGuiHelper();
			var message = "Message";
			var dialogDefaultContext = new DialogDefaultContext(dialogIdentifier: ZGuid.NewZGuid(),
									caption: "Caption",
									buttons: ZMessageBoxButtons.YesNo,
									icon: ZMessageBoxIcon.Warning,
									context: null,
									resultsNotToSave: new[] { ZDialogResult.No },
									showCheckboxOnly: true);

			var e = new DefaultableQueryUserEventArgs(dialogDefaultContext, message, false);

			AssertEquals("Precondition: no last message", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
			subscriber.QueryUser(e);

			AssertEquals("Postcondition: last message is correct", true, UnitTestUserNotification.Instance.LastMessage.Contains("Message"));
			AssertEquals("Postcondition: last caption is correct", true, UnitTestUserNotification.Instance.LastMessage.Caption.Contains("Caption"));
			AssertEquals("Postcondition: last message shown should have been defaultable", true, UnitTestUserNotification.Instance.LastMessage.WasDefaultable);
		}

		public void TestDefaultableYesNoCancelQuery()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var subscriber = new NotificationSubscriberGuiHelper();
			var message = "Message";
			var dialogDefaultContext = new DialogDefaultContext(dialogIdentifier: ZGuid.NewZGuid(),
						caption: "Caption",
						buttons: ZMessageBoxButtons.YesNoCancel,
						icon: ZMessageBoxIcon.Warning,
						context: null,
						resultsNotToSave: new[] { ZDialogResult.No, ZDialogResult.Cancel },
						showCheckboxOnly: true);

			var e = new DefaultableQueryUserEventArgs(dialogDefaultContext, message, false);

			AssertEquals("Precondition: no last message", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
			subscriber.QueryUser(e);

			AssertEquals("Postcondition: last message is correct", true, UnitTestUserNotification.Instance.LastMessage.Contains("Message"));
			AssertEquals("Postcondition: last caption is correct", true, UnitTestUserNotification.Instance.LastMessage.Caption.Contains("Caption"));
			AssertEquals("Postcondition: last message shown should have been defaultable", true, UnitTestUserNotification.Instance.LastMessage.WasDefaultable);
		}

		public void TestResetUpdateDuringImportFlags()
		{
			QueryUserYesNoCancelEventArgs args = new QueryUserYesNoCancelEventArgs("Caption", "Message", true);
			FakeHelperForYesNoAll.MessageBoxShown = false;
			FakeHelperForYesNoAll.FakeResultForShowMessageBox = DialogResult.Yes;
			FakeHelperForYesNoAll.QueryUser(args);
			Assert(FakeHelperForYesNoAll.MessageBoxShown);
			Assert(args.Response);

			FakeHelperForYesNoAll.ResetUpdateDuringImportFlags();
			FakeHelperForYesNoAll.QueryUser(args);
			Assert(FakeHelperForYesNoAll.MessageBoxShown);
			Assert(args.Response);
		}

		protected override void SetUp()
		{
			base.SetUp();
			FakeHelperForYesNoAll = new NotificationSubscriberGuiHelperTestClass();
			YesNoAllArgs = new QueryUserYesNoYesAllNoAllEventArgs();
		}

		NotificationSubscriberGuiHelperTestClass FakeHelperForYesNoAll;
		QueryUserYesNoYesAllNoAllEventArgs YesNoAllArgs;

		class NotificationSubscriberGuiHelperTestClass : NotificationSubscriberGuiHelper
		{
			public NotificationSubscriberGuiHelperTestClass()
			{
			}

			protected override YesNoYesAllNoAllMessageBoxResult ShowYesNoAllMessageBox(string message, string caption)
			{
				YesNoAllMessageBoxShown = true;
				return FakeResultForShowYesNoAllMessageBox;
			}

			protected override DialogResult ShowMessageBox(string message, string caption, MessageBoxButtons buttons, DialogResult defaultResult)
			{
				MessageBoxShown = true;
				return FakeResultForShowMessageBox;
			}

			public YesNoYesAllNoAllMessageBoxResult FakeResultForShowYesNoAllMessageBox;
			public DialogResult FakeResultForShowMessageBox;
			public bool YesNoAllMessageBoxShown;
			public bool MessageBoxShown;
		}
	}
}
