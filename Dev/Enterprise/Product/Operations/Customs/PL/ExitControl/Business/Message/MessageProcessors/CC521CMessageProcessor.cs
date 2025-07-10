using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.ExitControl.Business.ExitControlConstants;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CC521CMessageProcessor(LoggingInformation logger) : ExitControlMessageProcessorBase<ICC521C>(logger)
{
	protected override string MessageFriendlyNameCore => ExitControlMessageCodes.Descriptions.CC521;

	protected override Type MessageInterpreterType => typeof(CC521CMessageInterpreter);

	protected override string EmailSubject(BaseEDIMessage message, ICC521C messageDataProvider) => $"{InterpretationStrings.MessageTitles.CC521C} - {messageDataProvider.MRN}";

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC521C messageDataProvider)
	{
		var exitReport = (CusExitReport)message.EM_LinkedObject;

		exitReport.CER_Status = AESEntryStatusList.Codes.DiversionRequestRejected;
		exitReport.CER_MessageStatus = ExitReportMessageStatuses.Received;

		return ProcessingResult.Succeed;
	}
}
