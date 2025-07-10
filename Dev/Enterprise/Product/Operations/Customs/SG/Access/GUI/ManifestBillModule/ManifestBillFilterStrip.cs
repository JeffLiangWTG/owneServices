using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.Access.GUI
{
	public class ManifestBillFilterStrip : ASYCUDA.Module.ASYCUDAManifestBillFilterStrip
	{
		public ManifestBillFilterStrip()
			: base(false)
		{
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new ManifestBillFilterStrip();

		public static class SGFilterConstants
		{
			public const string BillCycleDate = "Bill Cycle Date";
			public const string BillCycleNumber = "Bill Cycle Number";
			public const string BillBatchDate = "Bill Batch Date";
			public const string BillBatchNumber = "Bill Batch Number";
			public const string PartyIdentifier = "Party Identifier";
			public const string PartyStatus = "Party Status";
			public const string PayeeIndicator = "Payee Indicator";
			public const string TotalGST = "Total GST";
			public const string TotalDuty = "Total Duty";
			public const string PackCustomsGoodsType = "Pack Customs Goods Type";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			var dateFilter = result.AddDateFilter(SGFilterConstants.BillCycleDate, GetBillCycleDateQuery);
			dateFilter.Category = FilterCategories.Dates;
			dateFilter.MultilingualDescription = ResString.GetMultilingualString("D3CDA164-3F36-4043-9EBC-E66679CF30C1",
				SGFilterConstants.BillCycleDate);

			var textFilter = result.AddTextFilter(SGFilterConstants.BillCycleNumber, GetBillCycleNumberQuery);
			textFilter.Category = FilterCategories.NumbersAndReferences;
			textFilter.MultilingualDescription = ResString.GetMultilingualString("13C369FE-C949-4D80-A9D0-A92EE0E195B0",
				SGFilterConstants.BillCycleNumber);

			dateFilter = result.AddDateFilter(SGFilterConstants.BillBatchDate, GetBillBatchDateQuery);
			dateFilter.Category = FilterCategories.Dates;
			dateFilter.MultilingualDescription = ResString.GetMultilingualString("E489DF07-BF90-4796-984A-C59A5B41F208",
				SGFilterConstants.BillBatchDate);

			textFilter = result.AddTextFilter(SGFilterConstants.BillBatchNumber, GetBillBatchNumberQuery);
			textFilter.Category = FilterCategories.NumbersAndReferences;
			textFilter.MultilingualDescription = ResString.GetMultilingualString("8CCA39F7-C138-45F3-BB2B-BB6FC3C78D6F",
				SGFilterConstants.BillBatchNumber);

			textFilter = result.AddTextFilter(SGFilterConstants.PartyIdentifier, (value, op) => GetSGQuery(value, op, AsycudaBill.Schema.SG_PartyID));
			textFilter.Category = FilterCategories.NumbersAndReferences;
			textFilter.MultilingualDescription = ResString.GetMultilingualString("2840B678-0C3D-4994-92BB-E2C1CF9DED24",
				SGFilterConstants.PartyIdentifier);

			textFilter = result.AddTextFilter(SGFilterConstants.PartyStatus, (value, op) => GetSGQuery(value, op, AsycudaBill.Schema.SG_PartyStatus), Business.AsycudaBillLookups.GetSGPartyStatusList(Factory));
			textFilter.Category = FilterCategories.StatusAndFlags;
			textFilter.MultilingualDescription = ResString.GetMultilingualString("8DF9EDA5-BDFB-4B6C-8189-7BEE6A0F638C",
				SGFilterConstants.PartyStatus);

			textFilter = result.AddTextFilter(SGFilterConstants.PayeeIndicator, (value, op) => GetSGQuery(value, op, AsycudaBill.Schema.SG_PayeeIndicator), Business.AsycudaBillLookups.GetSGPayeeIndicatorList(Factory));
			textFilter.Category = FilterCategories.NumbersAndReferences;
			textFilter.MultilingualDescription = ResString.GetMultilingualString("B88C24EC-D463-46D2-B6F4-0F4E2D9B99DC",
				SGFilterConstants.PayeeIndicator);

			var totalGSTFilter = GetFilterForGenAddOnColumn(SGFilterConstants.TotalGST, AsycudaBill.Schema.TaxAmount, AsycudaBillSchema.Constants.Prefix, typeof(AsycudaBill), null, 2);
			totalGSTFilter.Category = FilterCategories.FinancialDetails;
			totalGSTFilter.MultilingualDescription = ResString.GetMultilingualString("0A3872AB-FCDC-4656-A018-506E9D59B456",
				SGFilterConstants.TotalGST);
			result.AddFilter(totalGSTFilter);

			var totalDutyFilter = GetFilterForGenAddOnColumn(SGFilterConstants.TotalDuty, AsycudaBill.Schema.DutyAmount, AsycudaBillSchema.Constants.Prefix, typeof(AsycudaBill), null, 2);
			totalDutyFilter.Category = FilterCategories.FinancialDetails;
			totalDutyFilter.MultilingualDescription = ResString.GetMultilingualString("E10971B9-EA7F-4487-B92A-41C93977F03B",
				SGFilterConstants.TotalDuty);
			result.AddFilter(totalDutyFilter);

			var packCustomsGoodsTypeFilter = result.AddTextFilter(SGFilterConstants.PackCustomsGoodsType, GetGoodsTypeQuery, Business.AsycudaPackedItemLookups.GetGoodsTypeListForCountry(Factory, Core.Constants.CountryCodes.Singapore));
			packCustomsGoodsTypeFilter.MaxLength = AsycudaPackedItem.Schema.GoodsTypeMaxLength;
			packCustomsGoodsTypeFilter.Category = FilterCategories.ModesAndTypes;
			packCustomsGoodsTypeFilter.MultilingualDescription = ResString.GetMultilingualString("7010372D-E2F7-439E-9630-EB5E0CA5FC07",
				SGFilterConstants.PackCustomsGoodsType);

			return result;
		}

		ZQuery GetBillCycleDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var billCountryQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			var referenceDataSubQuery = BillCountryGenAddOnColumnHelper.GetQueryOnGenAddOnColumn(AsycudaBill.Schema.CycleDate, comparisonOperator, value1, value2);
			billCountryQuery.AddToFilter(referenceDataSubQuery);

			return billCountryQuery;
		}

		ZQuery GetBillCycleNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var billCountryQuery = new ZDBOnlyQuery(typeof(AsycudaBill));

			var referenceDataSubQuery = BillCountryGenAddOnColumnHelper.GetQueryHandlingBlanks(
				AsycudaBill.Schema.CycleNumber,
				comparisonOperator,
				value,
				comparisonOperator == SpecialComparisonOperator.IsBlank
			);
			billCountryQuery.AddToFilter(referenceDataSubQuery);

			return billCountryQuery;
		}

		ZQuery GetBillBatchDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var billCountryQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			var referenceDataSubQuery = BillCountryGenAddOnColumnHelper.GetQueryOnGenAddOnColumn(AsycudaBill.Schema.BatchDate, comparisonOperator, value1, value2);
			billCountryQuery.AddToFilter(referenceDataSubQuery);

			return billCountryQuery;
		}

		ZQuery GetBillBatchNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var billCountryQuery = new ZDBOnlyQuery(typeof(AsycudaBill));

			var referenceDataSubQuery = BillCountryGenAddOnColumnHelper.GetQueryHandlingBlanks(
				AsycudaBill.Schema.BatchNumber,
				comparisonOperator,
				value,
				comparisonOperator == SpecialComparisonOperator.IsBlank
			);
			billCountryQuery.AddToFilter(referenceDataSubQuery);

			return billCountryQuery;
		}

		ZQuery GetSGQuery(SQLComparisonOperator comparisonOperator, ZString value, string column)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaBill));

			var referenceDataSubQuery = BillCountryGenAddOnColumnHelper.GetQueryHandlingBlanks(column, comparisonOperator, value);
			result.AddToFilter(referenceDataSubQuery);

			return result;
		}

		ZQuery GetGoodsTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaBill));
			var packQuery = new ZDBOnlySubQuery(typeof(AsycudaPack), AsycudaPackSchema.PK);
			var packLineQuery = new ZDBOnlySubQuery(typeof(AsycudaPackedItem), AsycudaPackedItemSchema.PK);
			var referenceDataSubQuery = PackLineGenAddOnHelper.GetQueryOnGenAddOnColumn(AsycudaPackedItem.Schema.GoodsType, comparisonOperator, value);
			packLineQuery.AddToFilter(referenceDataSubQuery);

			var pivotQuery = new ZDBOnlySubQuery(typeof(ManifestBase.AsycudaPackPackedItemPivot), AsycudaPackPackedItemPivotSchema.PK);
			pivotQuery.AddSubQuery(AsycudaPackPackedItemPivotSchema.APP_API_Item, AsycudaPackedItemSchema.PK, packLineQuery, JoinCondition.And);

			packQuery.AddSubQuery(AsycudaPackSchema.PK, AsycudaPackPackedItemPivotSchema.APP_APA_Pack, pivotQuery, JoinCondition.And);
			result.AddSubQuery(AsycudaBillSchema.PK, AsycudaPackSchema.APA_ABL_Bill, packQuery, JoinCondition.And);
			return result;
		}
	}
}
