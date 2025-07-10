using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using DummyLogger = Enterprise.UniversalDataBuss.Integration.DummyLogger;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	public class JobDeclarationEventParentFinderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestLinkEventToTransportLegWhenThereIsOnlyOneContainerNumberForSea()
		{
			const string shipmentLevelEventXmlText = @"
<UniversalEvent>
	<Event>
		<EventType>ARV</EventType>
		<EventTime>10-JUL-2024 18:00</EventTime>
		<IsEstimate>true</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>HXU24072901</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>HXU24072902</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>HXU24072903</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>H072904</Value>
			</Context>
		</ContextCollection>
		<DataContext>
			<DataSource>
				<DataProvider>WTG Tracking & Automation</DataProvider>
			</DataSource>
		</DataContext>
	</Event>
</UniversalEvent>
";

			var matchingDeclaration = Factory.New<BaseJobDeclaration>();
			matchingDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			matchingDeclaration.JE_MasterBill = "HXU24072901";

			var container = matchingDeclaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "HXU24072902";

			var transportLeg = matchingDeclaration.Transports.AddNew();
			transportLeg.JW_Vessel = "HXU24072903";
			transportLeg.JW_VoyageFlight = "H072904";
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals(transportLeg, logParents[0]);

			transportLeg.Delete();
			Factory.SaveForTesting();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals(container.JobContainer, logParents[0]);
		}

		public void TestLinkEventToTransportLegWhenThereIsOnlyOneContainerNumberForAir()
		{
			const string shipmentLevelEventXmlText = @"
<UniversalEvent>
	<Event>
		<EventType>ARV</EventType>
		<EventTime>10-JUL-2024 18:00</EventTime>
		<IsEstimate>true</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>HXU24072901</Value>
			</Context>
			<Context>
				<Type>ULDIdentification</Type>
				<Value>HXU24072902</Value>
			</Context>
			<Context>
				<Type>FlightNumber</Type>
				<Value>H072904</Value>
			</Context>
		</ContextCollection>
		<DataContext>
			<DataSource>
				<DataProvider>WTG Tracking & Automation</DataProvider>
			</DataSource>
		</DataContext>
	</Event>
</UniversalEvent>
";

			var matchingDeclaration = Factory.New<BaseJobDeclaration_ContainersAlwaysRequired>();
			matchingDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			matchingDeclaration.JE_MasterBill = "HXU24072901";

			var container = matchingDeclaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "HXU24072902";

			var transportLeg = matchingDeclaration.Transports.AddNew();
			transportLeg.JW_TransportMode = "AIR";
			transportLeg.JW_VoyageFlight = "H072904";
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals(transportLeg, logParents[0]);

			transportLeg.Delete();
			Factory.SaveForTesting();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals(container.JobContainer, logParents[0]);
		}

		public void TestNotLinkEventToTransportLegForSeaAndAir_WhenEventTypeNotInARV_DEP_FLO_CAV_COF_RTC_SRC()
		{
			const string shipmentLevelEventXmlText = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2024 18:00</EventTime>
		<IsEstimate>true</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>CW124091001</Value>
			</Context>
			<Context>
				<Type>ULDIdentification</Type>
				<Value>CW124091002</Value>
			</Context>
			<Context>
				<Type>FlightNumber</Type>
				<Value>H072904</Value>
			</Context>
		</ContextCollection>
		<DataContext>
			<DataSource>
				<DataProvider>WTG Tracking & Automation</DataProvider>
			</DataSource>
		</DataContext>
	</Event>
</UniversalEvent>
";

			var matchingDeclaration = Factory.New<BaseJobDeclaration_ContainersAlwaysRequired>();
			matchingDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			matchingDeclaration.JE_MasterBill = "CW124091001";

			var container = matchingDeclaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CW124091002";

			var transportLeg = matchingDeclaration.Transports.AddNew();
			transportLeg.JW_TransportMode = "AIR";
			transportLeg.JW_VoyageFlight = "H072904";
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals(container.JobContainer, logParents.Single());
		}

		public void TestGenerateLogParentsForEventUsingContext()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedArabEmirates))
			{
				var factory = new BusinessObjectFactory();

				GlbCompany glbCompany1 = factory.New<GlbCompany>();
				glbCompany1.GC_Code = "XT";
				glbCompany1.GC_RN_NKCountryCode = "US";

				GlbBranch branch = glbCompany1.Branches.AddNew();
				branch.GB_GC = glbCompany1.PK;
				branch.GB_Code = "B1";

				var declaration2 = factory.New<BaseJobDeclaration>();
				declaration2.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration2.JE_DeclarationReference = "B00001119";
				declaration2.JE_HouseBill = "00200600001";
				declaration2.JE_MasterBill = "000-200001";
				declaration2.JE_GB = branch.PK;

				var declaration = factory.New<BaseJobDeclaration>();
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_DeclarationReference = "B00001119";
				declaration.JE_HouseBill = "20247600001";
				declaration.JE_MasterBill = "025-200001";
				declaration.JE_GB = GlbBranch.CurrentBranch.PK;
				factory.Save();

				TestErrorLogger logger = new TestErrorLogger();
				const string incomingEvent = @"
<UniversalEvent>
	<Event>
		<EventTime>2014-01-22T14:22:01</EventTime>
		<EventType>CLR</EventType>
		<EventReference></EventReference>
		<ContextCollection>
			<Context>
				<Type>prova tipo</Type>
				<Value>prova valore</Value>
			</Context>
			<Context>
				<Type>DeclarationReference</Type>
				<Value>B00001119</Value>
			</Context>
			<Context>
				<Type>EntryNumber</Type>
				<Value>123456</Value>
			</Context>
			<Context>
				<Type>EntryNumberType</Type>
				<Value>IMP</Value>
			</Context>
			<Context>
				<Type>EntryNumberCountryOfIssue</Type>
				<Value>AE</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

				var subscriber = GetNewEventParentFinderWithLogger(logger);
				var eventDeserializer = new XmlEventDeserializer();
				var xmlEvent = eventDeserializer.Parse(incomingEvent);
				UniversalEvent eventDataObject = xmlEvent as UniversalEvent;
				var logParents = subscriber.GenerateLogParentsForEventUsingContext(eventDataObject);
				AssertNotNull(logParents);

				var entry = (CusEntryHeader)logParents.First();
				AssertNotNull(entry);
				AssertEquals(entry.CH_BGMReference, "B00001119/123456");
				AssertEquals(entry.CH_JE, declaration.PK);
				AssertEquals(ZDateTime.Empty, entry.CH_EntryReleaseDate);
				AssertEquals(ZDateTime.Empty, entry.CH_EntryReleaseDate);
				AssertEquals(string.Empty, entry.CH_Status);
			}
		}

		public void TestGenerateLogParentsForEventUsingContext__WithMatchedFlightNumber()
		{
			var factory = new BusinessObjectFactory();

			var declaration = factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_DeclarationReference = "B00001119";
			declaration.JE_HouseBill = "20247600001";
			declaration.JE_MasterBill = "025-200001";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;

			var transport = declaration.Transports.AddNew();
			transport.JW_TransportMode = "AIR";
			transport.JW_VoyageFlight = "QF01";
			factory.Save();

			var logger = new TestErrorLogger();
			const string incomingEvent = @"
<UniversalEvent>
	<Event>
		<EventTime>2014-01-22T14:22:01</EventTime>
		<EventType>CLR</EventType>
		<EventReference></EventReference>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>025-200001</Value>
			</Context>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>20247600001</Value>
			</Context>
			<Context>
				<Type>FlightNumber</Type>
				<Value>QF1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingEvent);
			var eventDataObject = xmlEvent as UniversalEvent;
			var logParents = subscriber.GenerateLogParentsForEventUsingContext(eventDataObject);
			AssertNotNull(logParents);

			var parent = (Transport)logParents.First();
			AssertNotNull(parent);
			AssertEquals(parent.JW_VoyageFlight, "QF01");
		}

		public void TestGenerateLogParentsBasedOnNumberCountryAndTypeOnJobDeclaration()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00001119";

			var entryNumber = CusEntryNumber.LoadOrCreate(declaration, "ATL", Core.Constants.CountryCodes.Germany);
			entryNumber.CE_EntryNum = "TEST";

			Factory.SaveForTesting();

			var logger = new TestErrorLogger();
			const string incomingEvent = @"
<UniversalEvent>
	<Event>
		<ContextCollection>
			<Context>
				<Type>DeclarationReference</Type>
				<Value>B00001119</Value>
			</Context>
			<Context>
				<Type>EntryNumber</Type>
				<Value>TEST</Value>
			</Context>
			<Context>
				<Type>EntryNumberType</Type>
				<Value>ATL</Value>
			</Context>
			<Context>
				<Type>EntryNumberCountryOfIssue</Type>
				<Value>DE</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingEvent);
			var eventDataObject = xmlEvent as UniversalEvent;
			var logParents = subscriber.GenerateLogParentsForEventUsingContext(eventDataObject);
			AssertNotNull(logParents);

			var declarationMatched = (BaseJobDeclaration)logParents.First();
			AssertEquals("Declaration should be matched based on entry number/country/type.", declaration.PK, declarationMatched.PK);
		}

		public void TestGenerateLogParentsBasedOnNumberCountryAndTypeOnJobDeclarationWithMultipleDeclaration()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00001119";

			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_Code = "CP3";
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch3.GB_Code = "BR3";
			branch3.GB_GC = company3.PK;

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.JE_GB = branch3.PK;
			declaration2.JE_DeclarationReference = "B00001119";

			var entryNumber = CusEntryNumber.LoadOrCreate(declaration, "ATL", Core.Constants.CountryCodes.Germany);
			entryNumber.CE_EntryNum = "TEST";

			var entryNumber2 = CusEntryNumber.LoadOrCreate(declaration2, "ATL", Core.Constants.CountryCodes.Germany);
			entryNumber2.CE_EntryNum = "TEST";

			Factory.SaveForTesting();

			var logger = new TestErrorLogger();
			const string incomingEvent = @"
<UniversalEvent>
	<Event>
		<ContextCollection>
			<Context>
				<Type>DeclarationReference</Type>
				<Value>B00001119</Value>
			</Context>
			<Context>
				<Type>EntryNumber</Type>
				<Value>TEST</Value>
			</Context>
			<Context>
				<Type>EntryNumberType</Type>
				<Value>ATL</Value>
			</Context>
			<Context>
				<Type>EntryNumberCountryOfIssue</Type>
				<Value>DE</Value>
			</Context>
		</ContextCollection>
		<EventTime>2017-03-19T20:17:41</EventTime>
	<EventType>CLR</EventType>
	<EventReference>ZO</EventReference>
	</Event>
</UniversalEvent>";

			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingEvent);
			var eventDataObject = xmlEvent as UniversalEvent;
			var logParents = subscriber.GenerateLogParentsForEventUsingContext(eventDataObject);
			AssertNotNull(logParents);

			var declarationMatched = (BaseJobDeclaration)logParents.First();
			AssertEquals("Declaration should be matched based on entry number/country/type.", declaration.PK, declarationMatched.PK);

			var message = GetQueuedUniversalEventMessage(incomingEvent);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var sessionTracker = manager.Process(message);
			var attemptedImports = sessionTracker.ImportResults;
			AssertMultilineASCIIEquals("ProcessedOK", @"True|Linked Event to Declaration B00001119.|CustomsDeclaration-B00001119", attemptedImports.FormatAndOrderImportAttempts());
		}

		public void TestGenerateLogParentsBasedOnNumberCountryAndTypeOnCusEntryHeader()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_DeclarationReference = "B00001119";

				var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
				cusEntryHeader.EntryNumber = "123456";
				cusEntryHeader.CH_BGMReference = "112233445566";
				cusEntryHeader.CH_MessageType = "IMP";

				Factory.SaveForTesting();

				var logger = new TestErrorLogger();
				const string incomingEvent = @"
