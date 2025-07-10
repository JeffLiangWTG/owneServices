using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.PL.Business;

class CC604CMessageProcessor : ImpExpMessageProcessorBase<ICC604C>
{
	public CC604CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => AESMessageCodes.Descriptions.CC604;

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC604C messageDataProvider)
	{
		return ProcessingResult.Succeed;
	}
}
