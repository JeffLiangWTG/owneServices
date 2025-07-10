using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed class WhsPickAvailableInventorySplitByPickedDetails : WhsPickAvailableInventorySplitBase
	{
		public WhsPickAvailableInventorySplitByPickedDetails(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Lookups

		protected override WhsPickAvailableInventorySplitBaseLookups GetNewLookups() => new WhsPickAvailableInventorySplitByPickedDetailsLookups(this);

		#endregion

		#region Validation

		public override WhsPickAvailableInventorySplitBaseValidation GetNewValidation() => new WhsPickAvailableInventorySplitByPickedDetailsValidation(this);

		#endregion
	}
}
