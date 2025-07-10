using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class ReservedPickLineCollection : WhsPickLineCollection
	{
		public ReservedPickLineCollection(WhsPickableDocketLine master)
			: this(master, WhsPickLineSchema.WZ_WE_TransactionLine)
		{
		}

		public ReservedPickLineCollection(WhsInventoryView master)
			: this(Argument.NotNull(master.InDocketLine, "master.InDocketLine"), WhsPickLineSchema.WZ_WE_InventoryLine)
		{
		}

		public ReservedPickLineCollection(WhsDocketLine master, SchemaGuidColumn relationshipColumn)
			: base(master, GetReservedPickLinesFilter(), relationshipColumn)
		{
		}

		public static void InvalidateAll(BusinessObjectFactory factory) => InvalidateAll(typeof(ReservedPickLineCollection), factory);

		static ZQuery GetReservedPickLinesFilter()
		{
			var result = new ZQuery();

			result.AddToFilter(WhsPickLineSchema.WZ_OriginalReservedQty, SQLComparisonOperator.GreaterThan, 0m);
			result.AddToFilter(WhsPickLineSchema.WZ_PickedDateTime, null);

			return result;
		}

		protected override bool MatchesFilterCore(WhsPickLine element, bool fetchOnlyFromLocalCache)
		{
			bool matchesFilter = base.MatchesFilterCore(element, fetchOnlyFromLocalCache);
			if (matchesFilter)
			{
				var docketLine = element.DocketLine as WhsPickableDocketLine;
				matchesFilter = docketLine != null && docketLine.IsDocketUnpicked;
			}

			return matchesFilter;
		}
	}
}
