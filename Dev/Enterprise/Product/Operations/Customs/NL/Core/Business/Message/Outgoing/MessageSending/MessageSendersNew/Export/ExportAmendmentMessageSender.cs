using CargoWise.Customs.NL.MessageContracts.DMSAmendment.Export;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business;

public class ExportAmendmentMessageSender : NLMessageSender
{
	public ExportAmendmentMessageSender(JobDeclarationMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override void SetMessageTextAndInterpretation(NLEDIMessage message)
	{
		var messageWrapper = new ExportAmendmentMessageWrapper(messageSendingObject);
		var messageBuilder = new ExportAmendmentMessageBuilder(messageWrapper);
		if (messageBuilder is IXmlMessageBuilder messageBuilderWithAmendmentChanges)
		{
			var xmlMessageSerializedString = messageBuilderWithAmendmentChanges.GenerateXmlMessage()?.GetSerializedString() ?? string.Empty;

			SetMessageText(message, xmlMessageSerializedString);
		}
	}

	protected override void PostSendProcess(NLEDIMessage newMessage)
	{
		base.PostSendProcess(newMessage);

		entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;
	}

	protected override ZString MessageSubType => ExportSendMessageTypes.Codes.AMD;

	protected override ZString WcoType => NLConstants.WCoTypeCodes.ExportAmendment;
}
