using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC029MessageProcessor : BaseNctsMessageProcessor<IIE029>
{
	public CC029MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE029;

	protected override string EmailSubject(BaseEDIMessage message, IIE029 messageDataProvider)
		=> $"{MessageNameList.Codes.IE029} Released For Transit ({messageDataProvider.MRN})";

	protected override Type MessageInterpreterType => typeof(CC029CMessageInterpreter);

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE029 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		message.EM_ApplicationReference = messageDataProvider.LRN.IfNullOrEmpty(() => messageDataProvider.MRN);
	}

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE029 messageDataProvider)
	{
		var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		var header = movementHeader.Header;

		movementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;
		movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
		movementHeader.BM_EntryDate = messageDataProvider.TransitOperation.ReleaseDate;

		return ProcessingResult.Succeed;
	}

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;
}
