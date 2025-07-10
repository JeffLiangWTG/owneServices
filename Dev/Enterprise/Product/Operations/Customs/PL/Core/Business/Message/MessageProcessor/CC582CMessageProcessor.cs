using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using static Enterprise.Customs.PL.Business.Constants;
using PLEntryStatus = Enterprise.Customs.PL.Business.Declaration.PLEntryStatusList.Codes;

namespace Enterprise.Customs.PL.Business;

class CC582CMessageProcessor : ImpExpMessageProcessorBase<ICC582C>
{
	public CC582CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override Type MessageInterpreterType => typeof(CC582CMessageInterpreter);

	protected override string EmailSubject(BaseEDIMessage message, ICC582C messageDataProvider)
		=> InterpretationStrings.MessageTitles.CC582 + " - " + messageDataProvider.MRN;

	protected override string MessageFriendlyNameCore => AESMessageCodes.Descriptions.CC582;

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC582C messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;

		entryHeader.CH_EntryStatus = PLEntryStatus.REQ;
		entryHeader.CH_Status = EDIMessage.Status.Received;

		return ProcessingResult.Succeed;
	}
}
