using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NO.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NO.NCTS.Business;

abstract class NctsMessageInformationProvider : IMessageInformationProvider
{
	protected NctsMessageInformationProvider(NctsHeaderMessageSendingObject messageSendingObject)
	{
		MessageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
	}

	protected NctsHeaderMessageSendingObject MessageSendingObject { get; }

	ZString IMessageInformationProvider.MessageType => MessageSendingObject.MessageType;

	ZString IMessageInformationProvider.MessageSubType => ZString.Empty;

	ZString IMessageInformationProvider.MessageText => messageText.IsEmpty ? GetMessageText() : messageText;
	readonly ZString messageText;

	BusinessObject IMessageInformationProvider.Parent => GetParentCore();

	ZString IMessageInformationProvider.ApplicationReference => ZString.Empty;

	IMessageNumberStrategy IMessageInformationProvider.MessageNumberStrategy => messageNumberStrategy ??= new FixedMessageNumberStrategy();
	IMessageNumberStrategy messageNumberStrategy;

	ZString IMessageInformationProvider.ApplicationCode => Enterprise.Messaging.Integration.ApplicationCodeList.Codes.NOCustomsNcts;

	protected abstract IXmlMessageBuilder CreateMessageBuilder();

	protected abstract BusinessObject GetParentCore();

	protected NctsHeader Header => header ??= MessageSendingObject.NctsHeader;
	NctsHeader header;

	ZString GetMessageText()
	{
		var messageBuilder = CreateMessageBuilder();
		if (messageBuilder == null)
		{
			return ZString.Empty;
		}

		var message = messageBuilder.GenerateXmlMessage();
		return message.GetSerializedString();
	}
}
