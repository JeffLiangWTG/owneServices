using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirlineMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.AirlineMessaging;

public class AirlineMessagingProcessorTest : TestCaseWithFactory
{
	const string ResponseWithOneMessage = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\">\r\n  <Header>\r\n    <SenderID>AIR_CARGO_MESSAGING</SenderID>\r\n    <RecipientID>DFODAUPRO</RecipientID>\r\n  </Header>\r\n  <Body>\r\n    <UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2012/11\">\r\n      <Event>\r\n        <DataContext>\r\n          <DocumentaryOverride>\r\n            <SubmissionVersion>1</SubmissionVersion>\r\n          </DocumentaryOverride>\r\n          <DataTargetCollection>\r\n            <DataTarget>\r\n              <Key>C00705167</Key>\r\n              <Type>ForwardingConsol</Type>\r\n            </DataTarget>\r\n          </DataTargetCollection>\r\n        </DataContext>\r\n        <EventTime>2024-08-12T20:20:22</EventTime>\r\n        <EventType>ISN</EventType>\r\n        <EventParameters>\r\n          <Department>WiseTech Global</Department>\r\n          <Reason>Message queued for processing</Reason>\r\n          <MessageType>Air Messaging</MessageType>\r\n          <ReferenceNumber>a0d7010a-2ed6-47c6-8951-0d8f9728169b</ReferenceNumber>\r\n        </EventParameters>\r\n        <ContextCollection>\r\n          <Context>\r\n            <Type>MAWBNumber</Type>\r\n            <Value>618-98234850</Value>\r\n          </Context>\r\n          <Context>\r\n            <Type>OriginalFWBFHLMessage</Type>\r\n            <Value><![CDATA[FWB/16\r\n020-12345001ZRHHKG/T0K1502\r\nFLT/LX040/18/XX044/19\r\nRTG/LAXLX\r\nSHP\r\n/ANTWERP CFS\r\n/200 GEORGE STREET\r\n/SYDNEY/NSW\r\n/AU/2000/TE/3212345678\r\nCNE\r\n/ANTWERP CFS\r\n/200 GEORGE STREET\r\n/SYDNEY/NSW\r\n/AU/2000/TE/3212345678\r\nAGT/1931484/9147203/0005\r\n/TEST COMPANY\r\n/HEATHROW\r\nNFY/ANTWERP CFS\r\n/200 GEORGE STREET\r\n/SYDNEY/NSW\r\n/AU/2000/TE/3212345678\r\nCVD/AUD/PP/CC/NVD/NCV/XXX\r\nRTD/1/K751/CQ/W751/R2.5/T1877.5\r\n/NC/CONSOLIDATION AS PER/ND//CMT120-80-100/2\r\n/2/NC/ATTACHED LIST\r\n/3/K751/CQ/W751/R2.5/T1877.5\r\n/NG/OTHER ASSORTED PARTS/NC/CONSOLIDATION AS PER/NV/MC1.92\r\n/4/NC/ATTACHED LIST\r\nPPD/VC55.5/TX66\r\n/CT121.5\r\nCOL/WT3755/VC33/TX77\r\n/CT3865\r\nISU/14MAR23/BRISBANE CITY\r\nREF//SRW 2/FFW/CWIDWUTDCHF5F/BSL\r\nSPH/GEN/HLC\r\nOCI/AU/SHP/CT/3212345678\r\n/AU/CNE/CT/3212345678\r\n/AU/NFY/CT/3212345678\r\n]]></Value>\r\n          </Context>\r\n        </ContextCollection>\r\n      </Event>\r\n    </UniversalEvent>\r\n  </Body>\r\n</UniversalInterchange>";
	const string ResponseWithManyMessages = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\">\r\n  <Header>\r\n    <SenderID>AIR_CARGO_MESSAGING</SenderID>\r\n    <RecipientID>DFODAUPRO</RecipientID>\r\n  </Header>\r\n  <Body>\r\n    <UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2012/11\">\r\n      <Event>\r\n        <DataContext>\r\n          <DocumentaryOverride>\r\n            <SubmissionVersion>1</SubmissionVersion>\r\n          </DocumentaryOverride>\r\n          <DataTargetCollection>\r\n            <DataTarget>\r\n              <Key>C00705167</Key>\r\n              <Type>ForwardingConsol</Type>\r\n            </DataTarget>\r\n          </DataTargetCollection>\r\n        </DataContext>\r\n        <EventTime>2024-08-12T20:20:22</EventTime>\r\n        <EventType>ISN</EventType>\r\n        <EventParameters>\r\n          <Department>WiseTech Global</Department>\r\n          <Reason>Message queued for processing</Reason>\r\n          <MessageType>Air Messaging</MessageType>\r\n          <ReferenceNumber>a0d7010a-2ed6-47c6-8951-0d8f9728169b</ReferenceNumber>\r\n        </EventParameters>\r\n        <ContextCollection>\r\n          <Context>\r\n            <Type>MAWBNumber</Type>\r\n            <Value>618-98234850</Value>\r\n          </Context>\r\n          <Context>\r\n            <Type>OriginalFWBFHLMessage</Type>\r\n            <Value><![CDATA[FWB/17\r\n618-98234850ZRHHKG/T4K1502\r\nFLT/LX040/18/XX044/19\r\nRTG/LAXLX\r\nSHP\r\nNAM/ANTWERP CFS\r\nADR/200 GEORGE STREET\r\nLOC/SYDNEY/NSW\r\n/AU/2000/TE/3212345678\r\nCNE\r\nNAM/ANTWERP CFS\r\nADR/200 GEORGE STREET\r\nLOC/SYDNEY/NSW\r\n/AU/2000/TE/3212345678\r\nAGT/1931484/9147203/0005\r\n/TEST COMPANY\r\n/HEATHROW\r\nNFY\r\nNAM/ANTWERP CFS\r\nADR/200 GEORGE STREET\r\nLOC/SYDNEY/NSW\r\n/AU/2000/TE/3212345678\r\nCVD/AUD/PP/CC/NVD/NCV/XXX\r\nRTD/1/P2/K751/CQ/W751/R2.5/T1877.5\r\n/NC/CONSOLIDATION AS PER\r\n/2/NC/ATTACHED LIST\r\n/3/P2/K751/CQ/W751/R2.5/T1877.5\r\n/NG/OTHER ASSORTED PARTS\r\n/4/ND//CMT50-50-50/1\r\n/5/NV/MC0.2\r\n/6/NS/615\r\n/7/CX\r\n/NU/PMC12541ET\r\nPPD/VC55.5/TX66\r\n/CT121.5\r\nCOL/WT3755/VC33/TX77\r\n/CT3865\r\nISU/14MAR23/BRISBANE CITY\r\nREF//C00705167/FFW/CWIDWUTDCHF5F/BSL\r\nSPH/GEN/HLC\r\nOCI/AU/SHP/CT/3212345678\r\n/AU/CNE/CT/3212345678\r\n/AU/NFY/CT/3212345678]]></Value>\r\n          </Context>\r\n        </ContextCollection>\r\n      </Event>\r\n    </UniversalEvent>\r\n    <UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2012/11\">\r\n      <Event>\r\n        <DataContext>\r\n          <DocumentaryOverride>\r\n            <SubmissionVersion>1</SubmissionVersion>\r\n          </DocumentaryOverride>\r\n          <DataTargetCollection>\r\n            <DataTarget>\r\n              <Key>C00705167</Key>\r\n              <Type>ForwardingConsol</Type>\r\n            </DataTarget>\r\n          </DataTargetCollection>\r\n        </DataContext>\r\n        <EventTime>2024-08-12T20:20:22</EventTime>\r\n        <EventType>ISN</EventType>\r\n        <EventParameters>\r\n          <Department>WiseTech Global</Department>\r\n          <Reason>Message queued for processing</Reason>\r\n          <MessageType>Air Messaging</MessageType>\r\n          <ReferenceNumber>a0d7010a-2ed6-47c6-8951-0d8f9728169b</ReferenceNumber>\r\n        </EventParameters>\r\n        <ContextCollection>\r\n          <Context>\r\n            <Type>MAWBNumber</Type>\r\n            <Value>618-98234850</Value>\r\n          </Context>\r\n          <Context>\r\n            <Type>OriginalFWBFHLMessage</Type>\r\n            <Value><![CDATA[FHL/5\r\nMBI/618-98234850ZRHHKG/T4K1502\r\nHBS/P00003337/ZRHHKG/4/K1502/2/MOBILE PHONES\r\n/GEN/HLC\r\nTXT/OTHER ASSORTED PARTS\r\nHTS/920510\r\n/920511\r\nOCI/AU/SHP/CT/3212345678\r\n/AU/CNE/CT/3212345678\r\n/AU/NFY/CT/3212345678\r\nSHP\r\nNAM/ANTWERP CFS\r\nADR/200 GEORGE STREET\r\nLOC/SYDNEY/NSW\r\n/AU/2000/TE/3212345678\r\nCNE\r\nNAM/ANTWERP CFS\r\nADR/200 GEORGE STREET\r\nLOC/SYDNEY/NSW\r\n/AU/2000/TE/3212345678\r\nCVD/AUD/CC/NVD/NCV/XXX]]></Value>\r\n          </Context>\r\n        </ContextCollection>\r\n      </Event>\r\n    </UniversalEvent>\r\n    <UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2012/11\">\r\n      <Event>\r\n        <DataContext>\r\n          <DocumentaryOverride>\r\n            <SubmissionVersion>1</SubmissionVersion>\r\n          </DocumentaryOverride>\r\n          <DataTargetCollection>\r\n            <DataTarget>\r\n              <Key>C00705167</Key>\r\n              <Type>ForwardingConsol</Type>\r\n            </DataTarget>\r\n          </DataTargetCollection>\r\n        </DataContext>\r\n        <EventTime>2024-08-12T20:20:22</EventTime>\r\n        <EventType>ISN</EventType>\r\n        <EventParameters>\r\n          <Department>WiseTech Global</Department>\r\n          <Reason>Message queued for processing</Reason>\r\n          <MessageType>Air Messaging</MessageType>\r\n          <ReferenceNumber>a0d7010a-2ed6-47c6-8951-0d8f9728169b</ReferenceNumber>\r\n        </EventParameters>\r\n        <ContextCollection>\r\n          <Context>\r\n            <Type>MAWBNumber</Type>\r\n            <Value>618-98234850</Value>\r\n          </Context>\r\n          <Context>\r\n            <Type>OriginalFWBFHLMessage</Type>\r\n            <Value><![CDATA[FHL/5\r\nMBI/618-98234850ZRHHKG/T4K1502\r\nHBS/P00003337/ZRHHKG/4/K1502/2/MOBILE PHONES\r\n/GEN/HLC\r\nTXT/OTHER ASSORTED PARTS\r\nOCI/AU/NFY/CT/3212345678]]></Value>\r\n          </Context>\r\n        </ContextCollection>\r\n      </Event>\r\n    </UniversalEvent>\r\n    <UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2012/11\">\r\n      <Event>\r\n        <DataContext>\r\n          <DocumentaryOverride>\r\n            <SubmissionVersion>1</SubmissionVersion>\r\n          </DocumentaryOverride>\r\n          <DataTargetCollection>\r\n            <DataTarget>\r\n              <Key>C00705167</Key>\r\n              <Type>ForwardingConsol</Type>\r\n            </DataTarget>\r\n          </DataTargetCollection>\r\n        </DataContext>\r\n        <EventTime>2024-08-12T20:20:22</EventTime>\r\n        <EventType>ISN</EventType>\r\n        <EventParameters>\r\n          <Department>WiseTech Global</Department>\r\n          <Reason>Message queued for processing</Reason>\r\n          <MessageType>Air Messaging</MessageType>\r\n          <ReferenceNumber>a0d7010a-2ed6-47c6-8951-0d8f9728169b</ReferenceNumber>\r\n        </EventParameters>\r\n        <ContextCollection>\r\n          <Context>\r\n            <Type>MAWBNumber</Type>\r\n            <Value>618-98234850</Value>\r\n          </Context>\r\n          <Context>\r\n            <Type>OriginalFWBFHLMessage</Type>\r\n            <Value><![CDATA[FHL/5\r\nMBI/618-98234850ZRHHKG/T4K1502\r\nHBS/P00003337/ZRHHKG/4/K1502/2/MOBILE PHONES\r\n/GEN/HLC\r\nTXT/OTHER ASSORTED PARTS\r\nHTS/920510\r\n/920511\r\nOCI/AU/SHP/CT/3212345678\r\n/AU/CNE/CT/3212345678\r\n/AU/NFY/CT/3212345678\r\nSHP\r\nNAM/ANTWERP CFS\r\nADR/200 GEORGE STREET\r\nLOC/SYDNEY/NSW\r\n/AU/2000/TE/3212345678\r\nCNE\r\nNAM/ANTWERP CFS\r\nADR/200 GEORGE STREET\r\nLOC/SYDNEY/NSW\r\n/AU/2000/TE/3212345678\r\nCVD/AUD/CC/NVD/NCV/XXX]]></Value>\r\n          </Context>\r\n        </ContextCollection>\r\n      </Event>\r\n    </UniversalEvent>\r\n  </Body>\r\n</UniversalInterchange>";
	const string FailureResponseWithNoMessage = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\">\r\n    <Header>\r\n        <SenderID>AIR_CARGO_MESSAGING</SenderID>\r\n        <RecipientID>DFODAUPRO</RecipientID>\r\n    </Header>\r\n    <Body>\r\n        <UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2012/11\">\r\n            <Event>\r\n                <DataContext>\r\n                    <DataTargetCollection>\r\n                        <DataTarget>\r\n                            <Key>C00705167</Key>\r\n                            <Type>ForwardingConsol</Type>\r\n                        </DataTarget>\r\n                    </DataTargetCollection>\r\n                </DataContext>\r\n                <EventTime>2025-03-04T03:48:15</EventTime>\r\n                <EventType>IRJ</EventType>\r\n                <EventParameters>\r\n                    <Department>WiseTech Global</Department>\r\n                    <Reason>Error extracting value for property: shipment.EventBranchHomePort. The value must be Alpha only with a length of 3 characters. Provided value: CHBSL</Reason>\r\n                    <MessageType>Air Messaging</MessageType>\r\n                    <ReferenceNumber>280286c7-df25-4564-a3d1-6c34946a419b</ReferenceNumber>\r\n                </EventParameters>\r\n                <ContextCollection>\r\n                    <Context>\r\n                        <Type>MAWBNumber</Type>\r\n                        <Value>020-12345677</Value>\r\n                    </Context>\r\n                    <Context>\r\n                        <Type>FailureReason</Type>\r\n                        <Value>Error extracting value for property: shipment.EventBranchHomePort. The value must be Alpha only with a length of 3 characters. Provided value: CHBSL</Value>\r\n                    </Context>\r\n                </ContextCollection>\r\n            </Event>\r\n        </UniversalEvent>\r\n    </Body>\r\n</UniversalInterchange>";
	const string ResponseWoEvents = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\">\r\n    <Header>\r\n        <SenderID>AIR_CARGO_MESSAGING</SenderID>\r\n        <RecipientID>DFODAUPRO</RecipientID>\r\n    </Header>\r\n    <Body>There is no Universal Event in this message.</Body>\r\n</UniversalInterchange>";
	const string ResponseWoEventType = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\">\r\n    <Header>\r\n        <SenderID>AIR_CARGO_MESSAGING</SenderID>\r\n        <RecipientID>DFODAUPRO</RecipientID>\r\n    </Header>\r\n    <Body>\r\n        <UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2012/11\">\r\n            <Event>This event has no EventType</Event>\r\n        </UniversalEvent>\r\n    </Body>\r\n</UniversalInterchange>";
	const string ResponseInvalidXml = "This is invalid XML";

