using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC019C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC019CMessageProcessor : NCTSResponseMessageProcessor<ICC019CDataProvider>
{
	public CC019CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString GetNewCustomsStatus(NctsCommonMovementHeader movementHeader) => NCTS5DepartureCustomsStatusList.Codes.DiscrepanciesAtDestination;

	protected override ZBool IsMessageOkForProcessing(EDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		return moveHeader.BM_CustomsStatus != NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;
	}

	protected override ZString LogMessageWhenDiscarded => Res.GetString("0471D443-42F8-4F9D-A358-70611B8ADE8B", "The message is discarded because its 'Status at Customs' is not REL.");

	protected override ZString NoteMessageWhenDiscarded => Res.GetString("D68DE825-9D05-4DB8-AC61-921C07BA902B", "The message with interchange was discarded, because the Status at Customs of the declaration is not REL.");

	protected override IMessageInterpreter<ICC019CDataProvider> Interpreter => new CC019CMessageInterpreter();

	protected override ICC019CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc019CType, CC019CDataProvider>();
}
