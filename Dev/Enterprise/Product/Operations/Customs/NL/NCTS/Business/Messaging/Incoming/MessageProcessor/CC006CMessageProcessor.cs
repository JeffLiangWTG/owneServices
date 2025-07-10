using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC006C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC006CMessageProcessor : NCTSResponseMessageProcessor<ICC006CDataProvider>
{
	public CC006CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override bool SetNewCustomsStatus => false;

	protected override bool SetNewPhase => false;

	protected override bool SetNewMessageStatus => false;

	protected override ZBool IsMessageOkForProcessing(EDIMessage message)
	{
		var nctsHeader = ((NctsDepartureMovementHeader)message.EM_LinkedObject).Header;
		return !nctsHeader.Logs.Find(l => l.SL_SE_NKEvent == Events.CustomsGuaranteeUpdatedCode && (l.SL_Reference == NLNctsConstants.Logs.References.CC006CMessageReceived || l.SL_Reference == NLNctsConstants.Logs.References.CC045CMessageReceived)).Any();
	}

	protected override BusinessObject FindParentOfMessage(EDIMessage message) => FindParentOfMessageByMRN(message, NctsMovementType.Codes.Departure);

	protected override ZString LogMessageWhenDiscarded => discardMessage;

	protected override ZString NoteMessageWhenDiscarded => discardMessage;

	string discardMessage => NCTSResponseMessageHelper.DiscardedMessageByLogEvent;

	protected override IMessageInterpreter<ICC006CDataProvider> Interpreter => new CC006CMessageInterpreter();

	protected override ICC006CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc006CType, CC006CDataProvider>();

	protected override void ProcessMessageCore(NLEDIMessage message)
	{
		var nctsHeader = ((NctsDepartureMovementHeader)message.EM_LinkedObject).Header;
		var dataProvider = GetMessageDataProvider(message);
		nctsHeader.Logs.AddNew(AutoEvents.CustomsGuaranteeUpdated, NLNctsConstants.Logs.References.CC006CMessageReceived, dataProvider.ArrivalDateAndTimeActual);
	}

	protected override void UpdateGuaranteeTransactionsIfNeeded(NLEDIMessage message, ICC006CDataProvider messageDataProvider)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		moveHeader.GuaranteeTransactionCoordinator.CounterBalanceConfirmedTransactions(message.EM_MessageNum);
	}
}
