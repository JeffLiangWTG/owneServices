namespace Enterprise.Customs.TR.Business
{
	public class CusStatementLineLookups : Customs.Business.CusStatementLineLookups
	{
		public CusStatementLineLookups(CusStatementLine parent) : base(parent)
		{
		}

		public StatementLineStatusList StatementLineStatusList => Factory.GetCachedValue<StatementLineStatusList>();

		public StatementLineEntryTypeList StatementLineEntryTypeList => Factory.GetCachedValue<StatementLineEntryTypeList>();
	}
}
