using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.Testing
{
	public class WhsCartonSizeLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestDimensionUQs

		public void TestDimensionUQs()
		{
			var cartonSize = Factory.New<WhsCartonSize>();
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length), cartonSize.Lookups.DimensionUQs);
		}

		#endregion

		#region TestWeightUQs

		public void TestWeightUQs()
		{
			var cartonSize = Factory.New<WhsCartonSize>();
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), cartonSize.Lookups.WeightUQs);
		}

		#endregion

		#region TestVolumeUQs

		public void TestVolumeUQs()
		{
			var cartonSize = Factory.New<WhsCartonSize>();
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume), cartonSize.Lookups.VolumeUQs);
		}

		#endregion
	}
}
