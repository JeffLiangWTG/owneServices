using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.PL.Business;

class CC504CMessageProcessor : ImpExpMessageProcessorBase<ICC504C>
{
	public CC504CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => AESMessageCodes.Descriptions.CC504;

	protected override string EmailSubject(BaseEDIMessage message, ICC504C messageDataProvider) => Constants.InterpretationStrings.MessageTitles.CC504 + " - " + messageDataProvider.MRN;

	protected override Type MessageInterpreterType => typeof(CC504CMessageInterpreter);

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC504C messageDataProvider)
	{
		var transmitMessage = GetTransmitMessage(message, messageDataProvider);
		if (transmitMessage is null)
		{
			return ProcessingResult.Fail;
		}

		transmitMessage.EM_Status = LogicalStatusList.Codes.Accepted;
		return ProcessingResult.Succeed;
	}
}