<UniversalEvent>
	<Event>
		<ContextCollection>
			<Context>
				<Type>DeclarationReference</Type>
				<Value>B00001119</Value>
			</Context>
			<Context>
				<Type>EntryNumber</Type>
				<Value>123456</Value>
			</Context>
			<Context>
				<Type>EntryNumberType</Type>
				<Value>MRN</Value>
			</Context>
			<Context>
				<Type>EntryNumberCountryOfIssue</Type>
				<Value>ZA</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

				var subscriber = GetNewEventParentFinderWithLogger(logger);
				var eventDeserializer = new XmlEventDeserializer();
				var xmlEvent = eventDeserializer.Parse(incomingEvent);
				var eventDataObject = xmlEvent as UniversalEvent;
				var logParents = subscriber.GenerateLogParentsForEventUsingContext(eventDataObject);
				AssertNotNull(logParents);

				var entry = (CusEntryHeader)logParents.First();
				AssertEquals("CusEntryHeader should be matched based on entry number/country/type.", cusEntryHeader.PK, entry.PK);
				AssertEquals("112233445566", entry.CH_BGMReference);
			}
		}

		public void TestGenerateLogParentsWhenReferenceHasDeclarationReferenceAsPrefix()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_DeclarationReference = "B00001119";

				var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
				cusEntryHeader.EntryNumber = "123456";
				cusEntryHeader.CH_BGMReference = "B00001119\\112233445566";
				cusEntryHeader.CH_MessageType = "IMP";

				Factory.SaveForTesting();

				AssertEquals("pre-Condition: There should only be one entry", 1, declaration.CustomsEntryHeaders.Count);

				var logger = new TestErrorLogger();
				const string incomingEvent = @"
<UniversalEvent>
	<Event>
		<ContextCollection>
			<Context>
				<Type>DeclarationReference</Type>
				<Value>B00001119</Value>
			</Context>
			<Context>
				<Type>EntryNumber</Type>
				<Value>123456</Value>
			</Context>
			<Context>
				<Type>EntryNumberType</Type>
				<Value>MRN</Value>
			</Context>
			<Context>
				<Type>EntryNumberCountryOfIssue</Type>
				<Value>ZA</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

				var subscriber = GetNewEventParentFinderWithLogger(logger);
				var eventDeserializer = new XmlEventDeserializer();
				var xmlEvent = eventDeserializer.Parse(incomingEvent);
				var eventDataObject = xmlEvent as UniversalEvent;
				var logParents = subscriber.GenerateLogParentsForEventUsingContext(eventDataObject);
				AssertNotNull(logParents);

				var entry = (CusEntryHeader)logParents.First();
				AssertEquals("There should still only be one entry", 1, declaration.CustomsEntryHeaders.Count);
				AssertEquals("CusEntryHeader should be matched based on prefix/entry number/country/type.", cusEntryHeader.PK, entry.PK);
				AssertEquals("B00001119\\112233445566", entry.CH_BGMReference);
			}
		}

		public void TestUpdateEntrystatusWhenEmpty()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00001119";
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "123456";
			cusEntryHeader.CH_EntryStatus = "";
			Factory.SaveForTesting();
			var logger = new TestErrorLogger();
			const string incomingEvent = @"
	<UniversalEvent>
		<Event>
			<EventTime>2015-10-19T20:17:41</EventTime>
			<EventType>CES</EventType>
			<EventReference>CES</EventReference>
			<ContextCollection>
				<Context>
				<Type>EntryNumber</Type>
					<Value>123456</Value>
				</Context>
				<Context>
				<Type>DeclarationReference</Type>
					<Value>B00001119</Value>
				</Context>
				<Context>
				<Type>EntryNumberType</Type>
					<Value>MRN</Value>
				</Context>
				<Context>
				<Type>EntryNumberCountryOfIssue</Type>
					<Value>DE</Value>
				</Context>
			</ContextCollection>
		</Event>
	</UniversalEvent>
";

			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingEvent);
			var eventDataObject = xmlEvent as UniversalEvent;
			var logParents = subscriber.GenerateLogParentsForEventUsingContext(eventDataObject);
			var entry = (CusEntryHeader)logParents.First();
			AssertEquals("CH_EntryStatus should be updated", "CES", entry.CH_EntryStatus);
		}

		public void TestCanImportEntryWithInvalidMessageFieldLength()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00001119";
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "123456";
			cusEntryHeader.CH_EntryStatus = "";
			Factory.SaveForTesting();
			var logger = new TestErrorLogger();
			const string incomingEvent = @"
	<UniversalEvent>
		<Event>
			<EventTime>2015-10-19T20:17:41</EventTime>
			<EventType>CES</EventType>
			<EventReference>Rejected</EventReference>
			<ContextCollection>
				<Context>
				<Type>EntryNumber</Type>
					<Value>1234567890</Value>
				</Context>
				<Context>
				<Type>DeclarationReference</Type>
					<Value>B00001119</Value>
				</Context>
				<Context>
				<Type>EntryNumberType</Type>
					<Value>MRNR</Value>
				</Context>
				<Context>
				<Type>EntryNumberCountryOfIssue</Type>
					<Value>DE</Value>
				</Context>
			</ContextCollection>
		</Event>
	</UniversalEvent>
";

			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingEvent);
			var eventDataObject = xmlEvent as UniversalEvent;
			var logParents = subscriber.GenerateLogParentsForEventUsingContext(eventDataObject);
			var entry = (CusEntryHeader)logParents.First();
			AssertEquals("CH_MessageType should be updated", "MRN", entry.CH_MessageType);
			AssertEquals("CH_BGMReference should be updated", "B00001119/1234567890", entry.CH_BGMReference);
			AssertEquals("CH_EntryStatus should be updated", "Rej", entry.CH_EntryStatus);
		}

		public void TestUpdateReleaseDateWhenEmpty()
		{
			var boFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(boFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CustomsStatus");
			var cusCodeList = helper.CreateNewOrGetExistingCusCodeList("DE", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CES", "CES", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeList.PK, RefCusCodeListAttributeTypes.Codes.IUpdateReleaseDate, "");
			boFactory.Save();
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00001119";
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "123456";
			cusEntryHeader.CH_EntryStatus = "";
			Factory.SaveForTesting();
			var logger = new TestErrorLogger();
			var incomingEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<EventTime>2015-04-16T08:12:21.637</EventTime>
			<EventType>CES</EventType>
			<EventReference>CES</EventReference>
			<ContextCollection>
				<Context>
				<Type>EntryNumber</Type>
					<Value>123456</Value>
				</Context>
				<Context>
				<Type>DeclarationReference</Type>
					<Value>B00001119</Value>
				</Context>
				<Context>
				<Type>EntryNumberType</Type>
					<Value>MRN</Value>
				</Context>
				<Context>
				<Type>EntryNumberCountryOfIssue</Type>
					<Value>DE</Value>
				</Context>
			</ContextCollection>
		</Event>
	</UniversalEvent>
";
			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingEvent);
			var eventDataObject = xmlEvent as UniversalEvent;
			var logParents = subscriber.GenerateLogParentsForEventUsingContext(eventDataObject);
			var entry = (CusEntryHeader)logParents.First();
			AssertEquals(new ZDateTime(2015, 04, 16, 08, 12, 21, 637), entry.CH_EntryReleaseDate);
		}

		public void TestGenerateLogParentsForIntegratedCountryIfNoMatchedFound()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00001119";

			Factory.SaveForTesting();

			var logger = new TestErrorLogger();
			const string incomingEvent = @"
<UniversalEvent>
	<Event>
		<EventTime>2014-01-22T14:22:01</EventTime>
		<EventType>IMP</EventType>
		<ContextCollection>
			<Context>
				<Type>DeclarationReference</Type>
				<Value>B00001119</Value>
			</Context>
			<Context>
				<Type>EntryNumber</Type>
				<Value>123456</Value>
			</Context>
			<Context>
				<Type>EntryNumberType</Type>
				<Value>IMP</Value>
			</Context>
			<Context>
				<Type>EntryNumberCountryOfIssue</Type>
				<Value>CN</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingEvent);
			var eventDataObject = xmlEvent as UniversalEvent;
			var logParents = subscriber.GenerateLogParentsForEventUsingContext(eventDataObject);
			AssertNotNull(logParents);

			var entry = logParents.First();
			AssertNotNull("CusEntryHeader should be created, no matches are found.", entry);
			Assert("CusEntryHeader should be created, no matches are found.", entry is CusEntryHeader);
			AssertEquals("B00001119/123456", ((CusEntryHeader)entry).CH_BGMReference);
		}

		public void TestGenerateLogParentsForNonIntegratedCountry()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00001119";

			var entryNumber = CusEntryNumber.LoadOrCreate(declaration, "ENS", Core.Constants.CountryCodes.UnitedStates);
			entryNumber.CE_EntryNum = "00000295";

			Factory.SaveForTesting();

			var logger = new TestErrorLogger();
			const string incomingEvent = @"
<UniversalEvent>
	<Event>
		<EventTime>2014-01-22T14:22:01</EventTime>
		<EventType>IMP</EventType>
		<ContextCollection>
			<Context>
				<Type>DeclarationReference</Type>
				<Value>B00001119</Value>
			</Context>
			<Context>
				<Type>EntryNumber</Type>
				<Value>123456</Value>
			</Context>
			<Context>
				<Type>EntryNumberType</Type>
				<Value>IMP</Value>
			</Context>
			<Context>
				<Type>EntryNumberCountryOfIssue</Type>
				<Value>US</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingEvent);
			var eventDataObject = xmlEvent as UniversalEvent;
			var logParents = subscriber.GenerateLogParentsForEventUsingContext(eventDataObject);
			AssertNull("Noting should be matched.", logParents);
		}

		public void TestGenerateLogParentsIfJobReferenceSpecifiedButDoesnotExist()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00001119";

			Factory.SaveForTesting();

			var logger = new TestErrorLogger();
			const string incomingEvent = @"
