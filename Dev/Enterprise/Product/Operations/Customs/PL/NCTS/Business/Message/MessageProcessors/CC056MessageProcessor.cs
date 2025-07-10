using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC056MessageProcessor : BaseNctsMessageProcessor<IIE056>
{
	public CC056MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string EmailSubject(BaseEDIMessage message, IIE056 messageDataProvider)
	{
		var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		var lrn = movementHeader.Header.MovementHeader.BM_PaperlessInbondNum;

		return $"IE056_Functional_Error_({lrn})";
	}

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE056;

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE056 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		var mrn = messageDataProvider.MRN;
		message.EM_ApplicationReference = string.IsNullOrEmpty(mrn) ? messageDataProvider.LRN : mrn;
	}

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE056 dataProvider)
	{
		var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

		movementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Invalid;

		return ProcessingResult.Succeed;
	}

	protected override Type MessageInterpreterType => typeof(CC056CMessageInterpreter);

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendNctsErrors;
}