	public void TestParseResponse_FailsForInvalidXML()
	{
		var processor = new AirlineMessagingProcessor();
		var result = processor.TryProcessResponse(ResponseInvalidXml, out var originalMessagesByReferenceNumber, out var parsedEventsByReferenceNumber, out var reason);
		Assert("Parsing of response XML should have failed.", !result);
		AssertEquals("Failed to parse message received from Airline Messaging Gateway: There is an error in XML document (1, 1).", reason);
		Assert("Original messages should be empty for invalid xml.", originalMessagesByReferenceNumber.IsNullOrEmpty());
		Assert("Parsed events should be empty for invalid xml.", parsedEventsByReferenceNumber.IsNullOrEmpty());
	}

	public void TestParseResponse_HandlesMissingUniversalEventsInResponse()
	{
		var processor = new AirlineMessagingProcessor();
		var result = processor.TryProcessResponse(ResponseWoEvents, out var originalMessagesByReferenceNumber, out var parsedEventsByReferenceNumber, out var reason);
		AssertNotNull("Logger should generate a result.", result);
		AssertEquals("Failed to parse message received from Airline Messaging Gateway due to XML processing error: 'UniversalEvent' does not exist or it is empty.", reason);
		Assert("Original messages should be empty when missing universal events.", originalMessagesByReferenceNumber.IsNullOrEmpty());
		Assert("Parsed events should be empty when missing universal events.", parsedEventsByReferenceNumber.IsNullOrEmpty());
	}

