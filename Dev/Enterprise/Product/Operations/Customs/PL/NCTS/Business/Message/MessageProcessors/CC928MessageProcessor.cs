using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.BatchProcessor;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC928MessageProcessor : BaseNctsMessageProcessor<IIE928>
{
	public CC928MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override Type MessageInterpreterType => typeof(CC928CMessageInterpreter);

	protected override string EmailSubject(BaseEDIMessage message, IIE928 messageDataProvider) => $"IE928_Positive_Acknowledgment_({messageDataProvider.LRN})";

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE928;

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE928 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		message.EM_ApplicationReference = messageDataProvider.LRN;
	}

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE928 messageDataProvider)
	{
		var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		movementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;
		return ProcessingResult.Succeed;
	}
}
