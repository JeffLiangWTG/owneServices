using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ModelViewColumnQueryHelperTest : TestCaseWithFactory
	{
		public void TestModuleFilterQuery()
		{
			var helper = new ModelViewColumnQueryHelper<BaseJobDeclaration>();

			AssertQueryEquals(
				"IsBlank",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE JE_UI_NKCarrierSCAC = N'')",
				helper.GetModuleFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", "JE_UI_NKCarrierSCAC", SQLComparisonOperator.IsBlank, ZString.Empty));

			AssertQueryEquals(
				"IsNotBlank",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE JE_UI_NKCarrierSCAC <> N'')",
				helper.GetModuleFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", "JE_UI_NKCarrierSCAC", SQLComparisonOperator.IsNotBlank, ZString.Empty));

			AssertQueryEquals(
				"String Equal",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE JE_UI_NKCarrierSCAC = N'ABC')",
				helper.GetModuleFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", "JE_UI_NKCarrierSCAC", SQLComparisonOperator.Equal, "ABC"));

			AssertQueryEquals(
				"String NotEqual",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE JE_UI_NKCarrierSCAC <> N'ABC')",
				helper.GetModuleFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", "JE_UI_NKCarrierSCAC", SQLComparisonOperator.NotEqual, "ABC"));

			AssertQueryEquals(
				"String StartsWith",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE JE_UI_NKCarrierSCAC LIKE N'ABC%')",
				helper.GetModuleFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", "JE_UI_NKCarrierSCAC", SQLComparisonOperator.StartsWith, "ABC"));

			AssertQueryEquals(
				"String DoesNotStartWith",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE JE_UI_NKCarrierSCAC NOT LIKE N'ABC%')",
				helper.GetModuleFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", "JE_UI_NKCarrierSCAC", SQLComparisonOperator.DoesNotStartWith, "ABC"));

			AssertQueryEquals(
				"String Contains",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE JE_UI_NKCarrierSCAC LIKE N'%ABC%')",
				helper.GetModuleFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", "JE_UI_NKCarrierSCAC", SQLComparisonOperator.Contains, "ABC"));

			AssertQueryEquals(
				"String NotContains",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE JE_UI_NKCarrierSCAC NOT LIKE N'%ABC%')",
				helper.GetModuleFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", "JE_UI_NKCarrierSCAC", SQLComparisonOperator.NotContains, "ABC"));

			AssertQueryEquals(
				"Boolean Equal true",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE JE_NAFTAReconIndicator = 1)",
				helper.GetModuleFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", "JE_NAFTAReconIndicator", SQLComparisonOperator.Equal, true));

			AssertQueryEquals(
				"Boolean Equal false",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE JE_NAFTAReconIndicator = 0)",
				helper.GetModuleFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", "JE_NAFTAReconIndicator", SQLComparisonOperator.Equal, false));

			AssertQueryEquals(
				"Integer Equal",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE JE_NAFTAReconIndicator = 1)",
				helper.GetModuleFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", "JE_NAFTAReconIndicator", SQLComparisonOperator.Equal, 1));

			AssertQueryEquals(
				"Multiple conditions String Equal",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE A = N'B' AND C = N'D')",
				helper.GetModuleFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", false, new (ZString, object)[] { ("A", "B"), ("C", "D") }));

			AssertQueryEquals(
				"Multiple conditions Boolean Equal",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE A = 1 AND C = 0)",
				helper.GetModuleFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", false, new (ZString, object)[] { ("A", true), ("C", false) }));

			AssertQueryEquals(
				"Multiple conditions String",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE A = N'B' AND C <> N'D' AND E LIKE N'F%' AND G NOT LIKE N'H%' AND I LIKE N'%J%' AND K NOT LIKE N'%L%')",
				helper.GetModuleFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", false, new (ZString, SQLComparisonOperator, object)[]
				{
					("A", SQLComparisonOperator.Equal, "B"),
					("C", SQLComparisonOperator.NotEqual, "D"),
					("E", SQLComparisonOperator.StartsWith, "F"),
					("G", SQLComparisonOperator.DoesNotStartWith, "H"),
					("I", SQLComparisonOperator.Contains, "J"),
					("K", SQLComparisonOperator.NotContains, "L")
				}));

			AssertQueryEquals(
				"Multiple conditions Boolean",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE A = 1 AND B = 0 AND C <> 1 AND D <> 0)",
				helper.GetModuleFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", false, new (ZString, SQLComparisonOperator, object)[]
				{
					("A", SQLComparisonOperator.Equal, true),
					("B", SQLComparisonOperator.Equal, false),
					("C", SQLComparisonOperator.NotEqual, true),
					("D", SQLComparisonOperator.NotEqual, false)
				}));

			AssertQueryEquals(
				"Multiple conditions Integer",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE A = 2 AND B <> 3 AND C > 4 AND D >= 5 AND E < 6 AND F <= 7)",
				helper.GetModuleFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", false, new (ZString, SQLComparisonOperator, object)[]
				{
					("A", SQLComparisonOperator.Equal, 2),
					("B", SQLComparisonOperator.NotEqual, 3),
					("C", SQLComparisonOperator.GreaterThan, 4),
					("D", SQLComparisonOperator.GreaterThanOrEqualTo, 5),
					("E", SQLComparisonOperator.LessThan, 6),
					("F", SQLComparisonOperator.LessThanOrEqualTo, 7)
				}));

			AssertQueryEquals(
				"Multiple conditions same filter column",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE A <> 2 AND A <> 3)",
				helper.GetModuleFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", false, new (ZString, SQLComparisonOperator, object)[]
				{
					("A", SQLComparisonOperator.NotEqual, 2),
					("A", SQLComparisonOperator.NotEqual, 3)
				}));
		}

		public void TestModuleFilterQueryForNull()
		{
			var helper = new ModelViewColumnQueryHelper<BaseJobDeclaration>();

			AssertQueryEquals(
				"IN with IS NULL",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE A IS NULL)",
				helper.GetModuleFilterQueryForNull("JE_PK", "JE_PK", "USJobDeclaration", false, "A", false));

			AssertQueryEquals(
				"NOT IN with IS NULL",
				"JE_PK NOT IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE A IS NULL)",
				helper.GetModuleFilterQueryForNull("JE_PK", "JE_PK", "USJobDeclaration", true, "A", false));

			AssertQueryEquals(
				"IN with IS NOT NULL",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE A IS NOT NULL)",
				helper.GetModuleFilterQueryForNull("JE_PK", "JE_PK", "USJobDeclaration", false, "A", true));

			AssertQueryEquals(
				"NOT IN with IS NOT NULL",
				"JE_PK NOT IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE A IS NOT NULL)",
				helper.GetModuleFilterQueryForNull("JE_PK", "JE_PK", "USJobDeclaration", true, "A", true));
		}

		public void TestDateFilterQuery()
		{
			var helper = new ModelViewColumnQueryHelper<BaseJobDeclaration>();

			AssertQueryEquals(
				"HasNoDateEntered",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE JE_PreliminaryStatementPrintDate IS NULL)",
				helper.GetDateFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", "JE_PreliminaryStatementPrintDate", DateComparisonOperator.HasNoDateEntered, ZDateTime.Today, ZDateTime.Today));

			AssertQueryEquals(
				"HasDateEntered",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE JE_PreliminaryStatementPrintDate IS NOT NULL)",
				helper.GetDateFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", "JE_PreliminaryStatementPrintDate", DateComparisonOperator.HasDateEntered, ZDateTime.Today, ZDateTime.Today));

			AssertQueryEquals(
				"HasDateInRange with start date and end date",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE JE_PreliminaryStatementPrintDate >= '2023-07-05 00:00:00.000' AND JE_PreliminaryStatementPrintDate < '2023-07-06 00:00:00.000')",
				helper.GetDateFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", "JE_PreliminaryStatementPrintDate", DateComparisonOperator.HasDateInRange, new ZDateTime(2023, 7, 5), new ZDateTime(2023, 7, 5)));

			AssertQueryEquals(
				"HasDateInRange with only start date",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE JE_PreliminaryStatementPrintDate >= '2023-07-05 00:00:00.000')",
				helper.GetDateFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", "JE_PreliminaryStatementPrintDate", DateComparisonOperator.HasDateInRange, new ZDateTime(2023, 7, 5), ZDateTime.Empty));

			AssertQueryEquals(
				"HasDateInRange with only end date",
				"JE_PK IN (SELECT JE_PK FROM dbo.USJobDeclaration WHERE JE_PreliminaryStatementPrintDate < '2023-07-06 00:00:00.000')",
				helper.GetDateFilterQuery("JE_PK", "JE_PK", "USJobDeclaration", "JE_PreliminaryStatementPrintDate", DateComparisonOperator.HasDateInRange, ZDateTime.Empty, new ZDateTime(2023, 7, 5)));
		}

		void AssertQueryEquals(string message, string expectedSql, ZQuery actual)
		{
			var actualSql = Regex.Replace(actual.LiteralTextSqlFormatted, @"[\s]+", " ")
				.Replace("( ", "(")
				.Replace(" )", ")")
				.Trim();
			AssertEquals(message, expectedSql, actualSql);
		}
	}
}
