using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.PL.Business;

class CC628CMessageProcessor : ImpExpMessageProcessorBase<ICC628C>
{
	public CC628CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => AESMessageCodes.Descriptions.CC628;

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC628C messageDataProvider)
	{
		return ProcessingResult.Succeed;
	}
}
