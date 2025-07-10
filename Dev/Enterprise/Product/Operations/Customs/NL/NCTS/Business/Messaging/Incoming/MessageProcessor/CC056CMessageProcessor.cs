using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC056C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.Messaging.Business;
using CustomsStatusCodes = Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC056CMessageProcessor : NCTSResponseMessageProcessor<ICC056CDataProvider>
{
	public CC056CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString GetNewCustomsStatus(NctsCommonMovementHeader movementHeader) => ZString.Empty;

	protected override ZString NewPhase => ZString.Empty;

	protected override ZString NewMessageStatus => LogicalStatusList.Codes.Invalid;

	protected override bool SetNewCustomsStatus => false;

	protected override bool SetNewPhase => false;

	protected override ZBool IsMessageOkForProcessing(EDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		var nctsHeader = moveHeader.Header;
		var dataProvider = GetMessageDataProvider(message);
		var rejectionType = dataProvider.BusinessRejectionType;
		var rejectionCode = dataProvider.RejectionCode;

		return IsDeclarationStatusCorrect(rejectionType, rejectionCode, nctsHeader.EffectiveMessageStatus, moveHeader.BM_CustomsStatus, moveHeader.BM_Phase);
	}

	protected override ZString LogMessageWhenDiscarded => Res.GetString("8DE544F1-3F4C-40FF-930A-5CE7AC5A1A72", "The message with interchange was discarded, because the ‘Phase status’ and the ‘Departure Status’ of the declaration could not be mapped to correct value of the element {0}.", "businessRejectionType");

	protected override ZString NoteMessageWhenDiscarded => Res.GetString("3FDA98EF-F922-43E2-8973-4FA07714C5D8", "The message with interchange was discarded, because the ‘Phase status’ and the ‘Departure Status’ of the declaration could not be mapped to correct value of the element {0}.", "businessRejectionType");

	protected override IMessageInterpreter<ICC056CDataProvider> Interpreter => new CC056CMessageInterpreter();

	protected override ICC056CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc056CType, CC056CDataProvider>();

	protected override string GetLogMessageForFailingToLinkMessageToParentJob(EDIMessage message) => GetLogMessageForFailingToLinkMessageToParentJob_LRNFallbackOnMRN(message);

	protected override BusinessObject FindParentOfMessage(EDIMessage message) => FindParentOfMessageByLRNFallbackOnMRN(message);

	protected override void ProcessMessageCore(NLEDIMessage message)
	{
		var dataProvider = GetMessageDataProvider(message);
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		var nctsHeader = moveHeader?.Header;

		if (message.EM_Status == EDIMessage.Status.ProcessedOK && nctsHeader != null)
		{
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Invalid;
		}

		if (IsRejectionCode4ReceivedAfter928Message(dataProvider.BusinessRejectionType, dataProvider.RejectionCode, moveHeader.BM_CustomsStatus, moveHeader.BM_Phase))
		{
			moveHeader.BM_CustomsStatus = CustomsStatusCodes.Cancelled;
		}
	}

	protected override void UpdateGuaranteeTransactionsIfNeeded(EDIMessage message, ICC056CDataProvider messageDataProvider)
	{
		var rejectionType = messageDataProvider.BusinessRejectionType;
		if (rejectionType == NCTS5DeparturePhaseList.Codes.Declaration)
		{
			Customs.Business.PermitHelper.UpdatePendingTransactionsWithAdditionalCriteria(message.Factory, Core.Constants.CountryCodes.Netherlands, ((NctsDepartureMovementHeader)message.EM_LinkedObject).BM_PaperlessInbondNum, null, Customs.Business.PermitTransactionStatusList.Codes.Deleted);
		}
	}

	bool IsDeclarationStatusCorrect(ZString rejectionType, ZString rejectionCode, ZString messageStatus, ZString customsStatus, ZString phase)
	{
		ZBool messageStatusIsSentOrAcknowledge = messageStatus == LogicalStatusList.Codes.Sent || messageStatus == LogicalStatusList.Codes.Acknowledged;
		ZBool messageStatusIsAccepted = messageStatus == LogicalStatusList.Codes.Accepted;
		return (messageStatusIsSentOrAcknowledge && isPhaseAndCustomsStatusCorrect(rejectionType, customsStatus, phase))
			|| (messageStatusIsAccepted && IsRejectionCode4ReceivedAfter928Message(rejectionType, rejectionCode, customsStatus, phase));
	}

	static ZBool isPhaseAndCustomsStatusCorrect(ZString rejectionType, ZString customsStatus, ZString phase) => (string)rejectionType switch
	{
		NLNctsOutgoingMessageTypes.Codes.CC013C => (ZBool)(phase == NctsMovementHeaderTransactionStatusList.Codes.Amendment && customsStatus.In(new ZString[] { CustomsStatusCodes.Acknowledged, CustomsStatusCodes.PreLodged, CustomsStatusCodes.MrnAllocated, CustomsStatusCodes.AmendmentRequested })),
		NLNctsOutgoingMessageTypes.Codes.CC014C => (ZBool)(phase == NctsMovementHeaderTransactionStatusList.Codes.Cancellation && customsStatus.In(new ZString[] { CustomsStatusCodes.Acknowledged, CustomsStatusCodes.PreLodged, CustomsStatusCodes.MrnAllocated, CustomsStatusCodes.ReleasedForTransit })),
		NLNctsOutgoingMessageTypes.Codes.CC015C => (ZBool)(phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration && string.IsNullOrEmpty(customsStatus)),
		NLNctsOutgoingMessageTypes.Codes.CC141C => (ZBool)(customsStatus == CustomsStatusCodes.UnderEnquiry),
		NLNctsOutgoingMessageTypes.Codes.CC170C => (ZBool)customsStatus.In(new ZString[] { CustomsStatusCodes.Acknowledged, CustomsStatusCodes.PreLodged }),
		_ => (ZBool)false,
	};

	bool IsRejectionCode4ReceivedAfter928Message(ZString rejectionType, ZString rejectionCode, ZString customsStatus, ZString phase)
	{
		return rejectionCode == RejectionCodes.Codes.Code4
			&& rejectionType == NLNctsOutgoingMessageTypes.Codes.CC015C
			&& customsStatus == CustomsStatusCodes.PreLodged
			&& phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration;
	}
}
