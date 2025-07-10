using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.PL.Business;

class CC556CMessageProcessor : ImpExpMessageProcessorBase<ICC556C>
{
	public CC556CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => AESMessageCodes.Descriptions.CC556;

	protected override bool IsFailureNotification => true;

	protected override string EmailSubject(BaseEDIMessage message, ICC556C messageDataProvider)
		=> Constants.InterpretationStrings.MessageTitles.CC556 + " - "
		+ messageDataProvider.MRN.IfNullOrEmpty(() => messageDataProvider.CorrelationIdentifier);

	protected override Type MessageInterpreterType => typeof(CC556CMessageInterpreter);

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC556C messageDataProvider)
	{
		if (GetTransmitMessage(message, messageDataProvider) is not { } correlatedMessage)
		{
			return ProcessingResult.Fail;
		}

		correlatedMessage.EM_Status = EDIMessageStatusList.Codes.Rejected;
		if (correlatedMessage.EM_MessageSubType == AESMessageCodes.Descriptions.CC515)
		{
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.Rejected;
		}

		return ProcessingResult.Succeed;
	}

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendExportMessageErrors;
}
