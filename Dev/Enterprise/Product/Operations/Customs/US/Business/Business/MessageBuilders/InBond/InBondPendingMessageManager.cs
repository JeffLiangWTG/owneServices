// -----------------------------------------------------------------------
// <copyright file="InBondPendingMessageManager.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	using Enterprise.Customs.US.Messaging.Business;

	public class InBondPendingMessageManager
	{
		public const string InBondPendingMessagesCancelledBody = "\r\n\r\nPlease Note: There were pending messages waiting for this message to be acknowledged before being transmitted.\r\nAs this message has been rejected, those pending messages have been canceled.";
		public const string InBondPendingMessagesQueuedBody = "\r\n\r\nPlease Note: There were pending messages waiting for this message to be acknowledged. Those messages have just been transmitted.";

		public string Process(IMessageAttachee inbondHeader, ABIResponseStatus status)
		{
			string result = "";

			if (status == ABIResponseStatus.Rejected)
			{
				var pendingMessages = inbondHeader.Messages.UpdateStatusOfPendingMessagesTo(EDIMessage.Status.Cancelled);
				if (pendingMessages.Length > 0)
				{
					result = InBondPendingMessagesCancelledBody;
				}
			}
			else if (status == ABIResponseStatus.Cleared)
			{
				var pendingMessages = inbondHeader.Messages.UpdateStatusOfPendingMessagesTo(EDIMessage.Status.Queued);

				if (pendingMessages.Length > 0)//status needs to change to Awaiting ...
				{
					new InBondMessageStatusCalculator(inbondHeader).CalculateStatus((MQEDIMessage)pendingMessages[0], ABIResponseStatus.Undefined);
					result = InBondPendingMessagesQueuedBody;
				}
			}

			return result;
		}
	}
}
