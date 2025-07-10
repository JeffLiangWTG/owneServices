using System;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public abstract class NLMessageSender
{
	protected NLMessageSender(JobDeclarationMessageSendingObject messageSendingObject)
	{
		this.messageSendingObject = CargoWise.Common.Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		this.entryHeader = messageSendingObject.Header;
	}
	protected readonly JobDeclarationMessageSendingObject messageSendingObject;
	protected readonly CusEntryHeader entryHeader;

	public void Send()
	{
		var message = (NLEDIMessage)entryHeader.Messages.AddNew(GetEdiMessageType());
		message.EM_MessageType = NLEDIMessageTypes.Codes.DMS;
		message.EM_MessageSubType = MessageSubType;
		message.EM_IsTestMessage = NLCustomsRegistry.Instance.IsNLTestingSystem.Value;
		message.EM_ApplicationReference = entryHeader.EntryNumber;

		SetMessageTextAndInterpretation(message);

		ReplacePlaceHolders(message, message.EM_MessageText);

		PostSendProcess(message);
	}

	protected virtual void SetMessageTextAndInterpretation(NLEDIMessage message)
	{
		var xmlMessageSerializedString = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(messageSendingObject)
						 ?.GenerateXmlMessage()
						 .GetSerializedString() ??
					 string.Empty;

		SetMessageText(message, xmlMessageSerializedString);

		SetMessageInterpretation(message);
	}

	protected virtual void PostSendProcess(NLEDIMessage message)
	{
		entryHeader.CH_Status = NLConstants.StatusNew.SentToCustoms;
	}

	protected void SetMessageText(NLEDIMessage message, ZString xmlMessageSerializedString)
	{
		if (!xmlMessageSerializedString.IsEmpty)
		{
			message.EM_MessageText = XmlMessageHelper.RemoveEmptyXmlElements(xmlMessageSerializedString);
		}
	}

	protected abstract ZString MessageSubType { get; }

	protected abstract ZString WcoType { get; }

	protected virtual Type GetEdiMessageType() => typeof(NLEDIMessage);

	void SetMessageInterpretation(NLEDIMessage message)
	{
		var result = message.GetOutgoingMessageInterpretation();
		if (result.IsEmpty)
		{
			result = Res.GetString("B882F390-5C8A-4F30-84BA-C53092F6ED16", "The generated message has no content.");
		}
		message.EM_MessageInterpretation = result;
	}

	void ReplacePlaceHolders(NLEDIMessage message, ZString messageText)
	{
		message.EM_MessageText = message.EM_MessageText.Replace(NLEDIMessage.WCOTypePlaceHolderHtml, WcoType);
	}
}
