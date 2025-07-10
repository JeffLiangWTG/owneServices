using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	public abstract class ConsolEventParentFinderTest<TConsol, TShipment, TContainer> : TestCaseWithFactory
		where TConsol : CommonConsol
		where TShipment : CommonShipment
		where TContainer : CommonContainer
	{
		public void TestUniversalEventDoesNotHaveContextCollection()
		{
			const string shipmentLevelEventXmlText = @"
<UniversalEvent>
  <Event>
    <EventType>OCR</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
  </Event>
</UniversalEvent>";

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();

			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);
			AssertNoExceptionThrown(() => subscriber.GetLogParentsForEvent(xmlEvent));
		}

		public void TestIncomingEventDoesNotMatchShipmentWhereConsolReferencesAreIncludedButDoNoMatchToExistingConsol()
		{
			const string shipmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
				</Context>
				<Context>
				  <Type Description=""AMS Number"">AMS</Type>
				  <Value>134FREGT</Value>
				</Context>
				<Context>
				  <Type Description="""">COC</Type>
				  <Value>267AIRGT</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var shipment = Factory.New<TShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var additionalReference1 = shipment.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "134FREGT";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var additionalReference2 = shipment.Numbers.AddNew();
			additionalReference2.CE_EntryNum = "267AIRGT";
			additionalReference2.CE_EntryType = "COC";
			additionalReference2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);
			xmlEvent.DataContext = DataContextFactory.New();
			xmlEvent.DataContext.CodesMappedToTarget = true;

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("logParents", logParents);
		}

		public void TestIncomingEventMatchConsolWhereShipmentReferencesAreIncluded()
		{
			const string shipmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>CarriersBookingReference</Type>
				  <Value>02012345675</Value>
				</Context>
				<Context>
				  <Type>ShippersReference</Type>
				  <Value>20257654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var consol = Factory.New<TConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_BookingReference = "02012345675";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			AssertEquals(consol.PK, logParents[0].PK);
		}

		public void TestIncomingEventMatchesOnShipmentWhenMasterBillIsPresentButHasHouseBillThatMatchesToExistingShipment()
		{
			const string shipmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
				</Context>
				<Context>
				  <Type>HBOLNumber</Type>
				  <Value>20257654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var consol = Factory.New<TConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "02012345675";

			var matchingShipment = Factory.New<TShipment>();
			matchingShipment.JS_TransportMode = Constants.TransportModes.Sea;
			matchingShipment.JS_HouseBill = "20257654321";
			matchingShipment.JS_UniqueConsignRef = "S10101010";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TShipment), logParent.GetType());

			var logParentShipment = logParent as TShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));
		}

		public void TestIncomingEventMatchesOnShipmentWhenCarriersBookingReferenceIsPresentButNoMasterBillAndHasHouseBillThatMatchesToExistingShipment()
		{
			const string shipmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>CarriersBookingReference</Type>
				  <Value>02012345675</Value>
				</Context>
				<Context>
				  <Type>HBOLNumber</Type>
				  <Value>20257654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var consol = Factory.New<TConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_BookingReference = "02012345675";

			var matchingShipment = Factory.New<TShipment>();
			matchingShipment.JS_TransportMode = Constants.TransportModes.Sea;
			matchingShipment.JS_HouseBill = "20257654321";
			matchingShipment.JS_UniqueConsignRef = "S10101010";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TShipment), logParent.GetType());

			var logParentShipment = logParent as TShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));
		}

		public void TestIncomingEventMatchesOnShipmentWhenMasterBillIsPresentButHasReferencesThatCanMatchToTheShipment()
		{
			const string shipmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
				</Context>
				<Context>
				  <Type>ShippersReference</Type>
				  <Value>20257654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingShipment = Factory.New<TShipment>();
			matchingShipment.JS_TransportMode = Constants.TransportModes.Sea;
			matchingShipment.JS_BookingReference = "20257654321";
			matchingShipment.JS_UniqueConsignRef = "S10101010";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TShipment), logParent.GetType());

			var logParentShipment = logParent as TShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));
		}

		public void TestIncomingEventMatchingGetsLatestConsolWhenEqualMatch()
		{
			const string shipmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ShippersReference</Type>
				  <Value>20257654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var dummyShipment = Factory.New<TShipment>();
			dummyShipment.JS_BookingReference = "20257654321";

			Factory.Save();

			Thread.Sleep(200);

			var matchingShipment = Factory.New<TShipment>();
			matchingShipment.JS_BookingReference = "20257654321";
			matchingShipment.JS_UniqueConsignRef = "S10101010";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TShipment), logParent.GetType());

			var logParentShipment = logParent as TShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));
		}

		public void TestIncomingEventMatchingFallsBackToPortOfOriginAndDestinationForShipment()
		{
			const string shipmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ShippersReference</Type>
				  <Value>20257654321</Value>
				</Context>
				<Context>
				  <Type>HBOLOriginUNLOCO</Type>
				  <Value>NZCHC</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingShipment = Factory.New<TShipment>();
			matchingShipment.JS_BookingReference = "20257654321";
			matchingShipment.JS_RL_NKOrigin = "NZCHC";
			matchingShipment.JS_UniqueConsignRef = "S10101010";

			Factory.Save();

			Thread.Sleep(200);

			var dummyShipment = Factory.New<TShipment>();
			dummyShipment.JS_BookingReference = "20257654321";

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TShipment), logParent.GetType());

			var logParentShipment = logParent as TShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));
		}

		public void TestIncomingEventDoesNotMatchToShipmentWithPortOfOriginOrDestinationOnly()
		{
			const string shipmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>HBOLOriginUNLOCO</Type>
				  <Value>NZCHC</Value>
				</Context>
				<Context>
				  <Type>HBOLDestinationUNLOCO</Type>
				  <Value>AUMEL</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var shipment = Factory.New<TShipment>();
			shipment.JS_RL_NKOrigin = "NZCHC";
			shipment.JS_RL_NKDestination = "AUMEL";
			shipment.JS_UniqueConsignRef = "S10101010";

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			AssertNull("logParents", logParents);
		}

		public void TestIncomingEventMatchesOnInterimReceiptForShipment()
		{
			const string shipmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>InterimReceipt</Type>
				  <Value>INTERIM</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingShipment = Factory.New<TShipment>();
			matchingShipment.JS_InterimReceipt = "INTERIM";
			matchingShipment.JS_UniqueConsignRef = "S10101010";

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TShipment), logParent.GetType());

			var logParentShipment = logParent as TShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));
		}

		public void TestIncomingEventDoesNotMatchesOnOrderNumbersForShipment()
		{
			const string shipmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>OrderNumber</Type>
				  <Value>Waiter!</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingShipment = Factory.New<TShipment>();
			matchingShipment.DocsAndCartage.JP_OrderItemsAsString = "Waiter!";
			matchingShipment.JS_UniqueConsignRef = "S10101010";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull(logParents);
		}

		public void TestIncomingEventMatchesOnShippersReferenceForShipment()
		{
			const string shipmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ShippersReference</Type>
				  <Value>20257654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingShipment = Factory.New<TShipment>();
			matchingShipment.JS_BookingReference = "20257654321";
			matchingShipment.JS_UniqueConsignRef = "S10101010";

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TShipment), logParent.GetType());

			var logParentShipment = logParent as TShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));
		}

		public void TestIncomingEventLinksToTheLatestShipmentWhenUsingHBOLOnly()
		{
			const string shipmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>HBOLNumber</Type>
				  <Value>20257654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingShipment = Factory.New<TShipment>();
			matchingShipment.JS_TransportMode = Constants.TransportModes.Sea;
			matchingShipment.JS_HouseBill = "20257654321";

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TShipment), logParent.GetType());

			var logParentShipment = logParent as TShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));
		}

		public void TestIncomingEventLinksToTheLatestShipmentWhenUsingHAWBOnly()
		{
			const string shipmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>HAWBNumber</Type>
				  <Value>20257654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingShipment = Factory.New<TShipment>();
			matchingShipment.JS_TransportMode = Constants.TransportModes.Air;
			matchingShipment.JS_HouseBill = "20257654321";

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TShipment), logParent.GetType());

			var logParentShipment = logParent as TShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));
		}

		public void TestFlightNumberAndContainerNumberReturnsContainer()
		{
			const string containerLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>FlightNumber</Type>
				  <Value>12234</Value>
				</Context>
				<Context>
				  <Type>ContainerNumber</Type>
				  <Value>20257654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var consol = Factory.New<TConsol>();

			var transport = consol.Transports.AddNew();
			transport.JW_VoyageFlight = "12234";

			var matchingContainer = Factory.New<TContainer>();
			matchingContainer.JC_ContainerNum = "20257654321";

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Containers.Add(matchingContainer);

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TContainer), logParent.GetType());

			var logParentContainer = logParent as TContainer;

			AssertNotNull("logParentContainer", logParentContainer);
			AssertEquals("Should get best matching Container.", GetHumanReadableID(matchingContainer), GetHumanReadableID(logParentContainer));
		}

		public void TestIncomingEventLinksToContainerWhenUsingContainerNumberOnly()
		{
			const string containerLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ContainerNumber</Type>
				  <Value>20257654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var consol = Factory.New<TConsol>();

			var matchingContainer = Factory.New<TContainer>();
			matchingContainer.JC_ContainerNum = "20257654321";

			var dummyContainer = Factory.New<TContainer>();
			dummyContainer.JC_ContainerNum = "20257654321";

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Containers.Add(matchingContainer);

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TContainer), logParent.GetType());

			var logParentContainer = logParent as TContainer;

			AssertNotNull("logParentContainer", logParentContainer);
			AssertEquals("Should get best matching Container.", GetHumanReadableID(matchingContainer), GetHumanReadableID(logParentContainer));
		}

		public void TestIncomingEventShouldNotLinkToConsolShipmentContainer()
		{
			var eventXML = @"
			<UniversalEvent>
				<Event>
					<EventType>CCD</EventType>
					<EventTime>10-JUL-2010 18:00</EventTime>
					<EventReference>Dummy Description</EventReference>
					<DataProvider>Dummy</DataProvider>
					<ContextCollection>
						<Context>
							<Type>MBOLNumber</Type>
							<Value>11111</Value>
						</Context>
						<Context>
							<Type>CarriersBookingReference</Type>
							<Value>22222</Value>
						</Context>
						<Context>
							<Type>HBOLNumber</Type>
							<Value>33333</Value>
						</Context>
						<Context>
							<Type>ContainerNumber</Type>
							<Value>CONT4444444</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>";

			var consol = Factory.New<TConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "MBL11111";
			consol.JK_BookingReference = "BKG22222";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT4444444";

			var shipment = Factory.New<TShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "HBL33333";
			consol.GridShipments.Add(shipment);

			Factory.Save();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXML);

			var subscriber = GetNewConsolEventParentFinder();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("Should not match LogParents", logParents);
		}

		public void TestIncomingEventShouldLinkToContainer()
		{
			var eventXML = @"
			<UniversalEvent>
				<Event>
					<EventType>CCD</EventType>
					<EventTime>10-JUL-2010 18:00</EventTime>
					<EventReference>Dummy Description</EventReference>
					<DataProvider>Dummy</DataProvider>
					<ContextCollection>
						<Context>
							<Type>ContainerNumber</Type>
							<Value>CONT4444444</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>";

			var consol = Factory.New<TConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "MBL11111";
			consol.JK_BookingReference = "BKG22222";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT4444444";

			var shipment = Factory.New<TShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "HBL33333";
			consol.GridShipments.Add(shipment);

			Factory.Save();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXML);

			var subscriber = GetNewConsolEventParentFinder();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNotNull("logParents", logParents);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TContainer), logParent.GetType());
		}

		public void TestIncomingEventDoesntLinkToContainerWithoutConsolWhenUsingContainerNumberOnly()
		{
			const string containerLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ContainerNumber</Type>
				  <Value>20257654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var shipment = Factory.New<TShipment>();

			var dummyContainer = Factory.New<TContainer>();
			dummyContainer.JC_ContainerNum = "20257654321";
			dummyContainer.JC_JS_FCLBookingOnlyLink = shipment.PK;
			var packLine = shipment.OuterPackLines.AddNew();
			dummyContainer.PackLines.Add(packLine);

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("Should not match LogParents", logParents);
		}

		public void TestIncomingEventLinksToLegWhenUsingHBOLAndVoyageNumberAndFallsBackToShipmentIfCannotMatchLeg()
		{
			const string transportLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>HBOLNumber</Type>
				  <Value>20257654321</Value>
				</Context>
				<Context>
				  <Type>VoyageNumber</Type>
				  <Value>12234</Value>
				</Context>
				<Context>
				  <Type>VesselName</Type>
				  <Value>MAGIC SCHOOL BUS</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingShipment = Factory.New<TShipment>();
			matchingShipment.JS_TransportMode = Constants.TransportModes.Sea;
			matchingShipment.JS_HouseBill = "20257654321";

			var matchingTransport = matchingShipment.Transports.AddNew();
			matchingTransport.JW_VoyageFlight = "12234";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(transportLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(Transport), logParent.GetType());

			var logParentLeg = logParent as Transport;

			AssertNotNull("logParentLeg", logParentLeg);
			AssertEquals("Should get best matching Transport.", GetHumanReadableID(matchingTransport), GetHumanReadableID(logParentLeg));

			matchingShipment.Transports.RemoveAndDeleteAll();
			xmlEvent = eventDeserializer.Parse(transportLevelEventXmlText);
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			AssertEquals("Should match one LogParent", 1, logParents.Length);

			logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TShipment), logParent.GetType());

			var logParentShipment = logParent as TShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));
		}

		public void TestIncomingEventMatchingGetsLatestShipmentWhenEqualMatch()
		{
			const string consolLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>CarriersBookingReference</Type>
				  <Value>20257654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var timeNow = ZDateTime.Now;
			var dummyConsol = Factory.New<TConsol>();
			dummyConsol.JK_BookingReference = "20257654321";
			dummyConsol.JK_SystemCreateTimeUtc = timeNow;

			var matchingConsol = Factory.New<TConsol>();
			matchingConsol.JK_BookingReference = "20257654321";
			matchingConsol.JK_UniqueConsignRef = "S10101010";
			matchingConsol.JK_SystemCreateTimeUtc = timeNow.AddHours(6);

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(consolLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TConsol), logParent.GetType());

			var logParentConsol = logParent as TConsol;

			AssertNotNull("logParentConsol", logParentConsol);
			AssertEquals("Should get best matching Consol.", GetHumanReadableID(matchingConsol), GetHumanReadableID(logParentConsol));
		}

		public void TestIncomingEventPicksBestMatchWhenThereAreMultipleMatchesForConsol()
		{
			const string consolLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>CarriersBookingReference</Type>
				  <Value>20257654321</Value>
				</Context>
				<Context>
				  <Type>AgentsReference</Type>
				  <Value>DOUBLEAGENT</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingConsol = Factory.New<TConsol>();

			matchingConsol.JK_TransportMode = Constants.TransportModes.Air;
			matchingConsol.JK_BookingReference = "02012345675";
			matchingConsol.JK_AgentsReference = "DOUBLEAGENT";

			Factory.Save();

			Thread.Sleep(200);

			var dummyConsol = Factory.New<TConsol>();

			dummyConsol.JK_TransportMode = Constants.TransportModes.Air;
			dummyConsol.JK_BookingReference = "02012345675";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(consolLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TConsol), logParent.GetType());

			var logParentConsol = logParent as TConsol;

			AssertNotNull("logParentConsol", logParentConsol);
			AssertEquals("Should get best matching Consol.", GetHumanReadableID(matchingConsol), GetHumanReadableID(logParentConsol));
		}

		public void TestIncomingEventDoesNotMatchToConsolIfOnlyCarrierAndCoLoadWithC1CCode()
		{
			const string consolLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>CarrierC1CCode</Type>
				  <Value>C1CO</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "C1CO";
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			carrier.OH_IsShippingLine = true;
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var matchingConsol = Factory.New<TConsol>();

			matchingConsol.JK_TransportMode = Constants.TransportModes.Air;
			matchingConsol.JK_BookingReference = "02012345675";
			matchingConsol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(consolLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull(logParents);
		}

		public void TestIncomingEventFallsBackToCarrierC1CCodeWhenThereAreMultipleMatchesForConsol()
		{
			const string consolLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>CarriersBookingReference</Type>
				  <Value>20257654321</Value>
				</Context>
				<Context>
				  <Type>CarrierC1CCode</Type>
				  <Value>C1CO</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "C1CO";
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			carrier.OH_IsShippingLine = true;
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var matchingConsol = Factory.New<TConsol>();

			matchingConsol.JK_TransportMode = Constants.TransportModes.Air;
			matchingConsol.JK_BookingReference = "20257654321";
			matchingConsol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			Factory.Save();

			Thread.Sleep(200);

			var dummyConsol = Factory.New<TConsol>();

			dummyConsol.JK_TransportMode = Constants.TransportModes.Air;
			dummyConsol.JK_BookingReference = "20257654321";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(consolLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TConsol), logParent.GetType());

			var logParentConsol = logParent as TConsol;

			AssertNotNull("logParentConsol", logParentConsol);
			AssertEquals("Should get best matching Consol.", GetHumanReadableID(matchingConsol), GetHumanReadableID(logParentConsol));
		}

		public void TestIncomingEventDoesNotMatchToConsolIfOnlyOriginAndDestinationUNLOCO()
		{
			const string consolLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MBOLOriginUNLOCO</Type>
				  <Value>AUMEL</Value>
				</Context>
				<Context>
				  <Type>MBOLDestinationUNLOCO</Type>
				  <Value>NZCHC</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingConsol = Factory.New<TConsol>();

			matchingConsol.JK_TransportMode = Constants.TransportModes.Air;
			matchingConsol.JK_BookingReference = "02012345675";
			matchingConsol.JK_RL_NKLoadPort = "AUMEL";
			matchingConsol.JK_RL_NKDischargePort = "NZCHC";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(consolLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull(logParents);
		}

		public void TestIncomingEventFallsBackToDischargePortWhenThereAreMultipleMatchesForConsol()
		{
			const string consolLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>CarriersBookingReference</Type>
				  <Value>20257654321</Value>
				</Context>
				<Context>
				  <Type>MBOLDestinationUNLOCO</Type>
				  <Value>NZCHC</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingConsol = Factory.New<TConsol>();

			matchingConsol.JK_TransportMode = Constants.TransportModes.Air;
			matchingConsol.JK_BookingReference = "20257654321";
			matchingConsol.JK_RL_NKDischargePort = "NZCHC";

			Factory.Save();

			Thread.Sleep(200);

			var dummyConsol = Factory.New<TConsol>();

			dummyConsol.JK_TransportMode = Constants.TransportModes.Air;
			dummyConsol.JK_BookingReference = "20257654321";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(consolLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TConsol), logParent.GetType());

			var logParentConsol = logParent as TConsol;

			AssertNotNull("logParentConsol", logParentConsol);
			AssertEquals("Should get best matching Consol.", GetHumanReadableID(matchingConsol), GetHumanReadableID(logParentConsol));
		}

		public void TestIncomingEventFallsBackToLoadPortWhenThereAreMultipleMatchesForConsol()
		{
			const string consolLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>CarriersBookingReference</Type>
				  <Value>20257654321</Value>
				</Context>
				<Context>
				  <Type>MBOLOriginUNLOCO</Type>
				  <Value>AUMEL</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingConsol = Factory.New<TConsol>();

			matchingConsol.JK_TransportMode = Constants.TransportModes.Air;
			matchingConsol.JK_BookingReference = "20257654321";
			matchingConsol.JK_RL_NKLoadPort = "AUMEL";

			Factory.Save();

			Thread.Sleep(200);

			var dummyConsol = Factory.New<TConsol>();

			dummyConsol.JK_TransportMode = Constants.TransportModes.Air;
			dummyConsol.JK_BookingReference = "20257654321";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(consolLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TConsol), logParent.GetType());

			var logParentConsol = logParent as TConsol;

			AssertNotNull("logParentConsol", logParentConsol);
			AssertEquals("Should get best matching Consol.", GetHumanReadableID(matchingConsol), GetHumanReadableID(logParentConsol));
		}

		public void TestIncomingEventMatchesToConsolOnAgentsReference()
		{
			const string consolLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>AgentsReference</Type>
				  <Value>DOUBLEAGENT</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingConsol = Factory.New<TConsol>();

			matchingConsol.JK_TransportMode = Constants.TransportModes.Air;
			matchingConsol.JK_AgentsReference = "DOUBLEAGENT";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(consolLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TConsol), logParent.GetType());

			var logParentConsol = logParent as TConsol;

			AssertNotNull("logParentConsol", logParentConsol);
			AssertEquals("Should get best matching Consol.", GetHumanReadableID(matchingConsol), GetHumanReadableID(logParentConsol));
		}

		public void TestIncomingEventMatchesToConsolOnCarriersBookingReference()
		{
			const string consolLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>CarriersBookingReference</Type>
				  <Value>20257654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingConsol = Factory.New<TConsol>();

			matchingConsol.JK_TransportMode = Constants.TransportModes.Air;
			matchingConsol.JK_BookingReference = "20257654321";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(consolLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TConsol), logParent.GetType());

			var logParentConsol = logParent as TConsol;

			AssertNotNull("logParentConsol", logParentConsol);
			AssertEquals("Should get best matching Consol.", GetHumanReadableID(matchingConsol), GetHumanReadableID(logParentConsol));
		}

		public void TestOnlyHavingFlightNumberReturnsNoMatch()
		{
			const string transportLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>FlightNumber</Type>
				  <Value>12234</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var consol = Factory.New<TConsol>();

			var nonMatchingTransport = consol.Transports.AddNew();
			nonMatchingTransport.JW_VoyageFlight = "12234";

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.Transports.Add(nonMatchingTransport);

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(transportLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("Should match no LogParents", logParents);
		}

		public void TestLinkingIncomingEventToAnIndividualLegWithMBOLAndVoyageNumber()
		{
			const string tranpsortLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
				</Context>
				<Context>
				  <Type>VoyageNumber</Type>
				  <Value>11224</Value>
				</Context>
				<Context>
				  <Type>VesselName</Type>
				  <Value>MAGIC SCHOOL BUS</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var consol = Factory.New<TConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "02012345675";

			var dummyTransport = consol.Transports.AddNew();
			dummyTransport.JW_VoyageFlight = "11224";

			var matchingTransport = consol.Transports.AddNew();
			matchingTransport.JW_VoyageFlight = "11224";
			matchingTransport.JW_Vessel = "MAGIC SCHOOL BUS";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(tranpsortLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(Transport), logParent.GetType());

			var logParentTransport = logParent as Transport;

			AssertNotNull("logParentTransport", logParentTransport);
			AssertEquals("Should get best matching Transport.", GetHumanReadableID(matchingTransport), GetHumanReadableID(logParentTransport));
		}

		public void TestLinkingIncomingEventToAnIndividualContainerWithMBOLContainerNumberAndVoyageNumberAndFallsBackToLegWhenCannotMatchContainer()
		{
			const string containerLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
				</Context>
				<Context>
				  <Type>VoyageNumber</Type>
				  <Value>11224</Value>
				</Context>
				<Context>
				  <Type>VesselName</Type>
				  <Value>MAGIC SCHOOL BUS</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var consol = Factory.New<TConsol>();

			var matchingContainer = Factory.New<TContainer>();
			matchingContainer.JC_ContainerNum = "20257654321";

			consol.Containers.Add(matchingContainer);
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "02012345675";

			var matchingTransport = consol.Transports.AddNew();
			matchingTransport.JW_VoyageFlight = "11224";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(Transport), logParent.GetType());

			var logParentLeg = logParent as Transport;

			AssertNotNull("logParentTransport", logParentLeg);
			AssertEquals("Should get best matching Transport.", GetHumanReadableID(matchingTransport), GetHumanReadableID(logParentLeg));
		}

		public void TestLinkingIncomingEventToAnIndividualLegonShipmentWithMBOLAndHBOLAndVoyageNumberAndFallsBackToLegOnConsolWhenDoesNotMatchATransportOnShipment()
		{
			const string containerLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
				</Context>
				<Context>
				  <Type>HBOLNumber</Type>
				  <Value>11111111111</Value>
				</Context>
				<Context>
				  <Type>VoyageNumber</Type>
				  <Value>11224</Value>
				</Context>
				<Context>
				  <Type>VesselName</Type>
				  <Value>MAGIC SCHOOL BUS</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var consol = Factory.New<TConsol>();

			var shipment = Factory.New<TShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "11111111111";

			var matchingTransport1 = shipment.Transports.AddNew();
			matchingTransport1.JW_VoyageFlight = "11224";

			var matchingTransport2 = consol.Transports.AddNew();
			matchingTransport2.JW_VoyageFlight = "11224";

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "02012345675";
			shipment.Consols.Add(consol);

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(Transport), logParent.GetType());

			var logParentLeg = logParent as Transport;

			AssertNotNull("logParentLeg", logParentLeg);
			AssertEquals("Should get best matching Transport.", GetHumanReadableID(matchingTransport1), GetHumanReadableID(logParentLeg));

			shipment.Transports.RemoveAndDeleteAll();

			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(Transport), logParent.GetType());

			logParentLeg = logParent as Transport;

			AssertNotNull("logParentLeg", logParentLeg);
			AssertEquals("Should get best matching Transport.", GetHumanReadableID(matchingTransport2), GetHumanReadableID(logParentLeg));
		}

		public void TestLinkingIncomingEventToAnIndividualLegOnShipmentWithMBOLAndHBOLWhenMBOLNumberAndMAWBNumberAreBothSpecified()
		{
			const string containerLevelEventXmlText = @"
			<UniversalEvent>
				<Event>
					<EventType>CCD</EventType>
					<EventTime>10-JUL-2010 18:00</EventTime>
					<EventReference>Dummy Description</EventReference>
					<DataProvider>Dummy</DataProvider>
					<ContextCollection>
						<Context>
							<Type>MBOLNumber</Type>
							<Value>02012345675</Value>
						</Context>
						<Context>
							<Type>MAWBNumber</Type>
							<Value>02012345675</Value>
						</Context>
						<Context>
							<Type>HBOLNumber</Type>
							<Value>11111111111</Value>
						</Context>
						<Context>
							<Type>VoyageNumber</Type>
							<Value>11224</Value>
						</Context>
						<Context>
							<Type>VesselName</Type>
							<Value>MAGIC SCHOOL BUS</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>";

			var consol = Factory.New<TConsol>();

			var shipment = Factory.New<TShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "11111111111";

			var matchingTransport1 = shipment.Transports.AddNew();
			matchingTransport1.JW_VoyageFlight = "11224";

			var matchingTransport2 = consol.Transports.AddNew();
			matchingTransport2.JW_VoyageFlight = "11224";

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "02012345675";
			shipment.Consols.Add(consol);

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(Transport), logParent.GetType());

			var logParentLeg = logParent as Transport;

			AssertNotNull("logParentLeg", logParentLeg);
			AssertEquals("Should get best matching Transport.", GetHumanReadableID(matchingTransport1), GetHumanReadableID(logParentLeg));
		}

		public void TestRoutingLegLevelEventsGetLoggedAgainstTheLegWhenThereIsAFlightNumber()
		{
			const string legLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MAWBNumber</Type>
				  <Value>020-12345675</Value>
				</Context>
				<Context>
				  <Type>IATACarrierCode</Type>
				  <Value>LH</Value>
				</Context>
				<Context>
				  <Type>FlightNumber</Type>
				  <Value>LH451</Value>
				</Context>
				<Context>
				  <Type>IATAAirportCode</Type>
				  <Value>FRA</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var consol = Factory.New<TConsol>();

			var leg = consol.Transports[0];
			leg.JW_LegOrder = 1;
			leg.JW_RL_NKLoadPort = "USGLE";
			leg.JW_RL_NKDiscPort = "BEMLT";
			leg.JW_TransportMode = Constants.TransportModes.Air;
			leg.JW_TransportType = Constants.TransportPlanningType.Flight1;
			leg.JW_VoyageFlight = "LH451";

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "02012345675";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(legLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(Transport), logParent.GetType());

			var logParentLeg = logParent as Transport;

			AssertNotNull("logParentLeg", logParentLeg);
			AssertEquals("Should get best matching Leg.", GetHumanReadableID(leg), GetHumanReadableID(logParentLeg));
		}

		public void TestLinkingIncomingEventToAnIndividualLeg_FlightNumberBeatsNoMatch()
		{
			var consol = Factory.New<TConsol>();
			var firstLeg = consol.Transports[0];
			firstLeg.JW_LegOrder = 1;
			firstLeg.JW_RL_NKLoadPort = "USGLE";
			firstLeg.JW_RL_NKDiscPort = "AUSYD";
			firstLeg.JW_TransportMode = Constants.TransportModes.Air;
			firstLeg.JW_VoyageFlight = "LH1234";

			var secondLeg = consol.Transports.AddNew();
			secondLeg.JW_LegOrder = 2;
			secondLeg.JW_RL_NKLoadPort = "AUSYD";
			secondLeg.JW_RL_NKDiscPort = "AUMEL";
			secondLeg.JW_TransportMode = Constants.TransportModes.Air;
			secondLeg.JW_VoyageFlight = "SD1234";

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "02012345675";
			Factory.Save();

			var cCDEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MAWBNumber</Type>
				  <Value>020-12345675</Value>
				</Context>
				<Context>
				  <Type>IATACarrierCode</Type>
				  <Value>SD</Value>
				</Context>
				<Context>
				  <Type>FlightNumber</Type>
				  <Value>SD1234</Value>
				</Context>
				<Context>
				  <Type>IATAAirportCode</Type>
				  <Value>FRA</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>
			".Trim();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(cCDEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			Assert("logParents would not be null", !logParents.Any(parent => parent == null));
			AssertEquals("logParents has one Transport", 1, logParents.Count(parent => parent is Transport));

			var logParentTransport = logParents.First(parent => parent is Transport) as Transport;
			AssertNotNull("logParentLeg", logParentTransport);
			AssertEquals("Should get second transport as it matched flight number.", GetHumanReadableID(secondLeg), GetHumanReadableID(logParentTransport));
		}

		public void TestLinkingIncomingEventToAnIndividualLeg_OriginIATAAirportCodeBeatsFlightNumber()
		{
			var consol = Factory.New<TConsol>();
			var firstLeg = consol.Transports[0];
			firstLeg.JW_LegOrder = 1;
			firstLeg.JW_RL_NKLoadPort = "USGLE";
			firstLeg.JW_RL_NKDiscPort = "DEFRA";
			firstLeg.JW_TransportMode = Constants.TransportModes.Air;
			firstLeg.JW_VoyageFlight = "LH1234";

			var secondLeg = consol.Transports.AddNew();
			secondLeg.JW_LegOrder = 2;
			secondLeg.JW_RL_NKLoadPort = "AUSYD";
			secondLeg.JW_RL_NKDiscPort = "AUMEL";
			secondLeg.JW_TransportMode = Constants.TransportModes.Air;
			secondLeg.JW_VoyageFlight = "SD1234";

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "02012345675";
			Factory.Save();

			var cCDEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MAWBNumber</Type>
				  <Value>020-12345675</Value>
				</Context>
				<Context>
				  <Type>IATACarrierCode</Type>
				  <Value>SD</Value>
				</Context>
				<Context>
				  <Type>FlightNumber</Type>
				  <Value>SD1234</Value>
				</Context>
				<Context>
				  <Type>IATAAirportCode</Type>
				  <Value>FRA</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>
			".Trim();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(cCDEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			Assert("logParents would not be null", !logParents.Any(parent => parent == null));
			AssertEquals("logParents has one Transport", 1, logParents.Count(parent => parent is Transport));

			var logParentTransport = logParents.First(parent => parent is Transport) as Transport;
			AssertNotNull("logParentLeg", logParentTransport);
			AssertEquals("Should get first transport as it matched Origin Airport Code.", GetHumanReadableID(firstLeg), GetHumanReadableID(logParentTransport));
		}

		public void TestLinkingIncomingEventToAnIndividualLeg_DestinationIATAAirportCodeBeatsOriginIATAAirportCode()
		{
			var consol = Factory.New<TConsol>();
			var firstLeg = consol.Transports[0];
			firstLeg.JW_LegOrder = 1;
			firstLeg.JW_RL_NKLoadPort = "DEFRA";
			firstLeg.JW_RL_NKDiscPort = "USGLE";
			firstLeg.JW_TransportMode = Constants.TransportModes.Air;
			firstLeg.JW_VoyageFlight = "LH1234";

			var secondLeg = consol.Transports.AddNew();
			secondLeg.JW_LegOrder = 2;
			secondLeg.JW_RL_NKLoadPort = "USGLE";
			secondLeg.JW_RL_NKDiscPort = "DEFRA";
			secondLeg.JW_TransportMode = Constants.TransportModes.Air;
			secondLeg.JW_VoyageFlight = "LH1111";

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "02012345675";
			Factory.Save();

			var cCDEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MAWBNumber</Type>
				  <Value>020-12345675</Value>
				</Context>
				<Context>
				  <Type>IATACarrierCode</Type>
				  <Value>SD</Value>
				</Context>
				<Context>
				  <Type>FlightNumber</Type>
				  <Value>SD1234</Value>
				</Context>
				<Context>
				  <Type>IATAAirportCode</Type>
				  <Value>FRA</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>
			".Trim();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(cCDEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			Assert("logParents would not be null", !logParents.Any(parent => parent == null));
			AssertEquals("logParents has one Transport", 1, logParents.Count(parent => parent is Transport));

			var logParentTransport = logParents.First(parent => parent is Transport) as Transport;
			AssertNotNull("logParentLeg", logParentTransport);
			AssertEquals("Should get second transport as it matched Destination Airport Code.", GetHumanReadableID(secondLeg), GetHumanReadableID(logParentTransport));
		}

		public void TestLinkingIncomingEventToAnIndividualLeg_DestinationCodeAndOriginCodeBeatsDestinationIATAAirportCode()
		{
			var consol = Factory.New<TConsol>();
			var firstLeg = consol.Transports[0];
			firstLeg.JW_LegOrder = 1;
			firstLeg.JW_RL_NKLoadPort = "AUSYD";
			firstLeg.JW_RL_NKDiscPort = "DEFRA";
			firstLeg.JW_TransportMode = Constants.TransportModes.Air;
			firstLeg.JW_VoyageFlight = "LH1234";

			var secondLeg = consol.Transports.AddNew();
			secondLeg.JW_LegOrder = 2;
			secondLeg.JW_RL_NKLoadPort = "USGLE";
			secondLeg.JW_RL_NKDiscPort = "DEFRA";
			secondLeg.JW_TransportMode = Constants.TransportModes.Air;
			secondLeg.JW_VoyageFlight = "LH1111";

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "02012345675";
			Factory.Save();

			var cCDEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MAWBNumber</Type>
				  <Value>020-12345675</Value>
				</Context>
				<Context>
				  <Type>IATACarrierCode</Type>
				  <Value>SD</Value>
				</Context>
				<Context>
				  <Type>FlightNumber</Type>
				  <Value>SD1234</Value>
				</Context>
				<Context>
				  <Type>OriginIATAAirportCode</Type>
				  <Value>GLE</Value>
				</Context>
				<Context>
				  <Type>DestinationIATAAirportCode</Type>
				  <Value>FRA</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>
			".Trim();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(cCDEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			Assert("logParents would not be null", !logParents.Any(parent => parent == null));
			AssertEquals("logParents has one Transport", 1, logParents.Count(parent => parent is Transport));

			var logParentTransport = logParents.First(parent => parent is Transport) as Transport;
			AssertNotNull("logParentLeg", logParentTransport);
			AssertEquals("Should get second transport as it matched Destination Airport Code and Origin Airport Code.", GetHumanReadableID(secondLeg), GetHumanReadableID(logParentTransport));
		}

		public void TestLinkingIncomingEventToAnIndividualLeg_FlightNumAndDestinationCodeAndOriginCodeBeatsFlightNumberAndDestinationCode()
		{
			var consol = Factory.New<TConsol>();
			var firstLeg = consol.Transports[0];
			firstLeg.JW_LegOrder = 1;
			firstLeg.JW_RL_NKLoadPort = "USGLE";
			firstLeg.JW_RL_NKDiscPort = "AUMEL";
			firstLeg.JW_TransportMode = Constants.TransportModes.Air;
			firstLeg.JW_VoyageFlight = "SD1234";

			var secondLeg = consol.Transports.AddNew();
			secondLeg.JW_LegOrder = 2;
			secondLeg.JW_RL_NKLoadPort = "USGLE";
			secondLeg.JW_RL_NKDiscPort = "DEFRA";
			secondLeg.JW_TransportMode = Constants.TransportModes.Air;
			secondLeg.JW_VoyageFlight = "SD1234";

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "02012345675";
			Factory.Save();

			var cCDEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MAWBNumber</Type>
				  <Value>020-12345675</Value>
				</Context>
				<Context>
				  <Type>IATACarrierCode</Type>
				  <Value>SD</Value>
				</Context>
				<Context>
				  <Type>FlightNumber</Type>
				  <Value>SD1234</Value>
				</Context>
				<Context>
				  <Type>OriginIATAAirportCode</Type>
				  <Value>GLE</Value>
				</Context>
				<Context>
				  <Type>DestinationIATAAirportCode</Type>
				  <Value>FRA</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>
			".Trim();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(cCDEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			Assert("logParents would not be null", !logParents.Any(parent => parent == null));
			AssertEquals("logParents has one Transport", 1, logParents.Count(parent => parent is Transport));

			var logParentTransport = logParents.First(parent => parent is Transport) as Transport;
			AssertNotNull("logParentLeg", logParentTransport);
			AssertEquals("Should get second transport as it matched Destination Airport Code and Flight Number.", GetHumanReadableID(secondLeg), GetHumanReadableID(logParentTransport));
		}

		public void TestLinkingIncomingEventToAnIndividualLeg_DestinationCodeAndOriginCodeWorksWithoutFlightNumber()
		{
			var consol = Factory.New<TConsol>();
			var firstLeg = consol.Transports[0];
			firstLeg.JW_LegOrder = 1;
			firstLeg.JW_RL_NKLoadPort = "USGLE";
			firstLeg.JW_RL_NKDiscPort = "AUMEL";
			firstLeg.JW_TransportMode = Constants.TransportModes.Air;
			firstLeg.JW_VoyageFlight = "SD1234";

			var secondLeg = consol.Transports.AddNew();
			secondLeg.JW_LegOrder = 2;
			secondLeg.JW_RL_NKLoadPort = "USGLE";
			secondLeg.JW_RL_NKDiscPort = "DEFRA";
			secondLeg.JW_TransportMode = Constants.TransportModes.Air;
			secondLeg.JW_VoyageFlight = "SD1234";

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "02021345675";
			Factory.Save();

			var cCDEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MAWBNumber</Type>
				  <Value>020-21345675</Value>
				</Context>
				<Context>
				  <Type>OriginIATAAirportCode</Type>
				  <Value>GLE</Value>
				</Context>
				<Context>
				  <Type>DestinationIATAAirportCode</Type>
				  <Value>FRA</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>
			".Trim();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(cCDEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			Assert("logParents would not be null", !logParents.Any(parent => parent == null));
			AssertEquals("logParents has one Transport", 1, logParents.Count(parent => parent is Transport));

			var logParentTransport = logParents.First(parent => parent is Transport) as Transport;
			AssertNotNull("logParentLeg", logParentTransport);
			AssertEquals("Should get second transport as it matched Destination Airport Code and Flight Number.", GetHumanReadableID(secondLeg), GetHumanReadableID(logParentTransport));
		}

		public void TestLastArrivalEventGetsLoggedAgainstTheFirstLegIfThereIsNoMatchingLegAndAllLegsHaveAnArrivalEvent()
		{
			var consol = Factory.New<TConsol>();
			var firstLeg = consol.Transports[0];
			firstLeg.JW_LegOrder = 1;
			firstLeg.JW_RL_NKLoadPort = "USGLE";
			firstLeg.JW_RL_NKDiscPort = "DEFRA";
			firstLeg.JW_TransportMode = Constants.TransportModes.Air;
			firstLeg.JW_VoyageFlight = "LH451";
			firstLeg.Logs.AddNew(Events.Arrival, ZDateTimeOffset.Now);

			var secondLeg = consol.Transports.AddNew();
			secondLeg.JW_LegOrder = 2;
			secondLeg.JW_RL_NKLoadPort = "DEFRA";
			secondLeg.JW_RL_NKDiscPort = "USNLN";
			secondLeg.JW_TransportMode = Constants.TransportModes.Air;
			secondLeg.JW_VoyageFlight = "SD1050";
			secondLeg.Logs.AddNew(Events.Arrival, ZDateTimeOffset.Now);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "02012345675";
			Factory.Save();

			var aRVEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>ARV</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MAWBNumber</Type>
				  <Value>020-12345675</Value>
				</Context>
				<Context>
				  <Type>IATACarrierCode</Type>
				  <Value>SD</Value>
				</Context>
				<Context>
				  <Type>FlightNumber</Type>
				  <Value>SD1234</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>
			".Trim();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(aRVEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParentLeg = logParents.First() as Transport;

			AssertNotNull("logParent should be a Transport Leg", logParentLeg);

			AssertEquals("Should get First Leg.", GetHumanReadableID(firstLeg), GetHumanReadableID(logParentLeg));
		}

		public void TestEventUnmatchableToALegButWithAFlightNumberGetsLoggedAgainstTheFirstLegThatDoesNotYetHaveThisEvent()
		{
			var consol = Factory.New<TConsol>();
			var firstLeg = consol.Transports[0];
			firstLeg.JW_LegOrder = 1;
			firstLeg.JW_RL_NKLoadPort = "USGLE";
			firstLeg.JW_RL_NKDiscPort = "DEFRA";
			firstLeg.JW_TransportMode = Constants.TransportModes.Air;
			firstLeg.JW_VoyageFlight = "LH451";
			firstLeg.Logs.AddNew(Events.CargoCheckinDiscrepancy, ZDateTimeOffset.Now);

			var secondLeg = consol.Transports.AddNew();
			secondLeg.JW_LegOrder = 2;
			secondLeg.JW_RL_NKLoadPort = "DEFRA";
			secondLeg.JW_RL_NKDiscPort = "USNLN";
			secondLeg.JW_TransportMode = Constants.TransportModes.Air;
			secondLeg.JW_VoyageFlight = "SD1050";

			var lastLeg = consol.Transports.AddNew();
			lastLeg.JW_LegOrder = 3;
			lastLeg.JW_RL_NKLoadPort = "USNLN";
			lastLeg.JW_RL_NKDiscPort = "AUSYD";
			lastLeg.JW_TransportMode = Constants.TransportModes.Air;
			lastLeg.JW_VoyageFlight = "SD2222";

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "02012345675";
			Factory.Save();

			var aRVEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MAWBNumber</Type>
				  <Value>020-12345675</Value>
				</Context>
				<Context>
				  <Type>IATACarrierCode</Type>
				  <Value>SD</Value>
				</Context>
				<Context>
				  <Type>FlightNumber</Type>
				  <Value>SD1234</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>
			".Trim();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(aRVEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match two LogParents", 1, logParents.Length);

			Assert("logParents would not be null", !logParents.Any(parent => parent == null));
			AssertEquals("logParents has one transport", 1, logParents.Count(parent => parent is Transport));

			var logParentLeg = logParents.First(parent => parent is Transport) as Transport;

			AssertNotNull("logParentLeg", logParentLeg);
			AssertEquals("Should get second leg as it is the first leg that do not have CCD event.", GetHumanReadableID(secondLeg), GetHumanReadableID(logParentLeg));
		}

		public void TestMatchBasicEventToConsolUsingMAWBOnly()
		{
			var matchingConsol = Factory.New<TConsol>();
			matchingConsol.JK_TransportMode = Constants.TransportModes.Air;
			matchingConsol.JK_MasterBillNum = "02012345675";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();

			var xmlEvent = eventDeserializer.Parse(ConsolLevelEventXmlText);

			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];

			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TConsol), logParent.GetType());

			var logParentConsol = logParent as TConsol;

			AssertNotNull("logParentConsol", logParentConsol);
			AssertEquals("Should get best matching Consol.", GetHumanReadableID(matchingConsol), GetHumanReadableID(logParentConsol));
		}

		public void TestLinkingIncomingEventToAnIndividualShipmentWithMAWBAndHAWBAndDoesNotFallBackToConsolShipmentCannotBeMatched()
		{
			const string shipmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MAWBNumber</Type>
				  <Value>020-12345675</Value>
				</Context>
				<Context>
				  <Type>HAWBNumber</Type>
				  <Value>20257654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingConsol = Factory.New<TConsol>();

			var matchingShipment = Factory.New<TShipment>();
			matchingShipment.JS_TransportMode = Constants.TransportModes.Air;
			matchingShipment.JS_HouseBill = "20257654321";

			matchingConsol.GridShipments.Add(matchingShipment);
			matchingConsol.JK_TransportMode = Constants.TransportModes.Air;
			matchingConsol.JK_MasterBillNum = "02012345675";

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TShipment), logParent.GetType());

			var logParentShipment = logParent as TShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));

			matchingConsol.GridShipments.RemoveAndDeleteAll();

			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("logParents", logParents);
		}

		public void TestMatchBasicEventToConsolUsingMBOLOnly()
		{
			const string consolLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingConsol = Factory.New<TConsol>();

			matchingConsol.JK_TransportMode = Constants.TransportModes.Sea;
			matchingConsol.JK_MasterBillNum = "02012345675";

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(consolLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TConsol), logParent.GetType());

			var logParentConsol = logParent as TConsol;

			AssertNotNull("logParentConsol", logParentConsol);
			AssertEquals("Should get best matching Consol.", GetHumanReadableID(matchingConsol), GetHumanReadableID(logParentConsol));
		}

		public void TestLinkingIncomingEventToAnIndividualShipmentWithMBOLAndHBOLAndDoesNotFallBackToConsolIfShipmentCannotBeMatched()
		{
			const string shipmentLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
				</Context>
				<Context>
				  <Type>HBOLNumber</Type>
				  <Value>20257654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingConsol = Factory.New<TConsol>();

			var matchingShipment = Factory.New<TShipment>();
			matchingShipment.JS_TransportMode = Constants.TransportModes.Sea;
			matchingShipment.JS_HouseBill = "20257654321";

			matchingConsol.GridShipments.Add(matchingShipment);
			matchingConsol.JK_TransportMode = Constants.TransportModes.Sea;
			matchingConsol.JK_MasterBillNum = "02012345675";

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TShipment), logParent.GetType());

			var logParentShipment = logParent as TShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));

			matchingConsol.GridShipments.RemoveAndDeleteAll();

			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("logParents", logParents);
		}

		public void TestItShouldMatchConsolThatWasLastCreated()
		{
			var oldConsol = Factory.New<TConsol>();
			oldConsol.JK_TransportMode = Constants.TransportModes.Air;
			oldConsol.JK_MasterBillNum = "02012345675";
			oldConsol.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-10);
			oldConsol.JK_AgentType = Constants.AgentType.CoLoad;

			var newConsol = Factory.New<TConsol>();
			newConsol.JK_TransportMode = Constants.TransportModes.Air;
			newConsol.JK_MasterBillNum = "02012345675";
			newConsol.JK_SystemCreateTimeUtc = ZDateTime.Now;
			newConsol.JK_AgentType = Constants.AgentType.CoLoad;
			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();

			var xmlEvent = eventDeserializer.Parse(ConsolLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TConsol), logParent.GetType());

			var logParentConsol = logParent as TConsol;

			AssertNotNull("logParentConsol", logParentConsol);
			AssertEquals("Should get Consol with latest ETD.", newConsol.PK, logParentConsol.PK);
		}

		public void TestLinkingIncomingEventToAnIndividualContainerWithMBOLAndContainerNumberAndFallsBackToConsolWhenCannotMatchContainer()
		{
			const string containerLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingConsol = Factory.New<TConsol>();

			var matchingContainer = Factory.New<TContainer>();
			matchingContainer.JC_ContainerNum = "20257654321";

			matchingConsol.Containers.Add(matchingContainer);
			matchingConsol.JK_TransportMode = Constants.TransportModes.Sea;
			matchingConsol.JK_MasterBillNum = "02012345675";

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);

			matchingConsol.Containers.RemoveAndDeleteAll();

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TConsol), logParent.GetType());

			var logParentConsol = logParent as TConsol;

			AssertNotNull("logParentConsol", logParentConsol);
			AssertEquals("Should get best matching Consol.", GetHumanReadableID(matchingConsol), GetHumanReadableID(logParentConsol));
		}

		public void TestItShouldNotMatchSeaConsol()
		{
			var matchingConsol = Factory.New<TConsol>();
			matchingConsol.JK_TransportMode = Constants.TransportModes.Sea;
			matchingConsol.JK_MasterBillNum = "02012345675";

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();

			var xmlEvent = eventDeserializer.Parse(ConsolLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent);

			AssertNull("logParent", logParent);
		}

		public void TestItShouldNotMatchConsolWithDifferentMasterBillNum()
		{
			var matchingConsol = Factory.New<TConsol>();
			matchingConsol.JK_TransportMode = Constants.TransportModes.Sea;
			matchingConsol.JK_MasterBillNum = "11111111115";

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();

			var xmlEvent = eventDeserializer.Parse(ConsolLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("logParent", logParent);
		}

		public void TestItShouldNotMatchConsol_WhenEventTypeIsSBR()
		{
			const string eventText = @"
<UniversalEvent>
	<Event>
		<EventTime>2023-12-21T08:48:56.133</EventTime>
		<EventType>SBR</EventType>
		<EventParameters>
			<MessageType>Container Tracking</MessageType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>Z103439AKL</Value>
			</Context>
			<Context>
				<Type>CarriersBookingReference</Type>
				<Value>Z103439AKL</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var consol = Factory.New<TConsol>();
			consol.JK_BookingReference = "Z103439AKL";

			var subscriber = GetNewConsolEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("Should not match consol because the event type is SBR", logParents);
		}

		public void TestIncomingEventDoesNotCreateContainersForMultipleConsols()
		{
			const string eventText = @"
<UniversalEvent>
	<Event>
		<EventTime>2019-01-18T11:28</EventTime>
		<EventType>STU</EventType>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
		<Context>
			<Type>CarriersBookingReference</Type>
			<Value>CBR123</Value>
		</Context>
		<Context>
			<Type>MBOLNumber</Type>
			<Value>M123456789</Value>
		</Context>
		<Context>
			<Type>ContainerNumber</Type>
			<Value>C123456</Value>
		</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var consol1 = Factory.New<TConsol>();
			consol1.JK_BookingReference = "CBR123";
			consol1.JK_MasterBillNum = "M123456789";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Today;

			var consol2 = Factory.New<TConsol>();
			consol2.JK_BookingReference = "CBR123";
			consol2.JK_MasterBillNum = "M123456789";
			consol2.JK_SystemCreateTimeUtc = ZDateTime.Today.AddDays(1);

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("Should not match LogParent", logParents);
		}

		public void TestIncomingEventDoesNotDefaultWeightsOnChangingContainerCount()
		{
			const string eventText = @"
<UniversalEvent>
   <Event>
      <EventTime>2019-01-18T11:28</EventTime>
      <EventType>STU</EventType>
      <IsEstimate>false</IsEstimate>
      <ContextCollection>
         <Context>
            <Type>CarriersBookingReference</Type>
            <Value>CBR123</Value>
         </Context>
         <Context>
            <Type>MBOLNumber</Type>
            <Value>MBL84624</Value>
         </Context>
         <Context>
            <Type>ContainerNumber</Type>
            <Value>C136</Value>
         </Context>
		 <Context>
			<Type>ContainerISOCode</Type>
			<Value>22P1</Value>
		 </Context>
      </ContextCollection>
   </Event>
</UniversalEvent>
";
			var consol = Factory.New<TConsol>();
			consol.JK_BookingReference = "CBR123";
			consol.JK_MasterBillNum = "MBL84624";
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "TST1";
			refContainer.RC_Length = 41m;
			refContainer.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;
			refContainer.RC_ISOType = "22P1";
			refContainer.RC_TareWeight = 20;

			var container = consol.Containers.AddNew();
			container.JC_ContainerCount = 5;
			container.JC_RC = refContainer.PK;

			container.JC_TareWeight = 200;

			AssertEquals("Precondition: Overridden container tare weight should be 200kg", 200m, container.JC_TareWeight);
			AssertEquals("Precondition: Container calculated tare weight should be 100kg", 100m, container.JC_Calc_TareWeight);

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventText);

			subscriber.GetLogParentsForEvent(xmlEvent);

			AssertEquals("Consol has a new container added", 2, consol.Containers.Count);
			AssertEquals("Container Count of original container should be decreased", (short)4, container.JC_ContainerCount);
			AssertEquals("Overridden container tare weight should not be defaulted back", 200m, container.JC_TareWeight);
		}

		public void TestIncomingEventDoesntLinkToContainerWithNonTConsolWhenUsingContainerNumberOnly()
		{
			const string containerLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ContainerNumber</Type>
				  <Value>20257654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var cfsConsol = (CommonConsol)Factory.New<Integration.CFS.ICFSLoadListConsol>();

			cfsConsol.JK_TransportMode = Constants.TransportModes.Sea;
			cfsConsol.JK_MasterBillNum = "02012345675";

			var cfsContainer = cfsConsol.Containers.AddNew();
			cfsContainer.JC_ContainerNum = "20257654321";

			var shipment = cfsConsol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			cfsContainer.PackLines.Add(packLine);

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("Should not match LogParents", logParents);
		}

		public void TestMatchWhenMBOLAndMAWBAreSpecified_Sea()
		{
			const string consolLevelEventXmlText = @"
			<UniversalEvent>
				<Event>
					<EventType>CCD</EventType>
					<EventTime>10-JUL-2010 18:00</EventTime>
					<EventReference>Dummy Description</EventReference>
					<DataProvider>Dummy</DataProvider>
					<ContextCollection>
						<Context>
							<Type>MBOLNumber</Type>
							<Value>02012345675</Value>
						</Context>
						<Context>
							<Type>MAWBNumber</Type>
							<Value>02012345675</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>";

			var matchingConsol = Factory.New<TConsol>();

			matchingConsol.JK_TransportMode = Constants.TransportModes.Sea;
			matchingConsol.JK_MasterBillNum = "02012345675";

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(consolLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TConsol), logParent.GetType());

			var logParentConsol = logParent as TConsol;

			AssertNotNull("logParentConsol", logParentConsol);
			AssertEquals("Should get best matching Consol.", GetHumanReadableID(matchingConsol), GetHumanReadableID(logParentConsol));
		}

		public void TestMatchWhenMBOLAndMAWBAreSpecified_Air()
		{
			const string consolLevelEventXmlText = @"
			<UniversalEvent>
				<Event>
					<EventType>CCD</EventType>
					<EventTime>10-JUL-2010 18:00</EventTime>
					<EventReference>Dummy Description</EventReference>
					<DataProvider>Dummy</DataProvider>
					<ContextCollection>
						<Context>
							<Type>MBOLNumber</Type>
							<Value>02012345675</Value>
						</Context>
						<Context>
							<Type>MAWBNumber</Type>
							<Value>02012345675</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>";

			var matchingConsol = Factory.New<TConsol>();

			matchingConsol.JK_TransportMode = Constants.TransportModes.Air;
			matchingConsol.JK_MasterBillNum = "02012345675";
			matchingConsol.JK_SystemCreateTimeUtc = ZDateTime.Now;

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(consolLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TConsol), logParent.GetType());

			var logParentConsol = logParent as TConsol;

			AssertNotNull("logParentConsol", logParentConsol);
			AssertEquals("Should get best matching Consol.", GetHumanReadableID(matchingConsol), GetHumanReadableID(logParentConsol));
		}

		public void TestMatchWhenMBOLAndMAWBAreSpecified_AirIsMatchedOverSea()
		{
			const string consolLevelEventXmlText = @"
			<UniversalEvent>
				<Event>
					<EventType>CCD</EventType>
					<EventTime>10-JUL-2010 18:00</EventTime>
					<EventReference>Dummy Description</EventReference>
					<DataProvider>Dummy</DataProvider>
					<ContextCollection>
						<Context>
							<Type>MBOLNumber</Type>
							<Value>02012345675</Value>
						</Context>
						<Context>
							<Type>MAWBNumber</Type>
							<Value>02012345675</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>";

			var seaConsol = Factory.New<TConsol>();
			seaConsol.JK_TransportMode = Constants.TransportModes.Sea;
			seaConsol.JK_MasterBillNum = "02012345675";
			seaConsol.JK_SystemCreateTimeUtc = ZDateTime.Now;

			var airConsol = Factory.New<TConsol>();
			airConsol.JK_TransportMode = Constants.TransportModes.Air;
			airConsol.JK_MasterBillNum = "02012345675";
			airConsol.JK_SystemCreateTimeUtc = ZDateTime.Now;

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(consolLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TConsol), logParent.GetType());

			var logParentConsol = logParent as TConsol;

			AssertNotNull("logParentConsol", logParentConsol);
			AssertEquals("Air consol matching MAWB Number is chosen over Sea consol matching MBOL Number", GetHumanReadableID(airConsol), GetHumanReadableID(logParentConsol));
		}

		public void TestMatchForConsolShouldBeBasedOnTransportMode_WhenEventHasTransportMode()
		{
			const string tranpsortLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventParameters>
				  <TransportMode>SEA</TransportMode>
			  </EventParameters>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
				</Context>
				<Context>
				  <Type>VoyageNumber</Type>
				  <Value>11224</Value>
				</Context>
				<Context>
				  <Type>VesselName</Type>
				  <Value>MAGIC SCHOOL BUS</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var consol = Factory.New<TConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "02012345675";

			var dummyTransport = consol.Transports.AddNew();
			dummyTransport.JW_VoyageFlight = "11224";
			dummyTransport.JW_Vessel = "MAGIC SCHOOL BUS";
			dummyTransport.JW_TransportMode = Core.Constants.TransportModes.Air;

			var matchingTransport = consol.Transports.AddNew();
			matchingTransport.JW_VoyageFlight = "11224";
			matchingTransport.JW_Vessel = "MAGICSCHOOLBUS";
			matchingTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(tranpsortLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(Transport), logParent.GetType());

			var logParentTransport = logParent as Transport;

			AssertNotNull("logParentTransport", logParentTransport);
			AssertEquals("Should get best matching container.", GetHumanReadableID(matchingTransport), GetHumanReadableID(logParentTransport));
		}

		public void TestMatchForConsolShouldLinkEventToSpecifiedContainer_WhenEventTransportModeIsRail_ConsolIsSeaAndHasContainers()
		{
			const string tranpsortLevelEventXmlText = @"
	<UniversalEvent>
		<Event>
			  <EventType>DEP</EventType>
			  <EventParameters>
				  <TransportMode>RAI</TransportMode>
				  <Facility>CTO</Facility>
				  <Location>AUSYD</Location>
			  </EventParameters>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <IsEstimate>false</IsEstimate>
			  <ContextCollection>
				<Context>
				  <Type>ContainerNumber</Type>
				  <Value>OOLU4422850</Value>
				</Context>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
				</Context>
				<Context>
				  <Type>VoyageNumber</Type>
				  <Value>11224</Value>
				</Context>
				<Context>
				  <Type>VesselName</Type>
				  <Value>MAGIC SCHOOL BUS</Value>
				</Context>
				<Context>
				  <Type>LegOriginUNLOCO</Type>
				  <Value>AUSYD</Value>
				</Context>
				<Context>
				  <Type>LegDestinationUNLOCO</Type>
				  <Value>SGSIN</Value>
				</Context>
				<Context>
				  <Type>ContainerISOCode</Type>
				  <Value>42G1</Value>
				</Context>
			  </ContextCollection>
		</Event>
	</UniversalEvent>";

			var consol = Factory.New<TConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "02012345675";

			var dummyTransport = consol.Transports.AddNew();
			dummyTransport.JW_VoyageFlight = "11224";
			dummyTransport.JW_Vessel = "MAGIC SCHOOL BUS";
			dummyTransport.JW_TransportMode = Core.Constants.TransportModes.Air;

			var matchingTransport = consol.Transports.AddNew();
			matchingTransport.JW_VoyageFlight = "11224";
			matchingTransport.JW_Vessel = "MAGICSCHOOLBUS";
			matchingTransport.JW_TransportMode = Core.Constants.TransportModes.Rail;
			matchingTransport.JW_RL_NKLoadPort = "AUSYD";
			matchingTransport.JW_RL_NKDiscPort = "SGSIN";

			var dummyContainer = consol.Containers.AddNew();
			dummyContainer.JC_ContainerNum = "IRSU7865467";

			var matchingContainer = consol.Containers.AddNew();
			matchingContainer.JC_ContainerNum = "OOLU4422850";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(tranpsortLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);

			var logParentContainer = logParent as CommonContainer;

			AssertNotNull("logParentTransport", logParentContainer);
			AssertEquals("Should get best matching container.", "OOLU4422850", logParentContainer.JC_ContainerNum);
		}

		public void TestMatchForConsolShouldLinkEventToSpecifiedContainer_WhenEventTransportModeIsRoad_ConsolIsSeaAndHasContainers()
		{
			const string tranpsortLevelEventXmlText = @"
	<UniversalEvent>
		<Event>
			  <EventType>DEP</EventType>
			  <EventParameters>
				  <TransportMode>ROA</TransportMode>
				  <Facility>CTO</Facility>
				  <Location>AUSYD</Location>
			  </EventParameters>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <IsEstimate>false</IsEstimate>
			  <ContextCollection>
				<Context>
				  <Type>ContainerNumber</Type>
				  <Value>OOLU4422850</Value>
				</Context>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
				</Context>
				<Context>
				  <Type>VoyageNumber</Type>
				  <Value>11224</Value>
				</Context>
				<Context>
				  <Type>VesselName</Type>
				  <Value>MAGIC SCHOOL BUS</Value>
				</Context>
				<Context>
				  <Type>LegOriginUNLOCO</Type>
				  <Value>AUSYD</Value>
				</Context>
				<Context>
				  <Type>LegDestinationUNLOCO</Type>
				  <Value>SGSIN</Value>
				</Context>
				<Context>
				  <Type>ContainerISOCode</Type>
				  <Value>42G1</Value>
				</Context>
			  </ContextCollection>
		</Event>
	</UniversalEvent>";

			var consol = Factory.New<TConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "02012345675";

			var dummyTransport = consol.Transports.AddNew();
			dummyTransport.JW_VoyageFlight = "11224";
			dummyTransport.JW_Vessel = "MAGIC SCHOOL BUS";
			dummyTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;

			var matchingTransport = consol.Transports.AddNew();
			matchingTransport.JW_VoyageFlight = "11224";
			matchingTransport.JW_Vessel = "MAGICSCHOOLBUS";
			matchingTransport.JW_TransportMode = Core.Constants.TransportModes.Road;
			matchingTransport.JW_RL_NKLoadPort = "AUSYD";
			matchingTransport.JW_RL_NKDiscPort = "SGSIN";

			var dummyContainer = consol.Containers.AddNew();
			dummyContainer.JC_ContainerNum = "IRSU7865467";

			var matchingContainer = consol.Containers.AddNew();
			matchingContainer.JC_ContainerNum = "OOLU4422850";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(tranpsortLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);

			var logParentContainer = logParent as CommonContainer;

			AssertNotNull("logParentTransport", logParentContainer);
			AssertEquals("Should get best matching Transport.", "OOLU4422850", logParentContainer.JC_ContainerNum);
		}

		public void TestMatchForConsolShouldLinkEventToTransportLeg_WhenEventTransportModeIsSea()
		{
			const string tranpsortLevelEventXmlText = @"
	<UniversalEvent>
		<Event>
			  <EventType>DEP</EventType>
			  <EventParameters>
				  <TransportMode>SEA</TransportMode>
				  <Facility>CTO</Facility>
				  <Location>AUSYD</Location>
			  </EventParameters>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <IsEstimate>false</IsEstimate>
			  <ContextCollection>
				<Context>
				  <Type>ContainerNumber</Type>
				  <Value>OOLU4422850</Value>
				</Context>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
				</Context>
				<Context>
				  <Type>VoyageNumber</Type>
				  <Value>11224</Value>
				</Context>
				<Context>
				  <Type>VesselName</Type>
				  <Value>MAGIC SCHOOL BUS</Value>
				</Context>
				<Context>
				  <Type>LegOriginUNLOCO</Type>
				  <Value>AUSYD</Value>
				</Context>
				<Context>
				  <Type>LegDestinationUNLOCO</Type>
				  <Value>SGSIN</Value>
				</Context>
				<Context>
				  <Type>ContainerISOCode</Type>
				  <Value>42G1</Value>
				</Context>
			  </ContextCollection>
		</Event>
	</UniversalEvent>";

			var consol = Factory.New<TConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "02012345675";

			var dummyTransport = consol.Transports.AddNew();
			dummyTransport.JW_VoyageFlight = "11224";
			dummyTransport.JW_Vessel = "MAGIC SCHOOL BUS";
			dummyTransport.JW_TransportMode = Core.Constants.TransportModes.Air;

			var matchingTransport = consol.Transports.AddNew();
			matchingTransport.JW_VoyageFlight = "11224";
			matchingTransport.JW_Vessel = "MAGICSCHOOLBUS";
			matchingTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			matchingTransport.JW_RL_NKLoadPort = "AUSYD";
			matchingTransport.JW_RL_NKDiscPort = "SGSIN";

			var dummyContainer = consol.Containers.AddNew();
			dummyContainer.JC_ContainerNum = "IRSU7865467";

			var matchingContainer = consol.Containers.AddNew();
			matchingContainer.JC_ContainerNum = "OOLU4422850";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(tranpsortLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(Transport), logParent.GetType());

			var logParentTransport = logParent as Transport;

			AssertNotNull("logParentTransport", logParentTransport);
			AssertEquals("Should get best matching Transport.", GetHumanReadableID(matchingTransport), GetHumanReadableID(logParentTransport));
		}

		public void TestMatchForConsolShouldLinkEventToTransportLeg_WhenEventTransportModeIsAir()
		{
			const string tranpsortLevelEventXmlText = @"
	<UniversalEvent>
		<Event>
			  <EventType>DEP</EventType>
			  <EventParameters>
				  <TransportMode>AIR</TransportMode>
				  <Facility>CTO</Facility>
				  <Location>AUSYD</Location>
			  </EventParameters>
			  <EventTime>19-JUL-2012 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <IsEstimate>false</IsEstimate>
			  <ContextCollection>
				<Context>
				  <Type>ContainerNumber</Type>
				  <Value>OOLU4422850</Value>
				</Context>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
				</Context>
				<Context>
				  <Type>VoyageNumber</Type>
				  <Value>11224</Value>
				</Context>
				<Context>
				  <Type>VesselName</Type>
				  <Value>MAGIC SCHOOL BUS</Value>
				</Context>
				<Context>
				  <Type>LegOriginUNLOCO</Type>
				  <Value>AUSYD</Value>
				</Context>
				<Context>
				  <Type>LegDestinationUNLOCO</Type>
				  <Value>SGSIN</Value>
				</Context>
				<Context>
				  <Type>ContainerISOCode</Type>
				  <Value>42G1</Value>
				</Context>
				<Context>
				  <Type>FlightNumber</Type>
				  <Value>FA1224</Value>
				</Context>
				<Context>
				  <Type>FlightDate</Type>
				  <Value>2012-07-19T11:00:00</Value>
				</Context>
			  </ContextCollection>
		</Event>
	</UniversalEvent>";

			var consol = Factory.New<TConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "02012345675";

			var dummyTransport = consol.Transports.AddNew();
			dummyTransport.JW_VoyageFlight = "1124";
			dummyTransport.JW_Vessel = "MAGIC SCHOOL BUS";
			dummyTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;

			var matchingTransport = consol.Transports.AddNew();
			matchingTransport.JW_TransportMode = Core.Constants.TransportModes.Air;
			matchingTransport.JW_RL_NKLoadPort = "AUSYD";
			matchingTransport.JW_RL_NKDiscPort = "SGSIN";
			matchingTransport.JW_ETA = new ZDateTime("2012-07-19T11:00:00");
			matchingTransport.JW_ETD = new ZDateTime("2012-07-19T11:00:00");
			matchingTransport.JW_VoyageFlight = "FA1224";

			var dummyContainer = consol.Containers.AddNew();
			dummyContainer.JC_ContainerNum = "IRSU7865467";

			var matchingContainer = consol.Containers.AddNew();
			matchingContainer.JC_ContainerNum = "OOLU4422850";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(tranpsortLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(Transport), logParent.GetType());

			var logParentTransport = logParent as Transport;

			AssertNotNull("logParentTransport", logParentTransport);
			AssertEquals("Should get best matching Transport.", GetHumanReadableID(matchingTransport), GetHumanReadableID(logParentTransport));
		}

		public void TestMatchForConsolShouldLinkEventToTransportLeg_WhenEventTransportModeIsRailOrRoad_ConsolIsNotSea()
		{
			const string tranpsortLevelEventXmlText = @"
	<UniversalEvent>
		<Event>
			  <EventType>DEP</EventType>
			  <EventParameters>
				  <TransportMode>RAI</TransportMode>
				  <Facility>CTO</Facility>
				  <Location>AUSYD</Location>
			  </EventParameters>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <IsEstimate>false</IsEstimate>
			  <ContextCollection>
				<Context>
				  <Type>ContainerNumber</Type>
				  <Value>OOLU4422850</Value>
				</Context>
				<Context>
				  <Type>MAWBNumber</Type>
				  <Value>020-12345675</Value>
				</Context>
				<Context>
				  <Type>LegOriginUNLOCO</Type>
				  <Value>AUSYD</Value>
				</Context>
				<Context>
				  <Type>LegDestinationUNLOCO</Type>
				  <Value>SGSIN</Value>
				</Context>
				<Context>
				  <Type>ContainerISOCode</Type>
				  <Value>42G1</Value>
				</Context>
			  </ContextCollection>
		</Event>
	</UniversalEvent>";

			var consol = Factory.New<TConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "02012345675";

			var dummyTransport = consol.Transports.AddNew();
			dummyTransport.JW_VoyageFlight = "11224";
			dummyTransport.JW_Vessel = "MAGIC SCHOOL BUS";
			dummyTransport.JW_TransportMode = Core.Constants.TransportModes.Air;

			var matchingTransport = consol.Transports.AddNew();
			matchingTransport.JW_VoyageFlight = "11224";
			matchingTransport.JW_Vessel = "MAGICSCHOOLBUS";
			matchingTransport.JW_TransportMode = Core.Constants.TransportModes.Rail;
			matchingTransport.JW_RL_NKLoadPort = "AUSYD";
			matchingTransport.JW_RL_NKDiscPort = "SGSIN";
			matchingTransport.JW_ETA = new ZDateTime("2012-07-19T11:00:00");
			matchingTransport.JW_ETD = new ZDateTime("2012-07-19T11:00:00");

			var dummyContainer = consol.Containers.AddNew();
			dummyContainer.JC_ContainerNum = "IRSU7865467";

			var matchingContainer = consol.Containers.AddNew();
			matchingContainer.JC_ContainerNum = "OOLU4422850";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(tranpsortLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(Transport), logParent.GetType());

			var logParentTransport = logParent as Transport;

			AssertNotNull("logParentTransport", logParentTransport);
			AssertEquals("Should get best matching Transport.", GetHumanReadableID(matchingTransport), GetHumanReadableID(logParentTransport));
		}

		public void TestMatchForConsolShouldLinkEventToNewCreatedContainer_WhenEventTransportModeIsRailOrRoad_ConsolIsSeaAndHasNotAnyContainer()
		{
			const string tranpsortLevelEventXmlText = @"
	<UniversalEvent>
		<Event>
			  <EventType>DEP</EventType>
			  <EventParameters>
				  <TransportMode>RAI</TransportMode>
				  <Facility>CTO</Facility>
				  <Location>AUSYD</Location>
			  </EventParameters>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <IsEstimate>false</IsEstimate>
			  <ContextCollection>
				<Context>
				  <Type>ContainerNumber</Type>
				  <Value>OOLU4422850</Value>
				</Context>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
				</Context>
				<Context>
				  <Type>VoyageNumber</Type>
				  <Value>11224</Value>
				</Context>
				<Context>
				  <Type>VesselName</Type>
				  <Value>MAGIC SCHOOL BUS</Value>
				</Context>
				<Context>
				  <Type>LegOriginUNLOCO</Type>
				  <Value>AUSYD</Value>
				</Context>
				<Context>
				  <Type>LegDestinationUNLOCO</Type>
				  <Value>SGSIN</Value>
				</Context>
				<Context>
				  <Type>ContainerISOCode</Type>
				  <Value>42G1</Value>
				</Context>
			  </ContextCollection>
		</Event>
	</UniversalEvent>";

			var consol = Factory.New<TConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "02012345675";

			var dummyTransport = consol.Transports.AddNew();
			dummyTransport.JW_VoyageFlight = "11224";
			dummyTransport.JW_Vessel = "MAGIC SCHOOL BUS";
			dummyTransport.JW_TransportMode = Core.Constants.TransportModes.Air;

			var matchingTransport = consol.Transports.AddNew();
			matchingTransport.JW_VoyageFlight = "11224";
			matchingTransport.JW_Vessel = "MAGICSCHOOLBUS";
			matchingTransport.JW_TransportMode = Core.Constants.TransportModes.Rail;

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(tranpsortLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);

			var logParentContainer = logParent as CommonContainer;

			AssertNotNull("logParentTransport", logParentContainer);
			AssertEquals("Should get best matching container.", "OOLU4422850", logParentContainer.JC_ContainerNum);
		}

		#region CancelledParentIsNotMatched

		public void TestGetLogParent_CancelledConsolIsNotMatched()
		{
			var consol = Factory.New<TConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "02012345675";

			AssertCancelledParentIsNotMatched(consol, consol, ConsolLevelEventXmlText);
		}

		public void TestGetLogParent_CancelledShipmentIsNotMatched()
		{
			const string eventXmlText = @"
			<UniversalEvent>
			<Event>
				<EventType>CCD</EventType>
				<EventTime>09-FEB-2017 18:00</EventTime>
				<EventReference>Dummy Description</EventReference>
				<DataProvider>Dummy</DataProvider>
				<ContextCollection>
					<Context>
						<Type>HBOLNumber</Type>
						<Value>20170209</Value>
					</Context>
				</ContextCollection>
			</Event>
			</UniversalEvent>";

			var shipment = Factory.New<TShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "20170209";

			AssertCancelledParentIsNotMatched(shipment, shipment, eventXmlText);
		}

		public void TestGetLogParent_ContainersParentCancelledConsolIsNotMatched()
		{
			const string eventXmlText = @"
			<UniversalEvent>
			<Event>
				<EventType>CCD</EventType>
				<EventTime>09-FEB-2017 18:00</EventTime>
				<EventReference>Dummy Description</EventReference>
				<DataProvider>Dummy</DataProvider>
				<ContextCollection>
					<Context>
						<Type>ContainerNumber</Type>
						<Value>20170209</Value>
					</Context>
				</ContextCollection>
			</Event>
			</UniversalEvent>";

			var consol = Factory.New<TConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "20170209";

			AssertCancelledParentIsNotMatched(consol, container, eventXmlText);
		}

		public void TestGetLogParent_TransportLegsParentCancelledShipmentIsNotMatched()
		{
			const string eventXmlText = @"
			<UniversalEvent>
			<Event>
				<EventType>CCD</EventType>
				<EventTime>09-FEB-2017 18:00</EventTime>
				<EventReference>Dummy Description</EventReference>
				<DataProvider>Dummy</DataProvider>
				<ContextCollection>
					<Context>
						<Type>HBOLNumber</Type>
						<Value>0123456789</Value>
					</Context>
					<Context>
						<Type>VoyageNumber</Type>
						<Value>20170209</Value>
					</Context>
				</ContextCollection>
			</Event>
			</UniversalEvent>";

			var shipment = Factory.New<TShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "0123456789";

			var transport = shipment.Transports.AddNew();
			transport.JW_VoyageFlight = "20170209";

			AssertCancelledParentIsNotMatched(shipment, transport, eventXmlText);
		}

		protected void AssertCancelledParentIsNotMatched(ICancellable cancellableObj, BusinessObject expectedParentObj, string eventXmlText)
		{
			cancellableObj.IsCancelled = false;
			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();

			var xmlEvent = eventDeserializer.Parse(eventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			AssertNotNull(logParents);
			AssertEquals("Should get the expected parent object " + expectedParentObj.HumanReadableName, expectedParentObj, logParents.First());

			cancellableObj.IsCancelled = true;
			Factory.Save();

			logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			var actualResult = logParents == null || logParents.Length == 0;
			Assert("Should get nothing as the parent object is cancelled", actualResult);
		}

		#endregion

		#region Implementation

		protected static string GetHumanReadableID(BusinessObject businessObject)
		{
			return businessObject.HumanReadableName + " - PK: " + businessObject.PK;
		}

		protected const string ConsolLevelEventXmlText = @"
<UniversalEvent>
  <Event>
    <EventType>OCR</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>020-12345675</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";

		protected abstract ConsolAndShipmentEventParentFinder<TConsol, TShipment, TContainer> GetNewConsolEventParentFinder();

		public class TestLogger : IXmlImportLogger
		{
			public TestLogger()
			{
			}

			bool IXmlImportLogger.IsUpdatingConsol { get; set; }
			bool IXmlImportLogger.HasIgnoredModule { get; set; }
			bool IXmlImportLogger.OrgMatchingDisabled => false;

			ITopLevelDataObject IXmlImportLogger.TopLevelDataObject { get; }

			IDataContextDataObject IXmlImportLogger.TopLevelDataContext { get; }

			IEnumerable<IValidationRule> IXmlImportLogger.ValidationRuleCollection { get; set; }

			IEnumerable<ISimpleLog> ISimpleLogResult.Logs => Logs;

			List<TestLog> Logs => logs ?? (logs = new List<TestLog>());
			List<TestLog> logs;

			void IXmlImportLogger.FireDataImportedToBusinessObject(BusinessObject targetBO)
			{
			}

			void ISimpleLogger.Log(LogType type, string message)
			{
			}

			void IXmlImportLogger.LogBoth(LogType type, string message)
			{
				Logs.Add(new TestLog(type, message));
			}

			void IXmlImportLogger.LogTopLevelDataContextKey(GetDataContextKey getDataContextKey)
			{
			}

			void IXmlImportLogger.LogErrorToServiceTaskOnly(string message)
			{
			}

			class TestLog : ISimpleLog
			{
				internal TestLog(LogType type, string message)
				{
					Type = type;
					Message = message;
				}

				public LogType Type { get; }

				public string Message { get; }
			}
		}

		#endregion
	}
}
