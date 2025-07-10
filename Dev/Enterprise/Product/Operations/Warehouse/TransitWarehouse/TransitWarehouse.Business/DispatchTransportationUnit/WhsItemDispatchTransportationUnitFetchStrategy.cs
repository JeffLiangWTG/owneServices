namespace Enterprise.Warehouse.Transit.Business
{
	public sealed class WhsItemDispatchTransportationUnitFetchStrategy : WhsItemFetchStrategy
	{
		public WhsItemDispatchTransportationUnitFetchStrategy(WhsItemDispatchTransportationUnit whsItemDispatchTransportationUnit)
			: base(whsItemDispatchTransportationUnit)
		{
		}

		protected override string JobFieldName => nameof(WhsItemDispatchTransportationUnit.JobHeader);
	}
}
