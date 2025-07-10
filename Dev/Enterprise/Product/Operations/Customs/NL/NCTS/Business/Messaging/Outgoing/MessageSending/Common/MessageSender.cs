using System.Collections.Generic;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.NL.NCTS.Business;

public abstract class MessageSender<TProvider> : IMessageSender
	where TProvider : IMessageHeader
{
	protected MessageSender(MessageSendingAction messageSendingAction)
	{
		this.messageSendingAction = Argument.NotNull(messageSendingAction, nameof(messageSendingAction));
		MessageObject = Argument.NotNull(messageSendingAction.Header, nameof(messageSendingAction.Header));
		MessageSubType = messageSendingAction.MessageType;
		sendWithErrors = messageSendingAction.ShouldSend;
		IsTestMessage = messageSendingAction.IsTestDeclaration;

		if (messageSubTypeForEntryType.TryGetValue(messageSendingAction.MessageType, out var messageSubType))
		{
			MessageSubType = messageSubType;
		}
	}
	protected readonly MessageSendingAction messageSendingAction;
	readonly bool sendWithErrors;

	public string MessageSubType { get; }

	public BusinessObject MessageObject { get; set; }

	public bool IsTestMessage { get; }

	public void Send()
	{
		PreSend();

		var newMessage = MessageObject.Factory.New<NCTSMessage>();
		newMessage.EM_LinkedObject = MessageObject;
		newMessage.EM_MessageSubType = MessageSubType;
		var dataProvider = GetDataProvider(MessageObject);
		var initialStream = GetProducer(dataProvider).GenerateXmlMessage().GetSerializedStream();
		var messageContent = XmlMessageHelper.RemoveEmptyXmlElements(initialStream);
		newMessage.SetEM_MessageTextOrDataSource(messageContent);
		newMessage.EM_SendWithMessageErrors = sendWithErrors;
		newMessage.EM_IsTestMessage = IsTestMessage;

		PostSendProcess(newMessage);
	}

	protected abstract ZString NewPhase { get; }

	protected abstract IXmlMessageBuilder GetProducer(TProvider dataProvider);

	protected abstract TProvider GetDataProvider(BusinessObject messageObject);

	protected virtual ZString NewCustomsStatus => ZString.Empty;

	protected virtual void PostSendProcess(NLEDIMessage newMessage)
	{
		var nctsHeader = (NctsHeader)MessageObject;
		var movementHeader = nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader : (NctsCommonMovementHeader)nctsHeader.ArrivalMovementHeader;

		movementHeader.BM_CustomsStatus = NewCustomsStatus.IsEmpty ? movementHeader.BM_CustomsStatus : NewCustomsStatus;
		movementHeader.BM_Phase = NewPhase.IsEmpty ? movementHeader.BM_Phase : NewPhase;
		nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;

		newMessage.EM_MessageInterpretation = new NctsEdiMessagePrettier(newMessage).MakeOutboundPrettyForInterpretation(nctsHeader);

		if (movementHeader is NctsDepartureMovementHeader departureMovement)
		{
			departureMovement.Messages.Add(newMessage);
		}
		else
		{
			nctsHeader.Messages.Add(newMessage);
		}
	}

	readonly Dictionary<string, string> messageSubTypeForEntryType = new()
	{
		[NctsMessageTypeListNL.Codes.Declaration] = NLNctsOutgoingMessageTypes.Codes.CC015C,
		[NctsMessageTypeListNL.Codes.ArrivalNotification] = NLNctsOutgoingMessageTypes.Codes.CC007C,
		[NctsMessageTypeListNL.Codes.InvalidationCancellation] = NLNctsOutgoingMessageTypes.Codes.CC014C,
		[NctsMessageTypeListNL.Codes.Amendment] = NLNctsOutgoingMessageTypes.Codes.CC013C,
		[NctsMessageTypeListNL.Codes.UnloadingRemarks] = NLNctsOutgoingMessageTypes.Codes.CC044C,
		[NctsMessageTypeListNL.Codes.PresentationNotification] = NLNctsOutgoingMessageTypes.Codes.CC170C,
		[NctsMessageTypeListNL.Codes.ResponseOnRequestForNonArrivedMovement] = NLNctsOutgoingMessageTypes.Codes.CC141C
	};

	protected virtual void PreSend()
	{
	}
}
