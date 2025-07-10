using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class GenAddOnColumnQueryHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetQueryOnGenAddOnColumn()
		{
			const string startDate = "2008-01-01 00:00:00.000";
			const string endDate = "2009-01-01 00:00:00.000";
			var queryHelper = new GenAddOnColumnQueryHelper(typeof(StmEvent));
			var query = queryHelper.GetQueryOnGenAddOnColumn(TestColumnName, TestValue1);
			AssertEquals("SE_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = '" + TestColumnName + "' and XA_Data = '" + TestValue1 + "')", query.LiteralTextADO);

			query = queryHelper.GetQueryOnGenAddOnColumn(TestColumnName, TestValue2, true);
			AssertEquals("SE_PK NOT IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = '" + TestColumnName + "' and XA_Data = '" + TestValue2 + "')", query.LiteralTextADO);

			query = queryHelper.GetQueryOnGenAddOnColumn(TestColumnName, SQLComparisonOperator.Contains, TestValue3);
			AssertEquals("SE_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = '" + TestColumnName + "' and XA_Data like '%" + TestValue3 + "%')", query.LiteralTextADO);

			query = queryHelper.GetQueryOnGenAddOnColumn(TestColumnName, SQLComparisonOperator.LessThanOrEqualTo, TestValue4, true);
			AssertEquals("SE_PK NOT IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = '" + TestColumnName + "' and XA_Data <= '" + TestValue4 + "')", query.LiteralTextADO);

			var dateQuery = queryHelper.GetQueryOnGenAddOnColumn(TestColumnName, DateComparisonOperator.HasDateInRange, TestStartDate, TestEndDate);
			AssertEquals("SE_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'US_EntryType' and XA_Data >= '" + startDate + "' and XA_Data < '" + endDate + "')", dateQuery.LiteralTextADO);

			dateQuery = queryHelper.GetQueryOnGenAddOnColumn(TestColumnName, DateComparisonOperator.HasNoDateEntered, TestStartDate, TestEndDate);
			AssertEquals("SE_PK NOT IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'US_EntryType')", dateQuery.LiteralTextADO);

			dateQuery = queryHelper.GetQueryOnGenAddOnColumn(TestColumnName, DateComparisonOperator.HasDateEntered, TestStartDate, TestEndDate);
			AssertEquals("SE_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'US_EntryType')", dateQuery.LiteralTextADO);

			dateQuery = queryHelper.GetQueryOnGenAddOnColumn(TestColumnName, (ZInt)TestStartDay, (ZInt)TestEndDay);
			AssertEquals("SE_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'US_EntryType' and (TRY_PARSE(XA_Data AS DECIMAL) BETWEEN 1 AND 31))", dateQuery.LiteralTextADO);
		}

		#region TestValues

		const string TestColumnName = "US_EntryType";
		const string TestValue1 = "TestValue";
		const string TestValue2 = "Y";
		const string TestValue3 = "ABCD";
		const string TestValue4 = "12345";
		const int TestStartYear = 2008;
		const int TestStartMonth = 1;
		const int TestStartDay = 1;
		const int TestEndYear = 2008;
		const int TestEndMonth = 12;
		const int TestEndDay = 31;
		readonly ZDateTime TestStartDate = new ZDateTime(TestStartYear, TestStartMonth, TestStartDay);
		readonly ZDateTime TestEndDate = new ZDateTime(TestEndYear, TestEndMonth, TestEndDay);

		#endregion

		[ExpectNoExceptions]
		public void TestGetQueryWithSubQueryOnGenAddOnColumn()
		{
			const string endDate = "2009-01-01 00:00:00.000";
			var queryHelper = new GenAddOnColumnQueryHelper(typeof(StmEvent), typeof(StmALog), StmALogSchema.SL_Parent);

			var query = queryHelper.GetQueryWithSubQueryOnGenAddOnColumn(TestColumnName, TestValue1);
			AssertEquals("SE_PK IN (SELECT SL_Parent FROM dbo.StmALog WHERE SL_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = '" + TestColumnName + "' and XA_Data = '" + TestValue1 + "'))", query.LiteralTextADO);

			query = queryHelper.GetQueryWithSubQueryOnGenAddOnColumn(TestColumnName, TestValue2, true);
			AssertEquals("SE_PK IN (SELECT SL_Parent FROM dbo.StmALog WHERE SL_PK NOT IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = '" + TestColumnName + "' and XA_Data = '" + TestValue2 + "'))", query.LiteralTextADO);

			query = queryHelper.GetQueryWithSubQueryOnGenAddOnColumn(TestColumnName, SQLComparisonOperator.Contains, TestValue3);
			AssertEquals("SE_PK IN (SELECT SL_Parent FROM dbo.StmALog WHERE SL_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = '" + TestColumnName + "' and XA_Data like '%" + TestValue3 + "%'))", query.LiteralTextADO);

			query = queryHelper.GetQueryWithSubQueryOnGenAddOnColumn(TestColumnName, SQLComparisonOperator.LessThanOrEqualTo, TestValue4, true);
			AssertEquals("SE_PK IN (SELECT SL_Parent FROM dbo.StmALog WHERE SL_PK NOT IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = '" + TestColumnName + "' and XA_Data <= '" + TestValue4 + "'))", query.LiteralTextADO);

			query = queryHelper.GetQueryWithSubQueryOnGenAddOnColumn(TestColumnName, DateComparisonOperator.HasDateInRange, TestStartDate, TestEndDate);
			AssertEquals("SE_PK IN (SELECT SL_Parent FROM dbo.StmALog WHERE SL_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = '" + TestColumnName + "' and XA_Data >= '" + TestStartYear + "-" + TestStartMonth.ToString("00") + "-" + TestStartDay.ToString("00") + " 00:00:00.000' and XA_Data < '" + endDate + "'))", query.LiteralTextADO);

			query = queryHelper.GetQueryWithSubQueryOnGenAddOnColumn(TestColumnName, DateComparisonOperator.HasNoDateEntered, TestStartDate, TestEndDate);
			AssertEquals("SE_PK IN (SELECT SL_Parent FROM dbo.StmALog WHERE SL_PK NOT IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'US_EntryType'))", query.LiteralTextADO);

			query = queryHelper.GetQueryWithSubQueryOnGenAddOnColumn(TestColumnName, DateComparisonOperator.HasDateEntered, TestStartDate, TestEndDate);
			AssertEquals("SE_PK IN (SELECT SL_Parent FROM dbo.StmALog WHERE SL_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'US_EntryType'))", query.LiteralTextADO);

			try
			{
				var queryHelper2 = new GenAddOnColumnQueryHelper(typeof(StmEvent));
				query = queryHelper2.GetQueryWithSubQueryOnGenAddOnColumn(TestColumnName, SQLComparisonOperator.Contains, TestValue1, false);
				Fail("Should have thrown an exception");
			}
			catch (InvalidOperationException ex)
			{
				AssertEquals(GenAddOnColumnQueryHelper.InvalidOperationExceptionMessage, ex.Message);
			}
		}

		public void TestWhenThereAreMultipleLevelsOfSubQuery()
		{//the only part that needs testing is checking that the build up of the query works.
			var linkList = new List<GenAddOnColumnQueryHelper.ForeignKeyLink>();
			linkList.Add(new GenAddOnColumnQueryHelper.ForeignKeyLink(typeof(OrgContact), OrgContactSchema.OC_OH));
			linkList.Add(new GenAddOnColumnQueryHelper.ForeignKeyLink(typeof(OrgContactAllocation), OrgContactAttributeSchema.PC_OC));

			var queryHelper = new GenAddOnColumnQueryHelper(typeof(OrgHeader), linkList);
			var query = queryHelper.GetQueryWithSubQueryOnGenAddOnColumn(TestColumnName, TestValue1);
			AssertEquals("OH_PK IN (SELECT OC_OH FROM dbo.OrgContact WHERE OC_PK IN (SELECT PC_OC FROM dbo.OrgContactAttribute WHERE PC_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'US_EntryType' and XA_Data = 'TestValue')))", query.LiteralTextADO);
		}

		public void TestGetContainsValueForAnyQuery()
		{
			var queryHelper = new GenAddOnColumnQueryHelper(typeof(StmEvent));
			var query = queryHelper.GetContainsValueForAnyQuery(false, StmEventSchema.Constants.SE_Desc);
			AssertEquals(" IN (SELECT  FROM dbo.GenAddOnColumn WHERE XA_Name = 'SE_Desc')", query.LiteralTextADO);

			query = queryHelper.GetContainsValueForAnyQuery(true, StmEventSchema.Constants.SE_Desc);
			AssertEquals(" NOT IN (SELECT  FROM dbo.GenAddOnColumn WHERE XA_Name = 'SE_Desc')", query.LiteralTextADO);
		}

		public void TestGetQueryHandlingBlanks()
		{
			var queryHelper = new GenAddOnColumnQueryHelper(typeof(StmEvent));
			const string ValueIsEmptyQuery = "SE_PK NOT IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'US_EntryType')";
			const string ValueIsNotEmpty = "SE_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'US_EntryType')";

			AssertEquals(ValueIsEmptyQuery, queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.Equal, string.Empty, ignoreEmpty: false).LiteralTextADO);
			AssertSelectAllQuery(queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.Equal, string.Empty));
			AssertEquals("SE_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'US_EntryType' and XA_Data = 'TST')",
						 queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.Equal, "TST").LiteralTextADO);

			AssertEquals(ValueIsNotEmpty, queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.NotEqual, string.Empty, ignoreEmpty: false).LiteralTextADO);
			AssertSelectAllQuery(queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.NotEqual, string.Empty));
			AssertEquals("SE_PK NOT IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'US_EntryType' and XA_Data = 'TST')",
						 queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.NotEqual, "TST").LiteralTextADO);

			AssertEquals(ValueIsEmptyQuery, queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.Contains, string.Empty, ignoreEmpty: false).LiteralTextADO);
			AssertSelectAllQuery(queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.Contains, string.Empty));
			AssertEquals("SE_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'US_EntryType' and XA_Data like '%TST%')",
						 queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.Contains, "TST").LiteralTextADO);

			AssertEquals(ValueIsNotEmpty, queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.NotContains, string.Empty, ignoreEmpty: false).LiteralTextADO);
			AssertSelectAllQuery(queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.NotContains, string.Empty));
			AssertEquals("SE_PK NOT IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'US_EntryType' and XA_Data like '%TST%')",
						 queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.NotContains, "TST").LiteralTextADO);

			AssertEquals(ValueIsEmptyQuery, queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.StartsWith, string.Empty, ignoreEmpty: false).LiteralTextADO);
			AssertSelectAllQuery(queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.StartsWith, string.Empty));
			AssertEquals("SE_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'US_EntryType' and XA_Data like 'TST%')",
						 queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.StartsWith, "TST").LiteralTextADO);

			AssertEquals(ValueIsNotEmpty, queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.DoesNotStartWith, string.Empty, ignoreEmpty: false).LiteralTextADO);
			AssertSelectAllQuery(queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.DoesNotStartWith, string.Empty));
			AssertEquals("SE_PK NOT IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'US_EntryType' and XA_Data like 'TST%')",
						 queryHelper.GetQueryHandlingBlanks(TestColumnName, SQLComparisonOperator.DoesNotStartWith, "TST").LiteralTextADO);

			AssertEquals(ValueIsEmptyQuery, queryHelper.GetQueryHandlingBlanks(TestColumnName, SpecialComparisonOperator.IsBlank, string.Empty).LiteralTextADO);
			AssertEquals(ValueIsEmptyQuery, queryHelper.GetQueryHandlingBlanks(TestColumnName, SpecialComparisonOperator.IsBlank, string.Empty, ignoreEmpty: false).LiteralTextADO);

			AssertEquals(ValueIsNotEmpty, queryHelper.GetQueryHandlingBlanks(TestColumnName, SpecialComparisonOperator.IsNotBlank, string.Empty).LiteralTextADO);
			AssertEquals(ValueIsNotEmpty, queryHelper.GetQueryHandlingBlanks(TestColumnName, SpecialComparisonOperator.IsNotBlank, string.Empty, ignoreEmpty: false).LiteralTextADO);
		}

		[ExpectNoExceptions]
		public void TestGetQueryWithSubQueryOnDataColumn()
		{
			var queryHelper = new GenAddOnColumnQueryHelper(typeof(StmEvent));
			var subQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
			subQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, "ADD");
			AssertEquals("SE_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'SE_PK' and XA_Data IN (SELECT SL_Parent FROM dbo.StmALog WHERE SL_SE_NKEvent = 'ADD'))",
						 queryHelper.GetQueryWithSubQueryOnDataColumn(StmEventSchema.PK, subQuery).LiteralTextADO);
		}

		static void AssertSelectAllQuery(ZDBOnlyQuery query)
		{
			AssertEquals(string.Empty, query.LiteralTextADO);
			AssertEquals(false, query.IsNoResultQuery);
			AssertEquals(true, query.IsTableUsedInQuery("StmEvent"));
		}
	}
}
