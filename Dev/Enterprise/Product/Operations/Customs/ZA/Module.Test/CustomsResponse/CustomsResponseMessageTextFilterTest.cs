using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(CustomsResponseMessageTextFilter))]
	sealed class CustomsResponseMessageTextFilterTest : ModuleFilterTestCase<CustomsResponseMessageTextFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		public void TestQuery()
		{
			Filter.Property = "222";
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			AssertContains("Equal", "EM_MessageText LIKE '%RFF+ABT:222[:+'']%'", Filter.Query.LiteralTextADO);
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			AssertContains("Not Equal", "EM_MessageText NOT LIKE '%RFF+ABT:222[:+'']%'", Filter.Query.LiteralTextADO);
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			AssertContains("Starts With", "EM_MessageText LIKE '%RFF+ABT:222%'", Filter.Query.LiteralTextADO);
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			AssertContains("Not Starts With", "EM_MessageText NOT LIKE '%RFF+ABT:222%'", Filter.Query.LiteralTextADO);
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertContains("Blank", "EM_MessageText LIKE '%RFF+ABT:[:+'']%' OR EM_MessageText NOT LIKE '%RFF+ABT:%'", Filter.Query.LiteralTextADO);
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			AssertContains("Not Blank", "EM_MessageText LIKE '%RFF+ABT:%' AND EM_MessageText NOT LIKE '%RFF+ABT:[:+'']%'", Filter.Query.LiteralTextADO);
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			AssertContains("Contains", "IN (SELECT EM_PK FROM dbo.EDIMessage CROSS APPLY dbo.csfn_GetEdifactElementInline(EM_MessageText, 'RFF+ABT', 0, 1, 0) AS Data", Filter.Query.LiteralTextADO);
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			AssertContains("Not Contain", "NOT IN (SELECT EM_PK FROM dbo.EDIMessage CROSS APPLY dbo.csfn_GetEdifactElementInline(EM_MessageText, 'RFF+ABT', 0, 1, 0) AS Data", Filter.Query.LiteralTextADO);
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.TextSearch;

		protected override CustomsResponseMessageTextFilter GetNewModuleFilter()
		{
			return new CustomsResponseMessageTextFilter("moo", EDIMessageSchema.EM_MessageText, new ZString[] { "RFF+ABT:" });
		}
	}
}
