namespace Enterprise.Customs.US.DIS.Business
{
	public class MessageSendingActionValidation : AutoMessageSendingActionValidation
	{
		public MessageSendingActionValidation(AutoMessageSendingAction action)
			: base(action)
		{
		}

		new MessageSendingAction Parent
		{
			get { return (MessageSendingAction)base.Parent; }
		}

		protected override void CheckSend()
		{
			base.CheckSend();

			if (Parent.Send)
			{
				if (Parent.IsWaitingForResponse)
				{
					Parent.SendInfo.AddMessageError(PendingResponses);
				}

				var messageSendingWarning = Parent.MessageSendingWarning;
				if (!messageSendingWarning.IsEmpty)
				{
					Parent.SendInfo.AddWarning(messageSendingWarning);
				}

				var messageSendingError = Parent.MessageSendingError;
				if (!messageSendingError.IsEmpty)
				{
					Parent.SendInfo.AddMessageError(messageSendingError);
				}
			}
		}

		protected override void CheckSendWithdrawal()
		{
			base.CheckSendWithdrawal();

			if (Parent.SendWithdrawal && Parent.IsWaitingForResponse)
			{
				Parent.SendWithdrawalInfo.AddMessageError(PendingResponses);
			}
		}

		internal const string PendingResponses = "This document has been submitted to Customs and a response from Customs is outstanding.";
	}
}
