using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EDIFactMessageUnpacker))]
sealed class EDIFactMessageUnpackerTest : InterchangeUnpackerTest<EDIFactMessageUnpacker>
{
	public void TestUnpack_Parameters() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("When interchange is null", () => MessageUnpacker.Unpack(null, null, null, Mock.Of<LoggingInformation>()));
		AssertExceptionThrown<ArgumentNullException>("When logger is null", () => MessageUnpacker.Unpack(Factory.New<EDIInterchange>(), null, null, null));
	});

	public void TestUnpack_BodyWithMultipleResponseMessages()
	{
		var entryHeaderOne = CreateEntryHeaderWithReferenceNumber("9133600952024082600059101");
		var entryHeaderTwo = CreateEntryHeaderWithReferenceNumber("93336009520240926000591");

		var messageText = CreateMessageWithHeaderAndFooterSegment(TestMessageOne, TestMessageTwo);

		var ediInterchange = CreateEdiInterchange(messageText);
		var unpackResult = MessageUnpacker.Unpack(ediInterchange, null, null, Mock.Of<LoggingInformation>());
		var messages = unpackResult.EdiMessages;
		AssertEquals("Messages Count", 2, messages.Count);
		AssertEquals("Interchange Messages Count", 2, ediInterchange.ContainedMessages.Count);
		Assert("Success", unpackResult.IsSuccess);

		var messageOne = messages.SingleOrDefault(m => m.EM_LinkUniqueID == entryHeaderOne.PK);
		AssertNotNull("Message with 91336009520240826000591 Reference Number", messageOne);
		CombineAssertions("Message Properties having Reference Number: 91336009520240826000591", () =>
		{
			AssertEDIMessage(messageOne, TestMessageOne, EDIMessageStatusList.Codes.Queued, entryHeaderOne.Branch.PK);
		});

		var messageTwo = messages.SingleOrDefault(m => m.EM_LinkUniqueID == entryHeaderTwo.PK);
		AssertNotNull("Message with 93336009520240926000591 Reference Number", messageTwo);
		CombineAssertions("Message Properties having Reference Number: 93336009520240926000591", () =>
		{
			AssertEDIMessage(messageTwo, TestMessageTwo, EDIMessageStatusList.Codes.Queued, entryHeaderTwo.Branch.PK);
		});
	}

	public void TestUnpack_BodyWithSingleMessage()
	{
		var entryHeader = CreateEntryHeaderWithReferenceNumber("9133600952024082600059102");
		var ediInterchange = CreateEdiInterchange(CreateMessageWithHeaderAndFooterSegment(TestMessageOne));
		var unpackResult = MessageUnpacker.Unpack(ediInterchange, null, null, Mock.Of<LoggingInformation>());

		AssertEquals("Messages Count", 1, unpackResult.EdiMessages.Count);
		AssertEquals("Interchange Messages Count", 1, ediInterchange.ContainedMessages.Count);
		Assert("Success", unpackResult.IsSuccess);

		var message = unpackResult.EdiMessages.Single();
		CombineAssertions("Message Properties having Reference Number: 9133600952024082600059102", () =>
		{
			AssertEquals("EM_LinkUniqueID", entryHeader.PK, message.EM_LinkUniqueID);
			AssertEDIMessage(message, TestMessageOne, EDIMessageStatusList.Codes.Queued, entryHeader.Branch.PK);
		});
	}

	public void TestUnpack_WhenParentCannotBeFound()
	{
		var loggerMock = new Mock<LoggingInformation>();
		var ediInterchange = CreateEdiInterchange(CreateMessageWithHeaderAndFooterSegment(TestMessageOne));
		var unpackResult = MessageUnpacker.Unpack(ediInterchange, null, null, loggerMock.Object);

		AssertEquals("Messages Count", 1, unpackResult.EdiMessages.Count);
		AssertEquals("Interchange Messages Count", 1, ediInterchange.ContainedMessages.Count);
		Assert("Success", unpackResult.IsSuccess);

		var message = unpackResult.EdiMessages.Single();
		var expectedError = $"Unable to find linked entry header for Declaration Id: [9133600952024082600059101] from an Interchange: [{ediInterchange.PK}]. Message Id: [{message.PK}]";
		CombineAssertions("Message Properties for erroneous message", () =>
		{
			AssertEDIMessage(message, TestMessageOne, EDIMessageStatusList.Codes.Error, ediInterchange.EI_GB);
			loggerMock.Verify(l => l.LogError(expectedError));
		});
	}

	[ExpectNoExceptions]
	public void TestUnpackerLoggedInformationMessages()
	{
		_ = CreateEntryHeaderWithReferenceNumber("9133600952024082600059101");
		var loggerMock = new Mock<LoggingInformation>();
		var ediInterchange = CreateEdiInterchange(CreateMessageWithHeaderAndFooterSegment(TestMessageOne));
		_ = MessageUnpacker.Unpack(ediInterchange, null, null, loggerMock.Object);

		CombineAssertions(() =>
		{
			loggerMock.Verify(l => l.Log(LogType.Information, $"Started Interchange [{ediInterchange.PK}] Unpacking"));

			loggerMock.Verify(l => l.Log(LogType.Information, $"Interchange [{ediInterchange.PK}] Unpacking Generated Message(s) Count: [1]"));

			loggerMock.Verify(l => l.Log(LogType.Information, $"Finished Interchange [{ediInterchange.PK}] Unpacking"));
		});
	}

	public void TestUnpackerWithDeclarationIdLengthLessThanTrimLength()
	{
		_ = CreateEntryHeaderWithReferenceNumber("91336009520240826000591");
		var loggerMock = new Mock<LoggingInformation>();
		const string messageWithInvalidDeclarationId = "UNH+740427+CUSRES:1:902:UN:NEP-I+91'" +
			"BGM+932++137:20240826:102+32'" +
			"NAD+IM+988355925::NO1+DENTACARE AS'" +
			"NAD+DT+913360095::NO1'" +
			"DTM+58:20240826:102'" +
			"DTM+184:20240826132846:204'" +
			"TAX+4+161:215'" +
			"RFF+XC:202402028239008'" +
			"RFF+LAR:1'" +
			"RFF+ABT:4460402400183474'" +
			"UNT+11+740427'";

		var ediInterchange = CreateEdiInterchange(CreateMessageWithHeaderAndFooterSegment(messageWithInvalidDeclarationId));
		var unpackResult = MessageUnpacker.Unpack(ediInterchange, null, null, loggerMock.Object);

		AssertEquals("Messages Count", 1, unpackResult.EdiMessages.Count);
		var message = unpackResult.EdiMessages.Single();
		var expectedError = $"Unable to find linked entry header for Declaration Id: [91] from an Interchange: [{ediInterchange.PK}]. Message Id: [{message.PK}]";
		CombineAssertions("Message Properties for erroneous message", () =>
		{
			AssertEDIMessage(message, messageWithInvalidDeclarationId, EDIMessageStatusList.Codes.Error, ediInterchange.EI_GB);
			loggerMock.Verify(l => l.LogError(expectedError));
		});
	}

	protected override string[] ApplicationCodes => new[] { ApplicationCodeList.Codes.NOCustoms };

	void AssertEDIMessage(EDIMessage message, string expectedMessageText, string expectedMessageStatus, ZGuid branchPk)
	{
		AssertEquals("Message Text", expectedMessageText, message.EM_MessageText);
		AssertEquals("Status", expectedMessageStatus, message.EM_Status);
		AssertEquals("Receive Transmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
		AssertEquals("Application Code", ApplicationCodeList.Codes.NOCustoms, message.EM_ApplicationCode);
		AssertEquals("EM_GB", branchPk, message.EM_GB);
	}

	IUniversalCustomsInterchangeUnpacker MessageUnpacker => messgeUnpacker ??= new();
	EDIFactMessageUnpacker messgeUnpacker;

	EDIInterchange CreateEdiInterchange(string messageBody)
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_BodyText = messageBody;
		interchange.EI_ApplicationCode = "NOC";
		return interchange;
	}

	CusEntryHeader CreateEntryHeaderWithReferenceNumber(string referenceNumber)
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = referenceNumber;
		return entryHeader;
	}

	static string CreateMessageWithHeaderAndFooterSegment(params string[] childMessages)
	{
		var messageBuilder = new ZStringBuilder()
			.Append("UNB+UNOA:1+NO000101:NEP+NO011701:NEP+230126:0853+23012608531967'");
		childMessages.ForEach(m => messageBuilder.Append(m));
		messageBuilder.Append("UNZ+1+23012608531967'");
		return messageBuilder.ToString();
	}

	const string TestMessageOne =
		"UNH+740427+CUSRES:1:902:UN:NEP-I+9133600952024082600059101'" +
		"BGM+932++137:20240826:102+32'" +
		"NAD+IM+988355925::NO1+DENTACARE AS'" +
		"NAD+DT+913360095::NO1'" +
		"DTM+58:20240826:102'" +
		"DTM+184:20240826132846:204'" +
		"TAX+4+161:215'" +
		"RFF+XC:202402028239008'" +
		"RFF+LAR:1'" +
		"RFF+ABT:4460402400183474'" +
		"UNT+11+740427'";

	const string TestMessageTwo =
		"UNH+740428+CUSRES:1:902:UN:NEP+9333600952024092600059103'" +
		"BGM+932++137:20240826:102+32'" +
		"NAD+IM+985195455::NO1+JOHNSEN GLASS VINDUSFABRIKK AS'" +
		"NAD+DT+913360095::NO1'" +
		"DTM+58:20240826:102'" +
		"DTM+184:20240826132846:204'" +
		"RFF+XC:202402D'" +
		"RFF+ABT:4460012400690492'" +
		"UNT+9+740428'";
}