<UniversalEvent>
	<Event>
		<EventTime>2014-01-22T14:22:01</EventTime>
		<EventType>IMP</EventType>
		<ContextCollection>
			<Context>
				<Type>DeclarationReference</Type>
				<Value>B00009999</Value>
			</Context>
			<Context>
				<Type>EntryNumber</Type>
				<Value>123456</Value>
			</Context>
			<Context>
				<Type>EntryNumberType</Type>
				<Value>IMP</Value>
			</Context>
			<Context>
				<Type>EntryNumberCountryOfIssue</Type>
				<Value>CN</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingEvent);
			var eventDataObject = xmlEvent as UniversalEvent;
			var logParents = subscriber.GenerateLogParentsForEventUsingContext(eventDataObject);
			AssertNull(logParents);
			AssertEquals("Error - Cannot find any matching declaration for declaration reference 'B00009999'.", logger.Logs);
		}

		public void TestBGMReferenceLoggerThanMaxLength()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy);
			var factory = new BusinessObjectFactory();
			var declaration = factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_DeclarationReference = "B00001009";
			declaration.JE_HouseBill = "20247600001";
			declaration.JE_MasterBill = "025-200001";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			factory.Save();

			TestErrorLogger logger = new TestErrorLogger();
			const string incomingEvent = @"
			<UniversalEvent>
						<Event>
							<EventTime>2014-01-22T14:22:01</EventTime>
							<EventType>CLR</EventType>
							<EventReference></EventReference>
						<ContextCollection>
							<Context>
								<Type>prova tipo</Type>
								<Value>prova valore</Value>
							</Context>
							<Context>
							<Type>DeclarationReference</Type>
								<Value>B00001009</Value>
							</Context>
						<Context>
						<Type>EntryNumber</Type>
							<Value>012345678901234567890123456789</Value>
						</Context>
						<Context>
						<Type>EntryNumberType</Type>
							<Value>IMP</Value>
						</Context>
						<Context>
						<Type>EntryNumberCountryOfIssue</Type>
							<Value>IT</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>";

			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingEvent);
			UniversalEvent eventDataObject = xmlEvent as UniversalEvent;
			var logParents = subscriber.GenerateLogParentsForEventUsingContext(eventDataObject);
			var expectedBGMReference = xmlEvent.Context.DeclarationReference + "/" + xmlEvent.Context.EntryNumber;
			AssertNull(logParents);
			AssertContains(logger.GetErrors(), string.Format("Cannot create a new Entry for Declaration {0} as the combination of Declaration Reference and Entry Number ('{1}') is longer than {2} characters.",
							declaration.JE_DeclarationReference, expectedBGMReference, CusEntryHeader.Schema.CH_BGMReferenceMaxLength));
		}

		public void TestNoAnyDeclaration()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy);
			var logger = new TestErrorLogger();
			const string incomingEvent = @"
			<UniversalEvent>
						<Event>
							<EventTime>2014-01-22T14:22:01</EventTime>
							<EventType>CLR</EventType>
							<EventReference></EventReference>
						<ContextCollection>
							<Context>
								<Type>prova tipo</Type>
								<Value>prova valore</Value>
							</Context>
							<Context>
							<Type>DeclarationReference</Type>
								<Value>B00001119</Value>
							</Context>
						<Context>
						<Type>EntryNumber</Type>
							<Value>12456</Value>
						</Context>
						<Context>
						<Type>EntryNumberType</Type>
							<Value>IMP</Value>
						</Context>
						<Context>
						<Type>EntryNumberCountryOfIssue</Type>
							<Value>IT</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>";
			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingEvent);
			var eventDataObject = xmlEvent as UniversalEvent;
			var logParents = subscriber.GenerateLogParentsForEventUsingContext(eventDataObject);
			AssertNull(logParents);
		}

		[TestDate(2013, 11, 18, 0, 0, 0)]
		public void TestIncomingEventLinksToDeclarationsPreAndDuringMAWBRecyclePeriod()
		{
			var logger = new TestErrorLogger();

			const string transportLevelEventXmlText1 = @"
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
</UniversalEvent>
";

			var matchingDeclaration1 = Factory.New<BaseJobDeclaration>();
			matchingDeclaration1.JE_TransportMode = Core.Constants.TransportModes.Sea;
			matchingDeclaration1.JE_DeclarationReference = "BJOB1";
			matchingDeclaration1.JE_HouseBill = "20257654321";
			matchingDeclaration1.JE_SystemCreateTimeUtc = new ZDateTime(2010, 7, 10, 17, 55, 0);
			matchingDeclaration1.JE_MasterBill = "081-203212";
			var masterBill1 = matchingDeclaration1.Bills.AddNew();
			masterBill1.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill1.CU_BillNum = "072343343";
			var houseBill1 = masterBill1.ChildBills.AddNew();
			houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill1.CU_BillNum = "20257654321";

			var matchingTransport = matchingDeclaration1.Transports.AddNew();
			matchingTransport.JW_Vessel = "MAGIC SCHOOL BUS";
			matchingTransport.JW_VoyageFlight = "12234";
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinderWithLogger(logger);

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(transportLevelEventXmlText1);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("Should match zero LogParents", logParents);
			AssertContains("Should have excluded declaration", "Warning - Declaration BJOB1 has been excluded due to it being more than 4 months old.", logger.Logs);

			const string transportLevelEventXmlText2 = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2013 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>20257654322</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>12235</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>YELLOW SUBMARINE</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var matchingDeclaration2 = Factory.New<BaseJobDeclaration>();
			matchingDeclaration2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			matchingDeclaration2.JE_DeclarationReference = "BJOB2";
			matchingDeclaration2.JE_HouseBill = "20257654322";
			matchingDeclaration2.JE_SystemCreateTimeUtc = new ZDateTime(2013, 8, 10, 17, 55, 0);
			matchingDeclaration2.JE_MasterBill = "081-203213";
			var masterBill2 = matchingDeclaration2.Bills.AddNew();
			masterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "072343343";
			var houseBill2 = masterBill2.ChildBills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_BillNum = "20257654322";

			matchingTransport = matchingDeclaration2.Transports.AddNew();
			matchingTransport.JW_Vessel = "YELLOW SUBMARINE";
			matchingTransport.JW_VoyageFlight = "12235";
			Factory.SaveForTesting();

			subscriber = GetNewEventParentFinderWithLogger(logger);

			eventDeserializer = new XmlEventDeserializer();
			xmlEvent = eventDeserializer.Parse(transportLevelEventXmlText2);

			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertNotContains("Should not have excluded declaration", "Warning - Declaration BJOB2 has been excluded due to it being more than 4 months old.", logger.Logs);

			const string transportLevelEventXmlText3 = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>19-NOV-2013 00:05</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>20257654323</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>12236</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>CHITTY CHITTY BANG BANG</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var matchingDeclaration3 = Factory.New<BaseJobDeclaration>();
			matchingDeclaration3.JE_TransportMode = Core.Constants.TransportModes.Sea;
			matchingDeclaration3.JE_DeclarationReference = "BJOB3";
			matchingDeclaration3.JE_HouseBill = "20257654323";
			matchingDeclaration3.JE_SystemCreateTimeUtc = new ZDateTime(2013, 11, 19, 0, 0, 0);
			matchingDeclaration3.JE_MasterBill = "081-203213";
			var masterBill3 = matchingDeclaration3.Bills.AddNew();
			masterBill3.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill3.CU_BillNum = "072343343";
			var houseBill3 = masterBill2.ChildBills.AddNew();
			houseBill3.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill3.CU_BillNum = "20257654323";

			matchingTransport = matchingDeclaration3.Transports.AddNew();
			matchingTransport.JW_Vessel = "CHITTY CHITTY BANG BANG";
			matchingTransport.JW_VoyageFlight = "12236";
			Factory.SaveForTesting();

			subscriber = GetNewEventParentFinderWithLogger(logger);

			eventDeserializer = new XmlEventDeserializer();
			xmlEvent = eventDeserializer.Parse(transportLevelEventXmlText3);

			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match two LogParents", 2, logParents.Length);
			AssertNotContains("Should not have excluded declaration", "Warning - Declaration BJOB3 has been excluded due to it being more than 4 months old.", logger.Logs);
		}

		public void TestGOUEventLinkedToContainerUpdatesWharfGateOut()
		{
			var expectedGateOutDate = new ZDateTime(2016, 11, 13, 06, 21, 0);
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "AALSMEERGRACHT";
			vessel.RV_LloydsNumber = "9044748";

			var matchingDeclaration = Factory.New<BaseJobDeclaration>();
			matchingDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			matchingDeclaration.JE_VesselName = "AALSMEERGRACHT";
			matchingDeclaration.JE_VoyageFlightNo = "919P";
			matchingDeclaration.JE_RL_NKPortOfArrival = "AUMEL";

			var cusContainer = matchingDeclaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "GINU8377360";
			Factory.SaveForTesting();

			AssertEquals("WharfGateOut date pre-condition:", ZDateTime.Empty, cusContainer.JobContainer.JC_FCLWharfGateOut);
			var message = GetQueuedUniversalEventMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalTestFiles("UniversalGOUEvent.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"Linked Event to Container 'GINU8377360'.", serviceTaskLog.ToString());
			AssertContains("Message Log Note", @"Linked Event to Container 'GINU8377360'.", message.GetLogNoteText());
			AssertEquals("WharfGateOut date should have been updated", expectedGateOutDate, cusContainer.JobContainer.JC_FCLWharfGateOut);
		}

		public void TestGINEventLinkedToContainerUpdatesWharfGateIn()
		{
			var expectedGateInDate = new ZDateTime(2015, 06, 12, 06, 21, 0);
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "AALSMEERGRACHT";
			vessel.RV_LloydsNumber = "9044748";

			var matchingDeclaration = Factory.New<BaseJobDeclaration>();
			matchingDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			matchingDeclaration.JE_VesselName = "AALSMEERGRACHT";
			matchingDeclaration.JE_VoyageFlightNo = "1711";
			matchingDeclaration.JE_RL_NKPortOfArrival = "AUSYD";

			var cusContainer = matchingDeclaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "TFMO5265984";
			Factory.SaveForTesting();

			AssertEquals("WharfGateIn date pre-condition:", ZDateTime.Empty, cusContainer.JobContainer.JC_FCLWharfGateIn);
			var message = GetQueuedUniversalEventMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalTestFiles("UniversalGINEvent.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"Linked Event to Container 'TFMO5265984'.", serviceTaskLog.ToString());
			AssertContains("Message Log Note", @"Linked Event to Container 'TFMO5265984'.", message.GetLogNoteText());
			AssertEquals("WharfGateIn date (JC_ContainerYardEmptyReturnGateIn for Import) should have been updated", expectedGateInDate, cusContainer.JobContainer.JC_ContainerYardEmptyReturnGateIn);
		}

		public void TestBrokerageJobOnShipmentDoesNotUpdateEventTwice()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "AALSMEERGRACHT";
			vessel.RV_LloydsNumber = "9044748";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.JK_RL_NKLoadPort = "HKWNI";
			consol.JK_MasterBillNum = "MB324334";
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "GINU8377360";

			var transportLeg = consol.Transports[0];
			transportLeg.JW_Vessel = "AALSMEERGRACHT";
			transportLeg.JW_VoyageFlight = "919P";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HB9863521";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.JS_RL_NKDestination = "AUMEL";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var shipmentDeclaration = Factory.New<BaseJobDeclaration>();
			shipmentDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentDeclaration.JE_VesselName = "AALSMEERGRACHT";
			shipmentDeclaration.JE_VoyageFlightNo = "919P";
			shipmentDeclaration.JE_JS = shipment.PK;

			var cusContainer = shipmentDeclaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "GINU8377360";
			Factory.SaveForTesting();

			container.Reload();
			AssertEquals("WharfGateOut date pre-condition:", ZDateTime.Empty, container.JC_FCLWharfGateOut);
			var message = GetQueuedUniversalEventMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalTestFiles("UniversalGOUEvent.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"Linked Event to Container 'GINU8377360'.", serviceTaskLog.ToString());
			AssertContains("Message Log Note", @"Linked Event to Container 'GINU8377360'.", message.GetLogNoteText());
			AssertEquals("WharfGateOut date should have been updated", new ZDateTime(2016, 11, 13, 06, 21, 0), container.JC_FCLWharfGateOut);
			AssertEquals("But no two GOU events", 1, container.Logs.Find(x => x.SL_SE_NKEvent == "GOU").Count());
		}

		public void TestDeclarationJobNumberAndContainerNumberOnEventMatchToRightContainer()
		{
			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "ZUS";
			usCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			usCompany.GC_RN_NKCountryCode = "US";
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "ZBS";
			usBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var usDec = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.US.IJobDeclaration>());
			usDec.JE_GB = usBranch.PK;
			usDec.JE_DeclarationReference = "B00001535";
			var usDecContainer = usDec.CusContainers.AddNew();
			usDecContainer.CO_ContainerNumber = "OOCL0000011";

			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_DeclarationReference = "B00001535";
			var declaration1container1 = declaration1.CusContainers.AddNew();
			declaration1container1.CO_ContainerNumber = "OOCL0000006";
			var declaration1container2 = declaration1.CusContainers.AddNew();
			declaration1container2.CO_ContainerNumber = "OOCL0000011";

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_DeclarationReference = "B00001536";
			var declaration2container1 = declaration2.CusContainers.AddNew();
			declaration2container1.CO_ContainerNumber = "OOCL0000011";

			Factory.SaveForTesting();
			var builder = new ZStringBuilder();

			const string eventXML1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>CustomsDeclaration</Type>
		  <Key>B00001535</Key>
		</DataTarget>
	  </DataTargetCollection>
	  <CodesMappedToTarget>true</CodesMappedToTarget>
	  <Company>
		<Code>EDI</Code>
		<Name>Eagle Does Ignateous</Name>
	  </Company>
	</DataContext>

	<EventTime>2011-04-19T07:57:22.873</EventTime>
	<EventType>BKD</EventType>
	<IsEstimate>false</IsEstimate>

	<ContextCollection>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>OOCL0000011</Value>
	  </Context>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>OOCL0000012</Value>
	  </Context>
	  <Context>
		<Type>MBOLOriginUNLOCO</Type>
		<Value>AUSYD</Value>
	  </Context>
	  <Context>
		<Type>MBOLDestinationUNLOCO</Type>
		<Value>ZAJNB</Value>
	  </Context>
	</ContextCollection>
  </Event>
</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(eventXML1);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Container 'OOCL0000011'.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Container 'OOCL0000011'.
".Trim(), message.GetLogNoteText());

				usDecContainer.Reload();
				var logs = usDecContainer.JobContainer.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count should be 0 as not right company", 0, logs.Length);

				declaration1container2.Reload();
				logs = declaration1container2.JobContainer.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertEquals("Context Items on Event", @"
Container Number - OOCL0000011
Container Number - OOCL0000012
MBOL Origin UNLOCO - AUSYD
MBOL Destination UNLOCO - ZAJNB
Data Source Company - EDI - Eagle Does Ignateous
".Trim(), actualContextItems);
			});

			const string eventXML2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>CustomsDeclaration</Type>
		  <Key>B00001535</Key>
		</DataTarget>
	  </DataTargetCollection>
	  <CodesMappedToTarget>true</CodesMappedToTarget>
	  <Company>
		<Code>ZUS</Code>
		<Name>US Company</Name>
	  </Company>
	</DataContext>

	<EventTime>2011-04-19T07:57:22.873</EventTime>
	<EventType>ATH</EventType>
	<IsEstimate>false</IsEstimate>

	<ContextCollection>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>OOCL0000011</Value>
	  </Context>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>OOCL0000012</Value>
	  </Context>
	  <Context>
		<Type>MBOLOriginUNLOCO</Type>
		<Value>AUSYD</Value>
	  </Context>
	  <Context>
		<Type>MBOLDestinationUNLOCO</Type>
		<Value>ZAJNB</Value>
	  </Context>
	</ContextCollection>
  </Event>
</UniversalEvent>";

			message = GetQueuedUniversalEventMessage(eventXML2);

			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Container 'OOCL0000011'.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Container 'OOCL0000011'.
".Trim(), message.GetLogNoteText());

				declaration1container2.Reload();
				var logs = declaration1container2.JobContainer.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));
				AssertEquals("[ATH] - Action Authorised event count should be 0 as not right company", 0, logs.Length);

				usDecContainer.Reload();
				logs = usDecContainer.JobContainer.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));
				AssertEquals("[ATH] - Action Authorised count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertEquals("Context Items on Event", @"
Container Number - OOCL0000011
Container Number - OOCL0000012
MBOL Origin UNLOCO - AUSYD
MBOL Destination UNLOCO - ZAJNB
Data Source Company - ZUS - US Company
".Trim(), actualContextItems);
			});
		}

		public void TestDeclarationJobNumberAndLegDetailsOnEventMatchToRightLeg()
		{
			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "ZUS";
			usCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			usCompany.GC_RN_NKCountryCode = "US";
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "ZBS";
			usBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var usDec = Factory.New<BaseJobDeclaration>();
			usDec.JE_GB = usBranch.PK;
			usDec.JE_DeclarationReference = "B00001535";
			usDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var usDecTransport = usDec.Transports.AddNew();
			usDecTransport.JW_Vessel = "MAGIC SCHOOL BUS";
			usDecTransport.JW_VoyageFlight = "12234";

			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration1.JE_DeclarationReference = "B00001535";
			var declaration1Transport1 = declaration1.Transports.AddNew();
			declaration1Transport1.JW_Vessel = "MAGIC SCHOOL TRUCK";
			declaration1Transport1.JW_VoyageFlight = "56856";
			var declaration1Transport2 = declaration1.Transports.AddNew();
			declaration1Transport2.JW_Vessel = "MAGIC SCHOOL BUS";
			declaration1Transport2.JW_VoyageFlight = "12234";

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_DeclarationReference = "B00001536";
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var declaration2transport1 = declaration2.Transports.AddNew();
			declaration2transport1.JW_Vessel = "MAGIC SCHOOL BUS";
			declaration2transport1.JW_VoyageFlight = "12234";

			var builder = new ZStringBuilder();

			Factory.SaveForTesting();

			const string eventXML2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
			<DataTarget>
				<Type>CustomsDeclaration</Type>
				<Key>B00001535</Key>
			</DataTarget>
			</DataTargetCollection>
			<CodesMappedToTarget>true</CodesMappedToTarget>
			<Company>
			<Code>EDI</Code>
			<Name>Eagle Does Ignateous</Name>
			</Company>
		</DataContext>

		<EventTime>2011-04-19T07:57:22.873</EventTime>
		<EventType>BKD</EventType>
		<IsEstimate>false</IsEstimate>

		<ContextCollection>
			<Context>
				<Type>MBOLOriginUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>MBOLDestinationUNLOCO</Type>
				<Value>ZAJNB</Value>
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
</UniversalEvent>
";

			var message = GetQueuedUniversalEventMessage(eventXML2);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Transport Leg (Vessel='MAGIC SCHOOL BUS', Voyage='12234', Carrier='').
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Transport Leg (Vessel='MAGIC SCHOOL BUS', Voyage='12234', Carrier='').
".Trim(), message.GetLogNoteText());

				usDecTransport.Reload();
				var logs = usDecTransport.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count should be 0 as not right company", 0, logs.Length);

				declaration1Transport2.Reload();
				logs = declaration1Transport2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertEquals("Context Items on Event", @"
MBOL Origin UNLOCO - AUSYD
MBOL Destination UNLOCO - ZAJNB
Voyage Number - 12234
Vessel Name - MAGIC SCHOOL BUS
Data Source Company - EDI - Eagle Does Ignateous
".Trim(), actualContextItems);
			});

			const string eventXML3 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
			<DataTarget>
				<Type>CustomsDeclaration</Type>
				<Key>B00001535</Key>
			</DataTarget>
			</DataTargetCollection>
			<CodesMappedToTarget>true</CodesMappedToTarget>
			<Company>
			<Code>ZUS</Code>
			<Name>US Company</Name>
			</Company>
		</DataContext>

		<EventTime>2011-04-19T07:57:22.873</EventTime>
		<EventType>CCC</EventType>
		<IsEstimate>false</IsEstimate>

		<ContextCollection>
			<Context>
				<Type>MBOLOriginUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>MBOLDestinationUNLOCO</Type>
				<Value>ZAJNB</Value>
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
</UniversalEvent>
";
			message = GetQueuedUniversalEventMessage(eventXML3);

			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Transport Leg (Vessel='MAGIC SCHOOL BUS', Voyage='12234', Carrier='').
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Transport Leg (Vessel='MAGIC SCHOOL BUS', Voyage='12234', Carrier='').
".Trim(), message.GetLogNoteText());

				declaration1Transport2.Reload();
				var logs = declaration1Transport2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCommencedCode));
				AssertEquals("[CCC] - Customs Commenced event count should be 0 as not right company", 0, logs.Length);

				usDecTransport.Reload();
				logs = usDecTransport.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCommencedCode));
				AssertEquals("[CCC] - Customs Commenced count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertEquals("Context Items on Event", @"
MBOL Origin UNLOCO - AUSYD
MBOL Destination UNLOCO - ZAJNB
Voyage Number - 12234
Vessel Name - MAGIC SCHOOL BUS
Data Source Company - ZUS - US Company
".Trim(), actualContextItems);
			});
		}

		public void TestDoesNotTryAndApplyEventsToADeclarationLinkedShipmentWhenThereIsAlreadyAMatchOnTheParentShipment()
		{
			const string shipmentLevelEventXmlText = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<ContextCollection>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>20257654321</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var matchingShipment = Factory.New<ForwardingShipment>();
			matchingShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			matchingShipment.JS_HouseBill = "20257654321";
			matchingShipment.JS_UniqueConsignRef = "S00001286";
			var matchingDeclaration = Factory.New<BaseJobDeclaration>();
			matchingDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			matchingDeclaration.JE_HouseBill = "20257654321";
			matchingDeclaration.JE_JS = matchingShipment.PK;
			matchingDeclaration.JE_DeclarationReference = "S00001286";

			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_MessageText = shipmentLevelEventXmlText;
			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Shipment S00001286 (House Bill='20257654321').
".Trim(), serviceTaskLog.ToString());

			AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Shipment S00001286 (House Bill='20257654321').
".Trim(), message.GetLogNoteText());

			var ccdLogs = matchingShipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "CCD"));
			AssertEquals("ccdLogs.Length", 1, ccdLogs.Length);
			var athLog = ccdLogs[0];
			AssertEquals("ccdLogs.SL_EventTime", new ZDateTime(2010, 7, 10, 18, 0, 0), athLog.SL_EventTime);

			ccdLogs = matchingDeclaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "CCD"));
			AssertEquals("CCD event should be added to relatedShipment when the declaration is linked to a relatedShipment", 0, ccdLogs.Length);
		}

		public void TestIncomingEventLinksToTheLatestDeclarationWhenUsingHBOLOnly()
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
</UniversalEvent>
";

			var matchingDeclaration = Factory.New<BaseJobDeclaration>();
			matchingDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			matchingDeclaration.JE_HouseBill = "20257654321";
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals("Should get best matching Declaration.", GetHumanReadableID(matchingDeclaration), GetHumanReadableID(logParents[0]));
		}

		public void TestIncomingEventLinksToMultipleDeclarationsWhenUsingHAWBOnly()
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
</UniversalEvent>
";

			var matchingDeclaration1 = Factory.New<BaseJobDeclaration>();
			matchingDeclaration1.JE_TransportMode = Core.Constants.TransportModes.Air;
			matchingDeclaration1.JE_MasterBill = "12345678901";
			matchingDeclaration1.JE_HouseBill = "20257654321";
			var matchingDeclaration2 = Factory.New<BaseJobDeclaration>();
			matchingDeclaration2.JE_TransportMode = Core.Constants.TransportModes.Air;
			matchingDeclaration2.JE_MasterBill = "12345678902";
			matchingDeclaration2.JE_HouseBill = "20257654321";
			var matchingDeclaration3 = Factory.New<BaseJobDeclaration>();
			matchingDeclaration3.JE_TransportMode = Core.Constants.TransportModes.Air;
			matchingDeclaration3.JE_MasterBill = "12345678903";
			matchingDeclaration3.JE_HouseBill = "20257654321";
			var oldDeclaration = Factory.New<BaseJobDeclaration>();
			oldDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			oldDeclaration.JE_MasterBill = "12345678904";
			oldDeclaration.JE_HouseBill = "20257654321";
			oldDeclaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddYears(-1);
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = new TypedEnumerable<BaseJobDeclaration>(subscriber.GetLogParentsForEvent(xmlEvent));
			AssertEquals("Should match one LogParent", 3, logParents.Count());
			AssertCollectionNotContains("Should not match Declaration '12345678904' as it's too old", oldDeclaration, logParents);
			AssertEquals("Should get match Declaration '12345678901'.", GetHumanReadableID(matchingDeclaration1), GetHumanReadableID(logParents.First(x => x.JE_MasterBill == "12345678901")));
			AssertEquals("Should get match Declaration '12345678902'.", GetHumanReadableID(matchingDeclaration2), GetHumanReadableID(logParents.First(x => x.JE_MasterBill == "12345678902")));
			AssertEquals("Should get match Declaration '12345678903'.", GetHumanReadableID(matchingDeclaration3), GetHumanReadableID(logParents.First(x => x.JE_MasterBill == "12345678903")));
		}

		public void TestIncomingEventLinksToTheLatestDeclarationWhenUsingHAWBOnly()
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
</UniversalEvent>
";

			var matchingDeclaration = Factory.New<BaseJobDeclaration>();
			matchingDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			matchingDeclaration.JE_HouseBill = "20257654321";
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals("Should get best matching Declaration.", GetHumanReadableID(matchingDeclaration), GetHumanReadableID(logParents[0]));
		}

		public void TestIncomingEventLinksToContainerWhenUsingMBOLHBOLAndContainerNumberAndFallsBackToDeclarationIfCannotMatchContainer()
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
				<Value>BOL57654321</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>20257654321</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>10137654321</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>10137654322</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "MBOLNumber";
			var consolContainer = consol.Containers.AddNew();
			consolContainer.JC_ContainerNum = "10137654321";
			var shipment = consol.Shipments.AddNew();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			declaration.JE_MasterBill = "BOL57654321";
			declaration.JE_HouseBill = "20257654321";
			consol.JK_RL_NKLoadPort = declaration.CountryCode + "ZZZ";
			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "10137654321";
			AssertEquals(consolContainer, cusContainer.JobContainer);
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals("Should get best matching Container.", GetHumanReadableID(consolContainer), GetHumanReadableID(logParents[0]));

			cusContainer.CO_ContainerNumber = "9856456805";
			AssertEquals(consolContainer, cusContainer.JobContainer);
			xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals("Should get best matching Shipment.", GetHumanReadableID(declaration), GetHumanReadableID(logParents[0]));
		}

		public void TestIncomingEventLinksToContainerWhenUsingHBOLAndContainerNumberAndFallsBackToDeclarationIfCannotMatchContainer()
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
				<Type>ContainerNumber</Type>
				<Value>10137654322</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var matchingDeclaration = Factory.New<BaseJobDeclaration>();
			matchingDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			matchingDeclaration.JE_HouseBill = "20257654321";
			var matchingCusContainer = matchingDeclaration.CusContainers.AddNew();
			matchingCusContainer.CO_ContainerNumber = "10137654321";
			var matchingContainer = matchingCusContainer.JobContainer;
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals("Should get best matching Container.", GetHumanReadableID(matchingContainer), GetHumanReadableID(logParents[0]));

			matchingDeclaration.CusContainers.RemoveAndDeleteAll();
			xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals("Should get best matching Declaration.", GetHumanReadableID(matchingDeclaration), GetHumanReadableID(logParents[0]));
		}

		public void TestIncomingEventLinksToLegWhenUsingHBOLAndVoyageNumberAndFallsBackToDeclarationIfCannotMatchLeg()
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
</UniversalEvent>
";

			var matchingDeclaration = Factory.New<BaseJobDeclaration>();
			matchingDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			matchingDeclaration.JE_HouseBill = "20257654321";

			var matchingTransport = matchingDeclaration.Transports.AddNew();
			matchingTransport.JW_Vessel = "MAGIC SCHOOL BUS";
			matchingTransport.JW_VoyageFlight = "12234";
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(transportLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals("Should get best matching Transport.", GetHumanReadableID(matchingTransport), GetHumanReadableID(logParents[0]));

			matchingDeclaration.Transports.RemoveAndDeleteAll();
			xmlEvent = eventDeserializer.Parse(transportLevelEventXmlText);
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals("Should get best matching Declaration.", GetHumanReadableID(matchingDeclaration), GetHumanReadableID(logParents[0]));
		}

		public void TestIncomingEventLinksToContainerWhenUsingHBOLVesselDetailsAndContainerNumber_FallsBackToLegWithNoMatchingContainer_FallsBackToDeclarationWithNoMatchingLeg()
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
				<Type>ContainerNumber</Type>
				<Value>10137654322</Value>
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
</UniversalEvent>
";

			var matchingDeclaration = Factory.New<BaseJobDeclaration>();
			matchingDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			matchingDeclaration.JE_HouseBill = "20257654321";

			var matchingCusContainer = matchingDeclaration.CusContainers.AddNew();
			matchingCusContainer.CO_ContainerNumber = "10137654321";
			var matchingContainer = matchingCusContainer.JobContainer;

			var matchingTransport = matchingDeclaration.Transports.AddNew();
			matchingTransport.JW_Vessel = "MAGIC SCHOOL BUS";
			matchingTransport.JW_VoyageFlight = "12234";
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals("Should get best matching Container.", GetHumanReadableID(matchingContainer), GetHumanReadableID(logParents[0]));

			matchingDeclaration.CusContainers.RemoveAndDeleteAll();
			xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals("Should get best matching Transport.", GetHumanReadableID(matchingTransport), GetHumanReadableID(logParents[0]));

			matchingDeclaration.Transports.RemoveAndDeleteAll();
			xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText);
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals("Should get best matching Declaration.", GetHumanReadableID(matchingDeclaration), GetHumanReadableID(logParents[0]));
		}

		public void TestIncomingEventLinksToTheLatestDeclarationHavingMultipleBills()
		{
			const string shipmentLevelEventXmlText1 = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>081-203212</Value>
			</Context>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>20257654321</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var matchingDeclaration = Factory.New<BaseJobDeclaration>();
			matchingDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			matchingDeclaration.JE_HouseBill = "20257654321";
			matchingDeclaration.JE_MasterBill = "081-203212";
			var masterBill2 = matchingDeclaration.Bills.AddNew();
			masterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "072343343";
			var houseBill2 = masterBill2.ChildBills.AddNew();
			houseBill2.CU_BillNum = "4562343232";

			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText1);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals("Should get best matching Declaration.", GetHumanReadableID(matchingDeclaration), GetHumanReadableID(logParents[0]));

			const string shipmentLevelEventXmlText2 = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>081-203212</Value>
			</Context>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>4562343232</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText2);

			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals("Should get best matching Declaration.", GetHumanReadableID(matchingDeclaration), GetHumanReadableID(logParents[0]));

			const string shipmentLevelEventXmlText3 = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>072343343</Value>
			</Context>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>20257654321</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText3);

			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals("Should get best matching Declaration.", GetHumanReadableID(matchingDeclaration), GetHumanReadableID(logParents[0]));

			const string shipmentLevelEventXmlText4 = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>072343343</Value>
			</Context>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>695578554</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText4);

			AssertNull("Should not match", subscriber.GetLogParentsForEvent(xmlEvent));
		}

		public void TestUpdateDeclarationEntryStatusForIntegratedCountry()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

			CombineAssertions("ACK-IMP-PR-NotUpdate-NonIntegratedCountry", () =>
			{
				using (currentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
				{
					string jobID = "B00002005";

					string testMessage = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>" + jobID + @"</Key>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>" + GlbCompany.CurrentCompany.GC_Code + @"</Code>
			</Company>
		</DataContext>
		<EventTime>2015-08-05T00:00:00</EventTime>
		<EventType>CES</EventType>
		<EventReference>ACK</EventReference>
	</Event>
</UniversalEvent>
";

					NewFactory();
					var matchingDeclaration = Factory.New<BaseJobDeclaration>();
					matchingDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					matchingDeclaration.JE_DeclarationReference = jobID;
					matchingDeclaration.JE_GB = GlbBranch.CurrentBranch.PK;
					Factory.SaveForTesting();

					StmALog ecmEventLog = matchingDeclaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCommenced);
					AssertEquals("PRE: Declaration CountryCode", "PR", matchingDeclaration.Country.Code);
					AssertEquals("PRE: Declaration Entry Status", string.Empty, matchingDeclaration.JE_EntryStatus);
					AssertNull("PRE: CustomsCommenced Event", ecmEventLog);

					var subscriber = GetNewEventParentFinder();
					var eventDeserializer = new XmlEventDeserializer();
					var xmlEvent = eventDeserializer.Parse(testMessage);

					var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
					AssertEquals("Should match one LogParent", 1, logParents.Length);
					AssertEquals("Should get best matching Declaration.", GetHumanReadableID(matchingDeclaration),
						GetHumanReadableID(logParents[0]));
					AssertEquals("Should NOT update the entryType", string.Empty, matchingDeclaration.JE_EntryStatus);
					ecmEventLog = matchingDeclaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCommenced);
					AssertNull("Should not Add CustomsCommenced Event", ecmEventLog);
				}
			});

			CombineAssertions("ACK-IMP-ZA-Update", () =>
			{
				string jobID = "B00002001";
				string testMessage = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>" + jobID + @"</Key>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>" + GlbCompany.CurrentCompany.GC_Code + @"</Code>
			</Company>
		</DataContext>
		<EventTime>2015-08-05T00:00:00</EventTime>
		<EventType>CES</EventType>
		<EventReference>ACK</EventReference>
	</Event>
</UniversalEvent>
";

				using (currentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
				{
					var matchingDeclaration = Factory.New<BaseJobDeclaration>();
					matchingDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					matchingDeclaration.JE_DeclarationReference = jobID;
					Factory.SaveForTesting();

					StmALog cccEventLog = matchingDeclaration.Logs.MostRecentLogByEventTime(Events.CustomsCommenced);
					AssertEquals("PRE: Declaration CountryCode", "ZA", matchingDeclaration.Country.Code);
					AssertEquals("PRE: Declaration Entry Status", string.Empty, matchingDeclaration.JE_EntryStatus);
					AssertNull("PRE: CustomsCommenced Event", cccEventLog);

					var subscriber = GetNewEventParentFinder();
					var eventDeserializer = new XmlEventDeserializer();
					var xmlEvent = eventDeserializer.Parse(testMessage);

					var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
					AssertEquals("Should match one LogParent", 1, logParents.Length);
					AssertEquals("Should get best matching Declaration.", GetHumanReadableID(matchingDeclaration), GetHumanReadableID(logParents[0]));
					AssertEquals("Should update the entryType", "ACK", matchingDeclaration.JE_EntryStatus);
					cccEventLog = matchingDeclaration.Logs.MostRecentLogByEventTime(Events.CustomsCommenced);
					AssertNotNull("Should Add CustomsCommenced Event", cccEventLog);
				}
			});

			CombineAssertions("ACK-EXP-ZA-Update", () =>
			{
				string jobID = "B00002002";
				string testMessage = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>" + jobID + @"</Key>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>" + GlbCompany.CurrentCompany.GC_Code + @"</Code>
			</Company>
		</DataContext>
		<EventTime>2015-08-05T00:00:00</EventTime>
		<EventType>CES</EventType>
		<EventReference>ACK</EventReference>
	</Event>
</UniversalEvent>
";

				using (currentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
				{
					var matchingDeclaration = Factory.New<BaseJobDeclaration>();
					matchingDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					matchingDeclaration.JE_DeclarationReference = jobID;
					Factory.SaveForTesting();

					StmALog ecmEventLog = matchingDeclaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCommenced);
					AssertEquals("PRE: Declaration Entry Status", string.Empty, matchingDeclaration.JE_EntryStatus);
					AssertNull("PRE: CustomsCommenced Event", ecmEventLog);

					var subscriber = GetNewEventParentFinder();
					var eventDeserializer = new XmlEventDeserializer();
					var xmlEvent = eventDeserializer.Parse(testMessage);

					var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
					AssertEquals("Should match one LogParent", 1, logParents.Length);
					AssertEquals("Should get best matching Declaration.", GetHumanReadableID(matchingDeclaration), GetHumanReadableID(logParents[0]));
					AssertEquals("Should update the entryType", "ACK", matchingDeclaration.JE_EntryStatus);
					ecmEventLog = matchingDeclaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCommenced);
					AssertNotNull("Should Add CustomsCommenced Event", ecmEventLog);
				}
			});

			CombineAssertions("6-EXP-ZA-Update", () =>
			{
				string jobID = "B00002003";
				string testMessage = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>" + jobID + @"</Key>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>" + GlbCompany.CurrentCompany.GC_Code + @"</Code>
			</Company>
		</DataContext>
		<EventTime>2015-08-05T00:00:00</EventTime>
		<EventType>CES</EventType>
		<EventReference>6</EventReference>
	</Event>
</UniversalEvent>
";

				using (currentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
				{
					var universalReferenceTestHelper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
					universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CustomsStatus");
					var za6 = universalReferenceTestHelper.CreateCusCodeList("ZA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "6", "Reject To Clearer", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
					universalReferenceTestHelper.CreateCusCodeListAttribute(za6.PK, "CustomsRejected", "true");
					Factory.SaveForTesting();

					var matchingDeclaration = Factory.New<BaseJobDeclaration>();
					matchingDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					matchingDeclaration.JE_DeclarationReference = jobID;
					Factory.SaveForTesting();

					StmALog ecmEventLog = matchingDeclaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCommenced);
					AssertEquals("PRE: Declaration Entry Status", string.Empty, matchingDeclaration.JE_EntryStatus);
					AssertNull("PRE: CustomsCommenced Event", ecmEventLog);

					var subscriber = GetNewEventParentFinder();
					var eventDeserializer = new XmlEventDeserializer();
					var xmlEvent = eventDeserializer.Parse(testMessage);

					var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
					AssertEquals("Should match one LogParent", 1, logParents.Length);
					AssertEquals("Should get best matching Declaration.", GetHumanReadableID(matchingDeclaration), GetHumanReadableID(logParents[0]));
					AssertEquals("Should update the entryType", "6", matchingDeclaration.JE_EntryStatus);
					ecmEventLog = matchingDeclaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCommenced);
					AssertNull("Should not Add CustomsCommenced Event", ecmEventLog);
				}
			});

			CombineAssertions("XXX-EXP-ZA-NotUpdate-InvalidCode", () =>
			{
				string jobID = "B00002004";
				string testMessage = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>" + jobID + @"</Key>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>" + GlbCompany.CurrentCompany.GC_Code + @"</Code>
			</Company>
		</DataContext>
		<EventTime>2015-08-05T00:00:00</EventTime>
		<EventType>CES</EventType>
		<EventReference>XXX</EventReference>
	</Event>
</UniversalEvent>
";

				using (currentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
				{
					var matchingDeclaration = Factory.New<BaseJobDeclaration>();
					matchingDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					matchingDeclaration.JE_DeclarationReference = jobID;
					Factory.SaveForTesting();

					StmALog ecmEventLog = matchingDeclaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCommenced);
					AssertEquals("PRE: Declaration Entry Status", string.Empty, matchingDeclaration.JE_EntryStatus);
					AssertNull("PRE: CustomsCommenced Event", ecmEventLog);

					var subscriber = GetNewEventParentFinder();
					var eventDeserializer = new XmlEventDeserializer();
					var xmlEvent = eventDeserializer.Parse(testMessage);

					var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
					AssertEquals("Should match one LogParent", 1, logParents.Length);
					AssertEquals("Should get best matching Declaration.", GetHumanReadableID(matchingDeclaration), GetHumanReadableID(logParents[0]));
					AssertEquals("Should NOT update the entryType", string.Empty, matchingDeclaration.JE_EntryStatus);
					ecmEventLog = matchingDeclaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCommenced);
					AssertNull("Should not Add CustomsCommenced Event", ecmEventLog);
				}
			});
		}

		public void TestUpdateDeclarationEntryStatusForIntegratedCountry_NZ()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

			CombineAssertions("ACK-IMP-NZ-NotUpdate-WasNonIntegratedCountry", () =>
			{
				using (currentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
				{
					var jobID = "B00002005";

					var testMessage = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>" + jobID + @"</Key>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>" + GlbCompany.CurrentCompany.GC_Code + @"</Code>
			</Company>
		</DataContext>
		<EventTime>2015-08-05T00:00:00</EventTime>
		<EventType>CES</EventType>
		<EventReference>ACK</EventReference>
	</Event>
</UniversalEvent>
";

					NewFactory();
					var matchingDeclaration = Factory.New<BaseJobDeclaration>();
					matchingDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					matchingDeclaration.JE_DeclarationReference = jobID;
					matchingDeclaration.JE_GB = GlbBranch.CurrentBranch.PK;
					Factory.SaveForTesting();

					AssertEquals("PRE: Declaration CountryCode", "NZ", matchingDeclaration.Country.Code);
					AssertEquals("PRE: Declaration Entry Status", "NSC", matchingDeclaration.JE_EntryStatus);
					StmALog ecmEventLog = matchingDeclaration.Logs.MostRecentLogByEventTime(Events.CustomsCommenced);
					AssertNull("PRE: CustomsCommenced Event", ecmEventLog);

					var subscriber = GetNewEventParentFinder();
					var eventDeserializer = new XmlEventDeserializer();
					var xmlEvent = eventDeserializer.Parse(testMessage);

					var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
					AssertEquals("Should match one LogParent", 1, logParents.Length);
					AssertEquals("Should get best matching Declaration.", GetHumanReadableID(matchingDeclaration), GetHumanReadableID(logParents[0]));
					AssertEquals("Should NOT update the entryType", "NSC", matchingDeclaration.JE_EntryStatus);
					ecmEventLog = matchingDeclaration.Logs.MostRecentLogByEventTime(Events.CustomsCommenced);
					AssertNull("Should not Add CustomsCommenced Event", ecmEventLog);
				}
			});
		}

		public void TestIncomingEventLinksToContainerWhenUsingVoyageAndVesselNameAndContainerNumber()
		{
			string eventXML = @"
<UniversalEvent>
	<Event>
		<EventTime>2015-08-05T00:00:00</EventTime>
		<EventType>GOU</EventType>
		<ContextCollection>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>CONT1234565</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>CONT1234567</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>222</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>TEST VESSEL</Value>
			</Context>
			<Context>
				<Type>IsEmptyContainer</Type>
				<Value>false</Value>
			</Context>
		</ContextCollection>
		<AdditionalFieldsToUpdateCollection>
			<AdditionalFieldsToUpdate>
				<Type>JobContainer.JC_FCLWharfGateOut</Type>
				<Value>2015-09-05T00:00:00</Value>
			</AdditionalFieldsToUpdate>
			<AdditionalFieldsToUpdate>
				<Type>JobContainer.JC_ArrivalSlotReference</Type>
				<Value>SLOTREFERENCE</Value>
			</AdditionalFieldsToUpdate>
		</AdditionalFieldsToUpdateCollection>
	</Event>
</UniversalEvent>
";
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "TEST VESSEL";

			var matchingDeclaration = Factory.New<BaseJobDeclaration>();
			matchingDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			matchingDeclaration.JE_VesselName = "TEST VESSEL";
			matchingDeclaration.JE_VoyageFlightNo = "222";

			var cusContainerOne = matchingDeclaration.CusContainers.AddNew();
			cusContainerOne.CO_ContainerNumber = "CONT1234565";
			var cusContainerTwo = matchingDeclaration.CusContainers.AddNew();
			cusContainerTwo.CO_ContainerNumber = "CONT1234565";
			var cusContainerThree = matchingDeclaration.CusContainers.AddNew();
			cusContainerThree.CO_ContainerNumber = "CONT1234566";
			var cusContainerFour = matchingDeclaration.CusContainers.AddNew();
			cusContainerFour.CO_ContainerNumber = "CONT1234567";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(eventXML);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Container 'CONT1234565'.
Linked Event to Container 'CONT1234565'.
Linked Event to Container 'CONT1234567'.
".Trim(), serviceTaskLog.ToString());

				AssertEquals(new ZDateTime(2015, 09, 05), cusContainerOne.JobContainer.JC_FCLWharfGateOut);
				AssertEquals("SLOTREFERENCE", cusContainerOne.JobContainer.JC_ArrivalSlotReference);
				AssertEquals(new ZDateTime(2015, 09, 05), cusContainerTwo.JobContainer.JC_FCLWharfGateOut);
				AssertEquals("SLOTREFERENCE", cusContainerTwo.JobContainer.JC_ArrivalSlotReference);
				AssertEquals(ZDateTime.Empty, cusContainerThree.JobContainer.JC_FCLWharfGateOut);
				AssertEquals(ZString.Empty, cusContainerThree.JobContainer.JC_ArrivalSlotReference);
			});

			matchingDeclaration.JE_VesselName = "TEST VESSEL1";
			cusContainerOne.JobContainer.JC_FCLWharfGateOut = ZDateTime.Empty;
			cusContainerOne.JobContainer.JC_ArrivalSlotReference = "";
			cusContainerTwo.JobContainer.JC_FCLWharfGateOut = ZDateTime.Empty;
			cusContainerTwo.JobContainer.JC_ArrivalSlotReference = "";

			Factory.SaveForTesting();

			message = GetQueuedUniversalEventMessage(eventXML);
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Warning - No Module found a Business Entity to link this Universal Event to.
".Trim(), serviceTaskLog.ToString());

				AssertEquals(ZDateTime.Empty, cusContainerOne.JobContainer.JC_FCLWharfGateOut);
				AssertEquals(ZString.Empty, cusContainerOne.JobContainer.JC_ArrivalSlotReference);
				AssertEquals(ZDateTime.Empty, cusContainerTwo.JobContainer.JC_FCLWharfGateOut);
				AssertEquals(ZString.Empty, cusContainerTwo.JobContainer.JC_ArrivalSlotReference);
			});

			eventXML = @"
