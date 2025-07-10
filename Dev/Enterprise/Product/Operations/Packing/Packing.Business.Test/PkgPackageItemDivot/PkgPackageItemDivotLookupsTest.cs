using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Packing.Business.Testing
{
	public class PkgPackageItemDivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestWeightUQs()
		{
			var packageItemDivot = Factory.New<PkgPackageItemDivot>();
			var lookups = packageItemDivot.Lookups;
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), lookups.WeightUQs);
		}
	}
}
