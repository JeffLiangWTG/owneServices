namespace Enterprise.Customs.Business.MessagingProcess
{
	public interface ISendMessagesGuiActionProvider
	{
		ActionStep ShowPreSendValidationNotifications { get; }
		ActionStep ShowFormPreSaveDialog { get; }
		ActionStep ShowSendDialog { get; }
		ActionStep ShowSendMessagesWithErrorsSecurityCheckpointOverride { get; }
		ActionStep ShowCreditAndDPSCheckOverride { get; }
		ActionStep ShowPreviewDialog { get; }
		ActionStep ShowResultNotifications { get; }

		void ConfigureProcess(ActionChain sendChain);
	}
}
