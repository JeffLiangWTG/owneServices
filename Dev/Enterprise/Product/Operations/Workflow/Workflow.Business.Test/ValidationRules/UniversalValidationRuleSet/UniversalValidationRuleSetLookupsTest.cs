using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Workflow.Business.Testing
{
	sealed class UniversalValidationRuleSetLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDataContextList()
		{
			var ruleSet = Factory.New<UniversalValidationRuleSet>();
			var lookups = ruleSet.Lookups;
			var expected = Enum.GetNames(typeof(UniversalDataBuss.Integration.DataContextType)).OrderBy(x => x).ToArray();
			AssertArrayEqualsByElements(expected, lookups.DataContextList.ToArray().Select(x => x.Code).ToArray());
		}
	}
}
