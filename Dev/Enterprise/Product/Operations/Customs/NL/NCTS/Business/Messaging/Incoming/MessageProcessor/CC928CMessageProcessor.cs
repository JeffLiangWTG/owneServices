using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC928C;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC928CMessageProcessor : NCTSResponseMessageProcessor<ICC928CDataProvider>
{
	public CC928CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString GetNewCustomsStatus(NctsCommonMovementHeader movementHeader) => (string)movementHeader.BM_AdditionalDeclarationType switch
	{
		NctsTypeOfAdditionalDeclarationList.Codes.A => (ZString)NCTS5DepartureCustomsStatusList.Codes.Acknowledged,
		NctsTypeOfAdditionalDeclarationList.Codes.D => (ZString)NCTS5DepartureCustomsStatusList.Codes.PreLodged,
		_ => ZString.Empty,
	};

	protected override BusinessObject FindParentOfMessage(EDIMessage message) => FindParentOfMessageByLRN(message);

	protected override string GetLogMessageForFailingToLinkMessageToParentJob(EDIMessage message) => GetLogMessageForFailingToLinkMessageToParentJob_LRN(message);

	protected override ZBool IsMessageOkForProcessing(EDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		return moveHeader.BM_CustomsStatus.IsEmpty && moveHeader.BM_Phase.In(new ZString[] { ZString.Empty, NctsMovementHeaderTransactionStatusList.Codes.Declaration });
	}

	protected override ZString LogMessageWhenDiscarded => Res.GetString("D1C25829-72AA-4327-B1BC-3D25128B841D", "Failed to process the message with interchange because the message could not be linked to a NCTS Departure Declaration with LRN in the application.");

	protected override ZString NoteMessageWhenDiscarded => Res.GetString("EF4A6C78-A681-49FB-9585-E30F5D862432", "The message with interchange is discarded, because the 'Phase Status' of the declaration has not the value 015 or blanks and the 'Status at Customs' has not the value blanks.");

	protected override void ProcessMessageCore(NLEDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		var messageDataProvider = GetMessageDataProvider(message);
		var cidEntryNum = CusEntryNumber.LoadOrCreate(moveHeader, CusEntryNumberTypes.EU.CorrelationIdentifier, moveHeader.CountryCode);
		cidEntryNum.CE_EntryNum = messageDataProvider.CorrelationIdentifier;
		if (!messageDataProvider.MRN.IsEmpty())
		{
			var nctsHeader = moveHeader.Header;
			var movementReferenceNumber = nctsHeader.MovementReferenceEntryNumber;
			movementReferenceNumber.CE_EntryNum = messageDataProvider.MRN;
		}
	}

	protected override ICC928CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc928CType, CC928CDataProvider>();

	protected override IMessageInterpreter<ICC928CDataProvider> Interpreter => new CC928CMessageInterpreter();
}
