namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsComponentOrderLookups : WhsPickableDocketLookups
	{
		protected WhsComponentOrderLookups(WhsComponentOrder parent)
			: base(parent)
		{
		}
	}
}
