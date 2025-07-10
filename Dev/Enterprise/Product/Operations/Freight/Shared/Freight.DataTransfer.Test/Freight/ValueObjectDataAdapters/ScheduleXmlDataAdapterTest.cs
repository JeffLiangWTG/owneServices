using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.DataAdapters.Testing;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	[TestedType(typeof(ScheduleValueObjectDataAdapter))]
	public class ScheduleXmlDataAdapterTest : ValueObjectDataAdapterTest<JobVoyage, Xsd.Schedule>
	{
		public void TestCollectionSchema()
		{
			AssertEquals("Collection schema should be specified", FreightXmlSchemaDefinitions.Instance.SchedulesSchema, GetNewBizObjXmlDataAdapter().CollectionSchema);
		}

		public void TestExistingOriginUpdated()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VesselName";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "Voyage";
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUMEL";
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";

			Xsd.Schedule scheduleValue = new Xsd.Schedule();
			Xsd.ScheduleSailing scheduleDetailsValue = new Xsd.ScheduleSailing();
			scheduleValue.Item = scheduleDetailsValue;
			scheduleDetailsValue.VesselName = "VesselName";
			scheduleDetailsValue.VoyageNo = "Voyage";
			scheduleDetailsValue.Sailings = new Xsd.SailingWithLoadDischargePortsAndConsolsCollection();
			Xsd.SailingWithLoadDischargePortsAndConsols sailing = scheduleDetailsValue.Sailings.AddNew();
			sailing.LoadPort = "AUMEL";
			sailing.DepartureBerth = 56.ToString();

			ScheduleValueObjectDataAdapter scheduleAdapter = new ScheduleValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			JobVoyage updatedVoyage = scheduleAdapter.CreateOrUpdateFromValueObject(scheduleValue, context);
			AssertEquals("Should find the existing voyage", voyage.PK, updatedVoyage.PK);
			AssertEquals("Should update existing origin", "56", origin.JA_Berth);
		}

		public void TestExistingVesselNotCreated()
		{
			RefVessel vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			AssertNotNull("Vessel does not exists", vessel);
			int count1 = Factory.GetDatabaseCount(typeof(RefVessel));

			ScheduleValueObjectDataAdapter scheduleAdapter = new ScheduleValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			Xsd.Schedule schedule = new Xsd.Schedule();
			Xsd.ScheduleSailing xsdSailing = new Xsd.ScheduleSailing();
			xsdSailing.VesselName = vessel.RV_Name;
			schedule.Item = xsdSailing;

			scheduleAdapter.CreateOrUpdateFromValueObject(schedule, context);

			int count2 = Factory.GetDatabaseCount(typeof(RefVessel));
			AssertEquals("Vessel should not be inserted", count1, count2);
		}

		public void TestExistingDestinationUpdated()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VesselName";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "Voyage";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUMEL";
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "GBLON";
			JobSailing jobSailing = Factory.New<JobSailing>();
			jobSailing.JX_JA = origin.PK;
			jobSailing.JX_JB = destination.PK;

			Xsd.Schedule scheduleValue = new Xsd.Schedule();
			Xsd.ScheduleSailing scheduleDetailsValue = new Xsd.ScheduleSailing();
			scheduleValue.Item = scheduleDetailsValue;
			scheduleDetailsValue.VesselName = "VesselName";
			scheduleDetailsValue.VoyageNo = "Voyage";
			scheduleDetailsValue.Sailings = new Xsd.SailingWithLoadDischargePortsAndConsolsCollection();
			Xsd.SailingWithLoadDischargePortsAndConsols sailing = scheduleDetailsValue.Sailings.AddNew();
			sailing.DischargePort = "AUMEL";
			sailing.LoadPort = "GBLON";
			sailing.ArrivalBerth = 56.ToString();

			ScheduleValueObjectDataAdapter scheduleAdapter = new ScheduleValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			JobVoyage updatedVoyage = scheduleAdapter.CreateOrUpdateFromValueObject(scheduleValue, context);
			AssertNotNull(updatedVoyage);
			AssertEquals("Should find the existing voyage", voyage.PK, updatedVoyage.PK);
			AssertEquals("Should update existing Destination", "56", destination.JB_Berth);
		}

		public void TestExistingSailingUpdated()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VesselName";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "Voyage";

			VoyageOrigin origin = voyage.Origins.AddNew();
			VoyageDestination destination = voyage.Destinations.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			destination.JB_RL_NKPortOfDischarge = "AUMEL";
			JobSailing sailing = voyage.Sailings.AddNew();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			Xsd.Schedule scheduleValue = new Xsd.Schedule();
			Xsd.ScheduleSailing scheduleDetailsValue = new Xsd.ScheduleSailing();
			scheduleValue.Item = scheduleDetailsValue;
			scheduleDetailsValue.Sailings = new Xsd.SailingWithLoadDischargePortsAndConsolsCollection();
			Xsd.SailingWithLoadDischargePortsAndConsols sailingValue = scheduleDetailsValue.Sailings.AddNew();
			scheduleDetailsValue.VesselName = "VesselName";
			scheduleDetailsValue.VoyageNo = "Voyage";

			sailingValue.LoadPort = "AUSYD";
			sailingValue.DischargePort = "AUMEL";
			sailingValue.LCLDates = new Xsd.SailingDates();
			sailingValue.LCLDates.CutOffDate = new ZDateTime(2005, 2, 3).ToDateTime();

			ScheduleValueObjectDataAdapter scheduleAdapter = new ScheduleValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			JobVoyage updatedVoyage = scheduleAdapter.CreateOrUpdateFromValueObject(scheduleValue, context);
			AssertEquals("Should find the existing voyage", voyage.PK, updatedVoyage.PK);
			AssertEquals("Should update existing origin", sailingValue.LCLDates.CutOffDate, sailing.JX_DepotCutOff.ToDateTime());
		}

		public void TestVoyageFlightMaxLegth()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VesselName";

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "SydneyAirl";
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "JPTYO";

			Xsd.Schedule scheduleValue = new Xsd.Schedule();
			Xsd.ScheduleSailing scheduleDetailsValue = new Xsd.ScheduleSailing();
			scheduleValue.Item = scheduleDetailsValue;
			scheduleDetailsValue.VesselName = "VesselName";
			scheduleDetailsValue.VoyageNo = "SydneyAirline";

			var notificationBuffer = new NotificationBuffer();

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, notificationBuffer);
			JobVoyage updatedVoyage = new ScheduleValueObjectDataAdapter().CreateOrUpdateFromValueObject(scheduleValue, importContext);
			Assert(notificationBuffer.AsString.Contains("JV_VoyageFlight accepts max 10 characters but 13 were entered"));
			AssertEquals("VesselName", updatedVoyage.JV_RV_NKVessel);
			AssertEquals("SydneyAirl", updatedVoyage.JV_VoyageFlight);

			notificationBuffer.Clear();
			Xsd.Schedule scheduleValue1 = new Xsd.Schedule();
			Xsd.ScheduleSailing scheduleDetailsValue1 = new Xsd.ScheduleSailing();
			scheduleValue1.Item = scheduleDetailsValue1;
			scheduleDetailsValue1.VesselName = "VesselNoVoyagehastobethirtyfivecharacters";
			scheduleDetailsValue1.VoyageNo = "AirlineSYD";
			scheduleDetailsValue1.Sailings.AddNew();
			scheduleDetailsValue1.Sailings[0].LoadPort = "AUSYD";
			scheduleDetailsValue1.Sailings[0].DischargePort = "NZAKL";

			ValueObjectImportContext importContext1 = new ValueObjectImportContext(Factory, notificationBuffer);
			JobVoyage updatedVoyage1 = new ScheduleValueObjectDataAdapter().CreateOrUpdateFromValueObject(scheduleValue1, importContext1);
			Assert(notificationBuffer.AsString.Contains("JV_RV_NKVessel accepts max 35 characters but 41 were entered"));
			AssertEquals("VesselNoVoyagehastobethirtyfivechar", updatedVoyage1.JV_RV_NKVessel);
			AssertEquals("AirlineSYD", updatedVoyage1.JV_VoyageFlight);

			notificationBuffer.Clear();
			Xsd.Schedule scheduleValue2 = new Xsd.Schedule();
			scheduleValue2.TransportMode = Xsd.TransportMode.ROA;

			Xsd.ScheduleRoadRailFlight flight = new Xsd.ScheduleRoadRailFlight();
			scheduleValue2.Item = flight;

			flight.FlightNoJourneyNoTruckRegNo = "SydneyRoadWays";
			flight.Flights = new Xsd.FlightWithLoadDischargePortsAndConsolsCollection();
			flight.Flights.AddNew();
			flight.Flights[0].ETD = ZDateTime.Now;
			flight.Flights[0].LoadPort = "AUBNE";
			flight.Flights[0].DischargePort = "AUSYD";

			ValueObjectImportContext importContext2 = new ValueObjectImportContext(Factory, notificationBuffer);
			JobVoyage updatedVoyage2 = new ScheduleValueObjectDataAdapter().CreateOrUpdateFromValueObject(scheduleValue2, importContext2);
			Assert(notificationBuffer.AsString.Contains("JV_VoyageFlight accepts max 10 characters but 14 were entered"));
			AssertEquals("SydneyRoad", updatedVoyage2.JV_VoyageFlight);
		}

		public void TestImportCarrier()
		{
			Xsd.Schedule scheduleValue = new Xsd.Schedule();
			scheduleValue.Carrier = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "Carrier", OrganisationTypes.Carrier);

			Xsd.ScheduleSailing sailingDetails = new Xsd.ScheduleSailing();
			sailingDetails.VesselName = "VesselName";
			sailingDetails.VoyageNo = "Voyage";

			Xsd.SailingWithLoadDischargePorts portPair = sailingDetails.Sailings.AddNew();
			portPair.LoadPort = "AUSYD";
			portPair.DischargePort = "AUMEL";
			scheduleValue.Item = sailingDetails;

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			JobVoyage importedSchedule = new ScheduleValueObjectDataAdapter().CreateOrUpdateFromValueObject(scheduleValue, importContext);
			AssertEquals("The correct carrier should be imported", "Carrier", importedSchedule.Line.OH_FullName);
			AssertEquals("The correct carrier should be imported", true, importedSchedule.Line.OH_IsShippingProvider);
		}

		public void TestCreateOrUpdateFromValueObject_Sea_CarrierIsIncludedInMatching()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "Carrier1";

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_Code = "Carrier2";

			var helper = new VoyageTestHelper(Factory);
			var voyage1 = helper.CreateSeaVoyage("VesselName", "123", carrier1.PK);
			var voyage2 = helper.CreateSeaVoyage("VesselName", "123", carrier2.PK);

			var scheduleValue = new Xsd.Schedule();
			scheduleValue.Carrier = new Xsd.Organisation { EDICode = "Carrier1" };

			var sailingDetails = new Xsd.ScheduleSailing();
			sailingDetails.VesselName = "VesselName";
			sailingDetails.VoyageNo = "123";

			var portPair = sailingDetails.Sailings.AddNew();
			portPair.LoadPort = "AUSYD";
			portPair.DischargePort = "NZAKL";
			scheduleValue.Item = sailingDetails;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var importedSchedule = new ScheduleValueObjectDataAdapter().CreateOrUpdateFromValueObject(scheduleValue, context);
			AssertEquals("Existing schedule matched", voyage1.PK, importedSchedule.PK);

			scheduleValue.Carrier = new Xsd.Organisation { EDICode = "Carrier2" };
			importedSchedule = new ScheduleValueObjectDataAdapter().CreateOrUpdateFromValueObject(scheduleValue, context);
			AssertEquals("Existing schedule matched", voyage2.PK, importedSchedule.PK);
		}

		public void TestFindOrCeateSeaRailVoyage()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsShippingLine = true;
			carrier1.OH_FullName = "CarrierONE";

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_FullName = "CarrierTWO";

			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier3.OH_IsShippingLine = true;
			carrier3.OH_FullName = "CarrierTHREE";

			var helper = new VoyageTestHelper(Factory);
			var voyage1 = helper.CreateSeaVoyage("VesselName", "123", carrier1.PK);
			var voyage2 = helper.CreateSeaVoyage("VesselName", "123", carrier2.PK);

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			var voyage = new ScheduleValueObjectDataAdapter().FindOrCeateSeaRailVoyage(Factory, "SEA", "VesselName", "123", carrier1.PK);
			AssertEquals("Carrier is included in matching", voyage1, voyage);

			voyage = new ScheduleValueObjectDataAdapter().FindOrCeateSeaRailVoyage(Factory, "SEA", "VesselName", "123", carrier2.PK);
			AssertEquals("Carrier is included in matching", voyage2, voyage);

			voyage = new ScheduleValueObjectDataAdapter().FindOrCeateSeaRailVoyage(Factory, "SEA", "VesselName", "123", carrier3.PK);
			AssertEquals("Carrier was defaulted on new voyage", carrier3.PK, voyage.JV_OH_Line);
		}

		public void TestExport_Air_Ports()
		{
			var airSchedule = Factory.NewWithValidTestData<JobVoyage>();
			airSchedule.JV_AirSeaRoad = Constants.TransportModes.Air;

			airSchedule.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			airSchedule.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			airSchedule.GenerateSailings();

			var scheduleValue = new Xsd.Schedule();
			new ScheduleValueObjectDataAdapter().ExportToValueObject(airSchedule, scheduleValue, new ValueObjectExportContext(new NotificationBuffer()));

			var item = scheduleValue.Item as Xsd.ScheduleRoadRailFlight;
			AssertEquals("AUSYD", item.Flights[0].LoadPort);
			AssertEquals("NZAKL", item.Flights[0].DischargePort);
		}

		public void TestExport_Sea_Ports()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;

			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.GenerateSailings();

			var scheduleValue = new Xsd.Schedule();
			new ScheduleValueObjectDataAdapter().ExportToValueObject(voyage, scheduleValue, new ValueObjectExportContext(new NotificationBuffer()));

			var item = scheduleValue.Item as Xsd.ScheduleSailing;
			AssertEquals("AUSYD", item.Sailings[0].LoadPort);
			AssertEquals("NZAKL", item.Sailings[0].DischargePort);
		}

		#region Import Errors and Messages

		[ExpectNoExceptions]
		public void TestImportMessages()
		{
			var supportedTransportModes = new[] { Xsd.TransportMode.AIR, Xsd.TransportMode.RAI, Xsd.TransportMode.ROA, Xsd.TransportMode.SEA };

			var notSupportedTransportModes = from t in Enum.GetValues(typeof(Xsd.TransportMode)).Cast<Xsd.TransportMode>()
											 where !supportedTransportModes.Contains(t)
											 select t;

			notSupportedTransportModes.ToList().ForEach(AssertNotSupportedMessage);

			Xsd.Schedule schedule = new Xsd.Schedule();
			schedule.TransportMode = Xsd.TransportMode.ROA;
			schedule.Item = new Xsd.ScheduleSailing();
			AssertInvalidScheduleData(schedule);

			schedule.TransportMode = Xsd.TransportMode.AIR;
			AssertInvalidScheduleData(schedule);

			schedule.TransportMode = Xsd.TransportMode.RAI;
			schedule.Item = new Xsd.ScheduleRoadRailFlight();
			AssertInvalidScheduleData(schedule);

			schedule.TransportMode = Xsd.TransportMode.SEA;
			AssertInvalidScheduleData(schedule);

			AssertScheduleWithInvalidETD();
		}

		public void TestImportIncompleteSchedule()
		{
			var scheduleValue = new Xsd.Schedule();
			var scheduleDetailsValue = new Xsd.ScheduleSailing();
			scheduleValue.Item = scheduleDetailsValue;
			scheduleDetailsValue.Sailings = new Xsd.SailingWithLoadDischargePortsAndConsolsCollection();
			scheduleDetailsValue.VesselName = "VesselName1";
			scheduleDetailsValue.VoyageNo = "Voyage1";

			var sailingValue = scheduleDetailsValue.Sailings.AddNew();
			sailingValue.LoadPort = "AUSYD";
			sailingValue.ETD = new ZDateTime(2005, 2, 3).ToDateTime();

			var scheduleAdapter = new ScheduleValueObjectDataAdapter();
			var notificationBuffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notificationBuffer);
			var importedVoyage = scheduleAdapter.CreateOrUpdateFromValueObject(scheduleValue, context);

			AssertNull("Should not save incomplete voyage", importedVoyage);
			Assert(notificationBuffer.AsString.Contains(@"Error: Could not locate an existing matching schedule - Sailing Schedule (Vessel='VesselName1', Voyage='Voyage1', Carrier='').
Cannot create a schedule with an incomplete port pair. Schedule must have both Load port and Destination port data specified."));

			scheduleDetailsValue.VesselName = "VesselName2";
			scheduleDetailsValue.VoyageNo = "Voyage2";

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VesselName2";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "Voyage2";

			var origin = voyage.Origins.AddNew();
			var destination = voyage.Destinations.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			destination.JB_RL_NKPortOfDischarge = "AUMEL";
			var sailing = voyage.Sailings.AddNew();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			notificationBuffer = new NotificationBuffer();
			context = new ValueObjectImportContext(Factory, notificationBuffer);
			importedVoyage = scheduleAdapter.CreateOrUpdateFromValueObject(scheduleValue, context);

			AssertEquals("Existing voyage should be found", voyage.PK, importedVoyage.PK);
			AssertEquals("Should update existing origin ETD date", sailingValue.ETD, origin.JA_E_DEP.ToDateTime());
		}

		void AssertNotSupportedMessage(Xsd.TransportMode transportMode)
		{
			Xsd.Schedule scheduleValue = new Xsd.Schedule();
			scheduleValue.TransportMode = transportMode;

			var notificationBuffer = new NotificationBuffer();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, notificationBuffer);
			JobVoyage importedSchedule = new ScheduleValueObjectDataAdapter().CreateOrUpdateFromValueObject(scheduleValue, importContext);

			AssertNull(importedSchedule);
			Assert(notificationBuffer.AsString.Contains(String.Format("{0} schedule import is not supported.", transportMode)));
		}

		void AssertInvalidScheduleData(Xsd.Schedule invalidSchedule)
		{
			var notificationBuffer = new NotificationBuffer();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, notificationBuffer);
			JobVoyage importedSchedule = new ScheduleValueObjectDataAdapter().CreateOrUpdateFromValueObject(invalidSchedule, importContext);

			AssertNull(importedSchedule);
			Assert(notificationBuffer.AsString.Contains("Schedule data is not in valid format."));
		}

		void AssertScheduleWithInvalidETD()
		{
			JobVoyage airSchedule = Factory.NewWithValidTestData<JobVoyage>();
			airSchedule.JV_AirSeaRoad = Core.Constants.TransportModes.Air;

			var notificationBuffer = new NotificationBuffer();

			Xsd.Schedule scheduleValue = new Xsd.Schedule();
			new ScheduleValueObjectDataAdapter().ExportToValueObject(airSchedule, scheduleValue, new ValueObjectExportContext(notificationBuffer));
			(scheduleValue.Item as Xsd.ScheduleRoadRailFlight).Flights.Clear();

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, notificationBuffer);
			JobVoyage importedSchedule = new ScheduleValueObjectDataAdapter().CreateOrUpdateFromValueObject(scheduleValue, importContext);

			AssertNull(importedSchedule);
			Assert(notificationBuffer.AsString.Contains("Departure date is required for Road and Air schedules."));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			SystemDataRegistry.Instance.AllowDepartureContainerYardAddressImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.AllowDepartureCTOAddressImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.AllowDepartureDepotAddressImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.AllowExportBrokerImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
		}

		#region Overrides for base test

		protected override ValueObjectDataAdapter<JobVoyage, Xsd.Schedule> GetNewBizObjXmlDataAdapter()
		{
			return new ScheduleValueObjectDataAdapter();
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "Schedules"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "Schedule"; }
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return false; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		readonly string BaseTestFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Freight\Shared\Freight.DataTransfer\Freight\Testing\";

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "vesselname";

			JobVoyage emptyVoyage = Factory.New<JobVoyage>();
			emptyVoyage.JV_RV_NKVessel = vessel.RV_FK;
			emptyVoyage.JV_VoyageFlight = "voyage";
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.EmptySchedule.xml", "EmptySchedule.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyVoyage, expectedOutputFilename, ValidationKind.DontExpectToImportAnythingFromInterchange, "Empty Schedule");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			JobVoyage airVoyage = CreatePopulatedVoyage(Core.Constants.TransportModes.Air);
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.AirSchedule.xml", "AirSchedule.xml");
			return new BusinessObjectAndExpectedOutputFileName(airVoyage, expectedOutputFilename, ValidationKind.Xsd | ValidationKind.FactorySave, "Populated Air Schedule");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			List<BusinessObjectAndExpectedOutputFileName> result = new List<BusinessObjectAndExpectedOutputFileName>();

			var expectedSeaScheduleFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.SeaSchedule.xml", "SeaSchedule.xml");
			JobVoyage seaVoyage = CreatePopulatedVoyage(Core.Constants.TransportModes.Sea);
			result.Add(new BusinessObjectAndExpectedOutputFileName(seaVoyage, expectedSeaScheduleFilename, ValidationKind.Xsd | ValidationKind.FactorySave, "Populated Sea Schedule"));

			var expectedRailScheduleFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.RailSchedule.xml", "RailSchedule.xml");
			JobVoyage railVoyage = CreatePopulatedVoyage(Core.Constants.TransportModes.Rail);
			result.Add(new BusinessObjectAndExpectedOutputFileName(railVoyage, expectedRailScheduleFilename, ValidationKind.Xsd | ValidationKind.FactorySave, "Populated Rail Schedule"));

			var expectedRoadScheduleFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.RoadSchedule.xml", "RoadSchedule.xml");
			JobVoyage roadVoyage = CreatePopulatedVoyage(Core.Constants.TransportModes.Road);
			result.Add(new BusinessObjectAndExpectedOutputFileName(roadVoyage, expectedRoadScheduleFilename, ValidationKind.Xsd | ValidationKind.FactorySave, "Populated Road Schedule"));

			return result.ToArray();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		JobVoyage CreatePopulatedVoyage(string transportMode)
		{
			JobVoyage result = Factory.NewWithValidTestData<JobVoyage>(TestBusinessObjectKind.All);
			result.JV_AirSeaRoad = transportMode;
			result.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().PK;
			result.Line.OH_FullName = "Carrier";

			AssertEquals("Prequisite", 1, result.Origins.Count);
			AssertEquals("Prequisite", 1, result.Destinations.Count);
			AssertEquals("Prequisite", 1, result.Sailings.Count);

			JobSailing sailing = result.Sailings[0];

			sailing.FillWithValidTestData(TestBusinessObjectKind.All & ~TestBusinessObjectKind.PopulateDependentCollections, Array.Empty<PropertyDescriptor>());

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = transportMode;

			Transport transport = consol.Transports[0];
			transport.JW_JX = sailing.PK;

			consol.JK_UniqueConsignRef = "";
			consol.JK_AgentsReference = "";
			return result;
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					// covered by other data adapters
					"Carrier/OrganisationDetails",
					"Carrier/Notes/CustomNoteTypeName",
					"Carrier/Notes/NoteData",
					"Carrier/Notes/NoteCreatedDateTime",

					"LoadPorts/DepartureCTO/Organisation",
					"DischargePorts/ArrivalCTO/Organisation",
					"Sailings",
				};
			}
		}

		#endregion

		#endregion
	}
}
