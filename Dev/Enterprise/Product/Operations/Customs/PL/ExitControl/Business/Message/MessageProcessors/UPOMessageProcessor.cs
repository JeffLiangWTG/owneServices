using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.Business.Constants;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class UPOMessageProcessor(LoggingInformation logger) : ExitControlMessageProcessorBase<IUpo>(logger)
{
	protected override string MessageFriendlyNameCore => $"{ApplicationCodes.PLCustomsExitControl}/{PUESC.SystemMessages.UPO}";

	// TODO
	// protected override string EmailSubject(BaseEDIMessage message, IUpo dataProvider)
	//	=> $"{InterpretationStrings.MessageTitles.UPO}{UPOInterpreterBase<CusExitReport>.GetTransmitDocumentTypeForTitle(dataProvider)} {dataProvider.CorrelationIdentifier}.";

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IUpo upoDataProvider)
	{
		if (message.EM_LinkedObject is CusExitReport cusExitReport &&
			cusExitReport.CER_MessageStatus == LogicalStatusList.Codes.Sent)
		{
			cusExitReport.CER_MessageStatus = LogicalStatusList.Codes.Accepted;
		}
		return ProcessingResult.Succeed;
	}

	// TODO
	//protected override Type MessageInterpreterType => typeof(UPOMessageInterpreter);
}