	public void TestParseResponse_HandlesMissingEventTypeInResponse()
	{
		var processor = new AirlineMessagingProcessor();
		var result = processor.TryProcessResponse(ResponseWoEventType, out var originalMessagesByReferenceNumber, out var parsedEventsByReferenceNumber, out var reason);
		Assert("Parsing of response XML should have failed.", !result);
		AssertEquals("Failed to parse message received from Airline Messaging Gateway due to XML processing error: Line 2: Top Level Element <Event> opened at line 2 cannot be imported as it is missing mandatory elements. Missing: EventTime, EventType.", reason);
		AssertEquals(originalMessagesByReferenceNumber.Count, 1);
		Assert("Parsed events should be empty when missing event type.", parsedEventsByReferenceNumber.IsNullOrEmpty());
	}

	public void TestParseResponse_CanParseInterchangeRejectionMessage()
	{
		var processor = new AirlineMessagingProcessor();
		var result = processor.TryProcessResponse(FailureResponseWithNoMessage, out var originalMessagesByReferenceNumber, out var parsedEventsByReferenceNumber, out var reason);
		Assert("Parsing of response XML should have failed.", !result);
		AssertEquals("Failure/Exception reason should be parsed from message", "Interchange message rejected by API: Error extracting value for property: shipment.EventBranchHomePort. The value must be Alpha only with a length of 3 characters. Provided value: CHBSL", reason);
		AssertEquals(originalMessagesByReferenceNumber.Count, 1);
		AssertEquals(parsedEventsByReferenceNumber.Count, 1);
	}

