using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.US;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdjustmentLineValidationUS : WhsAdjustmentLineValidation
	{
		#region Constructors

		public WhsAdjustmentLineValidationUS(AutoWhsDocketLine parent)
			: base(parent)
		{
		}

		#endregion

		#region Overrides

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

		#region CheckWE_TransactionQuantity

		protected override void CheckWE_TransactionQuantity()
		{
			base.CheckWE_TransactionQuantity();

			if (Parent.IsCustomsTransaction)
			{
				Helper.CheckUnitsIsDivisibleByPerPackageQty(Parent.WE_TransactionQuantityInfo);
				Helper.CheckOnlyFullPackagesAreOrderedToBePicked(Parent.WE_TransactionQuantityInfo);
				Helper.CheckOnlyFullPackagesArePutawayIntoDestLocation(Parent.WE_TransactionQuantityInfo);
			}
		}

		#endregion

		#region CheckLocationString

		protected override void CheckLocationString()
		{
			base.CheckLocationString();

			if (Parent.IsAdjustmentIn)
			{
				WhsValidationHelper.CheckLocationIsTSAKnown(Parent, Parent.LocationStringInfo);
			}
		}

		#endregion

		#region CheckWE_PerPackageQty

		protected override void CheckWE_PerPackageQty()
		{
			base.CheckWE_PerPackageQty();

			Helper.CheckIfPackageIDIsEnteredPerPackageQtyIsAlsoEntered(Parent.WE_PerPackageQtyInfo);
		}

		#endregion

		#endregion

		#region Helper

		AdjustmentLineValidationHelper Helper
		{
			get { return helper ?? (helper = new AdjustmentLineValidationHelper(Parent)); }
		}

		AdjustmentLineValidationHelper helper;

		#endregion

		#region Implementation

		protected ZString LocationIsNotKnownByTSAErrorMessage
		{
			get { return Res.GetString("c0078abf-b4a0-4129-9fe6-31ad2f699293", "This location is not known by TSA."); }
		}

		protected ZString LocationIsKnownByTSAErrorMessage
		{
			get { return Res.GetString("8f4f0c30-e8c0-4d20-b15a-e06563ea557c", "This location is known by TSA."); }
		}

		#endregion
	}
}
