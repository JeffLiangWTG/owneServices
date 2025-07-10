using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business.US
{
	class WhsStocktakeLineValidationForManuallyAddedLinesUS : WhsStocktakeLineValidationForManuallyAddedLines
	{
		public WhsStocktakeLineValidationForManuallyAddedLinesUS(WhsStocktakeLine parent)
			: base(parent)
		{
		}

		#region CheckWU_PackageGroupId

		protected override void CheckWU_PackageGroupId()
		{
			base.CheckWU_PackageGroupId();
			CheckPackageGroupIDIsClosedAllAtOnce();
		}

		void CheckPackageGroupIDIsClosedAllAtOnce()
		{
			if (Parent.IsClosing && !Parent.WU_PackageGroupId.IsEmpty)
			{
				var stocktake = Parent.Stocktake;
				var matchingClosingLines = stocktake.ClosingLines.Where(l => l.WU_PackageGroupId.EqualsIgnoringCase(Parent.WU_PackageGroupId));
				var matchingStocktakeLines = stocktake.Lines.Where(l => l.WU_PackageGroupId.EqualsIgnoringCase(Parent.WU_PackageGroupId));

				var sumOfClosingLines = matchingClosingLines.Sum(l => l.CurrentCount);
				var sumOfAllMatchingLines = matchingStocktakeLines.Sum(l => l.CurrentCount);
				if (sumOfAllMatchingLines > sumOfClosingLines)
				{
					Parent.WU_PackageGroupIdInfo.AddError(Res.GetString("C1EB9F70-11CB-497D-A7FE-C1CE30F7A74E", "All Stocktake Lines in the same Package Group must be closed together."));
				}
			}
		}

		//#warning
		// Not sure we actually need this
		//void CheckPackageGroupIDIsUniqueAcrossCurrentStock()
		//{
		//	if (Parent.IsClosing)
		//	{
		//		Helper.CheckPackageGroupIDIsUniqueAcrossCurrentStock(Parent.WU_PackageGroupIdInfo);
		//	}
		//}

		#endregion

		#region CheckWU_PerPackageQty

		protected override void CheckWU_PerPackageQty()
		{
			base.CheckWU_PerPackageQty();

			MandatoryValidation.CheckNotNegative(Parent.WU_PerPackageQtyInfo);
			Helper.CheckIfPackageIDIsEnteredPerPackageQtyIsAlsoEntered(Parent.WU_PerPackageQtyInfo);
			Helper.CheckUnitsIsDivisibleByPerPackageQty(Parent.WU_PerPackageQtyInfo);
			Helper.CheckPerPackageQtyDoesNotHaveMoreDecimalsThanSpecifiedOnProduct(Parent.WU_PerPackageQtyInfo, Parent.SupplierPart);
			Helper.CheckSameProductsWithSamePackageGroupIDHaveSamePerPackageQty(Parent.WU_PerPackageQtyInfo);
		}

		#endregion
	}
}
