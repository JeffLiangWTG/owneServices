using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	public abstract class ILBaseMessagingExtensions : BaseMessagingExtensions
	{
		public ILBaseMessagingExtensions(IILElectronicMessageProvider messageProvider)
		{
			this.messageProvider = messageProvider;
		}

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications)
		{
			return CommandEnablerHelper.IsSendCustomCommandEnabled(messageProvider, DocumentName);
		}

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications)
		{
			return CommandEnablerHelper.IsSendWithdrawalCustomCommandEnabled(messageProvider, DocumentName);
		}

		public override bool? ContinueWithResetToOriginal(IUserNotifications notifications)
		{
			return CommandEnablerHelper.IsResetToOriginalCustomCommandEnabled(messageProvider, DocumentName);
		}

		public override string GetMessageStatus()
			=> (GetLatestMeaningfulEvent()?.Event.SE_Code.ToString()) switch
			{
				Events.MessageSentCode => DataObjects.Res.GetString("A0D3E4F1-5C7B-4F2A-8D6C-9E0A8B5E1F2C", "{0} message has been sent", MessageName),
				Events.MessageAcceptedCode => DataObjects.Res.GetString("9E02CC85-30D8-43DC-8E15-E783C747B917", "{0} message has been accepted", MessageName),
				Events.MessageRejectedCode => DataObjects.Res.GetString("9C56DF09-6789-4B36-894D-2739D1927001", "{0} message has been rejected", MessageName),
				Events.MessageWithdrawCancelRequestCode => DataObjects.Res.GetString("5A8ED570-56CF-4059-A910-31B1B3374EAD", "{0} message withdraw/cancel has been sent", MessageName),
				Events.MessageWithdrawCancelAcceptedCode => DataObjects.Res.GetString("79B6F761-5FD6-408D-AD17-B01EDD4D2328", "{0} message withdrawal has been accepted", MessageName),
				_ => base.GetMessageStatus(),
			};

		protected abstract string DocumentName { get; }

		protected abstract string MessageName { get; }

		protected abstract ZString MessageReference { get; }

		protected readonly IILElectronicMessageProvider messageProvider;

		StmALog GetLatestMeaningfulEvent()
			=> new[]
			{
				GetMostRecentMessageEvent(Events.MessageSent),
				GetMostRecentMessageEvent(Events.MessageAccepted),
				GetMostRecentMessageEvent(Events.MessageRejected),
				GetMostRecentMessageEvent(Events.MessageWithdrawCancelRequest),
				GetMostRecentMessageEvent(Events.MessageWithdrawCancelAccepted),
			}
			.OrderByDescending(e => e?.EventTime).First();

		StmALog GetMostRecentMessageEvent(Event @event)
			=> messageProvider.BusinessObject.GetMostRecentMessageSentEvent(@event, DocumentName, string.Empty);
	}
}
