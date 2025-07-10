using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.PL.Business;

public class CC599CMessageProcessor : ImpExpMessageProcessorBase<ICC599C>
{
	public CC599CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => AESMessageCodes.Descriptions.CC599;

	protected override Type MessageInterpreterType => typeof(CC599CMessageInterpreter);

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC599C messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
		if (entryHeader.CH_EntryStatus == AESEntryStatusList.Codes.Cancelled)
		{
			Logger.LogProcessingError(message, Res.GetString("1AB622FB-DC6E-4050-8E55-F34A89E4B809",
				"The message with interchange {0} is discarded, because the present status is Canceled.",
				message.Interchange?.EI_InterchangeNum));
			return ProcessingResult.Discarded;
		}

		var exitControlResult = messageDataProvider.ExitControlResult;
		entryHeader.CH_EntryStatus = exitControlResult?.ExitStoppedDate is null ? AESEntryStatusList.Codes.Exported : AESEntryStatusList.Codes.ExitReleaseRejected;

		return ProcessingResult.Succeed;
	}
}
