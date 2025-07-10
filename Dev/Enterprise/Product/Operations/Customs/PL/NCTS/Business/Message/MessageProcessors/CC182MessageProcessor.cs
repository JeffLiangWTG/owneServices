using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC182MessageProcessor(LoggingInformation logger) : BaseNctsMessageProcessor<IIE182>(logger)
{
	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE182;

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE182 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);
		message.EM_ApplicationReference = messageDataProvider.MRN;
	}

	protected override string EmailSubject(BaseEDIMessage message, IIE182 messageDataProvider) => $"{MessageNameList.Codes.IE182}_Incident_Reported_During_Transit_({messageDataProvider.MRN})";

	protected override Type MessageInterpreterType => typeof(CC182CMessageInterpreter);

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE182 messageDataProvider)
	{
		var header = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		header.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered;
		header.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;

		return ProcessingResult.Succeed;
	}
}
