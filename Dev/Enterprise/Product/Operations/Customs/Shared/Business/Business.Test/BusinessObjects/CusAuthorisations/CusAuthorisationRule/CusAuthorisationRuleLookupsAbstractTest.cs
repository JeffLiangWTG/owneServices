using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(CusAuthorisationRuleLookups))]
	public abstract class CusAuthorisationRuleLookupsAbstractTest<T> : BusinessObjectLookupsTestCase
	where T : CusAuthorisationRuleLookups
	{
		public void TestRuleCodesAreOrderedAndCached()
		{
			CombineAssertions(() =>
			{
				var lookups = CusAuthorisationRuleLookupsForTesting();
				var authorisationTypes = lookups.RuleCodeList;
				AssertArrayEqualsByElements("Sorted by Code", authorisationTypes.GetAllCodes().OrderBy(x => x).ToArray(), authorisationTypes.GetAllCodes());
				AssertSame("Cached", lookups.RuleCodeList, authorisationTypes);
			});
		}

		protected abstract T CusAuthorisationRuleLookupsForTesting();
	}
}
