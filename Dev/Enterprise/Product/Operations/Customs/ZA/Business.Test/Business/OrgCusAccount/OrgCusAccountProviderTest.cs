using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class OrgCusAccountProviderTest : TestCaseWithFactory
	{
		public void TestSetDefaultValues()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(orgHeader);
			var cusAccount = wrapper.FinancialAccountNumbers.AddNew();
			AssertEquals(OrgCusAccountProvider.FANCode, cusAccount.CZ_Code);
		}
	}
}
