using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NPPMessageProcessor(LoggingInformation logger) : BaseNctsMessageProcessor<IConfirmation>(logger)
{
	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.NPP;

	protected override bool IsFailureNotification => true;

	protected override string EmailSubject(BaseEDIMessage message, IConfirmation npp) => PL.Business.Constants.PUESC.SystemMessages.NPP;

	protected override Type MessageInterpreterType => typeof(NPPMessageInterpreter<NctsCommonMovementHeader>);

	protected override BusinessObject GetLinkedObject(BaseEDIMessage message, IConfirmation messageDataProvider) => message.EM_LinkedObject;

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IConfirmation messageDataProvider)
	{
		var nctsHeader = message.GetRelatedNctsHeader();
		nctsHeader.BH_MessageStatus = LogicalStatusList.Codes.Error;
		return ProcessingResult.Succeed;
	}
}
