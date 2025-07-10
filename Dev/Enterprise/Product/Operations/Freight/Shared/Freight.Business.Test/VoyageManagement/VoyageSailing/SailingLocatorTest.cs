using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class SailingLocatorTest : BaseFreightTest
	{
		[ExpectNoExceptions]
		public void TestDontBlowUpWhenLongValuesAreEntered()
		{
			ZString longLoadPortName = "BlahBlahBlah";
			ZString longDischargePortName = "OggaBooga";
			ZString longVesselName = "RealyLongVesselName-RealyLongVesselName-RealyLongVesselName-RealyLongVesselName";
			ZString longVoyageNumber = "saonteuhaosentuhasoenhaosn";

			JobSailing createdSailing = new SailingLocator(Factory).FindOrCreateSailingFromSailingManager(
				Constants.TransportModes.Sea,
				longLoadPortName,
				longDischargePortName,
				longVesselName,
				longVoyageNumber,
				ZGuid.Empty,
				ZDateTime.Now,
				ZDateTime.Now)
				.Sailing;

			Factory.Save();
		}

		public void TestFind()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABC";

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "VOYG";
			transport.JW_ETD = ZDateTime.Now;
			transport.JW_ETA = ZDateTime.Now;

			var foundSailing = new SailingLocator(Factory).FindSailingFromSailingManager(
				Constants.TransportModes.Sea,
				transport.JW_RL_NKLoadPort,
				transport.JW_RL_NKDiscPort,
				transport.JW_Vessel,
				transport.JW_VoyageFlight,
				ZGuid.Empty,
				transport.JW_ETD,
				transport.JW_ETA)
				.Sailing;

			AssertEquals("Should find an existing sailing", foundSailing.PK, consol.Schedule.PK);

			transport.JW_OA_CarrierAddress = org.MainAddress.PK;

			var foundSailingWithLineOperator = new SailingLocator(Factory).FindSailingFromSailingManager(
				Constants.TransportModes.Sea,
				transport.JW_RL_NKLoadPort,
				transport.JW_RL_NKDiscPort,
				transport.JW_Vessel,
				transport.JW_VoyageFlight,
				org.PK,
				transport.JW_ETD,
				transport.JW_ETA)
				.Sailing;

			AssertEquals("Should find an existing sailing", foundSailingWithLineOperator.PK, consol.Schedule.PK);

			var notFoundSailing = new SailingLocator(Factory).FindSailingFromSailingManager(
				Core.Constants.TransportModes.Sea,
				"AUBNE",
				"HKHKG",
				TestVessel1.RV_Name,
				"TEST",
				ZGuid.NewZGuid(),
				ZDateTime.Now,
				ZDateTime.Now)
				.Sailing;

			AssertNull("Should return null because it can't find anything that matches", notFoundSailing);
		}

		public void TestFindWithSTDAndSTA()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABC";

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "VOYG";
			transport.JW_ETD = ZDateTime.Now;
			transport.JW_ETA = ZDateTime.Now;
			transport.JW_STD = ZDateTime.Now;
			transport.JW_STA = ZDateTime.Now;

			var foundSailing = new SailingLocator(Factory).FindSailingFromSailingManager(
				Constants.TransportModes.Sea,
				transport.JW_RL_NKLoadPort,
				transport.JW_RL_NKDiscPort,
				transport.JW_Vessel,
				transport.JW_VoyageFlight,
				ZGuid.Empty,
				transport.JW_ETD,
				transport.JW_ETA,
				transport.JW_STD,
				transport.JW_STA)
				.Sailing;

			AssertEquals("Should find an existing sailing", foundSailing.PK, consol.Schedule.PK);
		}

		public void TestFindLinkedArchivedSchedule()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABC";

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "VOYG";
			transport.JW_ETD = ZDateTime.Now;
			transport.JW_ETA = ZDateTime.Now;

			var foundSailing = new SailingLocator(Factory).FindSailingFromSailingManager(
				Constants.TransportModes.Sea,
				transport.JW_RL_NKLoadPort,
				transport.JW_RL_NKDiscPort,
				transport.JW_Vessel,
				transport.JW_VoyageFlight,
				ZGuid.Empty,
				transport.JW_ETD,
				transport.JW_ETA,
				false)
				.Sailing;
			AssertNotNull("An sailing should exist if ignoring active filter", foundSailing);
			AssertEquals("Should match consol's schedule", foundSailing.PK, consol.Schedule.PK);
			AssertEquals("Transport should be linked to a sailing", true, transport.JW_IsLinked);
			AssertEquals("The existed sailing should not be archived", false, foundSailing.Voyage.IsArchived);

			transport.Sailing.Voyage.JV_IsActive = false;
			foundSailing = new SailingLocator(Factory).FindSailingFromSailingManager(
				Constants.TransportModes.Sea,
				transport.JW_RL_NKLoadPort,
				transport.JW_RL_NKDiscPort,
				transport.JW_Vessel,
				transport.JW_VoyageFlight,
				ZGuid.Empty,
				transport.JW_ETD,
				transport.JW_ETA)
				.Sailing;
			AssertNull("No active sailing found if ignoring active filter", foundSailing);

			foundSailing = new SailingLocator(Factory).FindSailingFromSailingManager(
				Constants.TransportModes.Sea,
				transport.JW_RL_NKLoadPort,
				transport.JW_RL_NKDiscPort,
				transport.JW_Vessel,
				transport.JW_VoyageFlight,
				ZGuid.Empty,
				transport.JW_ETD,
				transport.JW_ETA,
				true)
				.Sailing;
			AssertNotNull("A sailing should be found if not ignoring active filter", foundSailing);
			AssertEquals("Should match consol's schedule", foundSailing.PK, consol.Schedule.PK);
			AssertEquals("The found sailing should be archived", true, foundSailing.Voyage.IsArchived);
		}

		public void TestFindOrCreateFromSailingManager()
		{
			JobSailing createdSailing = new SailingLocator(Factory).FindOrCreateSailingFromSailingManager(
				Constants.TransportModes.Sea,
				"LOAD",
				"DISC",
				"VESS",
				"VOYG",
				ZGuid.Empty,
				ZDateTime.Now,
				ZDateTime.Now)
				.Sailing;

			AssertNotNull("Should create a new sailing", createdSailing);
			AssertEquals("TransportMode should be set", Core.Constants.TransportModes.Sea, createdSailing.JX_TransportMode);
			AssertEquals("Load port should be set", "LOAD", createdSailing.Origin.JA_RL_NKPortOfLoading);
			AssertEquals("Discharge port should be set", "DISC", createdSailing.Destination.JB_RL_NKPortOfDischarge);
			AssertEquals("Vessel should be set", "VESS", createdSailing.JX_JV_NKVessel);
			AssertEquals("Voyage should be set", "VOYG", createdSailing.JX_JV_VoyageFlight);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "VOYG";
			transport.JW_ETD = ZDateTime.Now;
			transport.JW_ETA = ZDateTime.Now;

			JobSailing foundSailing = new SailingLocator(Factory).FindOrCreateSailingFromSailingManager(
				Constants.TransportModes.Sea,
				transport.JW_RL_NKLoadPort,
				transport.JW_RL_NKDiscPort,
				transport.JW_Vessel,
				transport.JW_VoyageFlight,
				ZGuid.Empty,
				transport.JW_ETD,
				transport.JW_ETA)
				.Sailing;

			AssertEquals("Should find an existing sailing", foundSailing.PK, transport.JW_JX);

			var factory2 = new BusinessObjectFactory();
			var createdSailing2 = new SailingLocator(factory2).FindOrCreateSailingFromSailingManager(
				Constants.TransportModes.Sea,
				"LOAD",
				"DISC",
				"VESS",
				"VOYG",
				ZGuid.Empty,
				ZDateTime.Now,
				ZDateTime.Now,
				ZDateTime.Now,
				ZDateTime.Now)
				.Sailing;

			AssertNotNull("Should create a new sailing", createdSailing2);
		}

		public void TestFindOrCreateFromSailingManager_MatchingAir()
		{
			var today = ZDateTime.Today;
			var flightSchedule = new SailingLocator(Factory).FindOrCreateSailingFromSailingManager(
				Constants.TransportModes.Air,
				"AUSYD",
				"CNSHA",
				ZString.Empty,
				"QF456",
				ZGuid.Empty,
				today,
				today.AddHours(8))
				.Sailing;

			Factory.Save();

			AssertNotNull("Should create a new sailing", flightSchedule);
			AssertEquals("Should be air transport", Constants.TransportModes.Air, flightSchedule.JX_TransportMode);
			AssertEquals("Vessel should be empty", ZString.Empty, flightSchedule.JX_JV_NKVessel);
			AssertEquals("Flight number should be set", "QF456", flightSchedule.JX_JV_VoyageFlight);

			AssertEquals("Load port should be set", "AUSYD", flightSchedule.Origin.JA_RL_NKPortOfLoading);
			AssertEquals("Load port depature time should be set", today, flightSchedule.Origin.JA_E_DEP);

			AssertEquals("Discharge port should be set", "CNSHA", flightSchedule.Destination.JB_RL_NKPortOfDischarge);
			AssertEquals("Discharge port arrival time should be set", today.AddHours(8), flightSchedule.Destination.JB_E_ARV);

			AssertAirMatch_NonDataImport(today, flightSchedule.PK);
			AssertAirMatch_DataImportWithRegistrySettings(today, flightSchedule.PK);
		}

		void AssertAirMatch_NonDataImport(ZDateTime today, ZGuid existingSailing)
		{
			var flightSchedule = new SailingLocator(Factory).FindOrCreateSailingFromSailingManager(
				Constants.TransportModes.Air,
				"AUSYD",
				"CNSHA",
				ZString.Empty,
				"QF456",
				ZGuid.Empty,
				today.AddHours(12),
				today.AddHours(20))
				.Sailing;

			AssertEquals("Should find an existing sailing", existingSailing, flightSchedule.PK);

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var transport = consol.Transports[0];
			transport.JW_JX = flightSchedule.PK;

			AssertEquals(flightSchedule.PK, transport.JW_JX);
			AssertEquals(today, transport.JW_ETD);
			AssertEquals(today.AddHours(8), transport.JW_ETA);
		}

		void AssertAirMatch_DataImportWithRegistrySettings(ZDateTime today, ZGuid existingSailing)
		{
			SystemDataRegistry.Instance.FlightScheduleUpdateThresholdForDataImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 6);

			var flightSchedule = new SailingLocator(Factory, true).FindOrCreateSailingFromSailingManager(
				Constants.TransportModes.Air,
				"AUSYD",
				"CNSHA",
				ZString.Empty,
				"QF456",
				ZGuid.Empty,
				today.AddHours(12),
				today.AddHours(20))
				.Sailing;

			AssertNotEquals("Should not match to the existing sailing", existingSailing, flightSchedule.PK);

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			((ISupportDataImporting)consol).IsImportingData = true;
			var transport = consol.Transports[0];
			transport.JW_JX = flightSchedule.PK;

			Factory.Save();

			AssertEquals("Expected not to replace the flight schedule with the existing schedule as the minimum difference in time is now 6 hours instead of 24",
				flightSchedule.PK, transport.JW_JX);
			AssertEquals(today.AddHours(12), transport.JW_ETD);
			AssertEquals(today.AddHours(20), transport.JW_ETA);
		}
	}
}
