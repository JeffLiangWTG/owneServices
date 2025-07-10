using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC009C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC009CMessageProcessor : NCTSResponseMessageProcessor<ICC009CDataProvider>
{
	public CC009CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override bool SetNewPhase => false;

	protected override bool SetNewMessageStatus => false;

	protected override IMessageInterpreter<ICC009CDataProvider> Interpreter => new CC009CMessageInterpreter();

	protected override BusinessObject FindParentOfMessage(EDIMessage message) => FindParentOfMessageByLRNFallbackOnMRN(message, NctsMovementType.Codes.Departure);

	protected override string GetLogMessageForFailingToLinkMessageToParentJob(EDIMessage message) => GetLogMessageForFailingToLinkMessageToParentJob_LRNFallbackOnMRN(message);

	protected override ICC009CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc009CType, CC009CDataProvider>();

	protected override void ProcessMessageCore(NLEDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		var dataProvider = GetMessageDataProvider(message);
		var originCustomsStatus = moveHeader.BM_CustomsStatus;

		if (dataProvider.Invalidation.Decision && originCustomsStatus != NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed)
		{
			moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.Cancelled;
			moveHeader.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomsEntryStatus, eventTime: ZDateTimeOffset.Now, reference: NCTS5DepartureCustomsStatusList.Codes.Cancelled), true);
		}

		moveHeader.Header.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
		moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
	}

	protected override void UpdateGuaranteeTransactionsIfNeeded(EDIMessage message, ICC009CDataProvider messageDataProvider)
	{
		if (messageDataProvider.Invalidation.Decision)
		{
			var movementHeader = message.EM_LinkedObject as NctsDepartureMovementHeader;
			movementHeader.GuaranteeTransactionCoordinator.CounterBalanceConfirmedTransactions(message.EM_MessageNum);
		}
	}
}
