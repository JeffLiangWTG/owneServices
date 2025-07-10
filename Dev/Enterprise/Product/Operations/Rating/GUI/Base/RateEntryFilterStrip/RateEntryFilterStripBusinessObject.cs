using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Rating.GUI
{
	/// <summary>
	/// The filters for a category tab on a RatingHeader form (e.g., the Air Freight tab on a Costing form).
	/// </summary>
	public class RateEntryFilterStripBusinessObject : FilterStripBusinessObject, IRateEntryFilterStripBusinessObject
	{
		public RateEntryFilterStripBusinessObject(RateEntryCollection collection)
		{
			Collection = collection;
			ratingHeader = collection.Parent;
			var filterCategory = collection.CategoryForFiltering;

			ShouldIncludeTACTRatesFilter = filterCategory.ToString().In(RatingConstants.RateCategory.AIR, RatingConstants.RateCategory.CAI, RatingConstants.RateCategory.SummaryRatesCategory)
				&& ratingHeader.IsStandardCostRate();
			((IFilterStripBusinessObjectInternals)this).LayoutContext =
				ShouldIncludeTACTRatesFilter
				? Invariant($"Rating_{filterCategory}_StandardCostRate")
				: Invariant($"Rating_{filterCategory}");

			Collection.SetUserFilter(this);
		}

		public RateEntryFilterStripBusinessObject()
		{
		}

		public RateEntryCollection Collection { get; }
		readonly RatingHeader ratingHeader;
		bool ShouldIncludeTACTRatesFilter { get; }

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			filters = new ModuleFilterCollection();

			AddDateFilters();
			AddCommonRateEntryFilters();
			AddIncludeTACTRatesFilter();
			AddIATARegionFilter(RateEntrySchema.TI_OriginLRC);
			AddIATARegionFilter(RateEntrySchema.TI_DestinationLRC);
			if (!ratingHeader.IsIntercompanyTariff())
			{
				AddIATARegionFilter(RateEntrySchema.TI_ViaLRC);
			}
			AddLocationTypeFilter(RateEntrySchema.TI_OriginLRC);
			AddLocationTypeFilter(RateEntrySchema.TI_DestinationLRC);
			AddIncludeGlobalRatesFilter();
			AddPublisherFilter();

			ModuleAuditFilterProvider.AddAuditFilters(filters, RateEntrySchema.Instance, Factory, typeof(RateEntry));

			AddRateLineFilters();
			AddShowAllRateLinesFilter();

			return filters;
		}

		ModuleFilterCollection filters;

		void AddDateFilters()
		{
			showExpiredFilter = filters.AddFlagsFilter(RateEntryFilterUtility.Constants.Codes.ShowExpired, new string[] { RateEntryFilterUtility.Constants.Description.ShowExpired }, new GetFlagsQuery[] { GetShowExpiredQuery });
			showExpiredFilter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.ShowExpired;
			showExpiredFilter.Category = FilterCategories.Dates;
			if (ratingHeader.IsQuote())
			{
				showExpiredFilter.Property0 = true;
			}
		}

		ModuleFlagsFilter showExpiredFilter;

		#region Adding Include TACT Rates Filter

		void AddIncludeTACTRatesFilter()
		{
			if (filters != null && ShouldIncludeTACTRatesFilter)
			{
				var includeTACTRatesFilter = filters.AddTextFilter(RateEntryFilterUtility.Constants.Codes.IncludeTACTRates, GetIncludeTACTRatesQuery, IncludeTACTRatesList);
				includeTACTRatesFilter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.IncludeTACTRates;
				includeTACTRatesFilter.Category = FilterCategories.ModesAndTypes;
				includeTACTRatesFilter.DefaultProperty = IncludeTACTRatesConstants.Code.STD;
			}
		}

		ZQuery GetIncludeTACTRatesQuery(ZString includeTACTRates)
		{
			var result = new ZQuery();

			if (includeTACTRates == IncludeTACTRatesConstants.Code.STD)
			{
				result.AddToFilter(RateEntrySchema.TI_IsTact, false);
			}
			else if (includeTACTRates == IncludeTACTRatesConstants.Code.TAC)
			{
				result.AddToFilter(RateEntrySchema.TI_IsTact, true);
			}

			return result;
		}

		CodeDescriptionPairList IncludeTACTRatesList
		{
			get
			{
				var list = new CodeDescriptionPairList();

				list.AddPair(IncludeTACTRatesConstants.Code.STD, IncludeTACTRatesConstants.Description.STD);
				list.AddPair(IncludeTACTRatesConstants.Code.TAC, IncludeTACTRatesConstants.Description.TAC);
				list.AddPair(IncludeTACTRatesConstants.Code.ALL, IncludeTACTRatesConstants.Description.ALL);

				return list;
			}
		}

		public static class IncludeTACTRatesConstants
		{
			public static class Code
			{
				public const string STD = nameof(STD);
				public const string TAC = nameof(TAC);
				public const string ALL = nameof(ALL);
			}

			public static class Description
			{
				public static readonly ResourceString STD = ResString.GetMultilingualString("04BBB47A-AB04-4739-9C62-B30FE9A68412", "Show Non-TACT Rates");
				public static readonly ResourceString TAC = ResString.GetMultilingualString("5A26771C-40A3-4B4E-A0F7-6A00A4228C5C", "Show TACT Rates");
				public static readonly ResourceString ALL = ResString.GetMultilingualString("2368A26C-32FF-4FD0-BE8C-CA5E021B8795", "Show All Rates");
			}
		}

		#endregion

		#region Adding Origin/Destination/Via IATA Region Filter

		void AddIATARegionFilter(SchemaColumn column)
		{
			if (filters != null && ShouldIncludeTACTRatesFilter)
			{
				var (code, description) = GetIATARegionFilterCodeDescription(column);

				var iataRegionFilter = filters.AddTextFilter(code, x => GetIATARegionQuery(column, x), GetIATACityCodeDescriptionPairList);
				iataRegionFilter.MultilingualDescription = description;
				iataRegionFilter.Category = FilterCategories.Locations;
				iataRegionFilter.MaxLength = 3;
			}
		}

		(string code, ResourceString description) GetIATARegionFilterCodeDescription(SchemaColumn column)
		{
			switch (column.Name)
			{
				case RateEntry.Schema.TI_OriginLRC:
					return (RateEntryFilterUtility.Constants.Codes.OriginIATARegion, RateEntryFilterUtility.Constants.Description.OriginIATARegion);
				case RateEntry.Schema.TI_DestinationLRC:
					return (RateEntryFilterUtility.Constants.Codes.DestinationIATARegion, RateEntryFilterUtility.Constants.Description.DestinationIATARegion);
				case RateEntry.Schema.TI_ViaLRC:
					return (RateEntryFilterUtility.Constants.Codes.ViaIATARegion, RateEntryFilterUtility.Constants.Description.ViaIATARegion);
				default:
					throw new NotSupportedException();
			}
		}

		ZQuery GetIATARegionQuery(SchemaColumn column, ZString iataRegion)
		{
			var result = new ZDBOnlyQuery(typeof(RateEntry));

			result.AddToFilter(column, iataRegion);

			return result;
		}

		CodeDescriptionPairList GetIATACityCodeDescriptionPairList()
		{
			if (iataCityCodeDescriptionPairList == null)
			{
				var ports = Factory.Load<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_IATARegionCode, SQLComparisonOperator.NotEqual, ZString.Empty));
				var list = new CodeDescriptionPairList();

				foreach (var iataRegions in ports.GroupBy(x => x.RL_IATARegionCode, y => y.RL_NameWithDiacriticals).OrderBy(x => x.Key))
				{
					list.AddPair(iataRegions.Key, string.Join(", ", iataRegions));
				}

				iataCityCodeDescriptionPairList = list;
			}

			return iataCityCodeDescriptionPairList;
		}

		CodeDescriptionPairList iataCityCodeDescriptionPairList;

		#endregion

		#region Adding From/To Location Type Filter

		void AddLocationTypeFilter(SchemaColumn column)
		{
			var (code, description) = GetLocationTypeFilterCodeDescription(column);
			var locationTypeFilter = filters.AddTextFilter(code, locationType => GetLocationQuery(locationType, column), LocationTypeList);
			locationTypeFilter.MultilingualDescription = description;
			locationTypeFilter.Category = FilterCategories.Locations;
		}

		(string code, ResourceString description) GetLocationTypeFilterCodeDescription(SchemaColumn column)
		{
			switch (column.Name)
			{
				case RateEntry.Schema.TI_OriginLRC:
					return (RateEntryFilterUtility.Constants.Codes.FromLocationType, RateEntryFilterUtility.Constants.Description.FromLocationType);
				case RateEntry.Schema.TI_DestinationLRC:
					return (RateEntryFilterUtility.Constants.Codes.ToLocationType, RateEntryFilterUtility.Constants.Description.ToLocationType);
				default:
					throw new NotSupportedException();
			}
		}

		ZQuery GetLocationQuery(ZString locationType, SchemaColumn column)
		{
			var length = GetLengthFromLocationType(locationType);

			if (length != 0)
			{
				string sqlExpression = Invariant($"LEN({column.Name}) = {length}"); // SQL Expression
				return new ZDBOnlyQuery(typeof(RateEntry)).AddFilterAndZSQLParameterCollection(sqlExpression, new ZSqlParameterCollection());
			}

			return new ZQuery();
		}

		int GetLengthFromLocationType(ZString locationType)
		{
			switch (locationType)
			{
				case Core.Constants.LocationTypes.Codes.Port:
					return 5;
				case Core.Constants.LocationTypes.Codes.Zone:
					return 4;
				case Core.Constants.LocationTypes.Codes.Country:
					return 2;
				case Core.Constants.LocationTypes.Codes.IATARegion:
					return 3;
				default:
					return 0;
			}
		}

		CodeDescriptionPairList LocationTypeList
		{
			get
			{
				if (locationTypeList == null)
				{
					locationTypeList = new CodeDescriptionPairList();
					locationTypeList.AddPair(Core.Constants.LocationTypes.Codes.Port, Core.Constants.LocationTypes.Descriptions.Port);
					locationTypeList.AddPair(Core.Constants.LocationTypes.Codes.Country, Core.Constants.LocationTypes.Descriptions.Country);
					locationTypeList.AddPair(Core.Constants.LocationTypes.Codes.Zone, Core.Constants.LocationTypes.Descriptions.Zone);

					if (ShouldIncludeTACTRatesFilter)
					{
						locationTypeList.AddPair(Core.Constants.LocationTypes.Codes.IATARegion, Core.Constants.LocationTypes.Descriptions.IATARegion);
					}
				}

				return locationTypeList;
			}
		}

		CodeDescriptionPairList locationTypeList;

		#endregion

		void AddIncludeGlobalRatesFilter()
		{
			if (filters != null && ratingHeader.SupportsPublishedFilter())
			{
				includeGlobalRatesFilter = filters.AddFlagsFilter(RateEntryFilterUtility.Constants.Codes.ShowPublished, new string[] { RateEntryFilterUtility.Constants.Description.ShowPublished }, new GetFlagsQuery[] { GetShowPublishedQuery });
				includeGlobalRatesFilter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.ShowPublished;
				includeGlobalRatesFilter.Category = FilterCategories.StatusAndFlags;
			}
		}

		ModuleFlagsFilter includeGlobalRatesFilter;

		ZQuery GetShowPublishedQuery(ZBool showPublished)
		{
			var filter = new ZQuery();
			if (showPublished)
			{
				return filter;
			}

			var globalRatingHeader = ratingHeader.GlobalRatingHeader;
			if (globalRatingHeader != null)
			{
				filter.AddToFilter(RateEntrySchema.TI_TH, SQLComparisonOperator.NotEqual, globalRatingHeader.PK);
			}

			return filter;
		}

		void AddPublisherFilter()
		{
			var isPublisherFilterApplicable = filters != null && ratingHeader.IsGlobal() && !ratingHeader.IsIntercompanyTariff();
			if (isPublisherFilterApplicable)
			{
				var publisherFilter = filters.AddGuidFilter(RateEntryFilterUtility.Constants.Codes.Publisher, ModuleIDs.GlbCompany, GetPublisherQuery, Companies);
				publisherFilter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.Publisher;
				publisherFilter.Category = FilterCategories.ModesAndTypes;
			}
		}

		static ZQuery GetPublisherQuery(ZGuid company)
		{
			return new ZQuery(RateEntrySchema.TI_GC_Publisher, company);
		}

		GlbCompanyCollection Companies
		{
			get { return Factory.GetCachedValue("GlbCompanyCollection", () => new GlbCompanyCollection(Factory)); }
		}

		void AddCommonRateEntryFilters()
		{
			var includeAll = !ratingHeader.IsIntercompanyTariff();
			foreach (var filter in new RateEntryFilterProvider(Factory).GetRateEntryFilters(ratingHeader.RateTypeSafe(), Collection.CategoryForFiltering, includeAll))
			{
				filters.AddFilter(filter);
			}
		}

		public override ZQuery Filter
		{
			get
			{
				var result = base.Filter;

				if (filters != null)
				{
					if (!ratingHeader.IsQuote() && filters.Where(x => x.Category == FilterCategories.Dates).All(IsInactiveOrEmpty))
					{
						result.AddToFilter(GetShowExpiredQuery(false));
					}

					if (ratingHeader.GlobalRatingHeader != null && filters.Where(IsShowGlobalRates).All(IsInactiveOrEmpty))
					{
						result.AddToFilter(GetShowPublishedQuery(false));
					}

					if (ShouldIncludeTACTRatesFilter && filters.Where(IsTact).All(IsInactiveOrEmpty))
					{
						result.AddToFilter(GetIncludeTACTRatesQuery(IncludeTACTRatesConstants.Code.STD));
					}
				}
				return result;
			}
		}

		ZQuery GetShowExpiredQuery(ZBool value)
		{
			var result = new ZQuery();

			if (!value)
			{
				result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RateEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
				result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RateEndDate, null);
			}

			return result;
		}

		bool IsTact(ModuleFilter filter)
		{
			var result = filter.Category == FilterCategories.ModesAndTypes
				&& filter.Description == RateEntryFilterUtility.Constants.Codes.IncludeTACTRates;

			return result;
		}

		static bool IsShowGlobalRates(ModuleFilter filter)
		{
			var result = filter.Category == FilterCategories.StatusAndFlags
				&& filter.Description == RateEntryFilterUtility.Constants.Codes.ShowPublished;

			return result;
		}

		static bool IsInactiveOrEmpty(ModuleFilter filter)
		{
			return !filter.IsActive || filter.IsEmpty;
		}

		public void ClearRateEntryFilterStrips()
		{
			FilterStrips.ClearValues();
			if (showExpiredFilter != null)
			{
				showExpiredFilter.IsActive = true;
				showExpiredFilter.Property0 = true;
			}

			if (includeGlobalRatesFilter != null)
			{
				includeGlobalRatesFilter.IsActive = true;
				includeGlobalRatesFilter.Property0 = true;
			}
		}

		protected override bool ShouldAddUserDefinedFiltersCore => false;

		#region Rate Line filters

		readonly HashSet<string> lineFilterCodes = new HashSet<string>();

		void AddRateLineFilters()
		{
			var rateLineModuleFilters = new RateLineModuleFilters(Factory, ratingHeader.RateTypeSafe(), Collection?.CategoryForFiltering, ratingHeader.IsGlobal());
			var lineFilters = rateLineModuleFilters.AddForRateEntryFilter(filters);
			lineFilterCodes.Clear();
			foreach (var filter in lineFilters)
			{
				lineFilterCodes.Add(filter.Code);
			}
		}

		void AddShowAllRateLinesFilter()
		{
			var filter = filters.AddFlagsFilter(RateEntryFilterUtility.Constants.Codes.ShowAllRateLines, new string[] { RateEntryFilterUtility.Constants.Description.ShowAllRateLines }, new GetFlagsQuery[] { (ZBool flag) => new ZQuery() });
			filter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.ShowAllRateLines;
			filter.Category = FilterCategories.StatusAndFlags;
			filter.IsSingleInstanceOnly = true;
		}

		/// <summary>
		/// Returns the query for filtering the rate lines of each rate entry.
		/// </summary>
		public ZQuery RateLineFilter
		{
			get
			{
				ZQuery result;
				var activeLineFilters = ActiveModuleFiltersForQuery.Where(x => lineFilterCodes.Contains(x.Code)).ToList();

				// Remove the filter SubGroup temporarily
				var savedSubGroup = new List<BlueprintModuleFilterSubGroup>();
				try
				{
					foreach (var filter in activeLineFilters)
					{
						savedSubGroup.Add(filter.SubGroup);
						filter.SubGroup = null;
					}
					filters.InvalidateCachedQuery();
					var showAllRateLinesFilter = (ModuleFlagsFilter)filters.SingleOrDefault(x => x.Code == RateEntryFilterUtility.Constants.Codes.ShowAllRateLines);
					if (showAllRateLinesFilter != null && showAllRateLinesFilter.Property0 && showAllRateLinesFilter.IsActive)
					{
						result = filters.GetFilterQuery(new List<ModuleFilter>());
					}
					else
					{
						result = filters.GetFilterQuery(activeLineFilters, ApplyToFilterGroups);
					}
				}
				finally
				{
					filters.InvalidateCachedQuery();
					for (var i = 0; i < savedSubGroup.Count; i++)
					{
						if (savedSubGroup[i] != null)
						{
							activeLineFilters[i].SubGroup = savedSubGroup[i];
						}
					}
				}
				return result;
			}
		}

		#endregion
	}
}

