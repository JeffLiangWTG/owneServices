using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ScheduleManagerTest : BaseFreightTest
	{
		public void TestDontEnsureUniqueScheduleIfToldToKeepDates()
		{
			var today = ZDateTime.Today.AddHours(10);

			CreateSchedule("QF1234", HomePort, today.AddHours(30), OverseasPort, ZDateTime.Empty);
			Factory.Save();

			MockISchedule managerParent = new MockISchedule(Factory);
			managerParent.TransportMode = Constants.TransportModes.Air;
			managerParent.Flight = "QF1234";
			managerParent.Load = HomePort;
			managerParent.Discharge = OverseasPort;
			managerParent.ETD = today.AddHours(53);

			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);

			mockQueryProvider.Setup(m => m.QueryFreshMatchBehaviour(It.IsAny<QueryFreshMatchBehaviourArgs>())).Returns(SailingManagerUpdateMode.NewSchedule);
			var manager = BaseSailingManager.New(managerParent);
			manager.Dirty = false;
			manager.DepartureDirty = true;
			manager.NotifyRead();

			JobSailing sailing = manager.Sailing;
			AssertNotNull("should have a sailing", sailing);
			AssertEquals("sailing should be new", false, sailing.IsInDatabase);

			manager.NotifySave();
			AssertEquals("should still have the same sailing", sailing, manager.Sailing);
			AssertEquals("sailing should not have been deleted", false, sailing.IsDeleted);
		}

		[ExpectNoExceptions]
		public void TestDontDieOnDomesticWhenNotAllDatesAreSet()
		{
			ZDateTime now = ZDateTime.Now;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "321456";

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = HomePort;
			origin.JA_E_DEP = now.AddDays(5);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = AlternateHomePort;
			destination.JB_E_ARV = now.AddDays(6);

			voyage.GenerateSailings();
			Factory.Save();

			MockISchedule managerParent = new MockISchedule(Factory);
			managerParent.TransportMode = Constants.TransportModes.Air;
			managerParent.Flight = voyage.JV_VoyageFlight;
			managerParent.Load = origin.JA_RL_NKPortOfLoading;
			managerParent.Discharge = destination.JB_RL_NKPortOfDischarge;

			BaseSailingManager manager = BaseSailingManager.New(managerParent);
			managerParent.ETA = ZDateTime.Empty;
			managerParent.ETD = ZDateTime.Empty;
			manager.Dirty = true;
			manager.NotifyRead();

			managerParent.ETD = ZDateTime.Empty;
			managerParent.ETA = destination.JB_E_ARV;
			manager.Dirty = true;
			manager.NotifyRead();

			managerParent.ETD = origin.JA_E_DEP;
			managerParent.ETA = ZDateTime.Empty;
			manager.Dirty = true;
			manager.NotifyRead();
		}

		#region Schedule Date Updates

		public void TestScheduleDateUpdates_ExactMatch_Import()
		{
			var today = ZDateTime.Today.AddHours(10);

			var sailingPK = CreateSchedule("QF1234", OverseasPort, today.AddHours(24), HomePort, today.AddHours(30));
			Factory.Save();

			var managerParent = new MockISchedule(Factory)
			{
				TransportMode = Constants.TransportModes.Air,
				Flight = "QF1234",
				Load = OverseasPort,
				Discharge = HomePort
			};

			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);

			mockQueryProvider
				.Setup(m => m.QueryFreshMatchBehaviour(It.IsAny<QueryFreshMatchBehaviourArgs>()))
				.Returns(AssertArgsAndReturn("ETA", today.AddHours(30), today.AddHours(30),
					SailingManagerUpdateMode.ScheduleUnchanged));

			BaseSailingManager manager = BaseSailingManager.New(managerParent);
			manager.NotifyRead();

			managerParent.ETA = today.AddHours(30);
			manager.ArrivalDirty = true;
			manager.NotifyRead();

			AssertEquals("ETD should be unchanged", today.AddHours(24), manager.Sailing.Origin.JA_E_DEP);
			AssertEquals("ETA should be unchanged", today.AddHours(30), manager.Sailing.Destination.JB_E_ARV);

			manager.NotifySave();

			AssertEquals("ETD should still be unchanged", today.AddHours(24), manager.Sailing.Origin.JA_E_DEP);
			AssertEquals("ETA should still be unchanged", today.AddHours(30), manager.Sailing.Destination.JB_E_ARV);
		}

		public void TestScheduleDateUpdates_ExactMatch_Export()
		{
			var today = ZDateTime.Today.AddHours(10);

			var sailingPK = CreateSchedule("QF1234", HomePort, today.AddHours(24), OverseasPort, today.AddHours(30));
			Factory.Save();

			var managerParent = new MockISchedule(Factory)
			{
				TransportMode = Constants.TransportModes.Air,
				Flight = "QF1234",
				Load = HomePort,
				Discharge = OverseasPort
			};

			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);

			mockQueryProvider
				.Setup(m => m.QueryFreshMatchBehaviour(It.IsAny<QueryFreshMatchBehaviourArgs>()))
				.Returns(AssertArgsAndReturn("ETD", today.AddHours(24), today.AddHours(24), SailingManagerUpdateMode.ScheduleUnchanged));
			var manager = BaseSailingManager.New(managerParent);
			manager.NotifyRead();

			managerParent.ETD = today.AddHours(24);
			manager.DepartureDirty = true;
			manager.NotifyRead();

			AssertEquals("ETD should be unchanged", today.AddHours(24), manager.Sailing.Origin.JA_E_DEP);
			AssertEquals("ETA should be unchanged", today.AddHours(30), manager.Sailing.Destination.JB_E_ARV);

			manager.NotifySave();

			AssertEquals("ETD should still be unchanged", today.AddHours(24), manager.Sailing.Origin.JA_E_DEP);
			AssertEquals("ETA should still be unchanged", today.AddHours(30), manager.Sailing.Destination.JB_E_ARV);
		}

		public void TestScheduleDateUpdates_NearMatch_CurrentSchedule()
		{
			var today = ZDateTime.Today.AddHours(10);

			var sailingPK = CreateSchedule("QF1234", HomePort, today.AddHours(24), OverseasPort, today.AddHours(30));
			Factory.Save();

			MockISchedule managerParent = new MockISchedule(Factory);
			managerParent.TransportMode = Constants.TransportModes.Air;
			managerParent.SailingPK = sailingPK;
			managerParent.AllowScheduleDatesChanging = true;

			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>(MockBehavior.Strict);
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);

			var manager = BaseSailingManager.New(managerParent);
			manager.NotifyRead();

			managerParent.ETD = today.AddHours(25);
			manager.DepartureDirty = true;
			manager.NotifyRead();

			AssertEquals("ETD should be updated", today.AddHours(25), manager.Sailing.Origin.JA_E_DEP);
			AssertEquals("ETA should be unchanged", today.AddHours(30), manager.Sailing.Destination.JB_E_ARV);

			manager.NotifySave();

			AssertEquals("ETD should still be updated", today.AddHours(25), manager.Sailing.Origin.JA_E_DEP);
			AssertEquals("ETA should still be unchanged", today.AddHours(30), manager.Sailing.Destination.JB_E_ARV);
		}

		public void TestScheduleDateUpdates_NearMatch_DontUpdate()
		{
			var today = ZDateTime.Today.AddHours(10);

			var sailingPK = CreateSchedule("QF1234", HomePort, today.AddHours(24), OverseasPort, today.AddHours(30));
			Factory.Save();

			var managerParent = new MockISchedule(Factory)
			{
				TransportMode = Constants.TransportModes.Air,
				Flight = "QF1234",
				Load = HomePort,
				Discharge = OverseasPort
			};

			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);

			mockQueryProvider
				.Setup(m => m.QueryFreshMatchBehaviour(It.IsAny<QueryFreshMatchBehaviourArgs>()))
				.Returns(AssertArgsAndReturn("ETD", today.AddHours(25), today.AddHours(24), SailingManagerUpdateMode.ScheduleUnchanged));

			var manager = BaseSailingManager.New(managerParent);
			manager.NotifyRead();

			managerParent.ETD = today.AddHours(25);
			manager.DepartureDirty = true;
			manager.NotifyRead();

			AssertEquals("ETD should be unchanged", today.AddHours(24), manager.Sailing.Origin.JA_E_DEP);
			AssertEquals("ETA should be unchanged", today.AddHours(30), manager.Sailing.Destination.JB_E_ARV);

			manager.NotifySave();

			AssertEquals("ETD should still be unchanged", today.AddHours(24), manager.Sailing.Origin.JA_E_DEP);
			AssertEquals("ETA should still be unchanged", today.AddHours(30), manager.Sailing.Destination.JB_E_ARV);
		}

		public void TestScheduleDateUpdates_NearMatch_DontUpdate_TimeDifferenceOutsideTolerance()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var sailingPK = CreateSchedule("UA838", "JPTYO", new ZDateTime(2018, 10, 31, 16, 55, 0), "USSFO", new ZDateTime(2018, 10, 31, 10, 20, 0));
				Factory.Save();

				var managerParent = new MockISchedule(Factory);
				managerParent.TransportMode = Constants.TransportModes.Air;
				managerParent.Flight = "UA838";
				managerParent.Load = "JPTYO";
				managerParent.Discharge = "USSFO";

				var mockQueryProvider = new Mock<ISailingManagerQueryProvider>(MockBehavior.Strict);
				SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);

				var manager = BaseSailingManager.New(managerParent);
				manager.NotifyRead();

				managerParent.ETA = new ZDateTime(2018, 11, 1, 10, 20, 0);
				manager.DepartureDirty = true;
				manager.NotifyRead();

				AssertEquals("Should have created a new schedule (not in database)", false, manager.Sailing.IsInDatabase);
				AssertEquals("no data available for ETD", ZDateTime.Empty, manager.Sailing.Origin.JA_E_DEP);
				AssertEquals("ETA should set", new ZDateTime(2018, 11, 1, 10, 20, 0), manager.Sailing.Destination.JB_E_ARV);

				manager.NotifySave();
				AssertEquals("still no data available for ETD", ZDateTime.Empty, manager.Sailing.Origin.JA_E_DEP);
				AssertEquals("ETA still set", new ZDateTime(2018, 11, 1, 10, 20, 0), manager.Sailing.Destination.JB_E_ARV);
			}
		}

		public void TestScheduleDateUpdates_NearMatch_DoUpdate()
		{
			ZDateTime today = ZDateTime.Today.AddHours(10);

			ZGuid sailingPK = CreateSchedule("QF1234", HomePort, today.AddHours(24), OverseasPort, today.AddHours(30));
			Factory.Save();

			MockISchedule managerParent = new MockISchedule(Factory);
			managerParent.TransportMode = Constants.TransportModes.Air;
			managerParent.Flight = "QF1234";
			managerParent.Load = HomePort;
			managerParent.Discharge = OverseasPort;
			managerParent.AllowScheduleDatesChanging = true;

			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);

			mockQueryProvider
				.Setup(m => m.QueryFreshMatchBehaviour(It.IsAny<QueryFreshMatchBehaviourArgs>()))
				.Returns(AssertArgsAndReturn("ETD", today.AddHours(25), today.AddHours(24), SailingManagerUpdateMode.UpdateSchedule));

			var manager = BaseSailingManager.New(managerParent);
			manager.NotifyRead();

			managerParent.ETD = today.AddHours(25);
			manager.DepartureDirty = true;
			manager.NotifyRead();

			AssertEquals("ETD should updated", today.AddHours(25), manager.Sailing.Origin.JA_E_DEP);
			AssertEquals("ETA should be unchanged", today.AddHours(30), manager.Sailing.Destination.JB_E_ARV);

			manager.NotifySave();

			AssertEquals("ETD should still be updated", today.AddHours(25), manager.Sailing.Origin.JA_E_DEP);
			AssertEquals("ETA should still be unchanged", today.AddHours(30), manager.Sailing.Destination.JB_E_ARV);
		}

		public void TestScheduleDateUpdates_NearMatch_MultipleDirties()
		{
			ZDateTime today = ZDateTime.Today.AddHours(10);

			ZGuid sailingPK = CreateSchedule("QF1234", HomePort, today.AddHours(24), OverseasPort, today.AddHours(30));
			Factory.Save();

			var managerParent = new MockISchedule(Factory)
			{
				TransportMode = Constants.TransportModes.Air,
				Flight = "QF1234",
				Load = HomePort,
				Discharge = OverseasPort
			};

			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);

			mockQueryProvider
				.Setup(m => m.QueryFreshMatchBehaviour(It.IsAny<QueryFreshMatchBehaviourArgs>()))
			.Returns(AssertArgsAndReturn("ETD", today.AddHours(25), today.AddHours(24), SailingManagerUpdateMode.ScheduleUnchanged));

			BaseSailingManager manager = BaseSailingManager.New(managerParent);
			manager.NotifyRead();

			managerParent.ETD = today.AddHours(25);
			manager.Dirty = true;
			manager.NotifyRead();

			AssertEquals("ETD should be unchanged", today.AddHours(24), manager.Sailing.Origin.JA_E_DEP);
			AssertEquals("ETA should be unchanged", today.AddHours(30), manager.Sailing.Destination.JB_E_ARV);

			manager.NotifySave();

			AssertEquals("ETD should still be unchanged", today.AddHours(24), manager.Sailing.Origin.JA_E_DEP);
			AssertEquals("ETA should still be unchanged", today.AddHours(30), manager.Sailing.Destination.JB_E_ARV);
		}

		public void TestScheduleDateUpdates_NearMatch_NewSchedule()
		{
			ZDateTime today = ZDateTime.Today.AddHours(10);

			ZGuid sailingPK = CreateSchedule("QF1234", HomePort, today.AddHours(24), OverseasPort, today.AddHours(30));
			Factory.Save();

			MockISchedule managerParent = new MockISchedule(Factory);
			managerParent.TransportMode = Constants.TransportModes.Air;
			managerParent.Flight = "QF1234";
			managerParent.Load = HomePort;
			managerParent.Discharge = OverseasPort;

			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);

			mockQueryProvider.Setup(m => m.QueryFreshMatchBehaviour(It.IsAny<QueryFreshMatchBehaviourArgs>()))
			.Returns(AssertArgsAndReturn("ETD", today.AddHours(25), today.AddHours(24), SailingManagerUpdateMode.NewSchedule));
			BaseSailingManager manager = BaseSailingManager.New(managerParent);
			manager.NotifyRead();

			managerParent.ETD = today.AddHours(25);
			manager.DepartureDirty = true;
			manager.NotifyRead();

			AssertEquals("Should have created a new schedule (not in database)", false, manager.Sailing.IsInDatabase);
			AssertEquals("ETD should set", today.AddHours(25), manager.Sailing.Origin.JA_E_DEP);
			AssertEquals("no data available for ETA", ZDateTime.Empty, manager.Sailing.Destination.JB_E_ARV);

			manager.NotifySave();

			AssertEquals("ETD should still be set", today.AddHours(25), manager.Sailing.Origin.JA_E_DEP);
			AssertEquals("Still no data available for ETA", ZDateTime.Empty, manager.Sailing.Destination.JB_E_ARV);
		}

		public void TestScheduleDateUpdates_CompleteMiss()
		{
			ZDateTime today = ZDateTime.Today.AddHours(10);

			Factory.Save();

			MockISchedule managerParent = new MockISchedule(Factory);
			managerParent.TransportMode = Constants.TransportModes.Air;
			managerParent.Flight = "QF1234";
			managerParent.Load = HomePort;
			managerParent.Discharge = OverseasPort;

			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>(MockBehavior.Strict);
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);

			var manager = BaseSailingManager.New(managerParent);
			manager.NotifyRead();

			managerParent.ETD = today.AddHours(25);
			manager.DepartureDirty = true;
			manager.NotifyRead();

			AssertEquals("ETD should be set", today.AddHours(25), manager.Sailing.Origin.JA_E_DEP);
			AssertEquals("ETA not set", ZDateTime.Empty, manager.Sailing.Destination.JB_E_ARV);

			manager.NotifySave();

			AssertEquals("ETD should still be set", today.AddHours(25), manager.Sailing.Origin.JA_E_DEP);
			AssertEquals("ETA still not set", ZDateTime.Empty, manager.Sailing.Destination.JB_E_ARV);
		}

		#endregion

		#region Charter Flights

		public void TestCreateNewCharterFlight()
		{
			MockISchedule schedule = new MockISchedule(Factory);
			schedule.TransportMode = Constants.TransportModes.Air;
			schedule.IsCharter = true;
			schedule.Load = "AUBNE";
			schedule.Discharge = "SGSIN";
			schedule.ETD = ZDateTime.Now;
			schedule.Vessel = "Vessel";
			schedule.IsCargoOnly = false;

			ScheduleManager manager = new ScheduleManager(schedule);
			manager.LoadDirty = true;
			manager.DischargeDirty = true;
			manager.DepartureDirty = true;
			manager.NotifyRead();
			AssertEquals("Should not have a sailing yet", ZGuid.Empty, schedule.SailingPK);

			schedule.Flight = "Blah";
			manager.VoyageDirty = true;
			manager.NotifyRead();
			AssertEquals("Charter flights dont use VoyageFlight", ZGuid.Empty, schedule.SailingPK);

			schedule.RegistrationNo = "Reg";
			manager.RegistrationDirty = true;
			manager.NotifyRead();

			JobSailing sailing = Factory.Load<JobSailing>(schedule.SailingPK);
			AssertNotNull("Should have a sailing.", sailing);
			AssertEquals("Sailing should be new", false, sailing.IsInDatabase);
			AssertEquals("TransportMode", Constants.TransportModes.Air, sailing.Voyage.JV_AirSeaRoad);
			AssertEquals("IsCharter", true, sailing.Voyage.JV_IsChartered);
			AssertEquals("Vessel", "Vessel", sailing.Voyage.JV_RV_NKVessel);
			AssertEquals("IsCargoOnly", false, sailing.Voyage.JV_IsCargoOnly);
		}

		public void TestFindExistingCharterFlight()
		{
			ZDateTime today = ZDateTime.Today;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage.JV_IsChartered = true;
			voyage.JV_VoyageFlight = "Flight";
			voyage.JV_RegistrationNo = "Reg";
			voyage.JV_FlightDate = today;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = HomePort;
			origin.JA_E_DEP = today;

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = OverseasPort;

			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			Factory.Save();

			MockISchedule schedule = new MockISchedule(Factory);
			schedule.TransportMode = Core.Constants.TransportModes.Air;
			schedule.IsCharter = true;
			schedule.RegistrationNo = voyage.JV_RegistrationNo;
			schedule.Load = origin.JA_RL_NKPortOfLoading;
			schedule.Discharge = destination.JB_RL_NKPortOfDischarge;
			schedule.ETD = today;

			ScheduleManager manager = new ScheduleManager(schedule);
			manager.Dirty = true;
			manager.NotifyRead();

			AssertEquals("Should have found the existing charter flight", sailing.PK, schedule.SailingPK);
		}

		#endregion

		public void TestDontBlowUpWhenOutOfRangeDatesAreEntered()
		{
			var invalidDateTime = new ZDateTime(666, 6, 6);
			var invalidSmallDateTime = new ZDateTime(1800, 1, 1);

			var testObject = new MockISchedule(Factory);
			testObject.TransportMode = Constants.TransportModes.Air;
			testObject.Load = OverseasPort;
			testObject.Discharge = HomePort;
			testObject.Voyage = "QF8332";
			testObject.ETA = invalidDateTime;
			testObject.ETD = invalidDateTime;

			var manager = new ScheduleManager(testObject);
			manager.Dirty = true;
			manager.NotifyRead();

			AssertEquals(ZGuid.Empty, testObject.SailingPK);

			testObject.ETA = invalidSmallDateTime;
			testObject.ETD = invalidSmallDateTime;
			manager.Dirty = true;

			AssertNoExceptionThrown(() =>
			{
				manager.NotifyRead();
				manager.FindExistingSailing();
			});

			invalidSmallDateTime = new ZDateTime(1900, 1, 1);
			testObject.ETA = invalidSmallDateTime;
			testObject.ETD = invalidSmallDateTime;
			manager.Dirty = true;

			AssertNoExceptionThrown(() =>
			{
				manager.NotifyRead();
				manager.FindExistingSailing();
			});
		}

		#region FlightScheduleSelectsCorrectFlight

		public void TestFlightScheduleSelectsCorrectFlight_Import()
		{
			GenericFlightScheduleSelectsCorrectFlightTest(true);
		}

		public void TestFlightScheduleSelectsCorrectFlight_Export()
		{
			GenericFlightScheduleSelectsCorrectFlightTest(false);
		}

		void GenericFlightScheduleSelectsCorrectFlightTest(bool import)
		{
			ZDateTime today = ZDateTime.Today;
			TimeSpan almost1Day = new TimeSpan(20, 59, 59);

			ZString loadPort;
			ZString dischargePort;
			ZDateTime pointOfReference;

			if (import)
			{
				loadPort = OverseasPort;
				dischargePort = HomePort;
				pointOfReference = today.AddHours(-3);
			}
			else
			{
				loadPort = HomePort;
				dischargePort = OverseasPort;
				pointOfReference = today;
			}

			JobSailing flight0 = GenerateSailing(loadPort, dischargePort, pointOfReference.AddDays(-1));
			JobSailing flight1 = GenerateSailing(loadPort, dischargePort, pointOfReference);
			JobSailing flight2 = GenerateSailing(loadPort, dischargePort, pointOfReference.AddDays(1));
			JobSailing flight3 = GenerateSailing(loadPort, dischargePort, pointOfReference.AddDays(2));

			Factory.Save();

			MockISchedule testObject = new MockISchedule(Factory);
			testObject.TransportMode = Constants.TransportModes.Air;
			testObject.Load = loadPort;
			testObject.Discharge = dischargePort;
			testObject.Voyage = "QF8332";

			ScheduleManager manager = new ScheduleManager(testObject);

			SetLocalDate(import, testObject, today.AddDays(-1), manager);
			AssertEquals("Should find flight 0", flight0.PK, testObject.SailingPK);

			SetLocalDate(import, testObject, today.Add(-almost1Day), manager);
			AssertEquals("Should keep flight 0", flight0.PK, testObject.SailingPK);

			SetLocalDate(import, testObject, today, manager);
			AssertEquals("Should find flight 1", flight1.PK, testObject.SailingPK);

			SetLocalDate(import, testObject, today.Add(-almost1Day), manager);
			AssertEquals("Should keep flight 1", flight1.PK, testObject.SailingPK);

			SetLocalDate(import, testObject, today.Add(almost1Day), manager);
			AssertEquals("Should keep flight 1", flight1.PK, testObject.SailingPK);

			SetLocalDate(import, testObject, today.AddDays(1), manager);
			AssertEquals("Should find flight 2", flight2.PK, testObject.SailingPK);

			SetLocalDate(import, testObject, today.Add(almost1Day), manager);
			AssertEquals("Should keep flight 2", flight2.PK, testObject.SailingPK);
		}

		#endregion

		#region Creating New Schedules

		public void TestCreateNewSchedule()
		{
			MockISchedule testObject = new MockISchedule(Factory);
			ScheduleManager manager = new ScheduleManager(testObject);

			testObject.Load = Origin;
			testObject.Discharge = Stop3;
			testObject.Flight = FlightNumber1;
			testObject.ETA = Stop3Date;
			testObject.ETD = OriginDate;

			manager.LoadDirty = true;
			manager.DischargeDirty = true;
			manager.VoyageDirty = true;

			Assert("Pre-Condition TestObject SailingPK", testObject.SailingPK == ZGuid.Empty);
			manager.NotifyRead();
			Assert("Should have created a new sailing", testObject.SailingPK != ZGuid.Empty);
			Factory.Save();

			testObject.Discharge = Stop4;
			testObject.ETA = Stop4Date;
			manager.DischargeDirty = true;
			manager.NotifyRead();
			var createdSchedule = Factory.Load<JobSailing>(testObject.SailingPK);
			AssertEquals("New Sailing IsInDatabase", false, createdSchedule.IsInDatabase);
			AssertEquals("Sailing Load", Origin, createdSchedule.JX_JA_RL_NKPortOfLoading);
			AssertEquals("Sailing Discharge", Origin, createdSchedule.JX_JA_RL_NKPortOfLoading);
			AssertEquals("Sailing FlightNumber", FlightNumber1, createdSchedule.JX_JV_VoyageFlight);
			AssertEquals("Sailing DepartureDate", OriginDate, createdSchedule.JX_JA_E_DEP);
			AssertEquals("Update Arrival Date", Stop4Date, createdSchedule.JX_JB_E_ARV);
		}

		public void TestCreateNewSchedule_RegistryOverridesHourDifferenceCheck()
		{
			SystemDataRegistry.Instance.FlightScheduleUpdateThresholdForDataImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 6);

			var mockConsol = new MockISchedule(Factory)
			{
				TransportMode = Constants.TransportModes.Air,
				Flight = FlightNumber1,
				Load = Origin,
				ETD = OriginDate.AddHours(10),
				Discharge = Stop1,
				ETA = Stop1Date.AddHours(24)
			};

			var manager = new ScheduleManager(mockConsol)
			{
				ArrivalDirty = true,
				DischargeDirty = true
			};
			manager.NotifyRead();

			var generatedSailing = Factory.Load<JobSailing>(mockConsol.SailingPK);
			AssertNotNull(generatedSailing);
			AssertEquals(false, generatedSailing.IsInDatabase);

			var voyage = generatedSailing.Voyage;
			AssertEquals(false, voyage.IsInDatabase);

			AddTestSailingsToDB();

			manager.NotifySave();
			Factory.Save();

			AssertEquals("Expected consol's sailing to remain as the generated sailing and NOT to be replaced with the existing sailing within 24 hours",
				generatedSailing.PK, mockConsol.SailingPK);

			var consolSailing = Factory.Load<JobSailing>(mockConsol.SailingPK);

			AssertNotNull(consolSailing);
			AssertEquals(true, consolSailing.IsInDatabase);
			AssertEquals(true, consolSailing.Voyage.IsInDatabase);
			AssertEquals("Voyage should remain the voyage generated and not matched to an already persisted voyage", voyage.PK, consolSailing.Voyage.PK);
		}

		#endregion

		#region Loading Existing Sailings

		public void TestLoadExistingSailing()
		{
			ZGuid sailing1 = CreateSchedule(FlightNumber1, Origin, OriginDate, Stop1, Stop1Date);
			ZGuid sailing2 = CreateSchedule(FlightNumber1, Origin, OriginDate, Stop2, Stop2Date);
			ZGuid sailing3 = CreateSchedule(FlightNumber1, Stop1, Stop1Date, Stop3, Stop3Date);
			ZGuid sailing4 = CreateSchedule(FlightNumber1, Stop1, Stop1Date, Stop4, Stop4Date);
			ZGuid sailing5 = CreateSchedule(FlightNumber1, Origin, OriginDate, Stop5, Stop5Date);
			ZGuid sailing6 = CreateSchedule(FlightNumber1, Origin, OriginDate, Stop6, Stop6Date);
			ZGuid sailing7 = CreateSchedule(FlightNumber1, Stop1, Stop1Date, Stop6, Stop6Date);
			ZGuid sailing8 = CreateSchedule(FlightNumber1, Stop2, Stop2Date, Stop6, Stop6Date);
			ZGuid sailing9 = CreateSchedule(FlightNumber1, Origin, OriginDate, Destination, DestinationDate);

			MockISchedule mockConsol = new MockISchedule(Factory);
			mockConsol.Flight = FlightNumber1;
			ScheduleManager manager = new ScheduleManager(mockConsol);
			mockConsol.Discharge = Stop6;
			manager.DischargeDirty = true;
			mockConsol.Load = Stop1;
			manager.LoadDirty = true;
			mockConsol.ETD = Stop1Date;
			manager.NotifyRead();
			AssertEquals("Manager SailingPK", sailing7, mockConsol.SailingPK);

			mockConsol.Load = Origin;
			manager.LoadDirty = true;
			manager.NotifyRead();
			AssertEquals("Manager SailingPK", sailing6, mockConsol.SailingPK);

			mockConsol.Load = Stop2;
			manager.LoadDirty = true;
			mockConsol.Discharge = Stop6;
			manager.DischargeDirty = true;
			manager.NotifyRead();
			var newPartialSailing = Factory.Load<JobSailing>(mockConsol.SailingPK);

			AssertEquals("Partial Sailing Known Departure Date Loading Stop 2", Stop2Date, newPartialSailing.JX_JA_E_DEP);
			AssertEquals("Partial Sailing Arrival Date", Stop6Date, newPartialSailing.JX_JB_E_ARV);
			AssertEquals("Manager Sailing IsInDatabase", true, newPartialSailing.IsInDatabase);
		}

		#endregion

		public void TestConcurrencyFailureWithSailing()
		{
			MockISchedule mockConsol = new MockISchedule(Factory);
			mockConsol.TransportMode = Constants.TransportModes.Air;
			mockConsol.Flight = FlightNumber1;
			mockConsol.Load = Stop1;
			mockConsol.ETD = Stop1Date;
			mockConsol.Discharge = Stop6;
			mockConsol.ETA = Stop5Date;
			ScheduleManager manager = new ScheduleManager(mockConsol);
			manager.ArrivalDirty = true;
			manager.DischargeDirty = true;
			manager.NotifyRead();
			var createdSailing = Factory.Load<JobSailing>(mockConsol.SailingPK);
			AssertEquals("Created Sailing In Database", false, createdSailing.IsInDatabase);
			ZGuid createdSailingPK = createdSailing.PK;

			AddTestSailingsToDB();

			manager.NotifySave();
			Factory.Save();

			var savedSailing = Factory.Load<JobSailing>(mockConsol.SailingPK);
			Assert("Concurrency Problem should not have occured", savedSailing.PK != createdSailingPK);
		}

		[ExpectNoExceptions()]
		public void TestUpdatingValuesWithInvalidFlightDate()
		{
			const string CorrectFlightNumber = "QF4";
			const string IncorrectFlightNumber = "QF3";
			ZGuid flight1PK = CreateSchedule(IncorrectFlightNumber, "AUSYD", ZDateTime.Now.AddHours(-2), "SGSIN", ZDateTime.Now.AddHours(2));
			var flight1 = Factory.Load<JobSailing>(flight1PK);
			ZGuid flight2PK = CreateSchedule(CorrectFlightNumber, "AUSYD", ZDateTime.Now.AddHours(20), "SGSIN", ZDateTime.Now.AddHours(24));
			var flight2 = Factory.Load<JobSailing>(flight2PK);
			MockISchedule mockConsol = new MockISchedule(Factory);
			ScheduleManager manager = new ScheduleManager(mockConsol);
			manager.Sailing = flight1;
			mockConsol.Flight = CorrectFlightNumber;
			mockConsol.ETA = ZDateTime.Now.AddHours(-2);
			mockConsol.ETD = ZDateTime.Now.AddHours(2);
			mockConsol.Load = "AUSYD";
			mockConsol.Discharge = "SGSIN";
			manager.Dirty = true;
			manager.NotifyRead();
			flight1.Voyage.JV_FlightDate = ZDateTime.Invalid;
			manager.Dirty = true;
			manager.NotifyRead();
		}

		public void TestSetupFlightScheduleWhenKeyFieldsChanged_Export()
		{
			ZDateTime todaysFlight = ZDateTime.Today.AddHours(13);
			ZDateTime todaysFlightDelayed = todaysFlight.AddHours(1);
			ZDateTime yesterdaysFlight = todaysFlight.AddDays(-1);
			ZDateTime tomorrowsFlight = todaysFlight.AddDays(1);
			ZDateTime tomorrowsFlightDelayed = tomorrowsFlight.AddHours(3);
			ZDateTime dayAfterTomorrowFlight = tomorrowsFlight.AddDays(1);
			ZDateTime day2AfterTomorrowFlight = dayAfterTomorrowFlight.AddDays(1);

			ZGuid schedule0 = CreateSchedule(FlightNumber1, HomePort, yesterdaysFlight, OverseasPort3, yesterdaysFlight.AddHours(4));
			ZGuid schedule1 = CreateSchedule(FlightNumber1, HomePort, todaysFlightDelayed, OverseasPort3, todaysFlightDelayed.AddHours(4));
			ZGuid schedule2 = CreateSchedule(FlightNumber1, HomePort, tomorrowsFlight, OverseasPort3, tomorrowsFlight.AddHours(4));
			ZGuid schedule3 = CreateSchedule(FlightNumber1, HomePort, dayAfterTomorrowFlight, OverseasPort3, dayAfterTomorrowFlight.AddHours(4));
			ZGuid schedule4 = CreateSchedule(FlightNumber1, HomePort, day2AfterTomorrowFlight, OverseasPort3, day2AfterTomorrowFlight.AddHours(4));

			MockISchedule mockConsol = new MockISchedule(Factory);
			ScheduleManager manager = new ScheduleManager(mockConsol);

			// Test Selecting Existing Schedule and Modify Departure Date
			mockConsol.Load = HomePort;
			mockConsol.Discharge = OverseasPort3;
			mockConsol.ETD = tomorrowsFlightDelayed;
			mockConsol.Flight = FlightNumber1;
			manager.LoadDirty = true;
			manager.DischargeDirty = true;
			manager.VoyageDirty = true;
			manager.DepartureDirty = true;
			manager.NotifyRead();
			AssertEquals("Should Select Tomorrows Flight Schedule without changing Departure time", schedule2, mockConsol.SailingPK);
			mockConsol.ETD = tomorrowsFlightDelayed;
			manager.DepartureDirty = true;
			manager.NotifyRead();
			AssertEquals("Date Change < 24 hours, It should be considered that the flight is delayed or early "
				+ "and should update the date on the Voyage", tomorrowsFlightDelayed, mockConsol.ETD);
			AssertEquals("Schedule PK should not have changed after modifying departure time at Origin", schedule2, mockConsol.SailingPK);

			MockISchedule mockConsol2 = new MockISchedule(Factory);
			ScheduleManager manager2 = new ScheduleManager(mockConsol2);

			mockConsol2.Load = HomePort;
			mockConsol2.Discharge = OverseasPort3;
			mockConsol2.ETD = tomorrowsFlight;
			mockConsol2.Flight = FlightNumber1;
			manager2.LoadDirty = true;
			manager2.DischargeDirty = true;
			manager2.VoyageDirty = true;
			manager2.DepartureDirty = true;
			manager2.NotifyRead();
			AssertEquals("Should have selected existing flight for tomorrow keeping the delayed departure time", schedule2, mockConsol2.SailingPK);

			mockConsol2.ETD = tomorrowsFlightDelayed.AddDays(1);
			manager2.DepartureDirty = true;
			manager2.NotifyRead();
			AssertEquals("date >= 24 hours, it should look for a new schedule and not update the times yet", schedule3, mockConsol2.SailingPK);
			AssertEquals("Departure Time on Schedule3 Not Delayed Yet", dayAfterTomorrowFlight, Factory.Load<JobSailing>(mockConsol2.SailingPK).Origin.JA_E_DEP);

			mockConsol2.ETD = dayAfterTomorrowFlight.AddHours(12);
			manager2.DepartureDirty = true;
			manager2.NotifyRead();
			AssertEquals("Date < 24 Hours, but time is next day after midnight.  Existing Schedule Should Be Updated", schedule3, mockConsol2.SailingPK);

			mockConsol2.ETD = dayAfterTomorrowFlight.AddDays(1);
			manager2.DepartureDirty = true;
			manager2.NotifyRead();
			AssertEquals("Adding Another 12 hours puts us over the 24 hour limit and we should select the next days flight", schedule4, mockConsol2.SailingPK);
			manager2.NotifySave();

			Factory.Save();
			AssertEquals("The Origin Departure Time should be the original time", day2AfterTomorrowFlight, mockConsol2.ETD);

			MockISchedule mockConsol3 = new MockISchedule(Factory);
			ScheduleManager manager3 = new ScheduleManager(mockConsol3);

			mockConsol3.Load = HomePort;
			mockConsol3.Discharge = OverseasPort3;
			mockConsol3.ETD = day2AfterTomorrowFlight;
			mockConsol3.Flight = FlightNumber1;
			manager3.LoadDirty = true;
			manager3.DischargeDirty = true;
			manager3.VoyageDirty = true;
			manager3.DepartureDirty = true;
			manager3.NotifyRead();
			AssertEquals("Should have selected existing flight for Day 2 after tomorrow", schedule4, mockConsol3.SailingPK);
			mockConsol3.ETD = day2AfterTomorrowFlight.AddDays(1);
			manager3.DepartureDirty = true;
			manager3.NotifyRead();
			Assert("Should have selected New sailing as it is 24 hours in the future and used by another consol", schedule4 != mockConsol3.SailingPK);
		}

		public void TestEnsureUniqueVoyageIsAwareOfTransportModes()
		{
			MockISchedule testObject = new MockISchedule(Factory);
			testObject.TransportMode = Constants.TransportModes.Road;
			testObject.Flight = FlightNumber1;
			testObject.Load = Stop1;
			testObject.ETD = Stop1Date;
			testObject.Discharge = Stop6;
			testObject.ETA = Stop6Date;

			ScheduleManager manager = new ScheduleManager(testObject);
			manager.ArrivalDirty = true;
			manager.DischargeDirty = true;
			manager.NotifyRead();
			var createdSailing = Factory.Load<JobSailing>(testObject.SailingPK);
			AssertEquals("Created Sailing In Database", false, createdSailing.IsInDatabase);

			ZGuid createdSailingPK = createdSailing.PK;

			AddTestSailingsToDB();

			manager.NotifySave();
			Factory.Save();

			var savedSailing = Factory.Load<JobSailing>(testObject.SailingPK);
			AssertEquals("Dont grab the air sailing", Constants.TransportModes.Road, savedSailing.Voyage.JV_AirSeaRoad);
			AssertEquals("No road sailings have been added so we should end up with the created sailing", createdSailingPK, savedSailing.PK);
		}

		#region ScheduleDateUpdates_UserSelectsToCreateNewSchedule

		public void TestNewVoyageWillBeCreatedWhenUserExplicitlySelectThisWhenDateChanged()
		{
			ZDateTime initialFlightTime = new ZDateTime(2011, 12, 25, 10, 0, 0);

			Func<string, JobVoyage> createVoyage = (flightNumber) =>
			{
				JobVoyage voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = flightNumber;

				VoyageOrigin originAKL = voyage.Origins.AddNew();
				originAKL.JA_RL_NKPortOfLoading = "NZAKL";
				originAKL.JA_E_DEP = initialFlightTime;

				VoyageDestination destinationSYD = voyage.Destinations.AddNew();
				destinationSYD.JB_RL_NKPortOfDischarge = "AUSYD";
				destinationSYD.JB_E_ARV = initialFlightTime.AddHours(2);

				return voyage;
			};

			JobVoyage voyage1 = createVoyage("FL001");
			JobVoyage voyage2 = createVoyage("FL002");

			Factory.Save();

			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();

			Func<Transport> createTransport = () =>
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();

				CommonConsol consol = factory.NewWithValidTestData<CommonConsol>();
				consol.JK_UniqueConsignRef = ZGuid.NewZGuid().ToString().Substring(0, 16);
				consol.JK_TransportMode = Constants.TransportModes.Air;

				// Transport has nested ScheduleManager
				Transport transport = consol.Transports[0];

				transport.JW_VoyageFlight = voyage1.JV_VoyageFlight;
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "NZAKL";

				SailingManagerQueryProviderFactory.Set(factory, mockQueryProvider.Object);

				mockQueryProvider.Setup(m => m.QueryFreshMatchBehaviour(It.IsAny<QueryFreshMatchBehaviourArgs>()))
				.Returns(AssertArgsAndReturn("ETD", initialFlightTime.AddHours(4), ZDateTime.Empty, SailingManagerUpdateMode.NewSchedule));
				transport.JW_ETD = initialFlightTime.AddHours(4);
				transport.JW_ETA = initialFlightTime.AddHours(8);

				return transport;
			};

			Transport transport1 = createTransport();
			Transport transport2 = createTransport();

			mockQueryProvider.Setup(m => m.QueryFreshMatchBehaviour(It.IsAny<QueryFreshMatchBehaviourArgs>()))
			.Returns(AssertArgsAndReturn("ETD", initialFlightTime.AddHours(4), ZDateTime.Empty, SailingManagerUpdateMode.NewSchedule));
			transport2.JW_VoyageFlight = voyage2.JV_VoyageFlight;

			transport1.Factory.Save();
			transport2.Factory.Save();

			ZGuid[] existedVoyagePKs = new ZGuid[] { voyage1.PK, voyage2.PK };
			AssertCollectionNotContains("Another voyage created and saved", transport1.Voyage.PK, existedVoyagePKs);
			AssertCollectionNotContains("Another voyage created and saved", transport2.Voyage.PK, existedVoyagePKs);
		}

		#endregion

		#region Implementation

		Func<QueryFreshMatchBehaviourArgs, SailingManagerUpdateMode> AssertArgsAndReturn(ZString expectedDateName, ZDateTime expectedRequestedDate, ZDateTime expectedFoundDate, SailingManagerUpdateMode returnValue)
		{
			return (args) =>
			{
				AssertNotNull(args);
				AssertEquals(typeof(QueryFreshMatchBehaviourArgs), args.GetType());
				AssertEquals(Env.Security.FlightScheduleCreateFromJob, args.AddCheckpoint);
				AssertEquals(Env.Security.FlightScheduleEdit, args.EditCheckpoint);
				AssertEquals(expectedDateName, args.DateName);
				AssertEquals(expectedRequestedDate, args.RequestedDate);
				AssertEquals(expectedFoundDate, args.FoundDate);

				return returnValue;
			};
		}

		void SetLocalDate(bool import, MockISchedule testSchedule, ZDateTime value, ScheduleManager manager)
		{
			if (ImportExportHelper.IsImport(testSchedule.Load, testSchedule.Discharge))
			{
				testSchedule.ETA = value;
			}
			else
			{
				testSchedule.ETD = value;
			}

			manager.Dirty = true;
			manager.NotifyRead();
		}

		JobSailing GenerateSailing(ZString load, ZString discharge, ZDateTime eTD)
		{
			return GenerateSailing(load, discharge, eTD, eTD.AddHours(3));
		}

		JobSailing GenerateSailing(ZString load, ZString discharge, ZDateTime eTD, ZDateTime eTA)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "QF8332";
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = eTD;
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			destination.JB_E_ARV = eTA;
			voyage.GenerateSailings();

			return voyage.Sailings[0];
		}

		const string Origin = "CNSHA";
		const string Stop1 = "SGSIN";
		const string Stop2 = "HKHKG";
		const string Stop3 = "AUBNE";
		const string Stop4 = "AUSYD";
		const string Stop5 = "AUMEL";
		const string Stop6 = "NKALK";
		const string Destination = "USLAX";
		const string FlightNumber1 = "QF417";

		readonly ZDateTime OriginDate = new ZDateTime(2004, 10, 15, 6, 0, 0);
		readonly ZDateTime Stop1Date = new ZDateTime(2004, 10, 15, 8, 0, 0);
		readonly ZDateTime Stop2Date = new ZDateTime(2004, 10, 15, 10, 0, 0);
		readonly ZDateTime Stop3Date = new ZDateTime(2004, 10, 15, 12, 0, 0);
		readonly ZDateTime Stop4Date = new ZDateTime(2004, 10, 15, 14, 0, 0);
		readonly ZDateTime Stop5Date = new ZDateTime(2004, 10, 15, 16, 0, 0);
		readonly ZDateTime Stop6Date = new ZDateTime(2004, 10, 15, 20, 0, 0);
		readonly ZDateTime DestinationDate = new ZDateTime(2004, 10, 15, 22, 0, 0);

		void AddTestSailingsToDB()
		{
			CreateSchedule(FlightNumber1, Origin, OriginDate, Stop1, Stop1Date);
			CreateSchedule(FlightNumber1, Origin, OriginDate, Stop2, Stop2Date);
			CreateSchedule(FlightNumber1, Stop1, Stop1Date, Stop3, Stop3Date);
			CreateSchedule(FlightNumber1, Stop1, Stop1Date, Stop4, Stop4Date);
			CreateSchedule(FlightNumber1, Origin, OriginDate, Stop5, Stop5Date);
			CreateSchedule(FlightNumber1, Origin, OriginDate, Stop6, Stop6Date);
			CreateSchedule(FlightNumber1, Stop1, Stop1Date, Stop6, Stop6Date);
			CreateSchedule(FlightNumber1, Stop2, Stop2Date, Stop6, Stop6Date);
			CreateSchedule(FlightNumber1, Origin, OriginDate, Destination, DestinationDate);
		}

		ZGuid CreateSchedule(ZString flightNumber, ZString originPort, ZDateTime departureDate, ZString destinationPort, ZDateTime arrivalDate)
		{
			BusinessObjectFactory noDBRefreshFactory = new BusinessObjectFactory();
			noDBRefreshFactory.RefreshEnabled = false;
			JobSailing sailing = noDBRefreshFactory.New<JobSailing>();

			ZQuery voyageFilter = new ZQuery(JobVoyageSchema.JV_VoyageFlight, flightNumber);
			voyageFilter.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, Enterprise.Core.Constants.TransportModes.Air);
			voyageFilter.AddToFilter(new ZQuery(JobVoyageSchema.JV_FlightDate, SQLComparisonOperator.GreaterThan, departureDate.AddHours(-5)));
			voyageFilter.AddToFilter(new ZQuery(JobVoyageSchema.JV_FlightDate, SQLComparisonOperator.LessThan, departureDate.AddHours(5)));
			var voyage = noDBRefreshFactory.LoadTop1<JobVoyage>(voyageFilter);

			if (voyage == null)
			{
				voyage = noDBRefreshFactory.New<JobVoyage>();
				voyage.JV_VoyageFlight = flightNumber;
				voyage.JV_FlightDate = departureDate;
				voyage.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Air;
			}

			ZQuery voyageOriginFilter = new ZQuery(JobVoyOriginSchema.JA_JV, voyage.PK);
			voyageOriginFilter.AddToFilter(new ZQuery(JobVoyOriginSchema.JA_RL_NKPortOfLoading, originPort));

			var voyOrigin = Factory.LoadTop1<VoyageOrigin>(voyageOriginFilter);
			if (voyOrigin == null)
			{
				voyOrigin = noDBRefreshFactory.New<VoyageOrigin>();
				voyOrigin.JA_JV = voyage.PK;
				voyOrigin.JA_RL_NKPortOfLoading = originPort;
				voyOrigin.JA_E_DEP = departureDate;
			}
			sailing.JX_JA = voyOrigin.PK;

			ZQuery voyageDestinationFilter = new ZQuery(JobVoyDestinationSchema.JB_JV, voyage.PK);
			voyageDestinationFilter.AddToFilter(new ZQuery(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, destinationPort));
			var voyDestination = Factory.LoadTop1<VoyageDestination>(voyageDestinationFilter);
			if (voyDestination == null)
			{
				voyDestination = noDBRefreshFactory.New<VoyageDestination>();
				voyDestination.JB_JV = voyage.PK;
				voyDestination.JB_RL_NKPortOfDischarge = destinationPort;
				voyDestination.JB_E_ARV = arrivalDate;
			}
			sailing.JX_JB = voyDestination.PK;

			noDBRefreshFactory.Save();
			AssertEquals("Sailing Notifications: " + sailing.Notifications.ToUniqueMessageListString(), false, sailing.HasNotifications());
			return sailing.PK;
		}

		#endregion
	}
}
