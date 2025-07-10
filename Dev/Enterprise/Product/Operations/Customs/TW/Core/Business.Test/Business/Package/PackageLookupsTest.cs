using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class PackageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestWeightUQList()
		{
			AssertEquals("DT, G, HG, KG, KT, LB, LT, MC, MG, OT, OZ, T, TL, TN", Lookups.WeightUQList.CodesAsString);
		}

		public void TestVolumeUQList()
		{
			AssertEquals("CC, CF, CI, CY, D3, GA, GI, L, M3, ML, TE", Lookups.VolumeUQList.CodesAsString);
		}

		public void TestDimensionUQList()
		{
			var dimensionUQList = Lookups.DimensionUQList;
			var list = Factory.GetCachedValue<DimensionUQList>();
			AssertSame(list, dimensionUQList);
			AssertEquals(4, dimensionUQList.Count);
		}

		PackageLookups Lookups => Package.Lookups;
		#region InvoiceLine
		Package Package => package ?? (package = Factory.New<Package>());
		Package package;
		#endregion
	}
}
