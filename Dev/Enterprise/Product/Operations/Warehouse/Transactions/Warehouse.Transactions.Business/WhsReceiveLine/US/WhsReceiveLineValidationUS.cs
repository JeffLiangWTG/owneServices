using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business.US
{
	public class WhsReceiveLineValidationUS : WhsReceiveLineValidation
	{
		public WhsReceiveLineValidationUS(WhsReceiveLine parent)
			: base(parent)
		{
		}

		#region CheckHeldCodeChangeQuantity

		protected override void CheckHeldCodeChangeQuantityCore()
		{
			base.CheckHeldCodeChangeQuantityCore();
			Helper.CheckHoldCodeChangeQtyIsDivisibleByPerPackageQty(Parent);
		}

		#endregion

		#region CheckHeldCodeToChangeTo

		protected override void CheckHoldCodeToChangeToCore()
		{
			base.CheckHoldCodeToChangeToCore();
			Helper.CheckNotChangingHoldCodeOfInventoryInAPackageGroup(Parent);
		}

		#endregion

		#region CheckPackageGroupId

		protected override void CheckWE_PackageGroupId()
		{
			base.CheckWE_PackageGroupId();
			CheckPackageGroupIDIsUniqueAcrossCurrentStock();
		}

		void CheckPackageGroupIDIsUniqueAcrossCurrentStock()
		{
			if (!Parent.WE_PackageGroupIdInfo.HasErrors())
			{
				var receive = Parent.Docket;
				if (receive != null
						&& receive.IsCustomsTransaction
						&& receive.IsFinalising
						&& Parent.WE_StockOnHand > 0
						&& !Parent.WE_PackageGroupIdInfo.HasErrors()
						&& !Parent.WE_PackageGroupId.IsEmpty
						&& receive.IsPackageGroupAlreadyAssigned(Parent.WE_PackageGroupId)
					)
				{
					Parent.WE_PackageGroupIdInfo.AddError(Res.GetString("5d2a9ae5-6162-453b-927c-3bcef6b14e20", "Package Group ID previously assigned. Assign new unique Package Group ID."));
				}
			}
		}

		#endregion

		#region CheckWE_PerPackageQty

		protected override void CheckWE_PerPackageQty()
		{
			base.CheckWE_PerPackageQty();

			MandatoryValidation.CheckNotNegative(Parent.WE_PerPackageQtyInfo);
			Helper.CheckIfPackageIDIsEnteredPerPackageQtyIsAlsoEntered(Parent.WE_PerPackageQtyInfo);
			Helper.CheckUnitsIsDivisibleByPerPackageQty(Parent.WE_PerPackageQtyInfo);
			Helper.CheckPerPackageQtyDoesNotHaveMoreDecimalsThanSpecifiedOnProduct(Parent.WE_PerPackageQtyInfo, Parent.SupplierPart);
			Helper.CheckSameProductsWithSamePackageGroupIDHaveSamePerPackageQty(Parent.WE_PerPackageQtyInfo);
		}

		#endregion

		#region CheckWE_TransactionQuantity

		protected override void CheckWE_TransactionQuantity()
		{
			base.CheckWE_TransactionQuantity();

			Helper.CheckUnitsIsDivisibleByPerPackageQty(Parent.WE_TransactionQuantityInfo);
			Helper.CheckAllSameProductsWithSamePackageGroupIDHaveSameTotalPacks(Parent.WE_TransactionQuantityInfo, Parent.WE_PerPackageQtyInfo);
		}

		#endregion

		#region CheckSplitQuantity

		protected override void CheckSplitQuantity()
		{
			base.CheckSplitQuantity();
			WhsReceiveLineValidationHelperUS.CheckSplitQuantity(Parent.SplitQuantityInfo, Parent.WE_TransactionQuantity, Parent.WE_PerPackageQty, Parent.WE_PackageGroupId);
		}

		#endregion

		#region CheckWE_WL

		protected override void CheckWE_WL()
		{
			base.CheckWE_WL();

			if (Parent.IsPutaway)
			{
				WhsValidationHelper.CheckLocationIsTSAKnown(Parent, Parent.WE_WLInfo);
			}
		}

		#endregion

		#region Helper

		WhsReceiveLineValidationHelperUS Helper => helper ?? (helper = new WhsReceiveLineValidationHelperUS(Parent));
		WhsReceiveLineValidationHelperUS helper;

		#endregion
	}
}