	public void TestParseResponse_CanParseInterchangeSentMessage()
	{
		var processor = new AirlineMessagingProcessor();
		var result = processor.TryProcessResponse(ResponseWithOneMessage, out var originalMessagesByReferenceNumber, out var parsedEventsByReferenceNumber, out var reason);
		Assert("Parsing of response XML should have succeeded.", result);
		AssertNull("Failure/Exception reason should be null.", reason);
		AssertEquals("Total number of parsed events should be 1.", 1, parsedEventsByReferenceNumber.Count);

		var expectedMessage = Regex.Replace(ResponseWithOneMessage, @"\s+", "");
		foreach (var kvp in parsedEventsByReferenceNumber)
		{
			var refNum = kvp.Key;
			Assert(originalMessagesByReferenceNumber.ContainsKey(refNum));
			var message = Regex.Replace(originalMessagesByReferenceNumber[refNum], @"\s+", "");
			Assert(expectedMessage.Contains(message));
		}
		var parsedEvent = parsedEventsByReferenceNumber.First().Value;
		AssertEquals("Event Type should be interchange success code.", AutoEvents.InterchangeSentCode, parsedEvent.EventType);

		var mawbDetails = parsedEvent.ContextCollection.FirstOrDefault(ctxt => ctxt.Type == "MAWBNumber");
		var messageContents = parsedEvent.ContextCollection.FirstOrDefault(ctxt => ctxt.Type == "OriginalFWBFHLMessage");
		AssertNotNull(mawbDetails);
		AssertEquals("618-98234850", mawbDetails.Value);
		AssertNotNull(messageContents);
		AssertEquals("FWB", GetFwbFhlMessageType(messageContents.Value));
	}

