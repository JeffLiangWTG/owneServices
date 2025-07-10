using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class SeaSailingManagerTest : BaseFreightTest
	{
		public void TestHasSufficientInformation()
		{
			var parent = new MockISailing(Factory);
			parent.Load = "AAA";
			parent.Discharge = "BBB";
			parent.Vessel = "CCC";
			parent.Voyage = "DDD";
			parent.ShippingLine = ZGuid.Empty;
			parent.TransportMode = Constants.TransportModes.Rail;

			var manager = new SeaSailingManager(parent);
			Assert(!manager.HasSufficientInformation);

			parent.TransportMode = Constants.TransportModes.Sea;
			Assert(manager.HasSufficientInformation);

			parent.Voyage = "";
			Assert(!manager.HasSufficientInformation);
		}

		public void TestGetExistingVoyage()
		{
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_Code = "CARRIER1";
			carrier1.OH_IsShippingLine = true;
			carrier1.OH_IsShippingProvider = true;

			var carrier2 = Factory.New<OrgHeader>();
			carrier2.OH_Code = "CARRIER2";
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_IsShippingProvider = true;

			var helper = new VoyageTestHelper(Factory);
			var voyage1 = helper.CreateSeaVoyage("Visund", "123", carrier1.PK, "AUSYD", "AUMEL");
			var voyage2 = helper.CreateSeaVoyage("Visund", "123", carrier2.PK, "AUSYD", "AUMEL");

			var voyage3 = helper.CreateSeaVoyage("Kon-Tiki", "123", carrier1.PK, "AUSYD", "AUMEL");
			var voyage4 = helper.CreateSeaVoyage("Visund", "456", carrier1.PK, "AUSYD", "AUMEL");

			Factory.Save();

			var parent = new MockISailing(Factory);
			parent.TransportMode = Constants.TransportModes.Sea;
			parent.Load = "AUSYD";
			parent.Discharge = "AUMEL";
			parent.Vessel = "Visund";
			parent.Voyage = "123";
			parent.ShippingLine = carrier1.PK;

			var manager = new SeaSailingManager(parent);

			voyage1.JV_IsActive = false;
			manager.Dirty = true;
			manager.NotifyRead();
			AssertEquals("No active voyages matching", false, manager.Voyage.IsInDatabase);

			voyage1.JV_IsActive = true;
			manager.Dirty = true;
			manager.NotifyRead();
			AssertEquals("Voyage was matched by vessel-number-carrier", voyage1.PK, manager.Voyage.PK);

			manager.Dirty = true;
			manager.NotifyRead();
			AssertEquals("Voyage was matched by vessel-number-carrier", voyage1.PK, manager.Voyage.PK);

			parent.ShippingLine = carrier2.PK;
			manager.Dirty = true;
			manager.NotifyRead();
			AssertEquals("Voyage was matched by vessel-number-carrier", voyage2.PK, manager.Voyage.PK);

			parent.ShippingLine = ZGuid.NewZGuid();
			manager.Dirty = true;
			manager.NotifyRead();
			AssertEquals("Existing voyages was not matched by vessel-number-carrier", false, manager.Voyage.IsInDatabase);

			voyage1.JV_VoyageType = Constants.VoyageType.MainVoyage;
			voyage2.JV_VoyageType = Constants.VoyageType.SlotVoyage;

			parent.ShippingLine = ZGuid.Empty;

			manager.Sailing = null;
			manager.Dirty = true;
			manager.NotifyRead();
			AssertEquals("When parent has no carrier specified, main voyage take priority", voyage1.PK, manager.Voyage.PK);

			voyage1.JV_VoyageType = Constants.VoyageType.SlotVoyage;
			voyage2.JV_VoyageType = Constants.VoyageType.MainVoyage;

			manager.Dirty = true;
			manager.NotifyRead();
			AssertEquals("When parent has no carrier specified, main voyage take priority", voyage2.PK, manager.Voyage.PK);

			voyage1.JV_VoyageType = "";
			voyage2.JV_VoyageType = "";
			manager.Dirty = true;
			manager.NotifyRead();
			AssertEquals("When parent has no carrier specified and there is no main voyage, first found would be returned",
				true,
				manager.Voyage.PK == voyage1.PK || manager.Voyage.PK == voyage2.PK);
		}

		public void TestNewVoyage()
		{
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_Code = "CARRIER1";
			carrier1.OH_IsShippingLine = true;
			carrier1.OH_IsShippingProvider = true;

			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Name = "Visund";

			var parent = new MockISailing(Factory);
			parent.TransportMode = Constants.TransportModes.Sea;
			parent.Load = "AUSYD";
			parent.Discharge = "AUMEL";
			parent.Vessel = "Visund";
			parent.Voyage = "123";
			parent.IsCharter = false;
			parent.ShippingLine = carrier1.PK;

			var manager = new SeaSailingManager(parent);
			manager.Dirty = true;
			manager.NotifyRead();

			AssertEquals("New voyage was created", false, manager.Voyage.IsInDatabase);
			AssertEquals(Constants.TransportModes.Sea, manager.Voyage.JV_AirSeaRoad);
			AssertEquals("123", manager.Voyage.JV_VoyageFlight);
			AssertEquals("Visund", manager.Voyage.JV_RV_NKVessel);
			AssertEquals(false, manager.Voyage.JV_IsChartered);
			AssertEquals(carrier1.PK, manager.Voyage.JV_OH_Line);
			AssertEquals(Constants.VoyageType.MainVoyage, manager.Voyage.JV_VoyageType);

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "Asgard";
			vessel2.RV_OH = Factory.New<OrgHeader>().PK;

			parent.Vessel = "Asgard";
			manager.Dirty = true;
			manager.NotifyRead();

			AssertEquals(Constants.VoyageType.SlotVoyage, manager.Voyage.JV_VoyageType);
		}

		public void TestNewVoyage_VoyageTypeDefaulting()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Name = "Visund";

			var parent = new MockISailing(Factory);
			parent.TransportMode = Constants.TransportModes.Sea;
			parent.Load = "AUSYD";
			parent.Discharge = "AUMEL";
			parent.Vessel = "Visund";
			parent.Voyage = "123";
			parent.IsCharter = false;

			var manager = new SeaSailingManager(parent);
			manager.Dirty = true;
			manager.NotifyRead();

			AssertEquals("New voyage was created", false, manager.Voyage.IsInDatabase);
			AssertEquals("Voyage type defaulted to MAIN", true, manager.Voyage.IsMainVoyage);

			parent.ShippingLine = Factory.New<OrgHeader>().PK;
			manager.Dirty = true;
			manager.NotifyRead();

			AssertEquals("Voyage type defaulted to MAIN", true, manager.Voyage.IsMainVoyage);
		}

		public void TestShippingLineWouldBeSetOnParentFromVoyage()
		{
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_Code = "CARRIER1";
			carrier1.OH_IsShippingLine = true;
			carrier1.OH_IsShippingProvider = true;

			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Name = "Visund";

			var voyage = new VoyageTestHelper(Factory).CreateSeaVoyage("Visund", "123", carrier1.PK);

			var parent = new MockISailing(Factory);
			parent.TransportMode = Constants.TransportModes.Sea;
			parent.SailingPK = voyage.Sailings[0].PK;

			var manager = new SeaSailingManager(parent);
			manager.NotifyRead();

			AssertEquals("Parent's ShippingLine was set from voyage", carrier1.PK, parent.ShippingLine);
		}

		public void TestShouldUpdateETD()
		{
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_Code = "CARRIER1";
			carrier1.OH_IsShippingLine = true;
			carrier1.OH_IsShippingProvider = true;

			var voyage = new VoyageTestHelper(Factory).CreateSeaVoyage("Visund", "123", carrier1.PK, "AUSYD", "AUMEL");
			var origin = voyage.Origins.GetOriginFromLoading("AUSYD");

			ZDateTime originDate = ZDateTime.Now;
			origin.JA_E_DEP = originDate;

			var parent = new MockISailing(Factory);
			parent.TransportMode = Constants.TransportModes.Sea;
			parent.Load = "AUSYD";
			parent.Discharge = "AUMEL";
			parent.Vessel = "Visund";
			parent.Voyage = "123";
			parent.ShippingLine = carrier1.PK;
			parent.AllowScheduleDatesChanging = true;

			var manager = new SeaSailingManager(parent);
			manager.NotifyRead();

			AssertEquals("Precondition", false, manager.Dirty);
			AssertEquals("Precondition", originDate, origin.JA_E_DEP);

			ZDateTime newETD = ZDateTime.Now.AddDays(1);
			parent.ETD = newETD;
			manager.DepartureDirty = true;
			manager.NotifyRead();
			AssertEquals("Origin's ETD was updated from parent", newETD, origin.JA_E_DEP);
		}

		public void TestShouldUpdateETA()
		{
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_Code = "CARRIER1";
			carrier1.OH_IsShippingLine = true;
			carrier1.OH_IsShippingProvider = true;

			var voyage = new VoyageTestHelper(Factory).CreateSeaVoyage("Visund", "123", carrier1.PK, "AUSYD", "AUMEL");
			var destination = voyage.Destinations.GetDestinationFromDischarge("AUMEL");

			ZDateTime destinationDate = ZDateTime.Now;
			destination.JB_E_ARV = destinationDate;

			var parent = new MockISailing(Factory);
			parent.TransportMode = Constants.TransportModes.Sea;
			parent.Load = "AUSYD";
			parent.Discharge = "AUMEL";
			parent.Vessel = "Visund";
			parent.Voyage = "123";
			parent.ShippingLine = carrier1.PK;
			parent.AllowScheduleDatesChanging = true;

			var manager = new SeaSailingManager(parent);
			manager.NotifyRead();

			AssertEquals("Precondition", false, manager.Dirty);
			AssertEquals("Precondition", destinationDate, destination.JB_E_ARV);

			ZDateTime newETA = ZDateTime.Now.AddDays(1);
			parent.ETA = newETA;
			manager.ArrivalDirty = true;
			manager.NotifyRead();
			AssertEquals("Destination's ETA was updated from parent", newETA, destination.JB_E_ARV);
		}

		#region Legacy Tests

		public void TestCreateNewSailing()
		{
			MockISailing testObject = new MockISailing(Factory);
			var manager = new SeaSailingManager(testObject);

			testObject.Load = Origin;
			testObject.Discharge = Stop3;
			testObject.Vessel = Vessel1;
			testObject.Voyage = VoyageNumber1;
			testObject.ETA = Stop3Date;
			testObject.ETD = OriginDate;

			manager.LoadDirty = true;
			manager.DischargeDirty = true;
			manager.VesselDirty = true;
			manager.VoyageDirty = true;

			Assert("Pre-Condition TestObject SailingPK", testObject.SailingPK.IsEmpty);
			manager.NotifyRead();
			Assert("Should have created a new sailing", !testObject.SailingPK.IsEmpty);
			Factory.Save();

			testObject.Discharge = Stop4;
			testObject.ETA = Stop4Date;
			manager.DischargeDirty = true;
			manager.NotifyRead();
			var createdSailing = Factory.Load<JobSailing>(testObject.SailingPK);
			AssertEquals("New Sailing IsInDatabase", false, createdSailing.IsInDatabase);
			AssertEquals("Sailing Load", Origin, createdSailing.JX_JA_RL_NKPortOfLoading);
			AssertEquals("Sailing Discharge", Origin, createdSailing.JX_JA_RL_NKPortOfLoading);
			AssertEquals("Sailing Vessel", Vessel1, createdSailing.JX_JV_NKVessel);
			AssertEquals("Sailing VoyageNumber", VoyageNumber1, createdSailing.JX_JV_VoyageFlight);
			AssertEquals("Sailing DepartureDate", OriginDate, createdSailing.JX_JA_E_DEP);
			AssertEquals("Update Arrival Date", Stop4Date, createdSailing.JX_JB_E_ARV);
		}

		public void TestLoadExistingSailing()
		{
			ZGuid sailing1 = CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Stop1, Stop1Date);
			ZGuid sailing2 = CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Stop2, Stop2Date);
			ZGuid sailing3 = CreateSailing(Vessel1, VoyageNumber1, Stop1, Stop1Date, Stop3, Stop3Date);
			ZGuid sailing4 = CreateSailing(Vessel1, VoyageNumber1, Stop1, Stop1Date, Stop4, Stop4Date);
			ZGuid sailing5 = CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Stop5, Stop5Date);
			ZGuid sailing6 = CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Stop6, Stop6Date);
			ZGuid sailing7 = CreateSailing(Vessel1, VoyageNumber1, Stop1, Stop1Date, Stop6, Stop6Date);
			ZGuid sailing8 = CreateSailing(Vessel1, VoyageNumber1, Stop2, Stop2Date, Stop6, Stop6Date);
			ZGuid sailing9 = CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Destination, DestinationDate);

			MockISailing mockConsol = new MockISailing(Factory);
			mockConsol.Vessel = Vessel1;
			mockConsol.Voyage = VoyageNumber1;
			var manager = new SeaSailingManager(mockConsol);
			mockConsol.Discharge = Stop6;
			manager.DischargeDirty = true;
			mockConsol.Load = Stop1;
			manager.LoadDirty = true;
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

		public void TestLoadExistingVoyage_VoyageExists_UpdateIsCharterProperty()
		{
			var sailingPK = CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Stop1, Stop1Date);

			var mockConsol = new MockISailing(Factory);
			mockConsol.Vessel = Vessel1;
			mockConsol.Voyage = VoyageNumber1;
			mockConsol.Load = Origin;
			mockConsol.Discharge = Stop1;
			mockConsol.IsCharter = true;

			var managerToTest = new SeaSailingManager(mockConsol);
			managerToTest.Dirty = true;
			managerToTest.NotifyRead();

			var sailing = Factory.Load<JobSailing>(mockConsol.SailingPK);
			AssertEquals("IsChartered is updated", true, sailing.JX_JV_IsChartered);
		}

		public void TestConcurrencyFailureWithSailing()
		{
			MockISailing mockConsol = new MockISailing(Factory);
			mockConsol.TransportMode = Constants.TransportModes.Sea;
			mockConsol.Vessel = Vessel1;
			mockConsol.Voyage = VoyageNumber1;
			mockConsol.Load = Stop1;
			mockConsol.ETD = Stop1Date;
			mockConsol.Discharge = Stop6;
			mockConsol.ETA = Stop5Date;
			var manager = new SeaSailingManager(mockConsol);
			manager.ArrivalDirty = true;
			manager.DischargeDirty = true;
			manager.VesselDirty = true;
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

		public void TestVoyageMatching()
		{
			MockISailing mockConsol = new MockISailing(Factory);
			mockConsol.Vessel = Vessel2;
			mockConsol.ETD = OriginDate;
			mockConsol.Load = Origin;
			mockConsol.Discharge = Destination;
			mockConsol.ETA = DestinationDate;
			mockConsol.Voyage = VoyageNumber2;

			var manager1 = new SeaSailingManager(mockConsol);
			manager1.ArrivalDirty = true;
			manager1.DepartureDirty = true;
			manager1.LoadDirty = true;
			manager1.DischargeDirty = true;
			manager1.VesselDirty = true;
			manager1.VoyageDirty = true;
			manager1.NotifyRead();
			manager1.NotifySave();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			MockISailing unmatchingMockConsol = new MockISailing(secondFactory);
			unmatchingMockConsol.Vessel = Vessel2;
			unmatchingMockConsol.ETD = OriginDate;
			unmatchingMockConsol.Load = Origin;
			unmatchingMockConsol.Discharge = Destination;
			unmatchingMockConsol.ETA = DestinationDate;
			unmatchingMockConsol.Voyage = AltVoyageNumber2;
			var manager2 = new SeaSailingManager(unmatchingMockConsol);

			MockISailing matchingMockConsol = new MockISailing(secondFactory);
			matchingMockConsol.Vessel = Vessel2;
			matchingMockConsol.ETD = OriginDate;
			matchingMockConsol.Load = Origin;
			matchingMockConsol.Discharge = Destination;
			matchingMockConsol.ETA = DestinationDate;
			matchingMockConsol.Voyage = VoyageNumber2;
			var manager3 = new SeaSailingManager(matchingMockConsol);

			Factory.Save();

			manager2.ArrivalDirty = true;
			manager2.DepartureDirty = true;
			manager2.LoadDirty = true;
			manager2.DischargeDirty = true;
			manager2.VesselDirty = true;
			manager2.VoyageDirty = true;
			manager2.NotifyRead();
			AssertNotEquals("Failed to find sailing for MockConsol1", ZGuid.Empty, mockConsol.SailingPK);
			AssertNotEquals("Failed to find sailing for MockConsol2", ZGuid.Empty, unmatchingMockConsol.SailingPK);
			AssertNotEquals("014 and 14 are not the same voyages and should not be matched.", mockConsol.SailingPK, unmatchingMockConsol.SailingPK);

			manager3.ArrivalDirty = true;
			manager3.DepartureDirty = true;
			manager3.LoadDirty = true;
			manager3.DischargeDirty = true;
			manager3.VesselDirty = true;
			manager3.VoyageDirty = true;
			manager3.NotifyRead();
			AssertEquals("Should match the following sailing to the consols. Voyage number must be unique for each vessel", mockConsol.SailingPK, matchingMockConsol.SailingPK);
		}

		#region Implementation

		const string Origin = "CNSHA";
		const string Stop1 = "SGSIN";
		const string Stop2 = "HKHKG";
		const string Stop3 = "AUBNE";
		const string Stop4 = "AUSYD";
		const string Stop5 = "AUMEL";
		const string Stop6 = "NKALK";
		const string Destination = "USLAX";

		const string Vessel1 = "SOUTHERN CROSS MARU";
		const string Vessel2 = "APL IVORY";
		const string VoyageNumber1 = "415";
		const string VoyageNumber2 = "14";
		const string AltVoyageNumber2 = "014";

		readonly ZDateTime OriginDate = new ZDateTime(2004, 09, 23);
		readonly ZDateTime Stop1Date = new ZDateTime(2004, 09, 26);
		readonly ZDateTime Stop2Date = new ZDateTime(2004, 09, 29);
		readonly ZDateTime Stop3Date = new ZDateTime(2004, 10, 03);
		readonly ZDateTime Stop4Date = new ZDateTime(2004, 10, 05);
		readonly ZDateTime Stop5Date = new ZDateTime(2004, 10, 08);
		readonly ZDateTime Stop6Date = new ZDateTime(2004, 10, 15);
		readonly ZDateTime DestinationDate = new ZDateTime(2004, 11, 05);

		void AddTestSailingsToDB()
		{
			CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Stop1, Stop1Date);
			CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Stop2, Stop2Date);
			CreateSailing(Vessel1, VoyageNumber1, Stop1, Stop1Date, Stop3, Stop3Date);
			CreateSailing(Vessel1, VoyageNumber1, Stop1, Stop1Date, Stop4, Stop4Date);
			CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Stop5, Stop5Date);
			CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Stop6, Stop6Date);
			CreateSailing(Vessel1, VoyageNumber1, Stop1, Stop1Date, Stop6, Stop6Date);
			CreateSailing(Vessel1, VoyageNumber1, Stop2, Stop2Date, Stop6, Stop6Date);
			CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Destination, DestinationDate);
		}

		ZGuid CreateSailing(ZString vessel, ZString voyageNumber, ZString originPort, ZDateTime departureDate, ZString destinationPort, ZDateTime arrivalDate)
		{
			BusinessObjectFactory noDBRefreshFactory = new BusinessObjectFactory();
			noDBRefreshFactory.RefreshEnabled = false;
			JobSailing sailing = noDBRefreshFactory.New<JobSailing>();

			ZQuery voyageFilter = new ZQuery(JobVoyageSchema.JV_VoyageFlight, voyageNumber);
			voyageFilter.AddToFilter(new ZQuery(JobVoyageSchema.JV_RV_NKVessel, vessel));
			voyageFilter.AddToFilter(new ZQuery(JobVoyageSchema.JV_AirSeaRoad, Constants.TransportModes.Sea));

			JobVoyage voyage = noDBRefreshFactory.LoadTop1<JobVoyage>(voyageFilter);
			if (voyage == null)
			{
				voyage = noDBRefreshFactory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
				voyage.JV_RV_NKVessel = vessel;
				voyage.JV_VoyageFlight = voyageNumber;
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

		#endregion
	}
}
