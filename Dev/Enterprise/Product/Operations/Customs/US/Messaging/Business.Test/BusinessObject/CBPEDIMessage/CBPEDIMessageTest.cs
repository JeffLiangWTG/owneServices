using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	[TestedType(typeof(CBPMessageForTesting))]
	sealed class CBPEDIMessageTest : BaseEDIMessageTest
	{
		public override void TestIsInterpretationInHtmlFormat()
		{
			var message = GetEDIMessage();
			Assert("Should default to false.", !message.IsInterpretationInHtmlFormat);
		}

		public void TestUpperCaseOfEM_MessageText_AsSpecifiedByUSCustoms()
		{
			CBPMessageForTesting message = Factory.New<CBPMessageForTesting>();
			message.EM_MessageText = "test";
			AssertEquals("TEST", message.EM_MessageText);
		}

		[ExpectExceptionMessage(typeof(ZCannotSaveException),
			"During generation of messages, at least one message '" + ApplicationIdentifierCodeList.DummyForTesting1 + "' has exceeded the maximum number message blocks (9999) allowed by customs.\r\n" +
			"Please modify the job to reduce the size of the messages that are generated.\r\n" +
			"This can be done by changing the merge method or reducing the number of invoice lines on the job.")
		]
		public void TestThrowExceptionIfNewAndHasExceeded9999Limit()
		{
			CBPMessageForTesting message = CreateMessageWithSpecifiedBlocksCount(CBPMessageForTesting.MaxNumMessageBlocksInMessageAllowed - 1);
			Factory.Save();
		}

		public void TestNotifyWhenPlaceHolderReplaceChangesMessageLength()
		{
			var message = Factory.New<CBPMessageForTesting>();
			message.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message.MessageNumberPlaceHolderOverrideForTesting = "<<MSGNO HOLDER>>";
			message.EM_MessageText = "A <<MSGNO HOLDER>> B";
			message.MessageReferenceNumberForTesting = "A".PadRight(17, '1');
			Factory.Save();
			AssertEquals("ErrorReporter.LastKeyReported", "CBPEDIMessage.GetNumberFountainNumbersAndFillInPlaceHoldersBase" + message.GetType().FullName, ErrorReporter.LastKeyReported);
			AssertEquals("ErrorReporter.LastMessageReported", string.Format("Replacing placeholders in '{0} (PK:{1}) ' changed message length", message.GetType().FullName, message.PK), ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestMaxLengthIsNotEnforcedOnIncomingMessages()
		{
			CBPMessageForTesting message = CreateMessageWithSpecifiedBlocksCount(CBPMessageForTesting.MaxNumMessageBlocksInMessageAllowed);
			message.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Receive;
			Factory.Save();
		}

		[ExpectNoExceptions]
		public void TestNotThrowExceptionIfNewAndHasNotExceeded9999Limit()
		{
			CBPMessageForTesting message = CreateMessageWithSpecifiedBlocksCount(CBPMessageForTesting.MaxNumMessageBlocksInMessageAllowed - 2);
			Factory.Save();
		}

		public void TestHasExceeded9999Limit()
		{
			CBPMessageForTesting message = CreateMessageWithSpecifiedBlocksCount(CBPMessageForTesting.MaxNumMessageBlocksInMessageAllowed - 1);
			Assert("Message has more than 9999 blocks", message.HasExceeded9999Limit);

			message = CreateMessageWithSpecifiedBlocksCount(CBPMessageForTesting.MaxNumMessageBlocksInMessageAllowed - 2);
			Assert("Message has NOT more than 9999 blocks", !message.HasExceeded9999Limit);
		}

		public void TestSerialisingUnknownBlock()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "~~";
			staff.GS_FullName = "Nobody Here";

			var outgoing = Factory.New<CBPMessageForTesting>();
			outgoing.EM_ReceiveTransmit = CBPEDIMessage.Direction.Transmit;
			outgoing.EM_MessageNum = "~123";
			outgoing.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			outgoing.EM_SystemCreateUser = "~~";
			var zza1 = new ZZZA() { StringA = "A1", DateA = ZDate.BrettsBirthday.AddDays(1), DecimalA = 1m, IntA = 1, ShortA = 1 };
			var zzb1 = new ZZZB() { StringB = "B1", DateB = ZDate.BrettsBirthday.AddDays(2), DecimalB = 2m, IntB = 2, ShortB = 2 };
			var zzc1 = new ZZZC() { StringC = "C1", DateC = ZDate.BrettsBirthday.AddDays(3), DecimalC = 3m, IntC = 3, ShortC = 3 };
			var zzy1 = new ZZZY() { StringY = "Y1", DateY = ZDate.BrettsBirthday.AddDays(4), DecimalY = 4m, IntY = 4, ShortY = 4 };
			var zzz1 = new ZZZZ() { StringZ = "Z1", DateZ = ZDate.BrettsBirthday.AddDays(5), DecimalZ = 5m, IntZ = 5, ShortZ = 5 };
			outgoing.EM_MessageText = zzb1.Serialise() + zza1.Serialise() + "BOB IS UNKNOWN".PadRight(80) + zzc1.Serialise() + zzz1.Serialise() + zzy1.Serialise();
			ErrorReporter.Clear();
			AssertMultilineASCIIEquals("EM_MessageInterpretation", outgoing.EM_MessageInterpretation,
@"------------------ZZZB------------------
 Date B (5-10)     :20-Sep-71
 Decimal B (11-20) :2
 Int B (21-25)     :2
 Short B (26-30)   :2
 String B (31-70)  :B1

------------------ZZZA------------------
 Date A (5-10)     :19-Sep-71
 Decimal A (11-20) :1
 Int A (21-25)     :1
 Short A (26-30)   :1
 String A (31-70)  :A1

----------UnknownMessageBlock-----------
 Data (1-80) :BOB IS UNKNOWN

------------------ZZZC------------------
 Date C (5-10)     :21-Sep-71
 Decimal C (11-20) :3
 Int C (21-25)     :3
 Short C (26-30)   :3
 String C (31-70)  :C1

------------------ZZZZ------------------
 Date Z (5-10)     :23-Sep-71
 Decimal Z (11-20) :5
 Int Z (21-25)     :5
 Short Z (26-30)   :5
 String Z (31-70)  :Z1

------------------ZZZY------------------
 Date Y (5-10)     :22-Sep-71
 Decimal Y (11-20) :4
 Int Y (21-25)     :4
 Short Y (26-30)   :4
 String Y (31-70)  :Y1
");
		}

		public void TestPropertiesFromRelatedMessages()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "~~";
			staff.GS_FullName = "Nobody Here";

			CBPMessageForTesting outgoing = Factory.New<CBPMessageForTesting>();
			outgoing.EM_ReceiveTransmit = CBPEDIMessage.Direction.Transmit;
			outgoing.EM_MessageNum = "~123";
			outgoing.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			outgoing.EM_SystemCreateUser = "~~";
			var zza1 = new ZZZA() { StringA = "A1", DateA = ZDate.BrettsBirthday.AddDays(1), DecimalA = 1m, IntA = 1, ShortA = 1 };
			var zzb1 = new ZZZB() { StringB = "B1", DateB = ZDate.BrettsBirthday.AddDays(2), DecimalB = 2m, IntB = 2, ShortB = 2 };
			var zzc1 = new ZZZC() { StringC = "C1", DateC = ZDate.BrettsBirthday.AddDays(3), DecimalC = 3m, IntC = 3, ShortC = 3 };
			var zzy1 = new ZZZY() { StringY = "Y1", DateY = ZDate.BrettsBirthday.AddDays(4), DecimalY = 4m, IntY = 4, ShortY = 4 };
			var zzz1 = new ZZZZ() { StringZ = "Z1", DateZ = ZDate.BrettsBirthday.AddDays(5), DecimalZ = 5m, IntZ = 5, ShortZ = 5 };
			outgoing.EM_MessageText = zzb1.Serialise() + zza1.Serialise() + zzc1.Serialise() + zzz1.Serialise() + zzy1.Serialise();
			AssertEquals(false, outgoing.HasRelatedMessage);

			CBPMessageForTesting response = Factory.New<CBPMessageForTesting>();
			response.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			response.EM_MessageNum = "~123";
			response.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddHours(1);
			response.EM_SystemCreateUser = "~BP";
			response.EM_MessageText = zzb1.Serialise() + zzz1.Serialise() + zza1.Serialise() + zzc1.Serialise() + zzy1.Serialise();
			AssertEquals(true, outgoing.HasRelatedMessage);

			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "Customs";
			response.EM_EI = interchange.PK;

			AssertEquals("User", "Customs", outgoing.EM_RelatedMessageCreateUser);
			AssertEquals("CreateTime", ZDateTime.BrettsBirthday.AddHours(1), outgoing.EM_RelatedMessageCreateTime);
			AssertMultilineASCIIEquals("EM_RelatedMessageFormattedMessageText", outgoing.EM_RelatedMessageFormattedMessageText,
@"Z¿ºB09207100000000020000200002B1                                                
Z¿ºZ09237100000000050000500005Z1                                                
Z¿ºA09197100000000010000100001A1                                                
Z¿ºC09217100000000030000300003C1                                                
Z¿ºY09227100000000040000400004Y1");

			AssertMultilineASCIIEquals("EM_MessageInterpretation", outgoing.EM_RelatedMessageInterpretationText,
@"------------------ZZZB------------------
 Date B (5-10)     :20-Sep-71
 Decimal B (11-20) :2
 Int B (21-25)     :2
 Short B (26-30)   :2
 String B (31-70)  :B1

------------------ZZZZ------------------
 Date Z (5-10)     :23-Sep-71
 Decimal Z (11-20) :5
 Int Z (21-25)     :5
 Short Z (26-30)   :5
 String Z (31-70)  :Z1

------------------ZZZA------------------
 Date A (5-10)     :19-Sep-71
 Decimal A (11-20) :1
 Int A (21-25)     :1
 Short A (26-30)   :1
 String A (31-70)  :A1

------------------ZZZC------------------
 Date C (5-10)     :21-Sep-71
 Decimal C (11-20) :3
 Int C (21-25)     :3
 Short C (26-30)   :3
 String C (31-70)  :C1

------------------ZZZY------------------
 Date Y (5-10)     :22-Sep-71
 Decimal Y (11-20) :4
 Int Y (21-25)     :4
 Short Y (26-30)   :4
 String Y (31-70)  :Y1
");

			AssertEquals("User", "Nobody Here", response.EM_RelatedMessageCreateUser);
			AssertEquals("CreateTime", ZDateTime.BrettsBirthday, response.EM_RelatedMessageCreateTime);
			AssertMultilineASCIIEquals("EM_RelatedMessageFormattedMessageText", response.EM_RelatedMessageFormattedMessageText,
@"Z¿ºB09207100000000020000200002B1                                                
Z¿ºA09197100000000010000100001A1                                                
Z¿ºC09217100000000030000300003C1                                                
Z¿ºZ09237100000000050000500005Z1                                                
Z¿ºY09227100000000040000400004Y1");

			AssertMultilineASCIIEquals("EM_RelatedMessageInterpretationText", response.EM_RelatedMessageInterpretationText,
@"------------------ZZZB------------------
 Date B (5-10)     :20-Sep-71
 Decimal B (11-20) :2
 Int B (21-25)     :2
 Short B (26-30)   :2
 String B (31-70)  :B1

------------------ZZZA------------------
 Date A (5-10)     :19-Sep-71
 Decimal A (11-20) :1
 Int A (21-25)     :1
 Short A (26-30)   :1
 String A (31-70)  :A1

------------------ZZZC------------------
 Date C (5-10)     :21-Sep-71
 Decimal C (11-20) :3
 Int C (21-25)     :3
 Short C (26-30)   :3
 String C (31-70)  :C1

------------------ZZZZ------------------
 Date Z (5-10)     :23-Sep-71
 Decimal Z (11-20) :5
 Int Z (21-25)     :5
 Short Z (26-30)   :5
 String Z (31-70)  :Z1

------------------ZZZY------------------
 Date Y (5-10)     :22-Sep-71
 Decimal Y (11-20) :4
 Int Y (21-25)     :4
 Short Y (26-30)   :4
 String Y (31-70)  :Y1
");
		}

		public void TestResponseMessage()
		{
			var message = Factory.New<CBPMessageForTesting>();
			message.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Receive;
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ CBPEDIMessage accessed = message.ResponseMessage; });

			message.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			AssertNoExceptionThrown(delegate
			{ CBPEDIMessage accessed = message.ResponseMessage; });

			var message1 = Factory.New<CBPMessageForTesting>();
			message1.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message1.EM_MessageNum = "~123";
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2012, 6, 1);

			var message2 = Factory.New<CBPMessageForTesting>();
			message2.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Receive;
			message2.EM_MessageNum = "~123";
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2012, 10, 1);

			var message3 = Factory.New<CBPMessageForTesting>();
			message3.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Receive;
			message3.EM_MessageNum = "~123";
			message3.EM_SystemCreateTimeUtc = new ZDateTime(2012, 10, 2);

			AssertNotNull("Should match to received messages", message1.ResponseMessage);

			message1.EM_SystemCreateTimeUtc = message3.EM_SystemCreateTimeUtc.AddDays(-60);
			AssertEquals(message3.PK, message1.ResponseMessage.PK);

			message1 = Factory.New<CBPMessageForTesting>();
			message1.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message1.EM_MessageNum = "~123";
			message1.EM_SystemCreateTimeUtc = message3.EM_SystemCreateTimeUtc.AddDays(-61);
			AssertEquals(message3.PK, message1.ResponseMessage.PK);
		}

		public void TestOriginalMessageWhenMessageNumIsEmpty()
		{
			CBPMessageForTesting incoming1 = Factory.New<CBPMessageForTesting>();
			incoming1.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Receive;
			incoming1.EM_MessageNum = "";//NS messages are not response messages for any outbound. It is like AU CargoStatusMessage

			CBPMessageForTesting incoming2 = Factory.New<CBPMessageForTesting>();
			incoming2.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Receive;
			incoming2.EM_MessageNum = "";//NS messages are not response messages for any outbound. It is like AU CargoStatusMessage

			AssertNull(incoming1.OriginalMessage);
			AssertNull(incoming2.OriginalMessage);
		}

		public void TestFormattedMessage()
		{
			CBPMessageForTesting message = Factory.New<CBPMessageForTesting>();
			message.EM_MessageText = "A".PadRight(81, 'A');

			ZString expected = "A".PadRight(80, 'A') + "\r\n" + "A";
			AssertEquals(expected, message.EM_FormattedMessageText);
		}

		public void TestEM_SendWithMessageErrorsFormatted()
		{
			var message1 = Factory.New<CBPMessageForTesting>();
			message1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			var message2 = Factory.New<CBPMessageForTesting>();
			message2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			var message3 = Factory.New<CBPMessageForTesting>();
			message3.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Internal;

			message1.EM_SendWithMessageErrors = true;
			message2.EM_SendWithMessageErrors = true;
			message3.EM_SendWithMessageErrors = true;
			AssertEquals("Yes", message1.EM_SendWithMessageErrorsFormatted);
			AssertEquals("N/A", message2.EM_SendWithMessageErrorsFormatted);
			AssertEquals("N/A", message3.EM_SendWithMessageErrorsFormatted);

			message1.EM_SendWithMessageErrors = false;
			message2.EM_SendWithMessageErrors = false;
			message3.EM_SendWithMessageErrors = false;
			AssertEquals("No", message1.EM_SendWithMessageErrorsFormatted);
			AssertEquals("N/A", message2.EM_SendWithMessageErrorsFormatted);
			AssertEquals("N/A", message3.EM_SendWithMessageErrorsFormatted);
		}

		public void TestSavingMessageInsertsMessageNumber()
		{
			CBPMessageForTesting message = Factory.New<CBPMessageForTesting>();
			message.MessageReferenceNumberForTesting = "ZZ2343222";
			message.EM_MessageText = "A " + CBPEDIMessage.MessageNumberPlaceHolder + " B";
			message.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			Factory.Save();
			AssertEquals(string.Format("A {0} B", "ZZ2343222".PadRight(CBPEDIMessage.MessageNumberPlaceHolder.Length)), message.EM_MessageText);
		}

		public void TestOriginalMessage()
		{
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{
				var message = Factory.New<CBPMessageForTesting>();
				message.EM_MessageText = "A " + CBPEDIMessage.MessageNumberPlaceHolder + " B";
				var exceptionMessage = message.OriginalMessage;
			});

			var oldMessage = Factory.New<CBPMessageForTesting>();
			oldMessage.EM_MessageNum = "4";
			oldMessage.EM_MessageText = "A " + CBPEDIMessage.MessageNumberPlaceHolder + " B";
			oldMessage.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			oldMessage.EM_SystemCreateTimeUtc = new ZDateTime(2012, 5, 1);
			Factory.Save();

			var originalMessage = Factory.New<CBPMessageForTesting>();
			originalMessage.EM_MessageNum = "4";
			originalMessage.EM_MessageText = "A " + CBPEDIMessage.MessageNumberPlaceHolder + " B";
			originalMessage.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			originalMessage.EM_SystemCreateTimeUtc = new ZDateTime(2012, 6, 2);
			Factory.Save();

			oldMessage.EM_ReceiveTransmit = CBPEDIMessage.Direction.Transmit;
			originalMessage.EM_ReceiveTransmit = CBPEDIMessage.Direction.Transmit;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var responseMessage = newFactory.New<CBPMessageForTesting>();
			responseMessage.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "4";
			responseMessage.EM_SystemCreateTimeUtc = new ZDateTime(2012, 9, 1);

			AssertNotNull("Should match to Original Messages regardless of date ragne", responseMessage.OriginalMessage);
		}

		public void TestOriginalAndResponseMessagesAreMatchBasedOnCreateDateDesc()
		{
			var message1 = Factory.New<CBPMessageForTesting>();
			message1.EM_ReceiveTransmit = CBPEDIMessage.Direction.Transmit;
			message1.EM_MessageNum = "123";
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2015, 4, 1);
			var message2 = Factory.New<CBPMessageForTesting>();
			message2.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			message2.EM_MessageNum = "123";
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2015, 5, 1);

			AssertEquals(message2.PK, message1.ResponseMessage.PK);
			AssertEquals(message1.PK, message2.OriginalMessage.PK);

			var message3 = Factory.New<CBPMessageForTesting>();
			message3.EM_ReceiveTransmit = CBPEDIMessage.Direction.Transmit;
			message3.EM_MessageNum = "123";
			message3.EM_SystemCreateTimeUtc = new ZDateTime(2015, 10, 1);
			var message4 = Factory.New<CBPMessageForTesting>();
			message4.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			message4.EM_MessageNum = "123";
			message4.EM_SystemCreateTimeUtc = new ZDateTime(2015, 9, 1);

			AssertEquals(message4, message3.ResponseMessage);
			AssertEquals(message3, message4.OriginalMessage);
		}

		public void TestGetMessageBlocks()
		{
			CBPMessageForTesting message = Factory.New<CBPMessageForTesting>();
			var zza1 = new ZZZA() { StringA = "A1", DateA = ZDate.BrettsBirthday.AddDays(1), DecimalA = 1m, IntA = 1, ShortA = 1 };
			var zzb1 = new ZZZB() { StringB = "B1", DateB = ZDate.BrettsBirthday.AddDays(2), DecimalB = 2m, IntB = 2, ShortB = 2 };
			var zzc1 = new ZZZC() { StringC = "C1", DateC = ZDate.BrettsBirthday.AddDays(3), DecimalC = 3m, IntC = 3, ShortC = 3 };
			var zzy1 = new ZZZY() { StringY = "Y1", DateY = ZDate.BrettsBirthday.AddDays(4), DecimalY = 4m, IntY = 4, ShortY = 4 };
			var zzz1 = new ZZZZ() { StringZ = "Z1", DateZ = ZDate.BrettsBirthday.AddDays(5), DecimalZ = 5m, IntZ = 5, ShortZ = 5 };
			var zzc2 = new ZZZC() { StringC = "C2", DateC = ZDate.BrettsBirthday.AddDays(6), DecimalC = 6m, IntC = 6, ShortC = 7 };
			var zzc3 = new ZZZC() { StringC = "C3", DateC = ZDate.BrettsBirthday.AddDays(7), DecimalC = 7m, IntC = 7, ShortC = 7 };
			message.EM_MessageText = zzb1.Serialise() + zza1.Serialise() + zzc1.Serialise() + zzc2.Serialise() + zzc3.Serialise() + zzz1.Serialise() + zzy1.Serialise();

			List<ZZZB> zZB1list = message.GetMessageBlocks<ZZZB>();
			AssertEquals(1, zZB1list.Count);
			AssertNotNull(zZB1list.Find(x => x.ApplicationIdentifier == ApplicationIdentifierCodeList.DummyForTesting1 && x.StringB == "B1"));

			List<ZZZC> zZC1list = message.GetMessageBlocks<ZZZC>();
			AssertEquals(3, zZC1list.Count);
			AssertNotNull(zZC1list.Find(x => x.StringC == "C1" && x.IntC == 3));
			AssertNotNull(zZC1list.Find(x => x.StringC == "C2" && x.IntC == 6));
			AssertNotNull(zZC1list.Find(x => x.StringC == "C3" && x.IntC == 7));

			zZC1list = message.GetMessageBlocks<ZZZC>(x => x.IntC == 4);
			AssertEquals(0, zZC1list.Count);

			zZC1list = message.GetMessageBlocks<ZZZC>(x => x.ShortC == 7);
			AssertEquals(2, zZC1list.Count);
			AssertNotNull(zZC1list.Find(x => x.StringC == "C2" && x.ShortC == 7));
			AssertNotNull(zZC1list.Find(x => x.StringC == "C3" && x.ShortC == 7));

			zZC1list = message.GetMessageBlocks<ZZZC>(x => x.StringC == "C3" && x.ShortC == 7);
			AssertEquals(1, zZC1list.Count);

			List<ZZZY> zZY1list = message.GetMessageBlocks<ZZZY>();
			AssertEquals(1, zZY1list.Count);
			AssertNotNull(zZY1list.Find(x => x.StringY == "Y1" && x.ShortY == 4 && x.IntY == 4));
		}

		public void TestGetInvalidMessageBlocks()
		{
			CBPMessageForTesting message = Factory.New<CBPMessageForTesting>();
			var zzc1 = new ZZZC() { StringC = "C1", DateC = ZDate.BrettsBirthday.AddDays(3), DecimalC = 3m, IntC = 3, ShortC = 3 };

			message.EM_MessageText = @"Z¿ºB20097100000000020000200002B1".PadRight(80) + zzc1.Serialise();
			message.EM_MessageType = "FLG";

			var ex = AssertExceptionThrown<InvalidMessageFormatException>(() => message.GetMessageBlocks<ZZZB>());
#if NETFRAMEWORK
			var expectedMessage = @"Data Mismatch in Message.

Error reading DateB from block ZZZB.
'200971' is not a valid MMddyy format
Parameter name: value";
#else
			var expectedMessage = @"Data Mismatch in Message.

Error reading DateB from block ZZZB.
'200971' is not a valid MMddyy format (Parameter 'value')";
#endif
			AssertEquals(expectedMessage, ex.Message);

			AssertEquals(@"Deserialising to Enterprise.Customs.US.Messaging.Business.Testing.ZZZB.DateB
Offset=4, Length=6, Value='200971'
BlockData='Z¿ºB20097100000000020000200002B1                                                '
MessageType=FLG", ex.InnerException.Message);
		}

		public void TestGetMessageBlockApplicationCode()
		{
			var message = Factory.New<CBPMessageForTesting>();
			var appCode = message.GetMessageBlockApplicationCode();
			AssertEquals(CBPEDIInterchange.ApplicationCodeForTesting, appCode);
		}

		public void TestPopulateMessageNumber()
		{
			try
			{
				var message = Factory.New<CBPMessageForTesting>();
				message.MessageReferenceNumberForTesting = "123";
				AssertNullOrEmpty("pre-condition message number is empty", message.EM_MessageNum);
				message.PopulateMessageNumberForTest();
				AssertNotNullOrEmpty("message number should be populated", message.EM_MessageNum);

				var message2 = Factory.New<CBPMessageForTesting>();
				message2.MessageReferenceNumberForTesting = "123";
				message2.EM_MessageNum = "42069";
				message2.PopulateMessageNumberForTest();
				AssertEquals("message number should not be reallocated as there was one already", "42069", message2.EM_MessageNum);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		[UseSnapshotProtection]
		public void TestPopulateMessageNumber_NoDuplicateReference()
		{
			var factory1 = new BusinessObjectFactory();
			var mockMessage1 = factory1.NewMoq<CBPMessageForTesting>();
			mockMessage1.Setup(m => m.OnSaving()).Throws(new Exception("Blah"));
			var message1 = mockMessage1.Object;
			message1.MessageNumberPlaceHolderOverrideForTesting = "<<MSGNO HOLDER>>";
			message1.EM_MessageText = "A <<MSGNO HOLDER>> B";

			var dbConnection = ((CargoWise.Data.IDbConnected)factory1).Connection;
			AssertExceptionThrown(typeof(Exception), () =>
			{
				try
				{
					message1.PopulateMessageNumberForTest();
					factory1.Save();
				}
				finally
				{
					AssertEquals(ZString.Empty, message1.EM_MessageNum);
					AssertEquals("A <<MSGNO HOLDER>> B", message1.EM_MessageText);
				}
			});
			dbConnection.RollbackTransaction();

			var factory2 = new BusinessObjectFactory();
			var message2 = factory2.New<CBPMessageForTesting>();
			message2.MessageNumberPlaceHolderOverrideForTesting = "<<MSGNO HOLDER>>";
			message2.EM_MessageText = "A <<MSGNO HOLDER>> B";
			factory2.Save();
			var numberForMessage2 = message2.EM_MessageNum;
			AssertEquals("Place holder should be replaced by message number", "A " + numberForMessage2.PadRight(16) + " B", message2.EM_MessageText);
			mockMessage1.VerifyAll();

			mockMessage1.Reset();
			dbConnection.BeginTransaction();
			factory1.Save();
			var numberForMessage1 = message1.EM_MessageNum;
			AssertEquals("Place holder should be replaced by message number", "A " + numberForMessage1.PadRight(16) + " B", message1.EM_MessageText);
			AssertNotEquals("Messages should have different number", message1.EM_MessageNum, message2.EM_MessageNum);
			mockMessage1.VerifyAll();
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			CBPMessageForTesting result = Factory.New<CBPMessageForTesting>();
			result.EM_MessageText = "B018888XJ5                                  89             <<MSGNO PLACEHOLDER>>".PadRight(80) +
					"Y  8888XJ5".PadRight(80);
			return result;
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<CBPMessageForTesting>();

		protected override BaseEDIMessage GetEDIMessage() => Factory.New<CBPMessageForTesting>();

		CBPMessageForTesting CreateMessageWithSpecifiedBlocksCount(int blocksCount)
		{
			blocksCount = blocksCount - 2;//because there are 2 blocks added the B and Y block

			CBPMessageForTesting message = Factory.New<CBPMessageForTesting>();
			message.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting1;

			StringBuilder messageText = new StringBuilder(new ZZZB() { StringB = "<<MSGNO PLACEHOLDER>>" }.Serialise());

			var zzc1String = new ZZZC() { StringC = "C1", DateC = ZDate.BrettsBirthday.AddDays(3), DecimalC = 3m, IntC = 3, ShortC = 3 }.Serialise();
			for (int i = 0; i < blocksCount; i++)
			{
				messageText.Append(zzc1String);
			}

			messageText.Append(new ZZZB() { StringB = "<<MSGNO PLACEHOLDER>>", IntB = blocksCount, StringB2 = "ABC" }.Serialise());
			message.EM_MessageText = messageText.ToString();

			return message;
		}
	}

	sealed class FactoryValues
	{
		public bool Value;
	}
}
