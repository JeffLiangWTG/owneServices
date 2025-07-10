using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(VoyageFinder))]
	sealed class VoyageFinderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCtor()
		{
			var carrierPK = ZGuid.NewZGuid();

			var parent = new Mock<IVoyageFinderParent>();
			parent.Setup(m => m.CarrierPK).Returns(carrierPK);

			var finder = new VoyageFinder(parent.Object);
			AssertEquals(parent.Object, finder.Parent);
			AssertEquals(carrierPK, finder.CarrierPK);
		}

		public void TestSetUpVoyage()
		{
			CommonShipmentWithVoyageFinderParent shipment = Factory.New<CommonShipmentWithVoyageFinderParent>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEHAM";

			var carrier = Factory.New<OrgHeader>();

			var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();

			VoyageFinder newFinder = new VoyageFinder(shipment);
			newFinder.JV_RV_NKVessel = vessel.RV_FK;
			newFinder.JV_VoyageFlight = "12";
			newFinder.CarrierPK = carrier.PK;

			JobVoyage newVoyage = Factory.New<JobVoyage>();
			newFinder.SetupVoyage(newVoyage);

			AssertEquals("Expecting Vessel to be ARAFURA", "ARAFURA", newVoyage.JV_RV_NKVessel);
			AssertEquals("Expecting Voyage No to be 12", "12", newVoyage.JV_VoyageFlight);
			AssertEquals("Expecting Voyage to have one Origin.", 1, newVoyage.Origins.Count);
			AssertEquals("Expecting Load Port to be AUSYD.", "AUSYD", newVoyage.Origins[0].JA_RL_NKPortOfLoading);
			AssertEquals("Expecting Voyage to have one Destination.", 1, newVoyage.Destinations.Count);
			AssertEquals("Expecting Discharge Port to be DEHAM.", "DEHAM", newVoyage.Destinations[0].JB_RL_NKPortOfDischarge);
			AssertEquals("Carrier should be set", carrier.PK, newVoyage.JV_OH_Line);
		}

		public void TestGetMatchingVoyage_Air()
		{
			SetupBusinessObjects();

			Shipment1.JS_TransportMode = Constants.TransportModes.Air;
			finder = new VoyageFinder(Shipment1);
			finder.JV_VoyageFlight = Voyage1.JV_VoyageFlight;

			JobVoyage matchingVoyage = finder.GetMatchingVoyage();

			AssertEquals("Air should never match anything.", null, matchingVoyage);
		}

		public void TestGetMatchingVoyage_Sea()
		{
			SetupAdditionalBusinessObjects();

			var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();

			Shipment1.JS_TransportMode = Constants.TransportModes.Sea;
			finder = new VoyageFinder(Shipment1);
			finder.JV_VoyageFlight = "643";
			finder.JV_RV_NKVessel = vessel.RV_FK;

			JobVoyage matchingVoyage = finder.GetMatchingVoyage();

			AssertEquals("Expecting matching voyage to be voyage1", Voyage2.PK, matchingVoyage.PK);
		}

		public void TestGetMatchingVoyage_CarrierIsIncludedInMatching()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			var helper = new VoyageTestHelper(Factory);
			var voyage1 = helper.CreateSeaVoyage("Visund", "123", carrier1.PK);
			var voyage2 = helper.CreateSeaVoyage("Visund", "123", carrier2.PK);

			Factory.Save();

			var parent = new Mock<IVoyageFinderParent>();
			parent.Setup(m => m.TransportMode).Returns(Constants.TransportModes.Sea);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Visund";

			var finder = new VoyageFinder(parent.Object)
			{
				JV_RV_NKVessel = vessel.RV_FK,
				JV_VoyageFlight = "123",
				CarrierPK = carrier1.PK
			};

			AssertEquals(voyage1.PK, finder.GetMatchingVoyage().PK);

			finder.CarrierPK = carrier2.PK;
			AssertEquals(voyage2.PK, finder.GetMatchingVoyage().PK);
		}

		public void TestGetMatchingVoyage_Rail()
		{
			SetupAdditionalBusinessObjects();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "JName";

			Voyage2.JV_AirSeaRoad = Constants.TransportModes.Rail;
			Voyage2.JV_RV_NKVessel = vessel.RV_FK;
			Voyage2.JV_VoyageFlight = "JNum";
			Voyage2.Factory.Save();

			Shipment1.JS_TransportMode = Constants.TransportModes.Rail;
			finder = new VoyageFinder(Shipment1);
			finder.JV_VoyageFlight = "JNum";
			finder.JV_RV_NKVessel = vessel.RV_FK;

			JobVoyage matchingVoyage = finder.GetMatchingVoyage();

			AssertEquals("Expecting matching voyage to be voyage1", Voyage2.PK, matchingVoyage.PK);
		}

		public void TestGetSailingForShipmentFromVoyage()
		{
			SetupAdditionalBusinessObjects();

			var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();

			finder = new VoyageFinder(Shipment1);
			finder.JV_RV_NKVessel = vessel.RV_FK;
			finder.JV_VoyageFlight = "643";

			finder.AddOriginAndDestination(Voyage2);

			Voyage2.Factory.Save();
			Factory.Save();

			JobSailing sailingForShipment = finder.GetSailingForShipmentFromVoyage(Voyage2);

			Shipment1.JS_JX = sailingForShipment.PK;

			Assert("Expecting CommonShipment to have a sailing.", !Shipment1.JS_JX.IsEmpty);
			Assert("Not expecting sailing to be sailing3a.", Shipment1.JS_JX != Sailing3a.PK);
			AssertEquals("Expecting shipment1.sailing to be sailingforshipment.", sailingForShipment.PK, Shipment1.JS_JX);
		}

		public void TestValidation()
		{
			SetupAdditionalBusinessObjects();

			CombineAssertions(delegate
			{
				var map = new[]
				{
					new { Mode = Constants.TransportModes.Sea, Type = typeof(VoyageFinderSeaValidation) },
					new { Mode = Constants.TransportModes.Rail, Type = typeof(VoyageFinderRailValidation) },
					new { Mode = Constants.TransportModes.Road, Type = typeof(VoyageFinderValidation) },
					new { Mode = Constants.TransportModes.Air, Type = typeof(VoyageFinderValidation) },
				};

				for (int i = 0; i < map.Length; i++)
				{
					Shipment1.JS_TransportMode = map[i].Mode;
					AssertType("Validation class for " + map[i].Mode, map[i].Type, new VoyageFinder(Shipment1).Validation);
				}
			});
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			CommonShipmentWithVoyageFinderParent shipment = Factory.New<CommonShipmentWithVoyageFinderParent>();
			VoyageFinder finder = new VoyageFinder(shipment);
			return finder;
		}

		CommonShipmentWithVoyageFinderParent Shipment1;

		VoyageFinder finder;

		JobVoyage Voyage1;
		VoyageDestination Destination1a;
		VoyageOrigin Origin1a;
		VoyageOrigin Origin1b;
		JobSailing Sailing1a;
		JobSailing Sailing1b;

		JobVoyage Voyage2;
		VoyageDestination Destination2a;
		VoyageOrigin Origin2a;
		VoyageOrigin Origin2b;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in SetupAdditionalBusinessObjects")]
		JobSailing Sailing2a;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in SetupAdditionalBusinessObjects")]
		JobSailing Sailing2b;

		JobVoyage Voyage3;
		VoyageDestination Destination3a;
		VoyageOrigin Origin3a;
		JobSailing Sailing3a;

		void SetupBusinessObjects()
		{
			Shipment1 = Factory.New<CommonShipmentWithVoyageFinderParent>();
			Shipment1.JS_IsBooking = true;
			Shipment1.JS_IsForwardRegistered = false;

			Voyage1 = Factory.New<JobVoyage>();
			Voyage1.JV_AirSeaRoad = Constants.TransportModes.Air;
			Voyage1.JV_VoyageFlight = "QF123";

			Destination1a = Voyage1.Destinations.AddNew();
			Destination1a.JB_E_ARV = ZDateTime.Now.AddDays(12).AddHours(6);
			Destination1a.JB_RL_NKPortOfDischarge = "JPRMI";

			Origin1a = Voyage1.Origins.AddNew();
			Origin1a.JA_E_DEP = ZDateTime.Now.AddDays(11).AddHours(15);
			Origin1a.JA_RL_NKPortOfLoading = "AUMEL";

			Origin1b = Voyage1.Origins.AddNew();
			Origin1b.JA_E_DEP = ZDateTime.Now.AddDays(11).AddHours(21);
			Origin1b.JA_RL_NKPortOfLoading = "AUSYD";

			Voyage1.GenerateSailings();

			Sailing1a = Voyage1.Sailings.GetSailingFromLoadAndDischarge("AUMEL", "JPRMI");
			Sailing1a.JX_DepotCutOff = ZDateTime.Today.AddDays(10);
			Sailing1a.JX_DepotReceivalCommences = ZDateTime.Today;
			Sailing1a.Origin.JA_CutOff = ZDateTime.Today.AddDays(10);
			Sailing1a.Origin.JA_ReceivalCommences = ZDateTime.Today;

			Sailing1b = Voyage1.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "JPRMI");
			Sailing1b.JX_DepotCutOff = ZDateTime.Today.AddDays(10);
			Sailing1b.JX_DepotReceivalCommences = ZDateTime.Today;
			Sailing1b.Origin.JA_CutOff = ZDateTime.Today.AddDays(10);
			Sailing1b.Origin.JA_ReceivalCommences = ZDateTime.Today;

			Factory.Save();

			Shipment1.JS_A_BKD = ZDateTime.Today;
			Shipment1.JS_RL_NKDestination = "JPOSA";
			Shipment1.JS_RL_NKOrigin = "AUSYD";

			Factory.Save();
		}

		void SetupAdditionalBusinessObjects()
		{
			var vessel1 = RefVessel.LookupVesselByName("ARAFURA", Factory).First();
			var vessel2 = RefVessel.LookupVesselByName("APL IVORY", Factory).First();

			Voyage2 = Factory.New<JobVoyage>();
			Voyage2.JV_AirSeaRoad = Constants.TransportModes.Sea;
			Voyage2.JV_VoyageFlight = "643";
			Voyage2.JV_RV_NKVessel = vessel1.RV_FK;

			Destination2a = Voyage2.Destinations.AddNew();
			Destination2a.JB_E_ARV = ZDateTime.Now.AddDays(25);
			Destination2a.JB_RL_NKPortOfDischarge = "JPRMI";

			Origin2a = Voyage2.Origins.AddNew();
			Origin2a.JA_E_DEP = ZDateTime.Now.AddDays(2);
			Origin2a.JA_RL_NKPortOfLoading = "AUMEL";

			Origin2b = Voyage2.Origins.AddNew();
			Origin2b.JA_E_DEP = ZDateTime.Now.AddDays(6);
			Origin2b.JA_RL_NKPortOfLoading = "AUSYD";

			Voyage2.GenerateSailings();

			Sailing2a = Voyage2.Sailings.GetSailingFromLoadAndDischarge("AUMEL", "JPRMI");
			Sailing2b = Voyage2.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "JPRMI");

			Voyage3 = Factory.New<JobVoyage>();
			Voyage3.JV_AirSeaRoad = Constants.TransportModes.Sea;
			Voyage3.JV_VoyageFlight = "262212";
			Voyage3.JV_RV_NKVessel = vessel2.RV_FK;

			Destination3a = Voyage3.Destinations.AddNew();
			Destination3a.JB_E_ARV = ZDateTime.Now.AddDays(25);
			Destination3a.JB_RL_NKPortOfDischarge = "JPOSA";

			Origin3a = Voyage3.Origins.AddNew();
			Origin3a.JA_E_DEP = ZDateTime.Now.AddDays(2);
			Origin3a.JA_RL_NKPortOfLoading = "AUSYD";

			Voyage3.GenerateSailings();

			Sailing3a = Voyage3.Sailings[0];

			Factory.Save();

			Shipment1 = Factory.New<CommonShipmentWithVoyageFinderParent>();
			Shipment1.JS_A_BKD = ZDateTime.Today;
			Shipment1.JS_RL_NKDestination = "JPOSA";
			Shipment1.JS_RL_NKOrigin = "AUSYD";
			Shipment1.JS_IsBooking = true;
			Shipment1.JS_IsForwardRegistered = false;

			Factory.Save();
		}

		#endregion
	}
}
