using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.BatchProcessor;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC043MessageProcessor : BaseNctsMessageProcessor<IIE043>
{
	public CC043MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE043;

	protected override string EmailSubject(BaseEDIMessage message, IIE043 messageDataProvider) => $"IE043_Unloading_Permission_({messageDataProvider.MRN})";

	protected override Type MessageInterpreterType => typeof(CC043CMessageInterpreter);

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE043 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		message.EM_ApplicationReference = messageDataProvider.MRN;
	}

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE043 messageDataProvider)
	{
		var arrivalMovementHeader = ((NctsHeader)message.EM_LinkedObject).ArrivalMovementHeader;
		arrivalMovementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;
		arrivalMovementHeader.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		arrivalMovementHeader.BM_InBondEntryType = messageDataProvider.TransitOperation.DeclarationType;

		return ProcessingResult.Succeed;
	}
}
