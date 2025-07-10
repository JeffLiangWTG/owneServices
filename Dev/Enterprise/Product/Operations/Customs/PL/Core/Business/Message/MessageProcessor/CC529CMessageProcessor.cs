using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;
using static Enterprise.Customs.PL.Business.Constants;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;
using EntryHeaderStatus = Enterprise.Customs.Common.Shared.MessageStatusList.Codes;
using PLEntryStatus = Enterprise.Customs.PL.Business.Declaration.PLEntryStatusList.Codes;

namespace Enterprise.Customs.PL.Business;

class CC529CMessageProcessor : ImpExpMessageProcessorBase<ICC529C>
{
	public CC529CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => AESMessageCodes.Descriptions.CC529;

	protected override string EmailSubject(BaseEDIMessage message, ICC529C messageDataProvider)
		=> InterpretationStrings.MessageTitles.CC529 + " - "
			+ messageDataProvider.MRN.IfNullOrEmpty(() => messageDataProvider.LRN);

	protected override Type MessageInterpreterType => typeof(CC529CMessageInterpreter);

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC529C messageDataProvider)
	{
		var cusEntryHeader = (CusEntryHeader)message.EM_LinkedObject;
		if (cusEntryHeader.CH_EntryStatus == PLEntryStatus.MRN)
		{
			cusEntryHeader.CH_EntryStatus = PLEntryStatus.ReleasedForExport;
			cusEntryHeader.CH_Status = EntryHeaderStatus.AcknowledgedChange;
			cusEntryHeader.CH_EntryReleaseDate = messageDataProvider.ExportOperation.ReleaseDate;
		}
		else
		{
			cusEntryHeader.MovementReferenceNumberSetter(messageDataProvider.MRN, messageDataProvider.ExportOperation.DeclarationAcceptanceDate);
		}

		return ProcessingResult.Succeed;
	}
}
