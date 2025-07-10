using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class RailScheduleManagerTest : BaseFreightTest
	{
		public void TestCreateNewSailing()
		{
			MockISailing testObject = new MockISailing(Factory);
			var manager = new RailScheduleManager(testObject);

			testObject.TransportMode = Constants.TransportModes.Rail;
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
			mockConsol.TransportMode = Constants.TransportModes.Rail;
			mockConsol.Vessel = Vessel1;
			mockConsol.Voyage = VoyageNumber1;

			var manager = new RailScheduleManager(mockConsol);
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

		public void TestConcurrencyFailureWithSailing()
		{
			MockISailing mockConsol = new MockISailing(Factory);
			mockConsol.TransportMode = Constants.TransportModes.Rail;
			mockConsol.Vessel = Vessel1;
			mockConsol.Voyage = VoyageNumber1;
			mockConsol.Load = Stop1;
			mockConsol.ETD = Stop1Date;
			mockConsol.Discharge = Stop6;
			mockConsol.ETA = Stop5Date;
			var manager = new RailScheduleManager(mockConsol);
			manager.ArrivalDirty = true;
			manager.DischargeDirty = true;
			manager.VesselDirty = true;
			manager.NotifyRead();
			var createdSailing = Factory.Load<JobSailing>(mockConsol.SailingPK);
			AssertEquals("Created Sailing In Database", false, createdSailing.IsInDatabase);
			ZGuid createdSailingPK = createdSailing.PK;

			AddTestSailingsToDB(Constants.TransportModes.Rail);

			manager.NotifySave();
			Factory.Save();

			var savedSailing = Factory.Load<JobSailing>(mockConsol.SailingPK);
			Assert("Concurrency Problem should not have occured", savedSailing.PK != createdSailingPK);
		}

		public void TestNotifySaveShouldBeAwareOfTransportModes()
		{
			MockISailing mockConsol = new MockISailing(Factory);
			mockConsol.TransportMode = Constants.TransportModes.Rail;
			mockConsol.Vessel = Vessel1;
			mockConsol.Voyage = VoyageNumber1;
			mockConsol.Load = Stop1;
			mockConsol.ETD = Stop1Date;
			mockConsol.Discharge = Stop6;
			mockConsol.ETA = Stop5Date;

			var manager = new RailScheduleManager(mockConsol);
			manager.ArrivalDirty = true;
			manager.DischargeDirty = true;
			manager.VesselDirty = true;
			manager.NotifyRead();

			var createdSailing = Factory.Load<JobSailing>(mockConsol.SailingPK);
			AssertEquals("Created Sailing In Database", false, createdSailing.IsInDatabase);
			ZGuid createdSailingPK = createdSailing.PK;

			AddTestSailingsToDB(Constants.TransportModes.Sea);

			manager.NotifySave();
			Factory.Save();

			var savedSailing = Factory.Load<JobSailing>(mockConsol.SailingPK);
			AssertEquals("Should have chosen a rail voyage", Constants.TransportModes.Rail, savedSailing.Voyage.JV_AirSeaRoad);
			AssertEquals("none of the created voyages were rail so we shold still have the created voyage", savedSailing.PK, createdSailingPK);
		}

		public void TestEnteredShippingLineKept()
		{
			ZGuid sailing1 = CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Stop1, Stop1Date);
			ZGuid sailing2 = CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Stop2, Stop2Date);
			ZGuid sailing3 = CreateSailing(Vessel2, VoyageNumber2, Origin, OriginDate, Stop2, Stop2Date);
			ZGuid shippingLine1 = GetShippingLine().PK;
			ZGuid shippingLine2 = GetShippingLine().PK;
			ZGuid shippingLine3 = GetShippingLine().PK;
			SetShippingLineOnSailing(shippingLine1, sailing1);
			SetShippingLineOnSailing(shippingLine3, sailing3);

			MockISailing mockConsol = new MockISailing(Factory);
			mockConsol.TransportMode = Constants.TransportModes.Rail;
			mockConsol.Vessel = Vessel1;
			mockConsol.Voyage = VoyageNumber1;
			mockConsol.Load = Origin;
			mockConsol.ETD = Stop1Date;
			mockConsol.Discharge = Stop1;
			mockConsol.ETA = Stop5Date;

			var manager = new RailScheduleManager(mockConsol);
			manager.ArrivalDirty = true;
			manager.DischargeDirty = true;
			manager.VesselDirty = true;
			manager.NotifyRead();
			AssertEquals("Shipping Line Should have Defaulted", shippingLine1, mockConsol.ShippingLine);
			mockConsol.ShippingLine = shippingLine2;
			mockConsol.Discharge = Stop2;
			manager.DischargeDirty = true;
			manager.NotifyRead();
			AssertEquals("Shipping Line Should Not Default as it was already set", shippingLine2, mockConsol.ShippingLine);
			mockConsol.Vessel = Vessel2;
			mockConsol.Voyage = VoyageNumber2;
			manager.VoyageDirty = true;
			manager.NotifyRead();
			AssertEquals("Shipping Line on MockConsol should have updated as voyage changed", shippingLine3, mockConsol.ShippingLine);
		}

		public void TestVoyageMatching()
		{
			MockISailing mockConsol = new MockISailing(Factory);
			mockConsol.TransportMode = Constants.TransportModes.Rail;
			mockConsol.Vessel = Vessel2;
			mockConsol.ETD = OriginDate;
			mockConsol.Load = Origin;
			mockConsol.Discharge = Destination;
			mockConsol.ETA = DestinationDate;
			mockConsol.Voyage = VoyageNumber2;

			var manager1 = new RailScheduleManager(mockConsol);
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
			unmatchingMockConsol.TransportMode = Constants.TransportModes.Rail;
			unmatchingMockConsol.Vessel = Vessel2;
			unmatchingMockConsol.ETD = OriginDate;
			unmatchingMockConsol.Load = Origin;
			unmatchingMockConsol.Discharge = Destination;
			unmatchingMockConsol.ETA = DestinationDate;
			unmatchingMockConsol.Voyage = AltVoyageNumber2;
			var manager2 = new RailScheduleManager(unmatchingMockConsol);

			MockISailing matchingMockConsol = new MockISailing(secondFactory);
			matchingMockConsol.TransportMode = Constants.TransportModes.Rail;
			matchingMockConsol.Vessel = Vessel2;
			matchingMockConsol.ETD = OriginDate;
			matchingMockConsol.Load = Origin;
			matchingMockConsol.Discharge = Destination;
			matchingMockConsol.ETA = DestinationDate;
			matchingMockConsol.Voyage = VoyageNumber2;
			var manager3 = new RailScheduleManager(matchingMockConsol);

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

		void AddTestSailingsToDB(string transportMode)
		{
			CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Stop1, Stop1Date, transportMode);
			CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Stop2, Stop2Date, transportMode);
			CreateSailing(Vessel1, VoyageNumber1, Stop1, Stop1Date, Stop3, Stop3Date, transportMode);
			CreateSailing(Vessel1, VoyageNumber1, Stop1, Stop1Date, Stop4, Stop4Date, transportMode);
			CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Stop5, Stop5Date, transportMode);
			CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Stop6, Stop6Date, transportMode);
			CreateSailing(Vessel1, VoyageNumber1, Stop1, Stop1Date, Stop6, Stop6Date, transportMode);
			CreateSailing(Vessel1, VoyageNumber1, Stop2, Stop2Date, Stop6, Stop6Date, transportMode);
			CreateSailing(Vessel1, VoyageNumber1, Origin, OriginDate, Destination, DestinationDate, transportMode);
		}

		ZGuid CreateSailing(ZString vessel, ZString voyageNumber, ZString originPort, ZDateTime departureDate, ZString destinationPort, ZDateTime arrivalDate, string transportMode = Constants.TransportModes.Rail)
		{
			BusinessObjectFactory noDBRefreshFactory = new BusinessObjectFactory();
			noDBRefreshFactory.RefreshEnabled = false;
			JobSailing sailing = noDBRefreshFactory.New<JobSailing>();

			ZQuery voyageFilter = new ZQuery(JobVoyageSchema.JV_VoyageFlight, voyageNumber);
			voyageFilter.AddToFilter(new ZQuery(JobVoyageSchema.JV_RV_NKVessel, vessel));
			voyageFilter.AddToFilter(new ZQuery(JobVoyageSchema.JV_AirSeaRoad, transportMode));

			JobVoyage voyage = noDBRefreshFactory.LoadTop1<JobVoyage>(voyageFilter);
			if (voyage == null)
			{
				voyage = noDBRefreshFactory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = transportMode;
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

		void SetShippingLineOnSailing(ZGuid shippingLine, ZGuid sailingPK)
		{
			var result = Factory.Load<JobSailing>(sailingPK);
			result.Voyage.JV_OH_Line = shippingLine;
		}

		OrgHeader GetShippingLine()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_IsShippingLine = true;
			result.OH_FullName = "Shipping Line";
			result.MainAddress.OA_Address1 = "Somewhere on the docks";
			result.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			return result;
		}

		#endregion
	}
}
