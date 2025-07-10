using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

class CC509CMessageProcessor : ImpExpMessageProcessorBase<ICC509C>
{
	public CC509CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => AESMessageCodes.Descriptions.CC509;

	protected override string EmailSubject(BaseEDIMessage message, ICC509C messageDataProvider) =>
		InterpretationStrings.MessageTitles.CC509
		+ " - "
		+ messageDataProvider.MRN.IfNullOrEmpty(() => messageDataProvider.LRN);

	protected override Type MessageInterpreterType => typeof(CC509CMessageInterpreter);

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC509C messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
		entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.Cancelled;

		return ProcessingResult.Succeed;
	}
}

