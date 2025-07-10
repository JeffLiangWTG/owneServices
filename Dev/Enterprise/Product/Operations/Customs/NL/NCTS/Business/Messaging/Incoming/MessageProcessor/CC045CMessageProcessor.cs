using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC045C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC045CMessageProcessor : NCTSResponseMessageProcessor<ICC045CDataProvider>
{
	public CC045CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString GetNewCustomsStatus(NctsCommonMovementHeader movementHeader) => NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;

	protected override ZString NewPhase => NCTS5DeparturePhaseList.Codes.Declaration;

	protected override ZString NewMessageStatus => LogicalStatusList.Codes.Accepted;

	protected override ZBool IsMessageOkForProcessing(EDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		var nctsHeader = moveHeader.Header;
		interchangeNumber = message.EM_InterchangeNumber;
		isDiscardedByCustomsStatus = moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;
		isDiscardedByLogEvent = nctsHeader.Logs.Find(l => l.SL_SE_NKEvent == Events.CustomsGuaranteeUpdatedCode && l.SL_Reference == NLNctsConstants.Logs.References.CC045CMessageReceived).Any();
		return !isDiscardedByCustomsStatus && !isDiscardedByLogEvent;
	}

	protected override string GetLogMessageForFailingToLinkMessageToParentJob(EDIMessage message) => $"The processing of the message with Interchange No {message.EM_InterchangeNumber} has failed because the message could not be linked to a NCTS departure declaration.";

	protected override ZString LogMessageWhenDiscarded => ZString.Join(" ", new ZString[] { logDiscardMessageByCustomsStatus, discardMessageByLogEvent }).Trim();

	protected override ZString NoteMessageWhenDiscarded => ZString.Join(" ", new ZString[] { noteDiscardMessageByCustomsStatus, discardMessageByLogEvent }).Trim();

	string logDiscardMessageByCustomsStatus => isDiscardedByCustomsStatus ? Res.GetString("f1e2d3c4-b5a6-7890-1234-56789abcdef0", "The message is discarded because its 'Departure Status' has already the status WRO.") : ZString.Empty;

	string noteDiscardMessageByCustomsStatus => isDiscardedByCustomsStatus ? Res.GetString("A4FD6022-BF2F-4746-8FE5-E8C64F7386DB", $"The message with interchange {0} is discarded because its 'Status at Customs' has already the status WRO.", interchangeNumber) : ZString.Empty;

	string discardMessageByLogEvent => isDiscardedByLogEvent ? NCTSResponseMessageHelper.DiscardedMessageByLogEvent : ZString.Empty;

	protected override IMessageInterpreter<ICC045CDataProvider> Interpreter => new CC045CMessageInterpreter();

	protected override void ProcessMessageCore(NLEDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		moveHeader.CustomsEntryStatusLogAdded += MovementHeader_CustomsEntryStatusLogAdded;
		var provider = GetMessageDataProvider(message);
		writeOffDate = ((ZDateTime)(provider.WriteOffDate?.Add(provider.PreparationDateTime.TimeOfDay) ?? default)).ToOffset();

		var nctsHeader = moveHeader.Header;
		nctsHeader.Logs.AddNew(AutoEvents.CustomsGuaranteeUpdated, NLNctsConstants.Logs.References.CC045CMessageReceived, writeOffDate);
	}

	protected override ICC045CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc045CType, CC045CDataProvider>();

	protected override void UpdateGuaranteeTransactionsIfNeeded(NLEDIMessage message, ICC045CDataProvider messageDataProvider)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		if (!moveHeader.Header.Logs.Find(l => l.SL_SE_NKEvent == Events.CustomsGuaranteeUpdatedCode && l.SL_Reference == NLNctsConstants.Logs.References.CC006CMessageReceived).Any())
		{
			moveHeader.GuaranteeTransactionCoordinator.CounterBalanceConfirmedTransactions(message.EM_MessageNum);
		}
	}

	void MovementHeader_CustomsEntryStatusLogAdded(object sender, EventArgs e)
	{
		if (sender is NctsDepartureMovementHeader moveHeader)
		{
			moveHeader.Logs.CreateRecreateOrUpdateEventLog(new EventValue(AutoEvents.CustomsEntryStatus, eventTime: writeOffDate, reference: moveHeader.BM_CustomsStatus));
			moveHeader.CustomsEntryStatusLogAdded -= MovementHeader_CustomsEntryStatusLogAdded;
		}
	}

	string interchangeNumber;
	bool isDiscardedByCustomsStatus;
	bool isDiscardedByLogEvent;

	ZDateTimeOffset writeOffDate;
}
