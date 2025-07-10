using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.PL.Business;

public class CC525CMessageProcessor : ImpExpMessageProcessorBase<ICC525C>
{
	public CC525CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => AESMessageCodes.Descriptions.CC525;

	protected override Type MessageInterpreterType => typeof(CC525CMessageInterpreter);

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC525C messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
		if (entryHeader.CH_EntryStatus == AESEntryStatusList.Codes.ReleasedForExit)
		{
			Logger.LogProcessingError(message, Res.GetString("7A3DF419-5651-4C3A-93EE-DB96B1C14C90",
				"The message with interchange {0} is discarded, because the present status is not MRN.",
				message.Interchange?.EI_InterchangeNum));
			return ProcessingResult.Discarded;
		}

		entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.ReleasedForExit;
		entryHeader.CH_EntryReleaseDate = messageDataProvider.ExportOperation.ReleaseDate;
		return ProcessingResult.Succeed;
	}
}