<UniversalEvent>
	<Event>
		<EventTime>2015-08-05T00:00:00</EventTime>
		<EventType>GOU</EventType>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>081-203212</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>CONT1234565</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>CONT1234566</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>222</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>TEST VESSEL</Value>
			</Context>
			<Context>
				<Type>IsEmptyContainer</Type>
				<Value>false</Value>
			</Context>
		</ContextCollection>
		<AdditionalFieldsToUpdateCollection>
			<AdditionalFieldsToUpdate>
				<Type>JobContainer.JC_FCLWharfGateOut</Type>
				<Value>2015-09-05T00:00:00</Value>
			</AdditionalFieldsToUpdate>
			<AdditionalFieldsToUpdate>
				<Type>JobContainer.JC_ArrivalSlotReference</Type>
				<Value>SLOTREFERENCE</Value>
			</AdditionalFieldsToUpdate>
		</AdditionalFieldsToUpdateCollection>
	</Event>
</UniversalEvent>
";
			matchingDeclaration.JE_JS = ZGuid.Empty;
			Factory.SaveForTesting();

			message = GetQueuedUniversalEventMessage(eventXML);
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Warning - No Module found a Business Entity to link this Universal Event to.
".Trim(), serviceTaskLog.ToString());
			});
		}

		public void TestIncomingEventLinksToContainerWhenUsingVoyageAndLloydsAndContainerNumber()
		{
			string eventXML = @"
<UniversalEvent>
	<Event>
		<EventTime>2015-08-05T00:00:00</EventTime>
		<EventType>GOU</EventType>
		<ContextCollection>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>CONT1234565</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>CONT1234567</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>222</Value>
			</Context>
			<Context>
				<Type>LloydsNumber</Type>
				<Value>8123432</Value>
			</Context>
			<Context>
				<Type>IsEmptyContainer</Type>
				<Value>false</Value>
			</Context>
		</ContextCollection>
		<AdditionalFieldsToUpdateCollection>
			<AdditionalFieldsToUpdate>
				<Type>JobContainer.JC_FCLWharfGateOut</Type>
				<Value>2015-09-05T00:00:00</Value>
			</AdditionalFieldsToUpdate>
			<AdditionalFieldsToUpdate>
				<Type>JobContainer.JC_ArrivalSlotReference</Type>
				<Value>SLOTREFERENCE</Value>
			</AdditionalFieldsToUpdate>
		</AdditionalFieldsToUpdateCollection>
	</Event>
</UniversalEvent>
";
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "TEST VESSEL";
			vessel.RV_LloydsNumber = "8123432";

			var matchingDeclaration = Factory.New<BaseJobDeclaration>();
			matchingDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			matchingDeclaration.JE_VesselName = "TEST VESSEL";
			matchingDeclaration.JE_VoyageFlightNo = "222";

			var cusContainerOne = matchingDeclaration.CusContainers.AddNew();
			cusContainerOne.CO_ContainerNumber = "CONT1234565";
			var cusContainerTwo = matchingDeclaration.CusContainers.AddNew();
			cusContainerTwo.CO_ContainerNumber = "CONT1234565";
			var cusContainerThree = matchingDeclaration.CusContainers.AddNew();
			cusContainerThree.CO_ContainerNumber = "CONT1234566";
			var cusContainerFour = matchingDeclaration.CusContainers.AddNew();
			cusContainerFour.CO_ContainerNumber = "CONT1234567";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(eventXML);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Container 'CONT1234565'.
Linked Event to Container 'CONT1234565'.
Linked Event to Container 'CONT1234567'.
".Trim(), serviceTaskLog.ToString());

				AssertEquals(new ZDateTime(2015, 09, 05), cusContainerOne.JobContainer.JC_FCLWharfGateOut);
				AssertEquals("SLOTREFERENCE", cusContainerOne.JobContainer.JC_ArrivalSlotReference);
				AssertEquals(new ZDateTime(2015, 09, 05), cusContainerTwo.JobContainer.JC_FCLWharfGateOut);
				AssertEquals("SLOTREFERENCE", cusContainerTwo.JobContainer.JC_ArrivalSlotReference);
				AssertEquals(ZDateTime.Empty, cusContainerThree.JobContainer.JC_FCLWharfGateOut);
				AssertEquals(ZString.Empty, cusContainerThree.JobContainer.JC_ArrivalSlotReference);
			});

			vessel.RV_LloydsNumber = "8123433";
			cusContainerOne.JobContainer.JC_FCLWharfGateOut = ZDateTime.Empty;
			cusContainerOne.JobContainer.JC_ArrivalSlotReference = "";
			cusContainerTwo.JobContainer.JC_FCLWharfGateOut = ZDateTime.Empty;
			cusContainerTwo.JobContainer.JC_ArrivalSlotReference = "";

			Factory.SaveForTesting();

			message = GetQueuedUniversalEventMessage(eventXML);
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Warning - No Module found a Business Entity to link this Universal Event to.
".Trim(), serviceTaskLog.ToString());

				AssertEquals(ZDateTime.Empty, cusContainerOne.JobContainer.JC_FCLWharfGateOut);
				AssertEquals(ZString.Empty, cusContainerOne.JobContainer.JC_ArrivalSlotReference);
				AssertEquals(ZDateTime.Empty, cusContainerTwo.JobContainer.JC_FCLWharfGateOut);
				AssertEquals(ZString.Empty, cusContainerTwo.JobContainer.JC_ArrivalSlotReference);
			});

			eventXML = @"
