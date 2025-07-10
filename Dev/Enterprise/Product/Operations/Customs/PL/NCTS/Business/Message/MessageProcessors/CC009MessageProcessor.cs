using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC009MessageProcessor : BaseNctsMessageProcessor<IIE009>
{
	public CC009MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE009;

	protected override string EmailSubject(BaseEDIMessage message, IIE009 messageDataProvider)
		=> $"IE009_Invalidation_Response_({GetReferenceNumber(messageDataProvider)})";

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE009 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		message.EM_ApplicationReference = GetReferenceNumber(messageDataProvider);
	}

	protected override Type MessageInterpreterType => typeof(CC009CMessageInterpreter);

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE009 messageDataProvider)
	{
		var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

		switch (messageDataProvider.Invalidation?.Decision)
		{
			case Decision.Item1:
				movementHeader.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.Cancelled;
				movementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;
				break;
			case Decision.Item0:
				movementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Invalid;
				break;
			default:
				return ProcessingResult.Fail;
		}

		return ProcessingResult.Succeed;
	}

	string GetReferenceNumber(IIE009 messageDataProvider)
	{
		var lrn = messageDataProvider.LRN;
		return string.IsNullOrEmpty(lrn) ? messageDataProvider.MRN : lrn;
	}

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;
}
