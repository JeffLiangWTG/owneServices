using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.PL.Business;

class CC574CMessageProcessor : ImpExpMessageProcessorBase<ICC574C>
{
	public CC574CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => AESMessageCodes.Descriptions.CC574;

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC574C messageDataProvider)
	{
		return ProcessingResult.Succeed;
	}
}
