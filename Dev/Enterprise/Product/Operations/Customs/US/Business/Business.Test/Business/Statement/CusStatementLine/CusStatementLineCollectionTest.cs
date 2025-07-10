using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusStatementLineCollection))]
	sealed class CusStatementLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			CusStatementHeader statementHeader = Factory.New<CusStatementHeader>();
			AssertEquals(false, statementHeader.StatementLines.AllowNew);
		}

		public void TestAreAllLinesDeleted()
		{
			CusStatementHeader statementHeader = Factory.New<CusStatementHeader>();
			AssertEquals(false, statementHeader.StatementLines.AreAllLinesDeleted);

			CusStatementLine statementLine = statementHeader.StatementLines.AddNew();
			CusStatementLine statementLine1 = statementHeader.StatementLines.AddNew();
			CusStatementLine statementLine2 = statementHeader.StatementLines.AddNew();
			AssertEquals(false, statementHeader.StatementLines.AreAllLinesDeleted);

			statementLine.B3_Status = StatementLineStatusList.Codes.Deleted;
			statementLine1.B3_Status = StatementLineStatusList.Codes.DeletionPending;
			statementLine2.B3_Status = StatementLineStatusList.Codes.Active;
			AssertEquals(false, statementHeader.StatementLines.AreAllLinesDeleted);

			statementLine2.B3_Status = StatementLineStatusList.Codes.Deleted;
			AssertEquals(false, statementHeader.StatementLines.AreAllLinesDeleted);

			statementLine1.B3_Status = StatementLineStatusList.Codes.Deleted;
			AssertEquals(true, statementHeader.StatementLines.AreAllLinesDeleted);
		}

		public void TestHasLinesWithDeletionPending()
		{
			CusStatementHeader statementHeader = Factory.New<CusStatementHeader>();
			AssertEquals(false, statementHeader.StatementLines.HasLinesWithDeletionPending);

			CusStatementLine statementLine = statementHeader.StatementLines.AddNew();
			AssertEquals(false, statementHeader.StatementLines.HasLinesWithDeletionPending);

			statementLine.B3_Status = StatementLineStatusList.Codes.DeletionPending;
			AssertEquals(true, statementHeader.StatementLines.HasLinesWithDeletionPending);

			statementLine.B3_Status = StatementLineStatusList.Codes.Deleted;
			AssertEquals(false, statementHeader.StatementLines.HasLinesWithDeletionPending);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CusStatementHeader statementHeader = Factory.New<CusStatementHeader>();
			return new CusStatementLineCollection(statementHeader);
		}
	}
}
