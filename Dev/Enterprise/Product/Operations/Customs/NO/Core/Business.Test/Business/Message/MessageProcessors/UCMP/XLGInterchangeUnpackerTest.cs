using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(XLGInterchangeUnpacker))]
sealed class XLGInterchangeUnpackerTest : InterchangeUnpackerTest<EMMAMessageUnpacker>
{
	public void TestUnpack_xTMessageWithoutValidEventTypeContext()
	{
		var ediInterchange = CreateEdiInterchange(InvalidUniversalEvent);
		var unpackResult = MessageUnpacker.Unpack(ediInterchange, null, parentEDIMessage, Mock.Of<LoggingInformation>());
		AssertEquals("Error reason", "The xT interchange does not contain a valid Universal XML Event in the body text.", unpackResult.ErrorReason);
	}

	public void TestUnpackInterchange_xTMessageTypeUnknown()
	{
		var ediInterchange = CreateEdiInterchange(CreatexTMessage("XSD", "RES"));
		var unpackResult = MessageUnpacker.Unpack(ediInterchange, null, parentEDIMessage, Mock.Of<LoggingInformation>());
		AssertEquals("Error reason", "The xT interchange contains an invalid Universal EventType: [XSD].", unpackResult.ErrorReason);
	}

	public void TestUnpackInterchange_xTMessageTypeIAK()
		=> AssertUnpackInterchange_xTMessageType("IAK", "RES", xTMessageConstants.MessageTypes.Codes.Acknowledgement);

	public void TestUnpackInterchange_xTMessageTypeIRJ()
		=> AssertUnpackInterchange_xTMessageType("IRJ", "XER", xTMessageConstants.MessageTypes.Codes.Error);

	void AssertUnpackInterchange_xTMessageType(string eventType, string eventParamMessageType, string expMessageType)
	{
		var ediInterchange = CreateEdiInterchange(CreatexTMessage(eventType, eventParamMessageType));
		var unpackResult = MessageUnpacker.Unpack(ediInterchange, null, parentEDIMessage, Mock.Of<LoggingInformation>());

		AssertEquals("Messages Count", 1, unpackResult.EdiMessages.Count);
		AssertEquals("Interchange Messages Count", 1, ediInterchange.ContainedMessages.Count);
		Assert("Success", unpackResult.IsSuccess);

		var message = unpackResult.EdiMessages.Single();
		CombineAssertions(() =>
		{
			AssertEDIMessage(message, expMessageType, EDIMessageStatusList.Codes.Queued, GlbCompany.CurrentCompany.PK);
		});
	}

	void AssertEDIMessage(EDIMessage message, string expMessageType, string expectedMessageStatus, ZGuid branchPk)
	{
		AssertEquals("Message Type", expMessageType, message.EM_MessageType);
		AssertEquals("Message Text", "OriginalMessage", message.EM_MessageText);
		AssertEquals("Status", expectedMessageStatus, message.EM_Status);
		AssertEquals("Receive Transmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
		AssertEquals("Application Code", ApplicationCodeList.Codes.NOCustomsEmma, message.EM_ApplicationCode);
		AssertEquals("EM_GB", GlbBranch.CurrentBranch.PK, message.EM_GB);
		AssertEquals("EM_LinkUniqueID", entryHeader.PK, message.EM_LinkUniqueID);
		AssertEquals("EM_LinkTable", CusEntryHeader.Schema.TableName, message.EM_LinkTable);
	}

	[ExpectNoExceptions]
	public void TestUnpackerLoggedInformationMessages()
	{
		var loggerMock = new Mock<LoggingInformation>();
		var ediInterchange = CreateEdiInterchange(CreatexTMessage("IAK", "RES"));
		var unpackResult = MessageUnpacker.Unpack(ediInterchange, null, parentEDIMessage, loggerMock.Object);

		AssertEquals("[PRE-CONDITION]: Messages Count", 1, unpackResult.EdiMessages.Count);
		var generatedEDIMessage = unpackResult.EdiMessages.Single();
		CombineAssertions(() =>
		{
			loggerMock.Verify(l => l.Log(LogType.Information, $"The XLGInterchangeUnpacker [{ediInterchange.PK}] Unpacking Started."));
			loggerMock.Verify(l => l.Log(LogType.Information, $"The XLGInterchangeUnpacker [{ediInterchange.PK}] Unpacking Finished with EDIMessage: [{generatedEDIMessage.PK}]."));
		});
	}

	protected override string[] ApplicationCodes => [ApplicationCodeList.Codes.NOCustomsEmma];

	protected override void SetUp()
	{
		base.SetUp();
		interchangeSessionGuid = ZGuid.NewZGuid();
		entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		transmitEDIInterchange = CreateEdiInterchange(string.Empty, EDIInterchange.Direction.Transmit);
		parentEDIMessage = CreateEDIMessage();
	}

	CusEntryHeader entryHeader;
	ZGuid interchangeSessionGuid;
	EDIInterchange transmitEDIInterchange;
	EDIMessage parentEDIMessage;

	IUniversalCustomsInterchangeUnpacker MessageUnpacker => messgeUnpacker ??= new();
	XLGInterchangeUnpacker messgeUnpacker;

	EDIMessage CreateEDIMessage()
	{
		var ediMessage = Factory.New<EDIMessage>();
		ediMessage.EM_EI = transmitEDIInterchange.PK;
		ediMessage.EM_LinkUniqueID = entryHeader.PK;
		ediMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
		return ediMessage;
	}

	EDIInterchange CreateEdiInterchange(string messageBody, string direction = EDIInterchange.Direction.Receive)
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_BodyText = messageBody;
		interchange.EI_ApplicationCode = ApplicationCodeList.Codes.NOCustomsEmma;
		interchange.EI_InterchangeType = Constant.MessageTypes.XLG;
		interchange.EI_SessionGUID = interchangeSessionGuid;
		interchange.EI_ReceiveTransmit = direction;
		return interchange;
	}

	string CreatexTMessage(string eventType, string eventParamMessageType)
	{
		var universalEvent = CreateTestUniversalEvent(eventType, eventParamMessageType, "", "");
		return @$"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
			<Header>
				<SenderID>WTLXTT</SenderID>
				<RecipientID>NOCustomsEmma</RecipientID>
			</Header>
			<Body>
				{universalEvent}
			</Body>
		</UniversalInterchange>";
	}

	string CreateTestUniversalEvent(string eventType, string eventParamMessageType, string eventParamType, string reason)
	{
		return @$"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
			<Event>
				<EventTime>2024-09-09 01:23:45.678</EventTime>
				<EventType>{eventType}</EventType>
				<EventParameters>
					<MessageType>{eventParamMessageType}</MessageType>
					<Type>{eventParamType}</Type>
					<Reason>{reason}</Reason>
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

	const string InvalidUniversalEvent = @"<Event>
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
	</Event>";
}
