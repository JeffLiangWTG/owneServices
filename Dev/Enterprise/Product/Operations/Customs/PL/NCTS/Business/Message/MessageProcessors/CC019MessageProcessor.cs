using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.PL.NCTS;

public class CC019MessageProcessor : BaseNctsMessageProcessor<IIE019>
{
	public CC019MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE019;

	protected override Type MessageInterpreterType => typeof(CC019CMessageInterpreter);

	protected override string EmailSubject(BaseEDIMessage message, IIE019 messageDataProvider) => $"{MessageNameList.Codes.IE019}_Major_Discrepancies_({messageDataProvider.MRN})";

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE019 dataProvider)
	{
		var arrivalMovementHeader = ((NctsHeader)message.EM_LinkedObject).ArrivalMovementHeader;
		arrivalMovementHeader.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5ArrivalCustomsStatusList.Codes.DiscrepanciesAtDestination;
		arrivalMovementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;
		return ProcessingResult.Succeed;
	}

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;
}
