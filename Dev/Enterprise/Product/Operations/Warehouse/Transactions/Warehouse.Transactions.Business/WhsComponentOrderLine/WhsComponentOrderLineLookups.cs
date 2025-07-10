namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsComponentOrderLineLookups : WhsPickableDocketLineLookups
	{
		protected WhsComponentOrderLineLookups(WhsComponentOrderLine parent)
			: base(parent)
		{
		}
	}
}
