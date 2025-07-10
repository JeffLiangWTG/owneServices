using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC051MessageProcessor : BaseNctsMessageProcessor<IIE051>
{
	public CC051MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE051;

	protected override Type MessageInterpreterType => typeof(CC051CMessageInterpreter);

	protected override string EmailSubject(BaseEDIMessage message, IIE051 messageDataProvider) => $"IE051_Not_Released_For_Transit_({messageDataProvider.LRN})";

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE051 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		var mrn = messageDataProvider.MRN;
		message.EM_ApplicationReference = string.IsNullOrEmpty(mrn) ? messageDataProvider.LRN : mrn;
	}

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE051 messageDataProvider)
	{
		var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

		movementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;
		movementHeader.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit;

		return ProcessingResult.Succeed;
	}

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;
}
