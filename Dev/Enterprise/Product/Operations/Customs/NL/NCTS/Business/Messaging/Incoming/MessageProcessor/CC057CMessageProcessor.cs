using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC057C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC057CMessageProcessor : NCTSResponseMessageProcessor<ICC057CDataProvider>
{
	public CC057CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override bool SetNewCustomsStatus => false;

	protected override bool SetNewPhase => false;

	protected override ZString NewMessageStatus => LogicalStatusList.Codes.Invalid;

	protected override BusinessObject FindParentOfMessage(EDIMessage message) => FindParentOfMessageByMRN(message, NctsMovementType.Codes.Arrival);

	protected override ZBool IsMessageOkForProcessing(EDIMessage message)
	{
		var nctsHeader = GetNctsHeaderFromLinkedObject(message);
		var moveHeader = nctsHeader.ArrivalMovementHeader;
		var dataProvider = GetMessageDataProvider(message);
		return ((dataProvider.BusinessRejectionType == NLNctsOutgoingMessageTypes.Codes.CC007C && moveHeader.BM_Phase == NctsMovementHeaderTransactionStatusList.Codes.Arrival && moveHeader.BM_CustomsStatus == NctsTransitStatusList.Codes.Unknown)
			|| (dataProvider.BusinessRejectionType == NLNctsOutgoingMessageTypes.Codes.CC044C && moveHeader.BM_Phase == NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks && moveHeader.BM_CustomsStatus.ToString().In(NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks)))
			&& moveHeader.BM_MessageStatus.ToString().In(LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent);
	}

	protected override ZString LogMessageWhenDiscarded => Res.GetString("1F2BD999-9D82-4DCD-A359-1CAF9EC59998", "The message with interchange ... was discarded, because the ‘Phase status’ and the ‘Arrival Status’ of the declaration could not be mapped to correct value of the element {0}.", "businessRejectionType");

	protected override ZString NoteMessageWhenDiscarded => Res.GetString("C6F79FC1-2FD3-4CAE-863F-AB796679FA45", "The message with interchange ... was discarded, because the ‘Phase status’ and the ‘Arrival Status’ of the declaration could not be mapped to correct value of the element {0}.", "businessRejectionType");

	protected override IMessageInterpreter<ICC057CDataProvider> Interpreter => new CC057CMessageInterpreter();

	protected override ICC057CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc057CType, CC057CDataProvider>();

	protected override string GetLogMessageForFailingToLinkMessageToParentJob(EDIMessage message) => Res.GetString("D0D3F813-97D6-41FF-B03F-1CEC6D2C91F9", "The processing of the message with interchange ... failed, because the message could not be linked to a NCTS declaration.");
}
