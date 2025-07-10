using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC004C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC004CMessageProcessor : NCTSResponseMessageProcessor<ICC004CDataProvider>
{
	public CC004CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override BusinessObject FindParentOfMessage(EDIMessage message) => FindParentOfMessageByLRNFallbackOnMRN(message, NctsMovementType.Codes.Departure);

	protected override string GetLogMessageForFailingToLinkMessageToParentJob(EDIMessage message) => GetLogMessageForFailingToLinkMessageToParentJob_LRNFallbackOnMRN(message);

	protected override ZBool IsMessageOkForProcessing(EDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		return moveHeader.BM_CustomsStatus == ZString.Empty
			|| moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.Acknowledged
			|| moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.PreLodged
			|| moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.MrnAllocated
			|| moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid
			|| moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
	}

	protected override ZString LogMessageWhenDiscarded => Res.GetString("13F7E5D8-6156-4420-9C59-A9B66BEF99F1", "The message is discarded because its 'Status at Customs' is not ACK, PRE, MRN, GIV or AMR.");

	protected override ZString NoteMessageWhenDiscarded => Res.GetString("AB515464-2AB0-43B4-9E94-287C7D74FA2D", "The message was discarded, because the 'Status at Customs' of the declaration is different from ACK, PRE, MRN and AMR and/or its 'Phase Status' is not 013. Message status is set to DCD.");

	protected override IMessageInterpreter<ICC004CDataProvider> Interpreter => new CC004CMessageInterpreter();

	protected override void ProcessMessageCore(NLEDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		if (moveHeader.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.A)
		{
			var messageDataProvider = GetMessageDataProvider(message);
			moveHeader.BM_EntryDate = messageDataProvider.AmendmentAcceptanceDateTime;
		}

		var nctsHeader = moveHeader.Header;
		moveHeader.BM_CustomsStatus = GetNewCustomsStatus(nctsHeader.Logs);
	}

	protected override ICC004CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc004CType, CC004CDataProvider>();

	static ZString GetNewCustomsStatus(Logs logs)
	{
		if (logs.HasLogWith(l => l.SL_Reference == NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit))
		{
			return NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
		}

		if (logs.HasLogWith(l => l.SL_Reference == NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid))
		{
			return NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;
		}

		if (logs.HasLogWith(l => l.SL_Reference == NCTS5DepartureCustomsStatusList.Codes.MrnAllocated))
		{
			return NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
		}

		if (logs.HasLogWith(l => l.SL_Reference == NCTS5DepartureCustomsStatusList.Codes.PreLodged))
		{
			return NCTS5DepartureCustomsStatusList.Codes.PreLodged;
		}

		return NCTS5DepartureCustomsStatusList.Codes.Acknowledged;
	}

	protected override void UpdateGuaranteeTransactionsIfNeeded(NLEDIMessage message, ICC004CDataProvider messageDataProvider)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		var nctsHeader = moveHeader.Header;

		if (moveHeader.Logs.HasLogWith(log => log.SL_SE_NKEvent.EqualsIgnoringCase(Events.CustomsEntryStatus.Code) && log.SL_Reference.EqualsIgnoringCase(NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit)))
		{
			PermitHelper.UpdatePendingTransactionsWithAdditionalCriteria(message.Factory, Core.Constants.CountryCodes.Netherlands, moveHeader.BM_PaperlessInbondNum, null, PermitTransactionStatusList.Codes.Confirmed);
		}
	}
}
