using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class StatementProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var statementHeader = Factory.New<BaseCusStatementHeader>();
			var strategy = new StatementProcessTaskLoadStrategy();
			AssertEquals(typeof(StatementProcessTask), strategy.GetTypeForLoad("B2", statementHeader.PK, Factory));
		}
	}
}
