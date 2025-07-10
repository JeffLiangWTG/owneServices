using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ZArchitecture.Business.ModuleDateFilter;

namespace Enterprise.Customs.GUI.Guarantees
{
	public class GuaranteeTransactionFilterStripBusinessObject : FilterStripBusinessObject
	{
		public GuaranteeTransactionFilterStripBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "CusPermitLineTransaction";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "FilterConstants")]
		public static class FilterConstants
		{
			public const string TransactionDate = "Transaction Date";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTransactionDateFilter(filters);
			return filters;
		}

		void AddTransactionDateFilter(ModuleFilterCollection filters)
		{
			var transactionDateFilter = filters.AddDateFilter(FilterConstants.TransactionDate, GetTransactionDateQuery);
			transactionDateFilter.MultilingualDescription = ResString.GetMultilingualString("GuaranteesFilter|TransactionDate", FilterConstants.TransactionDate);
			transactionDateFilter.Category = FilterCategories.Dates;
			transactionDateFilter.PropertySearch = DateRangeSearchTexts.LastMonth;
		}

		ZQuery GetTransactionDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var cusPermitLineReferenceQuery = new ZQuery();
			AddDateTimeRange(cusPermitLineReferenceQuery, comparisonOperator, JoinCondition.And, CusPermitLineTransactionSchema.CPL_TransactionDate, fromDate, toDate, false);
			return cusPermitLineReferenceQuery;
		}
	}
}
