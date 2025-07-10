using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using WTG.Foundation.Http;
using static Enterprise.Core.Constants;
using AirlineConstants = Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirlineMessaging.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.AirlineMessaging
{
	public class AirlineMessagingManagerTest : TestCaseWithFactory
	{
		const string ResponseIRJ = "<UniversalInterchange xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\">\r\n  <Header>\r\n    <SenderID>AIR_CARGO_MESSAGING</SenderID>\r\n    <RecipientID>DFODAUPRO</RecipientID>\r\n  </Header>\r\n  <Body>\r\n\t\t<UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\" version=\"1.1\">\r\n\t\t\t<Event>\r\n\t\t\t\t<DataContext>\r\n\t\t\t\t\t<DataTargetCollection>\r\n\t\t\t\t\t\t<DataTarget>\r\n\t\t\t\t\t\t\t<Type>ForwardingConsol</Type>\r\n\t\t\t\t\t\t\t<Key>C00705167</Key>\r\n\t\t\t\t\t\t</DataTarget>\r\n\t\t\t\t\t</DataTargetCollection>\r\n\t\t\t\t</DataContext>\r\n\t\t\t\t<EventTime>2025-03-04T03:48:15</EventTime>\r\n\t\t\t\t<EventType>IRJ</EventType>\r\n\t\t\t\t<EventParameters>\r\n\t\t\t\t\t<Department>WiseTech Global</Department>\r\n\t\t\t\t\t<MessageType>Air Messaging</MessageType>\r\n\t\t\t\t\t<Reason>Something is mandatory.</Reason>\r\n\t\t\t\t\t<ReferenceNumber>280286c7-df25-4564-a3d1-6c34946a419b</ReferenceNumber>\r\n\t\t\t\t</EventParameters>\r\n\t\t\t\t<ContextCollection>\r\n\t\t\t\t\t<Context>\r\n\t\t\t\t\t\t<Type>MAWBNumber</Type>\r\n\t\t\t\t\t\t<Value>020-12345677</Value>\r\n\t\t\t\t\t</Context>\r\n\t\t\t\t\t<Context>\r\n\t\t\t\t\t\t<Type>FailureReason</Type>\r\n\t\t\t\t\t\t<Value>Something is mandatory.</Value>\r\n\t\t\t\t\t</Context>\r\n\t\t\t\t</ContextCollection>\r\n\t\t\t</Event>\r\n\t\t</UniversalEvent>\r\n\t</Body>\r\n</UniversalInterchange>";
		const string ResponseISN = "<UniversalInterchange xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\">\r\n  <Header>\r\n    <SenderID>AIR_CARGO_MESSAGING</SenderID>\r\n    <RecipientID>DFODAUPRO</RecipientID>\r\n  </Header>\r\n  <Body>\r\n\t\t<UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\" version=\"1.1\">\r\n\t\t\t<Event>\r\n\t\t\t\t<DataContext>\r\n\t\t\t\t\t<DocumentaryOverride>\r\n\t\t\t\t\t\t<SubmissionVersion>1</SubmissionVersion>\r\n\t\t\t\t\t</DocumentaryOverride>\r\n\t\t\t\t\t<DataTargetCollection>\r\n\t\t\t\t\t\t<DataTarget>\r\n\t\t\t\t\t\t\t<Type>ForwardingConsol</Type>\r\n\t\t\t\t\t\t\t<Key>C00705167</Key>\r\n\t\t\t\t\t\t</DataTarget>\r\n\t\t\t\t\t</DataTargetCollection>\r\n\t\t\t\t</DataContext>\r\n\t\t\t\t<EventTime>2024-08-12T20:20:22</EventTime>\r\n\t\t\t\t<EventType>ISN</EventType>\r\n\t\t\t\t<EventParameters>\r\n\t\t\t\t\t<Department>WiseTech Global</Department>\r\n\t\t\t\t\t<MessageType>Air Messaging</MessageType>\r\n\t\t\t\t\t<Reason>Message queued for processing</Reason>\r\n\t\t\t\t\t<ReferenceNumber>a0d7010a-2ed6-47c6-8951-0d8f9728169b</ReferenceNumber>\r\n\t\t\t\t</EventParameters>\r\n\t\t\t\t<ContextCollection>\r\n\t\t\t\t\t<Context>\r\n\t\t\t\t\t\t<Type>MAWBNumber</Type>\r\n\t\t\t\t\t\t<Value>020-12345001</Value>\r\n\t\t\t\t\t</Context>\r\n\t\t\t\t\t<Context>\r\n\t\t\t\t\t\t<Type>OriginalFWBFHLMessage</Type>\r\n\t\t\t\t\t\t<Value><![CDATA[FHL/4MBI/020-12345001ZRHHKG/T0K1502HBS/P00003337/ZRHHKG/0/K1502/2/MOBILE PHONES/GEN/HLCTXT/MODEL MEMORY BOARDS AND OTHER ASSORTED PARTSOCI/AU/NFY/CT/3212345678]]></Value>\r\n\t\t\t\t\t</Context>\r\n\t\t\t\t</ContextCollection>\r\n\t\t\t</Event>\r\n\t\t</UniversalEvent>\r\n\t</Body>\r\n</UniversalInterchange>";
		const string ResponseWoEvents = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\">\r\n    <Header>\r\n        <SenderID>AIR_CARGO_MESSAGING</SenderID>\r\n        <RecipientID>DFODAUPRO</RecipientID>\r\n    </Header>\r\n    <Body>There is no Universal Event in this message.</Body>\r\n</UniversalInterchange>";
		const string ResponseWoEventType = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\">\r\n    <Header>\r\n        <SenderID>AIR_CARGO_MESSAGING</SenderID>\r\n        <RecipientID>DFODAUPRO</RecipientID>\r\n    </Header>\r\n    <Body>\r\n        <UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2012/11\">\r\n            <Event>This event has no EventType</Event>\r\n        </UniversalEvent>\r\n    </Body>\r\n</UniversalInterchange>";
		const string ResponseInvalidXml = "This is invalid XML";

		public void TestResponse_MessageReceived()
		{
			var manager = ObjectFactory.Get<IAirlineMessagingManager>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandlerForTest(ResponseISN))))
			{
				var ret = manager.Send(consol, null, out var failureReason);
				var logs = consol.Logs.GetAllLogs().Cast<StmALog>();

				AssertEquals(true, ret);
				AssertNull(failureReason);
				AssertEquals("|DEP=Airline|MST=AirlineMessaging",
					logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.DataExportCode).SL_Reference);
				AssertEquals("|DEP=Airline|MST=AirlineMessaging",
					logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.MessageSentCode).SL_Reference);
				AssertEquals("|DEP=Airline|MST=AirlineMessaging|RES=Message queued for processing",
					logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.InterchangeSentCode).SL_Reference);
			}
		}

		public void TestResponse_MessageRejected()
		{
			var manager = ObjectFactory.Get<IAirlineMessagingManager>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandlerForTest(ResponseIRJ))))
			{
				var ret = manager.Send(consol, null, out var failureReason);
				var logs = consol.Logs.GetAllLogs().Cast<StmALog>();

				AssertEquals(false, ret);
				AssertEquals("Interchange message rejected by API: Something is mandatory.", failureReason);
				AssertEquals("|DEP=Airline|MST=AirlineMessaging",
					logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.DataExportCode).SL_Reference);
				AssertEquals("|DEP=Airline|MST=AirlineMessaging",
					logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.MessageSentCode).SL_Reference);
				AssertEquals("|DEP=Airline|MST=AirlineMessaging|RES=Something is mandatory.",
					logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).SL_Reference);
			}
		}

		public void TestResponse_MessageInvalid()
		{
			var manager = ObjectFactory.Get<IAirlineMessagingManager>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandlerForTest(ResponseWoEvents))))
			{
				var ret = manager.Send(consol, null, out var failureReason);
				var logs = consol.Logs.GetAllLogs().Cast<StmALog>();

				AssertEquals(false, ret);
				AssertEquals("Failed to parse message received from Airline Messaging Gateway due to XML processing error: 'UniversalEvent' does not exist or it is empty.", failureReason);
				AssertEquals("|DEP=Airline|MST=AirlineMessaging",
					logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.DataExportCode).SL_Reference);
				AssertEquals("|DEP=Airline|MST=AirlineMessaging",
					logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.MessageSentCode).SL_Reference);
				AssertEquals("|DEP=Airline|MST=AirlineMessaging|RES=Failed to parse message received from Airline Messaging Gateway due to XML processing error: 'UniversalEvent' does not exist or it is empty.",
					logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.MessageValidationFailedCode).SL_Reference);
			}

			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandlerForTest(ResponseWoEventType))))
			{
				var ret = manager.Send(consol, null, out var failureReason);
				var logs = consol.Logs.GetAllLogs().Cast<StmALog>();

				AssertEquals(false, ret);
				AssertEquals("Failed to parse message received from Airline Messaging Gateway due to XML processing error: Line 2: Top Level Element <Event> opened at line 2 cannot be imported as it is missing mandatory elements. Missing: EventTime, EventType.", failureReason);
				AssertEquals("|DEP=Airline|MST=AirlineMessaging",
					logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.DataExportCode).SL_Reference);
				AssertEquals("|DEP=Airline|MST=AirlineMessaging",
					logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.MessageSentCode).SL_Reference);
				AssertEquals("|DEP=Airline|MST=AirlineMessaging|RES=Failed to parse message received from Airline Messaging Gateway due to XML processing error: Line 2: Top Level Element <Event> opened at line 2 cannot be imported as it is missing mandatory elements. Missing: EventTime, EventType.",
					logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.MessageValidationFailedCode).SL_Reference);
			}

			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandlerForTest(ResponseInvalidXml))))
			{
				var ret = manager.Send(consol, null, out var failureReason);
				var logs = consol.Logs.GetAllLogs().Cast<StmALog>();

				AssertEquals(false, ret);
				AssertEquals("Failed to parse message received from Airline Messaging Gateway: There is an error in XML document (1, 1).", failureReason);
				AssertEquals("|DEP=Airline|MST=AirlineMessaging",
					logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.DataExportCode).SL_Reference);
				AssertEquals("|DEP=Airline|MST=AirlineMessaging",
					logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.MessageSentCode).SL_Reference);
				AssertEquals("|DEP=Airline|MST=AirlineMessaging|RES=Failed to parse message received from Airline Messaging Gateway: There is an error in XML document (1, 1).",
					logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.MessageValidationFailedCode).SL_Reference);
			}
		}

		public void TestSend_InvalidObject()
		{
			var manager = ObjectFactory.Get<IAirlineMessagingManager>();
			var consol = "abc";
			var ret = manager.Send(consol, null, out var failureReason);

			AssertEquals(false, ret);
			AssertEquals($"'{consol}' is not a valid ForwardingConsol instance.", failureReason);
			AssertContains("Exception should be reported", $"Error when calling Airline Messaging Gateway: '{consol}' is not a valid ForwardingConsol instance.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestSend_IncludeAddInfo()
		{
			var manager = ObjectFactory.Get<IAirlineMessagingManager>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var addInfoCollection = new KeyValuePair<string, string>[]
			{
				new ("SendFWB", "true"),
				new ("SendFHL", "false"),
				new ("IncludeECSD", "true"),
				new ("SendFWBOrFHLToAirlineBasedOnMAWBPrefix", "False"),
				new ("SendFWBNatureAndQuantityOfGoodsType", "TRUE"),
				new ("DefaultIdentifierForCneNfyName", "AB"),
				new ("DefaultIdentifierForCneNfyPhone", "CD"),
				new ("AnyOtherAddInfoKey", "TRUE")
			};

			var mockHttpMessageHandlerForTest = new MockHttpMessageHandlerForTest(ResponseISN);
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => mockHttpMessageHandlerForTest)))
			{
				manager.Send(consol, addInfoCollection, out var failureReason);
				AssertNull(failureReason);

				var doc = XDocument.Parse(mockHttpMessageHandlerForTest.ReceivedRequest);
				var pathTemplate = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']='{0}']/*[local-name()='Value']";

				AssertEquals(true, bool.Parse(doc.XPathSelectElement(string.Format(pathTemplate, "SendFWB"))?.Value));
				AssertEquals(false, bool.Parse(doc.XPathSelectElement(string.Format(pathTemplate, "SendFHL"))?.Value));
				AssertEquals(true, bool.Parse(doc.XPathSelectElement(string.Format(pathTemplate, "IncludeECSD"))?.Value));
				AssertEquals(false, bool.Parse(doc.XPathSelectElement(string.Format(pathTemplate, "SendFWBOrFHLToAirlineBasedOnMAWBPrefix"))?.Value));
				AssertEquals(true, bool.Parse(doc.XPathSelectElement(string.Format(pathTemplate, "SendFWBNatureAndQuantityOfGoodsType"))?.Value));
				AssertEquals("AB", doc.XPathSelectElement(string.Format(pathTemplate, "DefaultIdentifierForCneNfyName"))?.Value);
				AssertEquals("CD", doc.XPathSelectElement(string.Format(pathTemplate, "DefaultIdentifierForCneNfyPhone"))?.Value);
				AssertEquals(true, bool.Parse(doc.XPathSelectElement(string.Format(pathTemplate, "AnyOtherAddInfoKey"))?.Value));
			}
		}

		public void TestSend_IncludeAddInfoWithEmptyValue()
		{
			var manager = ObjectFactory.Get<IAirlineMessagingManager>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var addInfoCollection = new KeyValuePair<string, string>[]
			{
				new ("DefaultIdentifierForCneNfyName", ""),
				new ("DefaultIdentifierForCneNfyPhone", ""),
			};

			var mockHttpMessageHandlerForTest = new MockHttpMessageHandlerForTest(ResponseISN);
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => mockHttpMessageHandlerForTest)))
			{
				manager.Send(consol, addInfoCollection, out var failureReason);
				AssertNull(failureReason);

				var doc = XDocument.Parse(mockHttpMessageHandlerForTest.ReceivedRequest);
				var pathTemplate = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']='{0}']/*[local-name()='Value']";

				AssertEquals("", doc.XPathSelectElement(string.Format(pathTemplate, "DefaultIdentifierForCneNfyName"))?.Value);
				AssertEquals("", doc.XPathSelectElement(string.Format(pathTemplate, "DefaultIdentifierForCneNfyPhone"))?.Value);
			}
		}

		public void TestXUS_UNLOCO_To_IATA()
		{
			var manager = ObjectFactory.Get<IAirlineMessagingManager>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var mockHttpMessageHandlerForTest = new MockHttpMessageHandlerForTest(ResponseISN);
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => mockHttpMessageHandlerForTest)))
			{
				manager.Send(consol, null, out var failureReason);
				AssertNull(failureReason);

				var doc = XDocument.Parse(mockHttpMessageHandlerForTest.ReceivedRequest);
				var xPath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='EventBranchHomePort']";
				AssertEquals("EventBranchHomePort should be IATA code with length of 3", 3, doc.XPathSelectElement(xPath).Value.Length);
			}
		}

		public void TestXUS_Include_KnownConsignor()
		{
			var manager = ObjectFactory.Get<IAirlineMessagingManager>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var mockHttpMessageHandlerForTest = new MockHttpMessageHandlerForTest(ResponseISN);

			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => mockHttpMessageHandlerForTest)))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_RL_NKLoadPort = "SGSIN";
				consol.Transports[0].JW_TransportMode = TransportModes.Air;
				consol.Transports[0].JW_TransportType = "FL1";
				consol.Transports[0].JW_RL_NKLoadPort = "SGSIN";

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_InspectionTypeCode = "UNK";

				manager.Send(consol, null, out var failureReason);
				AssertNull(failureReason);

				var doc = XDocument.Parse(mockHttpMessageHandlerForTest.ReceivedRequest);
				var xPath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='CarrierDocumentsOverride']/*[local-name()='AWBHeader']/*[local-name()='KnownConsignor']";
				AssertEquals("RCAR-UC", doc.XPathSelectElement(xPath).Value);
			}
		}

		public void TestXUS_Include_ReceivedFromShipper_ExpiryDate()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "SGSIN";

			var header = consol.AWBHeader;
			header.EH_RN_NKAgentApprovalCountryCode = "AU";
			header.EH_AgentApprovalNumber = "12345";
			header.EH_AgentApprovalExpiryDate = new ZDate(2025, 6, 1);

			var line1 = header.ExportAWBSecurityStatusLines.AddNew();
			line1.EAS_ApprovalCategory = "AC";
			line1.EAS_RN_NKCountryCode = "NZ";
			line1.EAS_ApprovalNumber = "41235-45";
			line1.EAS_ApprovalExpiryDate = new ZDate(2025, 7, 1);

			var mockHttpMessageHandlerForTest = new MockHttpMessageHandlerForTest(ResponseISN);
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => mockHttpMessageHandlerForTest)))
			{
				var manager = ObjectFactory.Get<IAirlineMessagingManager>();
				manager.Send(consol, null, out var failureReason);
				AssertNull(failureReason);

				var doc = XDocument.Parse(mockHttpMessageHandlerForTest.ReceivedRequest);
				var xPath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='CarrierDocumentsOverride']/*[local-name()='AWBHeader']/*[local-name()='CargoSecurityDeclaration']/*[local-name()='ReceivedFromShipperCollection']/*[local-name()='ReceivedFromShipper']/*[local-name()='{0}']";
				AssertEquals("AC", doc.XPathSelectElement(string.Format(xPath, "Code")).Value);
				AssertEquals("Account Consignor", doc.XPathSelectElement(string.Format(xPath, "Description")).Value);
				AssertEquals("2025-07-01T00:00:00", doc.XPathSelectElement(string.Format(xPath, "ExpiryDate")).Value);
				AssertEquals("41235-45", doc.XPathSelectElement(string.Format(xPath, "Number")).Value);
			}
		}

		public void TestXUS_Include_AESCollection()
		{
			MockHttpMessageHandlerForTest mockHttpMessageHandlerForTest;
			CountryExportStatementSettingCollection defaultValue;
			MockingData(out mockHttpMessageHandlerForTest, out defaultValue);

			using (FreightDataRegistry.Instance.ExportStatementSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => mockHttpMessageHandlerForTest)))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_RL_NKOrigin = "USATL";
				shipment1.JS_RL_NKDestination = "AUSYD";
				shipment1.DocsAndCartage.JP_ExportStatement = "PRF";
				shipment1.JS_UniqueConsignRef = "shipment0001";

				var cusEntryNumber1 = shipment1.CusEntryNumbers.AddNew();
				cusEntryNumber1.CE_EntryType = CusEntryNumberTypes.UnitedStates.ITN;
				cusEntryNumber1.CE_EntryNum = "X20100101987654";

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_RL_NKOrigin = "USATL";
				shipment2.JS_RL_NKDestination = "AUSYD";
				shipment2.DocsAndCartage.JP_ExportStatement = "PRF";
				shipment2.JS_UniqueConsignRef = "shipment0002";

				var cusEntryNumber2 = shipment2.CusEntryNumbers.AddNew();
				cusEntryNumber2.CE_EntryType = CusEntryNumberTypes.UnitedStates.ITN;
				cusEntryNumber2.CE_EntryNum = "X20250101654321";

				var manager = ObjectFactory.Get<IAirlineMessagingManager>();
				manager.Send(consol, null, out var failureReason);
				AssertNull(failureReason);

				var doc = XDocument.Parse(mockHttpMessageHandlerForTest.ReceivedRequest);
				var xPath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='CarrierDocumentsOverride']/*[local-name()='AWBHeader']/*[local-name()='AESExportCollection']/*[local-name()='AESExport']";
				var shipmentAESNodes = doc.XPathSelectElements(xPath).ToArray();
				AssertEquals("AES X20100101987654", shipmentAESNodes[0].Value);
				AssertEquals("AES X20250101654321", shipmentAESNodes[1].Value);

				xPath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='SubShipmentCollection']/*[local-name()='SubShipment']/*[local-name()='CarrierDocumentsOverride']/*[local-name()='AWBHeader']/*[local-name()='AESExportCollection']/*[local-name()='AESExport']";
				var subShipmentAESNodes = doc.XPathSelectElements(xPath).ToArray();
				AssertEquals("AES X20100101987654", subShipmentAESNodes[0].Value);
				AssertEquals("AES X20250101654321", subShipmentAESNodes[1].Value);
			}
		}

		static void MockingData(out MockHttpMessageHandlerForTest mockHttpMessageHandlerForTest, out CountryExportStatementSettingCollection defaultValue)
		{
			mockHttpMessageHandlerForTest = new MockHttpMessageHandlerForTest(ResponseISN);
			defaultValue = new CountryExportStatementSettingCollection();
			var exportStatementSetting = defaultValue.AddNew();
			exportStatementSetting.CountryCode = CountryCodes.UnitedStates;
			exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "PRF",
				"AES", "AES Proof of Filing Citation", CusEntryNumberTypes.UnitedStates.ITN, "", "UDF", true,
				true, true, true, true, true));
		}

		public void TestCanNotSetConsHeaderInAESExportCollectionForSubShipment()
		{
			MockHttpMessageHandlerForTest mockHttpMessageHandlerForTest;
			CountryExportStatementSettingCollection defaultValue;
			MockingData(out mockHttpMessageHandlerForTest, out defaultValue);
			var xPath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='CarrierDocumentsOverride']/*[local-name()='AWBHeader']/*[local-name()='AESExportCollection']/*[local-name()='AESExport']";

			using (FreightDataRegistry.Instance.ExportStatementSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => mockHttpMessageHandlerForTest)))
			{
				ForwardingConsol consol;
				CreateConsolAndSubShipment(out consol, "PRF");

				var manager = ObjectFactory.Get<IAirlineMessagingManager>();
				manager.Send(consol, null, out var failureReason);
				AssertNull(failureReason);

				var doc = XDocument.Parse(mockHttpMessageHandlerForTest.ReceivedRequest);
				var consolAESNodes = doc.XPathSelectElements(xPath).ToArray();
				AssertEquals("There are 2 AES Nodes", 2, consolAESNodes.Length);
				AssertEquals("AES X20100101987654", consolAESNodes[0].Value);
				AssertEquals("AES X20250101654321", consolAESNodes[1].Value);

				CreateConsolAndSubShipment(out consol, "LOW");

				manager.Send(consol, null, out var failureReason2);
				AssertNull(failureReason2);

				doc = XDocument.Parse(mockHttpMessageHandlerForTest.ReceivedRequest);
				var consolAESNodes2 = doc.XPathSelectElements(xPath).ToArray();
				AssertEquals("There is only 1 AES Node", 1, consolAESNodes2.Length);
				AssertEquals("AES X20250101654321", consolAESNodes2[0].Value);
			}
		}

		void CreateConsolAndSubShipment(out ForwardingConsol consol, string exportStatement)
		{
			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "USATL";
			shipment1.JS_RL_NKDestination = "AUSYD";
			shipment1.DocsAndCartage.JP_ExportStatement = exportStatement;
			shipment1.JS_UniqueConsignRef = "shipment0001";

			var cusEntryNumber1 = shipment1.CusEntryNumbers.AddNew();
			cusEntryNumber1.CE_EntryType = CusEntryNumberTypes.UnitedStates.ITN;
			cusEntryNumber1.CE_EntryNum = "X20100101987654";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_RL_NKOrigin = "USATL";
			shipment2.JS_RL_NKDestination = "AUSYD";
			shipment2.DocsAndCartage.JP_ExportStatement = "PRF";
			shipment2.JS_UniqueConsignRef = "shipment0002";

			var cusEntryNumber2 = shipment2.CusEntryNumbers.AddNew();
			cusEntryNumber2 = shipment2.CusEntryNumbers.AddNew();
			cusEntryNumber2.CE_EntryType = CusEntryNumberTypes.UnitedStates.ITN;
			cusEntryNumber2.CE_EntryNum = "X20250101654321";
		}

		const string ACASinShipmentXPath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='CarrierDocumentsOverride']/*[local-name()='AWBHeader']/*[local-name()='ACAS']";
		const string ACASinSubShipmentXPath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='SubShipmentCollection']/*[local-name()='SubShipment']/*[local-name()='CarrierDocumentsOverride']/*[local-name()='AWBHeader']/*[local-name()='ACAS']";

		public void TestXUS_Include_ACASValues_AgentConsol()
		{
			var mockHttpMessageHandlerForTest = new MockHttpMessageHandlerForTest(ResponseISN);

			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => mockHttpMessageHandlerForTest)))
			{
				var senderAddress = Factory.NewWithValidTestData<OrgAddress>();
				senderAddress.OA_CompanyNameOverride = "AWBAUCOMPANYNAME";
				senderAddress.OA_RL_NKRelatedPortCode = "AUSYD";
				senderAddress.OA_Email = "sender@sender.com";
				senderAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

				var receiverAddress = Factory.NewWithValidTestData<OrgAddress>();
				receiverAddress.OA_CompanyNameOverride = "AWBUSCOMPANYNAME";
				receiverAddress.OA_RL_NKRelatedPortCode = "USLAX";
				receiverAddress.OA_Email = "receiver@receiver.com";
				receiverAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

				var refAirline = Factory.NewWithValidTestData<RefAirline>();
				refAirline.RM_TwoCharacterCode = "LH";
				refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "020";

				var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
				carrierOrg.CompanyData.OB_APAirlineAccountNumber = "Air Acc Num";
				carrierOrg.MiscServ.OM_RM_Airline = refAirline.PK;

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_AgentType = AgentType.Agent;
				consol.JK_OA_SendingForwarderAddress = senderAddress.PK;
				consol.JK_OA_ReceivingForwarderAddress = receiverAddress.PK;
				consol.SendingForwarder.CompanyData.OB_APCreditAgreedPaymentMethod = "CBC";
				consol.SendingForwarder.OH_IsCreditor = true;
				consol.SendingForwarder.OH_SystemCreateTimeUtc = new ZDateTime(2024, 06, 12, 12, 30, 00);
				consol.JK_OA_ShippingLineAddress = carrierOrg.MainAddress.PK;

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_RL_NKOrigin = "AUSYD";
				shipment1.JS_RL_NKDestination = "USATL";
				shipment1.JS_UniqueConsignRef = "shipment0001";

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_RL_NKOrigin = "AUSYD";
				shipment2.JS_RL_NKDestination = "USATL";
				shipment2.JS_UniqueConsignRef = "shipment0002";

				var consignee1 = Factory.NewWithValidTestData<OrgHeader>();
				consignee1.MainAddress.OA_Email = "consignee1@consignee1.com";

				var consignee2 = Factory.NewWithValidTestData<OrgHeader>();
				consignee2.MainAddress.OA_Email = "consignee2@consignee2.com";

				var shipper1 = Factory.NewWithValidTestData<OrgHeader>();
				shipper1.MainAddress.OA_Email = "shipper1@shipper1.com";

				var shipper2 = Factory.NewWithValidTestData<OrgHeader>();
				shipper2.MainAddress.OA_Email = "shipper2@shipper2.com";

				shipment1.ConsigneePK = consignee1.PK;
				shipment1.ConsignorPK = shipper1.PK;
				shipment2.ConsigneePK = consignee2.PK;
				shipment2.ConsignorPK = shipper2.PK;

				var manager = ObjectFactory.Get<IAirlineMessagingManager>();
				manager.Send(consol, null, out var ex);
				var doc = XDocument.Parse(mockHttpMessageHandlerForTest.ReceivedRequest);

				// BiographicData - Agent consol should not have BiographicData
				AssertEquals(null, doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='BiographicData']"));

				// CustAccBillingType and CustAccEstDate
				AssertEquals("CSH", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='CustAccBillingType']").Value);
				AssertEquals("12Jun24", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='CustAccEstDate']").Value);

				// CustomerAccountHolder and CustomerAccountName
				AssertEquals("3", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='CustomerAccountHolder']").Value);
				AssertEquals("AWBAUCOMPANYNAME", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='CustomerAccountName']").Value);

				// CustomerAccountIssuer and CustomerAccountNumber
				AssertEquals("020", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='CustomerAccountIssuer']").Value);
				AssertEquals("AirAccNum", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='CustomerAccountNumber']").Value);

				// IP addresses
				AssertEquals("127.0.0.1", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='IPAddressAccCreation']").Value);
				AssertEquals("127.0.0.1", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='IPAddressRqShpBillCreation']").Value);
				AssertEquals("127.0.0.1", doc.XPathSelectElements($"{ACASinSubShipmentXPath}/*[local-name()='IPAddressAccCreation']").ToArray()[0].Value);
				AssertEquals("127.0.0.1", doc.XPathSelectElements($"{ACASinSubShipmentXPath}/*[local-name()='IPAddressRqShpBillCreation']").ToArray()[0].Value);
				AssertEquals("127.0.0.1", doc.XPathSelectElements($"{ACASinSubShipmentXPath}/*[local-name()='IPAddressAccCreation']").ToArray()[1].Value);
				AssertEquals("127.0.0.1", doc.XPathSelectElements($"{ACASinSubShipmentXPath}/*[local-name()='IPAddressRqShpBillCreation']").ToArray()[1].Value);

				// Shipping frequency
				AssertEquals("O", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='CustAccShippingFrequency']").Value);
				AssertEquals("O", doc.XPathSelectElements($"{ACASinSubShipmentXPath}/*[local-name()='CustAccShippingFrequency']").ToArray()[0].Value);
				AssertEquals("O", doc.XPathSelectElements($"{ACASinSubShipmentXPath}/*[local-name()='CustAccShippingFrequency']").ToArray()[1].Value);

				// VerifiedKnownConsignor
				AssertEquals("N", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='VerifiedKnownConsignor']").Value);
				AssertEquals("N", doc.XPathSelectElements($"{ACASinSubShipmentXPath}/*[local-name()='VerifiedKnownConsignor']").ToArray()[0].Value);
				AssertEquals("N", doc.XPathSelectElements($"{ACASinSubShipmentXPath}/*[local-name()='VerifiedKnownConsignor']").ToArray()[1].Value);

				// Email local and domains
				AssertEquals("sender", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='ShipperEmailLocal']").Value);
				AssertEquals("sender.com", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='ShipperEmailDomain']").Value);
				AssertEquals("receiver", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='ConsigneeEmailLocal']").Value);
				AssertEquals("receiver.com", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='ConsigneeEmailDomain']").Value);

				AssertEquals("shipper1", doc.XPathSelectElements($"{ACASinSubShipmentXPath}/*[local-name()='ShipperEmailLocal']").ToArray()[0].Value);
				AssertEquals("shipper1.com", doc.XPathSelectElements($"{ACASinSubShipmentXPath}/*[local-name()='ShipperEmailDomain']").ToArray()[0].Value);
				AssertEquals("shipper2", doc.XPathSelectElements($"{ACASinSubShipmentXPath}/*[local-name()='ShipperEmailLocal']").ToArray()[1].Value);
				AssertEquals("shipper2.com", doc.XPathSelectElements($"{ACASinSubShipmentXPath}/*[local-name()='ShipperEmailDomain']").ToArray()[1].Value);

				AssertEquals("consignee1", doc.XPathSelectElements($"{ACASinSubShipmentXPath}/*[local-name()='ConsigneeEmailLocal']").ToArray()[0].Value);
				AssertEquals("consignee1.com", doc.XPathSelectElements($"{ACASinSubShipmentXPath}/*[local-name()='ConsigneeEmailDomain']").ToArray()[0].Value);
				AssertEquals("consignee2", doc.XPathSelectElements($"{ACASinSubShipmentXPath}/*[local-name()='ConsigneeEmailLocal']").ToArray()[1].Value);
				AssertEquals("consignee2.com", doc.XPathSelectElements($"{ACASinSubShipmentXPath}/*[local-name()='ConsigneeEmailDomain']").ToArray()[1].Value);
			}
		}

		public void TestXUS_Include_ACASValues_DirectConsol()
		{
			var mockHttpMessageHandlerForTest = new MockHttpMessageHandlerForTest(ResponseISN);

			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => mockHttpMessageHandlerForTest)))
			{
				var senderAddress = Factory.NewWithValidTestData<OrgAddress>();
				senderAddress.OA_CompanyNameOverride = "AWBAUCOMPANYNAME";
				senderAddress.OA_RL_NKRelatedPortCode = "AUSYD";
				senderAddress.OA_Email = "sender@sender.com";
				senderAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

				var receiverAddress = Factory.NewWithValidTestData<OrgAddress>();
				receiverAddress.OA_CompanyNameOverride = "AWBUSCOMPANYNAME";
				receiverAddress.OA_RL_NKRelatedPortCode = "USLAX";
				receiverAddress.OA_Email = "receiver@receiver.com";
				receiverAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

				var refAirline = Factory.NewWithValidTestData<RefAirline>();
				refAirline.RM_TwoCharacterCode = "LH";
				refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "020";

				var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
				carrierOrg.CompanyData.OB_APAirlineAccountNumber = "Air Acc Num";
				carrierOrg.MiscServ.OM_RM_Airline = refAirline.PK;

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_AgentType = AgentType.Direct;
				consol.JK_OA_SendingForwarderAddress = senderAddress.PK;
				consol.JK_OA_ReceivingForwarderAddress = receiverAddress.PK;
				consol.SendingForwarder.CompanyData.OB_APCreditAgreedPaymentMethod = "CBC";
				consol.SendingForwarder.OH_IsCreditor = true;
				consol.SendingForwarder.OH_SystemCreateTimeUtc = new ZDateTime(2024, 06, 12, 12, 30, 00);
				consol.JK_OA_ShippingLineAddress = carrierOrg.MainAddress.PK;
				consol.JK_OverrideWaybillDefaults = true;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USATL";
				shipment.JS_UniqueConsignRef = "shipment0001";

				var proxyOrgCusCode = GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew();
				proxyOrgCusCode.OK_RN_NKCodeCountry = "US";
				proxyOrgCusCode.OK_CodeType = "CCA";
				proxyOrgCusCode.SecuredCustomsRegNo = "USCCAREGNO";

				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				consignee.MainAddress.OA_Email = "consignee@consignee.com";

				var shipper = Factory.NewWithValidTestData<OrgHeader>();
				shipper.MainAddress.OA_Email = "shipper@shipper.com";

				var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
				controllingCustomer.OH_FullName = "Controlling Customer Name";
				controllingCustomer.OH_Category = "NAT";
				controllingCustomer.OH_IsCreditor = true;
				controllingCustomer.OH_SystemCreateTimeUtc = new ZDateTime(2024, 06, 12, 12, 30, 00);
				controllingCustomer.OH_Code = "OHCOD";

				var cusCode = controllingCustomer.CustomsCodes.AddNew();
				cusCode.OK_RN_NKCodeCountry = "US";
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
				cusCode.SecuredCustomsRegNo = "UsPassport123";

				shipment.ConsigneePK = consignee.PK;
				shipment.ConsignorPK = shipper.PK;
				shipment.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;

				var manager = ObjectFactory.Get<IAirlineMessagingManager>();
				manager.Send(consol, null, out var ex);
				var doc = XDocument.Parse(mockHttpMessageHandlerForTest.ReceivedRequest);

				// BiographicData
				AssertEquals("PPT-US-UsPassport123", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='BiographicData']").Value);
				AssertEquals("PPT-US-UsPassport123", doc.XPathSelectElement($"{ACASinSubShipmentXPath}/*[local-name()='BiographicData']").Value);

				// CustAccBillingType and CustAccEstDate
				AssertEquals("CHQ", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='CustAccBillingType']").Value);
				AssertEquals("12Jun24", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='CustAccEstDate']").Value);

				// CustomerAccountHolder and CustomerAccountName
				AssertEquals("3", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='CustomerAccountHolder']").Value);
				AssertEquals("Controlling Customer Name", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='CustomerAccountName']").Value);

				// CustomerAccountIssuer and CustomerAccountNumber
				AssertEquals("USCCAREGNO", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='CustomerAccountIssuer']").Value);
				AssertEquals("OHCOD", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='CustomerAccountNumber']").Value);

				// IP addresses
				AssertEquals("127.0.0.1", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='IPAddressAccCreation']").Value);
				AssertEquals("127.0.0.1", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='IPAddressRqShpBillCreation']").Value);

				// Shipping frequency
				AssertEquals("O", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='CustAccShippingFrequency']").Value);

				// VerifiedKnownConsignor
				AssertEquals("N", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='VerifiedKnownConsignor']").Value);

				// Email local and domains
				AssertEquals("shipper", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='ShipperEmailLocal']").Value);
				AssertEquals("shipper.com", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='ShipperEmailDomain']").Value);
				AssertEquals("consignee", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='ConsigneeEmailLocal']").Value);
				AssertEquals("consignee.com", doc.XPathSelectElement($"{ACASinShipmentXPath}/*[local-name()='ConsigneeEmailDomain']").Value);
			}
		}

		public void TestXUS_Include_ConsignorCompanyIdAndNumber()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "SGSIN";

			var header = consol.AWBHeader;
			header.EH_ShipperTraderNoCountryCode = "GB";

			header.EH_ShipperTraderNo = "GB123";
			header.EH_ShipperTraderNoType = "EOR";
			Factory.Save();
			AssertCompanyIdAndNumberAddInfo(consol, AirlineConstants.AddInfoCollectionTypes.ConsignorCompanyIdAndNumber, "GB123");

			header.EH_ShipperTraderNo = "111";
			header.EH_ShipperTraderNoType = "PIN";
			Factory.Save();
			AssertCompanyIdAndNumberAddInfo(consol, AirlineConstants.AddInfoCollectionTypes.ConsignorCompanyIdAndNumber, "PIN111");
		}

		public void TestXUS_Include_ConsigneeCompanyIdAndNumber()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "SGSIN";

			var header = consol.AWBHeader;
			header.EH_ConsigneeTraderNoCountryCode = "GB";

			header.EH_ConsigneeTraderNo = "GB123";
			header.EH_ConsigneeTraderNoType = "EOR";
			Factory.Save();
			AssertCompanyIdAndNumberAddInfo(consol, AirlineConstants.AddInfoCollectionTypes.ConsigneeCompanyIdAndNumber, "GB123");

			header.EH_ConsigneeTraderNo = "111";
			header.EH_ConsigneeTraderNoType = "PIN";
			Factory.Save();
			AssertCompanyIdAndNumberAddInfo(consol, AirlineConstants.AddInfoCollectionTypes.ConsigneeCompanyIdAndNumber, "PIN111");
		}

		public void TestXUS_Include_AlsoNotifyCompanyIdAndNumber()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "SGSIN";

			var header = consol.AWBHeader;
			header.EH_AlsoNotifyTraderNoCountryCode = "GB";

			header.EH_AlsoNotifyTraderNo = "GB123";
			header.EH_AlsoNotifyTraderNoType = "EOR";
			Factory.Save();
			AssertCompanyIdAndNumberAddInfo(consol, AirlineConstants.AddInfoCollectionTypes.AlsoNotifyCompanyIdAndNumber, "GB123");

			header.EH_AlsoNotifyTraderNo = "111";
			header.EH_AlsoNotifyTraderNoType = "PIN";
			Factory.Save();
			AssertCompanyIdAndNumberAddInfo(consol, AirlineConstants.AddInfoCollectionTypes.AlsoNotifyCompanyIdAndNumber, "PIN111");
		}

		public void TestXUS_Include_ConsignorCompanyIdAndNumber_InSubShipments()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "SGSIN";

			consol.Shipments.AddNew();

			var consolShipments = consol.Shipments.OfType<ForwardingShipment>().Where(shipment => shipment.IsFHLShipment());
			var forwardingShipment = consolShipments.FirstOrDefault();

			var header = forwardingShipment.AWBHeader;
			header.EH_ShipperTraderNoCountryCode = "GB";

			header.EH_ShipperTraderNo = "GB123";
			header.EH_ShipperTraderNoType = "EOR";
			Factory.Save();
			AssertCompanyIdAndNumberAddInfo_ForSubShipments(consol, AirlineConstants.AddInfoCollectionTypes.ConsignorCompanyIdAndNumber, "GB123");

			header.EH_ShipperTraderNo = "111";
			header.EH_ShipperTraderNoType = "PIN";
			Factory.Save();
			AssertCompanyIdAndNumberAddInfo_ForSubShipments(consol, AirlineConstants.AddInfoCollectionTypes.ConsignorCompanyIdAndNumber, "PIN111");
		}

		public void TestXUS_Include_ConsigneeCompanyIdAndNumber_InSubShipments()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "SGSIN";

			consol.Shipments.AddNew();

			var consolShipments = consol.Shipments.OfType<ForwardingShipment>().Where(shipment => shipment.IsFHLShipment());
			var forwardingShipment = consolShipments.FirstOrDefault();

			var header = forwardingShipment.AWBHeader;
			header.EH_ConsigneeTraderNoCountryCode = "GB";

			header.EH_ConsigneeTraderNo = "GB123";
			header.EH_ConsigneeTraderNoType = "EOR";
			Factory.Save();
			AssertCompanyIdAndNumberAddInfo_ForSubShipments(consol, AirlineConstants.AddInfoCollectionTypes.ConsigneeCompanyIdAndNumber, "GB123");

			header.EH_ConsigneeTraderNo = "111";
			header.EH_ConsigneeTraderNoType = "PIN";
			Factory.Save();
			AssertCompanyIdAndNumberAddInfo_ForSubShipments(consol, AirlineConstants.AddInfoCollectionTypes.ConsigneeCompanyIdAndNumber, "PIN111");
		}

		public void TestXUS_Include_AlsoNotifyCompanyIdAndNumber_InSubShipments()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "SGSIN";

			consol.Shipments.AddNew();

			var consolShipments = consol.Shipments.OfType<ForwardingShipment>().Where(shipment => shipment.IsFHLShipment());
			var forwardingShipment = consolShipments.FirstOrDefault();
			var header = forwardingShipment.AWBHeader;
			header.EH_AlsoNotifyTraderNoCountryCode = "GB";

			header.EH_AlsoNotifyTraderNo = "GB123";
			header.EH_AlsoNotifyTraderNoType = "EOR";
			Factory.Save();
			AssertCompanyIdAndNumberAddInfo_ForSubShipments(consol, AirlineConstants.AddInfoCollectionTypes.AlsoNotifyCompanyIdAndNumber, "GB123");

			header.EH_AlsoNotifyTraderNo = "111";
			header.EH_AlsoNotifyTraderNoType = "PIN";
			Factory.Save();
			AssertCompanyIdAndNumberAddInfo_ForSubShipments(consol, AirlineConstants.AddInfoCollectionTypes.AlsoNotifyCompanyIdAndNumber, "PIN111");
		}

		public void TestXUS_Include_CountrySpecificConsigneeTraderCode()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "SGSIN";

			var consolHeader = consol.AWBHeader;
			consolHeader.EH_ConsigneeCountryCode = "KE";
			consolHeader.EH_ConsigneeTraderNoType = "PIN";

			consol.Shipments.AddNew();
			var consolShipments = consol.Shipments.OfType<ForwardingShipment>().Where(shipment => shipment.IsFHLShipment());
			var forwardingShipment = consolShipments.FirstOrDefault();
			var shipmentHeader = forwardingShipment.AWBHeader;
			shipmentHeader.EH_ConsigneeCountryCode = "KE";
			shipmentHeader.EH_ConsigneeTraderNoType = "PIN";

			consolHeader.EH_ConsigneeTraderNo = "23.125.563";
			Factory.Save();
			AssertCompanyIdAndNumberAddInfo(consol, AirlineConstants.AddInfoCollectionTypes.ConsigneeCompanyIdAndNumber, "23.125.563");

			shipmentHeader.EH_ConsigneeTraderNo = "23.125.563";
			Factory.Save();
			AssertCompanyIdAndNumberAddInfo_ForSubShipments(consol, AirlineConstants.AddInfoCollectionTypes.ConsigneeCompanyIdAndNumber, "P23.125.563");
		}

		public void TestXUS_CompanyIdAndNumber_UpdateAndCombineExistingAddInfo()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "SGSIN";

			var header = consol.AWBHeader;
			var addInfoCollection = new KeyValuePair<string, string>[]
			{
				new ("AlsoNotifyCompanyIdAndNumber", "AU123"),
			};

			header.EH_AlsoNotifyTraderNoCountryCode = "GB";
			header.EH_AlsoNotifyTraderNo = "GB123";
			header.EH_AlsoNotifyTraderNoType = "EOR";

			Factory.Save();
			AssertEquals(1, CountAddInfoElementsInShipment(consol, addInfoCollection));

			addInfoCollection = new KeyValuePair<string, string>[]
			{
				new ("AlsoNotifyCompanyIdAndNumber", "GB123")
			};

			header.EH_ConsigneeTraderNoCountryCode = "GB";
			header.EH_ConsigneeTraderNo = "GB123";
			header.EH_ConsigneeTraderNoType = "EOR";

			Factory.Save();
			AssertEquals(2, CountAddInfoElementsInShipment(consol, addInfoCollection));
		}

		void AssertCompanyIdAndNumberAddInfo(ForwardingConsol consol, string key, string value)
		{
			var manager = ObjectFactory.Get<IAirlineMessagingManager>();
			var mockHttpMessageHandlerForTest = new MockHttpMessageHandlerForTest(ResponseISN);
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => mockHttpMessageHandlerForTest)))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				manager.Send(consol, null, out var ex);

				var doc = XDocument.Parse(mockHttpMessageHandlerForTest.ReceivedRequest);
				var node = doc.XPathSelectElement("/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo']");
				AssertEquals(key, node.Element(node.GetDefaultNamespace() + "Key").Value);
				AssertEquals(value, node.Element(node.GetDefaultNamespace() + "Value").Value);
			}
		}

		void AssertCompanyIdAndNumberAddInfo_ForSubShipments(ForwardingConsol consol, string key, string value)
		{
			var manager = ObjectFactory.Get<IAirlineMessagingManager>();
			var mockHttpMessageHandlerForTest = new MockHttpMessageHandlerForTest(ResponseISN);

			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => mockHttpMessageHandlerForTest)))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				manager.Send(consol, null, out var ex);

				var xmlDoc = XDocument.Parse(mockHttpMessageHandlerForTest.ReceivedRequest);
				var subShipmentPath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='SubShipmentCollection']/*[local-name()='SubShipment']";
				var subShipments = xmlDoc.XPathSelectElements(subShipmentPath);

				foreach (var subShipment in subShipments)
				{
					var nameSpace = subShipment.GetDefaultNamespace();
					var addInfo = subShipment
						.Element(nameSpace + "AddInfoCollection")
						.Element(nameSpace + "AddInfo");

					Assert("Missing AddInfo in SubShipments", addInfo != null);

					AssertEquals(key, addInfo.Element(addInfo.GetDefaultNamespace() + "Key").Value);
					AssertEquals(value, addInfo.Element(addInfo.GetDefaultNamespace() + "Value").Value);
				}
			}
		}

		int CountAddInfoElementsInShipment(ForwardingConsol consol, KeyValuePair<string, string>[] addInfoCollection)
		{
			var manager = ObjectFactory.Get<IAirlineMessagingManager>();
			var mockHttpMessageHandlerForTest = new MockHttpMessageHandlerForTest(ResponseISN);

			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => mockHttpMessageHandlerForTest)))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				manager.Send(consol, addInfoCollection, out var ex);

				var xmlDoc = XDocument.Parse(mockHttpMessageHandlerForTest.ReceivedRequest);
				var node = xmlDoc.XPathSelectElement("/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='AddInfoCollection']");

				return node.Descendants().Count(d => d.Name.LocalName == "AddInfo");
			}
		}
	}

		class MockHttpMessageHandlerForTest : HttpMessageHandler
	{
		readonly string responseContent = "";
		public MockHttpMessageHandlerForTest(string responseContent)
		{
			this.responseContent = responseContent;
		}

		public string ReceivedRequest { get; set; }

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			ReceivedRequest = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			var response = new HttpResponseMessage
			{
				Content = new StringContent(responseContent)
			};
			response.StatusCode = HttpStatusCode.OK;

			return await Task.FromResult(response);
		}
	}
}
