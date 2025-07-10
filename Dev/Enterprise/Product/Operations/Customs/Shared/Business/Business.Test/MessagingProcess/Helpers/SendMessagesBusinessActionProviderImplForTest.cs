using System;

namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	sealed class SendMessagesBusinessActionProviderImplForTest : SendMessagesBaseActionProviderForTest, ISendMessagesBusinessActionProvider
	{
		public SendMessagesBusinessActionProviderImplForTest(bool setSign = true)
		{
			if (setSign)
			{
				SignMessagesForTesting = CommonActionStep(nameof(ISendMessagesBusinessActionProvider.SignMessages));
			}
		}

		public ActionStep SignMessagesForTesting { get; set; }

		#region Business
		ActionStep ISendMessagesBusinessActionProvider.SendMessagesSecurityCheckpoint => CommonActionStep(nameof(ISendMessagesBusinessActionProvider.SendMessagesSecurityCheckpoint));
		ActionStep ISendMessagesBusinessActionProvider.PreSendValidation => CommonActionStep(nameof(ISendMessagesBusinessActionProvider.PreSendValidation));
		ActionStep ISendMessagesBusinessActionProvider.SendMessagesWithErrorsSecurityCheckpoint => CommonActionStep(nameof(ISendMessagesBusinessActionProvider.SendMessagesWithErrorsSecurityCheckpoint));
		ActionStep ISendMessagesBusinessActionProvider.CreditAndDPSCheck => CommonActionStep(nameof(ISendMessagesBusinessActionProvider.CreditAndDPSCheck));
		ActionStep ISendMessagesBusinessActionProvider.CreateMessages => CommonActionStep(nameof(ISendMessagesBusinessActionProvider.CreateMessages));
		ActionStep ISendMessagesBusinessActionProvider.SignMessages => SignMessagesForTesting;
		ActionStep ISendMessagesBusinessActionProvider.PreviewDialogFailure => CommonActionStep(nameof(ISendMessagesBusinessActionProvider.PreviewDialogFailure));
		ActionStep ISendMessagesBusinessActionProvider.SignMessagesFailure => CommonActionStep(nameof(ISendMessagesBusinessActionProvider.SignMessagesFailure));
		ActionStep ISendMessagesBusinessActionProvider.ProcessUpdates => CommonActionStep(nameof(ISendMessagesBusinessActionProvider.ProcessUpdates));
		ActionStep ISendMessagesBusinessActionProvider.RefreshBusinessObjects => CommonActionStep(nameof(ISendMessagesBusinessActionProvider.RefreshBusinessObjects));

		public Action<ActionChain> ConfigProcessForTesting { get; set; }
		void ISendMessagesBusinessActionProvider.ConfigureProcess(ActionChain sendChain)
		{
			ConfigProcessForTesting?.Invoke(sendChain);
		}
		#endregion
	}
}
