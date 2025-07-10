using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgCusCodeValidityLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestVerificationStatusList()
		{
			var cusCodeValidity = Factory.New<OrgCusCodeValidity>();
			var statusTypeList = cusCodeValidity.Lookups.VerificationStatusList;
			AssertEquals("Should contain 2 elements", 2, statusTypeList.Count);
			AssertEquals("VERIFIED", statusTypeList[0].Code);
			AssertEquals("NOT VERIFIED", statusTypeList[1].Code);
			AssertEquals("VERIFIED", statusTypeList[0].Description);
			AssertEquals("NOT VERIFIED", statusTypeList[1].Description);
		}
	}
}
