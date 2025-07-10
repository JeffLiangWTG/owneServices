using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Common;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business.MessagingProcess
{
	internal sealed class TRCustomsAutoSendMessenger : ITRCustomsMessenger, ICustomsMessenger
	{
		public TRCustomsAutoSendMessenger(IMessageSender sender, ICustomsMessageGenerator messageGenerator, Action<ActionResult> actionAfterSend = null)
		{
			this.messageGenerator = messageGenerator;
			this.actionAfterSend = actionAfterSend;

			owner = new SenderCollectionWrapper(sender);
		}
		readonly ICustomsMessageGenerator messageGenerator;
		readonly Action<ActionResult> actionAfterSend;
		readonly IEDIMessageCollectionOwner owner;

		ICustomsMessenger ITRCustomsMessenger.Messenger => this;
		bool ITRCustomsMessenger.IsMessageSigningRequired => false;

		IEDIMessageCollectionOwner ICustomsMessenger.Owner => owner;
		ICustomsMessageGenerator ICustomsMessenger.MessageGenerator => messageGenerator;
		bool ICustomsMessenger.ShouldCreateMessage(ActionResult previousResult) => true;
		bool ICustomsMessenger.ProcessUpdates(ActionResult previousResult)
		{
			var result = previousResult;

			if (result.Success && owner.MessageOwner is IMessageAttachee attachee)
			{
				attachee.MessageStatus = TRMessageStatusCodeList.Codes.Awaiting;
			}

			result.Notifications.AddInformation(result.Success ?
				Res.GetString("46C5D287-4E2D-470D-81F1-E422DE22EAB8", "Message sent successfully.") :
				Res.GetString("721C4C6B-7294-4C41-BBC1-A39C20845E35", "Message sending failed"));

			actionAfterSend?.Invoke(result);

			return result.Success;
		}

		class SenderCollectionWrapper : IEDIMessageCollectionOwner
		{
			public SenderCollectionWrapper(IMessageSender sender)
			{
				this.sender = sender;
			}
			readonly IMessageSender sender;

			public BusinessObject MessageOwner => sender.Parent;

			IBusinessObjectCollection IEDIMessageCollectionOwner.Messages => sender.Messages;
		}
	}
}