<UniversalEvent>
	<Event>
		<EventTime>2015-08-05T00:00:00</EventTime>
		<EventType>GOU</EventType>
		<ContextCollection>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>4562343232</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>CONT1234565</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>CONT1234567</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>222</Value>
			</Context>
			<Context>
				<Type>LloydsNumber</Type>
				<Value>8123432</Value>
			</Context>
			<Context>
				<Type>IsEmptyContainer</Type>
				<Value>false</Value>
			</Context>
		</ContextCollection>
		<AdditionalFieldsToUpdateCollection>
			<AdditionalFieldsToUpdate>
				<Type>JobContainer.JC_FCLWharfGateOut</Type>
				<Value>2015-09-05T00:00:00</Value>
			</AdditionalFieldsToUpdate>
			<AdditionalFieldsToUpdate>
				<Type>JobContainer.JC_ArrivalSlotReference</Type>
				<Value>SLOTREFERENCE</Value>
			</AdditionalFieldsToUpdate>
		</AdditionalFieldsToUpdateCollection>
	</Event>
</UniversalEvent>
";
			matchingDeclaration.JE_JS = ZGuid.Empty;
			Factory.SaveForTesting();

			message = GetQueuedUniversalEventMessage(eventXML);
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Warning - No Module found a Business Entity to link this Universal Event to.
".Trim(), serviceTaskLog.ToString());
			});
		}

		public void TestIncomingEventLinksToContainerWhenJobIsNotStandAlone()
		{
			string eventXML = @"
<UniversalEvent>
	<Event>
		<EventTime>2015-08-05T00:00:00</EventTime>
		<EventType>GOU</EventType>
		<ContextCollection>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>CONT1234565</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>CONT1234566</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>222</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>TEST VESSEL</Value>
			</Context>
			<Context>
				<Type>IsEmptyContainer</Type>
				<Value>false</Value>
			</Context>
		</ContextCollection>
		<AdditionalFieldsToUpdateCollection>
			<AdditionalFieldsToUpdate>
				<Type>JobContainer.JC_FCLWharfGateOut</Type>
				<Value>2015-09-05T00:00:00</Value>
			</AdditionalFieldsToUpdate>
			<AdditionalFieldsToUpdate>
				<Type>JobContainer.JC_ArrivalSlotReference</Type>
				<Value>SLOTREFERENCE</Value>
			</AdditionalFieldsToUpdate>
		</AdditionalFieldsToUpdateCollection>
	</Event>
</UniversalEvent>
";
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "TEST VESSEL";

			var declaration = Factory.New<BaseJobDeclaration>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = declaration.CountryCode + "ABC";

			var transport = consol.Transports[0];
			transport.JW_VoyageFlight = "222";
			transport.JW_Vessel = "TEST VESSEL";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1234565";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1234566";

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);

			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.SetEnabled(true, false);
			declaration.ShipmentSynchroniser.Synchronise(true);

			Factory.SaveForTesting();

			AssertEquals(1, declaration.CusContainers.Count);
			var cusContainer = declaration.CusContainers[0];

			var message = GetQueuedUniversalEventMessage(eventXML);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Container 'CONT1234565'.
