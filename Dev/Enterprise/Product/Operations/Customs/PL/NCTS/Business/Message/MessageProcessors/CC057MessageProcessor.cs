using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC057MessageProcessor : BaseNctsMessageProcessor<IIE057>
{
	public CC057MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE057;

	protected override string EmailSubject(BaseEDIMessage message, IIE057 messageDataProvider)
		=> $"IE057_Functional_Error_({messageDataProvider.MRN})";

	protected override Type MessageInterpreterType => typeof(CC057CMessageInterpreter);

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE057 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		message.EM_ApplicationReference = messageDataProvider.MRN;
	}

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE057 messageDataProvider)
	{
		var arrivalMovementHeader = ((NctsHeader)message.EM_LinkedObject).ArrivalMovementHeader;
		arrivalMovementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;

		return ProcessingResult.Succeed;
	}

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendNctsErrors;
}
