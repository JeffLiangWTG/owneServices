using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC140MessageProcessor : BaseNctsMessageProcessor<IIE140>
{
	public CC140MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override Type MessageInterpreterType => typeof(CC140CMessageInterpreter);

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE140;

	protected override string EmailSubject(BaseEDIMessage message, IIE140 messageDataProvider) => $"IE140_Search_Procedure_({messageDataProvider.MRN})";

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE140 messageDataProvider)
	{
		var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

		movementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;
		movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry;
		var customsOffice = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry, messageDataProvider.CustomsOfficeOfEnquiryAtDeparture);
		customsOffice.CY_Date = messageDataProvider.TransitOperation.LimitForResponseDate;

		var eventDate = new ZDate(messageDataProvider.TransitOperation.RequestOnNonArrivedMovementDate, DateTimeKind.Utc);
		var eventTime = eventDate.ToZDateTimeOffsetWithCurrentTimeIfDateIsToday();
		var logReference = $"{movementHeader.BM_CustomsStatus}-Limit Date To Response:{messageDataProvider.TransitOperation.LimitForResponseDate:yyyy-MM-dd}";
		movementHeader.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.CustomsImpedimentReceived, EstimateActual.Actual, eventTime: eventTime, logReference: logReference);

		return ProcessingResult.Succeed;
	}
}
