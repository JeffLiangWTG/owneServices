using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.PL.NCTS;

public class CC045MessageProcessor : BaseNctsMessageProcessor<IIE045>
{
	public CC045MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE045;

	protected override Type MessageInterpreterType => typeof(CC045CMessageInterpreter);

	protected override string EmailSubject(BaseEDIMessage message, IIE045 messageDataProvider) => $"IE045_Write_Off_({messageDataProvider.MRN})";

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE045 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		message.EM_ApplicationReference = messageDataProvider?.MRN;
	}

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE045 dataProvider)
	{
		var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

		movementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;
		movementHeader.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;

		var eventDate = new ZDate(dataProvider.WriteOffDate, DateTimeKind.Utc);
		var now = ZDateTime.UtcNow;
		var eventTime = eventDate == now.Date
			? now.ToOffset()
			: eventDate.ToZDateTime().ToOffset();
		movementHeader.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.CustomsCleared, EstimateActual.Actual, eventTime: eventTime, logReference: movementHeader.BM_CustomsStatus);

		return ProcessingResult.Succeed;
	}

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;
}
