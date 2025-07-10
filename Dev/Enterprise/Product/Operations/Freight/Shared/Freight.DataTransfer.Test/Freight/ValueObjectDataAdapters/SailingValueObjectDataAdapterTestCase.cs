using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using SystemDataRegistry = Enterprise.Registry.Business.SystemDataRegistry;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	public abstract class SailingValueObjectDataAdapterTestCase : ValueObjectDataAdapterTest<JobSailing, Xsd.SailingBase>
	{
		public void TestRunExport_ExecutesExportToValueObject()
		{
			var origin = Factory.New<VoyageOrigin>();
			origin.JA_E_ARV = new ZDateTime(2013, 5, 1);

			var sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;

			var context = new ValueObjectExportContext(Notifications);
			var adapter = new SailingValueObjectDataAdapter("", "", "", "", "", ZGuid.Empty);

			var sailingValue1 = new Xsd.Sailing();
			adapter.ExportToValueObject(sailing, sailingValue1, context);

			var sailingValue2 = new Xsd.Sailing();
			SailingValueObjectDataAdapter.RunExport(sailing, sailingValue2, context);

			AssertEquals("Export results are consistent", sailingValue1.LoadPortETA, sailingValue2.LoadPortETA);
		}

		public void TestExportArrivalAtLoadPortDates()
		{
			JobSailing sailing = Factory.New<JobSailing>();
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_E_ARV = new ZDateTime(2011, 6, 10);
			origin.JA_A_ARV = new ZDateTime(2011, 6, 15);
			sailing.JX_JA = origin.PK;

			ValueObjectExportContext context = new ValueObjectExportContext(Notifications);
			SailingValueObjectDataAdapter adapter = new SailingValueObjectDataAdapter("SEA", "", "", "VESSEL", "VOYG", ZGuid.Empty);

			Xsd.Sailing sailingValue = new Xsd.Sailing();
			adapter.ExportToValueObject(sailing, sailingValue, context);
			AssertEquals("LoadPortETA", new ZDateTime(2011, 6, 10), sailingValue.LoadPortETA);
			AssertEquals("LoadPortATA", new ZDateTime(2011, 6, 15), sailingValue.LoadPortATA);
		}

		public void TestImport_CarrierIsUsedToMatchVoyage()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			var helper = new VoyageTestHelper(Factory);
			var voyage1 = helper.CreateSeaVoyage("Visund", "123", carrier1.PK, "AUSYD", "AUMEL");
			var voyage2 = helper.CreateSeaVoyage("Visund", "123", carrier2.PK, "AUSYD", "AUMEL");

			var sailingValue = new Xsd.Sailing();
			sailingValue.LoadPortETA = new ZDateTime(2013, 5, 1);

			var context = new ValueObjectImportContext(Factory, Notifications);
			var adapter = new SailingValueObjectDataAdapter("SEA", "AUSYD", "AUMEL", "Visund", "123", carrier2.PK);

			var sailing = adapter.CreateOrUpdateFromValueObject(sailingValue, context);
			AssertEquals("Sailing's voyage matched using carrier", new ZDateTime(2013, 5, 1), voyage2.Sailings[0].Origin.JA_E_ARV);
		}

		public void TestImportArrivalAtLoadPortDates()
		{
			Xsd.Sailing sailingValue = new Xsd.Sailing();
			sailingValue.LoadPortETA = new ZDateTime(2011, 10, 5);
			sailingValue.LoadPortATA = new ZDateTime(2011, 10, 20);

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notifications);
			SailingValueObjectDataAdapter adapter = new SailingValueObjectDataAdapter("SEA", "AUSYD", "NZAKL", "VESSEL", "VOYG", ZGuid.Empty);

			JobSailing sailing = adapter.CreateOrUpdateFromValueObjectWithSchedule(sailingValue, context);
			AssertEquals("JA_E_ARV", new ZDateTime(2011, 10, 5), sailing.Origin.JA_E_ARV);
			AssertEquals("JA_A_ARV", new ZDateTime(2011, 10, 20), sailing.Origin.JA_A_ARV);
		}

		public void TestCreateOrUpdateFromValueObject_EmptyPorts()
		{
			Xsd.Sailing sailingValue = new Xsd.Sailing();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notifications);
			SailingValueObjectDataAdapter adapter = new SailingValueObjectDataAdapter("SEA", "", "", "VESSEL", "VOYG", ZGuid.Empty);
			adapter.CreateOrUpdateFromValueObject(sailingValue, context);
			Assert("Notification should have errors", Notifications.HasErrors);
			Assert(Notifications.AsString.Contains("Both Load and Discharge Ports are missing"));
		}

		public void TestCreateAndUpdateFromValueObject_EmptyLoadPort()
		{
			RefVessel vessel = (Factory.LoadTop1<RefVessel>(new ZQuery()));
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_FlightDate = new ZDateTime(2006, 12, 8);
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "VOYG";
			ZDateTime currentDate = new ZDateTime(2006, 12, 6);

			VoyageOrigin lON_Origin = GetNewOrigin(voyage, "GBLON", currentDate);
			VoyageOrigin sIN_Origin = GetNewOrigin(voyage, "SGSIN", currentDate.AddDays(4));
			VoyageDestination sIN_Destination = GetNewDestination(voyage, "SGSIN", currentDate.AddDays(2));
			VoyageDestination sYD_Destination = GetNewDestination(voyage, "AUSYD", currentDate.AddDays(6));

			Factory.Save();

			JobSailing sailingLON_SIN = voyage.Sailings.GetSailingFromLoadAndDischarge("GBLON", "SGSIN");
			JobSailing sailingLON_SYD = voyage.Sailings.GetSailingFromLoadAndDischarge("GBLON", "AUSYD");
			JobSailing sailingSIN_SYD = voyage.Sailings.GetSailingFromLoadAndDischarge("SGSIN", "AUSYD");

			Xsd.SailingWithLoadDischargePorts sailingValue = new Xsd.SailingWithLoadDischargePorts();
			sailingValue.DischargePort = "AUSYD";
			sailingValue.ETA = new ZDateTime(2006, 12, 22);
			sailingValue.FCLDates.AvailableDate = new ZDateTime(2006, 12, 22);
			sailingValue.FCLDates.StorageDate = new ZDateTime(2006, 12, 22);
			sailingValue.LCLDates.AvailableDate = new ZDateTime(2006, 12, 22);
			sailingValue.LCLDates.StorageDate = new ZDateTime(2006, 12, 22);

			SailingValueObjectDataAdapter adapter = new SailingValueObjectDataAdapter("SEA", "", "AUSYD", vessel.RV_Name, "VOYG", ZGuid.Empty);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notifications);
			adapter.CreateOrUpdateFromValueObject(sailingValue, context);
			Assert(Notifications.AsString.Contains("Updating Sailing Port Pair (Load='GBLON' Discharge='AUSYD')"));
			Assert(Notifications.AsString.Contains("Updating Sailing Port Pair (Load='SGSIN' Discharge='AUSYD')"));

			AssertAvailableAndStorageDates(sailingSIN_SYD);
			AssertAvailableAndStorageDates(sailingLON_SYD);
		}

		public void TestCreateAndUpdateFromValueObject_EmptyLoadPort_CreateDestination()
		{
			RefVessel vessel = (Factory.LoadTop1<RefVessel>(new ZQuery()));
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_FlightDate = new ZDateTime(2006, 12, 8);
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "VOYG";
			ZDateTime currentDate = new ZDateTime(2006, 12, 6);

			VoyageOrigin lON_Origin = GetNewOrigin(voyage, "GBLON", currentDate);
			VoyageOrigin sIN_Origin = GetNewOrigin(voyage, "SGSIN", currentDate.AddDays(4));
			VoyageDestination sIN_Destination = GetNewDestination(voyage, "SGSIN", currentDate.AddDays(2));

			Factory.Save();

			JobSailing sailingLON_SIN = voyage.Sailings.GetSailingFromLoadAndDischarge("GBLON", "SGSIN");
			AssertNotNull("Prequisite", sailingLON_SIN);

			Xsd.SailingWithLoadDischargePorts sailingValue = new Xsd.SailingWithLoadDischargePorts();
			sailingValue.DischargePort = "AUSYD";
			sailingValue.ETA = new ZDateTime(2006, 12, 22);
			sailingValue.FCLDates.AvailableDate = new ZDateTime(2006, 12, 22);
			sailingValue.FCLDates.StorageDate = new ZDateTime(2006, 12, 22);
			sailingValue.LCLDates.AvailableDate = new ZDateTime(2006, 12, 22);
			sailingValue.LCLDates.StorageDate = new ZDateTime(2006, 12, 22);

			SailingValueObjectDataAdapter adapter = new SailingValueObjectDataAdapter("SEA", "", "AUSYD", vessel.RV_Name, "VOYG", ZGuid.Empty);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notifications);
			adapter.CreateOrUpdateFromValueObject(sailingValue, context);
			Assert(Notifications.AsString.Contains("Updating Sailing Port Pair (Load='GBLON' Discharge='AUSYD')"));
			Assert(Notifications.AsString.Contains("Updating Sailing Port Pair (Load='SGSIN' Discharge='AUSYD')"));

			Factory.Save();

			Assert(voyage.Sailings.Count == 3);

			JobSailing sailingLON_SYD = voyage.Sailings.GetSailingFromLoadAndDischarge("GBLON", "AUSYD");
			AssertNotNull(sailingLON_SYD);
			JobSailing sailingSIN_SYD = voyage.Sailings.GetSailingFromLoadAndDischarge("SGSIN", "AUSYD");
			AssertNotNull(sailingSIN_SYD);

			AssertContainsExactElementsInAnyOrder(voyage.Sailings.ToArray(), new JobSailing[] { sailingLON_SIN, sailingSIN_SYD, sailingLON_SYD });

			AssertAvailableAndStorageDates(sailingSIN_SYD);
			AssertAvailableAndStorageDates(sailingLON_SYD);
		}

		void AssertAvailableAndStorageDates(JobSailing sailing)
		{
			ZDateTime currentDate = new ZDateTime(2006, 12, 22);
			AssertEquals("CTO Available", currentDate, sailing.JX_JB_CTOAvailabilityDate);
			AssertEquals("CTO Storage", currentDate, sailing.JX_JB_CTOStorageDate);
			AssertEquals("CFS Available", currentDate, sailing.JX_DepotAvailabilityDate);
			AssertEquals("CFS Storage", currentDate, sailing.JX_DepotStorageDate);
		}

		public void TestCreateAndUpdateFromValueObject_EmptyDestinationPort()
		{
			RefVessel vessel = (Factory.LoadTop1<RefVessel>(new ZQuery()));
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_FlightDate = new ZDateTime(2006, 12, 8);
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "VOYG";
			ZDateTime currentDate = new ZDateTime(2006, 12, 6);

			VoyageOrigin lON_Origin = GetNewOrigin(voyage, "GBLON", currentDate);
			VoyageOrigin sIN_Origin = GetNewOrigin(voyage, "SGSIN", currentDate.AddDays(4));
			VoyageDestination sIN_Destination = GetNewDestination(voyage, "SGSIN", currentDate.AddDays(2));
			VoyageDestination sYD_Destination = GetNewDestination(voyage, "AUSYD", currentDate.AddDays(6));

			Factory.Save();

			JobSailing sailingLON_SIN = voyage.Sailings.GetSailingFromLoadAndDischarge("GBLON", "SGSIN");
			JobSailing sailingLON_SYD = voyage.Sailings.GetSailingFromLoadAndDischarge("GBLON", "AUSYD");
			JobSailing sailingSIN_SYD = voyage.Sailings.GetSailingFromLoadAndDischarge("SGSIN", "AUSYD");

			Xsd.SailingWithLoadDischargePorts sailingValue = new Xsd.SailingWithLoadDischargePorts();
			sailingValue.LoadPort = "GBLON";
			sailingValue.ETA = new ZDateTime(2006, 12, 22);
			sailingValue.FCLDates.CutOffDate = new ZDateTime(2006, 12, 22);
			sailingValue.FCLDates.ReceivalCommencesDate = new ZDateTime(2006, 12, 22);
			sailingValue.LCLDates.CutOffDate = new ZDateTime(2006, 12, 22);
			sailingValue.LCLDates.ReceivalCommencesDate = new ZDateTime(2006, 12, 22);

			SailingValueObjectDataAdapter adapter = new SailingValueObjectDataAdapter("SEA", "GBLON", "", vessel.RV_Name, "VOYG", ZGuid.Empty);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notifications);
			adapter.CreateOrUpdateFromValueObject(sailingValue, context);
			Assert(Notifications.AsString.Contains("Updating Sailing Port Pair (Load='GBLON' Discharge='SGSIN')"));
			Assert(Notifications.AsString.Contains("Updating Sailing Port Pair (Load='GBLON' Discharge='AUSYD')"));

			AssertCutOffAndReceivalDates(sailingLON_SIN);
			AssertCutOffAndReceivalDates(sailingLON_SYD);
		}

		public void TestCreateAndUpdateFromValueObject_EmptyDestinationPort_CreateOrigin()
		{
			RefVessel vessel = (Factory.LoadTop1<RefVessel>(new ZQuery()));
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_FlightDate = new ZDateTime(2006, 12, 8);
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "VOYG";
			ZDateTime currentDate = new ZDateTime(2006, 12, 6);

			VoyageOrigin sIN_Origin = GetNewOrigin(voyage, "SGSIN", currentDate.AddDays(4));
			VoyageDestination sIN_Destination = GetNewDestination(voyage, "SGSIN", currentDate.AddDays(2));
			VoyageDestination sYD_Destination = GetNewDestination(voyage, "AUSYD", currentDate.AddDays(6));

			Factory.Save();

			JobSailing sailingSIN_SYD = voyage.Sailings.GetSailingFromLoadAndDischarge("SGSIN", "AUSYD");
			AssertNotNull("Prequisite", sailingSIN_SYD);

			Xsd.SailingWithLoadDischargePorts sailingValue = new Xsd.SailingWithLoadDischargePorts();
			sailingValue.LoadPort = "GBLON";
			sailingValue.ETA = new ZDateTime(2006, 12, 22);
			sailingValue.FCLDates.CutOffDate = new ZDateTime(2006, 12, 22);
			sailingValue.FCLDates.ReceivalCommencesDate = new ZDateTime(2006, 12, 22);
			sailingValue.LCLDates.CutOffDate = new ZDateTime(2006, 12, 22);
			sailingValue.LCLDates.ReceivalCommencesDate = new ZDateTime(2006, 12, 22);

			SailingValueObjectDataAdapter adapter = new SailingValueObjectDataAdapter("SEA", "GBLON", "", vessel.RV_Name, "VOYG", ZGuid.Empty);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notifications);
			adapter.CreateOrUpdateFromValueObject(sailingValue, context);
			Assert(Notifications.AsString.Contains("Updating Sailing Port Pair (Load='GBLON' Discharge='SGSIN')"));
			Assert(Notifications.AsString.Contains("Updating Sailing Port Pair (Load='GBLON' Discharge='AUSYD')"));

			Factory.Save();

			Assert(voyage.Sailings.Count == 3);

			JobSailing sailingLON_SIN = voyage.Sailings.GetSailingFromLoadAndDischarge("GBLON", "SGSIN");
			AssertNotNull(sailingLON_SIN);
			JobSailing sailingLON_SYD = voyage.Sailings.GetSailingFromLoadAndDischarge("GBLON", "AUSYD");
			AssertNotNull(sailingLON_SYD);

			AssertContainsExactElementsInAnyOrder(voyage.Sailings.ToArray(), new JobSailing[] { sailingLON_SIN, sailingSIN_SYD, sailingLON_SYD });

			AssertCutOffAndReceivalDates(sailingLON_SIN);
			AssertCutOffAndReceivalDates(sailingLON_SYD);
		}

		[ExpectNoExceptions]
		public void TestCreateAndUpdateFromValueObject_InvalidDateCombination()
		{
			var currentDate = new ZDateTime(2024, 7, 24);

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_FlightDate = currentDate;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "VOYG";
			voyage.EnableOrphanedVoyageReportingForTests = true;
			_ = GetNewOrigin(voyage, "SGSIN", currentDate.AddDays(1));
			_ = GetNewDestination(voyage, "AUSYD", currentDate.AddDays(5));
			Factory.Save();

			TestCase("SGSIN", "AUSYD", currentDate.AddDays(1), currentDate.AddDays(-1));
			TestCase("SGSIN", "AUSYD", currentDate.AddDays(6), currentDate.AddDays(5));
			TestCase("", "AUSYD", currentDate.AddDays(1), currentDate.AddDays(-1));
			TestCase("SGSIN", "", currentDate.AddDays(6), currentDate.AddDays(5));

			void TestCase(string loadPort, string dischargePort, ZDateTime etd, ZDateTime eta)
			{
				var sailingValue = new Xsd.SailingWithLoadDischargePorts();
				sailingValue.LoadPort = loadPort;
				sailingValue.DischargePort = dischargePort;
				sailingValue.ETD = etd;
				sailingValue.ETA = eta;

				var adapter = new SailingValueObjectDataAdapter("SEA", sailingValue.LoadPort, sailingValue.DischargePort, vessel.RV_Name, "VOYG", ZGuid.Empty);
				var context = new ValueObjectImportContext(Factory, Notifications);
				var sailing = adapter.CreateOrUpdateFromValueObject(sailingValue, context);

				AssertNull(sailing);
				Assert("Notification should have errors", Notifications.HasErrors);
				Assert(Notifications.AsString.Contains($"Invalid date combination from Origin (SGSIN {etd}) to Destination (AUSYD {eta})."));
				Notifications.Clear();

				using (Globals.SetIsUserInteractiveForTest(false))
				using (FreightDataRegistry.Instance.EnableSailingGenerationOnServiceTaskSaving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					Factory.Save();
				}
			}
		}

		void AssertCutOffAndReceivalDates(JobSailing sailing)
		{
			ZDateTime currentDate = new ZDateTime(2006, 12, 22);
			AssertEquals("CTO Cut Off", currentDate, sailing.JX_JA_CTOCutOff);
			AssertEquals("CTO Receival Start", currentDate, sailing.JX_JA_CTOReceivalCommences);
			AssertEquals("CFS Cut Off", currentDate, sailing.JX_DepotCutOff);
			AssertEquals("CFS Receival Start", currentDate, sailing.JX_DepotReceivalCommences);
		}

		VoyageOrigin GetNewOrigin(JobVoyage voyage, ZString port, ZDateTime eTD)
		{
			var result = voyage.Origins.AddNew();
			result.JA_RL_NKPortOfLoading = port;
			result.JA_E_DEP = eTD;
			return result;
		}

		VoyageDestination GetNewDestination(JobVoyage voyage, ZString port, ZDateTime eTA)
		{
			var result = voyage.Destinations.AddNew();
			result.JB_RL_NKPortOfDischarge = port;
			result.JB_E_ARV = eTA;
			return result;
		}

		public void TestNewBusinessObject()
		{
			AssertExceptionThrown(typeof(NotSupportedException), () =>
				{
					MethodInfo newBusinessObjectMethod = Adapter.GetType().GetMethod("NewBusinessObject", BindingFlags.NonPublic | BindingFlags.Instance);
					ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, Notifications);

					JobSailing sailing = (JobSailing)newBusinessObjectMethod.Invoke(Adapter, new object[] { new Xsd.SailingBase(), importContext });
				});
		}

		public void TestCollectionSchemaNull()
		{
			try
			{
				object notUsed = Adapter.CollectionSchema;
				Fail("Collection schema not supported as we don't serialise collections");
			}
			catch (NotSupportedException)
			{
				Assert(true);
			}
		}

		public void TestImport_IsPublished_IfNewPortPairBeingCreated()
		{
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, Notifications);
			JobSailing sailing = Factory.NewWithValidTestData<JobSailing>();
			Xsd.Sailing sailingValue = new Xsd.Sailing();

			sailingValue.IsPublished = true;
			sailingValue.IsPublishedSpecified = true;
			Adapter.ImportFromValueObject(sailing, sailingValue, importContext);
			AssertEquals("IsPublished=true", true, sailing.JX_IsPublished);

			sailingValue.IsPublished = false;
			sailingValue.IsPublishedSpecified = true;
			Adapter.ImportFromValueObject(sailing, sailingValue, importContext);
			AssertEquals("IsPublished=false", false, sailing.JX_IsPublished);

			Factory.Save();

			sailingValue.IsPublished = true;
			sailingValue.IsPublishedSpecified = true;
			Adapter.ImportFromValueObject(sailing, sailingValue, importContext);
			AssertEquals("JX_IsPublished should not update after the port pair has been saved", false, sailing.JX_IsPublished);
		}

		#region Overrides for base test

		protected override ValueObjectDataAdapter<JobSailing, Xsd.SailingBase> GetNewBizObjXmlDataAdapter()
		{
			return new SailingValueObjectDataAdapter(TransportMode, "AUSYD", "AUMEL", "ves", "voy", ZGuid.Empty);
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return null; }
		}

		protected override string ExpectedRootElementName
		{
			get { return (TransportMode == Core.Constants.TransportModes.Sea) ? "Sailing" : "RoadRailFlight"; }
		}

		protected override bool IsExportToCollectionSupported
		{
			get { return false; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		readonly string BaseTestFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Freight\Shared\Freight.DataTransfer\Freight\Testing\";

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var emptySailing = CreateNewEmptySailing();
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing." + TransportMode + "EmptySailing.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptySailing, expectedOutputFilename, ValidationKind.None, "Empty Sailing");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var populatedSailing = CreateNewPopulatedSailing();
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing." + TransportMode + "Sailing.xml");

			BusinessObjectAndExpectedOutputFileName result;
			if (TransportMode == Core.Constants.TransportModes.Sea)
			{
				result = new BusinessObjectAndExpectedOutputFileName(populatedSailing, new Xsd.Sailing(), expectedOutputFilename, ValidationKind.Xsd | ValidationKind.FactorySave, "Populated Sailing Sea");
			}
			else
			{
				result = new BusinessObjectAndExpectedOutputFileName(populatedSailing, new Xsd.Flight(), expectedOutputFilename, ValidationKind.Xsd | ValidationKind.FactorySave, "Populated Sailing Air");
			}
			return result;
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
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

		JobSailing CreateNewEmptySailing()
		{
			var vessel = RefVessel.LookupVesselByName("vesselname", Factory).First();

			JobVoyage emptyVoyage = Factory.New<JobVoyage>();
			emptyVoyage.JV_RV_NKVessel = vessel.RV_FK;
			emptyVoyage.JV_VoyageFlight = "voyage";
			JobSailing emptySailing = emptyVoyage.Sailings.AddNew();
			return emptySailing;
		}

		JobSailing CreateNewPopulatedSailing()
		{
			JobVoyage populatedVoyage = Factory.NewWithValidTestData<JobVoyage>(TestBusinessObjectKind.All);
			JobSailing sailing = populatedVoyage.Sailings.AddNew();
			populatedVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			sailing.JX_JA = populatedVoyage.Origins[0].PK;
			sailing.JX_JB = populatedVoyage.Destinations[0].PK;

			TestBusinessObjectKind allExceptDependentsTestKind = TestBusinessObjectKind.All & ~TestBusinessObjectKind.PopulateDependentCollections;
			sailing.FillWithValidTestData(allExceptDependentsTestKind, Array.Empty<PropertyDescriptor>());
			sailing.Destination.JB_IsTranshipment = true;

			sailing.Destination.ArrivalCTOAddress.Header.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(sailing.Destination.ArrivalCTOAddress.Header);
			sailing.Origin.DepartureCTOAddress.Header.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(sailing.Origin.DepartureCTOAddress.Header);
			return sailing;
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					// covered in other data adapters
					"Consols",
					"IsPublished",
					"LoadPort/DepartureCTO",
					"DischargePort/ArrivalCTO",
					"DepartureCTO/Organisation",
					"ArrivalCTO/Organisation"
				};
			}
		}

		protected override JobSailing NewBusinessObject()
		{
			JobSailing result = Factory.New<JobSailing>();
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			VoyageDestination destination = voyage.Destinations.AddNew();
			result.JX_JA = origin.PK;
			result.JX_JB = destination.PK;
			return result;
		}

		[ExpectNoExceptions]
		public void TestDateTimeNotSpecifiedConvertsToZDateTimeEmptyImportFromValueObject()
		{
			JobSailing sailing = NewBusinessObject();
			SailingValueObjectDataAdapter adapter = (SailingValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(sailing, new Xsd.SailingBase(), context);
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			SystemDataRegistry.Instance.AllowDepartureCTOAddressImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "vesselname";
		}

		protected abstract string TransportMode { get; }

		#endregion

		#region Test correct registry defaults used for testing

		public void TestValueOfRegistryDefaultForImporting()
		{
			try
			{
				SystemRegistry.UpdateSailingSchedulesDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				SailingValueObjectDataAdapterForTest adapter = new SailingValueObjectDataAdapterForTest("a", "b", "c", "d", "d", ZGuid.Empty);
				AssertEquals("Adapter should be using shipment registry item which is currently true", true, adapter.ValueOfRegistryDefaultForImporting());

				SystemRegistry.UpdateSailingSchedulesDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertEquals("Adapter should be using shipment registry item which is currently true", false, adapter.ValueOfRegistryDefaultForImporting());
			}
			finally
			{
				((IRegistryItemInternals)SystemRegistry.UpdateSailingSchedulesDuringAutomaticImport).ClearCache();
			}
		}

		SystemDataRegistry SystemRegistry
		{
			get { return SystemDataRegistry.Instance; }
		}

		class SailingValueObjectDataAdapterForTest : SailingValueObjectDataAdapter
		{
			public SailingValueObjectDataAdapterForTest(ZString transportMode, ZString loadPort, ZString dischargePort, ZString vesselName, ZString voyageNo, ZGuid carrierPK)
				: base(transportMode, loadPort, dischargePort, vesselName, voyageNo, carrierPK)
			{
			}

			public bool ValueOfRegistryDefaultForImporting()
			{
				return base.RegistryDefaultForImporting;
			}
		}
		#endregion

		#region Implementation

		readonly NotificationBuffer Notifications = new NotificationBuffer();

		SailingValueObjectDataAdapter Adapter
		{
			get
			{
				if (fAdapter == null)
				{
					fAdapter = (SailingValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
				}
				return fAdapter;
			}
		}
		SailingValueObjectDataAdapter fAdapter;

		#endregion
	}
}
