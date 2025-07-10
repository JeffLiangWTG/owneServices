using CargoWise.ComponentModel;

namespace Enterprise.Warehouse.Transactions.Business.US
{
	class WhsPickLineValidationUS : WhsPickLineValidation
	{
		public WhsPickLineValidationUS(WhsPickLine parent)
			: base(parent)
		{
		}

		#region CheckReservedQuantity

		protected override void CheckReservedQuantityCore()
		{
			base.CheckReservedQuantityCore();

			if (!Parent.ReservedQuantityInfo.HasErrors())
			{
				var inventory = Parent.Inventory;
				if (inventory != null && inventory.PerPackageQty > 0m && Parent.ReservedQuantity % inventory.PerPackageQty != 0m)
				{
					Parent.ReservedQuantityInfo.AddError(Res.GetString("853fad5e-7ef1-458a-97e1-9561903e225e", "Reserved Quantity must be divisible by Per Group Qty."));
				}
			}
		}

		#endregion
	}
}
