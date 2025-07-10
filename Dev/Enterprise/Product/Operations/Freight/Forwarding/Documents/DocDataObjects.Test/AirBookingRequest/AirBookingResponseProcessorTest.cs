using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.Foundation.Http;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class AirBookingResponseProcessorTest : TestCaseWithFactory
	{
		// possible responses

		// top level
		// ISN from WTG
		// IRJ from WTG
		// IRA from Carrier
		// IRJ from Carrier

		// transport level
		// BKC from Carrier
		// BKP from Carrier
		// BKJ from Carrier
		// BKL from Carrier

		// universal shipment with rates

		public void TestDisappearingConsolFetailsAfterABE()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "ANSAUSSYD2";
			carrier.OH_FullName = "ANSETT AUSTRALIA CARGO";
			carrier.OH_RL_NKClosestPort = "AUSYD";
			carrier.MainAddress.Address1 = "Unit 000";
			carrier.MainAddress.Address2 = "Hypocrea astronidii";
			carrier.MainAddress.City = "Mel";
			carrier.MainAddress.Postcode = "2019";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "COEABNE0000691313";
			consol.JK_AgentType = "AGT";
			consol.JK_TransportMode = "AIR";
			consol.JK_ConsolMode = "LSE";
			consol.JK_RL_NKLoadPort = "FRCDG";
			consol.JK_RL_NKDischargePort = "USJFK";
			consol.JK_Phase = "ALL";
			consol.JK_AWBServiceLevel = "STD";
			consol.MasterBillAirlinePrefix = "057";
			consol.MasterBillMAWB = "32323340";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var leg1 = consol.Transports[0];
			leg1.JW_VoyageFlightForBinding = "AF054";
			leg1.JW_RL_NKLoadPortForBinding = "FRCDG";
			leg1.JW_RL_NKDiscPortForBinding = "USBOS";
			leg1.JW_ETDForBinding = new ZDateTime(2021, 1, 1);
			leg1.JW_ETAForBinding = new ZDateTime(2021, 1, 2);
			var leg2 = consol.Transports.AddNew();
			leg2.JW_VoyageFlightForBinding = "AF120L";
			leg2.JW_RL_NKLoadPortForBinding = "USBOS";
			leg2.JW_RL_NKDiscPortForBinding = "USJFK";
			leg2.JW_ETDForBinding = new ZDateTime(2021, 1, 4);
			leg2.JW_ETAForBinding = new ZDateTime(2021, 1, 5);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "LSE";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_RL_NKOrigin = "FRCDG";
			shipment.JS_RL_NKDestination = "USJFK";
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 4;
			packLine.JL_ActualWeight = 420;
			packLine.JL_ActualWeightUQ = "KG";
			packLine.JL_Length = 0.75;
			packLine.JL_Width = 0.6;
			packLine.JL_Height = 0.55;
			packLine.JL_UnitOfDimension = "M";
			packLine.JL_ActualVolume = 420.000;
			packLine.JL_ActualVolumeUQ = "M3";

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_Code = "ABCCLISYD";
			shipper.OH_FullName = "ABC CLIENT";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "200 SMITH STREET SYDNEY";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2000";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "ARKCONFRE";
			consignee.OH_FullName = "Arkas Container Transport S.A.";
			consignee.OH_RL_NKClosestPort = "AUFRE";
			consignee.MainAddress.Address1 = "16 PHILLIMORE STREET";
			consignee.MainAddress.City = "FREMANTLE";
			consignee.MainAddress.Postcode = "6160";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var consol2 = factory2.Load<ForwardingConsol>(consol.PK);
			AssertEquals(2, consol2.Transports.Count);

			var processor = new AirBookingResponseProcessor(Factory);
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var res1 = processor.Process(resourceRetriever.GetString("Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.AirBookingRequest.TestFiles.637578168867036966_4.txt"));
				Factory.Save();
				var res2 = processor.Process(resourceRetriever.GetString("Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.AirBookingRequest.TestFiles.637578169515880183_5.txt"));
				Factory.Save();
				var res3 = processor.Process(resourceRetriever.GetString("Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.AirBookingRequest.TestFiles.637578169815434349_6.txt"));
				Factory.Save();
			}
			AssertEquals(2, consol.Transports.Count);
			AssertEquals(2, consol2.Transports.Count);
			AssertEquals(consol.Transports[0].PK, consol2.Transports[0].PK);
			AssertEquals(consol.Transports[1].PK, consol2.Transports[1].PK);
			AssertEquals(consol.Transports[0].JW_RL_NKLoadPort, consol2.Transports[0].JW_RL_NKLoadPort);
			AssertEquals(consol.Transports[0].JW_RL_NKDiscPort, consol2.Transports[0].JW_RL_NKDiscPort);
			AssertEquals(consol.Transports[1].JW_RL_NKLoadPort, consol2.Transports[1].JW_RL_NKLoadPort);
			AssertEquals(consol.Transports[1].JW_RL_NKDiscPort, consol2.Transports[1].JW_RL_NKDiscPort);
		}

		#region TestProcess_ISN

		public void TestProcess_ISN()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "CCN1406309";

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			Factory.Save();

			var processor = new AirBookingResponseProcessor(Factory);
			var res = processor.Process(eBookingAPIResponse_ISN);

			AssertEquals("main factory wasn't saved during universal xml import",
				1, Factory.SaveCount);

			AssertEquals("received correct response type", ResponseType.InterchangeSent, res.ResponseType);
			AssertEquals("received correct response message",
				"Booking request has been sent to the Airline. The Consol will be updated automatically when a pending response is received from the Airline. In the meantime, please check the Events Log for more information.",
				res.UserMessage);

			AssertEquals("did not import universal shipment", false, res.ImportedUniversalShipment);

			AssertNoExceptionThrown("expected no exceptions during save", Factory.Save);

			AssertContainsExactElementsInAnyOrder("consol logs imported ISN event",
				new[]
				{
					"ISN |DEP=WiseTech Global|MST=Air Booking|RFN=618-73808291",
				},
				GetLogsForTesting(consol, Events.InterchangeSentCode));

			AssertEventDateWasConvertedToLocal(consol, Events.InterchangeSentCode, "2020-04-08T10:25:21");

			AssertContainsExactElementsInAnyOrder("document data logs imported ISN event",
				new[]
				{
					"ISN |DEP=WiseTech Global|MST=Air Booking|RFN=618-73808291"
				},
				GetLogsForTesting(documentData, Events.InterchangeSentCode));

			AssertEventDateWasConvertedToLocal(documentData, Events.InterchangeSentCode, "2020-04-08T10:25:21");
		}

		const string eBookingAPIResponse_ISN = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>Air_Booking_Engine</SenderID>
	</Header>
	<Body>
		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
			<Event>
				<DataContext>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
					<DataTargetCollection>
						<DataTarget>
							<Key>CCN1406309</Key>
							<Type>ForwardingConsol</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventType>ISN</EventType>
				<EventParameters>
					<Department>WiseTech Global</Department>
					<MessageType>Air Booking</MessageType>
					<ReferenceNumber>618-73808291</ReferenceNumber>
				</EventParameters>
				<EventTime>2020-04-08T10:25:21</EventTime>
				<DataContext/>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>618-73808291</Value>
					</Context>
					<Context>
						<Type>MAWBOriginIATAAirportCode</Type>
						<Value>SYD</Value>
					</Context>
					<Context>
						<Type>MAWBDestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>
	</Body>
