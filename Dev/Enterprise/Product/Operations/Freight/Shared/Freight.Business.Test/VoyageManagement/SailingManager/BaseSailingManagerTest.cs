using System;
using System.Linq;
using System.Reflection;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class BaseSailingManagerTest : BaseFreightTest
	{
		public void TestReferencedSailingsArePublished()
		{
			var consol = Factory.New<CommonConsol>();

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_Vessel = "CONDOR";
			transport.JW_VoyageFlight = "27";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USCHI";
			Factory.Save();

			var voyage = transport.Voyage;

			var sydToChi = new AssertionSailing { Load = "AUSYD", Discharge = "USCHI", IsPublished = true };

			AssertSailings("Sailing on transport, should be published", voyage, sydToChi);

			transport.JW_RL_NKDiscPort = "USLAX";
			Factory.Save();

			var sydToLax = new AssertionSailing { Load = "AUSYD", Discharge = "USLAX", IsPublished = true };

			AssertSailings("New Sailing should exist as published, previous Sailing remains published", voyage, sydToChi, sydToLax);

			var manualDestination = voyage.Destinations.AddNew();
			manualDestination.JB_RL_NKPortOfDischarge = "GBLON";
			Factory.Save();

			var sydToLon = new AssertionSailing { Load = "AUSYD", Discharge = "GBLON", IsPublished = true };

			AssertSailings("Destination manually added, should create published Sailing", voyage, sydToChi, sydToLax, sydToLon);

			transport.JW_RL_NKLoadPort = "AUBNE";

			var bneToLax = new AssertionSailing { Load = "AUBNE", Discharge = "USLAX", IsPublished = true };
			var bneToChi = new AssertionSailing { Load = "AUBNE", Discharge = "USCHI", IsPublished = false };
			var bneToLon = new AssertionSailing { Load = "AUBNE", Discharge = "GBLON", IsPublished = false };

			AssertSailings("Sailing on transport should be published. Auto generated combinations should be unpublished.", voyage,
				sydToChi, sydToLax, sydToLon, bneToLax, bneToChi, bneToLon);
		}

		void AssertSailings(string message, JobVoyage voyage, params AssertionSailing[] expectedSailings)
		{
			var actualSailings = voyage.Sailings.Cast<JobSailing>().Select(x =>
				new AssertionSailing
				{
					Load = x.JX_JA_RL_NKPortOfLoading,
					Discharge = x.JX_JB_RL_NKPortOfDischarge,
					IsPublished = x.JX_IsPublished
				});

			AssertContainsExactElementsInAnyOrder(message, expectedSailings, actualSailings);
		}

		public void TestDoNotCreateImpossibleSailings()
		{
			string[] ports = new string[] { "AUSYD", "AUBNE", "SGSIN", "NLAMS" };

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("CONDOR", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "012";

			for (int i = 1; i < ports.Length; i++)
			{
				VoyageOrigin origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = ports[i - 1];
				origin.JA_E_DEP = ZDateTime.Now.AddDays((i << 2) + 1);

				VoyageDestination destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = ports[i];
				destination.JB_E_ARV = ZDateTime.Now.AddDays((i << 2) + 2);
			}

			voyage.GenerateSailings();
			Factory.Save();

			MockISailing parent = new MockISailing(Factory);
			parent.TransportMode = Constants.TransportModes.Sea;
			parent.Vessel = "CONDOR";
			parent.Voyage = "012";
			parent.Load = "SGSIN";
			parent.Discharge = "AUBNE";

			BaseSailingManager manager = BaseSailingManager.New(parent);
			manager.NotifyRead();

			AssertEquals("Sailing should be unmatched.", ZGuid.Empty, parent.SailingPK);
			AssertEquals("No new sailings should be created.", 0, CountNewObjects<JobSailing>(Factory));
		}

		public void TestCreateDenied()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("CONDOR", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "012";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];

			Factory.Save();

			MockISailing parent = new MockISailing(Factory);
			parent.TransportMode = Constants.TransportModes.Sea;
			parent.Vessel = "CONDOR";
			parent.Voyage = "012";
			parent.Load = "AUBNE";
			parent.Discharge = "SGSIN";
			parent.AllowScheduleCreation = false;

			BaseSailingManager manager = BaseSailingManager.New(parent);
			manager.NotifyRead();
			AssertEquals("loading an existing sailing is ok", sailing.PK, parent.SailingPK);

			parent.Load = "AUSYD";
			parent.Discharge = "SGSIN";
			manager.Dirty = true;
			manager.NotifyRead();
			AssertEquals("1) sailing should be unmatched", ZGuid.Empty, parent.SailingPK);
			AssertEquals("1) should not have created any new sailings", 0, CountNewObjects<JobSailing>(Factory));
			AssertEquals("1) should not have created any new origins", 0, CountNewObjects<VoyageOrigin>(Factory));
			AssertEquals("1) should not have created any new destinations", 0, CountNewObjects<VoyageDestination>(Factory));
			AssertEquals("1) should not have created any new voyages", 0, CountNewObjects<JobVoyage>(Factory));

			parent.Voyage = "013";
			manager.Dirty = true;
			manager.NotifyRead();
			AssertEquals("2) sailing should be unmatched", ZGuid.Empty, parent.SailingPK);
			AssertEquals("2) should not have created any new sailings", 0, CountNewObjects<JobSailing>(Factory));
			AssertEquals("2) should not have created any new origins", 0, CountNewObjects<VoyageOrigin>(Factory));
			AssertEquals("2) should not have created any new destinations", 0, CountNewObjects<VoyageDestination>(Factory));
			AssertEquals("2) should not have created any new voyages", 0, CountNewObjects<JobVoyage>(Factory));
		}

		public void TestDoNotMatchNonPersistedVoyage()
		{
			var consol1 = Factory.NewWithValidTestData<CommonConsol>();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;

			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage1.JV_FlightDate = new ZDateTime(2012, 1, 1, 9, 10, 0);

			var origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "USLAX";
			origin1.JA_E_DEP = new ZDateTime(2012, 1, 1, 9, 10, 0);

			var destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUSYD";
			destination1.JB_E_ARV = new ZDateTime(2012, 1, 1, 13, 10, 0);

			voyage1.GenerateSailings();

			var sailing1 = voyage1.Sailings[0];

			consol1.Transports.MostInterestingTransport.JW_JX = sailing1.PK;

			var consol2 = Factory.NewWithValidTestData<CommonConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage2.JV_FlightDate = new ZDateTime(2012, 1, 2, 9, 10, 0);

			var origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "USLAX";
			origin2.JA_E_DEP = new ZDateTime(2012, 1, 1, 9, 10, 0);

			var destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "AUSYD";
			destination2.JB_E_ARV = new ZDateTime(2012, 1, 1, 13, 10, 0);

			voyage2.GenerateSailings();

			var sailing2 = voyage2.Sailings[0];

			consol2.Transports.MostInterestingTransport.JW_JX = sailing2.PK;

			AssertNoExceptionThrown("should not throw CargoWise.EntityFramework.ZSaveException", Factory.Save);
		}

		public void TestSetReasonForSailingNotGenerated()
		{
			var parent = new MockISailing(Factory);
			parent.TransportMode = Constants.TransportModes.Sea;
			parent.Vessel = "CONDOR";
			parent.Voyage = "012";
			parent.Load = "AUBNE";
			parent.Discharge = "AUBNE";
			parent.AllowScheduleCreation = true;

			var manager = BaseSailingManager.New(parent);
			manager.NotifyRead();
			AssertNull("Should not have created any new sailings", manager.Sailing);
			AssertEquals("The Load (AUBNE) and Discharge (AUBNE) cannot be the same for SEA voyage.", manager.ReasonForSailingNotGenerated);

			manager.Sailing = Factory.NewWithValidTestData<JobSailing>();
			AssertEquals("Setting Sailing should reset ReasonForSailingNotGenerated", ZString.Empty, manager.ReasonForSailingNotGenerated);
		}

		#region Delete Origin/Destination/Sailing Duplicates on Save

		[ExpectNoExceptions]
		public void TestDuplicateOriginDestinationsDeletedOnSave()
		{
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Courier;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Courier;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";

			Factory.Save();

			BaseSailingManager manager = BaseSailingManager.New(consol.Transports[0]);

			JobVoyage newVoyage = Factory.New<JobVoyage>();

			VoyageOrigin newOrigin = newVoyage.Origins.AddNew();
			newOrigin.JA_RL_NKPortOfLoading = "AUSYD";

			VoyageOrigin newOrigin2 = newVoyage.Origins.AddNew();
			newOrigin2.JA_RL_NKPortOfLoading = "AUSYD";

			VoyageDestination newDestination = newVoyage.Destinations.AddNew();
			newDestination.JB_RL_NKPortOfDischarge = "USLAX";

			VoyageDestination newDestination2 = newVoyage.Destinations.AddNew();
			newDestination2.JB_RL_NKPortOfDischarge = "USLAX";

			manager.Sailing = newVoyage.Sailings[0];
			manager.NotifySave();

			Factory.Save();
		}

		public void TestAllDuplicatesAreDeletedOnNotifySaveIncludingOnesCreatedSimultaneouslyInConcurrentEnterprise()
		{
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Courier;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Courier;

			VoyageOrigin originSYD = voyage.Origins.AddNew();
			originSYD.JA_RL_NKPortOfLoading = "AUSYD";

			VoyageDestination destinationAKL = voyage.Destinations.AddNew();
			destinationAKL.JB_RL_NKPortOfDischarge = "NZAKL";

			Factory.Save();

			AssertEquals("Precondition", 1, voyage.Origins.Count);
			AssertEquals("Precondition", 1, voyage.Destinations.Count);
			AssertEquals("Precondition", 1, voyage.Sailings.Count);

			JobSailing sailingSYD_AKL = voyage.Sailings[0];

			Func<JobVoyage> reloadVoyageInIsolatedFactoryThenAddNewOriginDestinationAndGenerateSailings = () =>
			{
				BusinessObjectFactory factory = new BusinessObjectFactory() { RefreshEnabled = false };
				JobVoyage voyageReloaded = factory.Load<JobVoyage>(voyage.PK);

				voyageReloaded.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
				voyageReloaded.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";

				AssertContainsExactElementsInAnyOrder(new ZString[] { "AUSYD", "NZAKL" }, voyageReloaded.Origins.Cast<VoyageOrigin>().Select(x => x.JA_RL_NKPortOfLoading));
				AssertContainsExactElementsInAnyOrder(new ZString[] { "NZAKL", "USLAX" }, voyageReloaded.Destinations.Cast<VoyageDestination>().Select(x => x.JB_RL_NKPortOfDischarge));
				AssertEquals("New sailings generated", 3, voyageReloaded.Sailings.Count);

				return voyageReloaded;
			};

			// Simultaneously in two isolated factories we would add new origin/destination to existing voyage => new sailings would be generated

			JobVoyage voyageReloaded1 = reloadVoyageInIsolatedFactoryThenAddNewOriginDestinationAndGenerateSailings();
			var originAKL = voyageReloaded1.Origins.Cast<VoyageOrigin>().Single(x => x.JA_RL_NKPortOfLoading == "NZAKL");
			var destinationLAX = voyageReloaded1.Destinations.Cast<VoyageDestination>().Single(x => x.JB_RL_NKPortOfDischarge == "USLAX");

			var sailingAKL_LAX = voyageReloaded1.Sailings.GetSailingFromLoadAndDischarge("NZAKL", "USLAX");
			var sailingSYD_LAX = voyageReloaded1.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX");

			JobVoyage voyageReloaded2 = reloadVoyageInIsolatedFactoryThenAddNewOriginDestinationAndGenerateSailings();
			var originAKL2 = voyageReloaded2.Origins.Cast<VoyageOrigin>().Single(x => x.JA_RL_NKPortOfLoading == "NZAKL");
			var destinationLAX2 = voyageReloaded2.Destinations.Cast<VoyageDestination>().Single(x => x.JB_RL_NKPortOfDischarge == "USLAX");

			var sailingAKL_LAX2 = voyageReloaded2.Sailings.GetSailingFromLoadAndDischarge("NZAKL", "USLAX");
			var sailingSYD_LAX2 = voyageReloaded2.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX");

			CommonConsol consolReloaded2 = voyageReloaded2.Factory.Load<CommonConsol>(consol.PK);
			Transport newTransport = consolReloaded2.Transports.AddNew();
			newTransport.JW_IsLinked = true;
			newTransport.JW_JX = sailingAKL_LAX2.PK;

			BaseSailingManager manager = BaseSailingManager.New(newTransport);
			manager.Sailing = sailingAKL_LAX2;

			AssertEquals("Precondition", voyageReloaded2, manager.Voyage);
			AssertEquals("Precondition", sailingAKL_LAX2, manager.Sailing);

			// Saving first factory -- this should persist new origin, destination and sailings
			voyageReloaded1.Factory.Save();

			// Manager should match ALL duplicates of origins, destinations and sailings and delete them
			manager.NotifySave();
			voyageReloaded2.Factory.Save();

			var duplicates = new BusinessObject[] { originAKL2, destinationLAX2, sailingAKL_LAX2, sailingSYD_LAX2 };
			AssertEquals("Duplicates should be deleted", true, duplicates.All(bizo => bizo.IsDeleted && !bizo.IsInDatabase));

			voyage = new BusinessObjectFactory().Load<JobVoyage>(voyage.PK);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { originSYD.PK, originAKL.PK }, voyage.Origins.Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { destinationAKL.PK, destinationLAX.PK }, voyage.Destinations.Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { sailingSYD_AKL.PK, sailingAKL_LAX.PK, sailingSYD_LAX.PK }, voyage.Sailings.Select(x => x.PK));

			// Extra sanity check to ensure sailings are not messed up
			AssertEquals(sailingSYD_AKL.PK, voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "NZAKL").PK);
			AssertEquals(sailingAKL_LAX.PK, voyage.Sailings.GetSailingFromLoadAndDischarge("NZAKL", "USLAX").PK);
			AssertEquals(sailingSYD_LAX.PK, voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX").PK);
		}

		public void TestOriginDestinationUpdateIsSuppressedWhenEnsuringUniqueVoyOriginDestination()
		{
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Courier;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Courier;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";

			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			// Updating factory's query cache => to ensure only ZDBOnlyQuery in production methods will work
			anotherFactory.SeedQueryCache(VoyageOrigin.Schema.TableName, new ZQuery(JobVoyOriginSchema.JA_RL_NKPortOfLoading, "AUSYD"));
			anotherFactory.SeedQueryCache(VoyageDestination.Schema.TableName, new ZQuery(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, "USLAX"));

			VoyageOrigin duplicatedOrigin = anotherFactory.New<VoyageOrigin>();
			duplicatedOrigin.JA_RL_NKPortOfLoading = "AUSYD";
			duplicatedOrigin.JA_JV = voyage.PK;

			VoyageDestination duplicatedDestination = anotherFactory.New<VoyageDestination>();
			duplicatedDestination.JB_RL_NKPortOfDischarge = "USLAX";
			duplicatedDestination.JB_JV = voyage.PK;

			JobSailing duplicatedSailing = anotherFactory.New<JobSailing>();
			duplicatedSailing.JX_JA = duplicatedOrigin.PK;
			duplicatedSailing.JX_JB = duplicatedDestination.PK;

			BaseSailingManager manager = BaseSailingManager.New(anotherFactory.Load<CommonConsol>(consol.PK).Transports[0]);
			manager.Sailing = anotherFactory.Load<JobSailingForSuppressionTesting>(duplicatedSailing.PK);

			AssertEquals("Precondition", duplicatedOrigin.PK, manager.Sailing.JX_JA);
			AssertEquals("Precondition", duplicatedDestination.PK, manager.Sailing.JX_JB);
			AssertEquals("Precondition", false, JobSailingForSuppressionTesting.JX_JA_SetterCalled && JobSailingForSuppressionTesting.JX_JB_SetterCalled);

			manager.NotifySave();

			AssertEquals("Sailing's voyage origin was changed", origin.PK, manager.Sailing.JX_JA);
			AssertEquals("Sailing's voyage destination was changed", destination.PK, manager.Sailing.JX_JB);
			AssertEquals("Both property setters were called", true, JobSailingForSuppressionTesting.JX_JA_SetterCalled && JobSailingForSuppressionTesting.JX_JB_SetterCalled);

			AssertNoExceptionThrown(anotherFactory.Save);
		}

		public void TestReleaseOriginOrDestinationWhenItsChildSalingIsNotReferenced()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VesselForTest";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "TEST";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";

			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_JX = voyage.Sailings[0].PK;
			transport.JW_VoyageFlight = voyage.JV_VoyageFlight;
			transport.JW_Vessel = voyage.JV_RV_NKVessel;

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var consolInNewFactory = newFactory.Load<CommonConsol>(consol.PK);
			var transportInNewFactory = consolInNewFactory.Transports.MostInterestingTransport;

			transportInNewFactory.JW_RL_NKLoadPort = "00000";
			transportInNewFactory.JW_RL_NKDiscPort = "11111";

			AssertEquals("00000", transportInNewFactory.Sailing.Origin.JA_RL_NKPortOfLoading);
			AssertEquals("11111", transportInNewFactory.Sailing.Destination.JB_RL_NKPortOfDischarge);

			transportInNewFactory.JW_RL_NKLoadPort = "AUBNE";
			transportInNewFactory.JW_RL_NKDiscPort = "NZAKL";

			newFactory.Save();

			newFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var voyOriginList = newFactory.Load<VoyageOrigin>(new ZQuery());
			var voyDestinationList = newFactory.Load<VoyageDestination>(new ZQuery());

			var expectedOriginPorts = new[] { "AUSYD", "AUBNE" };
			var expectedDestinationPorts = new[] { "SGSIN", "NZAKL" };

			var actualOriginPorts = voyOriginList.Select(c => c.JA_RL_NKPortOfLoading);
			var actualDestinationPorts = voyDestinationList.Select(c => c.JB_RL_NKPortOfDischarge);

			AssertContainsExactElementsInAnyOrder("Should not contains 00000 as its sailing is not referenced",
				expectedOriginPorts, actualOriginPorts);

			AssertContainsExactElementsInAnyOrder("Should not contains 11111 as its sailing is not referenced",
				expectedDestinationPorts, actualDestinationPorts);
		}

		class JobSailingForSuppressionTesting : JobSailing
		{
			public JobSailingForSuppressionTesting(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
				JX_JA_SetterCalled = false;
				JX_JB_SetterCalled = false;
			}

			public override ZGuid JX_JA
			{
				set
				{
					AssertEquals(true, SailingScheduleDataVendor.IsOriginUpdateSuppressed(this.Factory));
					JX_JA_SetterCalled = true;
					base.JX_JA = value;
				}
			}
			public static bool JX_JA_SetterCalled;

			public override ZGuid JX_JB
			{
				set
				{
					AssertEquals(true, SailingScheduleDataVendor.IsDestinationUpdateSuppressed(this.Factory));
					JX_JB_SetterCalled = true;
					base.JX_JB = value;
				}
			}
			public static bool JX_JB_SetterCalled;
		}

		#endregion

		public void TestSyncParentOnDataRefreshHasNoConflictsInDatabase()
		{
			var dateTime = new ZDateTime(2019, 11, 11, 5, 0, 0);
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "HKHKG";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			var transport = Factory.NewWithValidTestData<CommonShipment>().Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			address.Address1 = "destination";
			var newAddress = orgHeader.Addresses.AddNew();
			newAddress.Address1 = "new address";

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var anotherVoyage = anotherFactory.Load<JobVoyage>(voyage.PK);
			var anotherSailing = anotherFactory.Load<JobSailing>(sailing.PK);
			var anotherDestination = anotherFactory.Load<VoyageDestination>(destination.PK);
			var anotherOrigin = anotherFactory.Load<VoyageOrigin>(origin.PK);

			#region Set in Another Factory

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "ABSDF";

			anotherVoyage.JV_VoyageFlight = "ASD1234";
			anotherVoyage.JV_RV_NKVessel = vessel.RV_FK;
			anotherVoyage.JV_IsChartered = true;
			anotherVoyage.JV_AircraftType = "AIR";

			anotherOrigin.JA_RL_NKPortOfLoading = "AU2CO";
			anotherOrigin.JA_E_DEP = dateTime;
			anotherOrigin.JA_A_DEP = dateTime;
			anotherOrigin.JA_S_DEP = dateTime;
			anotherOrigin.JA_OA_DepartureCTOAddress = address.PK;

			anotherDestination.JB_RL_NKPortOfDischarge = "HK8ST";
			anotherDestination.JB_E_ARV = dateTime;
			anotherDestination.JB_A_ARV = dateTime;
			anotherDestination.JB_S_ARV = dateTime;
			anotherDestination.JB_OA_ArrivalCTOAddress = address.PK;

			anotherFactory.Save();
			#endregion

			var sql = $@"
UPDATE dbo.JobVoyage
SET
	JV_VoyageFlight = 'XYZ4321',
	JV_RV_NKVessel = 'MNBVC',
	JV_IsChartered = 0,
	JV_AircraftType = 'SEA',
	JV_SystemLastEditTimeUtc = '2014-08-08 02:41:00',
	JV_SystemLastEditUser = 'E'
WHERE
	JV_PK = '{voyage.PK}'
UPDATE dbo.JobVoyOrigin
SET
	JA_RL_NKPortOfLoading = 'AU2SN',
	JA_E_DEP = DATEADD(DAY, 1, GETDATE()),
	JA_A_DEP = DATEADD(DAY, 1, GETDATE()),
	JA_S_DEP = DATEADD(DAY, 1, GETDATE()),
	JA_OA_DepartureCTOAddress = '{newAddress.PK}',
	JA_SystemLastEditTimeUtc = '2014-08-08 02:41:00',
	JA_SystemLastEditUser = 'E'
WHERE
	JA_PK = '{origin.PK}'
UPDATE dbo.JobVoyDestination
SET
	JB_RL_NKPortOfDischarge = 'HK8TT',
	JB_E_ARV = DATEADD(DAY, 1, GETDATE()),
	JB_A_ARV = DATEADD(DAY, 1, GETDATE()),
	JB_S_ARV = DATEADD(DAY, 1, GETDATE()),
	JB_OA_ArrivalCTOAddress = '{newAddress.PK}',
	JB_SystemLastEditTimeUtc = '2014-08-08 02:41:00',
	JB_SystemLastEditUser = 'E'
WHERE
	JB_PK = '{destination.PK}'";
			Db.Connection.ExecuteNonQuery(sql);

			transport.JW_LegOrder = 5;
			AssertNoExceptionThrown(Factory.Save);

			transport.JW_LegOrder = 7;
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestSyncOriginalValuesOnDataRefresh()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();

			var dateTime = new ZDateTime(2019, 11, 11, 5, 0, 0);
			var voyage = Factory.New<JobVoyage>();

			voyage.JV_RV_NKVessel = "CONDOR";
			voyage.JV_VoyageFlight = "012";
			voyage.JV_OH_Line = orgHeader1.PK;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = dateTime;
			origin.JA_ReceivalCommences = dateTime;
			origin.JA_CutOff = dateTime;
			origin.JA_DocumentaryCutoff = dateTime;
			origin.JA_VGMCutOff = dateTime;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "HKHKG";
			destination.JB_E_ARV = dateTime;

			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			sailing.JX_DepotReceivalCommences = dateTime;
			sailing.JX_DepotCutOff = dateTime;

			var transport = Factory.NewWithValidTestData<CommonShipment>().Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			Factory.Save();

			AssertEquals("AUSYD", transport.JW_RL_NKLoadPortOriginalValue.Value);
			AssertEquals("HKHKG", transport.JW_RL_NKDiscPortOriginalValue.Value);
			AssertEquals("CONDOR", transport.JW_VesselOriginalValue.Value);
			AssertEquals("012", transport.JW_VoyageFlightOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_ETDOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_ETAOriginalValue.Value);
			AssertEquals(orgHeader1.MainAddress.PK, transport.JW_OA_CarrierAddressOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_TerminalReceivalCommencesOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_DepotReceivalCommencesOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_TerminalCutOffOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_DepotCutOffOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_DocumentaryCutOffOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_VGMCutOffOriginalValue.Value);

			var anotherFactory = new BusinessObjectFactory();
			var anotherVoyage = anotherFactory.Load<JobVoyage>(voyage.PK);
			var anotherSailing = anotherFactory.Load<JobSailing>(sailing.PK);
			var anotherDestination = anotherFactory.Load<VoyageDestination>(destination.PK);
			var anotherOrigin = anotherFactory.Load<VoyageOrigin>(origin.PK);

			anotherVoyage.JV_RV_NKVessel = "CONDOR1";
			anotherVoyage.JV_VoyageFlight = "0122";
			anotherVoyage.JV_OH_Line = orgHeader2.PK;

			anotherFactory.Save();

			AssertEquals("AUSYD", transport.JW_RL_NKLoadPortOriginalValue.Value);
			AssertEquals("HKHKG", transport.JW_RL_NKDiscPortOriginalValue.Value);
			AssertEquals("CONDOR1", transport.JW_VesselOriginalValue.Value);
			AssertEquals("0122", transport.JW_VoyageFlightOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_ETDOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_ETAOriginalValue.Value);
			AssertEquals(orgHeader2.MainAddress.PK, transport.JW_OA_CarrierAddressOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_TerminalReceivalCommencesOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_DepotReceivalCommencesOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_TerminalCutOffOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_DepotCutOffOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_DocumentaryCutOffOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_VGMCutOffOriginalValue.Value);

			AssertEquals(transport.JW_RL_NKLoadPort, transport.JW_RL_NKLoadPortOriginalValue.Value);
			AssertEquals(transport.JW_RL_NKDiscPort, transport.JW_RL_NKDiscPortOriginalValue.Value);
			AssertEquals(transport.JW_Vessel, transport.JW_VesselOriginalValue.Value);
			AssertEquals(transport.JW_VoyageFlight, transport.JW_VoyageFlightOriginalValue.Value);
			AssertEquals(transport.JW_ETD, transport.JW_ETDOriginalValue.Value);
			AssertEquals(transport.JW_ETA, transport.JW_ETAOriginalValue.Value);
			AssertEquals(transport.JW_OA_CarrierAddress, transport.JW_OA_CarrierAddressOriginalValue.Value);
			AssertEquals(transport.JW_TerminalReceivalCommences, transport.JW_TerminalReceivalCommencesOriginalValue.Value);
			AssertEquals(transport.JW_DepotReceivalCommences, transport.JW_DepotReceivalCommencesOriginalValue.Value);
			AssertEquals(transport.JW_TerminalCutOff, transport.JW_TerminalCutOffOriginalValue.Value);
			AssertEquals(transport.JW_DepotCutOff, transport.JW_DepotCutOffOriginalValue.Value);
			AssertEquals(transport.JW_DocumentaryCutOff, transport.JW_DocumentaryCutOffOriginalValue.Value);
			AssertEquals(transport.JW_VGMCutOff, transport.JW_VGMCutOffOriginalValue.Value);

			anotherOrigin.JA_RL_NKPortOfLoading = "AUEML";
			anotherOrigin.JA_E_DEP = dateTime.AddDays(1);
			anotherOrigin.JA_ReceivalCommences = dateTime.AddDays(1);
			anotherOrigin.JA_CutOff = dateTime.AddDays(1);
			anotherOrigin.JA_DocumentaryCutoff = dateTime.AddDays(1);
			anotherOrigin.JA_VGMCutOff = dateTime.AddDays(1);

			anotherFactory.Save();

			AssertEquals("AUEML", transport.JW_RL_NKLoadPortOriginalValue.Value);
			AssertEquals("HKHKG", transport.JW_RL_NKDiscPortOriginalValue.Value);
			AssertEquals("CONDOR1", transport.JW_VesselOriginalValue.Value);
			AssertEquals("0122", transport.JW_VoyageFlightOriginalValue.Value);
			AssertEquals(dateTime.AddDays(1), transport.JW_ETDOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_ETAOriginalValue.Value);
			AssertEquals(orgHeader2.MainAddress.PK, transport.JW_OA_CarrierAddressOriginalValue.Value);
			AssertEquals(dateTime.AddDays(1), transport.JW_TerminalReceivalCommencesOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_DepotReceivalCommencesOriginalValue.Value);
			AssertEquals(dateTime.AddDays(1), transport.JW_TerminalCutOffOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_DepotCutOffOriginalValue.Value);
			AssertEquals(dateTime.AddDays(1), transport.JW_DocumentaryCutOffOriginalValue.Value);
			AssertEquals(dateTime.AddDays(1), transport.JW_VGMCutOffOriginalValue.Value);

			AssertEquals(transport.JW_RL_NKLoadPort, transport.JW_RL_NKLoadPortOriginalValue.Value);
			AssertEquals(transport.JW_RL_NKDiscPort, transport.JW_RL_NKDiscPortOriginalValue.Value);
			AssertEquals(transport.JW_Vessel, transport.JW_VesselOriginalValue.Value);
			AssertEquals(transport.JW_VoyageFlight, transport.JW_VoyageFlightOriginalValue.Value);
			AssertEquals(transport.JW_ETD, transport.JW_ETDOriginalValue.Value);
			AssertEquals(transport.JW_ETA, transport.JW_ETAOriginalValue.Value);
			AssertEquals(transport.JW_OA_CarrierAddress, transport.JW_OA_CarrierAddressOriginalValue.Value);
			AssertEquals(transport.JW_TerminalReceivalCommences, transport.JW_TerminalReceivalCommencesOriginalValue.Value);
			AssertEquals(transport.JW_DepotReceivalCommences, transport.JW_DepotReceivalCommencesOriginalValue.Value);
			AssertEquals(transport.JW_TerminalCutOff, transport.JW_TerminalCutOffOriginalValue.Value);
			AssertEquals(transport.JW_DepotCutOff, transport.JW_DepotCutOffOriginalValue.Value);
			AssertEquals(transport.JW_DocumentaryCutOff, transport.JW_DocumentaryCutOffOriginalValue.Value);
			AssertEquals(transport.JW_VGMCutOff, transport.JW_VGMCutOffOriginalValue.Value);

			anotherDestination.JB_RL_NKPortOfDischarge = "HK8ST";
			anotherDestination.JB_E_ARV = dateTime.AddDays(2);

			anotherFactory.Save();

			AssertEquals("AUEML", transport.JW_RL_NKLoadPortOriginalValue.Value);
			AssertEquals("HK8ST", transport.JW_RL_NKDiscPortOriginalValue.Value);
			AssertEquals("CONDOR1", transport.JW_VesselOriginalValue.Value);
			AssertEquals("0122", transport.JW_VoyageFlightOriginalValue.Value);
			AssertEquals(dateTime.AddDays(1), transport.JW_ETDOriginalValue.Value);
			AssertEquals(dateTime.AddDays(2), transport.JW_ETAOriginalValue.Value);
			AssertEquals(orgHeader2.MainAddress.PK, transport.JW_OA_CarrierAddressOriginalValue.Value);
			AssertEquals(dateTime.AddDays(1), transport.JW_TerminalReceivalCommencesOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_DepotReceivalCommencesOriginalValue.Value);
			AssertEquals(dateTime.AddDays(1), transport.JW_TerminalCutOffOriginalValue.Value);
			AssertEquals(dateTime, transport.JW_DepotCutOffOriginalValue.Value);
			AssertEquals(dateTime.AddDays(1), transport.JW_DocumentaryCutOffOriginalValue.Value);
			AssertEquals(dateTime.AddDays(1), transport.JW_VGMCutOffOriginalValue.Value);

			AssertEquals(transport.JW_RL_NKLoadPort, transport.JW_RL_NKLoadPortOriginalValue.Value);
			AssertEquals(transport.JW_RL_NKDiscPort, transport.JW_RL_NKDiscPortOriginalValue.Value);
			AssertEquals(transport.JW_Vessel, transport.JW_VesselOriginalValue.Value);
			AssertEquals(transport.JW_VoyageFlight, transport.JW_VoyageFlightOriginalValue.Value);
			AssertEquals(transport.JW_ETD, transport.JW_ETDOriginalValue.Value);
			AssertEquals(transport.JW_ETA, transport.JW_ETAOriginalValue.Value);
			AssertEquals(transport.JW_OA_CarrierAddress, transport.JW_OA_CarrierAddressOriginalValue.Value);
			AssertEquals(transport.JW_TerminalReceivalCommences, transport.JW_TerminalReceivalCommencesOriginalValue.Value);
			AssertEquals(transport.JW_DepotReceivalCommences, transport.JW_DepotReceivalCommencesOriginalValue.Value);
			AssertEquals(transport.JW_TerminalCutOff, transport.JW_TerminalCutOffOriginalValue.Value);
			AssertEquals(transport.JW_DepotCutOff, transport.JW_DepotCutOffOriginalValue.Value);
			AssertEquals(transport.JW_DocumentaryCutOff, transport.JW_DocumentaryCutOffOriginalValue.Value);
			AssertEquals(transport.JW_VGMCutOff, transport.JW_VGMCutOffOriginalValue.Value);

			anotherSailing.JX_DepotReceivalCommences = dateTime.AddDays(3);
			anotherSailing.JX_DepotCutOff = dateTime.AddDays(3);

			anotherFactory.Save();

			AssertEquals("AUEML", transport.JW_RL_NKLoadPortOriginalValue.Value);
			AssertEquals("HK8ST", transport.JW_RL_NKDiscPortOriginalValue.Value);
			AssertEquals("CONDOR1", transport.JW_VesselOriginalValue.Value);
			AssertEquals("0122", transport.JW_VoyageFlightOriginalValue.Value);
			AssertEquals(dateTime.AddDays(1), transport.JW_ETDOriginalValue.Value);
			AssertEquals(dateTime.AddDays(2), transport.JW_ETAOriginalValue.Value);
			AssertEquals(orgHeader2.MainAddress.PK, transport.JW_OA_CarrierAddressOriginalValue.Value);
			AssertEquals(dateTime.AddDays(1), transport.JW_TerminalReceivalCommencesOriginalValue.Value);
			AssertEquals(dateTime.AddDays(3), transport.JW_DepotReceivalCommencesOriginalValue.Value);
			AssertEquals(dateTime.AddDays(1), transport.JW_TerminalCutOffOriginalValue.Value);
			AssertEquals(dateTime.AddDays(3), transport.JW_DepotCutOffOriginalValue.Value);
			AssertEquals(dateTime.AddDays(1), transport.JW_DocumentaryCutOffOriginalValue.Value);
			AssertEquals(dateTime.AddDays(1), transport.JW_VGMCutOffOriginalValue.Value);

			AssertEquals(transport.JW_RL_NKLoadPort, transport.JW_RL_NKLoadPortOriginalValue.Value);
			AssertEquals(transport.JW_RL_NKDiscPort, transport.JW_RL_NKDiscPortOriginalValue.Value);
			AssertEquals(transport.JW_Vessel, transport.JW_VesselOriginalValue.Value);
			AssertEquals(transport.JW_VoyageFlight, transport.JW_VoyageFlightOriginalValue.Value);
			AssertEquals(transport.JW_ETD, transport.JW_ETDOriginalValue.Value);
			AssertEquals(transport.JW_ETA, transport.JW_ETAOriginalValue.Value);
			AssertEquals(transport.JW_OA_CarrierAddress, transport.JW_OA_CarrierAddressOriginalValue.Value);
			AssertEquals(transport.JW_TerminalReceivalCommences, transport.JW_TerminalReceivalCommencesOriginalValue.Value);
			AssertEquals(transport.JW_DepotReceivalCommences, transport.JW_DepotReceivalCommencesOriginalValue.Value);
			AssertEquals(transport.JW_TerminalCutOff, transport.JW_TerminalCutOffOriginalValue.Value);
			AssertEquals(transport.JW_DepotCutOff, transport.JW_DepotCutOffOriginalValue.Value);
			AssertEquals(transport.JW_DocumentaryCutOff, transport.JW_DocumentaryCutOffOriginalValue.Value);
			AssertEquals(transport.JW_VGMCutOff, transport.JW_VGMCutOffOriginalValue.Value);
		}

		[ExpectNoExceptions]
		public void TestSyncParentOnDataRefresh()
		{
			ExportSailing.Origin.JA_OA_DepartureCTOAddress = ExportSailing.Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			ExportSailing.Destination.JB_OA_ArrivalCTOAddress = ExportSailing.Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			ExportSailing.Factory.Save();

			var parentMock = new Mock<ISailingManaged>();
			var parent = parentMock.Object;

			parentMock.Setup(m => m.TransportMode).Returns(ZString.Empty);
			BaseSailingManager manager = BaseSailingManager.New(parent);
			parentMock.VerifyAll();
			parentMock.Reset();
			manager.Sailing = ExportSailing;

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			JobSailing sailingInAnotherFactory = anotherFactory.Load<JobSailing>(ExportSailing.PK);
			sailingInAnotherFactory.JX_DepotCutOff = ZDateTime.Now.AddDays(-2);
			sailingInAnotherFactory.Origin.JA_A_DEP = ZDateTime.Now;
			sailingInAnotherFactory.Destination.JB_A_ARV = ZDateTime.Now.AddDays(15);
			sailingInAnotherFactory.Voyage.JV_VoyageFlight = "Blah";

			parentMock.SetupProperty(m => m.Voyage, sailingInAnotherFactory.Voyage.JV_VoyageFlight);
			parentMock.SetupProperty(m => m.Vessel, sailingInAnotherFactory.Voyage.JV_RV_NKVessel);
			parentMock.SetupProperty(m => m.RegistrationNo, sailingInAnotherFactory.Voyage.JV_RegistrationNo);
			parentMock.SetupProperty(m => m.IsCargoOnly, sailingInAnotherFactory.Voyage.JV_IsCargoOnly);

			parentMock.SetupProperty(m => m.Load, sailingInAnotherFactory.Origin.JA_RL_NKPortOfLoading);
			parentMock.SetupProperty(m => m.ETD, sailingInAnotherFactory.Origin.JA_E_DEP);
			parentMock.SetupProperty(m => m.ATD, sailingInAnotherFactory.Origin.JA_A_DEP);
			parentMock.SetupProperty(m => m.Discharge, sailingInAnotherFactory.Destination.JB_RL_NKPortOfDischarge);
			parentMock.SetupProperty(m => m.ETA, sailingInAnotherFactory.Destination.JB_E_ARV);
			parentMock.SetupProperty(m => m.ATA, sailingInAnotherFactory.Destination.JB_A_ARV);
			parentMock.Setup(m => m.SetFCLReceivalCommences(sailingInAnotherFactory.Origin.JA_ReceivalCommences));
			parentMock.Setup(m => m.SetLCLReceivalCommences(sailingInAnotherFactory.JX_DepotReceivalCommences));
			parentMock.Setup(m => m.SetFCLCutOff(sailingInAnotherFactory.Origin.JA_CutOff));
			parentMock.Setup(m => m.SetLCLCutOff(sailingInAnotherFactory.JX_DepotCutOff));
			parentMock.Setup(m => m.SetAvailabilityDate(sailingInAnotherFactory.Destination.JB_AvailabilityDate));
			parentMock.Setup(m => m.SetLCLAvailabilityDate(sailingInAnotherFactory.JX_DepotAvailabilityDate));
			parentMock.Setup(m => m.SetStorageDate(sailingInAnotherFactory.Destination.JB_StorageDate));
			parentMock.Setup(m => m.SetLCLStorageDate(sailingInAnotherFactory.JX_DepotStorageDate));
			parentMock.Setup(m => m.SetDocsCutOff(sailingInAnotherFactory.Origin.JA_DocumentaryCutoff));
			parentMock.Setup(m => m.SetVGMCutOff(sailingInAnotherFactory.Origin.JA_VGMCutOff));
			parentMock.Setup(m => m.SetOA_DepartureLocation(sailingInAnotherFactory.Origin.JA_OA_DepartureCTOAddress));
			parentMock.Setup(m => m.SetOA_ArrivalLocation(sailingInAnotherFactory.Destination.JB_OA_ArrivalCTOAddress));
			parentMock.Setup(m => m.SetEmptyCutOff(sailingInAnotherFactory.Origin.JA_EmptyCutOff));
			parentMock.Setup(m => m.SetEmptyReceivalCommences(sailingInAnotherFactory.Origin.JA_EmptyReceivalCommences));
			parentMock.Setup(m => m.SetDGCutOff(sailingInAnotherFactory.Origin.JA_DGCutOff));
			parentMock.Setup(m => m.SetDGReceivalCommences(sailingInAnotherFactory.Origin.JA_DGReceivalCommences));
			parentMock.Setup(m => m.SetReeferCutOff(sailingInAnotherFactory.Origin.JA_ReeferCutOff));
			parentMock.Setup(m => m.SetReeferReceivalCommences(sailingInAnotherFactory.Origin.JA_ReeferReceivalCommences));

			anotherFactory.Save();
			parentMock.VerifyAll();

			// expect no exception
			manager.Sailing = null;
			sailingInAnotherFactory.Origin.JA_ReceivalCommences = ZDateTime.Now;
			anotherFactory.Save();
		}

		[ExpectNoExceptions]
		public void TestSyncParentOnDataRefresh_WhenSailingManagerSailingIsNull()
		{
			ExportSailing.Factory.Save();

			var parentMock = new Mock<ISailingManaged>();
			var parent = parentMock.Object;

			parentMock.Setup(m => m.TransportMode).Returns(ZString.Empty);
			BaseSailingManager manager = BaseSailingManager.New(parent);
			parentMock.VerifyAll();
			parentMock.Reset();
			manager.Sailing = ExportSailing;

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			JobSailing sailingInAnotherFactory = anotherFactory.Load<JobSailing>(ExportSailing.PK);
			sailingInAnotherFactory.Origin.JA_CutOff = ZDateTime.Now.AddDays(-2);
			sailingInAnotherFactory.Origin.JA_A_DEP = ZDateTime.Now;
			sailingInAnotherFactory.Destination.JB_A_ARV = ZDateTime.Now.AddDays(15);
			sailingInAnotherFactory.Voyage.JV_VoyageFlight = "Blah";

			manager.GetType().InvokeMember("sailing", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, manager, new object[] { null });
			anotherFactory.Save();
		}

		public void TestUnsavedSailingShouldBeDeletedWhenSailingChanged()
		{
			MockISailing mockSailing = new MockISailing(Factory);
			mockSailing.TransportMode = Core.Constants.TransportModes.Sea;
			BaseSailingManager manager = BaseSailingManager.New(mockSailing);

			manager.Sailing = ExportSailing;
			manager.Sailing = ExportSailing1;

			AssertEquals("Old Sailing Should be deleted", true, ExportSailing.IsDeleted);
		}

		public void TestResetOriginAndDestinationDetailsWhenChangingSailing_ChangeLoad()
		{
			GenericResetOriginAndDestinationDetailsWhenChangingSailingTest(true, false);
		}

		public void TestResetOriginAndDestinationDetailsWhenChangingSailing_ChangeDischarge()
		{
			GenericResetOriginAndDestinationDetailsWhenChangingSailingTest(false, true);
		}

		public void TestResetOriginAndDestinationDetailsWhenChangingSailing_ChangeLoadAndDischarge()
		{
			GenericResetOriginAndDestinationDetailsWhenChangingSailingTest(true, true);
		}

		public void TestResetOriginAndDestinationDetailsWhenChangingSailing_SameSailing()
		{
			GenericResetOriginAndDestinationDetailsWhenChangingSailingTest(false, false);
		}

		public void GenericResetOriginAndDestinationDetailsWhenChangingSailingTest(bool changeOrigin, bool changeDestination)
		{
			var today = ZDateTime.Today;
			var date1 = today.AddDays(-10);
			var date2 = today.AddDays(-5);
			var date3 = today.AddDays(5);
			var date4 = today.AddDays(10);

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "DODGY ARSE";

			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = OverseasPort;
			origin1.JA_E_DEP = date1;
			origin1.JA_S_DEP = date1.AddDays(1);

			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = OverseasPort2;
			origin2.JA_E_DEP = date2;
			origin2.JA_S_DEP = date2.AddDays(1);

			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = HomePort;
			destination1.JB_E_ARV = date3;
			destination1.JB_S_ARV = date3.AddDays(1);

			var destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = AlternateHomePort;
			destination2.JB_E_ARV = date4;
			destination2.JB_S_ARV = date4.AddDays(1);

			voyage.GenerateSailings();

			var sailing1 = voyage.Sailings.GetSailingFromLoadAndDischarge(OverseasPort, HomePort);

			var loadPort = (changeOrigin ? OverseasPort2 : OverseasPort);
			var dischargePort = (changeDestination ? AlternateHomePort : HomePort);

			var sailing2 = voyage.Sailings.GetSailingFromLoadAndDischarge(loadPort, dischargePort);

			Factory.Save();

			var mockSailing = new MockISailing(Factory);
			mockSailing.TransportMode = Core.Constants.TransportModes.Sea;
			var manager = BaseSailingManager.New(mockSailing);

			manager.Sailing = sailing1;
			sailing1.Origin.JA_E_DEP = today;
			sailing1.Origin.JA_S_DEP = today.AddDays(2);
			sailing1.Destination.JB_E_ARV = today;
			sailing1.Destination.JB_S_ARV = today.AddDays(3);

			manager.Sailing = sailing2;

			if (changeOrigin)
			{
				AssertEquals("The Origin has changed, should be reset", date1, origin1.JA_E_DEP);
				AssertEquals("The Origin has changed, should be reset", date1.AddDays(1), origin1.JA_S_DEP);
				AssertEquals("The Old Origin should not have changes", false, origin1.HasChanges);
			}
			else
			{
				AssertEquals("The Origin has not changed, leave alone", today, origin1.JA_E_DEP);
				AssertEquals("The Origin has not changed, leave alone", today.AddDays(2), origin1.JA_S_DEP);
				AssertEquals("The Origin should still have changes", true, origin1.HasChanges);
			}

			if (changeDestination)
			{
				AssertEquals("The Destination has changed, should be reset", date3, destination1.JB_E_ARV);
				AssertEquals("The Destination has changed, should be reset", date3.AddDays(1), destination1.JB_S_ARV);
				AssertEquals("The Old Destination should not have changes", false, destination1.HasChanges);
			}
			else
			{
				AssertEquals("The Destination has not changed, leave alone", today, destination1.JB_E_ARV);
				AssertEquals("The Destination has not changed, leave alone", today.AddDays(3), destination1.JB_S_ARV);
				AssertEquals("The Destination should still have changes", true, destination1.HasChanges);
			}
		}

		public void TestDeletedSailing()
		{
			MockISailing sailing = new MockISailing(Factory);

			sailing.TransportMode = Core.Constants.TransportModes.Sea;
			BaseSailingManager manager = BaseSailingManager.New(sailing);

			manager.Sailing = Factory.New<JobSailing>();
			manager.Sailing.Delete();
			AssertNull(manager.Sailing);
			manager.Sailing = Factory.New<JobSailing>();
			AssertNotNull(manager.Sailing);
		}

		public void TestSailingAndFriendsCorrectlyDeletedInNotifyRead()
		{
			MockISailing mockSailing = new MockISailing(Factory);
			mockSailing.TransportMode = Core.Constants.TransportModes.Sea;
			mockSailing.Load = OverseasPort;
			mockSailing.Discharge = HomePort;
			mockSailing.Vessel = TestVessel1.RV_Name;
			mockSailing.Voyage = "5057";

			BaseSailingManager manager = BaseSailingManager.New(mockSailing);
			manager.NotifyRead();

			JobSailing oldSailing = manager.Sailing;
			VoyageOrigin oldOrigin = manager.VoyOrigin;
			VoyageDestination oldDestination = manager.VoyDestination;
			JobVoyage oldVoyage = manager.Voyage;

			mockSailing.Voyage = "5030";
			manager.Dirty = true;
			manager.NotifyRead();

			AssertEquals("Old sailing should be deleted", true, oldSailing.IsDeleted);
			AssertEquals("Old origin should be deleted", true, oldOrigin.IsDeleted);
			AssertEquals("Old destination should be deleted", true, oldDestination.IsDeleted);
			AssertEquals("Old voyage should be deleted", true, oldVoyage.IsDeleted);
		}

		public void TestNew()
		{
			var parent = new MockISailing(Factory);
			parent.TransportMode = Constants.TransportModes.Sea;

			var manager = BaseSailingManager.New(parent);
			Assert("Sea requires a SeaSailingManager", manager is SeaSailingManager);
			AssertEquals("VoyageDirty must be true", true, manager.VoyageDirty);
			AssertEquals("VesselDirty must be true", true, manager.VesselDirty);
			AssertEquals("LoadDirty must be true", true, manager.LoadDirty);
			AssertEquals("DischargeDirty must be true", true, manager.DischargeDirty);
			AssertEquals("CarrierDirty must be true", true, manager.CarrierDirty);

			parent.TransportMode = Constants.TransportModes.Rail;
			manager = BaseSailingManager.New(parent);
			Assert("Rail requires a RailScheduleManager", manager is RailScheduleManager);
			AssertEquals("VoyageDirty must be true", true, manager.VoyageDirty);
			AssertEquals("VesselDirty must be true", true, manager.VesselDirty);
			AssertEquals("LoadDirty must be true", true, manager.LoadDirty);
			AssertEquals("DischargeDirty must be true", true, manager.DischargeDirty);

			parent.TransportMode = Constants.TransportModes.Air;
			manager = BaseSailingManager.New(parent);
			Assert("Air requires a ScheduleManager", manager is ScheduleManager);
			AssertEquals("VoyageDirty must be true", true, manager.VoyageDirty);
			AssertEquals("VesselDirty must be true", false, manager.VesselDirty);
			AssertEquals("LoadDirty must be true", true, manager.LoadDirty);
			AssertEquals("DischargeDirty must be true", true, manager.DischargeDirty);

			parent.TransportMode = Constants.TransportModes.Road;
			manager = BaseSailingManager.New(parent);
			Assert("Road requires a ScheduleManager", manager is ScheduleManager);
			AssertEquals("VoyageDirty must be false", true, manager.VoyageDirty);
			AssertEquals("VesselDirty must be true", false, manager.VesselDirty);
			AssertEquals("LoadDirty must be true", true, manager.LoadDirty);
			AssertEquals("DischargeDirty must be true", true, manager.DischargeDirty);
		}

		public void TestFindExisting()
		{
			ZGuid createdSailingPK = CreateSailingWithConsolAndGetPK();

			MockISailing mockSailing = new MockISailing(Factory);
			mockSailing.TransportMode = Core.Constants.TransportModes.Sea;
			mockSailing.Load = "AUSYD";
			mockSailing.Discharge = "AUMEL";
			mockSailing.Vessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_Name;
			mockSailing.Voyage = "VOYG";
			mockSailing.ETD = ZDateTime.Now;
			mockSailing.ETA = ZDateTime.Now;

			JobSailing foundSailing = new SeaSailingManager(mockSailing).FindExistingSailing();
			AssertEquals("Should find an existing sailing", foundSailing.PK, createdSailingPK);

			mockSailing.TransportMode = Core.Constants.TransportModes.Sea;
			mockSailing.Load = "AUBNE";
			mockSailing.Discharge = "HKHKG";
			mockSailing.Voyage = "TEST";

			JobSailing notFoundSailing = new SeaSailingManager(mockSailing).FindExistingSailing();
			AssertNull("Should return null because it can't find anything that matches", notFoundSailing);
		}

		public void TestSailingsExistOnReadAndSave()
		{
			MockISailing mockSailing1 = new MockISailing(Factory);
			mockSailing1.TransportMode = Core.Constants.TransportModes.Sea;
			mockSailing1.Load = HomePort;
			mockSailing1.Discharge = OverseasPort;
			mockSailing1.Vessel = TestVessel1.RV_Name;
			mockSailing1.Voyage = "snth";

			BaseSailingManager manager1 = BaseSailingManager.New(mockSailing1);
			manager1.Dirty = true;
			manager1.NotifyRead();

			MockISailing mockSailing2 = new MockISailing(Factory);
			mockSailing2.TransportMode = Core.Constants.TransportModes.Sea;
			mockSailing2.Load = HomePort;
			mockSailing2.Discharge = OverseasPort;
			mockSailing2.Vessel = TestVessel1.RV_Name;
			mockSailing2.Voyage = "snth";

			BaseSailingManager manager2 = BaseSailingManager.New(mockSailing2);
			manager2.Dirty = true;
			manager2.NotifyRead();

			AssertHasSailing("precondition:", mockSailing1);
			AssertHasSailing("precondition:", mockSailing2);

			mockSailing1.Load = OverseasPort;
			mockSailing1.Discharge = OverseasPort2;
			manager1.LoadDirty = true;
			manager1.DischargeDirty = true;
			manager1.NotifyRead();

			manager1.NotifySave();
			manager2.NotifySave();

			AssertHasSailing("MockSailing1 should have a reference to a real sailing before saving", mockSailing1);
			AssertHasSailing("MockSailing2 should have a reference to a real sailing before saving", mockSailing2);
		}

		public void TestAllowScheduleDatesChanging_ETD_ETA()
		{
			var today = ZDateTime.Today;
			var date1 = today.AddDays(-10);
			var date2 = today.AddDays(5);

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "VOYG";
			transport.JW_ETD = date1;
			transport.JW_ETA = date2;
			var sailing = transport.Sailing;

			var mockSailing = new MockISailing(Factory);
			mockSailing.TransportMode = Core.Constants.TransportModes.Sea;
			mockSailing.Load = "AUSYD";
			mockSailing.Discharge = "AUMEL";
			mockSailing.Vessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_Name;
			mockSailing.Voyage = "VOYG";
			mockSailing.ETD = date1;
			mockSailing.ETA = date2;

			var manager = new SeaSailingManager(mockSailing);

			var foundSailing = manager.FindExistingSailing();
			AssertEquals(sailing.PK, foundSailing.PK);
			AssertEquals(sailing.JX_JA_E_DEP, foundSailing.JX_JA_E_DEP);
			AssertEquals(sailing.JX_JB_E_ARV, foundSailing.JX_JB_E_ARV);

			mockSailing.AllowScheduleDatesChanging = false;
			mockSailing.ETD = today;
			mockSailing.ETA = today;
			manager.DepartureDirty = true;
			manager.ArrivalDirty = true;

			foundSailing = manager.FindExistingSailing();
			AssertEquals(sailing.PK, foundSailing.PK);
			AssertEquals(sailing.JX_JA_E_DEP, foundSailing.JX_JA_E_DEP);
			AssertEquals(sailing.JX_JB_E_ARV, foundSailing.JX_JB_E_ARV);

			mockSailing.AllowScheduleDatesChanging = true;
			mockSailing.ETD = today;
			mockSailing.ETA = today;
			manager.DepartureDirty = true;
			manager.ArrivalDirty = true;

			foundSailing = manager.FindExistingSailing();
			AssertEquals(sailing.PK, foundSailing.PK);
			AssertEquals(today, foundSailing.JX_JA_E_DEP);
			AssertEquals(today, foundSailing.JX_JB_E_ARV);
		}

		public void TestAllowScheduleDatesChanging_STD_STA()
		{
			var today = ZDateTime.Today;
			var date1 = today.AddDays(-10);
			var date2 = today.AddDays(5);

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "VOYG";
			transport.JW_STD = date1;
			transport.JW_STA = date2;
			var sailing = transport.Sailing;

			var mockSailing = new MockISailing(Factory);
			mockSailing.TransportMode = Core.Constants.TransportModes.Sea;
			mockSailing.Load = "AUSYD";
			mockSailing.Discharge = "AUMEL";
			mockSailing.Vessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_Name;
			mockSailing.Voyage = "VOYG";
			mockSailing.STD = date1;
			mockSailing.STA = date2;

			var manager = new SeaSailingManager(mockSailing);

			var foundSailing = manager.FindExistingSailing();
			AssertEquals(sailing.PK, foundSailing.PK);
			AssertEquals(sailing.JX_JA_S_DEP, foundSailing.JX_JA_S_DEP);
			AssertEquals(sailing.JX_JB_S_ARV, foundSailing.JX_JB_S_ARV);

			mockSailing.AllowScheduleDatesChanging = false;
			mockSailing.STD = today;
			mockSailing.STA = today;
			manager.DepartureDirty = true;
			manager.ArrivalDirty = true;

			foundSailing = manager.FindExistingSailing();
			AssertEquals(sailing.PK, foundSailing.PK);
			AssertEquals(sailing.JX_JA_S_DEP, foundSailing.JX_JA_S_DEP);
			AssertEquals(sailing.JX_JB_S_ARV, foundSailing.JX_JB_S_ARV);

			mockSailing.AllowScheduleDatesChanging = true;
			mockSailing.STD = today;
			mockSailing.STA = today;
			manager.DepartureDirty = true;
			manager.ArrivalDirty = true;

			foundSailing = manager.FindExistingSailing();
			AssertEquals(sailing.PK, foundSailing.PK);
			AssertEquals(today, foundSailing.JX_JA_S_DEP);
			AssertEquals(today, foundSailing.JX_JB_S_ARV);

			manager.DepartureDirty = false;
			manager.ArrivalDirty = false;
			mockSailing.STD = date1;
			mockSailing.STA = date2;
			foundSailing = manager.FindExistingSailing();
			AssertEquals(sailing.PK, foundSailing.PK);
			AssertEquals(today, foundSailing.JX_JA_S_DEP);
			AssertEquals(today, foundSailing.JX_JB_S_ARV);
		}

		public void TestAllowScheduleDatesChanging_STD_STA_EmptyDate()
		{
			var today = ZDateTime.Today;

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "VOYG";
			transport.JW_STD = today;
			transport.JW_STA = today;
			var sailing = transport.Sailing;

			var mockSailing = new MockISailing(Factory);
			mockSailing.TransportMode = Core.Constants.TransportModes.Sea;
			mockSailing.Load = "AUSYD";
			mockSailing.Discharge = "AUMEL";
			mockSailing.Vessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_Name;
			mockSailing.Voyage = "VOYG";
			mockSailing.STD = ZDateTime.Empty;
			mockSailing.STA = ZDateTime.Empty;

			var manager = new SeaSailingManager(mockSailing);

			mockSailing.AllowScheduleDatesChanging = true;
			manager.DepartureDirty = true;
			manager.ArrivalDirty = true;

			var foundSailing = manager.FindExistingSailing();
			AssertEquals(sailing.PK, foundSailing.PK);
			AssertEquals(today, foundSailing.JX_JA_S_DEP);
			AssertEquals(today, foundSailing.JX_JB_S_ARV);
			AssertEquals(sailing.JX_JA_S_DEP, foundSailing.JX_JA_S_DEP);
			AssertEquals(sailing.JX_JB_S_ARV, foundSailing.JX_JB_S_ARV);
		}

		public void TestHookAdditionalValidation()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			var transport = consol.Transports[0];

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VESSEL 111";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "AIR";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "111S";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSMV";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "USLAX";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "CNSHA";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			var sailing1 = voyage.Sailings[1];

			var mockSailing = new MockISailing(Factory);
			mockSailing.SailingAdditionalValidation = new TransportSailingAdditionalValidation(sailing, transport);
			mockSailing.VoyOriginAdditionalValidation = new TransportVoyOriginAdditionalValidation(sailing.Origin, transport);
			mockSailing.VoyDestinationAdditionalValidation = new TransportVoyDestinationAdditionalValidation(sailing.Destination, transport);

			AssertNull(sailing.AdditionalValidation);
			AssertNull(sailing.Origin.AdditionalValidation);
			AssertNull(sailing.Destination.AdditionalValidation);

			var manager = new SeaSailingManager(mockSailing);
			manager.Sailing = sailing;

			AssertNotNull(sailing.AdditionalValidation);
			AssertNotNull(sailing.Origin.AdditionalValidation);
			AssertNotNull(sailing.Destination.AdditionalValidation);

			manager.Sailing = sailing1;
			Assert(sailing.IsDeleted);
		}

		public void TestResetSailing()
		{
			var parent = new MockISailing(Factory);
			parent.TransportMode = Constants.TransportModes.Sea;
			parent.Vessel = "CONDOR";
			parent.Voyage = "012";
			parent.Load = "AUBNE";
			parent.Discharge = "SGSIN";
			parent.AllowScheduleCreation = true;

			var manager = BaseSailingManager.New(parent);
			manager.NotifyRead();
			AssertNotNull("Should create a new voyage.", manager.Voyage);
			AssertNotNull("Should create a new voyage origin.", manager.VoyOrigin);
			AssertNotNull("Should create a new voyage destination.", manager.VoyDestination);
			AssertNotNull("Should create a new sailing.", manager.Sailing);
			AssertEquals(false, manager.Voyage.IsDeleted);
			AssertEquals(false, manager.VoyOrigin.IsDeleted);
			AssertEquals(false, manager.VoyDestination.IsDeleted);
			AssertEquals(false, manager.Sailing.IsDeleted);

			var voyage = manager.Voyage;
			var voyOrigin = manager.VoyOrigin;
			var voyDestination = manager.VoyDestination;
			var sailing = manager.Sailing;

			manager.ResetSailing();
			AssertNull("Should reset voyage.", manager.Voyage);
			AssertNull("Should reset voyage origin.", manager.VoyOrigin);
			AssertNull("Should reset voyage destination.", manager.VoyDestination);
			AssertNull("Should reset sailing.", manager.Sailing);
			AssertEquals("Should delete voyage.", true, voyage.IsDeleted);
			AssertEquals("Should create voyage origin.", true, voyOrigin.IsDeleted);
			AssertEquals("Should create voyage destination.", true, voyDestination.IsDeleted);
			AssertEquals("Should create sailing.", true, sailing.IsDeleted);
		}

		public void TestReleaseVoyage_ShouldDeleteLogsNotInDB()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("CONDOR", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "012";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			Factory.Save();

			var parent = new MockISailing(Factory);
			parent.TransportMode = Constants.TransportModes.Sea;
			parent.Vessel = "CONDOR";
			parent.Voyage = "012";
			parent.Load = "AUBNE";
			parent.Discharge = "SGSIN";
			parent.AllowScheduleCreation = true;

			var manager = BaseSailingManager.New(parent);
			manager.NotifyRead();
			AssertEquals("Should find a matching voyage.", voyage, manager.Voyage);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			voyage.Logs.AddNew(AutoEvents.EditedARecord, "Dummy log");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			AssertEquals(1, voyage.Logs.LogsNotInDB.Length);
			AssertEquals(true, voyage.HasChanges);

			manager.ResetSailing();
			AssertEquals(0, voyage.Logs.LogsNotInDB.Length);
			AssertEquals(false, voyage.HasChanges);
		}

		#region Implementation

		static int CountNewObjects<T>(BusinessObjectFactory factory)
			where T : BusinessObject
		{
			T[] found = factory.Load<T>(new ZQuery() { FetchOnlyFromLocalCache = true });

			int result = 0;

			for (int i = 0; i < found.Length; i++)
			{
				if (!found[i].IsInDatabase)
				{
					result++;
				}
			}

			return result;
		}

		ZGuid CreateSailingWithConsolAndGetPK()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "VOYG";
			transport.JW_ETD = ZDateTime.Now;
			transport.JW_ETA = ZDateTime.Now;
			return transport.Sailing.PK;
		}

		void AssertHasSailing(string message, ISailingManaged parent)
		{
			AssertNotNull(message, parent.Factory.Load(typeof(JobSailing), parent.SailingPK));
		}

		#endregion
	}
}
