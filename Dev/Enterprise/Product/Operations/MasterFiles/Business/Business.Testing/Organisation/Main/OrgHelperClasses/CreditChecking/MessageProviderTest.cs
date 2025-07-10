using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.MessageProvider;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MessageProviderTest : TestCaseWithFactory
	{
		public void TestDefaultMessageProvider()
		{
			MessageProvider = new DefaultMessageProvider();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
			AssertEquals("Should be Denied", ConfirmationResult.Denied, MessageProvider.PromptForConfirm());
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
			AssertEquals("Should be Denied", ConfirmationResult.Denied, MessageProvider.PromptForConfirm());
		}

		public void TestNonInteractiveMessageProvider()
		{
			NonInteractiveMessageProvider nonInteractive = new NonInteractiveMessageProvider();
			MessageProvider = nonInteractive;
			nonInteractive.ISOKToProceed = ConfirmationResult.Confirmed;
			AssertEquals("Should be Confirmed", ConfirmationResult.Confirmed, MessageProvider.PromptForConfirm());
			nonInteractive.ISOKToProceed = ConfirmationResult.Denied;
			AssertEquals("Should be Denied", ConfirmationResult.Denied, MessageProvider.PromptForConfirm());
		}

		IMessageProvider MessageProvider;
	}
}
