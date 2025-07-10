using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.PL.Business;

class CC571CMessageProcessor : ImpExpMessageProcessorBase<ICC571C>
{
	public CC571CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => AESMessageCodes.Descriptions.CC571;

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC571C messageDataProvider)
	{
		return ProcessingResult.Succeed;
	}
}
