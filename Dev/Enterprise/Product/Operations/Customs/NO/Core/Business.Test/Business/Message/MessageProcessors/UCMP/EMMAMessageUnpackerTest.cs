using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EMMAMessageUnpacker))]
sealed class EMMAMessageUnpackerTest : InterchangeUnpackerTest<EMMAMessageUnpacker>
{
	public void TestUnpack_Parameters() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("When interchange is null", () => MessageUnpacker.Unpack(null, null, null, Mock.Of<LoggingInformation>()));
		var interchange = CreateEdiInterchange();
		AssertExceptionThrown<ArgumentNullException>("When parent EDIMessage is null", () => MessageUnpacker.Unpack(interchange, null, null, Mock.Of<LoggingInformation>()));
		AssertExceptionThrown<ArgumentNullException>("When logger is null", () => MessageUnpacker.Unpack(interchange, null, parentEDIMessage, null));
	});

	public void TestUnpack_UnknownInterchangeType()
	{
		var ediInterchange = CreateEdiInterchange();
		ediInterchange.EI_InterchangeType = Constant.MessageTypes.XER;

		var unpackResult = MessageUnpacker.Unpack(ediInterchange, null, parentEDIMessage, Mock.Of<LoggingInformation>());
		AssertEquals("Error reason", $"EDI Interchange Unpacker not found for an EDIInterchange [{ediInterchange.PK}] with type [{ediInterchange.EI_InterchangeType}].", unpackResult.ErrorReason);
	}

	[ExpectNoExceptions]
	public void TestUnpackerLoggedInformationMessages()
	{
		var loggerMock = new Mock<LoggingInformation>();
		var ediInterchange = CreateEdiInterchange();
		var unpackResult = MessageUnpacker.Unpack(ediInterchange, null, parentEDIMessage, loggerMock.Object);

		AssertEquals("[PRE-CONDITION]: Messages Count", 1, unpackResult.EdiMessages.Count);

		CombineAssertions(() =>
		{
			loggerMock.Verify(l => l.Log(LogType.Information, $"Unpacking start for the EDIInterchange [{ediInterchange.PK}]."));
			loggerMock.Verify(l => l.Log(LogType.Information, $"Unpacking finished for the EDIInterchange [{ediInterchange.PK}]."));
		});
	}

	protected override string[] ApplicationCodes => [ApplicationCodeList.Codes.NOCustomsEmma];

	protected override void SetUp()
	{
		base.SetUp();
		interchangeSessionGuid = ZGuid.NewZGuid();
		entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		transmitEDIInterchange = CreateEdiInterchange(EDIInterchange.Direction.Transmit);
		parentEDIMessage = CreateEDIMessage();
	}

	CusEntryHeader entryHeader;
	ZGuid interchangeSessionGuid;
	EDIInterchange transmitEDIInterchange;
	EDIMessage parentEDIMessage;

	IUniversalCustomsInterchangeUnpacker MessageUnpacker => messgeUnpacker ??= new();
	EMMAMessageUnpacker messgeUnpacker;

	EDIMessage CreateEDIMessage()
	{
		var ediMessage = Factory.New<EDIMessage>();
		ediMessage.EM_EI = transmitEDIInterchange.PK;
		ediMessage.EM_LinkUniqueID = entryHeader.PK;
		ediMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
		return ediMessage;
	}

	EDIInterchange CreateEdiInterchange(string direction = EDIInterchange.Direction.Receive)
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_BodyText = xTMessage;
		interchange.EI_ApplicationCode = ApplicationCodeList.Codes.NOCustomsEmma;
		interchange.EI_InterchangeType = Constant.MessageTypes.XLG;
		interchange.EI_SessionGUID = interchangeSessionGuid;
		interchange.EI_ReceiveTransmit = direction;
		return interchange;
	}

	const string xTMessage = @$"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
		<Header>
			<SenderID>WTLXTT</SenderID>
			<RecipientID>NOCustomsEmma</RecipientID>
		</Header>
		<Body>
			{UniversalEvent}
		</Body>
	</UniversalInterchange>";

	const string UniversalEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
		<Event>
			<EventTime>2024-09-09 01:23:45.678</EventTime>
			<EventType>IAK</EventType>
			<EventParameters>
				<MessageType>RES</MessageType>
				<Type></Type>
				<Reason></Reason>
			</EventParameters>
			<ContextCollection>
				<Context>
					<Type>ResponseMessage</Type>
					<Value>OriginalMessage</Value>
				</Context>
			</ContextCollection>
		</Event>
	</UniversalEvent>";
}
