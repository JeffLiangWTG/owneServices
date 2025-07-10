namespace Enterprise.Warehouse.Transit.Business
{
	public sealed class WhsItemReceiveConsignmentFetchStrategy : WhsItemFetchStrategy
	{
		public WhsItemReceiveConsignmentFetchStrategy(WhsItemReceiveConsignment whsItemReceiveConsignment)
			: base(whsItemReceiveConsignment)
		{
		}

		protected override string JobFieldName => nameof(WhsItemReceiveConsignment.JobHeader);
	}
}
