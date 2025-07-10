using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC051C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC051CMessageProcessor : NCTSResponseMessageProcessor<ICC051CDataProvider>
{
	public CC051CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString NewMessageStatus => LogicalStatusList.Codes.Accepted;

	protected override ZString GetNewCustomsStatus(NctsCommonMovementHeader movementHeader) => NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit;

	protected override ZString NewPhase => NCTS5DeparturePhaseList.Codes.Declaration;

	protected override bool SetNewCustomsStatus => true;

	protected override bool SetNewPhase => true;

	protected override ZBool IsMessageOkForProcessing(EDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		var customsStatus = moveHeader.BM_CustomsStatus;
		return customsStatus == NCTS5DepartureCustomsStatusList.Codes.Acknowledged
			|| customsStatus == NCTS5DepartureCustomsStatusList.Codes.PreLodged
			|| customsStatus == NCTS5DepartureCustomsStatusList.Codes.MrnAllocated
			|| customsStatus == NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid
			|| customsStatus == NCTS5DepartureCustomsStatusList.Codes.DecisionToControl
			|| customsStatus == NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest
			|| customsStatus == NCTS5DepartureCustomsStatusList.Codes.IntentionToControl
			|| customsStatus == NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
	}

	protected override ZString LogMessageWhenDiscarded => Res.GetString("E63FB64F-13B8-4E41-995F-4CC4E3DB467D", "The message is discarded because its 'Departure Status' has already the status ACK, PRE, MRN, GIV, CO1, CO2, CO3 or AMR.");

	protected override ZString NoteMessageWhenDiscarded => Res.GetString("83C087F2-3A76-4ED5-9EF6-8903D70816BE", "The message with interchange was discarded, because the Departure Status of the declaration is not ACK, PRE, MRN, GIV, CO1, CO2, CO3 or AMR.");

	protected override ICC051CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc051CType, CC051CDataProvider>();

	protected override IMessageInterpreter<ICC051CDataProvider> Interpreter => new CC051CMessageInterpreter();

	protected override void UpdateGuaranteeTransactionsIfNeeded(NLEDIMessage message, ICC051CDataProvider messageDataProvider)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		var nctsHeader = moveHeader.Header;

		foreach (var nctsGuarantee in nctsHeader.MovementHeader.Guarantees)
		{
			if (nctsGuarantee.CusGuarantee != null)
			{
				PermitHelper.UpdatePendingTransactionsWithAdditionalCriteria(message.Factory, Core.Constants.CountryCodes.Netherlands, moveHeader.BM_PaperlessInbondNum, (SharedCusPermitLineTransaction x) => x.CPL_CPH_PermitHeader == nctsGuarantee.CusGuarantee.PK, PermitTransactionStatusList.Codes.Deleted);
			}
		}
	}
}
