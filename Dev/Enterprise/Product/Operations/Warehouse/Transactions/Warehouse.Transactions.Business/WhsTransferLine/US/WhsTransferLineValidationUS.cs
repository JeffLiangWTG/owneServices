using Enterprise.Warehouse.Transactions.Business.US;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsTransferLineValidationUS : WhsTransferLineValidation
	{
		#region Constructors

		public WhsTransferLineValidationUS(WhsTransferLine parent)
			: base(parent)
		{
		}

		#endregion

		#region CheckHeldCodeChangeQuantityCore

		protected override void CheckHeldCodeChangeQuantityCore()
		{
			base.CheckHeldCodeChangeQuantityCore();
			Helper.CheckHoldCodeChangeQtyIsDivisibleByPerPackageQty(Parent);
		}

		#endregion

		#region CheckHeldCodeToChangeToCore

		protected override void CheckHoldCodeToChangeToCore()
		{
			base.CheckHoldCodeToChangeToCore();
			Helper.CheckNotChangingHoldCodeOfInventoryInAPackageGroup(Parent);
		}

		#endregion

		#region CheckWE_PerPackageQty

		protected override void CheckWE_PerPackageQty()
		{
			base.CheckWE_PerPackageQty();
			Helper.CheckIfPackageIDIsEnteredPerPackageQtyIsAlsoEntered(Parent.WE_PerPackageQtyInfo);
		}

		#endregion

		#region CheckQtyToMoveIncludingMatchingLines

		protected override void CheckQtyToMoveIncludingMatchingLines()
		{
			base.CheckQtyToMoveIncludingMatchingLines();

			if (Parent.IsCustomsTransaction)
			{
				Helper.CheckUnitsIsDivisibleByPerPackageQty(Parent.QtyToMoveIncludingMatchingLinesInfo);
				Helper.CheckOnlyFullPackagesAreOrderedToBePicked(Parent.QtyToMoveIncludingMatchingLinesInfo);
				Helper.CheckOnlyFullPackagesArePutawayIntoDestLocation(Parent.QtyToMoveIncludingMatchingLinesInfo);
			}
		}

		#endregion

		#region CheckLocationString

		protected override void CheckLocationString()
		{
			base.CheckLocationString();

			WhsValidationHelper.CheckLocationIsTSAKnown(Parent, Parent.LocationStringInfo);
		}

		#endregion

		#region Helper

		TransferLineValidationHelper Helper
		{
			get { return helper ?? (helper = new TransferLineValidationHelper(Parent)); }
		}

		TransferLineValidationHelper helper;

		#endregion
	}
}
