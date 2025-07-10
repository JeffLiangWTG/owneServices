namespace Enterprise.MasterFiles.Business.MessageProvider
{
	public enum ConfirmationResult
	{
		Confirmed,
		Denied
	}

	public interface IMessageProvider
	{
		ConfirmationResult PromptForConfirm();
	}

	public class DefaultMessageProvider : IMessageProvider
	{
		public ConfirmationResult PromptForConfirm()
		{
			return ConfirmationResult.Denied;
		}
	}

	class NonInteractiveMessageProvider : IMessageProvider
	{
		public ConfirmationResult PromptForConfirm()
		{
			fWasPromptForConfirmCalled = true;
			return ISOKToProceed;
		}

		public ConfirmationResult ISOKToProceed
		{
			get { return fIsOKToProceed; }
			set { fIsOKToProceed = value; }
		}

		ConfirmationResult fIsOKToProceed;

		public bool WasPromptForConfirmCalled
		{
			get { return fWasPromptForConfirmCalled; }
		}

		bool fWasPromptForConfirmCalled;

		public void ResetPromptForConfirm()
		{
			fWasPromptForConfirmCalled = false;
		}
	}
}
