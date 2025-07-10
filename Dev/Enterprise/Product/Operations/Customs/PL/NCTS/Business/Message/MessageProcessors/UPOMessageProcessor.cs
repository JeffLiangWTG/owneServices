using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.Business.Constants;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.NCTS.Business;

public class UPOMessageProcessor(LoggingInformation logger) : BaseNctsMessageProcessor<IUpo>(logger)
{
	protected override string MessageFriendlyNameCore => $"{ApplicationCodes.PLCustomsNCTS}/{PUESC.SystemMessages.UPO}";

	protected override string EmailSubject(BaseEDIMessage message, IUpo dataProvider)
		=> $"{InterpretationStrings.MessageTitles.UPO}{UPOInterpreterBase<NctsCommonMovementHeader>.GetTransmitDocumentTypeForTitle(dataProvider)} {dataProvider.CorrelationIdentifier}.";

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IUpo upoDataProvider)
	{
		var movementHeader = message.GetRelatedCommonMovementHeader();
		if (movementHeader.BM_MessageStatus == LogicalStatusList.Codes.Sent)
		{
			movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Accepted;
		}
		return ProcessingResult.Succeed;
	}

	protected override BusinessObject GetLinkedObject(BaseEDIMessage message, IUpo upoMessage) => message.EM_LinkedObject;

	protected override Type MessageInterpreterType => typeof(UPOMessageInterpreter);
}