Linked Event to Container 'CONT1234566'.
".Trim(), serviceTaskLog.ToString());

				AssertEquals(new ZDateTime(2015, 09, 05), cusContainer.JobContainer.JC_FCLWharfGateOut);
				AssertEquals("SLOTREFERENCE", cusContainer.JobContainer.JC_ArrivalSlotReference);
			});
		}

		public void TestMatchOnBGMReferenceOrCreate()
		{
			var decRef = "S800062175";
			var entryNo = "5-18-1645";
			var countryCode = Core.Constants.CountryCodes.Italy;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var dec = Factory.New<BaseJobDeclaration>();
				dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				AssertEquals(countryCode, dec.CountryCode);

				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.JE_DeclarationReference = decRef;

				AssertEquals("Pre-Req: Italy is integrated", true, dec.IsDeclarationIntegrated);

				var eventObj = new UniversalEvent();
				var eventValueObject = (IXmlEventValueObject)eventObj;

				List<Context> ctxCol = new List<Context>();

				Context ctx = new Context();
				ctx.Type = "DeclarationReference";
				ctx.Value = decRef;
				ctxCol.Add(ctx);

				ctx = new Context();
				ctx.Type = "EntryNumber";
				ctx.Value = entryNo;
				ctxCol.Add(ctx);

				ctx = new Context();
				ctx.Type = "EntryNumberType";
				ctx.Value = "IMP";
				ctxCol.Add(ctx);

				ctx = new Context();
				ctx.Type = "EntryNumberCountryOfIssue";
				ctx.Value = countryCode;
				ctxCol.Add(ctx);

				eventObj.ContextCollection = ctxCol;

				var manager = new JobDeclarationDataContextManager();
				var logger = new TestErrorLogger();
				var finder = new JobDeclarationEventParentFinder(Factory.BOFactory, manager, logger);
				var infoMsg1 = $"Existing entries collection of {dec.ActiveEntryHeaders.Count}, with keys [{dec.ActiveEntryHeaders.BGMReferencesAsCommaDelimitedString}], did not have a matching record.";
				var arrBO = finder.GenerateLogParentsForEventUsingContext(eventObj);
				var lastEntryHeaderAdded = dec.CustomsEntryHeaders[dec.CustomsEntryHeaders.Count - 1];
				var infoMsg2 = $"Created new customs entry header with BGM reference = {lastEntryHeaderAdded.CH_BGMReference}, and message type = {lastEntryHeaderAdded.CH_MessageType}.";

				Assert(ContainsLogEntry(logger.Logs, LogType.Information, infoMsg1));
				Assert(ContainsLogEntry(logger.Logs, LogType.Information, infoMsg2));

				AssertEquals(1, dec.CustomsEntryHeaders.Count);

				dec.CustomsEntryHeaders.RemoveAll();

				var entry = dec.CustomsEntryHeaders.AddNew();

				entry.CH_BGMReference = "ABC";
				entry.CH_MessageType = eventValueObject.Context.EntryNumberType;

				logger.ClearLogs();

				infoMsg1 = $"Existing entries collection of {dec.ActiveEntryHeaders.Count}, with keys [{dec.ActiveEntryHeaders.BGMReferencesAsCommaDelimitedString}], did not have a matching record.";
				arrBO = finder.GenerateLogParentsForEventUsingContext(eventObj);
				lastEntryHeaderAdded = dec.CustomsEntryHeaders[dec.CustomsEntryHeaders.Count - 1];
				infoMsg2 = $"Created new customs entry header with BGM reference = {lastEntryHeaderAdded.CH_BGMReference}, and message type = {lastEntryHeaderAdded.CH_MessageType}.";

				Assert(ContainsLogEntry(logger.Logs, LogType.Information, infoMsg1));
				Assert(ContainsLogEntry(logger.Logs, LogType.Information, infoMsg2));

				AssertEquals(2, dec.CustomsEntryHeaders.Count);

				dec.CustomsEntryHeaders.RemoveAll();

				entry = dec.CustomsEntryHeaders.AddNew();

				entry.CH_BGMReference = decRef + "/" + entryNo;
				entry.CH_MessageType = eventValueObject.Context.EntryNumberType;

				logger.ClearLogs();

				infoMsg1 = $"Existing entries collection of {dec.ActiveEntryHeaders.Count}, with keys [{dec.ActiveEntryHeaders.BGMReferencesAsCommaDelimitedString}], did not have a matching record.";
				arrBO = finder.GenerateLogParentsForEventUsingContext(eventObj);
				lastEntryHeaderAdded = dec.CustomsEntryHeaders[dec.CustomsEntryHeaders.Count - 1];
				infoMsg2 = $"Created new customs entry header with BGM reference = {lastEntryHeaderAdded.CH_BGMReference}, and message type = {lastEntryHeaderAdded.CH_MessageType}.";

				Assert(!ContainsLogEntry(logger.Logs, LogType.Information, infoMsg1));
				Assert(!ContainsLogEntry(logger.Logs, LogType.Information, infoMsg2));

				AssertEquals(1, dec.CustomsEntryHeaders.Count);
			}
		}

		public void TestIncomingEventLinksToVisualizerDocumentData()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_DeclarationReference = "B00001119";

				var menu = Factory.New<StmMenuItem>();
				menu.SU_BusinessContext = "Customs";
				menu.SU_MenuType = "FRM";
				menu.SU_MenuName = "test";

				var template = Factory.New<StmTemplate>();
				template.SO_TemplateType = "FRM";

				var documentPivot = Factory.New<StmMenuTemplatePivot>();
				documentPivot.SI_DocumentTitle = "Cargo Dues - Export";
				documentPivot.SI_DataStoreName = "Cargo Dues Brokerage - Export";
				documentPivot.SI_SU = menu.PK;
				documentPivot.SI_SO = template.PK;

				var documentData = (BusinessObject)Factory.BOFactory.New<IVisualizerDocumentData>();
				documentData[JobDocumentDataSchema.Constants.JDD_ParentID] = declaration.PK;
				documentData[JobDocumentDataSchema.Constants.JDD_ParentTableCode] = "JE";
				documentData[JobDocumentDataSchema.Constants.JDD_Name] = "Cargo Dues Brokerage - Export";

				Factory.SaveForTesting();

				var logger = new TestErrorLogger();
				const string incomingEvent = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DocumentaryOverride>
				<DocumentName>Cargo Dues - Export</DocumentName>
			</DocumentaryOverride>
			<DataTargetCollection>
				<DataTarget>
					<Key>B00001119</Key>
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2021-01-15T14:42:38</EventTime>
		<EventType>ISN</EventType>
		<EventParameters>
			<Department>WiseTechGlobal</Department>
			<MessageType>Cargo Dues - Export</MessageType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>MessageReference</Type>
				<Value>BCPTDZASYDAUSYDHYEDCPTZACPT00002600</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

				var subscriber = GetNewEventParentFinderWithLogger(logger);
				var eventDeserializer = new XmlEventDeserializer();
				var xmlEvent = eventDeserializer.Parse(incomingEvent);
				var eventDataObject = xmlEvent as UniversalEvent;
				var logParents = subscriber.GetLogParentsForEvent(eventDataObject);

				AssertNotNull(logParents);

				AssertContainsExactElementsInAnyOrder("should match declaration and document data", new[] { declaration, documentData }, logParents);
			}
		}

		public void TestIncomingEventLinksToAirImportDeclarationWhenUsingMAWBAndDepartmentIsCustoms()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MasterBill = "020-22143144";
				declaration.JE_HouseBill = "20257654321";
				declaration.JE_LandedPieces = 100;
				declaration.JE_DeclarationReference = "B00001000";
				declaration.JE_GB = GlbBranch.CurrentBranch.PK;
				Factory.SaveForTesting();

				string eventXML = @"
