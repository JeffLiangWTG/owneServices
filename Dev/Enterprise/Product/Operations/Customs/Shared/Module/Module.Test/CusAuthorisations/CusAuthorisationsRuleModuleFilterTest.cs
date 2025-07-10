using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusAuthorisationsRuleModuleFilter))]
	sealed class CusAuthorisationsRuleModuleFilterTest : ModuleFilterTestCase<CusAuthorisationsRuleModuleFilter>
	{
		public void TestDefaultComparisonOperator()
		{
			AssertEquals(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith, filter.ComparisonOperator);
		}

		public void TestComparisonOperatorList()
		{
			var expectedComparisonOperators = new[]
{
				ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact,
				ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith,
				ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains,
				ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual,
				ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotStartsWith,
				ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotContain
			};
			AssertArrayEqualsByElements(expectedComparisonOperators, filter.ComparisonOperator_List.GetAllCodes());
		}

		public void TestValidateRuleCode()
		{
			CombineAssertions(() =>
			{
				filter.Property1 = CusAuthorisationRuleTypeList.Codes.Location;
				AssertNoNotifications("Valid rule code", filter.Property1Info);
				filter.Property1 = "ZZZ";
				AssertHasWarningContaining("Invalid rule code", filter.Property1Info, ListValidation.InvalidCodeMessage.ToString());
			});
		}

		public void TestRuleCodeMaxLength()
		{
			AssertEquals(3, filter.Property1Info.MaxLength);
		}

		public void TestRuleValueMaxLength()
		{
			AssertEquals(255, filter.Property2Info.MaxLength);
		}

		public void TestQueryDelegateParameters()
		{
			filter.Property1 = CusAuthorisationRuleTypeList.Codes.Location;
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter.Property2 = "VALUE";
			AssertSequencesEqual(new object[] { CusAuthorisationRuleTypeList.Codes.Location, SQLComparisonOperator.NotEqual, "VALUE" }, filter.QueryDelegateParameters);
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override CusAuthorisationsRuleModuleFilter GetNewModuleFilter()
		{
			return new CusAuthorisationsRuleModuleFilter("moo", (val1, op, val2) => new ZQuery(), new CusAuthorisationRuleTypeList());
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.TextSearch;
		protected override void SetUp()
		{
			base.SetUp();
			filter = new CusAuthorisationsRuleModuleFilterForTest("moo", (val1, op, val2) => new ZQuery(), new CusAuthorisationRuleTypeList());
		}

		CusAuthorisationsRuleModuleFilterForTest filter;
		sealed class CusAuthorisationsRuleModuleFilterForTest : CusAuthorisationsRuleModuleFilter
		{
			public CusAuthorisationsRuleModuleFilterForTest(ZString description, GetAuthorisationRuleDetailsQuery queryDelegate, CodeDescriptionPairList list) : base(description, queryDelegate, list)
			{
			}

			public new object[] QueryDelegateParameters => base.QueryDelegateParameters;
		}
	}
}
