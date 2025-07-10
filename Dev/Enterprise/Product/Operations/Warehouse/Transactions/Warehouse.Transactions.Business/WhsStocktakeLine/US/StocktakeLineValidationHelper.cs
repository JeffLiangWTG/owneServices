using System.Linq;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.US
{
	// tested in WhsStocktakeLineValidation && WhsStocktakeLineValidationForManuallyAddedLinesUS
	class StocktakeLineValidationHelper : WhsValidationHelperUS<WhsStocktakeLine>
	{
		public StocktakeLineValidationHelper(WhsStocktakeLine stocktakeLine)
			: base(stocktakeLine)
		{
		}

		protected override ZGuid ClientPK => LineAttributes.WU_OH_Client;

		protected override ZGuid ProductPK => LineAttributes.WU_OP;

		protected override ZGuid LocationPK => LineAttributes.WU_WL;

		protected override ZDecimal Units => LineAttributes.CurrentCount;

		protected override ZDecimal PerPackageQty => LineAttributes.WU_PerPackageQty;

		protected override ZString PackageGroupID => LineAttributes.WU_PackageGroupId;

		protected override IValidateParentWithLines ProductPackageTotalsParent => LineAttributes.Stocktake;

		protected override string UnitsNotDivisibleByPerPackageQtyErrorMessage
		{
			get { return Res.GetString("e4749732-7dd4-4221-bb3f-de3b40eb4b27", "Current Units must be divisible by Per Group Quantity."); }
		}

		protected override WhsValidationHelperUS<WhsStocktakeLine> GetHelper(WhsStocktakeLine stocktakeLine)
		{
			return new StocktakeLineValidationHelper(stocktakeLine);
		}

		protected override WhsStocktakeLine[] GetSiblings()
		{
			var stocktake = LineAttributes.Stocktake;
			return stocktake != null ? stocktake.Lines.Where(s => !s.IsClosed).ToArray() : base.GetSiblings();
		}
	}
}
