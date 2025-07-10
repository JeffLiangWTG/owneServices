using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefContainerCodeMapLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUsageList()
		{
			var testParent = Factory.NewWithValidTestData<RefContainerCodeMap>();
			Assert(testParent.Lookups.UsageList.Count == 0);

			testParent.RCM_RN_NKCountry = "US";
			Assert(testParent.Lookups.UsageList.Count == 1);

			testParent.RCM_RN_NKCountry = "CZ";
			Assert(testParent.Lookups.UsageList.Count == 0);
		}

		public void TestCodeList()
		{
			var testParent = Factory.NewWithValidTestData<RefContainerCodeMap>();
			testParent.RCM_RN_NKCountry = "US";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var testList = testParent.Lookups.CodeList;
				AssertEquals(true, testList.ContainsCode("40"));
				AssertEquals(true, testList.ContainsCode("AR"));
			}
		}
	}
}
