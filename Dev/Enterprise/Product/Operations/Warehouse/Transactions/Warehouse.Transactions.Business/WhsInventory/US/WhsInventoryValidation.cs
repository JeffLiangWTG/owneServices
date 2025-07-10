namespace Enterprise.Warehouse.Transactions.Business.US
{
	public class WhsInventoryValidation : WhsInventoryViewValidation
	{
		#region Constructors

		public WhsInventoryValidation(WhsInventoryView parent)
			: base(parent)
		{
		}

		#endregion

		// persistent

		#region CheckWI_InDocketLineUnits

		protected override void CheckWI_InDocketLineUnits()
		{
			base.CheckWI_InDocketLineUnits();

			Helper.CheckUnitsIsDivisibleByPerPackageQty(Parent.WI_InDocketLineUnitsInfo);
		}

		#endregion

		// calculated

		#region CheckWI_SplitQuantity

		protected override void CheckWI_SplitQuantity()
		{
			base.CheckWI_SplitQuantity();
			WhsReceiveLineValidationHelperUS.CheckSplitQuantity(Parent.WI_SplitQuantityInfo, Parent.WI_InDocketLineUnits, Parent.PerPackageQty, Parent.PackageGroupId);
		}

		#endregion

		//

		#region Implementation

		InventoryValidationHelper Helper
		{
			get { return helper ?? (helper = new InventoryValidationHelper(Parent)); }
		}

		InventoryValidationHelper helper;

		#endregion
	}
}
