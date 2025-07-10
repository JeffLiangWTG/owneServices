using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusStatementLineStatusCollection))]
	sealed class CusStatementLineStatusCollectionTest : SubsetBusinessObjectCollectionTestCase<CusStatementLineStatusCollection, CusStatementLine>
	{
		public void TestIsThisPartOfTheCollection()
		{
			CusStatementHeader statementHeader = Factory.New<CusStatementHeader>();
			CusStatementLine activeLine = statementHeader.StatementLines.AddNew();
			activeLine.B3_Status = StatementLineStatusList.Codes.Active;

			CusStatementLine deletedLine = statementHeader.StatementLines.AddNew();
			deletedLine.B3_Status = StatementLineStatusList.Codes.Deleted;

			CusStatementLineStatusCollection activeLines = new CusStatementLineStatusCollection(statementHeader, StatementLineStatusList.Codes.Active);
			AssertEquals(true, activeLines.Contains(activeLine));
			AssertEquals(false, activeLines.Contains(deletedLine));
		}

		protected override CusStatementLineStatusCollection GetCollectionToTest()
		{
			return new CusStatementLineStatusCollection(Statement, StatementLineStatusList.Codes.Deleted);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CusStatementLine result = Statement.StatementLines.AddNew();
			result.B3_Status = StatementLineStatusList.Codes.Deleted;
			return result;
		}

		CusStatementHeader Statement
		{
			get { return fStatement ?? (fStatement = Factory.New<CusStatementHeader>()); }
		}
		CusStatementHeader fStatement;
	}
}
