using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC060MessageProcessor : BaseNctsMessageProcessor<IIE060>
{
	public CC060MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE060;

	protected override Type MessageInterpreterType => typeof(CC060CMessageInterpreter);

	protected override string EmailSubject(BaseEDIMessage message, IIE060 messageDataProvider) => $"IE060_Control_Decision_({messageDataProvider?.LRN})";

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE060 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		var mrn = messageDataProvider.MRN;
		message.EM_ApplicationReference = string.IsNullOrEmpty(mrn)
			? messageDataProvider.LRN
			: mrn;
	}

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE060 messageDataProvider)
	{
		var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		var header = movementHeader.Header;

		movementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;
		movementHeader.BM_CustomsStatus = GetNotificationCode(messageDataProvider.TransitOperation.NotificationType, movementHeader.BM_CustomsStatus);
		AddService(header, messageDataProvider);

		return ProcessingResult.Succeed;
	}

	static ZString GetNotificationCode(string notificationType, ZString defaultValue)
	{
		return notificationType switch
		{
			NCTS5NotificationTypes.Codes.DecisionToControlAndRequestedDocumentsIfNeeded => NCTS5DepartureCustomsStatusList.Codes.DecisionToControl,
			NCTS5NotificationTypes.Codes.AdditionalDocumentsRequest => NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest,
			NCTS5NotificationTypes.Codes.IntentionToControl => NCTS5DepartureCustomsStatusList.Codes.IntentionToControl,
			_ => defaultValue
		};
	}

	static void AddService(NctsHeader header, IIE060 dataProvider)
	{
		var service = header.Services.AddNew();
		service.ES_ServiceCode = Constants.ServiceTypes.CTL;
		service.ES_ServiceNote = dataProvider.TransitOperation.NotificationType
			+ " : " + new NCTS5NotificationTypes().GetDescriptionFromCode(dataProvider.TransitOperation.NotificationType ?? string.Empty);
		service.ES_Booked = new ZDateTime(dataProvider.TransitOperation.ControlNotificationDateAndTime);
		service.ES_References = dataProvider.MRN;
		service.ES_SubLocation = MessageInterpreterHelper.GetOfficeCodeWithDescription(header.Factory, dataProvider.CustomsOfficeOfDeparture);
	}
}
