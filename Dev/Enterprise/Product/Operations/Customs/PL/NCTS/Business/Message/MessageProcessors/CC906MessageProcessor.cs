using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.PL.NCTS;

public class CC906MessageProcessor : BaseNctsMessageProcessor<IIE906>
{
	public CC906MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE906;

	protected override Type MessageInterpreterType => typeof(CC906CMessageInterpreter);
	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE906 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		var lrn = messageDataProvider.LRN;
		message.EM_ApplicationReference = string.IsNullOrEmpty(lrn) ? messageDataProvider.MRN : lrn;
	}

	protected override string EmailSubject(BaseEDIMessage message, IIE906 messageDataProvider)
	=> $"{MessageNameList.Codes.IE906}_Functional_Error_{(string.IsNullOrEmpty(messageDataProvider.MRN) ? $"LRN({messageDataProvider.LRN})" : $"MRN({messageDataProvider.MRN})")}";

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE906 messageDataProvider)
	{
		var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

		movementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Error;
		return ProcessingResult.Succeed;
	}

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendNctsErrors;
}
