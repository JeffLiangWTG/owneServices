using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC028C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC028CMessageProcessor : NCTSResponseMessageProcessor<ICC028CDataProvider>
{
	public CC028CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString GetNewCustomsStatus(NctsCommonMovementHeader movementHeader) => NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;

	protected override BusinessObject FindParentOfMessage(EDIMessage message) => FindParentOfMessageByLRN(message);

	protected override string GetLogMessageForFailingToLinkMessageToParentJob(EDIMessage message) => GetLogMessageForFailingToLinkMessageToParentJob_LRN(message);

	protected override ZBool IsMessageOkForProcessing(EDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		return moveHeader.BM_CustomsStatus.IsEmpty || moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.Acknowledged || moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.PreLodged;
	}

	protected override ZString LogMessageWhenDiscarded => Res.GetString("1970BCF2-D6A2-4529-AACB-C243F5F2638A", "The message is discarded because its 'Status at Customs' is not blank, PRE or ACK.");

	protected override ZString NoteMessageWhenDiscarded => Res.GetString("84E86A0A-C5D5-419A-BD98-359C05E2AB03", "The message with interchange was discarded, because the Status at Customs of the declaration is not blank, PRE or ACK.");

	protected override IMessageInterpreter<ICC028CDataProvider> Interpreter => new CC028CMessageInterpreter();

	protected override void ProcessMessageCore(NLEDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		var nctsHeader = moveHeader.Header;
		var messageDataProvider = GetMessageDataProvider(message);

		if (moveHeader.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.D)
		{
			moveHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
		}

		var movementReferenceNumber = nctsHeader.MovementReferenceEntryNumber;
		movementReferenceNumber.CE_EntryNum = messageDataProvider.MRN;
		moveHeader.BM_EntryDate = messageDataProvider.DeclarationAcceptanceDate;
	}

	protected override ICC028CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc028CType, CC028CDataProvider>();
}
