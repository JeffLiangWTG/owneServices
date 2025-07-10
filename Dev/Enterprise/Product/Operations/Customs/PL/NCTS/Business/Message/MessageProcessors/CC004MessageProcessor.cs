using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.BatchProcessor;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC004MessageProcessor : BaseNctsMessageProcessor<IIE004>
{
	public CC004MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE004;

	protected override string EmailSubject(BaseEDIMessage message, IIE004 messageDataProvider) => $"{MessageNameList.Codes.IE004}_Amendment_Accepted_({messageDataProvider.LRN})";

	protected override Type MessageInterpreterType => typeof(CC004CMessageInterpreter);

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE004 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		var lrn = messageDataProvider.LRN;
		message.EM_ApplicationReference = string.IsNullOrEmpty(lrn) ? messageDataProvider.MRN : lrn;
	}

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE004 dataProvider)
	{
		var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		movementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;

		return ProcessingResult.Succeed;
	}
}
