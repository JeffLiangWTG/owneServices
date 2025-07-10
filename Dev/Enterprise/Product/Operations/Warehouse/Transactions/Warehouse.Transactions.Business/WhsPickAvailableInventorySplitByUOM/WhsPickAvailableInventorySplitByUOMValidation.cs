using System;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickAvailableInventorySplitByUOMValidation : WhsPickAvailableInventorySplitBaseValidation
	{
		public WhsPickAvailableInventorySplitByUOMValidation(WhsPickAvailableInventorySplitByUOM uomAvailableInventory)
			: base(uomAvailableInventory)
		{
		}

		public override Type AutoValidationType => typeof(WhsPickAvailableInventorySplitByUOMValidation);
	}
}
