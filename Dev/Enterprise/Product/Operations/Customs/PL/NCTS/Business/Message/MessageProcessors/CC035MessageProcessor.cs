using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.PL.NCTS;

public class CC035MessageProcessor : BaseNctsMessageProcessor<IIE035>
{
	public CC035MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE035;

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE035 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		message.EM_ApplicationReference = messageDataProvider.MRN;
	}

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE035 dataProvider)
	{
		var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

		movementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;
		movementHeader.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.UnderRecoveryProcedure;

		return ProcessingResult.Succeed;
	}

	protected override Type MessageInterpreterType => typeof(CC035CMessageInterpreter);

	protected override string EmailSubject(BaseEDIMessage message, IIE035 messageDataProvider) => $"{MessageNameList.Codes.IE035}_Recovery_Procedure_({messageDataProvider.MRN})";

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;
}
