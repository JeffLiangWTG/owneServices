using System;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(EntrySummaryQueryForm))]
	sealed class EntrySummaryQueryFormTest : ZFormBasherTest
	{
		public void TestSendButtonClick()
		{
			var bizObj = new EntrySummaryQueryBizObj(Factory);
			using (var form = new EntrySummaryQueryForm(bizObj))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.SendButton_Click(form.SendButton, EventArgs.Empty);
				AssertEquals("HasMessageError", true, bizObj.HasMessageErrors);
				AssertEquals(false, bizObj.SendMessage);
				AssertEquals("There are message errors. Are you sure you wish to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				bizObj.EntryNumber = "123456789";
				form.SendButton_Click(form.SendButton, EventArgs.Empty);
				AssertEquals(true, bizObj.SendMessage);
			}
		}

		public void TestDefaultApplicationCode()
		{
			var bizObj = new EntrySummaryQueryBizObj(Factory);
			using (var form = new EntrySummaryQueryForm(bizObj))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				Assert("Expected:Default value is ACE", bizObj.ApplicationCode.Equals(JobApplicationCodeList.Codes.ACE));
			}
		}

		public void TestEntrySummariesCheckBoxCaption()
		{
			var bizObj = new EntrySummaryQueryBizObj(Factory);
			using (var form = new EntrySummaryQueryForm(bizObj))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				var checkBox = form.FindSingleOrDefault<ZCheckBox>("EntrySummariesCheckBox");
				AssertNotNull(checkBox);
				AssertEquals("Entry Summaries", checkBox.CaptionResourceString.Caption);
			}
		}

		public override void TestBashingForm() => Assert(true);

		protected override Form GetFormToBashCore() => new EntrySummaryQueryForm(new EntrySummaryQueryBizObj(Factory));
	}
}
