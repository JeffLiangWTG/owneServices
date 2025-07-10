using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class AirBookingResponseConcurrencyTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region TestDuplicateEventWithSameSubmissionVersionIsNotProcessed_ISN

		public void TestDuplicateEventWithSameSubmissionVersionIsNotProcessed_ISN()
		{
			var factory = new BusinessObjectFactory();

			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "CCN1406309";

			var documentData = factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			factory.Save();

			var consolPreImportLogs = GetLogsForTesting(consol);

			var processor = new AirBookingResponseProcessor(factory);
			var res = processor.Process(eBookingAPIResponse_ISN);

			factory.Save();

			AssertEquals("received correct response type", ResponseType.InterchangeSent, res.ResponseType);

			factory = new BusinessObjectFactory();
			consol = factory.Load<ForwardingConsol>(consol.PK);

			AssertContainsExactElementsInAnyOrder("consol logs have imported ISN event",
				new[]
				{
					"ISN |DEP=WiseTech Global|MST=Air Booking|RFN=618-73808291"
				},
				GetLogsForTesting(consol).Except(consolPreImportLogs));

			documentData = factory.Load<VisualizerDocumentData>(documentData.PK);

			AssertContainsExactElementsInAnyOrder("documentData logs have imported ISN event",
				new[]
				{
					"ISN |DEP=WiseTech Global|MST=Air Booking|RFN=618-73808291"
				},
				GetLogsForTesting(documentData));

			var universalMessages = GetQueuedUniversalMessagesFromInterchange(eBookingAPIResponse_ISN);

			AssertEquals("prerequisite: we should have only 1 message created from ISN", 1, universalMessages.Length);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var processingResult = manager.Process(universalMessages.Single());

			Assert($"Event was rejected because one with the same SubmissionVersion has already been imported. Log: {GetLogText(processingResult)}",
				processingResult.HasErrors());

			factory = new BusinessObjectFactory();
			consol = factory.Load<ForwardingConsol>(consol.PK);

			AssertContainsExactElementsInAnyOrder("consol logs don't have duplicate ISN event",
				new[]
				{
					"ISN |DEP=WiseTech Global|MST=Air Booking|RFN=618-73808291"
				},
				GetLogsForTesting(consol).Except(consolPreImportLogs));

			documentData = factory.Load<VisualizerDocumentData>(documentData.PK);

			AssertContainsExactElementsInAnyOrder("document data logs don't have duplicate ISN event",
				new[]
				{
					"ISN |DEP=WiseTech Global|MST=Air Booking|RFN=618-73808291"
				},
				GetLogsForTesting(documentData));
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

		#region TestDuplicateEventWithSameSubmissionVersionIsNotProcessed_BKC

		public void TestDuplicateEventWithSameSubmissionVersionIsNotProcessed_BKC()
		{
			var factory = new BusinessObjectFactory();

			var consol = factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "CCN1406309";
			consol.JK_MasterBillNum = "60789506712";

			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKLoadPort = "SGSIN";

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_Status = "PLN";
			transport1.JW_RL_NKLoadPort = "DEHAM";
			transport1.JW_RL_NKDiscPort = "HKHKG";
			transport1.JW_ETD = new ZDateTime(2020, 05, 09);
			transport1.JW_ETA = new ZDateTime(2020, 05, 09);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_Status = "PLN";
			transport2.JW_RL_NKLoadPort = "HKHKG";
			transport2.JW_RL_NKDiscPort = "SGSIN";
			transport2.JW_ETD = new ZDateTime(2020, 05, 10);
			transport2.JW_ETA = new ZDateTime(2020, 05, 10);

			AssertEquals("prerequisite: consol has 2 transports", 2, consol.Transports.Count);

			var documentData = factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			factory.Save();

			var consolPreImportLogs = GetLogsForTesting(consol);

			var processor = new AirBookingResponseProcessor(factory);
			var res = processor.Process(eBookingAPIResponse_BKC);

			AssertEquals("received correct response type", ResponseType.BookingConfirmed, res.ResponseType);

			factory.Save();

			factory = new BusinessObjectFactory();
			consol = factory.Load<ForwardingConsol>(consol.PK);

			AssertContainsExactElementsInAnyOrder("consol logs have imported BKC events",
				new[]
				{
					"BKC |DEP=Carrier|FDT=09-May-20 00:00|FRM=DEHAM|LOC=DEHAM|MST=Air Booking|TO=HKHKG|VFL=EY7",
					"BKC Propagated: All Document Data|DEP=Carrier|FDT=2020-05-09|LOC=DEHAM|MST=Air Booking|VFL=EY7",
					"BKC |DEP=Carrier|FDT=10-May-20 00:00|FRM=HKHKG|LOC=HKHKG|MST=Air Booking|TO=SGSIN|VFL=EY8",
					"BKC Propagated: All Document Data|DEP=Carrier|FDT=2020-05-10|LOC=HKHKG|MST=Air Booking|VFL=EY8",
				},
				GetLogsForTesting(consol).Except(consolPreImportLogs));

			documentData = factory.Load<VisualizerDocumentData>(documentData.PK);

			AssertContainsExactElementsInAnyOrder("document data logs have imported BKC events",
				new[]
				{
					"BKC |DEP=Carrier|FDT=2020-05-09|LOC=DEHAM|MST=Air Booking|VFL=EY7",
					"BKC |DEP=Carrier|FDT=2020-05-10|LOC=HKHKG|MST=Air Booking|VFL=EY8"
				},
				GetLogsForTesting(documentData));

			var universalMessages = GetQueuedUniversalMessagesFromInterchange(eBookingAPIResponse_BKC);

			foreach (var universalMessage in universalMessages)
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var processingResult = manager.Process(universalMessage);

				Assert($"Shipment or Event was rejected because one with the same SubmissionVersion has already been imported. Log: {GetLogText(processingResult)}",
					processingResult.HasErrors());
			}

			factory = new BusinessObjectFactory();
			consol = factory.Load<ForwardingConsol>(consol.PK);

			AssertContainsExactElementsInAnyOrder("consol logs don't have duplicate BKC events",
				new[]
				{
					"BKC |DEP=Carrier|FDT=09-May-20 00:00|FRM=DEHAM|LOC=DEHAM|MST=Air Booking|TO=HKHKG|VFL=EY7",
					"BKC Propagated: All Document Data|DEP=Carrier|FDT=2020-05-09|LOC=DEHAM|MST=Air Booking|VFL=EY7",
					"BKC |DEP=Carrier|FDT=10-May-20 00:00|FRM=HKHKG|LOC=HKHKG|MST=Air Booking|TO=SGSIN|VFL=EY8",
					"BKC Propagated: All Document Data|DEP=Carrier|FDT=2020-05-10|LOC=HKHKG|MST=Air Booking|VFL=EY8",
				},
				GetLogsForTesting(consol).Except(consolPreImportLogs));

			documentData = factory.Load<VisualizerDocumentData>(documentData.PK);

			AssertContainsExactElementsInAnyOrder("document data don't have duplicate BKC events",
				new[]
				{
					"BKC |DEP=Carrier|FDT=2020-05-09|LOC=DEHAM|MST=Air Booking|VFL=EY7",
					"BKC |DEP=Carrier|FDT=2020-05-10|LOC=HKHKG|MST=Air Booking|VFL=EY8"
				},
				GetLogsForTesting(documentData));
		}

		const string eBookingAPIResponse_BKC = @"<?xml version=""1.0"" encoding=""utf-16""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>Air_Booking_Engine</SenderID>
	</Header>
	<Body>
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
	</Body>
