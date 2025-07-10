using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CC561CMessageProcessor(LoggingInformation logger) : ExitControlMessageProcessorBase<ICC561C>(logger)
{
	protected override string MessageFriendlyNameCore => ExitControlMessageCodes.Descriptions.CC561;

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC561C messageDataProvider)
	{
		return ProcessingResult.Succeed;
	}
}
