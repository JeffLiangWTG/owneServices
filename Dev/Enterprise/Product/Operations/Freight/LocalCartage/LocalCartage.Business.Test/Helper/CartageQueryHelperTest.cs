using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CartageQueryHelperTest : TestCaseWithFactory
	{
		public void TestGetCartageParentSubQuery_WithoutParam()
		{
			var mainFilter = new ZDBOnlyQuery(typeof(CommonCartage));
			mainFilter.AddToFilter(JobCartageSchema.JJ_ParentID, DBNull.Value);
			var expectedSql = ZQueryFormatter.GetFormattedText(@"JJ_ParentID is NULL");
			AssertEquals(expectedSql, CartageQueryHelper.GetCartageParentSubQuery(mainFilter).LiteralTextSqlFormatted);
		}

		public void TestGetCartageParentSubQuery_WithParam()
		{
			var mainFilter = new ZDBOnlyQuery(typeof(CommonCartage));
			mainFilter.AddToFilter(ViewJobCartageParentsSchema.VCP_JobNumber, "S00000001");
			var expectedSql = ZQueryFormatter.GetFormattedText(@"JJ_ParentID IN (SELECT VCP_PK FROM dbo.ViewJobCartageParents WHERE VCP_JobNumber = 'S00000001')");
			AssertEquals(expectedSql, CartageQueryHelper.GetCartageParentSubQuery(mainFilter).LiteralTextSqlFormatted);
		}

		public void TestGetCartageParentSubQuery_WithMoreElementsThanMaximumAllowed()
		{
			var mainFilter = new ZDBOnlyQuery(typeof(CommonCartage));
			var amountOfPropertiesToCreate = ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION + 5;
			var initialStart = 100;

			var elements = Enumerable.Range(initialStart, amountOfPropertiesToCreate);
			var filter = String.Join(", ", elements);
			mainFilter.AddToFilter_PossiblyCommaSeparated(ViewJobCartageParentsSchema.VCP_JobNumber, SQLComparisonOperator.Equal, (ZString)filter);

			var expectedValues = $"'{String.Join(@"', '", elements)}'";
			var expectedSQL = ZQueryFormatter.GetFormattedText(@$"JJ_ParentID IN (SELECT VCP_PK FROM dbo.ViewJobCartageParents WHERE (VCP_JobNumber in ({expectedValues})))");

			AssertEquals(expectedSQL, CartageQueryHelper.GetCartageParentSubQuery(mainFilter).LiteralTextSqlFormatted);
		}

		public void TestGetCartageParentJobNumberQuery()
		{
			var expectedSql = ZQueryFormatter.GetFormattedText(@"JJ_PK IN (SELECT JJ_PK FROM dbo.JobCartage WHERE JJ_ParentID IN (SELECT VCP_PK FROM dbo.ViewJobCartageParents WHERE (VCP_JobNumber like 'S0001234%' AND VCP_JobNumber >= 'S0001234' AND VCP_JobNumber <= 'S000123þ')))");
			AssertEquals(expectedSql, CartageQueryHelper.GetCartageParentJobNumberQuery(SQLComparisonOperator.StartsWith, "S0001234").LiteralTextSqlFormatted);
		}

		public void TestGetCartageParentJobTypeQuery_WithParent()
		{
			var expectedSql = ZQueryFormatter.GetFormattedText(@"JJ_ParentID IN (SELECT VCP_PK FROM dbo.ViewJobCartageParents WHERE VCP_JobType = 'SHP')");
			AssertEquals(expectedSql, CartageQueryHelper.GetCartageParentJobTypeQuery(SQLComparisonOperator.Equal, "SHP").LiteralTextSqlFormatted);
		}

		public void TestGetCartageParentJobTypeQuery_Standalone()
		{
			var expectedSql = ZQueryFormatter.GetFormattedText(@"JJ_ParentID is NULL");
			AssertEquals(expectedSql, CartageQueryHelper.GetCartageParentJobTypeQuery(SQLComparisonOperator.Equal, "STC").LiteralTextSqlFormatted);
		}
	}
}
