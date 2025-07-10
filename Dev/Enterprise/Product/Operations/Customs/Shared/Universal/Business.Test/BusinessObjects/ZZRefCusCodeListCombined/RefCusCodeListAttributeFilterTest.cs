using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	class RefCusCodeListAttributeFilterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(JoinCondition.Or, attributeFilter.JoinCondition);
		}

		public void TestFilterQuery()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZZD_PK IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST' and ZZE_Value = 'VALUE1')", attributeFilter.Filter.LiteralTextADO);
				attributeFilter = new RefCusCodeListAttributeFilter("TEST", JoinCondition.Or, "VALUE1", "VALUE2");
				AssertEquals("ZZD_PK IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST' and (ZZE_Value in ('VALUE1', 'VALUE2')))", attributeFilter.Filter.LiteralTextADO);
				AssertEquals("TEST_or_False_Equal_VALUE1^VALUE2__", attributeFilter.Key);
				attributeFilter = new RefCusCodeListAttributeFilter("TEST", JoinCondition.Or);
				AssertEquals("ZZD_PK IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST')", attributeFilter.Filter.LiteralTextADO);
				AssertEquals("TEST_or_False_Equal___", attributeFilter.Key);
				attributeFilter = new RefCusCodeListAttributeFilter("TEST", JoinCondition.And, false);
				AssertEquals("ZZD_PK IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST')", attributeFilter.Filter.LiteralTextADO);
				AssertEquals("TEST_and_False_Equal___", attributeFilter.Key);
				attributeFilter = new RefCusCodeListAttributeFilter("TEST", JoinCondition.And, false, null, "VALUE1", "VALUE2");
				AssertEquals("ZZD_PK IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST' and (ZZE_Value in ('VALUE1', 'VALUE2')))", attributeFilter.Filter.LiteralTextADO);
				AssertEquals("TEST_and_False_Equal_VALUE1^VALUE2__", attributeFilter.Key);
				var notExistValues = new ZString[] { "VALUE3", "VALUE4" };
				attributeFilter = new RefCusCodeListAttributeFilter("TEST", JoinCondition.And, false, notExistValues);
				AssertEquals("ZZD_PK IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST') and (ZZD_PK NOT IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST' and (ZZE_Value in ('VALUE3', 'VALUE4'))))", attributeFilter.Filter.LiteralTextADO);
				AssertEquals("TEST_and_False_Equal__VALUE3%VALUE4_", attributeFilter.Key);
				attributeFilter = new RefCusCodeListAttributeFilter("TEST", JoinCondition.And, false, notExistValues, "VALUE1", "VALUE2");
				AssertEquals("(ZZD_PK IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST' and (ZZE_Value in ('VALUE1', 'VALUE2')))) and (ZZD_PK NOT IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST' and (ZZE_Value in ('VALUE3', 'VALUE4'))))", attributeFilter.Filter.LiteralTextADO);
				AssertEquals("TEST_and_False_Equal_VALUE1^VALUE2_VALUE3%VALUE4_", attributeFilter.Key);
				attributeFilter = new RefCusCodeListAttributeFilter("TEST", JoinCondition.And, true);
				AssertEquals("ZZD_PK IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST') or (ZZD_PK NOT IN ( SELECT ZZE_ZZD_CodeList FROM RefDatabase_RefCusCodeListAttribute WHERE ZZE_ZXE_NKName = 'TEST'))", attributeFilter.Filter.LiteralTextADO);
				AssertEquals("TEST_and_True_Equal___", attributeFilter.Key);
				attributeFilter = new RefCusCodeListAttributeFilter("TEST", JoinCondition.And, true, null, "VALUE1", "VALUE2");
				AssertEquals("(ZZD_PK IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST' and (ZZE_Value in ('VALUE1', 'VALUE2')))) or (ZZD_PK NOT IN ( SELECT ZZE_ZZD_CodeList FROM RefDatabase_RefCusCodeListAttribute WHERE ZZE_ZXE_NKName = 'TEST'))", attributeFilter.Filter.LiteralTextADO);
				AssertEquals("TEST_and_True_Equal_VALUE1^VALUE2__", attributeFilter.Key);
				attributeFilter = new RefCusCodeListAttributeFilter("TEST", JoinCondition.And, true, notExistValues);
				AssertEquals("ZZD_PK IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST') and (ZZD_PK NOT IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST' and (ZZE_Value in ('VALUE3', 'VALUE4')))) or (ZZD_PK NOT IN ( SELECT ZZE_ZZD_CodeList FROM RefDatabase_RefCusCodeListAttribute WHERE ZZE_ZXE_NKName = 'TEST'))", attributeFilter.Filter.LiteralTextADO);
				AssertEquals("TEST_and_True_Equal__VALUE3%VALUE4_", attributeFilter.Key);
				attributeFilter = new RefCusCodeListAttributeFilter("TEST", JoinCondition.And, true, notExistValues, "VALUE1", "VALUE2");
				AssertEquals("(ZZD_PK IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST' and (ZZE_Value in ('VALUE1', 'VALUE2')))) and (ZZD_PK NOT IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST' and (ZZE_Value in ('VALUE3', 'VALUE4')))) or (ZZD_PK NOT IN ( SELECT ZZE_ZZD_CodeList FROM RefDatabase_RefCusCodeListAttribute WHERE ZZE_ZXE_NKName = 'TEST'))", attributeFilter.Filter.LiteralTextADO);
				AssertEquals("TEST_and_True_Equal_VALUE1^VALUE2_VALUE3%VALUE4_", attributeFilter.Key);
				attributeFilter = new RefCusCodeListAttributeFilter("TEST", SQLComparisonOperator.StartsWith, "VAL");
				AssertEquals("ZZD_PK IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST' and ZZE_Value like 'VAL%')", attributeFilter.Filter.LiteralTextADO);
				AssertEquals("TEST_and_False_StartsWith_VAL__", attributeFilter.Key);
				attributeFilter = new RefCusCodeListAttributeFilter("TEST", SQLComparisonOperator.StartsWith, "VAL", "XXX");
				AssertEquals("ZZD_PK IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST' and (ZZE_Value like 'VAL%' or ZZE_Value like 'XXX%'))", attributeFilter.Filter.LiteralTextADO);
				AssertEquals("TEST_and_False_StartsWith_VAL^XXX__", attributeFilter.Key);
				attributeFilter = new RefCusCodeListAttributeFilter("TEST", new ZDateTime(2024, 12, 3), SQLComparisonOperator.Equal, "VAL");
				AssertEquals("ZZD_PK IN (SELECT ZZE_ZZD_CodeList FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_ZXE_NKName = 'TEST' and ZZE_Value = 'VAL' and ((ZZE_StartDate is null or ZZE_StartDate <= #2024-12-03 00:00:00.000#) and (ZZE_EndDate is null or ZZE_EndDate >= #2024-12-03 00:00:00.000#)))", attributeFilter.Filter.LiteralTextADO);
				AssertEquals("TEST_and_False_Equal_VAL__20241203", attributeFilter.Key);
			}

			);
		}

		protected override void SetUp()
		{
			base.SetUp();
			attributeFilter = new RefCusCodeListAttributeFilter("TEST", JoinCondition.Or, "VALUE1");
		}

		RefCusCodeListAttributeFilter attributeFilter;
	}
}