	public void TestParseResponse_CanParseInterchangeSentMessageWithManyMessages()
	{
		var processor = new AirlineMessagingProcessor();
		var result = processor.TryProcessResponse(ResponseWithManyMessages, out var originalMessagesByReferenceNumber, out var parsedEventsByReferenceNumber, out var reason);
		Assert("Parsing of response XML should have succeeded.", result);
		AssertNull("Failure/Exception reason should be null.", reason);
		AssertEquals("Total number of parsed events should be 4.", 4, parsedEventsByReferenceNumber.Count);

		var expectedMessage = Regex.Replace(ResponseWithManyMessages, @"\s+", "");
		foreach (var kvp in parsedEventsByReferenceNumber)
		{
			var refNum = kvp.Key;
			var parsedEvent = kvp.Value;
			Assert(originalMessagesByReferenceNumber.ContainsKey(refNum));

			var message = Regex.Replace(originalMessagesByReferenceNumber[refNum], @"\s+", "");
			Assert(expectedMessage.Contains(message));
			AssertEquals("Event Type should be interchange success code.", AutoEvents.InterchangeSentCode, parsedEvent.EventType);

			var mawbDetails = parsedEvent.ContextCollection.FirstOrDefault(ctxt => ctxt.Type == "MAWBNumber");
			var messageContents = parsedEvent.ContextCollection.FirstOrDefault(ctxt => ctxt.Type == "OriginalFWBFHLMessage");
			AssertNotNull(mawbDetails);
			AssertNotNull(messageContents);

			var messageType = GetFwbFhlMessageType(messageContents.Value);
			Assert(messageType == "FHL" || messageType == "FWB");
		}
	}

	string GetFwbFhlMessageType(string message)
	{
		return message.Contains("FWB/")
			? EDIMessageTypeList.Codes.FWB
			: EDIMessageTypeList.Codes.FHL;
	}
}
