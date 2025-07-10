namespace Enterprise.Warehouse.Transit.Business
{
	public sealed class WhsItemReceiveTransportationUnitFetchStrategy : WhsItemFetchStrategy
	{
		public WhsItemReceiveTransportationUnitFetchStrategy(WhsItemReceiveTransportationUnit whsItemReceiveTransportationUnit)
			: base(whsItemReceiveTransportationUnit)
		{
		}

		protected override string JobFieldName => nameof(WhsItemReceiveTransportationUnit.JobHeader);
	}
}
