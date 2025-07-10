using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.PL.Business;

class CC548CMessageProcessor : ImpExpMessageProcessorBase<ICC548C>
{
	public CC548CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => AESMessageCodes.Descriptions.CC548;

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC548C messageDataProvider)
	{
		return ProcessingResult.Succeed;
	}
}
