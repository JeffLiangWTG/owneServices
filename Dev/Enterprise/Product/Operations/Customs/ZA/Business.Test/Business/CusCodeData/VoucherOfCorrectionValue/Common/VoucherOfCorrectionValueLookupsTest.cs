using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class VoucherOfCorrectionValueLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var voc = Factory.New<VoucherOfCorrectionValueAfter>();
			AssertEquals(true, object.ReferenceEquals(voc.Lookups.CY_CodeList, Factory.GetCachedValue<VOCValueTypeList>()));
		}
	}
}
