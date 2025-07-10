using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ImportRelatedActivityLinkInquiryToOrganisationPromptUserDeciderTest : TestCaseWithFactory
	{
		public void TestGetValue()
		{
			using (var form = new ZForm())
			{
				var provider = new ImportRelatedActivityPromptUserYesNoDecider(form);
				provider.DecideReason = (NoResString)"My Reason";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertEquals(true, provider.GetDecision((NoResString)"My Question?"));
				AssertEquals("My Reason", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("My Question?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals(false, provider.GetDecision((NoResString)"My Question 2?"));
				AssertEquals("My Reason", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("My Question 2?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
