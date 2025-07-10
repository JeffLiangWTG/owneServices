using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageFinderRailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJV_RV_NKVessel()
		{
			Finder.JV_RV_NKVessel = "blat";
			AssertNoNotifications(Finder.JV_RV_NKVesselInfo);

			Finder.JV_RV_NKVessel = "";
			AssertHasError(Finder.JV_RV_NKVesselInfo, "Please enter a Vessel.");
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
					parent.JS_TransportMode = Constants.TransportModes.Rail;
				}
				return parent;
			}
		}
		CommonShipmentWithVoyageFinderParent parent;

		#endregion
	}
}
