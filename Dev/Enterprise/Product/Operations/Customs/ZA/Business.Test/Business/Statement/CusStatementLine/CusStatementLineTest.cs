using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusStatementLine))]
	sealed class CusStatementLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHeader()
		{
			AssertSame(statementHeader, statementLine.Header);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return statementLine;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Deleting object should have an exception/error because of a trigger.", true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			statementHeader = Factory.New<CusStatementHeader>();
			statementLine = statementHeader.StatementLines.AddNew();
		}

		CusStatementHeader statementHeader;
		CusStatementLine statementLine;
	}
}
