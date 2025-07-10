using CargoWise.ComponentModel;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsBondedWarehouseAttributeValidationForReceive : WhsBondedWarehouseAttributeValidation
	{
		public WhsBondedWarehouseAttributeValidationForReceive(WhsBondedWarehouseAttribute parent)
			: base(parent)
		{
		}

		#region CheckWB_EntryKey

		protected override void CheckWB_EntryKey()
		{
			base.CheckWB_EntryKey();

			var bondedWarehouseAttribute = Parent;
			if (!bondedWarehouseAttribute.WB_EntryKeyInfo.HasErrors() && !bondedWarehouseAttribute.WB_EntryKey.IsEmpty)
			{
				var receive = (WhsReceive)bondedWarehouseAttribute.Parent.Docket;
				if (receive.IsCustomsTransaction && (receive.ParentDocket is not WhsComponentOrder order || order.IsAssembly) && receive.IsMismatchedEntryKey(bondedWarehouseAttribute))
				{
					bondedWarehouseAttribute.WB_EntryKeyInfo.AddError(Res.GetString("5141827b-9ee2-4a88-869d-58b093b8023b", "Only one Entry Key allowed per Receive."));
				}
			}
		}

		protected override bool ShouldCheckIfEntryNumberIsEntered
		{
			get
			{
				var docketLine = Parent.Parent;
				return docketLine.WE_TransactionQuantity != 0m && !((WhsReceive)docketLine.Docket).IsCreatedFromWorkOrder;
			}
		}

		#endregion
	}
}
