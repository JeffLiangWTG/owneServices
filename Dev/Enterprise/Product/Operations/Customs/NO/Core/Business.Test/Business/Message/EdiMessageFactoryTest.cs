using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.V902.Messages.CUSRES;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EdiMessageFactory))]
sealed class EdiMessageFactoryTest : TestCaseWithFactory
{
	public void TestGetMessage_ForNullParameter()
	{
		AssertExceptionThrown<ArgumentNullException>(() => EdiMessageFactory.GetMessage(null));
	}

	public void TestFactory()
	{
		AssertType<MessageFactory>(EdiMessageFactory.Factory);
	}

	public void TestGetMessage()
	{
		var ediMessage = Factory.New<EDIMessage>();
		ediMessage.EM_MessageText = TestMessageOne;
		var responseMessage = EdiMessageFactory.GetMessage(ediMessage);
		AssertType<CUSRESMessage>(responseMessage);
	}

	public void TestGetResponseMessages_MultipleMessagesInMessageText()
	{
		var messageText = CreateMessageWithHeaderAndFooterSegments(TestMessageOne, TestMessageTwo);
		var responseMessages = EdiMessageFactory.GetResponseMessages(messageText);

		AssertNotNull(responseMessages);

		var responseMessagesArray = responseMessages.ToArray();
		AssertEquals("MessagesCount", 2, responseMessagesArray.Length);
		CombineAssertions(() =>
		{
			var firstMessage = responseMessagesArray[0];
			AssertEquals("Message One Text", TestMessageOne, firstMessage.MessageText);
			AssertEquals("Message One Reference Number", "9133600952024082600059101", firstMessage.Message.UNH[0].CommonAccessReference);

			var secondMessage = responseMessagesArray[1];
			AssertEquals("Message Two Text", TestMessageTwo, secondMessage.MessageText);
			AssertEquals("Message Two Reference Number", "9333600952024092600059103", secondMessage.Message.UNH[0].CommonAccessReference);
		});
	}

	public void TestGetResponseMessages_WithSingleMessage()
	{
		var responseMessages = EdiMessageFactory.GetResponseMessages(CreateMessageWithHeaderAndFooterSegments(TestMessageOne));
		AssertNotNull(responseMessages);

		var responseMessagesArray = responseMessages.ToArray();
		AssertEquals("MessagesCount", 1, responseMessagesArray.Length);

		CombineAssertions(() =>
		{
			var message = responseMessagesArray[0];
			AssertEquals("Message One Text", TestMessageOne, message.MessageText);
			AssertEquals("Message One Reference Number", "9133600952024082600059101", message.Message.UNH[0].CommonAccessReference);
		});
	}

	public void TestGetResponseMessages_WithMultipleMessageHavingIncorrectMessage()
	{
		const string messageText = "UNH+740427+CUSRES:1:902:UN:NEP-I+9133600952024082600059101'" +
			"BGM+932++137:20240826:102+32'" +
			"NAD+IM+988355925::NO1+DENTACARE AS'" +
			"NAD+DT+913360095::NO1'" +
			"DTM+58:20240826:102'" +
			"DTM+184:20240826132846:204'" +
			"TAX+4+161:215'" +
			"RFF+XC:202402028239008'" +
			"RFF+LAR:1'RFF+ABT:4460402400183474'" +
			"UNT+11+740427'" +
			"UNH+740427+CUSRES:1:902:UN:NEP-I+9133600952024082611159101'" +
			"BGM+932++137:20240826:102+32'" +
			"RFF+ABT:4460402400183474'" +
			"UNT+11+740427'" +
			"ABC+740427+CUSRES:1:111'" +
			"PQG+2332'" +
			"UNH+740427+CUSRES:1:902:UN:NEP-I+9167600952024082611159101'" +
			"BGM+932++137:20240826:102+32'" +
			"RFF+ABT:4460402400183474'" +
			"UNT+11+740427'";

		var responseMessages = EdiMessageFactory.GetResponseMessages(CreateMessageWithHeaderAndFooterSegments(messageText));
		AssertNotNull(responseMessages);

		var responseMessagesArray = responseMessages.ToArray();
		AssertEquals("MessagesCount", 3, responseMessagesArray.Length);
	}