<UniversalEvent>
	<Event>
		<DataContext>
		 <DataTargetCollection>
			<DataTarget>
			   <Type>ForwardingConsol</Type>
			</DataTarget>
			<DataTarget>
			   <Type>CustomsDeclaration</Type>
			</DataTarget>
		 </DataTargetCollection>
	  </DataContext>
	  <EventTime>2020-06-04T10:23:00</EventTime>
	  <EventType>RCV</EventType>
	  <EventParameters>
		 <Facility>CTO</Facility>
		 <Location>FRA</Location>
		 <VoyageFlightNumber>LH7359S</VoyageFlightNumber>
		 <FlightDate>2020-06-04</FlightDate>
		 <Quantity>2</Quantity>
		 <Department>Customs</Department>
	  </EventParameters>
	  <ContextCollection>
		 <Context>
			<Type>MAWBNumber</Type>
			<Value>020-22143144</Value>
		 </Context>
	  </ContextCollection>
	</Event>
</UniversalEvent>
";
				AssertIncomingEventLinksToAirImportDeclarationWhenUsingMAWBAndDepartmentIsCustoms(eventXML, declaration, @"False|Warning - No Module found a Business Entity to link this Universal Event to.|NULL", 100, false);

				eventXML = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingConsol</Type>
				</DataTarget>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2020-06-04T10:23:00</EventTime>
		<EventType>RCV</EventType>
		<EventParameters>
			<Facility>CTO</Facility>
			<Location>FRA</Location>
			<VoyageFlightNumber>LH7359S</VoyageFlightNumber>
			<FlightDate>2020-06-04</FlightDate>
			<Quantity>2</Quantity>
			<Department>Customs</Department>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>020-22143144</Value>
			</Context>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>20257654321</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
				AssertIncomingEventLinksToAirImportDeclarationWhenUsingMAWBAndDepartmentIsCustoms(eventXML, declaration, @"True|Linked Event to Declaration B00001000.|CustomsDeclaration-B00001000", 2, false);

				declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MasterBill = "020-22143144";
				declaration.JE_HouseBill = "20257654321";
				declaration.JE_LandedPieces = 110;
				declaration.JE_DeclarationReference = "B00001001";
				declaration.JE_GB = GlbBranch.CurrentBranch.PK;
				Factory.SaveForTesting();

				AssertIncomingEventLinksToAirImportDeclarationWhenUsingMAWBAndDepartmentIsCustoms(eventXML, declaration, @"False|Warning - No Module found a Business Entity to link this Universal Event to.|NULL", 110, true);
			}
		}

		public void TestContainerAutomationEventsMatchingDeclarationsAreExcluded_2011xmlns()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MasterBill = "081-203212";
			declaration.JE_HouseBill = "20257654321";

			Factory.SaveForTesting();

			string GetEventXML(string dataProvider) => $@"
