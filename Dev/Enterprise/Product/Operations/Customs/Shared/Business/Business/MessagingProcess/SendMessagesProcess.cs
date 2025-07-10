namespace Enterprise.Customs.Business.MessagingProcess
{
	public sealed class SendMessagesProcess
	{
		public SendMessagesProcess(ISendMessagesBusinessActionProvider bActionProvider = null, ISendMessagesGuiActionProvider gActionProvider = null)
		{
			businessActionProvider = bActionProvider;
			guiActionProvider = gActionProvider;
		}
		readonly ISendMessagesBusinessActionProvider businessActionProvider;
		readonly ISendMessagesGuiActionProvider guiActionProvider;

		public static ActionResult SendMessages(ISendMessagesBusinessActionProvider bActionProvider = null, ISendMessagesGuiActionProvider gActionProvider = null)
		{
			return new SendMessagesProcess(bActionProvider, gActionProvider).SendMessages();
		}

		public ActionResult SendMessages()
		{
			var result = SendProcessChain.ProcessChain();

			result = ResultProcessChain.ProcessChain(result);

			return result;
		}

		public ActionChain SendProcessChain => sendProcessChain ?? (sendProcessChain = CreateSendProcessChain());
		ActionChain sendProcessChain;

		public ActionChain ResultProcessChain => resultProcessChain ?? (resultProcessChain = CreateResultProcessChain());
		ActionChain resultProcessChain;

		ActionChain CreateSendProcessChain()
		{
			var sendChain = new ActionChain(SendMessageActions.ShowFormPreSaveDialog, guiActionProvider?.ShowFormPreSaveDialog);

			sendChain.AppendAction(SendMessageActions.SendMessagesSecurityCheckpoint, businessActionProvider?.SendMessagesSecurityCheckpoint)
					 .AppendAction(SendMessageActions.ShowSendDialog, guiActionProvider?.ShowSendDialog)
					 .AppendAction(SendMessageActions.PreSendValidation, businessActionProvider?.PreSendValidation)
					 .AppendAction(SendMessageActions.ShowPreSendValidationNotifications, guiActionProvider?.ShowPreSendValidationNotifications)
					 .AppendAction(SendMessageActions.SendMessagesWithErrorsSecurityCheckpoint, businessActionProvider?.SendMessagesWithErrorsSecurityCheckpoint)
					 .AppendAction(SendMessageActions.ShowSendMessagesWithErrorsSecurityCheckpointOverride, guiActionProvider?.ShowSendMessagesWithErrorsSecurityCheckpointOverride)
					 .AppendAction(SendMessageActions.CreditAndDPSCheck, businessActionProvider?.CreditAndDPSCheck)
					 .AppendAction(SendMessageActions.ShowCreditAndDPSCheckOverride, guiActionProvider?.ShowCreditAndDPSCheckOverride)
					 .AppendAction(SendMessageActions.CreateMessages, businessActionProvider?.CreateMessages)
					 .AppendAction(SendMessageActions.ShowPreviewDialog, guiActionProvider?.ShowPreviewDialog)
					 .AppendAction(SendMessageActions.SignMessages, businessActionProvider?.SignMessages)
					 .AppendAction(SendMessageActions.ProcessUpdates, businessActionProvider?.ProcessUpdates);

			sendChain.FindAction(SendMessageActions.ShowPreviewDialog)?.AppendAction(SendMessageActions.PreviewDialogFailure, businessActionProvider?.PreviewDialogFailure, ActionLink.Failure);
			sendChain.FindAction(SendMessageActions.SignMessages)?.AppendAction(SendMessageActions.SignMessagesFailure, businessActionProvider?.SignMessagesFailure, ActionLink.Failure);

			businessActionProvider?.ConfigureProcess(sendChain);
			guiActionProvider?.ConfigureProcess(sendChain);

			return sendChain;
		}

		ActionChain CreateResultProcessChain()
		{
			var resultChain = new ActionChain(SendMessageActions.ShowResultNotifications, guiActionProvider?.ShowResultNotifications);
			resultChain.AppendAction(SendMessageActions.RefreshBusinessObjects, businessActionProvider?.RefreshBusinessObjects, ActionLink.Common);

			return resultChain;
		}

		public static class SendMessageActions
		{
			public const string ShowFormPreSaveDialog = nameof(ISendMessagesGuiActionProvider.ShowFormPreSaveDialog);
			public const string SendMessagesSecurityCheckpoint = nameof(ISendMessagesBusinessActionProvider.SendMessagesSecurityCheckpoint);
			public const string PreSendValidation = nameof(ISendMessagesBusinessActionProvider.PreSendValidation);
			public const string ShowPreSendValidationNotifications = nameof(ISendMessagesGuiActionProvider.ShowPreSendValidationNotifications);
			public const string ShowSendDialog = nameof(ISendMessagesGuiActionProvider.ShowSendDialog);
			public const string SendMessagesWithErrorsSecurityCheckpoint = nameof(ISendMessagesBusinessActionProvider.SendMessagesWithErrorsSecurityCheckpoint);
			public const string ShowSendMessagesWithErrorsSecurityCheckpointOverride = nameof(ISendMessagesGuiActionProvider.ShowSendMessagesWithErrorsSecurityCheckpointOverride);
			public const string CreditAndDPSCheck = nameof(ISendMessagesBusinessActionProvider.CreditAndDPSCheck);
			public const string ShowCreditAndDPSCheckOverride = nameof(ISendMessagesGuiActionProvider.ShowCreditAndDPSCheckOverride);
			public const string CreateMessages = nameof(ISendMessagesBusinessActionProvider.CreateMessages);
			public const string ShowPreviewDialog = nameof(ISendMessagesGuiActionProvider.ShowPreviewDialog);
			public const string PreviewDialogFailure = nameof(ISendMessagesBusinessActionProvider.PreviewDialogFailure);
			public const string SignMessages = nameof(ISendMessagesBusinessActionProvider.SignMessages);
			public const string SignMessagesFailure = nameof(ISendMessagesBusinessActionProvider.SignMessagesFailure);
			public const string ProcessUpdates = nameof(ISendMessagesBusinessActionProvider.ProcessUpdates);
			public const string ShowResultNotifications = nameof(ISendMessagesGuiActionProvider.ShowResultNotifications);
			public const string RefreshBusinessObjects = nameof(ISendMessagesBusinessActionProvider.RefreshBusinessObjects);
		}
	}
}
