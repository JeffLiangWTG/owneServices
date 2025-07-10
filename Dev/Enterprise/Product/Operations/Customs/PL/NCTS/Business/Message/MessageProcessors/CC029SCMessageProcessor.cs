using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC029SCMessageProcessor(LoggingInformation logger) : BaseNctsMessageProcessor<IIE029SC>(logger)
{
	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE029SC;

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE029SC messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		var mrn = messageDataProvider.MRN;
		message.EM_ApplicationReference = string.IsNullOrEmpty(mrn) ? messageDataProvider.LRN : mrn;
	}

	protected override Type MessageInterpreterType => typeof(CC029SCCMessageInterpreter);

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE029SC messageDataProvider)
	{
		var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

		movementHeader.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
		movementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;

		var eventDate = new ZDate(messageDataProvider.ReleaseDate, DateTimeKind.Utc);
		var eventTime = eventDate.ToZDateTimeOffsetWithCurrentTimeIfDateIsToday();
		var logReference = $"{movementHeader.BM_CustomsStatus}";
		movementHeader.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.CustomsEntryStatus, EstimateActual.Actual, eventTime: eventTime, logReference: logReference);

		return ProcessingResult.Succeed;
	}

	protected override string EmailSubject(BaseEDIMessage message, IIE029SC messageDataProvider) => $"{MessageNameList.Codes.IE029SC}_Released_For_Transit_({messageDataProvider.MRN})";

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;
}
