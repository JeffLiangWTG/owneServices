using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC022C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC022CMessageProcessor : NCTSResponseMessageProcessor<ICC022CDataProvider>
{
	public CC022CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString GetNewCustomsStatus(NctsCommonMovementHeader movementHeader) => NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;

	protected override BusinessObject FindParentOfMessage(EDIMessage message) => FindParentOfMessageByMRN(message, NctsMovementType.Codes.Departure);

	protected override string GetLogMessageForFailingToLinkMessageToParentJob(EDIMessage message) => GetLogMessageForFailingToLinkMessageToParentJob_MRN(message);

	protected override ZBool IsMessageOkForProcessing(EDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		return moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.Acknowledged || moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.PreLodged || moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.MrnAllocated || moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;
	}

	protected override ZString LogMessageWhenDiscarded => Res.GetString("DB41702D-AA2A-4F35-B6E1-54810B1B4A49", "The message is discarded because its 'Status at Customs' is not ACK, PRE, MRN or GIV.");

	protected override ZString NoteMessageWhenDiscarded => Res.GetString("52CE700C-40D7-4B7D-A989-31DCC2782EA8", "The message with interchange was discarded, because the Status at Customs of the declaration is not ACK, PRE, MRN or GIV.");

	protected override IMessageInterpreter<ICC022CDataProvider> Interpreter => new CC022CMessageInterpreter();

	protected override ICC022CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc022CType, CC022CDataProvider>();
}
