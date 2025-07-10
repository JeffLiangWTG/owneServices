using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC140C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.Messaging.Business;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class NCTSMessageProcessorBaseOnlyTest : NCTSResponseMessageProcessorAbstractTest<NCTSResponseMessageProcessorForTest, ICC140CDataProvider>
{
	public void TestMessageFriendlyName()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		AssertEquals(ExpectedMessageFriendlyName, messageProcessor.MessageFriendlyName);
	}

	protected override ICC140CDataProvider GetMessageDataProviderMock()
	{
		return Mock.Of<ICC140CDataProvider>(p => p.MRN == "22NL000000000012J1");
	}

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override string InitialCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) =>
	{
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
		mrnEntryNumber.CE_EntryNum = "22NL000000000012J1";
	};

	protected override string ExpectedCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry;
}

public class NCTSResponseMessageProcessorForTest : NCTSResponseMessageProcessor<ICC140CDataProvider>
{
	public NCTSResponseMessageProcessorForTest(LoggingInformation logger) : base(logger)
	{
	}

	protected override ICC140CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc140CType, CC140CDataProvider>();

	protected override BusinessObject FindParentOfMessage(EDIMessage message) => FindParentOfMessageByMRN(message, "D");

	protected override ZString GetNewCustomsStatus(NctsCommonMovementHeader movementHeader) => "ENQ";

	protected override ZBool IsMessageOkForProcessing(EDIMessage message) => true;

	protected override void ProcessMessageCore(NLEDIMessage message)
	{
	}

	protected override IMessageInterpreter<ICC140CDataProvider> Interpreter => new CC140CMessageInterpreter();

	protected override ZString NewPhase => "015";

	protected override ZString NewMessageStatus => "ACC";

	protected override ZString LogMessageWhenDiscarded => "Message for log";

	protected override ZString NoteMessageWhenDiscarded => "Message for note";
}
