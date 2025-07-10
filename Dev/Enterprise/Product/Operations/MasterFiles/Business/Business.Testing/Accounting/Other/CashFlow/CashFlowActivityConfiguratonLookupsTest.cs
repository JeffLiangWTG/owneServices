using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CashFlowActivityConfiguratonLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			CashFlowActivityConfiguration configuration = new CashFlowActivityConfiguration();
			CashFlowActivityConfiguratonLookups lookups = new CashFlowActivityConfiguratonLookups(configuration);

			AssertNotNull("CodeList should not be null", lookups.CodeList);
			AssertEquals("Code.Count", CashFlowCodeLists.CashFlowTypeList.Count, lookups.CodeList.Count);
			AssertContainsExactElementsInAnyOrder(CashFlowCodeLists.CashFlowTypeList, lookups.CodeList);
		}

		public void TestActivityTypeList()
		{
			CashFlowActivityConfiguration configuration = new CashFlowActivityConfiguration();
			CashFlowActivityConfiguratonLookups lookups = new CashFlowActivityConfiguratonLookups(configuration);

			AssertNotNull("ActivityTypeList should not be null", lookups.ActivityTypeList);
			AssertEquals("ActivityTypeList.Count", 7, lookups.ActivityTypeList.Count);

			AssertEquals(CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Undefined, lookups.ActivityTypeList[0].Code);
			AssertEquals(CashFlowActivityConfiguratonLookups.ActivityTypeDescriptions.Undefined, lookups.ActivityTypeList[0].Description);
			AssertEquals(CashFlowActivityConfiguratonLookups.ActivityTypeCodes.NonCash, lookups.ActivityTypeList[1].Code);
			AssertEquals(CashFlowActivityConfiguratonLookups.ActivityTypeDescriptions.NonCash, lookups.ActivityTypeList[1].Description);
			AssertEquals(CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Cash, lookups.ActivityTypeList[2].Code);
			AssertEquals(CashFlowActivityConfiguratonLookups.ActivityTypeDescriptions.Cash, lookups.ActivityTypeList[2].Description);
			AssertEquals(CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Exchange, lookups.ActivityTypeList[3].Code);
			AssertEquals(CashFlowActivityConfiguratonLookups.ActivityTypeDescriptions.Exchange, lookups.ActivityTypeList[3].Description);
			AssertEquals(CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Operating, lookups.ActivityTypeList[4].Code);
			AssertEquals(CashFlowActivityConfiguratonLookups.ActivityTypeDescriptions.Operating, lookups.ActivityTypeList[4].Description);
			AssertEquals(CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Investing, lookups.ActivityTypeList[5].Code);
			AssertEquals(CashFlowActivityConfiguratonLookups.ActivityTypeDescriptions.Investing, lookups.ActivityTypeList[5].Description);
			AssertEquals(CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Financing, lookups.ActivityTypeList[6].Code);
			AssertEquals(CashFlowActivityConfiguratonLookups.ActivityTypeDescriptions.Financing, lookups.ActivityTypeList[6].Description);
		}
	}
}
