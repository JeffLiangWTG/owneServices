using CargoWise.EntityFramework.Testing;

namespace Enterprise.Workflow.Business.Testing
{
	class UniversalValidationRuleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusList()
		{
			var lookups = Factory.New<UniversalValidationRule>().Lookups;
			AssertArrayEqualsByElements(new[] { "ERR", "WRN" }, lookups.StatusList.GetAllCodes());
		}
	}
}
