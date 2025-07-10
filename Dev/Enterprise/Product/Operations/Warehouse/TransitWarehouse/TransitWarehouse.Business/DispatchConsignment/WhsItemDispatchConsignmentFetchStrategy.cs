namespace Enterprise.Warehouse.Transit.Business
{
	public sealed class WhsItemDispatchConsignmentFetchStrategy : WhsItemFetchStrategy
	{
		public WhsItemDispatchConsignmentFetchStrategy(WhsItemDispatchConsignment whsItemDispatchConsignment)
			: base(whsItemDispatchConsignment)
		{
		}

		protected override string JobFieldName => nameof(WhsItemDispatchConsignment.JobHeader);
	}
}
