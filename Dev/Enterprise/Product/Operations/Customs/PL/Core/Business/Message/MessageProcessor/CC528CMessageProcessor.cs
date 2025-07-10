using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

class CC528CMessageProcessor : ImpExpMessageProcessorBase<ICC528C>
{
	public CC528CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => AESMessageCodes.Descriptions.CC528;

	protected override string EmailSubject(BaseEDIMessage message, ICC528C messageDataProvider)
		=> InterpretationStrings.MessageTitles.CC528 + " - " + messageDataProvider.MRN;

	protected override Type MessageInterpreterType => typeof(CC528CMessageInterpreter);

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC528C messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
		var mrn = (ZString)messageDataProvider.MRN;
		if (mrn.IsEmpty || !entryHeader.MovementReferenceNumber.IsEmpty)
		{
			return ProcessingResult.Fail;
		}

		entryHeader.MovementReferenceNumberSetter(mrn, messageDataProvider.DeclarationAcceptanceDate);
		entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;

		return ProcessingResult.Succeed;
	}
}
