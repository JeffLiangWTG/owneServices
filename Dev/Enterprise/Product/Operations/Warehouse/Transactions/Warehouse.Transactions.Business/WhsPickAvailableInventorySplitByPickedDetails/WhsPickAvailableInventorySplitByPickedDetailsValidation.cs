using System;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickAvailableInventorySplitByPickedDetailsValidation : WhsPickAvailableInventorySplitBaseValidation
	{
		public WhsPickAvailableInventorySplitByPickedDetailsValidation(WhsPickAvailableInventorySplitByPickedDetails parent)
			: base(parent)
		{
		}

		public override Type AutoValidationType => typeof(WhsPickAvailableInventorySplitByPickedDetailsValidation);
	}
}