</UniversalInterchange>";

		#endregion

		#region TestProcess_IRJ

		public void TestProcess_IRJ_ShouldUseContextFailureReasonAsUserMessage_WhenContextHasFailureReason()
		{
			var consol = PopulateConsol(out var documentData);

			var processor = new AirBookingResponseProcessor(Factory);
			var res = processor.Process(eBookingAPIResponse_IRJ_WithContextFailureReason);

			AssertResponseIsValid(res, consol, documentData);

			AssertEquals("received correct response message", @"The selected Product of ""Pharma"" requires the following Special Handling Code (SHC) conditions:
1. At least one mandatory SHC from this list: PER,PEP,PEM,PES,PEF,HEG
2. At least one mandatory SHC from this list: COL,CRT,ERT
3. The following SHC for this Product are not allowed: FRO,AOG,ASH,AVI,AXA,COU,DIP,HUM,PIL,VAL,VEH,VUN,XPS,CAO,ELI,ELM,ICE,MAG,MUW,RCL,RCM,REQ,RFG,RFL,RFS,RFW,RIS,RLI,RLM,RMD,RNG,ROP,ROX,RPB,RPG,RRE,RRW,RRY,RSB,RSC,RXS,SWP", res.UserMessage);
		}

		const string eBookingAPIResponse_IRJ_WithContextFailureReason = @"<?xml version=""1.0"" encoding=""utf-16""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>AIR_BOOKING_ENGINE</SenderID>
		<RecipientID>EDIAUSSYD</RecipientID>
	</Header>
	<Body>
		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
			<Event>
				<DataContext>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
					<DataTargetCollection>
						<DataTarget>
							<Key>CCN1406309</Key>
							<Type>ForwardingConsol</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventType>IRJ</EventType>
				<EventParameters>
					<Department>WiseTech Global</Department>
					<MessageType>Air Booking</MessageType>
					<ReferenceNumber>618-73808291</ReferenceNumber>
					<Reason>[ERROR MESSAGE]</Reason>
				</EventParameters>
				<EventTime>2020-04-08T10:25:21</EventTime>
				<DataContext/>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>618-73808291</Value>
					</Context>
					<Context>
						<Type>MAWBOriginIATAAirportCode</Type>
						<Value>SYD</Value>
					</Context>
					<Context>
						<Type>MAWBDestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
					<Context>
						<Type>NumberOfPieces</Type>
						<Value>1</Value>
					</Context>
					<Context>
						<Type>WeightOfGoods</Type>
						<Value>1400</Value>
					</Context>
					<Context>
						<Type>FailureReason</Type>
						<Value>The selected Product of ""Pharma"" requires the following Special Handling Code (SHC) conditions:
1. At least one mandatory SHC from this list: PER,PEP,PEM,PES,PEF,HEG
2. At least one mandatory SHC from this list: COL,CRT,ERT
3. The following SHC for this Product are not allowed: FRO,AOG,ASH,AVI,AXA,COU,DIP,HUM,PIL,VAL,VEH,VUN,XPS,CAO,ELI,ELM,ICE,MAG,MUW,RCL,RCM,REQ,RFG,RFL,RFS,RFW,RIS,RLI,RLM,RMD,RNG,ROP,ROX,RPB,RPG,RRE,RRW,RRY,RSB,RSC,RXS,SWP</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>
	</Body>
</UniversalInterchange>";

		public void TestProcess_IRJ_ShouldUseReasonEventParameterAsUserMessage_WhenMessageDoesNotHaveContextFailureReason()
		{
			var consol = PopulateConsol(out var documentData);

			var processor = new AirBookingResponseProcessor(Factory);
			var res = processor.Process(eBookingAPIResponse_IRJ);

			AssertResponseIsValid(res, consol, documentData);
			AssertEquals("received correct response message", "[ERROR MESSAGE]", res.UserMessage);
		}

		const string eBookingAPIResponse_IRJ = @"<?xml version=""1.0"" encoding=""utf-16""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>Air_Booking_Engine</SenderID>
	</Header>
	<Body>
		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
			<Event>
				<DataContext>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
					<DataTargetCollection>
						<DataTarget>
							<Key>CCN1406309</Key>
							<Type>ForwardingConsol</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventType>IRJ</EventType>
				<EventParameters>
					<Department>WiseTech Global</Department>
					<MessageType>Air Booking</MessageType>
					<ReferenceNumber>618-73808291</ReferenceNumber>
					<Reason>[ERROR MESSAGE]</Reason>
				</EventParameters>
				<EventTime>2020-04-08T10:25:21</EventTime>
				<DataContext/>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>618-73808291</Value>
					</Context>
					<Context>
						<Type>MAWBOriginIATAAirportCode</Type>
						<Value>SYD</Value>
					</Context>
					<Context>
						<Type>MAWBDestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
					<Context>
						<Type>NumberOfPieces</Type>
						<Value>1</Value>
					</Context>
					<Context>
						<Type>WeightOfGoods</Type>
						<Value>1400</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>
	</Body>
</UniversalInterchange>";

		public void TestProcess_IRJ_ShouldUseReasonEventParameterAsUserMessage_WhenContextFailureReasonIsEmpty()
		{
			var consol = PopulateConsol(out var documentData);

			var processor = new AirBookingResponseProcessor(Factory);
			var res = processor.Process(eBookingAPIResponse_IRJ_WithEmptyContextFailureReason);

			AssertResponseIsValid(res, consol, documentData);
			AssertEquals("received correct response message", "[ERROR MESSAGE]", res.UserMessage);
		}

		const string eBookingAPIResponse_IRJ_WithEmptyContextFailureReason = @"<?xml version=""1.0"" encoding=""utf-16""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>Air_Booking_Engine</SenderID>
	</Header>
	<Body>
		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
			<Event>
				<DataContext>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
					<DataTargetCollection>
						<DataTarget>
							<Key>CCN1406309</Key>
							<Type>ForwardingConsol</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventType>IRJ</EventType>
				<EventParameters>
					<Department>WiseTech Global</Department>
					<MessageType>Air Booking</MessageType>
					<ReferenceNumber>618-73808291</ReferenceNumber>
					<Reason>[ERROR MESSAGE]</Reason>
				</EventParameters>
				<EventTime>2020-04-08T10:25:21</EventTime>
				<DataContext/>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>618-73808291</Value>
					</Context>
					<Context>
						<Type>MAWBOriginIATAAirportCode</Type>
						<Value>SYD</Value>
					</Context>
					<Context>
						<Type>MAWBDestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
					<Context>
						<Type>NumberOfPieces</Type>
						<Value>1</Value>
					</Context>
					<Context>
						<Type>WeightOfGoods</Type>
						<Value>1400</Value>
					</Context>
					<Context>
						<Type>FailureReason</Type>
						<Value></Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>
	</Body>
</UniversalInterchange>";

		public void TestProcess_IRJ_ShouldResendRequest_WhenContextHasNotificationMessageID()
		{
			var shipment = new UShipment();
			shipment.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);
			AssertNull(shipment.AddInfoCollection);

			var handler = TestHandler.Create(HttpStatusCode.OK, eBookingAPIResponse_IRJ_WithoutNotificationMessageID);
			ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => handler));

			var client = new AirBookingRequestApiClient(new Uri("http://test.com/"));

			var processor = new AirBookingResponseProcessor(Factory);

			var requestProcessor = new AirBookingRequestProcessor(shipment, u => client.Post("test", CancellationToken.None).Content);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			var res = processor.Process(string.Format(eBookingAPIResponse_IRJ_WithNotificationMessageID, "Message1"), requestProcessor);
			AssertNull(shipment.AddInfoCollection);
			AssertEquals("The existing functionality remains unchanged", "[Response with NotificationMessageID: Message1]", res.UserMessage);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			res = processor.Process(string.Format(eBookingAPIResponse_IRJ_WithNotificationMessageID, "Message2"), requestProcessor);
			AssertEquals("[Response without NotificationMessageID]", res.UserMessage);

			var lastAddInfo = shipment.AddInfoCollection.Last();
			AssertEquals(1, shipment.AddInfoCollection.Count);
			AssertEquals("UserAction", lastAddInfo.Key);
			AssertEquals("Message2", lastAddInfo.Value);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			res = processor.Process(string.Format(eBookingAPIResponse_IRJ_WithNotificationMessageID, "Message3"), requestProcessor);
			AssertEquals("[Response without NotificationMessageID]", res.UserMessage);

			lastAddInfo = shipment.AddInfoCollection.Last();
			AssertEquals(2, shipment.AddInfoCollection.Count);
			AssertEquals("UserAction", lastAddInfo.Key);
			AssertEquals("Message3", lastAddInfo.Value);
		}

		const string eBookingAPIResponse_IRJ_WithNotificationMessageID = @"<?xml version=""1.0"" encoding=""utf-16""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>Air_Booking_Engine</SenderID>
	</Header>
	<Body>
		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
			<Event>
				<DataContext>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
					<DataTargetCollection>
						<DataTarget>
							<Key>CCN1406309</Key>
							<Type>ForwardingConsol</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventType>IRJ</EventType>
				<EventParameters>
					<Department>WiseTech Global</Department>
					<MessageType>Air Booking</MessageType>
					<ReferenceNumber>618-73808291</ReferenceNumber>
					<Reason>[Response with NotificationMessageID: {0}]</Reason>
				</EventParameters>
				<EventTime>2020-04-08T10:25:21</EventTime>
				<DataContext/>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>618-73808291</Value>
					</Context>
					<Context>
						<Type>MAWBOriginIATAAirportCode</Type>
						<Value>SYD</Value>
					</Context>
					<Context>
						<Type>MAWBDestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
					<Context>
						<Type>NumberOfPieces</Type>
						<Value>1</Value>
					</Context>
					<Context>
						<Type>WeightOfGoods</Type>
						<Value>1400</Value>
					</Context>
					<Context>
						<Type>FailureReason</Type>
						<Value></Value>
					</Context>
					<Context>
						<Type>NotificationType</Type>
						<Value>Warning</Value>
					</Context>
					<Context>
						<Type>NotificationText</Type>
						<Value>NotificationText Test Value</Value>
					</Context>
					<Context>
						<Type>NotificationMessageID</Type>
						<Value>{0}</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>
	</Body>
</UniversalInterchange>";
		const string eBookingAPIResponse_IRJ_WithoutNotificationMessageID = @"<?xml version=""1.0"" encoding=""utf-16""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>Air_Booking_Engine</SenderID>
	</Header>
	<Body>
		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
			<Event>
				<DataContext>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
					<DataTargetCollection>
						<DataTarget>
							<Key>CCN1406309</Key>
							<Type>ForwardingConsol</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventType>IRJ</EventType>
				<EventParameters>
					<Department>WiseTech Global</Department>
					<MessageType>Air Booking</MessageType>
					<ReferenceNumber>618-73808291</ReferenceNumber>
					<Reason>[Response without NotificationMessageID]</Reason>
				</EventParameters>
				<EventTime>2020-04-08T10:25:21</EventTime>
				<DataContext/>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>618-73808291</Value>
					</Context>
					<Context>
						<Type>MAWBOriginIATAAirportCode</Type>
						<Value>SYD</Value>
					</Context>
					<Context>
						<Type>MAWBDestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
					<Context>
						<Type>NumberOfPieces</Type>
						<Value>1</Value>
					</Context>
					<Context>
						<Type>WeightOfGoods</Type>
						<Value>1400</Value>
					</Context>
					<Context>
						<Type>FailureReason</Type>
						<Value></Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>
	</Body>
</UniversalInterchange>";

		ForwardingConsol PopulateConsol(out VisualizerDocumentData documentData)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "CCN1406309";

			documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			Factory.Save();
			return consol;
		}

		void AssertResponseIsValid(ResponseProcessResult res, ForwardingConsol consol, VisualizerDocumentData documentData)
		{
			AssertEquals("main factory wasn't saved during universal xml import",
				1, Factory.SaveCount);

			AssertEquals("received correct response type", ResponseType.InterchangeRejected, res.ResponseType);
			AssertEquals("did not import universal shipment", false, res.ImportedUniversalShipment);

			AssertNoExceptionThrown("expected no exceptions during save", Factory.Save);

			AssertContainsExactElementsInAnyOrder("consol logs imported IRJ event",
				new[]
				{
					"IRJ |DEP=WiseTech Global|MST=Air Booking|RES=[ERROR MESSAGE]|RFN=618-73808291",
				},
				GetLogsForTesting(consol, Events.InterchangeRejectedCode));

			AssertEventDateWasConvertedToLocal(consol, Events.InterchangeRejectedCode, "2020-04-08T10:25:21");

			AssertContainsExactElementsInAnyOrder("document data logs imported IRJ event",
				new[]
				{
					"IRJ |DEP=WiseTech Global|MST=Air Booking|RES=[ERROR MESSAGE]|RFN=618-73808291",
				},
				GetLogsForTesting(documentData, Events.InterchangeRejectedCode));

			AssertEventDateWasConvertedToLocal(documentData, Events.InterchangeRejectedCode, "2020-04-08T10:25:21");
		}

		#endregion

		#region TestProcess_IRA

		public void TestProcess_IRA()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "CCN1406309";

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			Factory.Save();

			var processor = new AirBookingResponseProcessor(Factory);
			var res = processor.Process(eBookingAPIResponse_IRA);

			AssertEquals("main factory wasn't saved during universal xml import",
				1, Factory.SaveCount);

			AssertEquals("received correct response type", ResponseType.InterchangeAccepted, res.ResponseType);
			AssertEquals("received correct response message",
				"Booking request has been sent to the Airline. The Consol will be updated automatically when a pending response is received from the Airline. In the meantime, please check the Events Log for more information.",
				res.UserMessage);

			AssertEquals("did not import universal shipment", false, res.ImportedUniversalShipment);

			AssertNoExceptionThrown("expected no exceptions during save", Factory.Save);

			AssertContainsExactElementsInAnyOrder("consol logs imported IRA event",
				new[]
				{
					"IRA |DEP=Carrier|MST=Air Booking|RFN=618-73808291",
				},
				GetLogsForTesting(consol, Events.InterchangeReceiptAcknowledgedCode));

			AssertEventDateWasConvertedToLocal(consol, Events.InterchangeReceiptAcknowledgedCode, "2020-04-08T10:25:21");

			AssertContainsExactElementsInAnyOrder("document data logs imported IRJ event",
				new[]
				{
					"IRA |DEP=Carrier|MST=Air Booking|RFN=618-73808291",
				},
				GetLogsForTesting(documentData, Events.InterchangeReceiptAcknowledgedCode));

			AssertEventDateWasConvertedToLocal(documentData, Events.InterchangeReceiptAcknowledgedCode, "2020-04-08T10:25:21");
		}

		const string eBookingAPIResponse_IRA = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>Air_Booking_Engine</SenderID>
	</Header>
	<Body>
		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
			<Event>
				<DataContext>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
					<DataTargetCollection>
						<DataTarget>
							<Key>CCN1406309</Key>
							<Type>ForwardingConsol</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventType>IRA</EventType>
				<EventParameters>
					<Department>Carrier</Department>
					<MessageType>Air Booking</MessageType>
					<ReferenceNumber>618-73808291</ReferenceNumber>
				</EventParameters>
				<EventTime>2020-04-08T10:25:21</EventTime>
				<DataContext/>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>618-73808291</Value>
					</Context>
					<Context>
						<Type>MAWBOriginIATAAirportCode</Type>
						<Value>SYD</Value>
					</Context>
					<Context>
						<Type>MAWBDestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>
	</Body>
