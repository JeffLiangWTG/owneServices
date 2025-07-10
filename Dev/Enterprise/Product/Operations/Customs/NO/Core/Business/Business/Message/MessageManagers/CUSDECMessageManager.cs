using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.NO.Business;

class CUSDECMessageManager : EDIFACTMessageManager
{
	public CUSDECMessageManager(IEDIFACTMessageAttachee dataWrapper, EDIFACTMessageStatusCalculator statusCalculator, IUserNotification notification)
		: base(dataWrapper, statusCalculator, notification)
	{
	}

	public CUSDECMessageManager(MessageSendingObject source, IMessageNotificationCollector notification)
		: base(source.Header, source.Header?.MessageStatusCalculator, notification)
	{
		messageSendingObject = source;
	}

	readonly MessageSendingObject messageSendingObject;

	public CusEntryHeader Header => messageSendingObject.Header;

	public override string MessageFriendlyName => "CUSDEC";

	protected override bool ShouldSendMessagesInTestMode => ShouldSendMessagesInTestModeImpl;
	public bool ShouldSendMessagesInTestModeImpl;

	protected override IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		=> new CUSDECMessageBuilder(new CUSDECMessageDataProviderWrapper(messageSendingObject), actionCode);

	public MessageSubTypes ActionCode => TranslateToMessageSubType(messageSendingObject.MessageType);

	MessageSubTypes TranslateToMessageSubType(ZString messageType)
	{
		return MessageSubTypes.Create;
	}

	internal bool SendMessage()
	{
		var result = false;
		result = SendMessage(ActionCode);
		return result;
	}
}