</UniversalInterchange>";

		#endregion

		#region Implementation

		const string airBookingDataStoreName = "AirBooking";

		string[] GetLogsForTesting(IStmALogParent logParent) => logParent
			.Logs
			.GetAllLogs()
			.Cast<StmALog>()
			.Where(l => l.SL_SE_NKEvent != AutoEvents.WorkflowTemplateAppliedCode
				&& l.SL_SE_NKEvent != AutoEvents.EditedARecordCode
				&& l.SL_SE_NKEvent != AutoEvents.DataImportCode
				&& l.SL_SE_NKEvent != AutoEvents.NoteAddedCode)
			.OrderBy(l => l.SL_PostedTimeUtc)
			.Select(l => $"{l.SL_SE_NKEvent} {l.SL_Reference}".TrimEnd())
			.ToArray();

		IEDIMessage[] GetQueuedUniversalMessagesFromInterchange(string interchange)
		{
			var doc = XDocument.Parse(interchange);

			var body = doc
				.Root
				.Elements()
				.FirstOrDefault(elem => elem.Name.LocalName == "Body");

			var res = new List<IEDIMessage>();

			foreach (var element in body.Elements())
			{
				switch (element.Name.LocalName)
				{
					case "UniversalShipment":
						res.Add(GetQueuedUniversalShipmentMessage(element.ToString()));
						break;

					case "UniversalEvent":
						res.Add(GetQueuedUniversalEventMessage(element.ToString()));
						break;
				}
			}

			return res.ToArray();
		}

		string GetLogText(ISimpleLogResult log)
		{
			return string.Join(System.Environment.NewLine,
				log.Logs.Select(l => $"{l.Type}: {l.Message}"));
		}

		#endregion
	}
}
