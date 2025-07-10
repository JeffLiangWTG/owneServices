using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC917MessageProcessor(LoggingInformation logger) : BaseNctsMessageProcessor<IIE917>(logger)
{
	protected override string EmailSubject(BaseEDIMessage message, IIE917 messageDataProvider) => $"{MessageNameList.Codes.IE917}_Syntax_Error";

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE917;

	protected override bool IsFailureNotification => true;

	protected override Type MessageInterpreterType => typeof(CC917CMessageInterpreter);

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE917 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		var lrn = messageDataProvider.LRN;
		message.EM_ApplicationReference = string.IsNullOrEmpty(lrn) ? messageDataProvider.MRN : lrn;
	}

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE917 messageDataProvider)
	{
		var nctsHeader = message.GetRelatedNctsHeader();
		var movementHeader = nctsHeader.MovementHeader;

		movementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Error;
		return ProcessingResult.Succeed;
	}

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendNctsErrors;
}
