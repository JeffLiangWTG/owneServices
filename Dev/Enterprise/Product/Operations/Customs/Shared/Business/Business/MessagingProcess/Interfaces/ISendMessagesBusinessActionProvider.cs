namespace Enterprise.Customs.Business.MessagingProcess
{
	public interface ISendMessagesBusinessActionProvider
	{
		ActionStep SendMessagesSecurityCheckpoint { get; }
		ActionStep PreSendValidation { get; }
		ActionStep SendMessagesWithErrorsSecurityCheckpoint { get; }
		ActionStep CreditAndDPSCheck { get; }
		ActionStep CreateMessages { get; }
		ActionStep SignMessages { get; }
		ActionStep PreviewDialogFailure { get; }
		ActionStep SignMessagesFailure { get; }
		ActionStep ProcessUpdates { get; }
		ActionStep RefreshBusinessObjects { get; }

		void ConfigureProcess(ActionChain sendChain);
	}
}
