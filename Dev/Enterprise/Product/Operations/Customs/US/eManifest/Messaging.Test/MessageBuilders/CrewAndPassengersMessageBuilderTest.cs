using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.US.eManifest.Business;

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	sealed class CrewAndPassengersMessageBuilderTest : TestCaseWithFactory
	{
		public void TestPopulateCrewAndPassengersDetailsOriginal()
		{
			var builder = new CrewAndPassengersMessageBuilder(MessagingTestHelper.GetCompleteManifestData(Factory, MessageTypes.Codes.CrewAndPassenger, MessageSubTypes.Create, isFinalized: false), MessageTypes.Codes.CrewAndPassenger, MessageSubTypes.Create);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedMessage = @"UNH+1+PAXLST:D:03B:UN
BGM+10:::STANDARD+LOCKMAN0000001+2
RFF+ABO:CRW1
TDT+11++03++LOCK:172
DTM+132:20110826:102
NAD+VW+0000041153:109++AOMAD:CHRIS::::1+11107 SUNSET HILLS ROAD+RESTON+VA:163+20190
ATT+2++M
DTM+329:19350919:102
EMP+4+++1:::8456
NAT+2+US::5
DOC+5K+P100971204141
LOC+91+VA:163
LOC+91+US:162
DOC+39+15504141
LOC+91+US:162
DOC+OTD+13465
NAD+FL+1234:8++TURNER:BILL:BOOTSTRAP:::1
ATT+2++M
DTM+329:19701230:102
NAT+2+US::5
DOC+39+EA12343
LOC+91+US:162
UNT+23+1";
			#endregion
			Factory.Save();
			AssertMultilineASCIIEquals("Message text", expectedMessage, message.EM_FormattedMessageText);
			const string expectedInterpretation = @"<td>10 - Crew/Passengers Details</td></tr><tr><td>Trip Reference</td><td>LOCKMAN0000001</td></tr><tr><td>Message Action Code</td><td>2 - Original</td>";
			AssertContains("Complete Manifest Message Interpretation", expectedInterpretation, message.EM_MessageInterpretation);
		}

		public void TestPopulateCrewAndPassengersDetailsChange()
		{
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var builder = new CrewAndPassengersMessageBuilder(MessagingTestHelper.GetCompleteManifestData(Factory, MessageTypes.Codes.CrewAndPassenger, MessageSubTypes.Change, isFinalized: true), MessageTypes.Codes.CrewAndPassenger, MessageSubTypes.Change);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedMessage = @"UNH+1+PAXLST:D:03B:UN
BGM+10:::STANDARD+LOCKMAN0000001+4
RFF+ABO:CRW1
RFF+RFA:03
TDT+11++03++LOCK:172
DTM+132:20110826:102
NAD+VW+0000041153:109++AOMAD:CHRIS::::1+11107 SUNSET HILLS ROAD+RESTON+VA:163+20190
ATT+2++M
DTM+329:19350919:102
EMP+4+++1:::8456
NAT+2+US::5
DOC+5K+P100971204141
LOC+91+VA:163
LOC+91+US:162
DOC+39+15504141
LOC+91+US:162
DOC+OTD+13465
NAD+FL+1234:8++TURNER:BILL:BOOTSTRAP:::1
ATT+2++M
DTM+329:19701230:102
NAT+2+US::5
DOC+39+EA12343
LOC+91+US:162
UNT+24+1";
			#endregion
			Factory.Save();
			AssertMultilineASCIIEquals("Message text", expectedMessage, message.EM_FormattedMessageText);
			var expectedInterpretation = resourceRetriever.GetString("Enterprise.Customs.US.eManifest.Messaging.Testing.TestFiles.CrewAndPassengersMessageInterpretation.html");
			AssertMultilineASCIIEquals("Complete Manifest Message Interpretation", expectedInterpretation, message.EM_MessageInterpretation.Replace("<tr><td style=", "\r\n<tr><td style="));
		}
	}
}
