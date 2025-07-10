using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC055C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC055CMessageProcessor : NCTSResponseMessageProcessor<ICC055CDataProvider>
{
	public CC055CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString GetNewCustomsStatus(NctsCommonMovementHeader movementHeader) => NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;

	protected override ZBool IsMessageOkForProcessing(EDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		switch (moveHeader.BM_CustomsStatus)
		{
			case NCTS5DepartureCustomsStatusList.Codes.MrnAllocated:
			case NCTS5DepartureCustomsStatusList.Codes.DecisionToControl:
			case NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest:
			case NCTS5DepartureCustomsStatusList.Codes.IntentionToControl:
				return true;
			default:
				InterchangeNumber = message.EM_InterchangeNumber;
				CurrentCustomsStatus = moveHeader.BM_CustomsStatus;
				return false;
		}
	}

	protected override ZString LogMessageWhenDiscarded => $"The message is discarded because its 'Departure Status' has the status {CurrentCustomsStatus}.";

	protected override ZString NoteMessageWhenDiscarded => $"The message with interchange {InterchangeNumber} was discarded because its 'Departure Status' has the status {CurrentCustomsStatus}.";

	protected override ICC055CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc055CType, CC055CDataProvider>();

	ZString InterchangeNumber { get; set; }

	ZString CurrentCustomsStatus { get; set; }
	protected override IMessageInterpreter<ICC055CDataProvider> Interpreter => new CC055CMessageInterpreter();

	protected override void UpdateGuaranteeTransactionsIfNeeded(NLEDIMessage message, ICC055CDataProvider messageDataProvider)
	{
		if (message.EM_LinkedObject is not NctsDepartureMovementHeader moveHeader)
		{
			return;
		}

		var nctsHeader = moveHeader.Header;
		foreach (var guaranteeReference in messageDataProvider.GuaranteeReferences)
		{
			if (!guaranteeReference.InvalidGuaranteeReasons.Any(x => x.Code.In(NCTS5InvalidGuaranteeReason.Codes.G01, NCTS5InvalidGuaranteeReason.Codes.G02, NCTS5InvalidGuaranteeReason.Codes.G05, NCTS5InvalidGuaranteeReason.Codes.G08, NCTS5InvalidGuaranteeReason.Codes.G09, NCTS5InvalidGuaranteeReason.Codes.G10, NCTS5InvalidGuaranteeReason.Codes.G12)))
			{
				continue;
			}

			foreach (var nctsGuarantee in nctsHeader.MovementHeader.Guarantees.Where(x => x.PW_BondNumber == guaranteeReference.GRN))
			{
				PermitHelper.UpdatePendingTransactionsWithAdditionalCriteria(message.Factory, Core.Constants.CountryCodes.Netherlands, moveHeader.BM_PaperlessInbondNum, (SharedCusPermitLineTransaction x) => x.CPL_CPH_PermitHeader == nctsGuarantee.CusGuarantee.PK, PermitTransactionStatusList.Codes.Deleted);
			}
		}
	}
}
