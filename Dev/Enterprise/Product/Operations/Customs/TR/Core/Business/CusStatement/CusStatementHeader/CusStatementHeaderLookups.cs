namespace Enterprise.Customs.TR.Business
{
	public class CusStatementHeaderLookups : Customs.Business.CusStatementHeaderLookups
	{
		public CusStatementHeaderLookups(CusStatementHeader parent)
			: base(parent)
		{
		}

		public new CusStatementHeader Parent => (CusStatementHeader)base.Parent;

		public PaymentTypesList PaymentTypesList => Factory.GetCachedValue<PaymentTypesList>();
		public StatementHeaderStatusList StatementHeaderStatusList => Factory.GetCachedValue<StatementHeaderStatusList>();
		public PaymentStatusList PaymentStatusList => Factory.GetCachedValue<PaymentStatusList>();
		public PaymentPartyList PaymentPartyList => Factory.GetCachedValue<PaymentPartyList>();
	}
}
