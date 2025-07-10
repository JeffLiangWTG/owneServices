using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.US.eManifest.Business;

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	sealed class TripReportMessageBuilderTest : TestCaseWithFactory
	{
		public void TestPopulateCompleteTripDetailsOriginal()
		{
			var builder = new TripReportMessageBuilder(MessagingTestHelper.GetCompleteManifestData(Factory, MessageTypes.Codes.CompleteTrip, MessageSubTypes.Create, isFinalized: false), MessageTypes.Codes.CompleteTrip, MessageSubTypes.Create);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedMessage = @"UNH+1+CUSREP:D:03B:UN
BGM+336:::STANDARD+LOCKMAN0000001+22
DTM+132:201108260423:203
RFF+ABO:CTR1
RFF+SN:12345
RFF+SN:45613
RFF+IIT:EC
RFF+IIT:MC
LOC+60+2704:77
DOC+700+:23
RFF+AAM:1234654894
QTY+11:6
DOC+700+:23
RFF+AAM:1234654894
QTY+11:6
TAX+10
MOA+67:2000000
FII+SY++::::::HAZMAT SHIPMENT INSURANCE
RFF+ICO:23494564
DTM+429:2011:602
NAD+CA+LOCK:172
NAD+VW+0000041153:109+++11107 SUNSET HILLS ROAD+RESTON+VA:163+20190
NAD+FL+1234:8
TDT+11++03+:::PU++I++:109::64894654
TDT+11++03+:::PU++I++:172::46765464
TDT+11++03+:::PU++I++:8::789543218
TDT+11++03+:::PU++I++:146::1234567890
TDT+11++03+:::PU++I++:274::46456487
TDT+11++03+:::PU++I++:215::BBDD11:US
LOC+89+IL:163
TDT+11++03+:::PU++I++:215::AABB23:US
LOC+89+IL:163
EQD+T1+8987964:109
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
RFF+ABZ:230JIU
LOC+89+IL:163
LOC+89+US:162
EQD+T1+68465464:172
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
RFF+ABZ:230JIU
LOC+89+IL:163
LOC+89+US:162
EQD+T1
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
RFF+ABZ:230JIU
LOC+89+IL:163
LOC+89+US:162
UNT+66+1
";
			#endregion
			Factory.Save();
			AssertMultilineASCIIEquals("Message text", expectedMessage, message.EM_FormattedMessageText);
			const string expectedInterpretation = @"<td>336 - Complete Trip Details</td></tr><tr><td>Trip Reference</td><td>LOCKMAN0000001</td></tr><tr><td>Message Action Code</td><td>22 - Original</td>";
			AssertContains("Complete Trip Details Interpretation", expectedInterpretation, message.EM_MessageInterpretation);
		}

		public void TestPopulateCompleteTripDetailsChange()
		{
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var builder = new TripReportMessageBuilder(MessagingTestHelper.GetCompleteManifestData(Factory, MessageTypes.Codes.CompleteTrip, MessageSubTypes.Change, isFinalized: true), MessageTypes.Codes.CompleteTrip, MessageSubTypes.Change);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedMessage = @"UNH+1+CUSREP:D:03B:UN
BGM+336:::STANDARD+LOCKMAN0000001+4
DTM+132:201108260423:203
RFF+ABO:CTR1
RFF+RFA:03
RFF+SN:12345
RFF+SN:45613
RFF+IIT:EC
RFF+IIT:MC
LOC+60+2704:77
DOC+700+:23
RFF+AAM:1234654894
QTY+11:6
DOC+700+:22
RFF+AAM:1234654894
QTY+11:6
TAX+10
MOA+67:2000000
FII+SY++::::::HAZMAT SHIPMENT INSURANCE
RFF+ICO:23494564
DTM+429:2011:602
NAD+CA+LOCK:172
NAD+VW+0000041153:109+++11107 SUNSET HILLS ROAD+RESTON+VA:163+20190
NAD+FL+1234:8
TDT+11++03+:::PU++I++:109::64894654
TDT+11++03+:::PU++I++:172::46765464
TDT+11++03+:::PU++I++:8::789543218
TDT+11++03+:::PU++I++:146::1234567890
TDT+11++03+:::PU++I++:274::46456487
TDT+11++03+:::PU++I++:215::BBDD11:US
LOC+89+IL:163
TDT+11++03+:::PU++I++:215::AABB23:US
LOC+89+IL:163
EQD+T1+8987964:109
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
RFF+ABZ:230JIU
LOC+89+IL:163
LOC+89+US:162
EQD+T1+68465464:172
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
RFF+ABZ:230JIU
LOC+89+IL:163
LOC+89+US:162
EQD+T1
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
RFF+ABZ:230JIU
LOC+89+IL:163
LOC+89+US:162
UNT+67+1
";
			#endregion
			Factory.Save();
			AssertMultilineASCIIEquals("Message text", expectedMessage, message.EM_FormattedMessageText);
			var expectedInterpretation = resourceRetriever.GetString("Enterprise.Customs.US.eManifest.Messaging.Testing.TestFiles.CompleteTripDetailsMessageInterpretation.html");
			AssertMultilineASCIIEquals("Complete Trip Details Interpretation", expectedInterpretation, message.EM_MessageInterpretation.Replace("<tr><td style=", "\r\n<tr><td style="));
		}

		public void TestPopulateCompleteTripDetailsCancelation()
		{
			var builder = new TripReportMessageBuilder(MessagingTestHelper.GetCompleteManifestData(Factory, MessageTypes.Codes.CompleteTrip, MessageSubTypes.Withdraw, isFinalized: true), MessageTypes.Codes.CompleteTrip, MessageSubTypes.Withdraw);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			const string expectedMessage = @"UNH+1+CUSREP:D:03B:UN
BGM+336:::STANDARD+LOCKMAN0000001+3
DTM+132:201108260423:203
RFF+ABO:CTR1
UNT+5+1
";
			Factory.Save();
			AssertMultilineASCIIEquals("Message text", expectedMessage, message.EM_FormattedMessageText);
			const string expectedInterpretation = @"<td>336 - Complete Trip Details</td></tr><tr><td>Trip Reference</td><td>LOCKMAN0000001</td></tr><tr><td>Message Action Code</td><td>3 - Cancellation</td>";
			AssertContains("Complete Trip Details Interpretation", expectedInterpretation, message.EM_MessageInterpretation);
		}

		public void TestPopulatePreliminaryTripDetailsOriginal()
		{
			var builder = new TripReportMessageBuilder(MessagingTestHelper.GetCompleteManifestData(Factory, MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Create, isFinalized: false), MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Create);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedMessage = @"UNH+1+CUSREP:D:03B:UN
BGM+336:::STANDARD+LOCKMAN0000001+2
DTM+132:201108260423:203
RFF+ABO:PTR1
RFF+SN:12345
RFF+SN:45613
RFF+IIT:EC
RFF+IIT:MC
LOC+60+2704:77
DOC+700+:23
RFF+AAM:1234654894
QTY+11:6
DOC+700+:23
RFF+AAM:1234654894
QTY+11:6
TAX+10
MOA+67:2000000
FII+SY++::::::HAZMAT SHIPMENT INSURANCE
RFF+ICO:23494564
DTM+429:2011:602
NAD+CA+LOCK:172
TDT+11++03+:::PU++I++:109::64894654
TDT+11++03+:::PU++I++:172::46765464
TDT+11++03+:::PU++I++:8::789543218
TDT+11++03+:::PU++I++:146::1234567890
TDT+11++03+:::PU++I++:274::46456487
TDT+11++03+:::PU++I++:215::BBDD11:US
LOC+89+IL:163
TDT+11++03+:::PU++I++:215::AABB23:US
LOC+89+IL:163
EQD+T1+8987964:109
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
RFF+ABZ:230JIU
LOC+89+IL:163
LOC+89+US:162
EQD+T1+68465464:172
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
RFF+ABZ:230JIU
LOC+89+IL:163
LOC+89+US:162
EQD+T1
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
RFF+ABZ:230JIU
LOC+89+IL:163
LOC+89+US:162
UNT+64+1
";
			#endregion
			Factory.Save();
			AssertMultilineASCIIEquals("Message text", expectedMessage, message.EM_FormattedMessageText);
			const string expectedInterpretation = @"<td>336 - Preliminary Trip Details</td></tr><tr><td>Trip Reference</td><td>LOCKMAN0000001</td></tr><tr><td>Message Action Code</td><td>2 - Original</td>";
			AssertContains("Preliminary Trip Details Interpretation", expectedInterpretation, message.EM_MessageInterpretation);
		}

		public void TestPopulatePreliminaryTripDetailsChange()
		{
			var builder = new TripReportMessageBuilder(MessagingTestHelper.GetCompleteManifestData(Factory, MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Change, isFinalized: false), MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Change);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedMessage = @"UNH+1+CUSREP:D:03B:UN
BGM+336:::STANDARD+LOCKMAN0000001+4
DTM+132:201108260423:203
RFF+ABO:PTR1
RFF+SN:12345
RFF+SN:45613
RFF+IIT:EC
RFF+IIT:MC
LOC+60+2704:77
DOC+700+:23
RFF+AAM:1234654894
QTY+11:6
DOC+700+:22
RFF+AAM:1234654894
QTY+11:6
TAX+10
MOA+67:2000000
FII+SY++::::::HAZMAT SHIPMENT INSURANCE
RFF+ICO:23494564
DTM+429:2011:602
NAD+CA+LOCK:172
TDT+11++03+:::PU++I++:109::64894654
TDT+11++03+:::PU++I++:172::46765464
TDT+11++03+:::PU++I++:8::789543218
TDT+11++03+:::PU++I++:146::1234567890
TDT+11++03+:::PU++I++:274::46456487
TDT+11++03+:::PU++I++:215::BBDD11:US
LOC+89+IL:163
TDT+11++03+:::PU++I++:215::AABB23:US
LOC+89+IL:163
EQD+T1+8987964:109
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
RFF+ABZ:230JIU
LOC+89+IL:163
LOC+89+US:162
EQD+T1+68465464:172
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
RFF+ABZ:230JIU
LOC+89+IL:163
LOC+89+US:162
EQD+T1
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
RFF+ABZ:230JIU
LOC+89+IL:163
LOC+89+US:162
UNT+64+1
";
			#endregion
			Factory.Save();
			AssertMultilineASCIIEquals("Message text", expectedMessage, message.EM_FormattedMessageText);
			const string expectedInterpretation = @"<td>336 - Preliminary Trip Details</td></tr><tr><td>Trip Reference</td><td>LOCKMAN0000001</td></tr><tr><td>Message Action Code</td><td>4 - Change</td>";
			AssertContains("Preliminary Trip Details Interpretation", expectedInterpretation, message.EM_MessageInterpretation);
		}

		public void TestPopulatePreliminaryTripDetailsConfirmation()
		{
			var builder = new TripReportMessageBuilder(MessagingTestHelper.GetCompleteManifestData(Factory, MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Confirmation, isFinalized: false), MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Confirmation);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			const string expectedMessage = @"UNH+1+CUSREP:D:03B:UN
BGM+336:::STANDARD+LOCKMAN0000001+6
DTM+132:201108260423:203
RFF+ABO:PTR1
UNT+5+1
";
			Factory.Save();
			AssertEquals("EM_MessageSubType", MessageActionCodes.Codes.Confirmation, message.EM_MessageSubType);
			AssertMultilineASCIIEquals("Message text", expectedMessage, message.EM_FormattedMessageText);
			const string expectedInterpretation = @"<td>336 - Preliminary Trip Details</td></tr><tr><td>Trip Reference</td><td>LOCKMAN0000001</td></tr><tr><td>Message Action Code</td><td>6 - Manifest Completed Confirmation</td>";
			AssertContains("Preliminary Trip Details Interpretation", expectedInterpretation, message.EM_MessageInterpretation);
		}

		public void TestPopulatePreliminaryTripDetailsCancelation()
		{
			var builder = new TripReportMessageBuilder(MessagingTestHelper.GetCompleteManifestData(Factory, MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Withdraw, isFinalized: false), MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Withdraw);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			const string expectedMessage = @"UNH+1+CUSREP:D:03B:UN
BGM+336:::STANDARD+LOCKMAN0000001+3
DTM+132:201108260423:203
RFF+ABO:PTR1
UNT+5+1
";
			Factory.Save();
			AssertMultilineASCIIEquals("Message text", expectedMessage, message.EM_FormattedMessageText);
			const string expectedInterpretation = @"<td>336 - Preliminary Trip Details</td></tr><tr><td>Trip Reference</td><td>LOCKMAN0000001</td></tr><tr><td>Message Action Code</td><td>3 - Cancellation</td>";
			AssertContains("Preliminary Trip Details Interpretation", expectedInterpretation, message.EM_MessageInterpretation);
		}
	}
}
