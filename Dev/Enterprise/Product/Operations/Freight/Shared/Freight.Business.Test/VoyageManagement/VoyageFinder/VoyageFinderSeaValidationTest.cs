using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageFinderSeaValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJV_RV_NKVessel()
		{
			Finder.JV_RV_NKVessel = "Invalid";
			AssertHasError(Finder.JV_RV_NKVesselInfo, "Enter a valid Vessel.");

			var vessel = RefVessel.LookupVesselByName("BANOWATI", Factory).First();
			Finder.JV_RV_NKVessel = vessel.RV_FK;
			AssertNoNotifications(Finder.JV_RV_NKVesselInfo);

			Finder.JV_RV_NKVessel = "";
			AssertHasError(Finder.JV_RV_NKVesselInfo, "Please enter a Vessel.");
		}

		public void TestJV_VoyageFlight()
		{
			Finder.JV_VoyageFlight = "V01";
			AssertHasWarning("Voyage starts with a single 'V' - Warning expected", Finder.JV_VoyageFlightInfo, "Voyage Number should not start with a 'V'. The system will add this automatically.");

			Finder.JV_VoyageFlight = "N183";
			AssertNoErrors("Valid Voyage - No error expected", Finder.JV_VoyageFlightInfo);

			Finder.JV_VoyageFlight = "01";
			AssertNoNotifications(Finder.JV_VoyageFlightInfo);

			Finder.JV_VoyageFlight = "";
			AssertHasError(Finder.JV_VoyageFlightInfo, "Please enter a Voyage Number.");
		}

		public void TestCarrierPK()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			Factory.Save();

			var notCarrier = Factory.New<OrgHeader>();

			Finder.CarrierPK = ZGuid.Empty;
			AssertNoNotifications(Finder.CarrierPKInfo);

			Finder.CarrierPK = notCarrier.PK;
			Assert(Finder.CarrierPKInfo.HasErrors());

			Finder.CarrierPK = carrier.PK;
			Assert(!Finder.CarrierPKInfo.HasErrors());
		}

		#region Implementation

		VoyageFinder Finder
		{
			get { return finder ?? (finder = new VoyageFinder(Parent)); }
		}
		VoyageFinder finder;

		CommonShipmentWithVoyageFinderParent Parent
		{
			get
			{
				if (parent == null)
				{
					parent = Factory.New<CommonShipmentWithVoyageFinderParent>();
					parent.JS_TransportMode = Constants.TransportModes.Sea;
				}
				return parent;
			}
		}
		CommonShipmentWithVoyageFinderParent parent;

		#endregion
	}
}
