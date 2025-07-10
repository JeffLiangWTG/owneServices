using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS;

public class CC022MessageProcessor : BaseNctsMessageProcessor<IIE022>
{
	public CC022MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE022;

	protected override Type MessageInterpreterType => typeof(CC022CMessageInterpreter);

	protected override string EmailSubject(BaseEDIMessage message, IIE022 messageDataProvider) => $"IE022_Amendment_Requested_({messageDataProvider.MRN})";

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE022 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		message.EM_ApplicationReference = messageDataProvider.MRN;
	}

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE022 dataProvider)
	{
		var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

		movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
		movementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;

		return ProcessingResult.Succeed;
	}
}
