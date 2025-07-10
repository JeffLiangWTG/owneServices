using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefPackTypeLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestUOMPackTypeList

		public void TestUOMPackTypeList()
		{
			var refPackType = Factory.New<RefPackType>();
			AssertContainsExactElementsInAnyOrder(new UOMPackTypesList(), refPackType.Lookups.UOMPackTypeList);
		}

		#endregion
	}
}
