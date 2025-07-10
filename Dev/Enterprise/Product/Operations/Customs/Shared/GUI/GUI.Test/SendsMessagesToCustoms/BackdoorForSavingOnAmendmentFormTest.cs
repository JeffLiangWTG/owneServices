using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(BackdoorForSavingOnAmendmentForm))]
	sealed class BackdoorForSavingOnAmendmentFormTest : ZFormBasherTest
	{
		public void TestCancelButtonClick()
		{
			DeferredAmendmentSavingOptions savingOptions = new DeferredAmendmentSavingOptions();
			using (BackdoorForSavingOnAmendmentForm form = new BackdoorForSavingOnAmendmentForm(savingOptions))
			{
				form.Show();
				AssertEquals("SavingOptions default value is NOT cancelled", false, savingOptions.IsCancelled);

				form.CancelButton2.PerformClick();
				AssertEquals("SavingOptions cancelled", true, savingOptions.IsCancelled);
			}
		}

		public void TestExplanationButtonsClick()
		{
			DeferredAmendmentSavingOptions savingOptions = new DeferredAmendmentSavingOptions();
			using (BackdoorForSavingOnAmendmentForm form = new BackdoorForSavingOnAmendmentForm(savingOptions))
			{
				form.Show();
				Assert("SendWithoutSendingAmendmentAtAll is NOT ReadOnly", !form.SendWithoutSendingAmendmentAtAllRadioButton.ReadOnly);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendAmendmentExplanationButton.PerformClick();
				ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Should be explanation for send an amendment", BackdoorForSavingOnAmendmentForm.SendAmendmentExplanation, lastMessage);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SaveWithoutEntryChangesExplanationButton.PerformClick();
				lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Should be explanation for saving without entry changes", BackdoorForSavingOnAmendmentForm.SaveWithoutEntryChangesExplanation, lastMessage);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SaveWithEntryChangesExplanationButton.PerformClick();
				lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Should be explanation for saving with entry changes", BackdoorForSavingOnAmendmentForm.SaveWithEntryChangesExplanation, lastMessage);
			}
		}

		public void TestSendWithoutSendingAmendmentAtAllReadOnly()
		{
			DeferredAmendmentSavingOptionsHelper savingOptions = new DeferredAmendmentSavingOptionsHelper();
			savingOptions.SignificantAmendmentsExposed = true;
			using (BackdoorForSavingOnAmendmentForm form = new BackdoorForSavingOnAmendmentForm(savingOptions))
			{
				form.Show();
				Assert("SendWithoutSendingAmendmentAtAll is ReadOnly", form.SendWithoutSendingAmendmentAtAllRadioButton.ReadOnly);
			}
		}

		protected override Form GetFormToBashCore() => new BackdoorForSavingOnAmendmentForm(new DeferredAmendmentSavingOptions());

		sealed class DeferredAmendmentSavingOptionsHelper : DeferredAmendmentSavingOptions
		{
			public bool SignificantAmendmentsExposed
			{
				get { return SignificantAmendments; }
				set { SignificantAmendments = value; }
			}
		}
	}
}
