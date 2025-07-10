using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.PL.Business;

class CC609CMessageProcessor : ImpExpMessageProcessorBase<ICC609C>
{
	public CC609CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => AESMessageCodes.Descriptions.CC609;

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC609C messageDataProvider)
	{
		return ProcessingResult.Succeed;
	}
}

