using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

class CC531CMessageProcessor : ImpExpMessageProcessorBase<ICC531C>
{
	public CC531CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => AESMessageCodes.Descriptions.CC531;

	protected override string EmailSubject(BaseEDIMessage message, ICC531C messageDataProvider)
		=> InterpretationStrings.MessageTitles.CC531 + " - " + messageDataProvider.MRN;

	protected override Type MessageInterpreterType => typeof(CC531CMessageInterpreter);

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC531C messageDataProvider)
	{
		return ProcessingResult.Succeed;
	}
}
