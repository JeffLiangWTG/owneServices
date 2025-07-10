namespace Enterprise.Warehouse.Transactions.Business.US
{
	public class WhsOrderLineValidationUS : WhsOrderLineValidation
	{
		public WhsOrderLineValidationUS(WhsOrderLine parent)
			: base(parent)
		{
		}

		#region CheckWE_TransactionQuantity

		protected override void CheckWE_TransactionQuantity()
		{
			base.CheckWE_TransactionQuantity();

			Helper.CheckUnitsIsDivisibleByPerPackageQty(Parent.WE_TransactionQuantityInfo);
			Helper.CheckOnlyFullPackagesAreOrderedToBePicked(Parent.WE_TransactionQuantityInfo);
		}

		#endregion

		#region CheckWE_PackageGroupId

		protected override void CheckWE_PackageGroupId()
		{
			base.CheckWE_PackageGroupId();
			Helper.ChechOrderingNonExistingPackageGroupID(Parent.WE_PackageGroupIdInfo);
		}

		#endregion

		#region  Implementations

		OrderLineValidationHelperUS Helper
		{
			get { return helper ?? (helper = new OrderLineValidationHelperUS(Parent)); }
		}

		OrderLineValidationHelperUS helper;

		#endregion
	}
}
