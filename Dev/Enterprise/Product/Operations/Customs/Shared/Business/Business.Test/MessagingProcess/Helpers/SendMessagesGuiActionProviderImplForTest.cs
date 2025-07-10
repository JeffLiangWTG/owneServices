using System;

namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	public sealed class SendMessagesGuiActionProviderImplForTest : SendMessagesBaseActionProviderForTest, ISendMessagesGuiActionProvider
	{
		#region Gui
		ActionStep ISendMessagesGuiActionProvider.ShowPreSendValidationNotifications => CommonActionStep(nameof(ISendMessagesGuiActionProvider.ShowPreSendValidationNotifications));
		ActionStep ISendMessagesGuiActionProvider.ShowFormPreSaveDialog => CommonActionStep(nameof(ISendMessagesGuiActionProvider.ShowFormPreSaveDialog));
		ActionStep ISendMessagesGuiActionProvider.ShowSendDialog => CommonActionStep(nameof(ISendMessagesGuiActionProvider.ShowSendDialog));
		ActionStep ISendMessagesGuiActionProvider.ShowSendMessagesWithErrorsSecurityCheckpointOverride => CommonActionStep(nameof(ISendMessagesGuiActionProvider.ShowSendMessagesWithErrorsSecurityCheckpointOverride));
		ActionStep ISendMessagesGuiActionProvider.ShowCreditAndDPSCheckOverride => CommonActionStep(nameof(ISendMessagesGuiActionProvider.ShowCreditAndDPSCheckOverride));
		ActionStep ISendMessagesGuiActionProvider.ShowPreviewDialog => CommonActionStep(nameof(ISendMessagesGuiActionProvider.ShowPreviewDialog));
		ActionStep ISendMessagesGuiActionProvider.ShowResultNotifications => CommonActionStep(nameof(ISendMessagesGuiActionProvider.ShowResultNotifications));

		public Action<ActionChain> ConfigProcessForTesting { get; set; }
		void ISendMessagesGuiActionProvider.ConfigureProcess(ActionChain sendChain)
		{
			ConfigProcessForTesting?.Invoke(sendChain);
		}
		#endregion
	}
}
