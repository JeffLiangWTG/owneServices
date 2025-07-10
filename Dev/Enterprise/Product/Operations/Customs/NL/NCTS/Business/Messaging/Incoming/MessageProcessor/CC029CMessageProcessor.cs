using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC029C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC029CMessageProcessor : NCTSResponseMessageProcessor<ICC029CDataProvider>
{
	public CC029CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString GetNewCustomsStatus(NctsCommonMovementHeader movementHeader) => NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

	protected override BusinessObject FindParentOfMessage(EDIMessage message) => FindParentOfMessageByLRNFallbackOnMRN(message, NctsMovementType.Codes.Departure);

	protected override string GetLogMessageForFailingToLinkMessageToParentJob(EDIMessage message) => GetLogMessageForFailingToLinkMessageToParentJob_LRNFallbackOnMRN(message);

	protected override ZBool IsMessageOkForProcessing(EDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		return !(moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.Acknowledged || moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.PreLodged || moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested);
	}

	protected override ZString LogMessageWhenDiscarded => Res.GetString("F9464015-8563-436D-96B9-7878C6EB6985", "The message is discarded because its 'Status at Customs' is PRE, ACK, or AMR.");

	protected override ZString NoteMessageWhenDiscarded => Res.GetString("8C21149F-AE82-47C6-A7AE-4BD5712B74E9", "The message with interchange is discarded, because the Status at Customs is PRE, ACK, or AMR.");

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
		if (messageDataProvider.ReleaseDate != null)
		{
			movementReferenceNumber.CE_IssueDate = (ZDateTime)messageDataProvider.ReleaseDate;
		}
		moveHeader.BM_EntryDate = messageDataProvider.DeclarationAcceptanceDate;
	}

	protected override IMessageInterpreter<ICC029CDataProvider> Interpreter => new CC029CMessageInterpreter();

	protected override ICC029CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc029CType, CC029CDataProvider>();

	protected override void UpdateGuaranteeTransactionsIfNeeded(NLEDIMessage message, ICC029CDataProvider messageDataProvider)
	{
		if (message.EM_LinkedObject is NctsDepartureMovementHeader departureMovmentHeader)
		{
			Customs.Business.PermitHelper.UpdatePendingTransactionsWithAdditionalCriteria(message.Factory, Core.Constants.CountryCodes.Netherlands, departureMovmentHeader.BM_PaperlessInbondNum, null, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
		}
	}
}
