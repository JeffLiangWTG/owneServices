using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusTWProductLabelRangeLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestProductLabelRangeStatusList()
		{
			var productLabelRangeStatusList = Lookups.ProductLabelRangeStatusList;
			var list = Factory.GetCachedValue<ProductLabelRangeStatusList>();
			NUnit.Framework.Assert.That(productLabelRangeStatusList, NUnit.Framework.Is.SameAs(list));
			NUnit.Framework.Assert.That(productLabelRangeStatusList.Count, NUnit.Framework.Is.EqualTo(8));
		}

		CusTWProductLabelRangeLookups Lookups => CusTWProductLabelRange.Lookups;
		#region CusTWProductLabelRange
		CusTWProductLabelRange CusTWProductLabelRange => cusTWProductLabelRange ?? (cusTWProductLabelRange = Factory.New<CusTWProductLabelRange>());
		CusTWProductLabelRange cusTWProductLabelRange;
		#endregion
	}
}
