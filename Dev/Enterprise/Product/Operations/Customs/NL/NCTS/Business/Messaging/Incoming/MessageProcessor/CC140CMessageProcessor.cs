using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC140C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC140CMessageProcessor : NCTSResponseMessageProcessor<ICC140CDataProvider>
{
	public CC140CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString GetNewCustomsStatus(NctsCommonMovementHeader movementHeader) => NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry;

	protected override ZBool IsMessageOkForProcessing(EDIMessage message) => ((NctsDepartureMovementHeader)message.EM_LinkedObject).BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

	protected override ZString LogMessageWhenDiscarded => Res.GetString("ED67D6BE-3665-4315-9C86-D3460CEBB245", "The message is discarded because its 'Status at Customs' is not REL.");

	protected override ZString NoteMessageWhenDiscarded => Res.GetString("AEFE33AA-9DFA-4178-BF47-046326DDC3C7", "The message with interchange was discarded, because the Status at Customs of the declaration is not REL.");

	protected override IMessageInterpreter<ICC140CDataProvider> Interpreter => new CC140CMessageInterpreter();

	protected override ICC140CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc140CType, CC140CDataProvider>();

	protected override void ProcessMessageCore(NLEDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		var nctsHeader = moveHeader.Header;
		var dataProvider = GetMessageDataProvider(message);

		nctsHeader.Logs.AddNew(AutoEvents.CustomsImpedimentReceived, moveHeader.BM_CustomsStatus + ";Limit Date to respond: " + ((ZDateTime)dataProvider.LimitForResponseDate).ToOffset() + ";MRN: " + dataProvider.MRN, ((ZDateTime)dataProvider.RequestOnNonArrivedMovementDate).ToOffset());

		var customsOfficeOfDestination = moveHeader.CustomsOffices.AddNew();
		customsOfficeOfDestination.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry;
		customsOfficeOfDestination.CY_Data = dataProvider.CustomsOfficeOfEnquiryReferenceNumber;
	}
}
