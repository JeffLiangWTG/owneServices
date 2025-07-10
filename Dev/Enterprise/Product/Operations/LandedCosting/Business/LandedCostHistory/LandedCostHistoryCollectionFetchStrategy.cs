using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.Business
{
	class LandedCostHistoryCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public LandedCostHistoryCollectionFetchStrategy(IBusinessObjectCollection collection) : base(collection)
		{
		}

		protected new LandedCostHistoryCollection Collection => (LandedCostHistoryCollection)base.Collection;

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			base.FetchForViewCore(businessObjects, columns);

			if (columns.Any(x => IsLandedLineCostItem(x.ColumnName)))
			{
				var factory = Collection.Factory;
				foreach (LandedCostHistory landedCostHistory in businessObjects)
				{
					factory.AddFetchHint(LandedLineCostItemSchema.LZ_LH, landedCostHistory.PK);
				}
			}
		}

		bool IsLandedLineCostItem(ZString columnName)
		{
			return columnName == LandedCostHistory.Schema.RoundedPerUnitCustomsDisbursementCharges || columnName == LandedCostHistory.Schema.RoundedPerUnitLandingCost ||
				columnName == LandedCostHistory.Schema.RoundedPerUnitTotalCost || columnName == LandedCostHistory.Schema.RoundedSellPrice1ExGST || columnName == LandedCostHistory.Schema.TotalCost ||
				columnName.StartsWith("LH_LandedCostGroup", System.StringComparison.Ordinal) ||
				columnName.StartsWith("RoundedSellPrice", System.StringComparison.Ordinal);
		}
	}
}
