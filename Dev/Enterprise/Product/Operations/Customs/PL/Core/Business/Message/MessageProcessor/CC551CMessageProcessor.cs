using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

class CC551CMessageProcessor : ImpExpMessageProcessorBase<ICC551C>
{
	public CC551CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => AESMessageCodes.Descriptions.CC551;

	protected override string EmailSubject(BaseEDIMessage message, ICC551C messageDataProvider)
		=> InterpretationStrings.MessageTitles.CC551 + " - " + messageDataProvider.MRN;

	protected override Type MessageInterpreterType => typeof(CC551CMessageInterpreter);

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC551C messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;

		if (entryHeader.CH_EntryStatus == AESEntryStatusList.Codes.ControlledForExport
			|| entryHeader.CH_EntryStatus == AESEntryStatusList.Codes.MrnAllocated
			|| entryHeader.CH_EntryStatus == LogicalStatusList.Codes.Acknowledged
			|| entryHeader.CH_EntryStatus == LogicalStatusList.Codes.Sent)
		{
			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.Rejected;
		}

		return ProcessingResult.Succeed;
	}
}
