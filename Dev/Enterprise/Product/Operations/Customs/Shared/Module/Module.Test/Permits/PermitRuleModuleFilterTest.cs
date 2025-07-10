using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(PermitRuleModuleFilter))]
	sealed class PermitRuleModuleFilterTest : ModuleFilterTestCase<PermitRuleModuleFilter>
	{
		public void TestPermitRuleCodes()
		{
			var tester = new PermitRuleModuleFilterForTest("DESC", (val0, val1, val2) => new ZQuery());
			AssertEquals(2, tester.PermitRuleCodes.Count);
			Assert(tester.PermitRuleCodes.ContainsCode("TAR"));
			Assert(tester.PermitRuleCodes.ContainsCode("PRD"));
		}

		public void TestAdditionalValidationOnCodes()
		{
			var tester = new DummyPermitRuleModuleFilterForTest("DESC", (val0, val1, val2) => new ZQuery());
			tester.Property1 = "TAR";
			AssertNoNotifications(tester.Property1Info);
			tester.Property1 = "PRD";
			AssertNoNotifications(tester.Property1Info);
			tester.Property1 = "IMP";
			AssertHasWarningOfInvalidCodeOnly(tester.Property1Info);
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override PermitRuleModuleFilter GetNewModuleFilter() => new PermitRuleModuleFilterForTest("moo", (val0, val1, val2) => new ZQuery());

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.TextSearch;

		static void AssertHasWarningOfInvalidCodeOnly(ZPropertyInfo info)
		{
			AssertNoErrors(info);
			AssertNoMessageErrors(info);
			AssertHasWarningContaining(info, ListValidation.InvalidCodeMessage);
		}
	}

	sealed class DummyPermitRuleModuleFilterForTest : PermitRuleModuleFilterForTest
	{
		public DummyPermitRuleModuleFilterForTest(ZString description, GetPermitRuleQuery queryDelegate)
			: base(description, queryDelegate)
		{
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new DummyPermitRuleModuleFilterForTest("moo", (val0, val1, val2) => new ZQuery());
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			DummyPermitRuleModuleFilterForTest filter = (DummyPermitRuleModuleFilterForTest)filterToCopyFrom;
			Property1 = filter.Property1;
			Property2 = filter.Property2;
		}
	}
}
