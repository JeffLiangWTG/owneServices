using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class ScheduleTransportLegDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestPopulateBusinessObject_Air()
		{
			Globals.SetIsUserInteractiveForTest(false);
			var voyage = Factory.New<JobVoyage>();
			var logger = new TestErrorLogger();
			var transportLegDataObject = SetupTransportLeg(TransportMode.Air, logger);

			Factory.SaveForTesting();

			var reader = new ScheduleTransportLegDataObjectReader(transportLegDataObject, logger, Factory, voyage);
			var sailingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(sailingBO);
			AssertEquals("sailingBO.JX_JV_AircraftType", "E90", sailingBO.JX_JV_AircraftType);
			AssertEquals("sailingBO.Voyage.JV_IsCargoOnly", true, sailingBO.Voyage.JV_IsCargoOnly);
			AssertEquals("sailingBO.JX_TransportMode", "AIR", sailingBO.JX_TransportMode);

			AssertGeneralContents(sailingBO);
			AssertDepartureContents(sailingBO);
			AssertArrivalContents(sailingBO);
		}

		public void TestPopulateBusinessObject_Sea()
		{
			var voyage = Factory.New<JobVoyage>();
			var logger = new TestErrorLogger();
			var transportLegDataObject = SetupTransportLeg(TransportMode.Sea, logger);

			Factory.SaveForTesting();

			var reader = new ScheduleTransportLegDataObjectReader(transportLegDataObject, logger, Factory, voyage);
			var sailingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(sailingBO);
			AssertEquals("sailingBO.JX_JV_AircraftType", "", sailingBO.JX_JV_AircraftType);
			AssertEquals("sailingBO.Voyage.JV_IsCargoOnly", Registry.Business.FreightDataRegistry.Instance.CargoOnlyVoyageDefault.Value, sailingBO.Voyage.JV_IsCargoOnly);
			AssertEquals("sailingBO.JX_TransportMode", "SEA", sailingBO.JX_TransportMode);

			AssertGeneralContents(sailingBO);
			AssertDepartureContents(sailingBO);
			AssertArrivalContents(sailingBO);
		}

		public void Test_NewSailing_NoPortsOfLoading()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew("AUSYD");
			var logger = new TestErrorLogger();
			var transportLegDataObject = SetupTransportLeg(TransportMode.Sea, logger);
			transportLegDataObject.PortOfLoading = null;

			Factory.SaveForTesting();

			var reader = new ScheduleTransportLegDataObjectReader(transportLegDataObject, logger, Factory, voyage);

			AssertExceptionThrown<DataObjectReadFailureException>("Transport leg is missing Port Of Loading or Port Of Discharge.", () => reader.ReadIntoBusinessObject());
		}

		public void Test_NewSailing_NoPortsOfDischarge()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Destinations.AddNew("AUSYD");
			var logger = new TestErrorLogger();
			var transportLegDataObject = SetupTransportLeg(TransportMode.Sea, logger);
			transportLegDataObject.PortOfDischarge = null;

			Factory.SaveForTesting();

			var reader = new ScheduleTransportLegDataObjectReader(transportLegDataObject, logger, Factory, voyage);

			AssertExceptionThrown<DataObjectReadFailureException>("Transport leg is missing Port Of Loading or Port Of Discharge.", () => reader.ReadIntoBusinessObject());
		}

		public void Test_ExistingSailing_NoUpdatesOnPortsOfDischarge()
		{
			var logger = new TestErrorLogger();
			var transportLegDataObject = SetupTransportLeg(TransportMode.Sea, logger);
			var originalPortOfDischarge = transportLegDataObject.PortOfDischarge.Code.Value;
			var originalActualArrivalTime = new ZDateTime(1900, 1, 1);

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = transportLegDataObject.PortOfLoading.Code.Value;
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = originalPortOfDischarge;
			destination.JB_A_ARV = originalActualArrivalTime;
			voyage.GenerateSailings();
			AssertEquals(1, voyage.Sailings.Count);

			Factory.SaveForTesting();

			transportLegDataObject.PortOfDischarge = null;
			transportLegDataObject.ActualArrival = new ZDate(2017, 1, 1);

			var reader = new ScheduleTransportLegDataObjectReader(transportLegDataObject, logger, Factory, voyage);

			AssertExceptionThrown<DataObjectReadFailureException>("Transport leg is missing Port Of Loading or Port Of Discharge.", () => reader.ReadIntoBusinessObject());

			voyage = Factory.Load<JobVoyage>(voyage.PK);
			AssertEquals("No extra sailings get generated", 1, voyage.Sailings.Count);
			var sailingBO = voyage.Sailings[0];
			AssertEquals("Port Of Discharge should not get updated", sailingBO.Destination.PortOfDischarge.Code, originalPortOfDischarge);
			AssertEquals("Actual Arrival Time should not get updated", sailingBO.Destination.JB_A_ARV, originalActualArrivalTime);
		}

		public void Test_NewSailing_SameLoadingPortAndDischargePort()
		{
			var sydney = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew("AUSYD");
			voyage.Destinations.AddNew("FRPAR");
			Factory.SaveForTesting();
			TestCase(TransportMode.Sea, true);
			TestCase(TransportMode.Air, true);
			TestCase(TransportMode.Rail, false);
			TestCase(TransportMode.InlandWaterway, false);
			TestCase(TransportMode.Road, false);
			
			void TestCase(TransportMode mode, bool shouldHaveException)
			{
				var logger = new TestErrorLogger();
				var transportLegDataObject = SetupTransportLeg(mode, logger);
				transportLegDataObject.PortOfLoading = sydney;
				transportLegDataObject.PortOfDischarge = sydney;

				var reader = new ScheduleTransportLegDataObjectReader(transportLegDataObject, logger, Factory, voyage);

				if (shouldHaveException)
				{
					AssertExceptionThrown<DataObjectReadFailureException>("Transport leg cannot have the same Loading & Discharge port when the mode is not ROA or RAI.", () => reader.ReadIntoBusinessObject());
				}
				else
				{
					AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
				}
			}
		}

		public void Test_ExistingSailing_SameLoadingPortAndDischargePort()
		{
			var sydney = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew("AUSYD");
			voyage.Destinations.AddNew("FRPAR");
			voyage.GenerateSailings();
			AssertEquals(1, voyage.Sailings.Count);
			Factory.SaveForTesting();

			TestCase(TransportMode.Sea, true);
			TestCase(TransportMode.Air, true);
			TestCase(TransportMode.Rail, false);
			TestCase(TransportMode.InlandWaterway, false);
			TestCase(TransportMode.Road, false);

			void TestCase(TransportMode mode, bool shouldHaveException)
			{
				var logger = new TestErrorLogger();
				var transportLegDataObject = SetupTransportLeg(mode, logger);
				transportLegDataObject.PortOfLoading = sydney;
				transportLegDataObject.PortOfDischarge = sydney;

				var reader = new ScheduleTransportLegDataObjectReader(transportLegDataObject, logger, Factory, voyage);

				if (shouldHaveException)
				{
					AssertExceptionThrown<DataObjectReadFailureException>("Transport leg cannot have the same Loading & Discharge port when the mode is not ROA or RAI.", () => reader.ReadIntoBusinessObject());
					voyage = Factory.Load<JobVoyage>(voyage.PK);
					AssertEquals("No extra sailings get generated", 1, voyage.Sailings.Count);
					AssertEquals("Port Of Loading should not get updated", voyage.Sailings[0].Origin.PortOfLoading.Code, "AUSYD");
					AssertEquals("Port Of Discharge should not get updated", voyage.Sailings[0].Destination.PortOfDischarge.Code, "FRPAR");
				}
				else
				{
					AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
				}
			}
		}

		#region Population When EstimatedDeparture Greater Than EstimatedArrival

		public void TestPopulateBusinessObject_SeaMode_WhenEstimatedDepartureGreaterThanEstimatedArrival()
		{
			AssertPopulateBusinessObject_WhenEstimatedDepartureGreaterThanEstimatedArrival(TransportMode.Sea, new ZDateTime(2017, 3, 7), true);
		}

		public void TestPopulateBusinessObject_RailMode_WhenEstimatedDepartureGreaterThanEstimatedArrival()
		{
			AssertPopulateBusinessObject_WhenEstimatedDepartureGreaterThanEstimatedArrival(TransportMode.Rail, new ZDateTime(2017, 3, 7), true);
		}

		public void TestPopulateBusinessObject_RoadMode_WhenEstimatedDepartureGreaterThanEstimatedArrival()
		{
			AssertPopulateBusinessObject_WhenEstimatedDepartureGreaterThanEstimatedArrival(TransportMode.Road, new ZDateTime(2017, 3, 7), true);
		}

		public void TestPopulateBusinessObject_AirMode_WhenEstimatedDepartureGreaterThanEstimatedArrival_LessOrEqualToOneDay()
		{
			AssertPopulateBusinessObject_WhenEstimatedDepartureGreaterThanEstimatedArrival(TransportMode.Air, new ZDateTime(2017, 3, 7), false);
		}

		public void TestPopulateBusinessObject_AirMode_WhenEstimatedDepartureGreaterThanEstimatedArrival_GreaterThanOneDay()
		{
			AssertPopulateBusinessObject_WhenEstimatedDepartureGreaterThanEstimatedArrival(TransportMode.Air, new ZDateTime(2017, 3, 8), true);
		}

		void AssertPopulateBusinessObject_WhenEstimatedDepartureGreaterThanEstimatedArrival(TransportMode mode, ZDateTime estimatedDeparture, bool expectExceptionThrown)
		{
			Globals.SetIsUserInteractiveForTest(false);

			var voyage = Factory.New<JobVoyage>();
			var logger = new TestErrorLogger();
			var transportLegDataObject = SetupTransportLeg(mode, logger);

			transportLegDataObject.EstimatedDeparture = estimatedDeparture;
			transportLegDataObject.EstimatedArrival = new ZDateTime(2017, 3, 6);
			Assert("Prerequisite: EstimatedDeparture must be greater than EstimatedArrival", transportLegDataObject.EstimatedDeparture > transportLegDataObject.EstimatedArrival);

			Factory.SaveForTesting();

			var reader = new ScheduleTransportLegDataObjectReader(transportLegDataObject, logger, Factory, voyage);
			if (expectExceptionThrown)
			{
				AssertExceptionThrown<DataObjectReadFailureException>(() => reader.ReadIntoBusinessObject());
			}
			else
			{
				AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
			}

			voyage.JV_AirSeaRoad = new TransportModeConverter().FromEnumValue(mode);
			transportLegDataObject.TransportMode = null;

			reader = new ScheduleTransportLegDataObjectReader(transportLegDataObject, logger, Factory, voyage);
			if (expectExceptionThrown)
			{
				AssertExceptionThrown<DataObjectReadFailureException>(() => reader.ReadIntoBusinessObject());
			}
			else
			{
				AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
			}
		}

		#endregion

		public void TestPopulateBusinessObject_WhenEstimatedDepartureNotSpecified()
		{
			TestPopulateBusinessObject_IsInvalidDateCombinationForSailing(new ZDateTime(2024, 1, 1), new ZDateTime(2024, 2, 1), null, new ZDateTime(2023, 12, 1));
		}

		public void TestPopulateBusinessObject_WhenEstimatedArrivalNotSpecified()
		{
			TestPopulateBusinessObject_IsInvalidDateCombinationForSailing(new ZDateTime(2024, 1, 1), new ZDateTime(2024, 2, 1), new ZDateTime(2024, 3, 1), null);
		}

		void TestPopulateBusinessObject_IsInvalidDateCombinationForSailing(ZDateTime originEstimatedDeparture, ZDateTime destinationEstimatedArrival, ZDateTime? estimatedDeparture, ZDateTime? estimatedArrival)
		{
			Globals.SetIsUserInteractiveForTest(false);

			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NZAKL";
			origin.JA_E_DEP = originEstimatedDeparture;
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUMEL";
			destination.JB_E_ARV = destinationEstimatedArrival;

			var logger = new TestErrorLogger();
			var transportLegDataObject = SetupTransportLeg(TransportMode.Sea, logger);
			transportLegDataObject.EstimatedDeparture = estimatedDeparture;
			transportLegDataObject.EstimatedArrival = estimatedArrival;
			var reader = new ScheduleTransportLegDataObjectReader(transportLegDataObject, logger, Factory, voyage);
			AssertExceptionThrown<DataObjectReadFailureException>(() => reader.ReadIntoBusinessObject());
		}

		#region Implementation

		TransportLeg SetupTransportLeg(TransportMode mode, IXmlImportLogger logger)
		{
			var transportLegDataObject = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);

			transportLegDataObject.LegOrder = 1;
			transportLegDataObject.TransportMode = mode;
			transportLegDataObject.ActualArrival = new ZDateTime(2017, 3, 7);
			transportLegDataObject.ActualDeparture = new ZDateTime(2017, 3, 5);
			transportLegDataObject.EstimatedArrival = new ZDateTime(2017, 3, 6);
			transportLegDataObject.EstimatedDeparture = new ZDateTime(2017, 3, 4);
			transportLegDataObject.ScheduledDeparture = new ZDateTime(2017, 3, 1);
			transportLegDataObject.ScheduledArrivalInPortOfLoading = new ZDateTime(2017, 3, 8);
			transportLegDataObject.ScheduledArrival = new ZDateTime(2017, 3, 9);
			transportLegDataObject.LegType = LegType.Main;
			transportLegDataObject.VoyageFlightNo = "RL33";
			transportLegDataObject.AircraftType = new CodeDescriptionPair() { Code = "E90" };
			transportLegDataObject.Carrier = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD("Fooey");
			transportLegDataObject.IsCargoOnly = true;

			var carrierAddressForSetup = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD("Fooey");
			var carrierReaderForSetup = new OrganisationDataObjectReader(carrierAddressForSetup, logger, Factory);
			var carrierAddressBoToLoad = carrierReaderForSetup.GetMatchedOrNewForTesting();

			transportLegDataObject.PortOfLoading = new UNLOCO() { Code = "NZAKL", Name = "Auckland" };
			transportLegDataObject.FCLReceivalCommences = new ZDateTime(2017, 3, 1);
			transportLegDataObject.FCLCutOff = new ZDateTime(2017, 3, 3);
			transportLegDataObject.DepartureCTO = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.DepartureCTOAddress);
			transportLegDataObject.DepartureBerth = "DEPBER";
			transportLegDataObject.DepartureReference = "DEPREF";
			transportLegDataObject.EstimatedArrivalInPortOfLoading = new ZDateTime(2017, 2, 27);
			transportLegDataObject.DocumentCutOff = new ZDateTime(2017, 2, 28);
			transportLegDataObject.HazzardReceivalCommences = new ZDateTime(2017, 3, 4);
			transportLegDataObject.HazzardCutOffDate = new ZDateTime(2017, 3, 5);
			transportLegDataObject.VGMCutOff = new ZDateTime(2017, 3, 1);

			var departureCtoAddressForSetup = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD("ctoSyd");
			var departureCtoReaderForSetup = new OrganisationDataObjectReader(departureCtoAddressForSetup, logger, Factory);
			var departureCtoAddressBoToLoad = departureCtoReaderForSetup.GetMatchedOrNewForTesting();

			transportLegDataObject.PortOfDischarge = new UNLOCO() { Code = "AUMEL", Name = "Melbourne" };
			transportLegDataObject.ArrivalCTO = OrganizationAddressTestHelper.GetNewAddressData_WUFSHIJNB(DocAddressType.ArrivalCTOAddress);
			transportLegDataObject.FCLAvailability = new ZDateTime(2017, 3, 8);
			transportLegDataObject.FCLStorage = new ZDateTime(2017, 3, 9);
			transportLegDataObject.ArrivalBerth = "ARVBER";
			transportLegDataObject.ArrivalReference = "ARVREF";

			transportLegDataObject.LCLReceivalCommences = new ZDateTime(2017, 2, 28);
			transportLegDataObject.LCLCutOff = new ZDateTime(2017, 3, 2);
			transportLegDataObject.LCLAvailability = new ZDateTime(2017, 3, 7);
			transportLegDataObject.LCLStorageDate = new ZDateTime(2017, 3, 8);

			transportLegDataObject.EmptyReceivalCommences = new ZDateTime(2017, 3, 10);
			transportLegDataObject.EmptyCutOff = new ZDateTime(2017, 3, 11);
			transportLegDataObject.ReeferReceivalCommences = new ZDateTime(2017, 3, 12);
			transportLegDataObject.ReeferCutOff = new ZDateTime(2017, 3, 13);

			var arrivalCtoAddressForSetup = OrganizationAddressTestHelper.GetNewAddressData_WUFSHIJNB("ctoJnb");
			var arrivalCtoReaderForSetup = new OrganisationDataObjectReader(arrivalCtoAddressForSetup, logger, Factory);
			var arrivalCtoAddressBoToLoad = arrivalCtoReaderForSetup.GetMatchedOrNewForTesting();

			return transportLegDataObject;
		}

		void AssertGeneralContents(JobSailing sailingBO)
		{
			AssertEquals("sailingBO.Line.OH_Code", "INTHEMSYD", sailingBO.Line.OH_Code);
			AssertEquals("sailingBO.JX_JV_VoyageFlight", "RL33", sailingBO.JX_JV_VoyageFlight);
		}

		void AssertDepartureContents(JobSailing sailingBO)
		{
			AssertEquals("sailingBO.JX_JA_RL_NKPortOfLoading", "NZAKL", sailingBO.JX_JA_RL_NKPortOfLoading);
			AssertEquals("sailingBO.JX_JA_E_DEP", new ZDateTime(2017, 3, 4), sailingBO.JX_JA_E_DEP);
			AssertEquals("sailingBO.JX_JA_A_DEP", new ZDateTime(2017, 3, 5), sailingBO.JX_JA_A_DEP);
			AssertEquals("sailingBO.JX_JA_CTOReceivalCommences", new ZDateTime(2017, 3, 1), sailingBO.JX_JA_CTOReceivalCommences);
			AssertEquals("sailingBO.JX_JA_CTOCutOff", new ZDateTime(2017, 3, 3), sailingBO.JX_JA_CTOCutOff);
			AssertEquals("sailingBO.Origin.DepartureCTOAddress.Header.OH_Code", "CRAHOLSYD", sailingBO.Origin.DepartureCTOAddress.Header.OH_Code);
			AssertEquals("sailingBO.JX_JA_DepartureBerth", "DEPBER", sailingBO.JX_JA_DepartureBerth);
			AssertEquals("sailingBO.JX_JA_DepartureReference", "DEPREF", sailingBO.JX_JA_DepartureReference);
			AssertEquals("sailingBO.Origin.JA_E_ARV", new ZDateTime(2017, 2, 27), sailingBO.Origin.JA_E_ARV);
			AssertEquals("sailingBO.Origin.JA_DocumentaryCutoff", new ZDateTime(2017, 2, 28), sailingBO.Origin.JA_DocumentaryCutoff);
			AssertEquals("sailingBO.Origin.JA_DGReceivalCommences", new ZDateTime(2017, 3, 4), sailingBO.Origin.JA_DGReceivalCommences);
			AssertEquals("sailingBO.Origin.JA_DGCutOff", new ZDateTime(2017, 3, 5), sailingBO.Origin.JA_DGCutOff);
			AssertEquals("sailingBO.Origin.JA_VGMCutOff", new ZDateTime(2017, 3, 1), sailingBO.Origin.JA_VGMCutOff);
			AssertEquals("sailingBO.JX_JA_EmptyReceivalCommences", new ZDateTime(2017, 3, 10), sailingBO.JX_JA_EmptyReceivalCommences);
			AssertEquals("sailingBO.JX_JA_EmptyCutOff", new ZDateTime(2017, 3, 11), sailingBO.JX_JA_EmptyCutOff);
			AssertEquals("sailingBO.JX_JA_ReeferReceivalCommences", new ZDateTime(2017, 3, 12), sailingBO.JX_JA_ReeferReceivalCommences);
			AssertEquals("sailingBO.JX_JA_ReeferCutOff", new ZDateTime(2017, 3, 13), sailingBO.JX_JA_ReeferCutOff);
			AssertEquals("sailingBO.JX_JA_S_DEP", new ZDateTime(2017, 3, 1), sailingBO.JX_JA_S_DEP);
			AssertEquals("sailingBO.JX_JA_S_ARV", new ZDateTime(2017, 3, 8), sailingBO.JX_JA_S_ARV);
		}

		void AssertArrivalContents(JobSailing sailingBO)
		{
			AssertEquals("sailingBO.JX_JB_RL_NKPortOfDischarge", "AUMEL", sailingBO.JX_JB_RL_NKPortOfDischarge);
			AssertEquals("sailingBO.JX_JB_E_ARV", new ZDateTime(2017, 3, 6), sailingBO.JX_JB_E_ARV);
			AssertEquals("sailingBO.JX_JB_A_ARV", new ZDateTime(2017, 3, 7), sailingBO.JX_JB_A_ARV);
			AssertEquals("sailingBO.Destination.ArrivalCTOAddress.Header.OH_Code", "WUFSHIJNB", sailingBO.Destination.ArrivalCTOAddress.Header.OH_Code);
			AssertEquals("sailingBO.JX_JB_CTOAvailabilityDate", new ZDateTime(2017, 3, 8), sailingBO.JX_JB_CTOAvailabilityDate);
			AssertEquals("sailingBO.JX_JB_CTOStorageDate", new ZDateTime(2017, 3, 9), sailingBO.JX_JB_CTOStorageDate);
			AssertEquals("sailingBO.JX_JB_ArrivalBerth", "ARVBER", sailingBO.JX_JB_ArrivalBerth);
			AssertEquals("sailingBO.JX_JB_ArrivalReference", "ARVREF", sailingBO.JX_JB_ArrivalReference);

			AssertEquals("sailingBO.JX_DepotReceivalCommences", new ZDateTime(2017, 2, 28), sailingBO.JX_DepotReceivalCommences);
			AssertEquals("sailingBO.JX_DepotCutOff", new ZDateTime(2017, 3, 2), sailingBO.JX_DepotCutOff);
			AssertEquals("sailingBO.JX_DepotAvailabilityDate", new ZDateTime(2017, 3, 7), sailingBO.JX_DepotAvailabilityDate);
			AssertEquals("sailingBO.JX_DepotStorageDate", new ZDateTime(2017, 3, 8), sailingBO.JX_DepotStorageDate);
			AssertEquals("sailingBO.JX_JB_S_ARV", new ZDateTime(2017, 3, 9), sailingBO.JX_JB_S_ARV);
		}

		#endregion
	}
}
