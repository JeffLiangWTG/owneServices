using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ForwardingShipmentEventParentFinderTest : TestCaseWithFactory
	{
		public void TestMatchOnHouseBillWhenEventTypeIsSBR()
		{
			const string eventXmlText = @"
			<UniversalEvent>
			<Event>
			  <EventType>SBR</EventType>
			  <EventParameters>
				<Type>Shipment Visibility</Type>
			  </EventParameters>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <EventParameters>
				<Type>Shipment Visibility</Type>
			  </EventParameters>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
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
			shippingLine.RSL_IsNVO = true;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "02012345675";
			shipment.IsCancelled = false;

			Factory.Save();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var logger = new XmlSessionTracker(new SimpleLogger());
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, logger);

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);

			var logParents = parentFinder.GetLogParentsForEvent(xmlEvent);
			AssertNull(logParents);
			AssertContains("Cannot link Forwarding Shipment because: [* Tracking is not supported for this Carrier. *]", logger.ToString());

			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			{
				logParents = parentFinder.GetLogParentsForEvent(xmlEvent);
				AssertNotNull(logParents);
				AssertEquals("Should get the expected parent shipment", shipment, logParents.First());
				AssertContains("Successfully saved: [*Provider subscription for Master Bill '02012345675' was Confirmed. Subscription created*]", logger.ToString());
			}
		}

		public void TestMatchOnUniqueConsignRefWhenEventTypeIsSBR()
		{
			const string eventXmlText = @"
			<UniversalEvent>
			<Event>
			  <EventType>SBR</EventType>
			  <EventParameters>
				<Type>Shipment Visibility</Type>
			  </EventParameters>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <EventParameters>
				<Type>Shipment Visibility</Type>
			  </EventParameters>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>CarriersBookingReference</Type>
				  <Value>Z103439AKL</Value>
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
			shippingLine.RSL_IsNVO = true;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "Z103439AKL";
			shipment.IsCancelled = false;

			Factory.Save();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var logger = new XmlSessionTracker(new SimpleLogger());
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, logger);

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);

			var logParents = parentFinder.GetLogParentsForEvent(xmlEvent);
			AssertNull(logParents);
			AssertContains("Cannot link Forwarding Shipment because: [* Tracking is not supported for this Carrier. *]", logger.ToString());

			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			{
				logParents = parentFinder.GetLogParentsForEvent(xmlEvent);
				AssertNotNull(logParents);
				AssertEquals("Should get the expected parent shipment", shipment, logParents.First());
				AssertContains("Successfully saved: [*Provider subscription for Carrier Booking Reference 'Z103439AKL' was Confirmed. Subscription created*]", logger.ToString());
			}
		}

		public void TestIfMoreThanOneMatchWhenFallingBackToPortOfOriginAndDestinationWhenEventTypeIsSBR()
		{
			const string eventXmlText = @"
			<UniversalEvent>
			<Event>
			  <EventType>SBR</EventType>
			  <EventParameters>
				<Type>Shipment Visibility</Type>
			  </EventParameters>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <EventParameters>
				<Type>Shipment Visibility</Type>
			  </EventParameters>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>Z103439AKL</Value>
				</Context>
				<Context>
				  <Type>CarrierC1CCode</Type>
				  <Value>C1CO</Value>
				</Context>
				<Context>
				  <Type>MBOLOriginUNLOCO</Type>
				  <Value>AUSYD</Value>
				</Context>
				<Context>
				  <Type>MBOLDestinationUNLOCO</Type>
				  <Value>NZAKL</Value>
				</Context>
			  </ContextCollection>
			</Event>
			</UniversalEvent>";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "C1CO";
			shippingLine.RSL_IsNVO = true;

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_HouseBill = "Z103439AKL";
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "SGSIN";
			shipment1.IsCancelled = false;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_HouseBill = "Z103439AKL";
			shipment2.JS_RL_NKOrigin = "USLAX";
			shipment2.JS_RL_NKDestination = "NZAKL";
			shipment2.IsCancelled = false;

			var shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_HouseBill = "Z103439AKL";
			shipment3.JS_RL_NKOrigin = "AUSYD";
			shipment3.JS_RL_NKDestination = "NZAKL";
			shipment3.IsCancelled = false;

			Factory.Save();

			var logger = new XmlSessionTracker(new SimpleLogger());
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, new ForwardingShipmentDataContextManager(), logger);

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);

			var logParents = parentFinder.GetLogParentsForEvent(xmlEvent);
			AssertNull(logParents);
			AssertContains("Cannot link Forwarding Shipment because: [* Tracking is not supported for this Carrier. *]", logger.ToString());

			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			{
				logParents = parentFinder.GetLogParentsForEvent(xmlEvent);
				AssertNotNull(logParents);
				AssertEquals("Should get the expected parent shipment", shipment3, logParents.First());
				AssertContains("Successfully saved: [*Provider subscription for Master Bill 'Z103439AKL' was Confirmed. Subscription created*]", logger.ToString());
			}
		}

		public void TestMatchFailureLogWhenEventTypeIsSBR()
		{
			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			{
				const string eventXmlText = @"
			<UniversalEvent>
			<Event>
			  <EventType>SBR</EventType>
			  <EventParameters>
				<Type>Shipment Visibility</Type>
			  </EventParameters>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <EventParameters>
				<Type>Shipment Visibility</Type>
			  </EventParameters>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>CarrierC1CCode</Type>
				  <Value>C1CO</Value>
				</Context>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
				</Context>
				<Context>
				  <Type>CarriersBookingReference</Type>
				  <Value>BookingRef001</Value>
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
				shippingLine.RSL_IsNVO = true;

				var logger = new XmlSessionTracker(new SimpleLogger());
				var parentFinder = new ForwardingShipmentEventParentFinder(Factory, new ForwardingShipmentDataContextManager(), logger);
				var eventDeserializer = new XmlEventDeserializer();
				var xmlEvent = eventDeserializer.Parse(eventXmlText);

				var logParents = parentFinder.GetLogParentsForEvent(xmlEvent);
				AssertNull(logParents);
				AssertContains("Cannot link Forwarding Shipment because: [* Master Bill Number and Carrier Booking Number are Invalid. *]", logger.ToString());

				shippingLine.RSL_IsNVO = false;

				logger = new XmlSessionTracker(new SimpleLogger());
				parentFinder = new ForwardingShipmentEventParentFinder(Factory, new ForwardingShipmentDataContextManager(), logger);
				logParents = parentFinder.GetLogParentsForEvent(xmlEvent);
				AssertNull(logParents);
				AssertNotContains("Cannot link Forwarding Shipment because: [* Master Bill Number and Carrier Booking Number are Invalid. *]", logger.ToString());
			}
		}

		public void TestGetLogParentsForEvent_SpecifiedAndCancelledShipmentIsNotMatched()
		{
			const string eventXmlText = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>SHP20170209</Key>
					<Type>ForwardingShipment</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>

		<EventType>XXX</EventType>
		<EventTime>09-FEB-2017 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
	</Event>
</UniversalEvent>";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHP20170209";
			shipment.IsCancelled = false;

			Factory.Save();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);

			var logParents = parentFinder.GetLogParentsForEvent(xmlEvent);

			AssertNotNull(logParents);
			AssertEquals("Should get the expected parent shipment", shipment, logParents.First());

			shipment.IsCancelled = true;
			Factory.Save();

			logParents = parentFinder.GetLogParentsForEvent(xmlEvent);

			var actualResult = logParents == null || logParents.Length == 0;
			Assert("Should get nothing as the parent shipment is cancelled", actualResult);
		}

		public void TestGetLogParentsForEvent_ContextHasContainerNumbers_ReturnsMatchedContainers()
		{
			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>

    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>HELLO</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventType>PUP</EventType>
    <EventTime>10-SEP-2016 18:00</EventTime>
    <EventReference>Hello PUP!</EventReference>

    <ContextCollection>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>CONA</Value>
      </Context>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>CONB</Value>
      </Context>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>CONX</Value>
      </Context>
    </ContextCollection>

  </Event>
</UniversalEvent>
";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var containerA = consol.Containers.AddNew();
			containerA.JC_ContainerNum = "CONA";

			var containerB = consol.Containers.AddNew();
			containerB.JC_ContainerNum = "CONB";

			var containerC = consol.Containers.AddNew();
			containerC.JC_ContainerNum = "CONC";

			var containerD = consol.Containers.AddNew();
			containerD.JC_ContainerNum = "COND";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "HELLO";

			shipment.OuterPackLines.RemoveAndDeleteAll();

			shipment.OuterPackLines.AddNew().SetContainer(consol, containerA);
			shipment.OuterPackLines.AddNew().SetContainer(consol, containerB);
			shipment.OuterPackLines.AddNew().SetContainer(consol, containerC);

			Factory.Save();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlMessage);

			var logParents = parentFinder.GetLogParentsForEvent(xmlEvent);
			AssertContainsExactElementsInAnyOrder(new[] { containerA, containerB }, logParents.Cast<ForwardingContainer>());
		}

		public void TestGetLogParentsForEvent_DocumentData_EManifest()
		{
			const string eventXmlMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
    <Event>
        <DataContext>
            <DocumentaryOverride>
                <DocumentName>eManifest</DocumentName>
            </DocumentaryOverride>
            <DataTargetCollection>
                <DataTarget>
                    <Key>S00001000</Key>
                    <Type>ForwardingShipment</Type>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <EventTime>2016-09-01T15:13:30</EventTime>
        <EventType>IRJ</EventType>
        <EventParameters>
            <Department>WiseTechGlobal</Department>
            <MessageType>eManifest</MessageType>
            <ReferenceNumber>849437384/A</ReferenceNumber>
            <Reason>Reason for rejection</Reason>
        </EventParameters>

        <ContextCollection>
            <Context>
                <Type>SO Number</Type>
                <Value>849437384/A</Value>
            </Context>
            <Context>
                <Type>MessageReference</Type>
                <Value>CSG00000000029</Value>
            </Context>
            <Context>
                <Type>Interchange Number</Type>
                <Value>1000</Value>
            </Context>
            <Context>
                <Type>Response</Type>
                <Value>Rejected</Value>
            </Context>
        </ContextCollection>
    </Event>
</UniversalEvent>
";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001000";

			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = "menu a";

			var template = Factory.New<StmTemplate>();
			var documentPivot = Factory.New<StmMenuTemplatePivot>();
			documentPivot.SI_DocumentTitle = "eManifest";
			documentPivot.SI_DataStoreName = "eManifest";
			documentPivot.SI_SU = menu.PK;
			documentPivot.SI_SO = template.PK;

			var docData = (BusinessObject)Factory.New<IVisualizerDocumentData>();
			docData[JobDocumentDataSchema.Constants.JDD_ParentID] = shipment.PK;
			docData[JobDocumentDataSchema.Constants.JDD_ParentTableCode] = shipment.TablePrefix;
			docData[JobDocumentDataSchema.Constants.JDD_Name] = "eManifest";

			Factory.Save();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var exportXmlEvent = eventDeserializer.Parse(eventXmlMessage);
			var parents = parentFinder.GetLogParentsForEvent(exportXmlEvent);

			AssertContainsExactElementsInAnyOrder("should match shipment and document data.", new[] { shipment, docData }, parents);
		}

		public void TestGetLogParentsForEvent_DocumentData_AdvancedCargoReport()
		{
			const string eventXmlMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
    <Event>
        <DataContext>
            <DocumentaryOverride>
                <DocumentName>Advanced Cargo Report</DocumentName>
            </DocumentaryOverride>
            <DataTargetCollection>
                <DataTarget>
                    <Key>S00001000</Key>
                    <Type>ForwardingShipment</Type>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <EventTime>2016-09-01T15:13:30</EventTime>
        <EventType>IRJ</EventType>
        <EventParameters>
            <Department>WiseTechGlobal</Department>
            <MessageType>Advanced Cargo Report</MessageType>
            <ReferenceNumber>849437384/A</ReferenceNumber>
            <Reason>Reason for rejection</Reason>
            <Location>US</Location>
        </EventParameters>

        <ContextCollection>
            <Context>
                <Type>MessageReference</Type>
                <Value>CSG00000000029</Value>
            </Context>
            <Context>
                <Type>Interchange Number</Type>
                <Value>1000</Value>
            </Context>
            <Context>
                <Type>Response</Type>
                <Value>Rejected</Value>
            </Context>
        </ContextCollection>
    </Event>
</UniversalEvent>
";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001000";

			var docData = (BusinessObject)Factory.New<IVisualizerDocumentData>();
			docData[JobDocumentDataSchema.Constants.JDD_ParentID] = shipment.PK;
			docData[JobDocumentDataSchema.Constants.JDD_ParentTableCode] = shipment.TablePrefix;
			docData[JobDocumentDataSchema.Constants.JDD_Name] = "ACAS Shipment Report";

			Factory.Save();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var exportXmlEvent = eventDeserializer.Parse(eventXmlMessage);
			var parents = parentFinder.GetLogParentsForEvent(exportXmlEvent);

			AssertContainsExactElementsInAnyOrder("should match shipment and document data.", new[] { shipment, docData }, parents);
		}

		public void TestGetLogParentsForEvent_ContextHasContainerNumbers_MultipleConsols_ReturnsMatchedContainersFromMatchedConsol()
		{
			const string exportEventXmlMessage = @"
<UniversalEvent>
  <Event>

    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>HELLO</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventType>PUP</EventType>
    <EventTime>10-SEP-2016 18:00</EventTime>
    <EventReference>|FAC=CY|TYP=EMT</EventReference>

    <ContextCollection>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>CONA</Value>
      </Context>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>CONB</Value>
      </Context>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>CONX</Value>
      </Context>
    </ContextCollection>

  </Event>
</UniversalEvent>
";

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol1.Transports[0].JW_ETD = ZDateTime.Today;

			var containerA1 = consol1.Containers.AddNew();
			containerA1.JC_ContainerNum = "CONA";

			var containerB1 = consol1.Containers.AddNew();
			containerB1.JC_ContainerNum = "CONB";

			var containerC1 = consol1.Containers.AddNew();
			containerC1.JC_ContainerNum = "CONC";

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol2.Transports[0].JW_ETD = ZDateTime.Today.AddDays(10);

			var containerA2 = consol2.Containers.AddNew();
			containerA2.JC_ContainerNum = "CONA";

			var containerB2 = consol2.Containers.AddNew();
			containerB2.JC_ContainerNum = "CONB";

			var containerC2 = consol2.Containers.AddNew();
			containerC2.JC_ContainerNum = "CONC";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_UniqueConsignRef = "HELLO";

			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.SetContainer(consol1, containerA1);
			packline1.SetContainer(consol2, containerA2);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.SetContainer(consol1, containerB1);
			packline2.SetContainer(consol2, containerB2);

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.SetContainer(consol1, containerC1);
			packline3.SetContainer(consol2, containerC2);

			Factory.Save();

			AssertEquals("Prerequisite: earliest consol", consol1, shipment.Consols.GetEarliestConsol());
			AssertEquals("Prerequisite: latest consol", consol2, shipment.Consols.GetLatestConsol());

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var exportXmlEvent = eventDeserializer.Parse(exportEventXmlMessage);

			var logParents = parentFinder.GetLogParentsForEvent(exportXmlEvent);
			AssertContainsExactElementsInAnyOrder("Export event: containers from earliest consol should be matched",
				new[] { containerA1, containerB1 },
				logParents.Cast<ForwardingContainer>());

			var importEventXmlMessage = exportEventXmlMessage.Replace("<EventReference>|FAC=CY|TYP=EMT", "<EventReference>|FAC=CTO|TYP=FUL");
			var importXmlEvent = eventDeserializer.Parse(importEventXmlMessage);

			logParents = parentFinder.GetLogParentsForEvent(importXmlEvent);
			AssertContainsExactElementsInAnyOrder("Import event: containers from latest consol should be matched",
				new[] { containerA2, containerB2 },
				logParents.Cast<ForwardingContainer>());

			var nonPickupOrDeliveryEventXmlMessage = exportEventXmlMessage.Replace("<EventType>PUP", "<EventType>FLO");
			var nonPickupOrDeliveryEventXml = eventDeserializer.Parse(nonPickupOrDeliveryEventXmlMessage);

			logParents = parentFinder.GetLogParentsForEvent(nonPickupOrDeliveryEventXml);
			AssertContainsExactElementsInAnyOrder("Non pickup or delivery event: containers from all consols should be matched",
				new[] { containerA1, containerB1, containerA2, containerB2 },
				logParents.Cast<ForwardingContainer>());
		}

		public void TestGetLogParentsForEvent_ContextHasNoContainerNumbers_ReturnsShipment()
		{
			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>

    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>HELLO</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventType>PUP</EventType>
    <EventTime>10-SEP-2016 18:00</EventTime>
    <EventReference>Hello PUP!</EventReference>
    <DataProvider>Pupping Dummy</DataProvider>

    <ContextCollection>
      <Context>
        <Type>ShippersReference</Type>
        <Value>SOMEREFERENCE</Value>
      </Context>
    </ContextCollection>

  </Event>
</UniversalEvent>
";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var containerA = consol.Containers.AddNew();
			containerA.JC_ContainerNum = "CONA";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "HELLO";

			shipment.OuterPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.AddNew().SetContainer(consol, containerA);

			Factory.Save();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlMessage);

			var logParents = parentFinder.GetLogParentsForEvent(xmlEvent);
			AssertContainsExactElementsInAnyOrder(new[] { shipment }, logParents.Cast<ForwardingShipment>());
		}

		public void TestGetLogParentsForEventUsingContext_WhenForwardingConsolIsIncluded()
		{
			const string eventXmlMessage = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingShipment</Type>
				</DataTarget>
				<DataTarget>
					<Type>ForwardingConsol</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>

		<EventTime>2020-06-02T09:50:33.27</EventTime>
		<EventType>ARV</EventType>
		<EventParameters>
			<Department>Customs</Department>
			<Facility>CTO</Facility>
			<Location>MEL</Location>
			<VoyageFlightNumber>QF16</VoyageFlightNumber>
			<FlightDate>2020-06-02</FlightDate>
			<MessageType>AWB Automation</MessageType>
			<ReferenceNumber>554640661S</ReferenceNumber>
			<Total>1</Total>
		</EventParameters>
		<IsEstimate>false</IsEstimate>

		<ContextCollection>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>S54640661S</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_MasterBillNum = "012345678905";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "S54640661S";
			Factory.Save();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlMessage);

			var parents = parentFinder.GetLogParentsForEvent(xmlEvent);
			AssertNull("no parent should have been found because this case will be handled by ConsolEventParentFinder as there is a ForwardingConsol DataTarget", parents);
		}

		public void TestGetLogParentsForEventUsingContext_WithoutForwardingConsol()
		{
			const string eventXmlMessage = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingShipment</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>

		<EventTime>2020-06-02T09:50:33.27</EventTime>
		<EventType>ARV</EventType>
		<EventParameters>
			<Department>Customs</Department>
			<Facility>CTO</Facility>
			<Location>MEL</Location>
			<VoyageFlightNumber>QF16</VoyageFlightNumber>
			<FlightDate>2020-06-02</FlightDate>
			<MessageType>AWB Automation</MessageType>
			<ReferenceNumber>554640661S</ReferenceNumber>
			<Total>1</Total>
		</EventParameters>
		<IsEstimate>false</IsEstimate>

		<ContextCollection>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>S54640661S</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_MasterBillNum = "012345678905";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "S54640661S";
			Factory.Save();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlMessage);

			var parents = parentFinder.GetLogParentsForEvent(xmlEvent);
			AssertNotNull("Should have found parent", parents);
			AssertEquals("GetLogParentsForEvent.Length", 1, parents.Length);
			AssertEquals("", shipment.JS_UniqueConsignRef, ((ForwardingShipment)parents[0]).JS_UniqueConsignRef);
		}

		ForwardingShipment CreateShipmentForTransitReceive(string shipmentnumber, string ercnumber)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = shipmentnumber;
			shipment.JS_HouseBill = shipmentnumber;

			var cfs = Factory.New<OrgHeader>();
			cfs.OH_FullName = "CONSPA";
			cfs.OH_RL_NKClosestPort = "FRDKK";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "FR";
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;

			var packline1 = shipment.OuterPackLines.AddNew();

			var panNumber = (CusEntryNumber)packline1.PortReferences.AddNew();
			panNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN;
			panNumber.CE_EntryNum = "RF0001";
			panNumber.CE_RN_NKCountryCode = "FR";
			panNumber.CE_Category = CusEntryNumber.Categories.PortReferenceNumber;

			var ercNumber = packline1.AdditionalReferenceNumbers.AddNew();
			ercNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
			ercNumber.CE_EntryNum = ercnumber;
			ercNumber.CE_RN_NKCountryCode = "FR";
			ercNumber.CE_Category = CusEntryNumber.Categories.PortReferenceNumber;

			var packline2 = shipment.OuterPackLines.AddNew();

			Factory.Save();

			return shipment;
		}

		public void TestSCMEvent_UpdateTransitReceivePackLine_WithDataTarget()
		{
			const string eventXmlMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>

      <DataSourceCollection>
        <DataSource>
          <Type>TransitReceive</Type>
          <Key>ERC0000001</Key>
        </DataSource>
      </DataSourceCollection>

      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00001001</Key>
        </DataTarget>
      </DataTargetCollection>

    </DataContext>

    <EventTime>2022-03-22T09:06:00</EventTime>
    <EventType>SCM</EventType>
    <CreatedTime>2022-03-23T01:29:52.667</CreatedTime>
    <EventReference>|CRF=RF0001|DEP=Customs|LOC=FRDKK|TYP=Export</EventReference>
    <IsEstimate>false</IsEstimate>

  </Event>
</UniversalEvent>
";
			var shipment = CreateShipmentForTransitReceive("S00001001", "ERC0000001");

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var exportXmlEvent = eventDeserializer.Parse(eventXmlMessage);
			var parents = parentFinder.GetLogParentsForEvent(exportXmlEvent);
			var cusEntryNumber = shipment.OuterPackLines[0].PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN);

			AssertType<ForwardingShipment>(parents.FirstOrDefault());
			AssertEquals(TransitWarehouseReferenceStatus.Codes.Cleared, cusEntryNumber.CE_EntryStatus);
		}

		public void TestSCMEvent_UpdateTransitReceivePackLine_WithoutDataTarget()
		{
			const string eventXmlMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>

      <DataSourceCollection>
        <DataSource>
          <Type>TransitReceive</Type>
          <Key>ERC0000001</Key>
        </DataSource>
      </DataSourceCollection>

    </DataContext>

    <EventTime>2022-03-22T09:06:00</EventTime>
    <EventType>SCM</EventType>
    <CreatedTime>2022-03-23T01:29:52.667</CreatedTime>
    <EventReference>|CRF=RF0001|DEP=Customs|LOC=FRDKK|TYP=Export</EventReference>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
        <Context>
          <Type>HBOLNumber</Type>
          <Value>S00001001</Value>
        </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var shipment = CreateShipmentForTransitReceive("S00001001", "ERC0000001");

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var exportXmlEvent = eventDeserializer.Parse(eventXmlMessage);
			var parents = parentFinder.GetLogParentsForEvent(exportXmlEvent);
			var cusEntryNumber = shipment.OuterPackLines[0].PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN);

			AssertType<ForwardingShipment>(parents.FirstOrDefault());
			AssertEquals(TransitWarehouseReferenceStatus.Codes.Cleared, cusEntryNumber.CE_EntryStatus);
		}

		public void TestSCMEvent_UpdateTransitReceivePackLine_WithoutDataTarget_MultipleShipment()
		{
			const string eventXmlMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>

      <DataSourceCollection>
        <DataSource>
          <Type>TransitReceive</Type>
          <Key>ERC0000001</Key>
        </DataSource>
      </DataSourceCollection>

    </DataContext>

    <EventTime>2022-03-22T09:06:00</EventTime>
    <EventType>SCM</EventType>
    <CreatedTime>2022-03-23T01:29:52.667</CreatedTime>
    <EventReference>|CRF=RF0001|DEP=Customs|LOC=FRDKK|TYP=Export</EventReference>
    <IsEstimate>false</IsEstimate>

  </Event>
</UniversalEvent>
";
			var shipment1 = CreateShipmentForTransitReceive("S00001001", "ERC0000001");
			var shipment2 = CreateShipmentForTransitReceive("S00001002", "ERC0000001");
			var shipment3 = CreateShipmentForTransitReceive("S00001003", "ERC0000003");

			var eventDeserializer = new XmlEventDeserializer();
			var exportXmlEvent = eventDeserializer.Parse(eventXmlMessage);

			var manager1 = shipment1.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder1 = new ForwardingShipmentEventParentFinder(Factory, manager1, new DummyLogger());
			var parents1 = parentFinder1.GetLogParentsForEvent(exportXmlEvent);
			var cusEntryNumber1 = shipment1.OuterPackLines[0].PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN);

			AssertType<ForwardingShipment>(parents1.FirstOrDefault());
			AssertEquals(TransitWarehouseReferenceStatus.Codes.Cleared, cusEntryNumber1.CE_EntryStatus);

			var manager2 = shipment2.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder2 = new ForwardingShipmentEventParentFinder(Factory, manager2, new DummyLogger());
			var parents2 = parentFinder2.GetLogParentsForEvent(exportXmlEvent);
			var cusEntryNumber2 = shipment2.OuterPackLines[0].PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN);

			AssertType<ForwardingShipment>(parents2.FirstOrDefault());
			AssertEquals(TransitWarehouseReferenceStatus.Codes.Cleared, cusEntryNumber2.CE_EntryStatus);

			var manager3 = shipment3.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder3 = new ForwardingShipmentEventParentFinder(Factory, manager3, new DummyLogger());
			var parents3 = parentFinder3.GetLogParentsForEvent(exportXmlEvent);
			var cusEntryNumber3 = shipment3.OuterPackLines[0].PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN);

			AssertType<ForwardingShipment>(parents3.FirstOrDefault());
			AssertEquals("", cusEntryNumber3.CE_EntryStatus);
		}

		public void TestSHLEvent_UpdateTransitReceivePackLine_WithDataTarget()
		{
			const string eventXmlMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>

      <DataSourceCollection>
        <DataSource>
          <Type>TransitReceive</Type>
          <Key>ERC0000001</Key>
        </DataSource>
      </DataSourceCollection>

      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00001001</Key>
        </DataTarget>
      </DataTargetCollection>

    </DataContext>

    <EventTime>2022-03-22T09:06:00</EventTime>
    <EventType>SHL</EventType>
    <CreatedTime>2022-03-23T01:29:52.667</CreatedTime>
    <EventReference>|CRF=RF0001|FAC=CFS|LOC=FRDKK|MST=Port Notification Export Status</EventReference>
    <IsEstimate>false</IsEstimate>

  </Event>
</UniversalEvent>
";
			var shipment = CreateShipmentForTransitReceive("S00001001", "ERC0000001");

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var exportXmlEvent = eventDeserializer.Parse(eventXmlMessage);
			var parents = parentFinder.GetLogParentsForEvent(exportXmlEvent);
			var cusEntryNumber = shipment.OuterPackLines[0].PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN);

			AssertType<ForwardingShipment>(parents.FirstOrDefault());
			AssertEquals("", cusEntryNumber.CE_EntryStatus);
		}

		public void TestSHLEvent_UpdateTransitReceivePackLine_WithoutDataTarget()
		{
			const string eventXmlMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>

      <DataSourceCollection>
        <DataSource>
          <Type>TransitReceive</Type>
          <Key>ERC0000001</Key>
        </DataSource>
      </DataSourceCollection>

    </DataContext>

    <EventTime>2022-03-22T09:06:00</EventTime>
    <EventType>SHL</EventType>
    <CreatedTime>2022-03-23T01:29:52.667</CreatedTime>
    <EventReference>|CRF=RF0001|FAC=CFS|LOC=FRDKK|MST=Port Notification Export Status</EventReference>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
        <Context>
          <Type>HBOLNumber</Type>
          <Value>S00001001</Value>
        </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var shipment = CreateShipmentForTransitReceive("S00001001", "ERC0000001");

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var exportXmlEvent = eventDeserializer.Parse(eventXmlMessage);
			var parents = parentFinder.GetLogParentsForEvent(exportXmlEvent);
			var cusEntryNumber = shipment.OuterPackLines[0].PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN);

			AssertType<ForwardingShipment>(parents.FirstOrDefault());
			AssertEquals("", cusEntryNumber.CE_EntryStatus);
		}

		public void TestGetLogParentsForEvent_MatchingPackLine_WithDataTarget()
		{
			const string eventXmlMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>TransitReceive</Type>
          <Key>RC00002149</Key>
        </DataSource>
      </DataSourceCollection>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>A00001060</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <EventTime>2022-03-22T09:06:00</EventTime>
    <EventType>MAA</EventType>
    <CreatedTime>2022-03-23T01:29:52.667</CreatedTime>
    <EventReference>|CRF=3568386|DEP=Terminal|LOC=FRDKK|MST=Goods Received (CRESA)|RFN=3568386</EventReference>
    <IsEstimate>false</IsEstimate>
  </Event>
</UniversalEvent>
";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "A00001060";

			var cfs = Factory.New<OrgHeader>();
			cfs.OH_FullName = "CONSPA";
			cfs.OH_RL_NKClosestPort = "FRDKK";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "FR";
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;

			var packline1 = shipment.OuterPackLines.AddNew();
			var ercNumber = packline1.AdditionalReferenceNumbers.AddNew();
			ercNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
			ercNumber.CE_EntryNum = "RC00002149";
			ercNumber.CE_Category = "OTH";
			var packline2 = shipment.OuterPackLines.AddNew();

			Factory.Save();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var exportXmlEvent = eventDeserializer.Parse(eventXmlMessage);
			var parents = parentFinder.GetLogParentsForEvent(exportXmlEvent);

			AssertContainsExactElementsInAnyOrder(shipment, parents);
			AssertType<ForwardingShipment>(parents.FirstOrDefault());

			var panNumber = packline1.PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN);

			AssertEquals(null, panNumber);
			ercNumber.CE_RN_NKCountryCode = "FR";

			parents = parentFinder.GetLogParentsForEvent(exportXmlEvent);
			panNumber = packline1.PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN);

			AssertContainsExactElementsInAnyOrder(shipment, parents);
			AssertEquals("3568386", panNumber.CE_EntryNum);
			AssertEquals("PAN", panNumber.CE_EntryType);
			AssertEquals("FR", panNumber.CE_RN_NKCountryCode);
			AssertEquals("PRT", panNumber.CE_Category);
			AssertEquals("3568386", packline1.JL_ExportRefNumber);
			AssertEquals("Time", new ZDateTime(2022, 03, 22, 09, 06, 00), panNumber.CE_IssueDate);
			AssertEquals(true, panNumber.CE_EntryIsSystemGenerated);
			AssertNullOrEmpty(packline2.JL_ExportRefNumber);
			AssertNull(packline2.PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN));

			panNumber.CE_EntryNum = "ABCD";
			panNumber.CE_EntryIsSystemGenerated = false;
			AssertEquals("ABCD", panNumber.CE_EntryNum);
			AssertEquals("PAN", panNumber.CE_EntryType);
			AssertEquals("FR", panNumber.CE_RN_NKCountryCode);
			AssertEquals("PRT", panNumber.CE_Category);
			AssertEquals(false, panNumber.CE_EntryIsSystemGenerated);

			parents = parentFinder.GetLogParentsForEvent(exportXmlEvent);
			AssertContainsExactElementsInAnyOrder(shipment, parents);

			AssertEquals("3568386", panNumber.CE_EntryNum);
			AssertEquals("PAN", panNumber.CE_EntryType);
			AssertEquals("FR", panNumber.CE_RN_NKCountryCode);
			AssertEquals("PRT", panNumber.CE_Category);
			AssertEquals("3568386", packline1.JL_ExportRefNumber);
			AssertEquals("Time", new ZDateTime(2022, 03, 22, 09, 06, 00), panNumber.CE_IssueDate);
			AssertEquals(true, panNumber.CE_EntryIsSystemGenerated);
			AssertNullOrEmpty(packline2.JL_ExportRefNumber);
		}

		public void TestGetLogParentsForEvent_MatchingPackLine_WithoutDataTarget()
		{
			const string eventXmlMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>TransitReceive</Type>
          <Key>RC00002149</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>
    <EventTime>2022-03-22T09:06:00</EventTime>
    <EventType>MAA</EventType>
    <CreatedTime>2022-03-23T01:29:52.667</CreatedTime>
    <EventReference>|CRF=3568386|DEP=Terminal|LOC=FRDKK|MST=Goods Received (CRESA)|RFN=3568386</EventReference>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
        <Context>
          <Type>HBOLNumber</Type>
          <Value>S54640661S</Value>
        </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "S54640661S";

			var cfs = Factory.New<OrgHeader>();
			cfs.OH_FullName = "CONSPA";
			cfs.OH_RL_NKClosestPort = "FRDKK";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "FR";
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;

			var packline1 = shipment.OuterPackLines.AddNew();
			var ercNumber = packline1.AdditionalReferenceNumbers.AddNew();
			ercNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
			ercNumber.CE_EntryNum = "RC00002149";
			ercNumber.CE_Category = "OTH";
			var packline2 = shipment.OuterPackLines.AddNew();

			Factory.Save();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var exportXmlEvent = eventDeserializer.Parse(eventXmlMessage);
			var parents = parentFinder.GetLogParentsForEvent(exportXmlEvent);

			AssertType<ForwardingShipment>(parents.FirstOrDefault());
			AssertContainsExactElementsInAnyOrder(shipment, parents);

			var panNumber = packline1.PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN);

			AssertEquals(null, panNumber);
			ercNumber.CE_RN_NKCountryCode = "FR";

			parents = parentFinder.GetLogParentsForEvent(exportXmlEvent);
			panNumber = packline1.PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN);

			AssertContainsExactElementsInAnyOrder(shipment, parents);
			AssertEquals("3568386", panNumber.CE_EntryNum);
			AssertEquals("PAN", panNumber.CE_EntryType);
			AssertEquals("FR", panNumber.CE_RN_NKCountryCode);
			AssertEquals("PRT", panNumber.CE_Category);
			AssertEquals("3568386", packline1.JL_ExportRefNumber);
			AssertEquals("Time", new ZDateTime(2022, 03, 22, 09, 06, 00), panNumber.CE_IssueDate);
			AssertEquals(true, panNumber.CE_EntryIsSystemGenerated);
			AssertNullOrEmpty(packline2.JL_ExportRefNumber);
			AssertNull(packline2.PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN));

			panNumber.CE_EntryNum = "ABCD";
			panNumber.CE_EntryIsSystemGenerated = false;
			AssertEquals("ABCD", panNumber.CE_EntryNum);
			AssertEquals("PAN", panNumber.CE_EntryType);
			AssertEquals("FR", panNumber.CE_RN_NKCountryCode);
			AssertEquals("PRT", panNumber.CE_Category);
			AssertEquals(false, panNumber.CE_EntryIsSystemGenerated);

			parents = parentFinder.GetLogParentsForEvent(exportXmlEvent);
			AssertContainsExactElementsInAnyOrder(shipment, parents);

			AssertEquals("3568386", panNumber.CE_EntryNum);
			AssertEquals("PAN", panNumber.CE_EntryType);
			AssertEquals("FR", panNumber.CE_RN_NKCountryCode);
			AssertEquals("PRT", panNumber.CE_Category);
			AssertEquals("3568386", packline1.JL_ExportRefNumber);
			AssertEquals(true, panNumber.CE_EntryIsSystemGenerated);
			AssertEquals("Time", new ZDateTime(2022, 03, 22, 09, 06, 00), panNumber.CE_IssueDate);
			AssertNullOrEmpty(packline2.JL_ExportRefNumber);
		}

		public void TestGetLogParentsForEvent_MatchingPackLine_WithoutMAA()
		{
			const string eventXmlMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>TransitReceive</Type>
          <Key>RC00002149</Key>
        </DataSource>
      </DataSourceCollection>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>A00001060</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <EventTime>2022-03-22T09:06:00</EventTime>
    <EventType>MRJ</EventType>
    <CreatedTime>2022-03-23T01:29:52.667</CreatedTime>
    <EventReference>|CRF=3568386|DEP=Terminal|LOC=FRDKK|MST=Goods Received (CRESA)|RFN=3568386</EventReference>
    <IsEstimate>false</IsEstimate>
  </Event>
</UniversalEvent>
";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "A00001060";

			var cfs = Factory.New<OrgHeader>();
			cfs.OH_FullName = "CONSPA";
			cfs.OH_RL_NKClosestPort = "FRDKK";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "FR";
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;

			var packline1 = shipment.OuterPackLines.AddNew();
			var ercNumber = packline1.AdditionalReferenceNumbers.AddNew();
			ercNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
			ercNumber.CE_EntryNum = "RC00002149";
			ercNumber.CE_Category = "OTH";
			ercNumber.CE_RN_NKCountryCode = "FR";
			var packline2 = shipment.OuterPackLines.AddNew();

			Factory.Save();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var exportXmlEvent = eventDeserializer.Parse(eventXmlMessage);
			var parents = parentFinder.GetLogParentsForEvent(exportXmlEvent);

			AssertContainsExactElementsInAnyOrder(shipment, parents);
			AssertNull(packline1.PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == CusEntryNumber.EntryType.PortAuthorityNumber));
			AssertNull(packline2.PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == CusEntryNumber.EntryType.PortAuthorityNumber));
			AssertNullOrEmpty(packline1.JL_ExportRefNumber);
			AssertNullOrEmpty(packline2.JL_ExportRefNumber);
		}

		public void TestGetLogParentsForEvent_WithoutTransitReceive()
		{
			const string eventXmlMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>A00001060</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <EventTime>2022-03-22T09:06:00</EventTime>
    <EventType>MAA</EventType>
    <CreatedTime>2022-03-23T01:29:52.667</CreatedTime>
    <EventReference>|CRF=3568386|DEP=Terminal|LOC=FRDKK|MST=Goods Received (CRESA)|RFN=3568386</EventReference>
    <IsEstimate>false</IsEstimate>
  </Event>
</UniversalEvent>
";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "A00001060";

			var cfs = Factory.New<OrgHeader>();
			cfs.OH_FullName = "CONSPA";
			cfs.OH_RL_NKClosestPort = "FRDKK";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "FR";
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;

			var packline1 = shipment.OuterPackLines.AddNew();
			var ercNumber = packline1.AdditionalReferenceNumbers.AddNew();
			ercNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
			ercNumber.CE_EntryNum = "RC00002149";
			ercNumber.CE_Category = "OTH";
			ercNumber.CE_RN_NKCountryCode = "FR";
			var packline2 = shipment.OuterPackLines.AddNew();

			Factory.Save();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new ForwardingShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var exportXmlEvent = eventDeserializer.Parse(eventXmlMessage);
			var parents = parentFinder.GetLogParentsForEvent(exportXmlEvent);

			AssertContainsExactElementsInAnyOrder(shipment, parents);
			AssertNull(packline1.PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == CusEntryNumber.EntryType.PortAuthorityNumber));
			AssertNull(packline2.PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == CusEntryNumber.EntryType.PortAuthorityNumber));
			AssertNullOrEmpty(packline1.JL_ExportRefNumber);
			AssertNullOrEmpty(packline2.JL_ExportRefNumber);
		}
	}
}
