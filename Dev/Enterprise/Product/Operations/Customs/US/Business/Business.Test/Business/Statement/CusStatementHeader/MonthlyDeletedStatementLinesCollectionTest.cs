using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(MonthlyDeletedStatementLinesCollection))]
	sealed class MonthlyDeletedStatementLinesCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationshipFilter()
		{
			CusStatementHeader dailyStatement = StatementHeader.DailyStatements.AddNew();
			CusStatementLine statementLine = dailyStatement.StatementLines.AddNew();
			statementLine.B3_Status = StatementLineStatusList.Codes.Deleted;

			CusStatementLine statementLine2 = dailyStatement.StatementLines.AddNew();
			statementLine2.B3_Status = StatementLineStatusList.Codes.Active;

			CusStatementHeader dailyStatement2 = StatementHeader.DailyStatements.AddNew();
			CusStatementLine statementLine3 = dailyStatement2.StatementLines.AddNew();
			statementLine3.B3_Status = StatementLineStatusList.Codes.Deleted;

			MonthlyDeletedStatementLinesCollection coll = new MonthlyDeletedStatementLinesCollection(StatementHeader);
			coll.Load();

			AssertEquals(2, coll.Count);
			AssertEquals(true, coll.Contains(statementLine));
			AssertEquals(true, coll.Contains(statementLine3));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new MonthlyDeletedStatementLinesCollection(StatementHeader);
		}

		CusStatementHeader StatementHeader
		{
			get
			{
				if (fStatementHeader == null)
				{
					fStatementHeader = Factory.New<CusStatementHeader>();
					fStatementHeader.B2_StatementNumber = "1234P";
					fStatementHeader.B2_IsMonthlyStatement = true;
					AssertEquals("PreCondition:IsMonthlyStatement", true, fStatementHeader.IsMonthlyStatement);
				}
				return fStatementHeader;
			}
		}
		CusStatementHeader fStatementHeader;
	}
}
