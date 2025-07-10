using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class SaveDataFirstTest : TestCaseWithFactory
	{
		public void TestConfirm()
		{
			var bo = Factory.New<DummyBusinessObject>();
			bo.Z0_Code = "ABC";
			using (var form = new ZForm(bo))
			{
				AssertEquals("pre-condition", true, bo.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals(false, SaveDataFirst.Confirm(bo, form));
				AssertEquals(true, bo.HasChanges);
				AssertEquals("You need to save first. Would you like to save now and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertEquals(true, SaveDataFirst.Confirm(bo, form));
				AssertEquals(false, bo.HasChanges);
				AssertEquals("You need to save first. Would you like to save now and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals(true, SaveDataFirst.Confirm(bo, form));
				AssertEquals(false, bo.HasChanges);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConfirm_DefaultCaptionAndMessage()
		{
			var bo = Factory.New<DummyBusinessObject>();
			bo.Z0_Code = "ABC";
			using (var form = new ZForm(bo))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				SaveDataFirst.Confirm(bo, form);
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Save first?", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("Message", "You need to save first. Would you like to save now and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestConfirm_CaptionAndMessageAreAllowedToCustomize()
		{
			var bo = Factory.New<DummyBusinessObject>();
			bo.Z0_Code = "ABC";
			using (var form = new ZForm(bo))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				SaveDataFirst.Confirm(bo, form, "New Caption", "New Message");
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "New Caption", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("Message", "New Message", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}
}