<UniversalEvent xmlns = ""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2019 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>081-203212</Value>
			</Context>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>20257654321</Value>
			</Context>
		</ContextCollection>
		<DataContext>
			<DataProvider>{dataProvider}</DataProvider>
		</DataContext>
	</Event>
</UniversalEvent>
";

			var eventDeserializer = new XmlEventDeserializer();
			var finder = new JobDeclarationEventParentFinder(Factory.BOFactory, new JobDeclarationDataContextManager(), new DummyLogger());

			var eventDataObject = eventDeserializer.Parse(GetEventXML("WTG Tracking & Automation"));
			var parents = finder.GetLogParentsForEvent(eventDataObject);
			AssertEquals("XML has Container Automation data context and matching standalone declaration, so finder should match parents.", 1, parents.Length);
			AssertEquals(parents[0].PK, declaration.PK);

			declaration.JE_JS = shipment.PK;
			parents = finder.GetLogParentsForEvent(eventDataObject);
			AssertEquals("XML has Container Automation data context, but matching declaration is linked, so finder should not match parents.", 0, parents.Length);

			eventDataObject = eventDeserializer.Parse(GetEventXML("A Very Random Company"));
			parents = finder.GetLogParentsForEvent(eventDataObject);
			AssertEquals("XML has matching linked declaration, but non-Container Automation data context, so finder should match parents.", 1, parents.Length);
			AssertEquals(parents[0].PK, declaration.PK);
		}

		public void TestContainerAutomationEventsMatchingDeclarationsAreExcluded_2012xmlns()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MasterBill = "081-203212";
			declaration.JE_HouseBill = "20257654321";

			Factory.SaveForTesting();

			string GetEventXML(string dataProvider) => $@"
<UniversalEvent xmlns = ""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2019 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>081-203212</Value>
			</Context>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>20257654321</Value>
			</Context>
		</ContextCollection>
		<DataContext>
			<DataSource>
				<DataProvider>{dataProvider}</DataProvider>
			</DataSource>
		</DataContext>
	</Event>
</UniversalEvent>
";

			var eventDeserializer = new XmlEventDeserializer();
			var finder = new JobDeclarationEventParentFinder(Factory.BOFactory, new JobDeclarationDataContextManager(), new DummyLogger());

			var eventDataObject = eventDeserializer.Parse(GetEventXML("WTG Tracking & Automation"));
			var parents = finder.GetLogParentsForEvent(eventDataObject);
			AssertEquals("XML has Container Automation data context and matching standalone declaration, so finder should match parents.", 1, parents.Length);
			AssertEquals(parents[0].PK, declaration.PK);

			declaration.JE_JS = shipment.PK;
			parents = finder.GetLogParentsForEvent(eventDataObject);
			AssertEquals("XML has Container Automation data context, but matching declaration is linked, so finder should not match parents.", 0, parents.Length);

			eventDataObject = eventDeserializer.Parse(GetEventXML("A Very Random Company"));
			parents = finder.GetLogParentsForEvent(eventDataObject);
			AssertEquals("XML has matching linked declaration, but non-Container Automation data context, so finder should match parents.", 1, parents.Length);
			AssertEquals(parents[0].PK, declaration.PK);
		}

		public void TestContainerAutomationEventsMatchingDeclarationContainersAreExcluded()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "081-203212";

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "12345";

			Factory.SaveForTesting();

			string GetEventXML(string dataProvider) => $@"
<UniversalEvent xmlns = ""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2019 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>12345</Value>
			</Context>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>081-203212</Value>
			</Context>
		</ContextCollection>
		<DataContext>
			<DataSource>
				<DataProvider>{dataProvider}</DataProvider>
			</DataSource>
		</DataContext>
	</Event>
</UniversalEvent>
";

			var eventDeserializer = new XmlEventDeserializer();
			var finder = new JobDeclarationEventParentFinder(Factory.BOFactory, new JobDeclarationDataContextManager(), new DummyLogger());

			var eventDataObject = eventDeserializer.Parse(GetEventXML("WTG Tracking & Automation"));
			var parents = finder.GetLogParentsForEvent(eventDataObject);
			AssertEquals("XML has Container Automation data context, and matching container with standalone declaration, so finder should match parents.", 1, parents.Length);
			AssertEquals(parents[0].PK, container.CO_JC);

			declaration.JE_JS = shipment.PK;
			parents = finder.GetLogParentsForEvent(eventDataObject);
			AssertEquals("XML has Container Automation data context, but maching container's declaration is linked, so finder should not match parents.", 0, parents.Length);

			eventDataObject = eventDeserializer.Parse(GetEventXML("A Very Random Company"));
			parents = finder.GetLogParentsForEvent(eventDataObject);
			AssertEquals("XML has matching container whose declaration is linked, but non-Container Automation data context, so finder should match parents.", 1, parents.Length);
			AssertEquals(parents[0].PK, container.CO_JC);
		}

		protected virtual JobDeclarationEventParentFinder GetNewEventParentFinder()
		{
			return new JobDeclarationEventParentFinder(Factory.BOFactory, new JobDeclarationDataContextManager(), new DummyLogger());
		}

		protected virtual JobDeclarationEventParentFinder GetNewEventParentFinderWithLogger(IXmlImportLogger logger)
		{
			return new JobDeclarationEventParentFinder(Factory.BOFactory, new JobDeclarationDataContextManager(), logger);
		}

		static string GetHumanReadableID(BusinessObject businessObject)
		{
			return businessObject.HumanReadableName + " (" + businessObject.GetType().FullName + ") - PK: " + businessObject.PK;
		}

		void AssertIncomingEventLinksToAirImportDeclarationWhenUsingMAWBAndDepartmentIsCustoms(string eventXML, BaseJobDeclaration declaration, string expectedAttempts, int expectedLandedPieces, bool expectedContainsMoreThanOneMatchedLog)
		{
			var message = GetQueuedUniversalEventMessage(eventXML);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var sessionTracker = manager.Process(message);
			var attemptedImports = sessionTracker.ImportResults;

			AssertMultilineASCIIEquals("ProcessedOK", expectedAttempts, attemptedImports.FormatAndOrderImportAttempts());
			AssertEquals("JE_LandedPieces ", expectedLandedPieces, declaration.JE_LandedPieces);
			if (expectedContainsMoreThanOneMatchedLog)
			{
				AssertContains("Message Log Note Contains", @"Matching failed. There are more than one declaration matched.", message.GetLogNoteText());
			}
			else
			{
				AssertNotContains("Message Log Note Not Contains", @"Matching failed. There are more than one declaration matched.", message.GetLogNoteText());
			}
		}

		bool ContainsLogEntry(string logText, LogType type, string message)
		{
			string lookingFor = type + " - " + message;
			return (null != logText) && logText.Contains(lookingFor);
		}

		protected override void TearDown()
		{
			base.TearDown();
			testFileHelper?.Dispose();
			testFileHelper = null;
		}

		TestFileHelper TestFileHelper => testFileHelper ??= new ();
		TestFileHelper testFileHelper;

		class BaseJobDeclaration_ContainersAlwaysRequired : BaseJobDeclaration
		{
			public BaseJobDeclaration_ContainersAlwaysRequired(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			public override ZBool ContainersAlwaysRequired => true;
		}
	}
}
