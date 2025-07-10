using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class HouseBillRefNoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var houseBillRefNo = Factory.New<HouseBillRefNo>();
			AssertType<ReferenceQualifierList>(houseBillRefNo.Lookups.CY_CodeList);
		}
	}
}
