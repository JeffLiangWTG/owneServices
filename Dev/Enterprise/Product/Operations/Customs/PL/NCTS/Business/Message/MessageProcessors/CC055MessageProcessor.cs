using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC055MessageProcessor : BaseNctsMessageProcessor<IIE055>
{
	public CC055MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string EmailSubject(BaseEDIMessage message, IIE055 messageDataProvider)
	{
		var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		var lrn = movementHeader.Header.MovementHeader.BM_PaperlessInbondNum;

		return string.IsNullOrEmpty(lrn)
			? "IE055_GUARANTEE_INVALID"
			: $"IE055_GUARANTEE_INVALID_({lrn})";
	}

	protected override Type MessageInterpreterType => typeof(CC055CMessageInterpreter);

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE055;

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE055 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		message.EM_ApplicationReference = messageDataProvider.MRN;
	}

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE055 dataProvider)
	{
		var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

		movementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;
		movementHeader.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;

		return ProcessingResult.Succeed;
	}

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;
}
