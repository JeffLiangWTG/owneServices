using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(AESMessageSubmitForm))]
	sealed class AESMessageSubmitFormTest : ZFormBasherTest
	{
		[TestDate(2012, 12, 23)]
		public void TestSubmitButton_ClickForAESTIR()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			using (AESMessageSubmitForm form = new AESMessageSubmitForm(declaration))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.SubmitButton.PerformClick();
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("No SED has been flagged for submission", UnitTestUserNotification.Instance.LastMessage.Text);

				CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
				entry.CH_BGMReference = "B0002345";
				entry.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
				AssertEquals(true, entry.IsWaitingForResponse);
				AssertEquals(false, entry.US_ShouldBeReportToCustoms);
				form.SubmitButton.PerformClick();
				AssertNull(ZFormModaliser.ActiveForm);
				AssertEquals("No SED has been flagged for submission", UnitTestUserNotification.Instance.LastMessage.Text);

				entry.US_ShouldBeReportToCustoms = true;
				form.SubmitButton.PerformClick();
				AssertNull(ZFormModaliser.ActiveForm);
				AssertEquals("Submission Cancelled By User", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, entry.US_ShouldBeReportToCustoms);
				AssertEquals(ZDateTime.Empty, declaration.JE_EntrySubmittedDate);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				entry.US_ShouldBeReportToCustoms = true;
				form.SubmitButton.PerformClick();
				AssertEquals(ZDateTime.Today, declaration.JE_EntrySubmittedDate);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}
		protected override Form GetFormToBashCore() => new AESMessageSubmitForm(Factory.New<JobDeclaration>());
	}
}
