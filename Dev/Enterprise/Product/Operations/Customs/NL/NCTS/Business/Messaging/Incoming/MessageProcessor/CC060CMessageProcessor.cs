using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC060C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC060CMessageProcessor : NCTSResponseMessageProcessor<ICC060CDataProvider>
{
	public CC060CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ICC060CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc060CType, CC060CDataProvider>();

	protected override BusinessObject FindParentOfMessage(EDIMessage message) => FindParentOfMessageByMRNFallbackOnLRN(message, NctsMovementType.Codes.Departure);

	protected override IMessageInterpreter<ICC060CDataProvider> Interpreter => new CC060CMessageInterpreter();

	protected override void ProcessMessageCore(NLEDIMessage message)
	{
		base.ProcessMessageCore(message);
		var movementHeader = message.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;
		var dataProvider = GetMessageDataProvider(message);

		switch (dataProvider.NotificationType)
		{
			case NCTS5NotificationTypes.Codes.DecisionToControlAndRequestedDocumentsIfNeeded:
				movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.DecisionToControl;
				break;
			case NCTS5NotificationTypes.Codes.AdditionalDocumentsRequest:
				movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest;
				break;
			case NCTS5NotificationTypes.Codes.IntentionToControl:
				movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.IntentionToControl;
				break;
			default:
				break;
		}

		var service = nctsHeader.Services.AddNew();
		service.ES_ServiceCode = NLConstants.ServiceTypes.ControlByCustoms;
		service.ES_Booked = dataProvider.ControlNotificationDateAndTime;
		service.ES_References = dataProvider.MRN;
		switch (dataProvider.NotificationType)
		{
			case NCTS5NotificationTypes.Codes.DecisionToControlAndRequestedDocumentsIfNeeded:
				service.ES_ServiceNote = Res.GetString("1E335F2C-E981-41C8-AB46-6F3712D75C53", "Intention to control. Type of Controls: ");
				break;
			case NCTS5NotificationTypes.Codes.AdditionalDocumentsRequest:
				service.ES_ServiceNote = Res.GetString("098C2833-7768-4268-AE65-DEAD034992A0", "Decision to control. Type of Controls: ");
				break;
			case NCTS5NotificationTypes.Codes.IntentionToControl:
				service.ES_ServiceNote = Res.GetString("2B73FB1B-B11D-40A3-831D-2611128CBFF8", "Additional document request. Type of Controls: ");
				break;
			default:
				break;
		}

		foreach (var typeOfControl in dataProvider.TypeOfControls)
		{
			var type = typeOfControl.Type;
			service.ES_ServiceNote = $"{service.ES_ServiceNote} {typeOfControl.SequenceNumeric}. {type} {new NCTS5TypeOfControlTypes().GetDescriptionFromCode(type)} {typeOfControl.Text};";
		}

		nctsHeader.Logs.AddNew(AutoEvents.CustomsImpedimentReceived, movementHeader.BM_CustomsStatus, ((ZDateTime)dataProvider.ControlNotificationDateAndTime).ToOffset());
	}

	protected override ZBool IsMessageOkForProcessing(EDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		var customsStatus = moveHeader.BM_CustomsStatus;

		return customsStatus == NCTS5DepartureCustomsStatusList.Codes.Acknowledged || customsStatus == NCTS5DepartureCustomsStatusList.Codes.PreLodged || customsStatus == NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
	}

	protected override ZString LogMessageWhenDiscarded => Res.GetString("B0C80743-5BF5-46EE-8D1B-20B194FD68AB", "The message is discarded because the Departure Status of the declaration is not ACK, PRE or MRN.");

	protected override ZString NoteMessageWhenDiscarded => Res.GetString("CDF5C2FE-077D-42E1-A864-877BB0505D5F", "The message with interchange was discarded, because the Departure Status of the declaration is not ACK, PRE or MRN.");

	protected override string GetLogMessageForFailingToLinkMessageToParentJob(EDIMessage message) => GetLogMessageForFailingToLinkMessageToParentJob_MRNFallbackOnLRN(message);

	protected override ZString NewMessageStatus => LogicalStatusList.Codes.Accepted;

	protected override ZString NewPhase => NCTS5DeparturePhaseList.Codes.Declaration;

	protected override bool SetNewPhase => true;
}