</UniversalInterchange>";

		#endregion

		#region TestProcess_BKC_UpdateExistingLegs

		public void TestProcess_BKC_UpdateExistingLegs()
		{
			var loadPorts = new List<string>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "CCN1406309";

			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKLoadPort = "SGSIN";

			var transport1 = consol.Transports[0];
			var firstLoadPort = "DEHAM";
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_Status = "PLN";
			transport1.JW_RL_NKLoadPort = firstLoadPort;
			transport1.JW_RL_NKDiscPort = "HKHKG";
			loadPorts.Add(firstLoadPort);

			var transport2 = consol.Transports.AddNew();
			var secondLoadPort = "HKHKG";
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_Status = "PLN";
			transport2.JW_RL_NKLoadPort = secondLoadPort;
			transport2.JW_RL_NKDiscPort = "SGSIN";
			loadPorts.Add(secondLoadPort);

			AssertEquals("prerequisite: consol has 2 transports", 2, consol.Transports.Count);

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			var preImportTransportLegsPKs = consol
				.Transports
				.Select(t => t.PK)
				.ToArray();

			Factory.Save();

			var processor = new AirBookingResponseProcessor(Factory);
			var res = processor.Process(eBookingAPIResponse_BKC);

			AssertEquals("main factory wasn't saved during universal xml import",
				1, Factory.SaveCount);

			AssertEquals("received correct response type", ResponseType.BookingConfirmed, res.ResponseType);
			AssertEquals("received correct response message", @"Airline Booking Response:
HAM-HKG EY7/09May - Confirmed
HKG-SIN EY8/10May - Confirmed

The Consol will be updated automatically if any additional response(s) is received from the Airline at a later time. In the meantime, please check the Events Log for more information.", res.UserMessage);
			AssertEquals("did import universal shipment", true, res.ImportedUniversalShipment);

			AssertNoExceptionThrown("expected no exceptions during save", Factory.Save);

			AssertContainsExactElementsInAnyOrder("consol logs imported BKC events",
				new[]
				{
					"BKC |DEP=Carrier|FDT=09-May-20 00:00|FRM=DEHAM|LOC=DEHAM|MST=Air Booking|TO=HKHKG|VFL=EY7",
					"BKC Propagated: All Document Data|DEP=Carrier|FDT=2020-05-09|LOC=DEHAM|MST=Air Booking|VFL=EY7",
					"BKC |DEP=Carrier|FDT=10-May-20 00:00|FRM=HKHKG|LOC=HKHKG|MST=Air Booking|TO=SGSIN|VFL=EY8",
					"BKC Propagated: All Document Data|DEP=Carrier|FDT=2020-05-10|LOC=HKHKG|MST=Air Booking|VFL=EY8"
				},
				GetLogsForTesting(consol, Events.BookingConfirmedCode));

			AssertEventDateWasConvertedToLocal(consol, Events.BookingConfirmedCode, "2020-05-05T09:46:00", loadPorts);

			AssertContainsExactElementsInAnyOrder("document data logs imported BKC events",
				new[]
				{
					"BKC |DEP=Carrier|FDT=2020-05-09|LOC=DEHAM|MST=Air Booking|VFL=EY7",
					"BKC |DEP=Carrier|FDT=2020-05-10|LOC=HKHKG|MST=Air Booking|VFL=EY8"
				},
				GetLogsForTesting(documentData, Events.BookingConfirmedCode));

			AssertEventDateWasConvertedToLocal(documentData, Events.BookingConfirmedCode, "2020-05-05T09:46:00", loadPorts);

			AssertEquals("prerequisite: consol has 2 transports", 2, consol.Transports.Count);

			var postImportTransport1 = consol.Transports[0];
			AssertEquals("Leg 1 - JW_TransportMode", Core.Constants.TransportModes.Air, postImportTransport1.JW_TransportMode);
			AssertEquals("Leg 1 - JW_Status", "CNF", postImportTransport1.JW_Status);
			AssertEquals("Leg 1 - JW_RL_NKLoadPort", "DEHAM", postImportTransport1.JW_RL_NKLoadPort);
			AssertEquals("Leg 1 - JW_RL_NKDiscPort", "HKHKG", postImportTransport1.JW_RL_NKDiscPort);

			AssertEventDateWasConvertedToLocal(postImportTransport1, Events.BookingConfirmedCode, "2020-05-05T09:46:00", firstLoadPort);

			var postImportTransport2 = consol.Transports[1];
			AssertEquals("Leg 2 - JW_TransportMode", Core.Constants.TransportModes.Air, postImportTransport2.JW_TransportMode);
			AssertEquals("Leg 2 - JW_Status", "CNF", postImportTransport2.JW_Status);
			AssertEquals("Leg 2 - JW_RL_NKLoadPort", "HKHKG", postImportTransport2.JW_RL_NKLoadPort);
			AssertEquals("Leg 2 - JW_RL_NKDiscPort", "SGSIN", postImportTransport2.JW_RL_NKDiscPort);

			AssertEventDateWasConvertedToLocal(postImportTransport2, Events.BookingConfirmedCode, "2020-05-05T09:46:00", secondLoadPort);

			AssertContainsExactElementsInAnyOrder("new transport legs were added/removed",
				preImportTransportLegsPKs,
				new[]
				{
					postImportTransport1.PK,
					postImportTransport2.PK
				});

			AssertAllMessagesHaveDataImportLogNote();
		}

		const string eBookingAPIResponse_BKC = @"<?xml version=""1.0"" encoding=""utf-16""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>Air_Booking_Engine</SenderID>
	</Header>
	<Body>

		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
			<Event>
				<DataContext>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
					<DataTargetCollection>
						<DataTarget>
							<Type>ForwardingConsol</Type>
							<Key>CCN1406309</Key>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventTime>2020-05-05T09:46:00</EventTime>
				<EventType>BKC</EventType>
				<EventParameters>
					<Department>Carrier</Department>
					<Location>HAM</Location>
					<VoyageFlightNumber>EY7</VoyageFlightNumber>
					<FlightDate>2020-05-09</FlightDate>
					<MessageType>Air Booking</MessageType>
				</EventParameters>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>607-89506712</Value>
					</Context>
					<Context>
						<Type>MAWBOriginIATAAirportCode</Type>
						<Value>HAM</Value>
					</Context>
					<Context>
						<Type>MAWBDestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
					<Context>
						<Type>OriginIATAAirportCode</Type>
						<Value>HAM</Value>
					</Context>
					<Context>
						<Type>DestinationIATAAirportCode</Type>
						<Value>HKG</Value>
					</Context>
					<Context>
						<Type>FlightNumber</Type>
						<Value>EY7</Value>
					</Context>
					<Context>
						<Type>FlightDate</Type>
						<Value>2020-05-09</Value>
					</Context>
					<Context>
						<Type>BookingStatus</Type>
						<Value>CNF</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>

		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
			<Event>
				<DataContext>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
					<DataTargetCollection>
						<DataTarget>
							<Type>ForwardingConsol</Type>
							<Key>CCN1406309</Key>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventTime>2020-05-05T09:46:00</EventTime>
				<EventType>BKC</EventType>
				<EventParameters>
					<Department>Carrier</Department>
					<Location>HKG</Location>
					<VoyageFlightNumber>EY8</VoyageFlightNumber>
					<FlightDate>2020-05-10</FlightDate>
					<MessageType>Air Booking</MessageType>
				</EventParameters>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>607-89506712</Value>
					</Context>
					<Context>
						<Type>MAWBOriginIATAAirportCode</Type>
						<Value>HAM</Value>
					</Context>
					<Context>
						<Type>MAWBDestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
					<Context>
						<Type>OriginIATAAirportCode</Type>
						<Value>HKG</Value>
					</Context>
					<Context>
						<Type>DestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
					<Context>
						<Type>FlightNumber</Type>
						<Value>EY8</Value>
					</Context>
					<Context>
						<Type>FlightDate</Type>
						<Value>2020-05-10</Value>
					</Context>
					<Context>
						<Type>BookingStatus</Type>
						<Value>CNF</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>

		<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
			<Shipment>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Key>CCN1406309</Key>
							<Type>ForwardingConsol</Type>
						</DataTarget>
					</DataTargetCollection>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
				</DataContext>

				<BookingConfirmationReference>XXXX</BookingConfirmationReference>
				<PortOfOrigin>HAM</PortOfOrigin>
				<PortOfDestination>HKG</PortOfDestination>
				<WayBillNumber>607-89506712</WayBillNumber>
				<TotalNoOfPacks>5</TotalNoOfPacks>
				<TotalWeight>749.6</TotalWeight>
				<TotalWeightUnit>KG</TotalWeightUnit>
				<TotalVolume>60.03</TotalVolume>
				<TotalVolumeUnit>M3</TotalVolumeUnit>

				<TransportLegCollection>
					<TransportLeg>
						<LegOrder>1</LegOrder>
						<TransportMode>Air</TransportMode>
						<PortOfLoading>HAM</PortOfLoading>
						<PortOfDischarge>HKG</PortOfDischarge>
						<EstimatedArrival>2020-05-09</EstimatedArrival>
						<EstimatedDeparture>2020-05-09</EstimatedDeparture>
						<LegType>Flight1</LegType>
						<TransportMode>Air</TransportMode>
						<VoyageFlightNo>EY7</VoyageFlightNo>
						<BookingStatus>CNF</BookingStatus>
					</TransportLeg>
					<TransportLeg>
						<LegOrder>2</LegOrder>
						<TransportMode>Air</TransportMode>
						<PortOfLoading>HKG</PortOfLoading>
						<PortOfDischarge>SIN</PortOfDischarge>
						<EstimatedArrival>2020-05-10</EstimatedArrival>
						<EstimatedDeparture>2020-05-10</EstimatedDeparture>
						<LegType>Flight2</LegType>
						<TransportMode>Air</TransportMode>
						<VoyageFlightNo>EY8</VoyageFlightNo>
						<BookingStatus>CNF</BookingStatus>
					</TransportLeg>
				</TransportLegCollection>

				<NoteCollection>
					<Note>
						<Description>CarrierResponse</Description>
						<NoteText>Flight XXX has been changed</NoteText>
						<IsCustomDescription>false</IsCustomDescription>
					</Note>
				</NoteCollection>

			</Shipment>
		</UniversalShipment>
	</Body>
</UniversalInterchange>";

		#endregion

		#region TestProcess_BKC_CreateNewLegs

		public void TestProcess_BKC_CreateNewLegs()
		{
			var loadPorts = new List<string>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "CCN1406309";

			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKLoadPort = "SGSIN";

			var transport1 = consol.Transports[0];
			var firstLoadingPort = "DEHAM";
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_Status = "PLN";
			transport1.JW_RL_NKLoadPort = firstLoadingPort;
			transport1.JW_RL_NKDiscPort = "HKHKG";
			loadPorts.Add(firstLoadingPort);

			var transport2 = consol.Transports.AddNew();
			var secondLoadingPort = "HKHKG";
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_Status = "PLN";
			transport2.JW_RL_NKLoadPort = secondLoadingPort;
			transport2.JW_RL_NKDiscPort = "SGSIN";
			loadPorts.Add(secondLoadingPort);

			AssertEquals("prerequisite: consol has 2 transports", 2, consol.Transports.Count);

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			var preImportTransportLegsPKs = consol
				.Transports
				.Select(t => t.PK)
				.ToArray();

			Factory.Save();

			var processor = new AirBookingResponseProcessor(Factory);
			var res = processor.Process(eBookingAPIResponse_BKC_CreateNewLegs);

			AssertEquals("main factory wasn't saved during universal xml import",
				1, Factory.SaveCount);

			AssertEquals("received correct response type", ResponseType.BookingConfirmed, res.ResponseType);
			AssertEquals("received correct response message", @"Airline Booking Response:
HAM-TPE EY7/09May - Confirmed
TPE-SIN EY8/10May - Confirmed

The Consol will be updated automatically if any additional response(s) is received from the Airline at a later time. In the meantime, please check the Events Log for more information.", res.UserMessage);
			AssertEquals("did import universal shipment", true, res.ImportedUniversalShipment);

			AssertNoExceptionThrown("expected no exceptions during save", Factory.Save);

			AssertContainsExactElementsInAnyOrder("consol logs imported BKC event",
				new[]
				{
					"BKC |DEP=Carrier|FDT=09-May-20 00:00|FRM=DEHAM|LOC=DEHAM|MST=Air Booking|TO=TWTPE|VFL=EY7",
					"BKC Propagated: All Document Data|DEP=Carrier|FDT=2020-05-09|LOC=DEHAM|MST=Air Booking|VFL=EY7",
					"BKC |DEP=Carrier|FDT=10-May-20 00:00|FRM=TWTPE|LOC=HKHKG|MST=Air Booking|TO=SGSIN|VFL=EY8",
					"BKC Propagated: All Document Data|DEP=Carrier|FDT=2020-05-10|LOC=HKHKG|MST=Air Booking|VFL=EY8"
				},
				GetLogsForTesting(consol, Events.BookingConfirmedCode));

			AssertEventDateWasConvertedToLocal(consol, Events.BookingConfirmedCode, "2020-05-05T09:46:00", loadPorts);

			AssertContainsExactElementsInAnyOrder("document data logs imported BKC event",
				new[]
				{
					"BKC |DEP=Carrier|FDT=2020-05-09|LOC=DEHAM|MST=Air Booking|VFL=EY7",
					"BKC |DEP=Carrier|FDT=2020-05-10|LOC=HKHKG|MST=Air Booking|VFL=EY8"
				},
				GetLogsForTesting(documentData, Events.BookingConfirmedCode));

			AssertEventDateWasConvertedToLocal(documentData, Events.BookingConfirmedCode, "2020-05-05T09:46:00", loadPorts);

			AssertEquals("prerequisite: consol has 2 transports", 2, consol.Transports.Count);

			var postImportTransport1 = consol.Transports[0];
			AssertEquals("Leg 1 - JW_TransportMode", Core.Constants.TransportModes.Air, postImportTransport1.JW_TransportMode);
			AssertEquals("Leg 1 - JW_Status", "CNF", postImportTransport1.JW_Status);
			AssertEquals("Leg 1 - JW_RL_NKLoadPort", "DEHAM", postImportTransport1.JW_RL_NKLoadPort);
			AssertEquals("Leg 1 - JW_RL_NKDiscPort", "TWTPE", postImportTransport1.JW_RL_NKDiscPort);

			AssertEventDateWasConvertedToLocal(postImportTransport1, Events.BookingConfirmedCode, "2020-05-05T09:46:00", firstLoadingPort);

			var postImportTransport2 = consol.Transports[1];
			AssertEquals("Leg 2 - JW_TransportMode", Core.Constants.TransportModes.Air, postImportTransport2.JW_TransportMode);
			AssertEquals("Leg 2 - JW_Status", "CNF", postImportTransport2.JW_Status);
			AssertEquals("Leg 2 - JW_RL_NKLoadPort", "TWTPE", postImportTransport2.JW_RL_NKLoadPort);
			AssertEquals("Leg 2 - JW_RL_NKDiscPort", "SGSIN", postImportTransport2.JW_RL_NKDiscPort);

			AssertEventDateWasConvertedToLocal(postImportTransport2, Events.BookingConfirmedCode, "2020-05-05T09:46:00", secondLoadingPort);

			Assert("all transport legs were recreated",
				!new[]
				{
					postImportTransport1.PK,
					postImportTransport2.PK
				}
				.Intersect(preImportTransportLegsPKs).Any());

			AssertAllMessagesHaveDataImportLogNote();
		}

		const string eBookingAPIResponse_BKC_CreateNewLegs = @"<?xml version=""1.0"" encoding=""utf-16""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>Air_Booking_Engine</SenderID>
	</Header>
	<Body>

		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
			<Event>
				<DataContext>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
					<DataTargetCollection>
						<DataTarget>
							<Type>ForwardingConsol</Type>
							<Key>CCN1406309</Key>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventTime>2020-05-05T09:46:00</EventTime>
				<EventType>BKC</EventType>
				<EventParameters>
					<Department>Carrier</Department>
					<Location>HAM</Location>
					<VoyageFlightNumber>EY7</VoyageFlightNumber>
					<FlightDate>2020-05-09</FlightDate>
					<MessageType>Air Booking</MessageType>
				</EventParameters>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>607-89506712</Value>
					</Context>
					<Context>
						<Type>MAWBOriginIATAAirportCode</Type>
						<Value>HAM</Value>
					</Context>
					<Context>
						<Type>MAWBDestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
					<Context>
						<Type>OriginIATAAirportCode</Type>
						<Value>HAM</Value>
					</Context>
					<Context>
						<Type>DestinationIATAAirportCode</Type>
						<Value>TPE</Value>
					</Context>
					<Context>
						<Type>FlightNumber</Type>
						<Value>EY7</Value>
					</Context>
					<Context>
						<Type>FlightDate</Type>
						<Value>2020-05-09</Value>
					</Context>
					<Context>
						<Type>BookingStatus</Type>
						<Value>CNF</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>

		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
			<Event>
				<DataContext>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
					<DataTargetCollection>
						<DataTarget>
							<Type>ForwardingConsol</Type>
							<Key>CCN1406309</Key>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventTime>2020-05-05T09:46:00</EventTime>
				<EventType>BKC</EventType>
				<EventParameters>
					<Department>Carrier</Department>
					<Location>HKG</Location>
					<VoyageFlightNumber>EY8</VoyageFlightNumber>
					<FlightDate>2020-05-10</FlightDate>
					<MessageType>Air Booking</MessageType>
				</EventParameters>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>607-89506712</Value>
					</Context>
					<Context>
						<Type>MAWBOriginIATAAirportCode</Type>
						<Value>HAM</Value>
					</Context>
					<Context>
						<Type>MAWBDestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
					<Context>
						<Type>OriginIATAAirportCode</Type>
						<Value>TPE</Value>
					</Context>
					<Context>
						<Type>DestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
					<Context>
						<Type>FlightNumber</Type>
						<Value>EY8</Value>
					</Context>
					<Context>
						<Type>FlightDate</Type>
						<Value>2020-05-10</Value>
					</Context>
					<Context>
						<Type>BookingStatus</Type>
						<Value>CNF</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>

		<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
			<Shipment>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Key>CCN1406309</Key>
							<Type>ForwardingConsol</Type>
						</DataTarget>
					</DataTargetCollection>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
				</DataContext>

				<BookingConfirmationReference>XXXX</BookingConfirmationReference>
				<PortOfOrigin>HAM</PortOfOrigin>
				<PortOfDestination>SIN</PortOfDestination>
				<WayBillNumber>607-89506712</WayBillNumber>
				<TotalNoOfPacks>5</TotalNoOfPacks>
				<TotalWeight>749.6</TotalWeight>
				<TotalWeightUnit>KG</TotalWeightUnit>
				<TotalVolume>60.03</TotalVolume>
				<TotalVolumeUnit>M3</TotalVolumeUnit>

				<TransportLegCollection>
					<TransportLeg>
						<LegOrder>1</LegOrder>
						<TransportMode>Air</TransportMode>
						<PortOfLoading>HAM</PortOfLoading>
						<PortOfDischarge>TPE</PortOfDischarge>
						<EstimatedArrival>2020-05-09</EstimatedArrival>
						<EstimatedDeparture>2020-05-09</EstimatedDeparture>
						<LegType>Flight1</LegType>
						<TransportMode>Air</TransportMode>
						<VoyageFlightNo>EY7</VoyageFlightNo>
						<BookingStatus>CNF</BookingStatus>
					</TransportLeg>
					<TransportLeg>
						<LegOrder>2</LegOrder>
						<TransportMode>Air</TransportMode>
						<PortOfLoading>TPE</PortOfLoading>
						<PortOfDischarge>SIN</PortOfDischarge>
						<EstimatedArrival>2020-05-10</EstimatedArrival>
						<EstimatedDeparture>2020-05-10</EstimatedDeparture>
						<LegType>Flight2</LegType>
						<TransportMode>Air</TransportMode>
						<VoyageFlightNo>EY8</VoyageFlightNo>
						<BookingStatus>CNF</BookingStatus>
					</TransportLeg>
				</TransportLegCollection>

				<NoteCollection>
					<Note>
						<Description>CarrierResponse</Description>
						<NoteText>Flight XXX has been changed</NoteText>
					</Note>
				</NoteCollection>

			</Shipment>
		</UniversalShipment>
	</Body>
</UniversalInterchange>";

		#endregion

		#region TestProcess_ShouldReturnRejectionMessageAsResponseuserMessage_WhenAtLeastOneLegHasRejected

		public void TestProcess_ShouldReturnRejectionMessageAsResponseuserMessage_WhenAtLeastOneLegHasRejected()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "CCN1406309";

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKLoadPort = "GBLHR";

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_Status = "PLN";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AEDXB";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_Status = "PLN";
			transport2.JW_RL_NKLoadPort = "AEDXB";
			transport2.JW_RL_NKDiscPort = "GBLHR";

			AssertEquals("prerequisite: consol has 2 transports", 2, consol.Transports.Count);

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			var preImportTransportLegsPKs = consol
				.Transports
				.Select(t => t.PK)
				.ToArray();

			Factory.Save();

			var processor = new AirBookingResponseProcessor(Factory);
			var res = processor.Process(eBookingAPIResponse_WithDifferentStatusForEachLegOneHasRejected);

			AssertEquals("main factory wasn't saved during universal xml import",
				1, Factory.SaveCount);

			AssertEquals("received correct response type", ResponseType.BookingConfirmed, res.ResponseType);
			AssertEquals("received correct response message", "Booking request has been rejected. Please check the Events Log for more information.", res.UserMessage);
		}

		const string eBookingAPIResponse_WithDifferentStatusForEachLegOneHasRejected = @"<?xml version=""1.0"" encoding=""utf-16""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>AIR_BOOKING_ENGINE</SenderID>
    <RecipientID>WTLEDIMM8</RecipientID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Shipment>
        <DataContext>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
            <SubmissionVersion>29</SubmissionVersion>
          </DocumentaryOverride>
          <DataTargetCollection>
            <DataTarget>
              <Key>CCN1406309</Key>
              <Type>ForwardingConsol</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <WayBillNumber>176-62353561</WayBillNumber>
        <PortOfOrigin>SYD</PortOfOrigin>
        <PortOfDestination>LHR</PortOfDestination>
        <TotalNoOfPacks>2</TotalNoOfPacks>
        <TotalWeight>10</TotalWeight>
        <TotalWeightUnit>KG</TotalWeightUnit>
        <TotalVolume>2</TotalVolume>
        <TotalVolumeUnit>M3</TotalVolumeUnit>
        <BookingConfirmationReference>46434952</BookingConfirmationReference>
        <TransportLegCollection>
          <TransportLeg>
            <LegOrder>1</LegOrder>
            <PortOfLoading>SYD</PortOfLoading>
            <PortOfDischarge>DXB</PortOfDischarge>
            <EstimatedDeparture>2021-08-21T21:10:00</EstimatedDeparture>
            <EstimatedArrival>2021-08-22T05:20:00</EstimatedArrival>
            <VoyageFlightNo>EK0415</VoyageFlightNo>
            <BookingStatus>CNF</BookingStatus>
            <TransportMode>AIR</TransportMode>
            <LegType>Flight1</LegType>
          </TransportLeg>
          <TransportLeg>
            <LegOrder>2</LegOrder>
            <PortOfLoading>DXB</PortOfLoading>
            <PortOfDischarge>LHR</PortOfDischarge>
            <EstimatedDeparture>2021-08-22T07:45:00</EstimatedDeparture>
            <EstimatedArrival>2021-08-22T12:25:00</EstimatedArrival>
            <VoyageFlightNo>EK0001</VoyageFlightNo>
            <BookingStatus>FNO</BookingStatus>
            <TransportMode>AIR</TransportMode>
            <LegType>Flight2</LegType>
          </TransportLeg>
        </TransportLegCollection>
      </Shipment>
    </UniversalShipment>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
            <SubmissionVersion>29</SubmissionVersion>
          </DocumentaryOverride>
          <DataTargetCollection>
            <DataTarget>
              <Key>C00001274</Key>
              <Type>ForwardingConsol</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2021-08-19T03:29:22</EventTime>
        <EventType>BKC</EventType>
        <EventReference>Confirmed</EventReference>
        <EventParameters>
          <Department>Carrier</Department>
          <Location>SYD</Location>
          <Type>AWB</Type>
          <VoyageFlightNumber>EK0415</VoyageFlightNumber>
          <FlightDate>2021-08-21</FlightDate>
          <MessageType>Air Booking</MessageType>
          <ReferenceNumber>176-62353561</ReferenceNumber>
        </EventParameters>
        <ContextCollection>
          <Context>
            <Type>MAWBNumber</Type>
            <Value>176-62353561</Value>
          </Context>
          <Context>
            <Type>MAWBOriginIATAAirportCode</Type>
            <Value>SYD</Value>
          </Context>
          <Context>
            <Type>MAWBDestinationIATAAirportCode</Type>
            <Value>LHR</Value>
          </Context>
          <Context>
            <Type>OriginIATAAirportCode</Type>
            <Value>SYD</Value>
          </Context>
          <Context>
            <Type>DestinationIATAAirportCode</Type>
            <Value>DXB</Value>
          </Context>
          <Context>
            <Type>FlightNumber</Type>
            <Value>EK0415</Value>
          </Context>
          <Context>
            <Type>FlightDate</Type>
            <Value>2021-08-21</Value>
          </Context>
          <Context>
            <Type>NumberOfPieces</Type>
            <Value>2</Value>
          </Context>
          <Context>
            <Type>WeightOfGoods</Type>
            <Value>10KG</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
            <SubmissionVersion>29</SubmissionVersion>
          </DocumentaryOverride>
          <DataTargetCollection>
            <DataTarget>
              <Key>CCN1406309</Key>
              <Type>ForwardingConsol</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2021-08-19T03:29:22</EventTime>
        <EventType>BKJ</EventType>
        <EventReference>Rejected</EventReference>
        <EventParameters>
          <Department>Carrier</Department>
          <Location>DXB</Location>
          <Type>AWB</Type>
          <VoyageFlightNumber>EK0001</VoyageFlightNumber>
          <FlightDate>2021-08-22</FlightDate>
          <MessageType>Air Booking</MessageType>
          <ReferenceNumber>176-62353561</ReferenceNumber>
        </EventParameters>
        <ContextCollection>
          <Context>
            <Type>MAWBNumber</Type>
            <Value>176-62353561</Value>
          </Context>
          <Context>
            <Type>MAWBOriginIATAAirportCode</Type>
            <Value>SYD</Value>
          </Context>
          <Context>
            <Type>MAWBDestinationIATAAirportCode</Type>
            <Value>LHR</Value>
          </Context>
          <Context>
            <Type>OriginIATAAirportCode</Type>
            <Value>DXB</Value>
          </Context>
          <Context>
            <Type>DestinationIATAAirportCode</Type>
            <Value>LHR</Value>
          </Context>
          <Context>
            <Type>FlightNumber</Type>
            <Value>EK0001</Value>
          </Context>
          <Context>
            <Type>FlightDate</Type>
            <Value>2021-08-22</Value>
          </Context>
          <Context>
            <Type>NumberOfPieces</Type>
            <Value>2</Value>
          </Context>
          <Context>
            <Type>WeightOfGoods</Type>
            <Value>10KG</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>";

		#endregion

		#region public void TestProcess_ShouldReturnRejectionMessageAsResponseuserMessage_WhenAtLeastOneLegHasCancelled()

		public void TestProcess_ShouldReturnRejectionMessageAsResponseuserMessage_WhenAtLeastOneLegHasCancelled()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "CCN1406309";

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKLoadPort = "GBLHR";

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_Status = "PLN";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AEDXB";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_Status = "PLN";
			transport2.JW_RL_NKLoadPort = "AEDXB";
			transport2.JW_RL_NKDiscPort = "GBLHR";

			AssertEquals("prerequisite: consol has 2 transports", 2, consol.Transports.Count);

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			var preImportTransportLegsPKs = consol
				.Transports
				.Select(t => t.PK)
				.ToArray();

			Factory.Save();

			var processor = new AirBookingResponseProcessor(Factory);
			var res = processor.Process(eBookingAPIResponse_WithDifferentStatusForEachLegOneHasCancelled);

			AssertEquals("main factory wasn't saved during universal xml import",
				1, Factory.SaveCount);

			AssertEquals("received correct response type", ResponseType.BookingConfirmed, res.ResponseType);
			AssertEquals("received correct response message", "Booking has been canceled with the Airline. Please check the Events Log for more information.", res.UserMessage);
		}

		const string eBookingAPIResponse_WithDifferentStatusForEachLegOneHasCancelled = @"<?xml version=""1.0"" encoding=""utf-16""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>AIR_BOOKING_ENGINE</SenderID>
    <RecipientID>WTLEDIMM8</RecipientID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Shipment>
        <DataContext>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
            <SubmissionVersion>29</SubmissionVersion>
          </DocumentaryOverride>
          <DataTargetCollection>
            <DataTarget>
              <Key>CCN1406309</Key>
              <Type>ForwardingConsol</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <WayBillNumber>176-62353561</WayBillNumber>
        <PortOfOrigin>SYD</PortOfOrigin>
        <PortOfDestination>LHR</PortOfDestination>
        <TotalNoOfPacks>2</TotalNoOfPacks>
        <TotalWeight>10</TotalWeight>
        <TotalWeightUnit>KG</TotalWeightUnit>
        <TotalVolume>2</TotalVolume>
        <TotalVolumeUnit>M3</TotalVolumeUnit>
        <BookingConfirmationReference>46434952</BookingConfirmationReference>
        <TransportLegCollection>
          <TransportLeg>
            <LegOrder>1</LegOrder>
            <PortOfLoading>SYD</PortOfLoading>
            <PortOfDischarge>DXB</PortOfDischarge>
            <EstimatedDeparture>2021-08-21T21:10:00</EstimatedDeparture>
            <EstimatedArrival>2021-08-22T05:20:00</EstimatedArrival>
            <VoyageFlightNo>EK0415</VoyageFlightNo>
            <BookingStatus>CNF</BookingStatus>
            <TransportMode>AIR</TransportMode>
            <LegType>Flight1</LegType>
          </TransportLeg>
          <TransportLeg>
            <LegOrder>2</LegOrder>
            <PortOfLoading>DXB</PortOfLoading>
            <PortOfDischarge>LHR</PortOfDischarge>
            <EstimatedDeparture>2021-08-22T07:45:00</EstimatedDeparture>
            <EstimatedArrival>2021-08-22T12:25:00</EstimatedArrival>
            <VoyageFlightNo>EK0001</VoyageFlightNo>
            <BookingStatus>CAN</BookingStatus>
            <TransportMode>AIR</TransportMode>
            <LegType>Flight2</LegType>
          </TransportLeg>
        </TransportLegCollection>
      </Shipment>
    </UniversalShipment>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
            <SubmissionVersion>29</SubmissionVersion>
          </DocumentaryOverride>
          <DataTargetCollection>
            <DataTarget>
              <Key>C00001274</Key>
              <Type>ForwardingConsol</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2021-08-19T03:29:22</EventTime>
        <EventType>BKC</EventType>
        <EventReference>Confirmed</EventReference>
        <EventParameters>
          <Department>Carrier</Department>
          <Location>SYD</Location>
          <Type>AWB</Type>
          <VoyageFlightNumber>EK0415</VoyageFlightNumber>
          <FlightDate>2021-08-21</FlightDate>
          <MessageType>Air Booking</MessageType>
          <ReferenceNumber>176-62353561</ReferenceNumber>
        </EventParameters>
        <ContextCollection>
          <Context>
            <Type>MAWBNumber</Type>
            <Value>176-62353561</Value>
          </Context>
          <Context>
            <Type>MAWBOriginIATAAirportCode</Type>
            <Value>SYD</Value>
          </Context>
          <Context>
            <Type>MAWBDestinationIATAAirportCode</Type>
            <Value>LHR</Value>
          </Context>
          <Context>
            <Type>OriginIATAAirportCode</Type>
            <Value>SYD</Value>
          </Context>
          <Context>
            <Type>DestinationIATAAirportCode</Type>
            <Value>DXB</Value>
          </Context>
          <Context>
            <Type>FlightNumber</Type>
            <Value>EK0415</Value>
          </Context>
          <Context>
            <Type>FlightDate</Type>
            <Value>2021-08-21</Value>
          </Context>
          <Context>
            <Type>NumberOfPieces</Type>
            <Value>2</Value>
          </Context>
          <Context>
            <Type>WeightOfGoods</Type>
            <Value>10KG</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
            <SubmissionVersion>29</SubmissionVersion>
          </DocumentaryOverride>
          <DataTargetCollection>
            <DataTarget>
              <Key>CCN1406309</Key>
              <Type>ForwardingConsol</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2021-08-19T03:29:22</EventTime>
        <EventType>BKL</EventType>
        <EventReference>Cancelled</EventReference>
        <EventParameters>
          <Department>Carrier</Department>
          <Location>DXB</Location>
          <Type>AWB</Type>
          <VoyageFlightNumber>EK0001</VoyageFlightNumber>
          <FlightDate>2021-08-22</FlightDate>
          <MessageType>Air Booking</MessageType>
          <ReferenceNumber>176-62353561</ReferenceNumber>
        </EventParameters>
        <ContextCollection>
          <Context>
            <Type>MAWBNumber</Type>
            <Value>176-62353561</Value>
          </Context>
          <Context>
            <Type>MAWBOriginIATAAirportCode</Type>
            <Value>SYD</Value>
          </Context>
          <Context>
            <Type>MAWBDestinationIATAAirportCode</Type>
            <Value>LHR</Value>
          </Context>
          <Context>
            <Type>OriginIATAAirportCode</Type>
            <Value>DXB</Value>
          </Context>
          <Context>
            <Type>DestinationIATAAirportCode</Type>
            <Value>LHR</Value>
          </Context>
          <Context>
            <Type>FlightNumber</Type>
            <Value>EK0001</Value>
          </Context>
          <Context>
            <Type>FlightDate</Type>
            <Value>2021-08-22</Value>
          </Context>
          <Context>
            <Type>NumberOfPieces</Type>
            <Value>2</Value>
          </Context>
          <Context>
            <Type>WeightOfGoods</Type>
            <Value>10KG</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>";

		#endregion

		#region TestProcess_ShouldHaveEachLegStatusInResultMessage_WhenThereIsNotCancellationOrRejection

		public void TestProcess_ShouldHaveEachLegStatusInResultMessage_WhenThereIsNotCancellationOrRejection()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "CCN1406309";

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKLoadPort = "GBLHR";

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_Status = "PLN";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AEDXB";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_Status = "PLN";
			transport2.JW_RL_NKLoadPort = "AEDXB";
			transport2.JW_RL_NKDiscPort = "GBLHR";

			AssertEquals("prerequisite: consol has 2 transports", 2, consol.Transports.Count);

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			var preImportTransportLegsPKs = consol
				.Transports
				.Select(t => t.PK)
				.ToArray();

			Factory.Save();

			var processor = new AirBookingResponseProcessor(Factory);
			var res = processor.Process(eBookingAPIResponse_WithDifferentStatusForEachLeg);

			AssertEquals("main factory wasn't saved during universal xml import",
				1, Factory.SaveCount);

			AssertEquals("received correct response type", ResponseType.BookingConfirmed, res.ResponseType);
			AssertEquals("received correct response message", @"Airline Booking Response:
SYD-DXB EK0415/21Aug - Confirmed
DXB-LHR EK0001/22Aug - Queued

The Consol will be updated automatically when a pending response is received from the Airline. In the meantime, please check the Events Log for more information.", res.UserMessage);
		}

		const string eBookingAPIResponse_WithDifferentStatusForEachLeg = @"<?xml version=""1.0"" encoding=""utf-16""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>AIR_BOOKING_ENGINE</SenderID>
    <RecipientID>WTLEDIMM8</RecipientID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Shipment>
        <DataContext>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
            <SubmissionVersion>29</SubmissionVersion>
          </DocumentaryOverride>
          <DataTargetCollection>
            <DataTarget>
              <Key>CCN1406309</Key>
              <Type>ForwardingConsol</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <WayBillNumber>176-62353561</WayBillNumber>
        <PortOfOrigin>SYD</PortOfOrigin>
        <PortOfDestination>LHR</PortOfDestination>
        <TotalNoOfPacks>2</TotalNoOfPacks>
        <TotalWeight>10</TotalWeight>
        <TotalWeightUnit>KG</TotalWeightUnit>
        <TotalVolume>2</TotalVolume>
        <TotalVolumeUnit>M3</TotalVolumeUnit>
        <BookingConfirmationReference>46434952</BookingConfirmationReference>
        <TransportLegCollection>
          <TransportLeg>
            <LegOrder>1</LegOrder>
            <PortOfLoading>SYD</PortOfLoading>
            <PortOfDischarge>DXB</PortOfDischarge>
            <EstimatedDeparture>2021-08-21T21:10:00</EstimatedDeparture>
            <EstimatedArrival>2021-08-22T05:20:00</EstimatedArrival>
            <VoyageFlightNo>EK0415</VoyageFlightNo>
            <BookingStatus>CNF</BookingStatus>
            <TransportMode>AIR</TransportMode>
            <LegType>Flight1</LegType>
          </TransportLeg>
          <TransportLeg>
            <LegOrder>2</LegOrder>
            <PortOfLoading>DXB</PortOfLoading>
            <PortOfDischarge>LHR</PortOfDischarge>
            <EstimatedDeparture>2021-08-22T07:45:00</EstimatedDeparture>
            <EstimatedArrival>2021-08-22T12:25:00</EstimatedArrival>
            <VoyageFlightNo>EK0001</VoyageFlightNo>
            <BookingStatus>QUE</BookingStatus>
            <TransportMode>AIR</TransportMode>
            <LegType>Flight2</LegType>
          </TransportLeg>
        </TransportLegCollection>
      </Shipment>
    </UniversalShipment>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
            <SubmissionVersion>29</SubmissionVersion>
          </DocumentaryOverride>
          <DataTargetCollection>
            <DataTarget>
              <Key>C00001274</Key>
              <Type>ForwardingConsol</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2021-08-19T03:29:22</EventTime>
        <EventType>BKC</EventType>
        <EventReference>Confirmed</EventReference>
        <EventParameters>
          <Department>Carrier</Department>
          <Location>SYD</Location>
          <Type>AWB</Type>
          <VoyageFlightNumber>EK0415</VoyageFlightNumber>
          <FlightDate>2021-08-21</FlightDate>
          <MessageType>Air Booking</MessageType>
          <ReferenceNumber>176-62353561</ReferenceNumber>
        </EventParameters>
        <ContextCollection>
          <Context>
            <Type>MAWBNumber</Type>
            <Value>176-62353561</Value>
          </Context>
          <Context>
            <Type>MAWBOriginIATAAirportCode</Type>
            <Value>SYD</Value>
          </Context>
          <Context>
            <Type>MAWBDestinationIATAAirportCode</Type>
            <Value>LHR</Value>
          </Context>
          <Context>
            <Type>OriginIATAAirportCode</Type>
            <Value>SYD</Value>
          </Context>
          <Context>
            <Type>DestinationIATAAirportCode</Type>
            <Value>DXB</Value>
          </Context>
          <Context>
            <Type>FlightNumber</Type>
            <Value>EK0415</Value>
          </Context>
          <Context>
            <Type>FlightDate</Type>
            <Value>2021-08-21</Value>
          </Context>
          <Context>
            <Type>NumberOfPieces</Type>
            <Value>2</Value>
          </Context>
          <Context>
            <Type>WeightOfGoods</Type>
            <Value>10KG</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
            <SubmissionVersion>29</SubmissionVersion>
          </DocumentaryOverride>
          <DataTargetCollection>
            <DataTarget>
              <Key>CCN1406309</Key>
              <Type>ForwardingConsol</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2021-08-19T03:29:22</EventTime>
        <EventType>BKP</EventType>
        <EventReference>Queued</EventReference>
        <EventParameters>
          <Department>Carrier</Department>
          <Location>DXB</Location>
          <Type>AWB</Type>
          <VoyageFlightNumber>EK0001</VoyageFlightNumber>
          <FlightDate>2021-08-22</FlightDate>
          <MessageType>Air Booking</MessageType>
          <ReferenceNumber>176-62353561</ReferenceNumber>
        </EventParameters>
        <ContextCollection>
          <Context>
            <Type>MAWBNumber</Type>
            <Value>176-62353561</Value>
          </Context>
          <Context>
            <Type>MAWBOriginIATAAirportCode</Type>
            <Value>SYD</Value>
          </Context>
          <Context>
            <Type>MAWBDestinationIATAAirportCode</Type>
            <Value>LHR</Value>
          </Context>
          <Context>
            <Type>OriginIATAAirportCode</Type>
            <Value>DXB</Value>
          </Context>
          <Context>
            <Type>DestinationIATAAirportCode</Type>
            <Value>LHR</Value>
          </Context>
          <Context>
            <Type>FlightNumber</Type>
            <Value>EK0001</Value>
          </Context>
          <Context>
            <Type>FlightDate</Type>
            <Value>2021-08-22</Value>
          </Context>
          <Context>
            <Type>NumberOfPieces</Type>
            <Value>2</Value>
          </Context>
          <Context>
            <Type>WeightOfGoods</Type>
            <Value>10KG</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>";

		#endregion

		#region TestProcess_BKC_WithConsolCosting

		public void TestProcess_BKC_WithConsolCosting_Success()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "COBA0000690080";

			consol.JK_RL_NKLoadPort = "FRCDG";
			consol.JK_RL_NKLoadPort = "USJFK";

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_Status = "PLN";
			transport.JW_TransportType = "FL1";
			transport.JW_VoyageFlightForBinding = "EY345";
			transport.JW_RL_NKLoadPort = "FRCDG";
			transport.JW_RL_NKDiscPort = "USORD";
			transport.JW_IsLinked = false;

			AssertEquals("prerequisite: consol has 1 transport", 1, consol.Transports.Count);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKLoadPort = "FRCDG";
			shipment.JS_RL_NKDischargePort = "USORD";

			var forwardingExportAirDept = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));
			var shipmentJob = new JobHeader.Loader(Factory, shipment).TryLoadOrCreate();
			shipmentJob.JH_GE = forwardingExportAirDept.PK;

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			var costs = new JobConsolCostCollection(Factory, consol);
			AssertEquals("no consol costs have been created", 0, costs.Count);

			Factory.Save();

			var apiResponse = GetAPIResponse_BKC_WithConsolCosting(Core.Constants.CurrencyCodes.EuropeanUnion);

			var processor = new AirBookingResponseProcessor(Factory);
			var res = processor.Process(apiResponse);

			AssertEquals("main factory wasn't saved during universal xml import",
				1, Factory.SaveCount);

			AssertEquals("received correct response type", ResponseType.BookingConfirmed, res.ResponseType);
			AssertEquals("received correct response message", @"Airline Booking Response:
CDG-ORD AF136/27Feb - Confirmed

The Consol will be updated automatically if any additional response(s) is received from the Airline at a later time. In the meantime, please check the Events Log for more information.", res.UserMessage);
			AssertEquals("did import universal shipment", true, res.ImportedUniversalShipment);

			AssertNoExceptionThrown("expected no exceptions during save", Factory.Save);

			AssertContainsExactElementsInAnyOrder("consol logs imported BKC event",
				new[]
				{
					"BKC |DEP=Carrier|FDT=27-Feb-21 00:00|FRM=FRCDG|LOC=FRCDG|MST=Air Booking|RFN=057-78864796|TO=USORD|TYP=AWB|VFL=AF136",
					"BKC Propagated: All Document Data|DEP=Carrier|FDT=2021-02-27|LOC=FRCDG|MST=Air Booking|RFN=057-78864796|TYP=AWB|VFL=AF136"
				},
				GetLogsForTesting(consol, Events.BookingConfirmedCode));

			AssertEquals("prerequisite: consol has 1 transport", 1, consol.Transports.Count);

			var postImportTransport = consol.Transports[0];
			AssertEquals($"Leg 1 - {nameof(postImportTransport.JW_TransportMode)}", Core.Constants.TransportModes.Air, postImportTransport.JW_TransportMode);
			AssertEquals($"Leg 1 - {nameof(postImportTransport.JW_Status)}", "CNF", postImportTransport.JW_Status);
			AssertEquals($"Leg 1 - {nameof(postImportTransport.JW_TransportType)}", "FL1", postImportTransport.JW_TransportType);
			AssertEquals($"Leg 1 - {nameof(postImportTransport.JW_TransportType)}", "AF136", postImportTransport.JW_VoyageFlightForBinding);
			AssertEquals($"Leg 1 - {nameof(postImportTransport.JW_RL_NKLoadPort)}", "FRCDG", postImportTransport.JW_RL_NKLoadPort);
			AssertEquals($"Leg 1 - {nameof(postImportTransport.JW_RL_NKDiscPort)}", "USORD", postImportTransport.JW_RL_NKDiscPort);
			AssertEquals($"Leg 1 - {nameof(postImportTransport.JW_IsLinked)}", false, postImportTransport.JW_IsLinked);

			costs.Reload(true);
			AssertEquals("created cost from ABE response have been created", 1, costs.Count);

			AssertEquals("Consol Cost charge code", "FRT", costs[0].ChargeCode.AC_Code);
			AssertEquals("Consol Cost charge code", "93c45e64-0467-4fd1-8ef3-9e2b7b45fb55", costs[0].E6_CostReference);
			AssertEquals("Consol Cost charge code", 1251.9m, costs[0].E6_LocalCostAmount);
			AssertEquals("Consol Cost charge code", "EUR", costs[0].E6_RX_NKCurrency);
			AssertEquals("Consol Cost charge code", "CHG", costs[0].E6_ApportionmentMethod);
			AssertEquals("Consol Cost charge code", "SPT", costs[0].E6_RatingBehaviour);

			var messages = Factory.Load<IEDIMessage>(new ZQuery());

			AssertEquals("EDI messages were created (USXml and UEXml)", 2, messages.Length);
			AssertEquals("message marked as recevided", "eBooking API", messages[0].Interchange.EI_From);
			Assert("all messages marked as processed", messages.All(m => m.EM_Status == "PRS"));
			Assert("messages have been saved to db", messages.All(m => m.IsInDatabase));

			AssertAllMessagesHaveDataImportLogNote(messages);
		}

		public void TestProcess_BKC_WithConsolCosting_UniversalXmlParsingError()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "COBA0000690080";

			consol.JK_RL_NKLoadPort = "FRCDG";
			consol.JK_RL_NKLoadPort = "USJFK";

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_Status = "PLN";
			transport.JW_TransportType = "FL1";
			transport.JW_VoyageFlightForBinding = "EY345";
			transport.JW_RL_NKLoadPort = "FRCDG";
			transport.JW_RL_NKDiscPort = "USORD";
			transport.JW_IsLinked = false;

			AssertEquals("prerequisite: consol has 1 transport", 1, consol.Transports.Count);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKLoadPort = "FRCDG";
			shipment.JS_RL_NKDischargePort = "USORD";

			AssertNull("prerequisite: shipment has no JobHeader", shipment.JobHeader);

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			var costs = new JobConsolCostCollection(Factory, consol);
			AssertEquals("no consol costs have been created", 0, costs.Count);

			Factory.Save();

			var processor = new AirBookingResponseProcessor(Factory);
			var res = processor.Process("<bad xml");

			AssertEquals("main factory wasn't saved during universal xml import",
				1, Factory.SaveCount);

			AssertEquals("received correct response type", ResponseType.Invalid, res.ResponseType);
			AssertEquals("received correct response message", "Message received from ABE could not be processed. The content of the message can be found in the EDI Interchange module.", res.UserMessage);
			AssertEquals("did import universal shipment", false, res.ImportedUniversalShipment);

			var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());

			AssertEquals("EDI interchange was created", 1, interchanges.Length);
			AssertEquals("interchange marked as from ABE", "eBooking API", interchanges[0].EI_From);
			AssertEquals("interchange marked as rejected", "REJ", interchanges[0].EI_Status);
			Assert("interchange has been saved to db", interchanges[0].IsInDatabase);
		}

		string GetAPIResponse_BKC_WithConsolCosting(string currencyCode) => $@"<?xml version=""1.0"" encoding=""utf-16""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>AIR_BOOKING_ENGINE</SenderID>
    <RecipientID>HYEBNEUAT</RecipientID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Shipment>
        <DataContext>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
            <SubmissionVersion>1</SubmissionVersion>
          </DocumentaryOverride>
          <DataTargetCollection>
            <DataTarget>
              <Key>COBA0000690080</Key>
              <Type>ForwardingConsol</Type>
            </DataTarget>
          </DataTargetCollection>
          <Workflow>
            <CodesMappedToTarget>true</CodesMappedToTarget>
            <Company>
              <Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
            </Company>
          </Workflow>
        </DataContext>
        <WayBillNumber>057-78864796</WayBillNumber>
        <PortOfOrigin>CDG</PortOfOrigin>
        <PortOfDestination>ORD</PortOfDestination>
        <TotalNoOfPacks>6</TotalNoOfPacks>
        <TotalWeight>300.0</TotalWeight>
        <TotalWeightUnit>KG</TotalWeightUnit>
        <TotalVolume>1.92</TotalVolume>
        <TotalVolumeUnit>M3</TotalVolumeUnit>
        <TransportLegCollection>
          <TransportLeg>
            <LegOrder>1</LegOrder>
            <PortOfLoading>CDG</PortOfLoading>
            <PortOfDischarge>ORD</PortOfDischarge>
            <EstimatedDeparture>2021-02-27T13:05:00</EstimatedDeparture>
            <EstimatedArrival>2021-02-27T15:25:00</EstimatedArrival>
            <VoyageFlightNo>AF136</VoyageFlightNo>
            <BookingStatus>CNF</BookingStatus>
            <TransportMode>AIR</TransportMode>
            <LegType>Flight1</LegType>
          </TransportLeg>
        </TransportLegCollection>
        <NoteCollection>
          <Note>
            <Description>Booking Confirmation Notes</Description>
            <NoteText>Cost as provided by carrier is EUR1251.9
