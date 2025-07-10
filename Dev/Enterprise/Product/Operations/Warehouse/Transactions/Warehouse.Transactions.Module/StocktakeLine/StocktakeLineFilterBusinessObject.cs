using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class StocktakeLineFilterBusinessObject : FilterStripBusinessObject
	{
		/// <summary>
		/// Use this constructor only when GridColorSchemeManager wants to retrieve filters
		/// </summary>
		public StocktakeLineFilterBusinessObject()
		{
		}

		public StocktakeLineFilterBusinessObject(WhsStocktake stocktake)
		{
			Argument.NotNull(stocktake, "StocktakeBizO"); // for developers only

			((IFilterStripBusinessObjectInternals)this).LayoutContext = ModuleIDs.WhsStocktakeLine.Name;
			StocktakeBizO = stocktake;
		}

		#region FilterNames

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter code")]
		public static class FilterNames
		{
			public const string Status = "Status";
			public const string Row = "Row";
			public const string PickArea = "PickAreaName";
			public const string PickMethod = "PickMethod";
			public const string NotCounted = "NotCounted";
			public const string OddEven = "OddEvenColumns";
			public const string EmptyLocations = "EmptyLocationsOnly";
			public const string ManuallyAddedLines = "ManuallyAdded";
			public const string CountComparison = "CountComparison";
			public const string Column = "Column";
			public const string Level = "Level";
			public const string Tray = "Tray";
		}

		#endregion

		#region CountComparisonOptions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public static class CountValuesComparisonOptions
		{
			public const string LastCountGreaterThanSystemCount = "Last Count > System Count";
			public const string LastCountEqualToSystemCount = "Last Count = System Count";
			public const string LastCountLessThanSystemCount = "Last Count < System Count";
		}

		#endregion

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddStatusFilters(filters);
			AddRowFilters(filters);
			AddAreaNameFilters(filters);
			AddPickMethodFilters(filters);
			AddNotCountedFilters(filters);
			AddOddEvenFilters(filters);
			AddEmptyLocationsFilters(filters);
			AddManuallyAddedLineFilters(filters);
			AddCountComparisonFilters(filters);
			AddColumnFilters(filters);
			AddLevelFilters(filters);
			AddTrayFilters(filters);

			return filters;
		}

		#region AddStatusFilters

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(FilterNames.Status, GetStatusQuery, new StocktakeLineStatus());
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("83ea9563-7fd3-4814-9b3a-40be77e690b2", "Status");
		}

		#endregion

		#region AddRowFilters

		void AddRowFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(FilterNames.Row, GetRowQuery);
			filter.Category = FilterCategories.Locations;
			filter.MaxLength = WhsRowSchema.WR_Name.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("7dac51db-0e29-42e7-a39e-05513e91a327", "Row");
		}

		#endregion

		#region AddAreaNameFilters

		void AddAreaNameFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(FilterNames.PickArea, GetAreaNameQuery);
			filter.Category = FilterCategories.Locations;
			filter.MaxLength = WhsAreaSchema.WA_Name.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("StocktakeLineFilterBusinessObject|PickAreaName", "Pick Area Name");
		}

		#endregion

		#region AddPickMethodFilters

		void AddPickMethodFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(FilterNames.PickMethod, GetPickMethodsQuery, PickMethods);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MaxLength = WhsLocationViewSchema.WLV_PickMethod.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("0e2a0c8a-e53c-443a-b2b2-414638f590f9", "Pick Method");
		}

		#endregion

		#region AddNotCountedFilters

		void AddNotCountedFilters(ModuleFilterCollection filters)
		{
			var notCountedDescription = new string[] { Res.GetString("c3c66009-b26c-4b5d-9441-93900c42fbe6", "0 Count Only") };
			var notCountedQuery = new GetFlagsQuery[] { GetNotCountedStocktakeLinesQuery };
			var filter = filters.AddFlagsFilter(FilterNames.NotCounted, notCountedDescription, notCountedQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("365bd998-6ae7-452d-ba72-46442400cbe4", "Last Count (Only Open Lines)");
		}

		#endregion

		#region AddOddEvenFilters

		void AddOddEvenFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(FilterNames.OddEven, GetOddEvenColumnsQuery, new LocationColumnNumber());
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("b16b0f63-87c0-4fb4-990a-75a8c7a7a5ea", "Location Odd/Even");
		}

		#endregion

		#region AddEmptyLocationsFilters

		void AddEmptyLocationsFilters(ModuleFilterCollection filters)
		{
			if (StocktakeBizO != null && StocktakeBizO.WS_CountEmptyLocationsCategory == CountEmptyLocationCategory.Codes.IncludeEmptyLocations)
			{
				var emptyLocationDescription = new string[] { Res.GetString("2e2351c3-9ab9-40b2-b6c6-242ce80bffdb", "Empty Locations Only") };
				var emptyLocationQuery = new GetFlagsQuery[] { GetEmptyLocations };
				var filter = filters.AddFlagsFilter(FilterNames.EmptyLocations, emptyLocationDescription, emptyLocationQuery);
				filter.MultilingualDescription = ResString.GetMultilingualString("2e2351c3-9ab9-40b2-b6c6-242ce80bffdb", "Empty Locations Only");
			}
		}

		ZQuery GetEmptyLocations(ZBool value)
		{
			var query = new ZQuery();

			if (value)
			{
				query.AddToFilter(WhsStocktakeLineSchema.WU_OP, null);
			}

			return query;
		}

		#endregion

		#region AddManuallyAddedLineFilters

		void AddManuallyAddedLineFilters(ModuleFilterCollection filters)
		{
			var description = new string[] { Res.GetString("9d3327b8-d843-4d96-9336-d65146931eeb", "Manually Added Line") };
			var query = new GetFlagsQuery[] { GetManuallyAddedStocktakeLinesQuery };
			var filter = filters.AddFlagsFilter(FilterNames.ManuallyAddedLines, description, query);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("cfbc9748-2fce-467e-82e9-6aa94ef842c9", "Manually Added Line");
		}

		#endregion

		#region AddCountComparisonFilters

		void AddCountComparisonFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(FilterNames.CountComparison, GetCountsComparisonQuery, CountComparisonOptions);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("d8a82134-f19f-4c20-9d6e-f012cca158b7", "Count Comparison");
		}

		#endregion

		#region AddColumnFilters

		void AddColumnFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilterForExactComparison(FilterNames.Column, GetColumnsQuery);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("b4ea9681-a97c-469b-b6bd-db72a5ffac28", "Column");
		}

		#endregion

		#region AddLevelFilters

		void AddLevelFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilterForExactComparison(FilterNames.Level, GetLevelQuery);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("8e78deab-bbf9-487b-9c24-4921c61c783c", "Level");
		}

		#endregion

		#region AddTrayFilters

		void AddTrayFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilterForExactComparison(FilterNames.Tray, GetTrayQuery);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("7920f839-04d0-4e27-8ed8-fc011c9cd988", "Tray");
		}

		#endregion

		#endregion

		#region Queries

		#region GetPickMethodsQuery

		ZQuery GetPickMethodsQuery(SQLComparisonOperator comparisonOperator, ZString pickMethod)
		{
			var query = new ZDBOnlyQuery(typeof(WhsStocktakeLine));
			if (!pickMethod.EqualsIgnoringCase("ANY"))
			{
				var locationSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsStocktakeLineSchema.WU_WL);

				locationSubQuery.AddToFilter(WhsLocationViewSchema.WLV_PickMethod, comparisonOperator, pickMethod);
				query.AddSubQuery(locationSubQuery, JoinCondition.And);
			}
			return query;
		}

		#endregion

		#region GetStatusQuery

		ZQuery GetStatusQuery(ZString status)
		{
			var result = new ZQuery();

			if (status != StocktakeLineStatus.Codes.All)
			{
				result = new ZQuery(WhsStocktakeLineSchema.WU_Status, status);
			}

			return result;
		}

		#endregion

		#region GetRowQuery

		ZQuery GetRowQuery(SQLComparisonOperator comparisonOperator, ZString rowName)
		{
			var query = new ZDBOnlyQuery(typeof(WhsStocktakeLine));
			var locationSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsStocktakeLineSchema.WU_WL);
			var rowSubQuery = new ZDBOnlySubQuery(typeof(WhsRow), WhsLocationViewSchema.WLV_WR);

			rowSubQuery.AddToFilter(WhsRowSchema.WR_Name, comparisonOperator, rowName);
			locationSubQuery.AddSubQuery(rowSubQuery, JoinCondition.And);
			query.AddSubQuery(locationSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region GetAreaNameQuery

		ZQuery GetAreaNameQuery(SQLComparisonOperator comparisonOperator, ZString areaName)
		{
			var query = new ZDBOnlyQuery(typeof(WhsStocktakeLine));
			var locationSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsStocktakeLineSchema.WU_WL);
			var areaSubQuery = new ZDBOnlySubQuery(typeof(WhsArea), WhsLocationViewSchema.WLV_WA_PickingArea);

			areaSubQuery.AddToFilter(WhsAreaSchema.WA_Name, comparisonOperator, areaName);
			locationSubQuery.AddSubQuery(areaSubQuery, JoinCondition.And);
			query.AddSubQuery(locationSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region GetNotCountedStocktakeLinesQuery

		ZQuery GetNotCountedStocktakeLinesQuery(ZBool value)
		{
			var query = new ZQuery();

			if (value)
			{
				query.PopulateZeroCountStocktakeLinesQuery();
			}

			return query;
		}

		#endregion

		#region GetOddEvenColumnsQuery

		ZQuery GetOddEvenColumnsQuery(ZString option)
		{
			var query = new ZDBOnlyQuery(typeof(WhsStocktakeLine));

			var isZeroBased = StocktakeBizO.Warehouse.WW_LocationColumnsZeroBased;

			if ((option == LocationColumnNumber.Codes.Odd && !isZeroBased) || (option == LocationColumnNumber.Codes.Even && isZeroBased))
			{
				var locationSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsStocktakeLineSchema.WU_WL);
				locationSubQuery.AddFilterAndZSQLParameterCollection(WhsLocationViewSchema.Constants.WLV_Column + " % 2 = 1", null);
				query.AddSubQuery(locationSubQuery, JoinCondition.And);
			}
			else if ((option == LocationColumnNumber.Codes.Even && !isZeroBased) || (option == LocationColumnNumber.Codes.Odd && isZeroBased))
			{
				var locationSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsStocktakeLineSchema.WU_WL);
				locationSubQuery.AddFilterAndZSQLParameterCollection(WhsLocationViewSchema.Constants.WLV_Column + " % 2 <> 1", null);
				query.AddSubQuery(locationSubQuery, JoinCondition.And);
			}

			return query;
		}

		#endregion

		#region GetManuallyAddedStocktakeLinesQuery

		ZQuery GetManuallyAddedStocktakeLinesQuery(ZBool value)
		{
			var query = new ZQuery();
			query.AddToFilter(WhsStocktakeLineSchema.WU_IsManuallyAdded, value);
			return query;
		}

		#endregion

		#region GetCountsComparisonQuery

		ZQuery GetCountsComparisonQuery(ZString comparisonOption)
		{
			var query = new ZQuery();

			switch (comparisonOption)
			{
				case CountValuesComparisonOptions.LastCountGreaterThanSystemCount:
					PopulateCountComparisonStocktakeLinesQuery(query, SQLComparisonOperator.GreaterThan);
					break;
				case CountValuesComparisonOptions.LastCountEqualToSystemCount:
					PopulateCountComparisonStocktakeLinesQuery(query, SQLComparisonOperator.Equal);
					break;
				case CountValuesComparisonOptions.LastCountLessThanSystemCount:
					PopulateCountComparisonStocktakeLinesQuery(query, SQLComparisonOperator.LessThan);
					break;
			}

			return query;
		}

		void PopulateCountComparisonStocktakeLinesQuery(ZQuery subQuery, SQLComparisonOperator comparisonOperator)
		{
			PopulateCountStocktakeLinesQuery(subQuery, comparisonOperator, new ZByte(1), WhsStocktakeLineSchema.WU_LastCount, WhsStocktakeLineSchema.WU_DateVerified);
			PopulateCountStocktakeLinesQuery(subQuery, comparisonOperator, new ZByte(2), WhsStocktakeLineSchema.WU_Count2, WhsStocktakeLineSchema.WU_Count2DateVerified);
			PopulateCountStocktakeLinesQuery(subQuery, comparisonOperator, new ZByte(3), WhsStocktakeLineSchema.WU_Count3, WhsStocktakeLineSchema.WU_Count3DateVerified);
		}

		void PopulateCountStocktakeLinesQuery(ZQuery subQuery, SQLComparisonOperator comparisonOperator, ZByte countColumnNumber, SchemaColumn lastCount, SchemaColumn lastCountDateVerified)
		{
			var query = new ZQuery();
			query.AddToFilter(lastCount, comparisonOperator, WhsStocktakeLineSchema.WU_SystemUnits);
			AddVerifiedColumns(query, lastCount, lastCountDateVerified);
			query.AddToFilter(WhsStocktakeLineSchema.WU_TotalCounts, countColumnNumber);

			subQuery.AddToFilter(query, JoinCondition.Or);
		}

		void AddVerifiedColumns(ZQuery subQuery, SchemaColumn lastCount, SchemaColumn lastCountDateVerified)
		{
			var query = new ZQuery();
			query.AddToFilter(lastCountDateVerified, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
			query.AddToFilter(JoinCondition.Or, lastCount, SQLComparisonOperator.NotEqual, 0);
			query.AddToFilter(JoinCondition.Or, WhsStocktakeLineSchema.WU_Status, StocktakeLineStatus.Codes.Closed);
			subQuery.AddToFilter(query);
		}

		#endregion

		#region GetColumnsQuery

		ZQuery GetColumnsQuery(SQLComparisonOperator comparisonOperator, ZString column)
		{
			return LocationComponentQueryHelper.GetLocationParserSubQueryForStocktake(WhsLocationViewSchema.WLV_Column, WhsWarehouseSchema.WW_LocationColumnsAlpha, WhsWarehouseSchema.WW_LocationColumnsZeroBased, column);
		}

		#endregion

		#region GetLevelQuery

		ZQuery GetLevelQuery(SQLComparisonOperator comparisonOperator, ZString level)
		{
			return LocationComponentQueryHelper.GetLocationParserSubQueryForStocktake(WhsLocationViewSchema.WLV_Level, WhsWarehouseSchema.WW_LocationLevelsAlpha, WhsWarehouseSchema.WW_LocationLevelsZeroBased, level);
		}

		#endregion

		#region GetTrayQuery

		ZQuery GetTrayQuery(SQLComparisonOperator comparisonOperator, ZString tray)
		{
			return LocationComponentQueryHelper.GetLocationParserSubQueryForStocktake(WhsLocationViewSchema.WLV_Tray, WhsWarehouseSchema.WW_LocationTraysAlpha, WhsWarehouseSchema.WW_LocationTraysZeroBased, tray);
		}

		#endregion

		#endregion

		#region Lookups

		#region PickMethods

		ICodeDescriptionPairList PickMethods => WarehouseDataRegistry.Instance.PickMethod.Value;

		#endregion

		#region CountComparisonOptions

		ICodeDescriptionPairList CountComparisonOptions
		{
			get
			{
				return new CodeDescriptionPairList()
				{
					{ new CodeDescriptionPair(CountValuesComparisonOptions.LastCountGreaterThanSystemCount, ResString.GetMultilingualString("a7005d6f-0bbd-4401-9566-bb1898563b84", "Last Count > System Count")) },
					{ new CodeDescriptionPair(CountValuesComparisonOptions.LastCountEqualToSystemCount, ResString.GetMultilingualString("2e1b62fe-c512-4c88-b6f4-9c9c8fe93894", "Last Count = System Count")) },
					{ new CodeDescriptionPair(CountValuesComparisonOptions.LastCountLessThanSystemCount, ResString.GetMultilingualString("f31f4846-7480-4730-bee6-bb49db2c386b", "Last Count < System Count")) }
				};
			}
		}

		#endregion

		#endregion

		#region Error Messages

#if DEBUG
		public
#endif
		static string EmptyLocationsErrorMessage
			=> Res.GetString("66c39d18-ca84-4340-9544-d0a4417eafa0", "Empty location filter can only by used when a location limiting filter is also used, i.e. Can only be used with Row, Area, Pick Method, or Odd/Even filters.");

#if DEBUG
		public
#endif
		static string EmptyLocationsErrorMessageForColorSchemeConfiguration
			=> Res.GetString("aca69755-d8da-4b6e-be1d-a1c7c36f93d4", "Empty locations filter cannot be used during color scheme configuration. Please use Status filter instead.");

		#endregion

		readonly WhsStocktake StocktakeBizO;
	}

	#region StocktakeLineFilterExtensions

	static class StocktakeLineFilterExtensions
	{
		public static void PopulateZeroCountStocktakeLinesQuery(this ZQuery subQuery)
		{
			subQuery.PopulateZeroCountStocktakeLinesQuery(new ZByte(1), new SchemaColumn[] { WhsStocktakeLineSchema.WU_LastCount });
			subQuery.PopulateZeroCountStocktakeLinesQuery(new ZByte(2), new SchemaColumn[] { WhsStocktakeLineSchema.WU_LastCount, WhsStocktakeLineSchema.WU_Count2 });
			subQuery.PopulateZeroCountStocktakeLinesQuery(new ZByte(3), new SchemaColumn[] { WhsStocktakeLineSchema.WU_LastCount, WhsStocktakeLineSchema.WU_Count2, WhsStocktakeLineSchema.WU_Count3 });
		}

		static void PopulateZeroCountStocktakeLinesQuery(this ZQuery subQuery, ZByte countColumnNumber, SchemaColumn[] columns)
		{
			var query = new ZQuery();
			AddColumnFilters(query, columns);
			query.AddToFilter(WhsStocktakeLineSchema.WU_TotalCounts, countColumnNumber);
			query.AddToFilter(WhsStocktakeLineSchema.WU_Status, StocktakeLineStatus.Codes.Open);
			subQuery.AddToFilter(query, JoinCondition.Or);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Comparing different columns")]
		static void AddColumnFilters(ZQuery subQuery, SchemaColumn[] columns)
		{
			var query = new ZQuery();
			foreach (var column in columns) // foreach statement to compare different columns
			{
				query.AddToFilter(JoinCondition.Or, column, 0); // comparing different columns
			}
			subQuery.AddToFilter(query, JoinCondition.And);
		}
	}

	#endregion
}
