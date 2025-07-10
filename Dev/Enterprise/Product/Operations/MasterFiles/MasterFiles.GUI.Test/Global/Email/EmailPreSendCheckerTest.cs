using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class EmailPreSendCheckerTest : TestCaseWithFactory
	{
		public void TestPromptUserIfSendingToNdrRecipients()
		{
			var email1 = GlbEmailAddress.LoadOrNew(Factory, "email@1.com");
			email1.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.Unverified;
			var email2 = GlbEmailAddress.LoadOrNew(Factory, "email@2.com");
			email2.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.ValidReport;
			var email3 = GlbEmailAddress.LoadOrNew(Factory, "email@3.com");
			email3.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			Factory.Save();

			var checker = new EmailPreSendChecker();
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var result = checker.PromptUserIfSendingToNdrRecipients(new ZString[] { "email@1.com", "email@2.com" });
				AssertEquals("Should not show any prompts since emails are not NDR", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Should return true if no NDR", true, result);
			}

			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var result = checker.PromptUserIfSendingToNdrRecipients(new ZString[] { "email@3.com", "email@4.com" });
				AssertEquals("At least one recipient has received a Non-Delivery Receipt. Do you want to continue to send to these recipients?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Delivering to Non-Delivery recipients", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Should return false when user selects 'No'", false, result);

				var newFactory = new BusinessObjectFactory();
				AssertEquals("Should remain NDR", EmailDeliveryReportStatus.Codes.NonDeliveryReport, GlbEmailAddress.Load(newFactory, "email@3.com").GI_DeliveryStatus);
				AssertNull("Should not change status since it was not NDR", GlbEmailAddress.Load(newFactory, "email@4.com"));
			}

			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var result = checker.PromptUserIfSendingToNdrRecipients(new ZString[] { "email@3.com", "email@4.com" });
				AssertEquals("At least one recipient has received a Non-Delivery Receipt. Do you want to continue to send to these recipients?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Delivering to Non-Delivery recipients", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Should return true when user selects 'Yes'", true, result);

				var newFactory = new BusinessObjectFactory();
				AssertEquals("Should change to unverified", ZString.Empty, GlbEmailAddress.Load(newFactory, "email@3.com").GI_DeliveryStatus);
				AssertNull("Should not change status since it was not NDR", GlbEmailAddress.Load(newFactory, "email@4.com"));
			}
		}
	}
}