Please note the Total Volume requested for this booking (1.924 M3) is different to what is confirmed (1.92 M3).</NoteText>
            <IsCustomDescription>true</IsCustomDescription>
          </Note>
        </NoteCollection>
        <ConsolCosts>
          <ConsolCostLineCollection>
            <ConsolCostLine>
              <ChargeCode>
                <Code>FRT</Code>
                <Description>International Freight</Description>
              </ChargeCode>
              <SupplierReference>93c45e64-0467-4fd1-8ef3-9e2b7b45fb55</SupplierReference>
              <CostOSAmount>1251.9</CostOSAmount>
              <CostOSCurrency>{currencyCode}</CostOSCurrency>
              <RatingBehaviour>SPT</RatingBehaviour>
              <ApportionmentMethod>CHG</ApportionmentMethod>
              <ImportMetaData>
                <Instruction>Insert</Instruction>
              </ImportMetaData>
            </ConsolCostLine>
          </ConsolCostLineCollection>
        </ConsolCosts>
      </Shipment>
    </UniversalShipment>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
            <SubmissionVersion>1</SubmissionVersion>
          </DocumentaryOverride>
          <DataTargetCollection>
            <DataTarget>
              <Key>COBA0000690080</Key>
              <Type>ForwardingConsol</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2021-02-22T21:31:33</EventTime>
        <EventType>BKC</EventType>
        <EventParameters>
          <Department>Carrier</Department>
          <Location>CDG</Location>
          <Type>AWB</Type>
          <VoyageFlightNumber>AF136</VoyageFlightNumber>
          <FlightDate>2021-02-27</FlightDate>
          <MessageType>Air Booking</MessageType>
          <ReferenceNumber>057-78864796</ReferenceNumber>
        </EventParameters>
        <ContextCollection>
          <Context>
            <Type>MAWBNumber</Type>
            <Value>057-78864796</Value>
          </Context>
          <Context>
            <Type>MAWBOriginIATAAirportCode</Type>
            <Value>CDG</Value>
          </Context>
          <Context>
            <Type>MAWBDestinationIATAAirportCode</Type>
            <Value>ORD</Value>
          </Context>
          <Context>
            <Type>OriginIATAAirportCode</Type>
            <Value>CDG</Value>
          </Context>
          <Context>
            <Type>DestinationIATAAirportCode</Type>
            <Value>ORD</Value>
          </Context>
          <Context>
            <Type>FlightNumber</Type>
            <Value>AF136</Value>
          </Context>
          <Context>
            <Type>FlightDate</Type>
            <Value>2021-02-27</Value>
          </Context>
          <Context>
            <Type>NumberOfPieces</Type>
            <Value>6</Value>
          </Context>
          <Context>
            <Type>WeightOfGoods</Type>
            <Value>300.0Kilograms</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>";

		#endregion

		#region TestProcess_InvalidXml

		public void TestProcess_InvalidXml()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "CCN1406309";

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			Factory.Save();

			const string badXml = "<invalid xml";

			var processor = new AirBookingResponseProcessor(Factory);
			var res = processor.Process(badXml);

			AssertEquals("main factory wasn't saved during universal xml import",
				1, Factory.SaveCount);

			AssertEquals("received correct response type", ResponseType.Invalid, res.ResponseType);
			AssertEquals("received correct response message", "Message received from ABE could not be processed. The content of the message can be found in the EDI Interchange module.", res.UserMessage);
			AssertEquals("did not import universal shipment", false, res.ImportedUniversalShipment);

			var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());

			AssertEquals("EDI interchange was created with bad xml", 1, interchanges.Length);
			AssertEquals("interchange marked as recevided", "eBooking API", interchanges[0].EI_From);
			AssertEquals("interchange has the reply content", badXml, interchanges[0].EI_BodyText);

			AssertNoExceptionThrown("expected no exceptions during save", Factory.Save);
			Assert("interchange was saved", interchanges[0].IsInDatabase);
		}

		#endregion

		#region TestProcess_Rates

		public void TestProcess_Rates()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "CCN1406309";

			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKLoadPort = "SGSIN";

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_Status = "PLN";
			transport1.JW_RL_NKLoadPort = "DEHAM";
			transport1.JW_RL_NKDiscPort = "HKHKG";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_Status = "PLN";
			transport2.JW_RL_NKLoadPort = "HKHKG";
			transport2.JW_RL_NKDiscPort = "SGSIN";

			AssertEquals("prerequisite: consol has 2 transports", 2, consol.Transports.Count);

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			var preImportTransportLegsPKs = consol
				.Transports
				.Select(t => t.PK)
				.ToArray();

			Factory.Save();

			var processor = new AirBookingResponseProcessor(Factory);
			var res = processor.Process(eBookingAPIResponse_Rates);

			AssertEquals("main factory wasn't saved during universal xml import",
				1, Factory.SaveCount);

			AssertEquals("received correct response type", ResponseType.Rates, res.ResponseType);
			AssertNull("no user message", res.UserMessage);
			AssertEquals("did not import universal shipment", false, res.ImportedUniversalShipment);
			AssertNotNull("has universal shipment with rates", res.Shipment);
			CombineAssertions("has job costing", () =>
			{
				Assert(res.Shipment.SubShipmentCollection.All(s => s.JobCosting != null));
				AssertNotNull("Charge Code 1 should not be null", res.Shipment.SubShipmentCollection[0].JobCosting.ChargeLineCollection[0].ChargeCode);
				AssertNull("Charge Code 2 should be null because of missing Code", res.Shipment.SubShipmentCollection[1].JobCosting.ChargeLineCollection[0].ChargeCode);
			});
		}

		const string eBookingAPIResponse_Rates = @"
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>AIR_BOOKING_ENGINE</SenderID>
    <RecipientID>WTLDAUXX1</RecipientID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Shipment>
        <DataContext>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
            <SubmissionVersion>1</SubmissionVersion>
          </DocumentaryOverride>
          <DataTargetCollection>
            <DataTarget>
              <Key>CCN1406309</Key>
              <Type>ForwardingConsol</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <PortOfOrigin>CDG</PortOfOrigin>
        <PortOfDestination>JFK</PortOfDestination>
        <SubShipmentCollection>
          <SubShipment>
            <TransportLegCollection>
              <TransportLeg>
                <LegOrder>1</LegOrder>
                <PortOfLoading>CDG</PortOfLoading>
                <PortOfDischarge>JFK</PortOfDischarge>
                <EstimatedDeparture>2020-12-01T10:20:00</EstimatedDeparture>
                <EstimatedArrival>2020-12-01T13:01:00</EstimatedArrival>
                <VoyageFlightNo>DL263</VoyageFlightNo>
                <BookingStatus>PLN</BookingStatus>
                <LegType>Other</LegType>
                <AircraftType Description=""Airbus A330-300 Passenger"">333</AircraftType>
              </TransportLeg>
            </TransportLegCollection>
            <ConsolCosts>
              <ConsolCostLineCollection>
                <ConsolCostLine>
                  <ChargeCode>
                    <Code>STANDARD</Code>
                    <Description>BOOKABLE</Description>
                  </ChargeCode>
                  <SupplierReference>2847abac-e09e-44b8-a43f-74b51973b008</SupplierReference>
                  <CostOSAmount>641.28</CostOSAmount>
                  <CostOSCurrency>EUR</CostOSCurrency>
                  <RatingBehaviour>SPT</RatingBehaviour>
                </ConsolCostLine>
              </ConsolCostLineCollection>
            </ConsolCosts>
            <JobCosting>
              <ChargeLineCollection>
                <ChargeLine>
                  <ChargeCode>
                    <Code>FRC</Code>
                    <Description>Freight Charge</Description>
                  </ChargeCode>
                  <CostOSAmount>641.28</CostOSAmount>
                  <CostOSCurrency>EUR</CostOSCurrency>
                </ChargeLine>
              </ChargeLineCollection>
            </JobCosting>
            <AddInfoCollection>
              <AddInfo>
                <Key>RateDescription</Key>
                <Value>Online Confirmation - Booking will be confirmed online</Value>
              </AddInfo>
              <AddInfo>
                <Key>Customer</Key>
                <Value>132812</Value>
              </AddInfo>
              <AddInfo>
                <Key>Shipment</Key>
                <Value>31635553-eb00-4896-b11b-416e6158f442</Value>
              </AddInfo>
            </AddInfoCollection>
          </SubShipment>
          <SubShipment>
            <TransportLegCollection>
              <TransportLeg>
                <LegOrder>1</LegOrder>
                <PortOfLoading>CDG</PortOfLoading>
                <PortOfDischarge>JFK</PortOfDischarge>
                <EstimatedDeparture>2020-12-02T10:20:00</EstimatedDeparture>
                <EstimatedArrival>2020-12-02T13:01:00</EstimatedArrival>
                <VoyageFlightNo>DL263</VoyageFlightNo>
                <BookingStatus>PLN</BookingStatus>
                <LegType>Other</LegType>
                <AircraftType Description=""Airbus A330-300 Passenger"">333</AircraftType>
              </TransportLeg>
            </TransportLegCollection>
            <ConsolCosts>
              <ConsolCostLineCollection>
                <ConsolCostLine>
                  <ChargeCode>
                    <Code>STANDARD</Code>
                    <Description>BOOKABLE</Description>
                  </ChargeCode>
                  <SupplierReference>3d828051-43ff-48f4-abe1-32714fff4b50</SupplierReference>
                  <CostOSAmount>641.28</CostOSAmount>
                  <CostOSCurrency>EUR</CostOSCurrency>
                  <RatingBehaviour>SPT</RatingBehaviour>
                </ConsolCostLine>
              </ConsolCostLineCollection>
            </ConsolCosts>
            <JobCosting>
              <ChargeLineCollection>
                <ChargeLine>
                  <ChargeCode>
                    <Description>Freight Charge</Description>
                  </ChargeCode>
                  <CostOSAmount>641.28</CostOSAmount>
                  <CostOSCurrency>EUR</CostOSCurrency>
                </ChargeLine>
              </ChargeLineCollection>
            </JobCosting>
            <AddInfoCollection>
              <AddInfo>
                <Key>RateDescription</Key>
                <Value>Online Confirmation - Booking will be confirmed online</Value>
              </AddInfo>
              <AddInfo>
                <Key>Customer</Key>
                <Value>132812</Value>
              </AddInfo>
              <AddInfo>
                <Key>Shipment</Key>
                <Value>31635553-eb00-4896-b11b-416e6158f442</Value>
              </AddInfo>
            </AddInfoCollection>
          </SubShipment>
        </SubShipmentCollection>
      </Shipment>
    </UniversalShipment>
  </Body>