	public void TestGetResponseMessages_WithSingleMessageHavingNoFooterSegment()
	{
		var messageText = new ZStringBuilder()
			.Append("UNB+UNOA:1+NO000101:NEP+NO011701:NEP+230126:0853+23012608531967'")
			.Append(TestMessageOne)
			.ToString();
		var responseMessages = EdiMessageFactory.GetResponseMessages(messageText);
		AssertNotNull(responseMessages);

		var responseMessagesArray = responseMessages.ToArray();
		AssertEquals("MessagesCount", 1, responseMessagesArray.Length);

		CombineAssertions(() =>
		{
			var message = responseMessagesArray[0];
			AssertEquals("Message Text", TestMessageOne, message.MessageText);
		});
	}

	public void TestGetResponseMessage_WithHeaderSegmentInText()
	{
		const string messageText = "UNH+740428+CUSRES:1:902:UN:NEP+9333600952024092600059103'" +
			"BGM+932++137:20240826:102+32'" +
			"NAD+IM+985195455::NO1+JOHANNES UNH+Enders Co.'" +
			"NAD+DT+913360095::NO1'DTM+58:20240826:102'" +
			"DTM+184:20240826132846:204'" +
			"RFF+XC:202402D'" +
			"RFF+ABT:4460012400690492'" +
			"UNT+9+740428'";

		var responseMessages = EdiMessageFactory.GetResponseMessages(CreateMessageWithHeaderAndFooterSegments(messageText));
		AssertNotNull(responseMessages);

		var responseMessagesArray = responseMessages.ToArray();
		AssertEquals("MessagesCount", 1, responseMessagesArray.Length);

		CombineAssertions(() =>
		{
			var message = responseMessagesArray[0];
			AssertEquals("Message Text", messageText, message.MessageText);
		});
	}

	public void TestGetResponseMessage_WithInBetweenHeaderSegmentWithDelimiter()
	{
		const string messageOne =
			"UNH+740428+CUSRES:1:902:UN:NEP+9333600952024092600059103'" +
			"BGM+932++137:20240826:102+32'" +
			"UNT+9+740428'";

		const string messageTwo =
			"UNH+740428+CUSRES:1:902:UN:NEP+9333600952024092600059103'" +
			"BGM+932++137:20240826:102+32'" +
			"TDT+8++3+:::DESTINY?'UNH++WXYZ:2'" +
			"UNT+9+740428'";

		var responseMessages = EdiMessageFactory.GetResponseMessages(CreateMessageWithHeaderAndFooterSegments(messageOne, messageTwo));
		AssertContainsExactElementsInAnyOrder(new[] { messageOne, messageTwo }, responseMessages.Select(m => m.MessageText));
	}

	public void TestGetResponseMessage_WithInvalidCodesMessageText()
	{
		const string message =
			"UNH+740428+CUSRES:1:902:UN:NEP+9333600952024092600059103'" +
			"BGM+932++137:20240826:102+32'" +
			"''''" +
			"NAD+1+2+test'" +
			"AB'" +
			"TDT+5+2+2+:::VALUE'" +
			"X'" +
			"ZZDR+1+2++3++'" +
			"UNT+9+740428'";

		var responseMessages = EdiMessageFactory.GetResponseMessages(CreateMessageWithHeaderAndFooterSegments(message)).ToArray();
		AssertEquals("MessagesCount", 1, responseMessages.Length);
		CombineAssertions(() =>
		{
			var responseMessage = responseMessages[0];
			AssertEquals("Message Text", message, responseMessage.MessageText);
			AssertType<CUSRESMessage>(responseMessage.Message);
		});
	}

	static string CreateMessageWithHeaderAndFooterSegments(params string[] childMessages)
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
