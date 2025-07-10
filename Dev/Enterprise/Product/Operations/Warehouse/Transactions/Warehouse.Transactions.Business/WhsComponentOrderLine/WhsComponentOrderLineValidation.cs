namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsComponentOrderLineValidation : WhsPickableDocketLineValidation
	{
		protected WhsComponentOrderLineValidation(WhsComponentOrderLine parent)
			: base(parent)
		{
		}
	}
}
