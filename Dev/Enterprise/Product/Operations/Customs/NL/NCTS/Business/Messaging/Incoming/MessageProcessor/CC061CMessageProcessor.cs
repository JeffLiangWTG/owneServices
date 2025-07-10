using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC061C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC061CMessageProcessor : NCTSResponseMessageProcessor<ICC061CDataProvider>
{
	public CC061CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override BusinessObject FindParentOfMessage(EDIMessage message) => FindParentOfMessageByMRN(message, NctsMovementType.Codes.Arrival);

	protected override ZBool IsMessageOkForProcessing(EDIMessage message)
		=> message.EM_LinkedObject is NctsHeader nctsHeader && nctsHeader.ArrivalMovementHeader is NctsArrivalMovementHeader movementHeader
		&& movementHeader.BM_CustomsStatus.In<ZString>(ZString.Empty, NLNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, NLNCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks);

	protected override string GetLogMessageForFailingToLinkMessageToParentJob(EDIMessage message) => GetLogMessageForFailingToLinkMessageToParentJob_MRN(message);

	protected override ZString NoteMessageWhenDiscarded => Res.GetString("A84150B9-2B60-41E9-93B7-9AD79320DD57", "The message with interchange is discarded because its 'Status at Customs' is not blanks, UAP or ULR.");

	protected override ICC061CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc061CType, CC061CDataProvider>();

	protected override IMessageInterpreter<ICC061CDataProvider> Interpreter => new CC061CMessageInterpreter();

	protected override ZString GetNewCustomsStatus(NctsCommonMovementHeader movementHeader) => NLNCTS5ArrivalCustomsStatusList.Codes.DecisionToControl;

	protected override bool SetNewPhase => false;

	protected override void ProcessMessageCore(NLEDIMessage message)
	{
		base.ProcessMessageCore(message);
		message.EM_Status = EDIMessage.Status.ProcessedOK;
		var nctsHeader = (NctsHeader)message.EM_LinkedObject;
		var movementHeader = nctsHeader.ArrivalMovementHeader;
		var dataProvider = GetMessageDataProvider(message);

		var service = nctsHeader.Services.AddNew();
		service.ES_ServiceCode = NLConstants.ServiceTypes.ControlByCustoms;
		service.ES_Booked = dataProvider.ControlNotificationDateAndTime;
		service.ES_ServiceNote = Res.GetString("385BB9AD-5D20-4A4F-99A7-140A0C8F9A88", "Decision for a physical control");
		service.ES_References = dataProvider.MRN;

		nctsHeader.Logs.AddNew(AutoEvents.CustomsImpedimentReceived, movementHeader.BM_CustomsStatus, ((ZDateTime)dataProvider.ControlNotificationDateAndTime).ToOffset());
	}
}
