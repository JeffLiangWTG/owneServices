namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsBondedWarehouseAttributeValidationForTransfers : WhsBondedWarehouseAttributeValidation
	{
		public WhsBondedWarehouseAttributeValidationForTransfers(WhsBondedWarehouseAttribute parent)
			: base(parent)
		{
		}

		protected override bool ShouldCheckIfEntryNumberIsEntered
		{
			get { return false; }
		}
	}
}
