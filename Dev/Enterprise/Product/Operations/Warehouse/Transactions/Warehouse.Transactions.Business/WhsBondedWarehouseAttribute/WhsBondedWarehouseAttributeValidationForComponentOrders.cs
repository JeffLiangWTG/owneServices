namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsBondedWarehouseAttributeValidationForComponentOrders : WhsBondedWarehouseAttributeValidation
	{
		public WhsBondedWarehouseAttributeValidationForComponentOrders(WhsBondedWarehouseAttribute parent)
			: base(parent)
		{
		}

		protected override bool ShouldCheckIfEntryNumberIsEntered => false;
	}
}
