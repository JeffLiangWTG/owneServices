using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusStatementLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatementLineStatusList()
		{
			AssertType<StatementLineStatusList>(lookups.StatementLineStatusList);
		}

		public void TestStatementEntryStatus()
		{
			AssertType<StatementEntryStatus>(lookups.StatementEntryStatus);
		}

		protected override void SetUp()
		{
			base.SetUp();
			line = Factory.New<CusStatementLine>();
			lookups = new CusStatementLineLookups(line);
		}
		CusStatementLineLookups lookups;
		CusStatementLine line;
	}
}
