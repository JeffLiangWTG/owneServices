using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageFinderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestVoyages()
		{
			AssertType(typeof(RefVesselCollection), Finder.Lookups.Vessels);
		}

		public void TestCarriers()
		{
			AssertType(typeof(SeaShippingProviderCollection), Finder.Lookups.Carriers);
		}

		#region Implementation

		VoyageFinder Finder
		{
			get { return finder ?? (finder = new VoyageFinder(Parent)); }
		}
		VoyageFinder finder;

		CommonShipmentWithVoyageFinderParent Parent
		{
			get { return parent ?? (parent = Factory.New<CommonShipmentWithVoyageFinderParent>()); }
		}
		CommonShipmentWithVoyageFinderParent parent;

		#endregion
	}
}