</UniversalInterchange>";

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);
		}

		protected override void TearDown()
		{
			base.TearDown();

			ObjectFactory.DisposeSubstitutions();
		}

		const string airBookingDataStoreName = "AirBooking";

		string[] GetLogsForTesting(IStmALogParent logParent, string eventCode) => logParent
			.Logs
			.GetAllLogs()
			.Cast<StmALog>()
			.Where(l => l.SL_SE_NKEvent == eventCode)
			.OrderBy(l => l.SL_PostedTimeUtc)
			.Select(l => $"{l.SL_SE_NKEvent} {l.SL_Reference}".TrimEnd())
			.ToArray();

		void AssertEventDateWasConvertedToLocal(IStmALogParent logParent, ZString eventCode, string dateTimeInXml, string unloco)
		{
			AssertEventDateWasConvertedToLocal(logParent, eventCode, dateTimeInXml, new List<string> { unloco });
		}

		void AssertEventDateWasConvertedToLocal(IStmALogParent logParent, ZString eventCode, string dateTimeInXml, IEnumerable<string> loadPorts = null)
		{
			if (!ZDateTime.TryParseISO8601Date(dateTimeInXml, out var xmlDateTime))
			{
				Fail($"{dateTimeInXml} is not a valid ISO8601 date/time");
			}

			var defaultUtcOffset = System.TimeZoneInfo.Local.GetUtcOffset(xmlDateTime.ToDateTime());
			var localDateTime = xmlDateTime.Add(defaultUtcOffset);

			var logs = logParent
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(l => l.SL_SE_NKEvent == eventCode)
				.ToArray();

			if (logs.Length == 0)
			{
				Fail($"Expected at least 1 '{eventCode}' event type");
			}

			foreach (var log in logs)
			{
				AssertEquals("Event Date was mapped to local date/time", localDateTime, log.SL_EventTime);

				var logUtcOffset = defaultUtcOffset;
				if (loadPorts != null)
				{
					var matchedTimezoneFound = false;
					foreach (var unloco in loadPorts)
					{
						logUtcOffset = TimeFactory.Instance.GetUtcOffsetBasedOnLocal(unloco, localDateTime.ToDateTime());
						if (log.SL_EventTimeOffset == new ZDateTimeOffset(localDateTime, logUtcOffset))
						{
							matchedTimezoneFound = true;
						}
					}
					Assert("Event Date was mapped to loading port's date/time", matchedTimezoneFound);
				}
				else
				{
					AssertEquals("Event Date was mapped to local date/time", new ZDateTimeOffset(localDateTime, logUtcOffset), log.SL_EventTimeOffset);
				}
			}
		}

		void AssertAllMessagesHaveDataImportLogNote() => AssertAllMessagesHaveDataImportLogNote(Factory.Load<IEDIMessage>(new ZQuery()));

		void AssertAllMessagesHaveDataImportLogNote(IEDIMessage[] messages)
		{
			foreach (var noteParent in messages.Cast<IStmNoteParent>())
			{
				var dataImportNotes = noteParent.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description, false);
				AssertEquals("Exactly Data Import Note is present", 1, dataImportNotes.Length);
				Assert("Data Import Note is not empty", !dataImportNotes[0].ST_NoteText.IsEmpty);
			}
		}

		#endregion
	}
}
