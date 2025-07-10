using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class CusStatementLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatementLineStatusList()
		{
			var list = lookups.StatementLineStatusList;
			CombineAssertions(() =>
			{
				AssertType<StatementLineStatusList>(lookups.StatementLineStatusList);
				AssertEquals("CodeAsString", "INA, NRL, RLS", list.CodesAsString);
			});
		}

		public void TestStatementLineEntryTypeList()
		{
			var list = lookups.StatementLineEntryTypeList;
			CombineAssertions(() =>
			{
				AssertType<StatementLineEntryTypeList>(lookups.StatementLineEntryTypeList);
				AssertEquals("CodeAsString", "ACC, ETR, EXP, IMP, MAN, MIS, NCT, SPT", list.CodesAsString);
			});
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
