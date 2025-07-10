using System.Linq;
using System.Threading;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ForwardingConsolEventParentFinderTest : Freight.DataTransfer.Universal.Testing.ConsolEventParentFinderTest<ForwardingConsol, ForwardingShipment, ForwardingContainer>
	{
		public void TestIncomingEventWithDocumentData()
		{
			AssertIncomingConsolEventWithDocumentData("DataStore", "Document1", "DataStore", true);
		}

		public void TestIncomingEventWithDocumentData_MatchByDocumentNameForSystemTitles()
		{
			AssertIncomingConsolEventWithDocumentData("Shipping Instruction", "Shipping Instruction", "SeaBookingRequest2", true);
		}

		public void TestIncomingEventWithDocumentData_DoNotMatchByDocumentNameForNonSystemTitles()
		{
			AssertIncomingConsolEventWithDocumentData("Document1", "Document1", "DataStore", false);
		}

		void AssertIncomingConsolEventWithDocumentData(string documentNameInXml,
		   string pivotDocumentTitle, string pivotDataStoreName, bool expectToMatchDocumentData)
		{
			string shipmentLevelEventXmlText = $@"
<UniversalEvent>
	<Event>
		<DataContext>

			<DataTargetCollection>
				<DataTarget>
					<Key>CONSOL0001</Key>
					<Type>ForwardingConsol</Type>
				</DataTarget>
			</DataTargetCollection>

			<DocumentaryOverride>
				<DocumentName>{documentNameInXml}</DocumentName>
			</DocumentaryOverride>

		</DataContext>

		<EventType>XXX</EventType>
		<EventTime>06-JUL-2014 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
	</Event>
</UniversalEvent>";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "CONSOL0001";

			var menu = Factory.New<StmMenuItem>();
			menu.SU_BusinessContext = nameof(BusinessContext.Consol);
			menu.SU_MenuType = "FRM";
			menu.SU_MenuName = "test";

			var template = Factory.New<StmTemplate>();
			template.SO_TemplateType = "FRM";

			var documentPivot = Factory.New<StmMenuTemplatePivot>();
			documentPivot.SI_DocumentTitle = pivotDocumentTitle;
			documentPivot.SI_DataStoreName = pivotDataStoreName;
			documentPivot.SI_SU = menu.PK;
			documentPivot.SI_SO = template.PK;

			var documentData = (BusinessObject)Factory.New<IVisualizerDocumentData>();
			documentData[JobDocumentDataSchema.Constants.JDD_ParentID] = consol.PK;
			documentData[JobDocumentDataSchema.Constants.JDD_ParentTableCode] = consol.Prefix;
			documentData[JobDocumentDataSchema.Constants.JDD_Name] = pivotDataStoreName;

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			var expectedElements = expectToMatchDocumentData
				? new[] { consol, documentData }
				: new[] { consol };

			AssertContainsExactElementsInAnyOrder("should match consol and document data",
				expectedElements,
				logParents);
		}

		public void TestGetLogParent_SpecifiedAndCancelledConsolIsNotMatched()
		{
			const string eventXmlText = @"
			<UniversalEvent>
				<Event>
					<DataContext>
						<DataTargetCollection>
							<DataTarget>
								<Key>CON20170209</Key>
								<Type>ForwardingConsol</Type>
							</DataTarget>
						</DataTargetCollection>
					</DataContext>

					<EventType>XXX</EventType>
					<EventTime>09-FEB-2017 18:00</EventTime>
					<EventReference>Dummy Description</EventReference>
					<DataProvider>Dummy</DataProvider>
				</Event>
			</UniversalEvent>";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "CON20170209";

			AssertCancelledParentIsNotMatched(consol, consol, eventXmlText);
		}

		public void TestIncomingEventDoesntNotMatchNonForwardingAirConsol()
		{
			var cfsConsol = (CommonConsol)Factory.New<CFS.ICFSLoadListConsol>();

			cfsConsol.JK_TransportMode = Constants.TransportModes.Air;
			cfsConsol.JK_MasterBillNum = "02012345675";

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(ConsolLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("Should not match LogParent", logParents);
		}

		public void TestIncomingEventDoesntNotMatchNonForwardingConsol()
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

			var cfsConsol = (CommonConsol)Factory.New<CFS.ICFSLoadListConsol>();

			cfsConsol.JK_TransportMode = Constants.TransportModes.Sea;
			cfsConsol.JK_MasterBillNum = "02012345675";

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(consolLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("Should not match LogParent", logParents);
		}

		public void TestIncomingEventDoesntMatchNonForwardingShipment()
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
				<Value>02012345675</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var anotherFactory = new BusinessObjectFactory();
			var cfsShipment = (CommonShipment)anotherFactory.New<CFS.ICFSShipment>();
			cfsShipment.JS_TransportMode = Constants.TransportModes.Sea;
			cfsShipment.JS_HouseBill = "02012345675";
			anotherFactory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("Should not match LogParent", logParents);
		}

		public void TestIncomingEventLinksToContainerWhenUsingHBOLAndContainerNumberAndNotToLatestContainer()
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
				  <Type>HBOLNumber</Type>
				  <Value>20257654321</Value>
				</Context>
				<Context>
				  <Type>ContainerNumber</Type>
				  <Value>10137654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingShipment = Factory.New<ForwardingShipment>();
			matchingShipment.JS_TransportMode = Constants.TransportModes.Sea;
			matchingShipment.JS_HouseBill = "20257654321";

			var matchingContainer = Factory.New<ForwardingContainer>();
			matchingContainer.JC_ContainerNum = "10137654321";

			matchingShipment.Consols.AddNew();
			matchingShipment.Consols[0].Containers.Add(matchingContainer);
			matchingShipment.OuterPackLines.AddNew();

			Factory.Save();

			Thread.Sleep(200);

			var dummyShipment = Factory.New<ForwardingShipment>();
			dummyShipment.JS_TransportMode = Constants.TransportModes.Sea;

			var dummyConsol = dummyShipment.Consols.AddNew();
			var dummyContainer = dummyConsol.Containers.AddNew();
			dummyContainer.JC_ContainerNum = "10137654321";

			dummyShipment.OuterPackLines.AddNew();

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(ForwardingContainer), logParent.GetType());

			var logParentContainer = logParent as ForwardingContainer;

			AssertNotNull("logParentContainer", logParentContainer);
			AssertEquals("Should get best matching Container.", GetHumanReadableID(matchingContainer), GetHumanReadableID(logParentContainer));
		}

		public void TestIncomingEventLinksToContainerWhenUsingHBOLAndContainerNumberAndFallsBackToShipmentIfCannotMatchContainer()
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
				  <Type>HBOLNumber</Type>
				  <Value>20257654321</Value>
				</Context>
				<Context>
				  <Type>ContainerNumber</Type>
				  <Value>10137654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingShipment = Factory.New<ForwardingShipment>();
			matchingShipment.JS_TransportMode = Constants.TransportModes.Sea;
			matchingShipment.JS_HouseBill = "20257654321";

			var matchingContainer = Factory.New<ForwardingContainer>();
			matchingContainer.JC_ContainerNum = "10137654321";

			matchingShipment.Consols.AddNew();
			matchingShipment.Consols[0].Containers.Add(matchingContainer);
			matchingShipment.OuterPackLines.AddNew();

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(ForwardingContainer), logParent.GetType());

			var logParentContainer = logParent as ForwardingContainer;

			AssertNotNull("logParentContainer", logParentContainer);
			AssertEquals("Should get best matching Container.", GetHumanReadableID(matchingContainer), GetHumanReadableID(logParentContainer));

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			matchingShipment.Consols.RemoveAndDeleteAll();
			xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			AssertEquals("Should match one LogParent", 1, logParents.Length);

			logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(ForwardingShipment), logParent.GetType());

			var logParentShipment = logParent as ForwardingShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));
		}

		public void TestIncomingEventLinksToContainerWhenUsingHBOLVoyageNumberAndContainerNumberAndFallsBackToLegWithNoMatchingContainerAndFallsBackToShipmentWithNoMatchingLeg()
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
				  <Type>HBOLNumber</Type>
				  <Value>20257654321</Value>
				</Context>
				<Context>
				  <Type>ContainerNumber</Type>
				  <Value>10137654321</Value>
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

			var matchingShipment = Factory.New<ForwardingShipment>();
			matchingShipment.JS_TransportMode = Constants.TransportModes.Sea;
			matchingShipment.JS_HouseBill = "20257654321";

			var matchingContainer = Factory.New<ForwardingContainer>();
			matchingContainer.JC_ContainerNum = "10137654321";

			matchingShipment.Consols.AddNew();
			matchingShipment.Consols[0].Containers.Add(matchingContainer);
			matchingShipment.OuterPackLines.AddNew();

			var matchingTransport = matchingShipment.Transports.AddNew();
			matchingTransport.JW_VoyageFlight = "12234";

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(ForwardingContainer), logParent.GetType());

			var logParentContainer = logParent as ForwardingContainer;

			AssertNotNull("logParentContainer", logParentContainer);
			AssertEquals("Should get best matching Container.", GetHumanReadableID(matchingContainer), GetHumanReadableID(logParentContainer));

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			matchingShipment.Consols.RemoveAndDeleteAll();
			xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			AssertEquals("Should match one LogParent", 1, logParents.Length);

			logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(Transport), logParent.GetType());

			var logParentLeg = logParent as Transport;

			AssertNotNull("logParentLeg", logParentLeg);
			AssertEquals("Should get best matching Transport.", GetHumanReadableID(matchingTransport), GetHumanReadableID(logParentLeg));

			matchingShipment.Transports.RemoveAndDeleteAll();
			xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			AssertEquals("Should match one LogParent", 1, logParents.Length);

			logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(ForwardingShipment), logParent.GetType());

			var logParentShipment = logParent as ForwardingShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));
		}

		public void TestIncomingEventMatchesOnAdditionalReferencesForShipment()
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
				  <Type Description=""AMS Number"">AMS</Type>
				  <Value>134FREGT</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingShipment = Factory.New<ForwardingShipment>();
			matchingShipment.JS_UniqueConsignRef = "S10101010";

			var additionalReference1 = matchingShipment.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "134FREGT";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);
			xmlEvent.DataContext = DataContextFactory.New();
			xmlEvent.DataContext.CodesMappedToTarget = true;

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(ForwardingShipment), logParent.GetType());

			var logParentShipment = logParent as ForwardingShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));
		}

		public void TestIncomingEventMatchesOnShipmentWithMoreMatchesOnAdditionalReferences()
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

			var matchingShipment = Factory.New<ForwardingShipment>();
			matchingShipment.JS_UniqueConsignRef = "S10101010";

			var additionalReference1 = matchingShipment.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "134FREGT";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var additionalReference2 = matchingShipment.Numbers.AddNew();
			additionalReference2.CE_EntryNum = "267AIRGT";
			additionalReference2.CE_EntryType = "COC";
			additionalReference2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.Save();

			Thread.Sleep(200);

			var dummyShipment = Factory.New<ForwardingShipment>();

			var dummyReference = dummyShipment.Numbers.AddNew();
			dummyReference.CE_EntryNum = "134FREGT";
			dummyReference.CE_EntryType = "AMS";
			dummyReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);
			xmlEvent.DataContext = DataContextFactory.New();
			xmlEvent.DataContext.CodesMappedToTarget = true;

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(ForwardingShipment), logParent.GetType());

			var logParentShipment = logParent as ForwardingShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));
		}

		public void TestIncomingEventMatchesToConsolOnAdditionalReferences()
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

			var matchingConsol = Factory.New<ForwardingConsol>();

			matchingConsol.JK_TransportMode = Constants.TransportModes.Air;
			matchingConsol.JK_AgentsReference = "DOUBLEAGENT";

			var additionalReference1 = matchingConsol.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "134FREGT";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var additionalReference2 = matchingConsol.Numbers.AddNew();
			additionalReference2.CE_EntryNum = "267AIRGT";
			additionalReference2.CE_EntryType = "COC";
			additionalReference2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(consolLevelEventXmlText);
			xmlEvent.DataContext = DataContextFactory.New();
			xmlEvent.DataContext.CodesMappedToTarget = true;

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(ForwardingConsol), logParent.GetType());

			var logParentConsol = logParent as ForwardingConsol;

			AssertNotNull("logParentConsol", logParentConsol);
			AssertEquals("Should get best matching Consol.", GetHumanReadableID(matchingConsol), GetHumanReadableID(logParentConsol));
		}

		public void TestIncomingEventMatchesToConsolWhereNoShipmentOrConsolReferencesAreIncludedButOverlappingReferencesDoNotMatchToExistingShipment()
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

			var matchingConsol = Factory.New<ForwardingConsol>();
			matchingConsol.JK_TransportMode = Constants.TransportModes.Sea;

			var consolAdditionalReference1 = matchingConsol.Numbers.AddNew();
			consolAdditionalReference1.CE_EntryNum = "134FREGT";
			consolAdditionalReference1.CE_EntryType = "AMS";
			consolAdditionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var consolAdditionalReference2 = matchingConsol.Numbers.AddNew();
			consolAdditionalReference2.CE_EntryNum = "267AIRGT";
			consolAdditionalReference2.CE_EntryType = "COC";
			consolAdditionalReference2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);
			xmlEvent.DataContext = DataContextFactory.New();
			xmlEvent.DataContext.CodesMappedToTarget = true;

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(ForwardingConsol), logParent.GetType());

			var logParentConsol = logParent as ForwardingConsol;

			AssertNotNull("logParentConsol", logParentConsol);
			AssertEquals("Should get best matching Consol.", GetHumanReadableID(matchingConsol), GetHumanReadableID(logParentConsol));
		}

		public void TestIncomingEventMatchesToShipmentWhereNoShipmentOrConsolReferencesAreIncludedButOverlappingReferencesMatchToExistingShipment()
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

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var consolAdditionalReference1 = consol.Numbers.AddNew();
			consolAdditionalReference1.CE_EntryNum = "134FREGT";
			consolAdditionalReference1.CE_EntryType = "AMS";
			consolAdditionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var consolAdditionalReference2 = consol.Numbers.AddNew();
			consolAdditionalReference2.CE_EntryNum = "267AIRGT";
			consolAdditionalReference2.CE_EntryType = "COC";
			consolAdditionalReference2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var matchingShipment = Factory.New<ForwardingShipment>();
			matchingShipment.JS_TransportMode = Constants.TransportModes.Sea;

			var shipmentAdditionalReference1 = matchingShipment.Numbers.AddNew();
			shipmentAdditionalReference1.CE_EntryNum = "134FREGT";
			shipmentAdditionalReference1.CE_EntryType = "AMS";
			shipmentAdditionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var shipmentAdditionalReference2 = matchingShipment.Numbers.AddNew();
			shipmentAdditionalReference2.CE_EntryNum = "267AIRGT";
			shipmentAdditionalReference2.CE_EntryType = "COC";
			shipmentAdditionalReference2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);
			xmlEvent.DataContext = DataContextFactory.New();
			xmlEvent.DataContext.CodesMappedToTarget = true;

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(ForwardingShipment), logParent.GetType());

			var logParentShipment = logParent as ForwardingShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));
		}

		public void TestLinkingIncomingEventToAnIndividualContainerWithMBOLAndHBOLAndContainerNumberAndFallsBackToShipmentWithNoMatchingContainerAndDoesNotFallBackToConsolWithNoMatchingShipment()
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
				  <Type>ContainerNumber</Type>
				  <Value>20257654321</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingConsol = Factory.New<ForwardingConsol>();

			var matchingShipment = Factory.New<ForwardingShipment>();
			matchingShipment.JS_TransportMode = Constants.TransportModes.Sea;
			matchingShipment.JS_HouseBill = "11111111111";

			var matchingContainer = Factory.New<ForwardingContainer>();
			matchingContainer.JC_ContainerNum = "20257654321";

			matchingConsol.Containers.Add(matchingContainer);
			matchingConsol.JK_TransportMode = Constants.TransportModes.Sea;
			matchingConsol.JK_MasterBillNum = "02012345675";
			matchingShipment.Consols.Add(matchingConsol);
			matchingShipment.OuterPackLines.AddNew();

			Factory.Save();

			var subscriber = GetNewConsolEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			var logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(ForwardingContainer), logParent.GetType());

			var logParentContainer = logParent as ForwardingContainer;

			AssertNotNull("logParentContainer", logParentContainer);
			AssertEquals("Should get best matching Container.", GetHumanReadableID(matchingContainer), GetHumanReadableID(logParentContainer));

			matchingConsol.Containers.RemoveAndDeleteAll();

			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);

			logParent = logParents[0];
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(ForwardingShipment), logParent.GetType());

			var logParentShipment = logParent as ForwardingShipment;

			AssertNotNull("logParentShipment", logParentShipment);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(matchingShipment), GetHumanReadableID(logParentShipment));

			matchingConsol.Shipments.Remove(matchingShipment);
			matchingShipment.JS_IsCancelled = true;

			Factory.Save();

			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("logParents", logParents);
		}

		#region ULD Container

		public void TestIncomingEventShouldLinkToULDContainer_UpdateContainer()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "MBL11111";
			consol.JK_BookingReference = "BKG22222";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_BookingReference = "BOOKS";
			consol.JK_MasterBillNum = "081";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT4444444";
			container1.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "PM-2H")).PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT4444455";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "HBL33333";
			consol.GridShipments.Add(shipment);

			Factory.Save();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(ULDEventXML);

			var subscriber = GetNewConsolEventParentFinder();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNotNull("logParents", logParents);
			AssertEquals(2, logParents.Length);

			var containers = logParents.OfType<ForwardingContainer>().ToArray();
			AssertEquals(2, containers.Length);
			AssertContainsExactElementsInAnyOrder(new[] { container1.PK, container2.PK }, containers.Select(c => c.PK));
			Assert("Container type should be updated", containers.All(c => c.JC_RC == container1.JC_RC));
		}

		public void TestIncomingEventShouldLinkToULDContainer_CreateContainer()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "MBL11111";
			consol.JK_BookingReference = "BKG22222";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_BookingReference = "BOOKS";
			consol.JK_MasterBillNum = "081";
			consol.JK_ConsolMode = "ULD";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "HBL33333";
			consol.GridShipments.Add(shipment);

			Factory.Save();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(ULDEventXML);

			var subscriber = GetNewConsolEventParentFinder();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNotNull("logParents", logParents);
			AssertEquals(2, logParents.Length);

			var containers = logParents.OfType<ForwardingContainer>().ToArray();
			AssertEquals(2, containers.Length);
			AssertContainsExactElementsInAnyOrder("Two containers are created.", new[] { "CONT4444444", "CONT4444455" }, containers.Select(c => c.JC_ContainerNum));
			Assert("Container type should be empty", containers.All(c => c.JC_RC.IsEmpty));
			AssertContainsExactElementsInAnyOrder(new[] { "CONT4444444", "CONT4444455" }, consol.Containers.Cast<ForwardingContainer>().Select(c => c.JC_ContainerNum));
		}

		public void TestIncomingEventShouldLinkToULDContainer_CreateContainerAndApplyContainerType()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "MBL11111";
			consol.JK_BookingReference = "BKG22222";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_BookingReference = "BOOKS";
			consol.JK_MasterBillNum = "081";
			consol.JK_ConsolMode = "ULD";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT4444444";
			container1.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "PM-2H")).PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "HBL33333";
			consol.GridShipments.Add(shipment);

			Factory.Save();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(ULDEventXML);

			var subscriber = GetNewConsolEventParentFinder();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNotNull("logParents", logParents);
			AssertEquals(2, logParents.Length);

			var containers = logParents.OfType<ForwardingContainer>().ToArray();
			AssertEquals(2, containers.Length);
			AssertContainsExactElementsInAnyOrder("One container are created.", new[] { "CONT4444444", "CONT4444455" }, containers.Select(c => c.JC_ContainerNum));
			Assert("Container type is applied", containers.All(c => c.JC_RC == container1.JC_RC));
			AssertContainsExactElementsInAnyOrder(new[] { "CONT4444444", "CONT4444455" }, consol.Containers.Cast<ForwardingContainer>().Select(c => c.JC_ContainerNum));
		}

		public void TestIncomingEventShouldLinkToULDContainer_CreateContainerAndApplyContainerType_WithDuplicatedContainerID()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "MBL11111";
			consol.JK_BookingReference = "BKG22222";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_BookingReference = "BOOKS";
			consol.JK_MasterBillNum = "081";
			consol.JK_ConsolMode = "ULD";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT4444444";
			container1.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "PM-2H")).PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "HBL33333";
			consol.GridShipments.Add(shipment);

			Factory.Save();

			#region Event XML

			var uldEventXMLWithDuplicateContainerID = @"
		<UniversalEvent>
			<Event>
				<EventType>CCD</EventType>
				<EventTime>10-JUL-2010 18:00</EventTime>
				<EventReference>Dummy Description</EventReference>
				<DataProvider>Dummy</DataProvider>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>081</Value>
					</Context>
					<Context>
						<Type>CarriersBookingReference</Type>
						<Value>BOOKS</Value>
					</Context>
					<Context>
						<Type>ULDIdentification</Type>
						<Value>CONT4444444</Value>
					</Context>
					<Context>
						<Type>ULDIdentification</Type>
						<Value>CONT4444455</Value>
					</Context>
					<Context>
						<Type>ULDIdentification</Type>
						<Value>CONT4444455</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>";

			#endregion

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(uldEventXMLWithDuplicateContainerID);

			var subscriber = GetNewConsolEventParentFinder();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNotNull("logParents", logParents);
			AssertEquals(2, logParents.Length);

			var containers = logParents.OfType<ForwardingContainer>().ToArray();
			AssertEquals(2, containers.Length);
			AssertContainsExactElementsInAnyOrder("One container are created.", new[] { "CONT4444444", "CONT4444455" }, containers.Select(c => c.JC_ContainerNum));
			Assert("Container type is applied", containers.All(c => c.JC_RC == container1.JC_RC));
			AssertContainsExactElementsInAnyOrder(new[] { "CONT4444444", "CONT4444455" }, consol.Containers.Cast<ForwardingContainer>().Select(c => c.JC_ContainerNum));
		}

		public void TestIncomingEventShouldNotCreateContainer_WhenConsolModeIsLSE()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "MBL11111";
			consol.JK_BookingReference = "BKG22222";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_BookingReference = "BOOKS";
			consol.JK_MasterBillNum = "081";
			consol.JK_ConsolMode = "LSE";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "HBL33333";
			consol.GridShipments.Add(shipment);

			Factory.Save();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(ULDEventXML);

			var subscriber = GetNewConsolEventParentFinder();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNotNull("logParents", logParents);
			AssertEquals(1, logParents.Length);

			var containers = logParents.OfType<ForwardingContainer>().ToArray();
			AssertEquals("No containers should be created when Consol Mode is LSE", 0, containers.Length);
		}

		#region Event XML

		string ULDEventXML => @"
		<UniversalEvent>
			<Event>
				<EventType>CCD</EventType>
				<EventTime>10-JUL-2010 18:00</EventTime>
				<EventReference>Dummy Description</EventReference>
				<DataProvider>Dummy</DataProvider>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>081</Value>
					</Context>
					<Context>
						<Type>CarriersBookingReference</Type>
						<Value>BOOKS</Value>
					</Context>
					<Context>
						<Type>ULDIdentification</Type>
						<Value>CONT4444444</Value>
					</Context>
					<Context>
						<Type>ULDIdentification</Type>
						<Value>CONT4444455</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>";

		#endregion

		#endregion

		#region Implementation

		protected override ConsolAndShipmentEventParentFinder<ForwardingConsol, ForwardingShipment, ForwardingContainer> GetNewConsolEventParentFinder()
		{
			return new ConsolEventParentFinder<ForwardingConsol, ForwardingShipment, ForwardingContainer>(Factory, new ForwardingConsolDataContextManager(), new TestLogger(), new UniversalForwardingHelper());
		}

		#endregion
	}
}
