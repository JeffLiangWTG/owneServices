using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CC522CMessageProcessor(LoggingInformation logger) : ExitControlMessageProcessorBase<ICC522C>(logger)
{
	protected override string MessageFriendlyNameCore => ExitControlMessageCodes.Descriptions.CC522;

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC522C messageDataProvider)
	{
		return ProcessingResult.Succeed;
	}
}
