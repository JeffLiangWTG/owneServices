using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefCommodityCodeFilterBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter codes")]
		public static class FilterCodes
		{
			public readonly static string CommodityType = "Commodity Type";
			public readonly static string IATACommodityCode = "IATA Commodity Code";
			public readonly static string RatingCodeDescription = "Rating Code Description";
			public readonly static string RatingCode = "Rating Code";
			public readonly static string RatingLocalCode = "Rating Local Code";
			public readonly static string LocalCodeUsage = "Local Code Usage";
			public readonly static string LocalCode = "Local Code";
			public readonly static string LocalCodeCountry = "Local Code Country";
			public readonly static string IsHazardous = "IsHazardous";
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddFlagsFilters(result);
			AddTextFilters(result);
			AddNKFilters(result);
			AddRatingCodeFilters(result);
			AddLocalCodeFilters(result);
			AddIsHazardousFilter(result);

			return result;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			if (Res.CurrentLanguage == Res.DefaultLanguage)
			{
				var filter = filters.AddTextFilter("Code OR Description", GetCodeOrDescriptionQuery);
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCommodityCodeFilter|CodeORDescription", "Code OR Description");
				filter.MaxLength = RefCommodityCodeSchema.RH_Description.MaxLength;
			}
			else
			{
				filters.AddTextFilter("Code", RefCommodityCodeSchema.RH_Code).MultilingualDescription = ResString.GetMultilingualString("cb5e623d-e477-4fe6-8dc7-f58bbad6cd00", "Code");
				filters.AddFiltersForTranslatableText("Description", RefCommodityCodeSchema.RH_Description, typeof(RefCommodityCode), ResString.GetMultilingualString("MasterFiles|RefCommodityCodeFilter|Description", "Description"));
			}
		}

		ZQuery GetCodeOrDescriptionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();

			query.AddToFilter(RefCommodityCodeSchema.RH_Code, comparisonOperator, value.SubstringSafe(0, RefCommodityCodeSchema.RH_Code.MaxLength));
			query.AddToFilter(JoinCondition.Or, RefCommodityCodeSchema.RH_Description, comparisonOperator, value.SubstringSafe(0, RefCommodityCodeSchema.RH_Description.MaxLength));

			return query;
		}

		#endregion

		#region Flags

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			var commodityTypeList = new List<string>()
			{
				Res.GetString("27A6FECC-0FDF-4DCF-8B02-9888621C8F80", "Is Forwarding"),
				Res.GetString("61FCF23A-D271-4518-B5E7-73CCAA07A1CE", "Is Shipping"),
				Res.GetString("3EC60E0E-83A8-4F97-B8B7-DF6511D2306E", "Is Land Transport"),
			};

			var commodityTypeColumnList = new List<SchemaBoolColumn>()
			{
				RefCommodityCodeSchema.RH_IsForwarding,
				RefCommodityCodeSchema.RH_IsShipping,
				RefCommodityCodeSchema.RH_IsLandTransport,
			};

			if (ReferenceFilesDataRegistry.Instance.ShowPersonalEffects.Value)
			{
				commodityTypeList.Add(Res.GetString("57813AB8-2E2C-4022-BB08-89BC8DFFD25D", "Is Personal Effects"));
				commodityTypeColumnList.Add(RefCommodityCodeSchema.RH_IsPersonalEffects);
			}

			var commodityTypeFilter = filters.AddFlagsFilter(FilterCodes.CommodityType, commodityTypeList.ToArray(), commodityTypeColumnList.ToArray());
			commodityTypeFilter.ShowAddOrRadioBox = true;
			commodityTypeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCommodityCodeFilter|CommodityType", "Commodity Type");
		}

		#endregion

		#region IATA Commodity Code

		void AddNKFilters(ModuleFilterCollection filters)
		{
			var commodityCodeFilter = filters.AddNkFilter(FilterCodes.IATACommodityCode, GetCommodityCodeQuery, ModuleIDs.RefAirlineCommodityCode, new IATACommodityCodeCollection(Factory));
			commodityCodeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCommodityCodeFilter|IATACommodityCode", "IATA Commodity Code");
			commodityCodeFilter.Category = FilterCategories.Other;
		}

		ZQuery GetCommodityCodeQuery(ZDBOnlySubQuery filtersMatchQuery, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var commodityCodeQuery = new ZDBOnlyQuery(typeof(RefCommodityCode));
			if (filtersMatchQuery != null)
			{
				commodityCodeQuery.AddSubQuery(RefCommodityCodeSchema.RH_IATACommodityItem, RefAirlineCommodityCodeSchema.RAC_Code, filtersMatchQuery, JoinCondition.And);
			}
			else
			{
				commodityCodeQuery.AddToFilter(RefCommodityCodeSchema.RH_IATACommodityItem, comparisonOperator, value);
			}
			return commodityCodeQuery;
		}

		#endregion

		#region Rating Codes

		void AddRatingCodeFilters(ModuleFilterCollection filters)
		{
			var ratingCodeCategory = new FilterCategory(ResString.GetMultilingualString("2082dc46-df82-40a0-9d3c-b5388b68a8f5", "Rating Code"));

			var ratingCodeFilter = filters.AddNkFilter(FilterCodes.RatingCode, GetRatingCodeQuery, ModuleIDs.RefCommodityCode, new RefCommodityCodeCollection(Factory));
			ratingCodeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCommodityCodeFilter|RatingCode", "Rating Code");
			ratingCodeFilter.Category = ratingCodeCategory;

			var ratingCodeDescriptionFilter = filters.AddTextFilter(FilterCodes.RatingCodeDescription, GetRatingCodeDescriptionQuery);
			ratingCodeDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCommodityCodeFilter|RatingCodeDescription", "Rating Code Description");
			ratingCodeDescriptionFilter.Category = ratingCodeCategory;

			var ratingLocalCodeDescriptionFilter = filters.AddTextFilter(FilterCodes.RatingLocalCode, GetRatingLocalCodeQuery);
			ratingLocalCodeDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCommodityCodeFilter|RatingLocalCode", "Rating Local Code");
			ratingLocalCodeDescriptionFilter.Category = ratingCodeCategory;
			ratingLocalCodeDescriptionFilter.MaxLength = RefCommodityCodeMapSchema.LC_LocalCode.MaxLength;
		}

		ZQuery GetRatingCodeQuery(ZDBOnlySubQuery filtersMatchQuery, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var commodityCodeQuery = new ZDBOnlyQuery(typeof(RefCommodityCode));
			var ratingCodeSubQuery = new ZDBOnlySubQuery(typeof(RefCommodityRatingCodeMap), RefCommodityRatingCodeMapSchema.PK);

			if (filtersMatchQuery != null)
			{
				ratingCodeSubQuery.AddSubQuery(RefCommodityRatingCodeMapSchema.RI_RH_NKCommodityChild, RefCommodityCodeSchema.RH_Code, filtersMatchQuery, JoinCondition.And);
			}
			else
			{
				ratingCodeSubQuery.AddToFilter(RefCommodityRatingCodeMapSchema.RI_RH_NKCommodityChild, comparisonOperator, value);
			}

			commodityCodeQuery.AddSubQuery(RefCommodityCodeSchema.RH_Code, RefCommodityRatingCodeMapSchema.RI_RH_NKCommodityParent, ratingCodeSubQuery, JoinCondition.And);

			return commodityCodeQuery;
		}

		ZQuery GetRatingCodeDescriptionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var commodityCodeQuery = new ZDBOnlyQuery(typeof(RefCommodityCode));
			var ratingCodeSubQuery = new ZDBOnlySubQuery(typeof(RefCommodityRatingCodeMap), RefCommodityRatingCodeMapSchema.PK);
			var childCommodityCodeQuery = new ZDBOnlySubQuery(typeof(RefCommodityCode), RefCommodityCodeSchema.PK);

			childCommodityCodeQuery.AddToFilter(RefCommodityCodeSchema.RH_Description, comparisonOperator, value);
			ratingCodeSubQuery.AddSubQuery(RefCommodityRatingCodeMapSchema.RI_RH_NKCommodityChild, RefCommodityCodeSchema.RH_Code, childCommodityCodeQuery, JoinCondition.And);
			commodityCodeQuery.AddSubQuery(RefCommodityCodeSchema.RH_Code, RefCommodityRatingCodeMapSchema.RI_RH_NKCommodityParent, ratingCodeSubQuery, JoinCondition.And);

			return commodityCodeQuery;
		}

		ZQuery GetRatingLocalCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var commodityCodeQuery = new ZDBOnlyQuery(typeof(RefCommodityCode));
			var ratingCodeSubQuery = new ZDBOnlySubQuery(typeof(RefCommodityRatingCodeMap), RefCommodityRatingCodeMapSchema.PK);
			var childCommodityCodeQuery = new ZDBOnlySubQuery(typeof(RefCommodityCode), RefCommodityCodeSchema.PK);
			var childLocalCodeQuery = new ZDBOnlySubQuery(typeof(RefCommodityCodeMap), RefCommodityCodeMapSchema.PK);

			childLocalCodeQuery.AddToFilter(RefCommodityCodeMapSchema.LC_LocalCodeProvider, SQLComparisonOperator.Equal, GlobalCommodityCodeProviderList.Codes.Rating);
			childLocalCodeQuery.AddToFilter(RefCommodityCodeMapSchema.LC_LocalCode, comparisonOperator, value);
			childCommodityCodeQuery.AddSubQuery(RefCommodityCodeSchema.RH_Code, RefCommodityCodeMapSchema.LC_RH_NKCommodityCode, childLocalCodeQuery, JoinCondition.And);
			ratingCodeSubQuery.AddSubQuery(RefCommodityRatingCodeMapSchema.RI_RH_NKCommodityChild, RefCommodityCodeSchema.RH_Code, childCommodityCodeQuery, JoinCondition.And);
			commodityCodeQuery.AddSubQuery(RefCommodityCodeSchema.RH_Code, RefCommodityRatingCodeMapSchema.RI_RH_NKCommodityParent, ratingCodeSubQuery, JoinCondition.And);

			return commodityCodeQuery;
		}

		#endregion

		#region Local Codes

		void AddLocalCodeFilters(ModuleFilterCollection filters)
		{
			var localCodeCategory = new FilterCategory(ResString.GetMultilingualString("30dccbed-354b-4a4f-a6c4-9bd51ba8c11c", "Local Code"));

			var localCodeFilter = filters.AddTextFilter(FilterCodes.LocalCode, GetLocalCodeQuery);
			localCodeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCommodityCodeFilter|LocalCode", "Local Code");
			localCodeFilter.Category = localCodeCategory;
			localCodeFilter.MaxLength = RefCommodityCodeMapSchema.LC_LocalCode.MaxLength;

			var localCodeCountryFilter = filters.AddTextFilter(FilterCodes.LocalCodeCountry, GetLocalCodeCountryQuery, new RefCountryCollection(Factory));
			localCodeCountryFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCommodityCodeFilter|LocalCodeCountry", "Local Code Country");
			localCodeCountryFilter.Category = localCodeCategory;
			localCodeCountryFilter.MaxLength = RefCommodityCodeMapSchema.LC_RN_NKCountry.MaxLength;

			var localCodeUsageFilter = filters.AddTextFilter(FilterCodes.LocalCodeUsage, GetLocalCodeUsageQuery, new RefCommodityCodeMapLookups(null).AllProviders);
			localCodeUsageFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCommodityCodeFilter|LocalCodeUsage", "Local Code Usage");
			localCodeUsageFilter.Category = localCodeCategory;
			localCodeUsageFilter.MaxLength = RefCommodityCodeMapSchema.LC_LocalCodeProvider.MaxLength;
		}

		ZQuery GetLocalCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
			=> GetLocalCodeColumnQuery(RefCommodityCodeMapSchema.LC_LocalCode, comparisonOperator, value);

		ZQuery GetLocalCodeUsageQuery(ZString value)
			=> GetLocalCodeColumnQuery(RefCommodityCodeMapSchema.LC_LocalCodeProvider, SQLComparisonOperator.Equal, value);

		ZQuery GetLocalCodeCountryQuery(ZString value)
			=> GetLocalCodeColumnQuery(RefCommodityCodeMapSchema.LC_RN_NKCountry, SQLComparisonOperator.Equal, value);

		ZQuery GetLocalCodeColumnQuery(SchemaColumn column, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var localCodeSubQuery = new ZDBOnlySubQuery(typeof(RefCommodityCodeMap), RefCommodityCodeMapSchema.PK);
			localCodeSubQuery.AddToFilter(column, comparisonOperator, value);

			var commodityCodeQuery = new ZDBOnlyQuery(typeof(RefCommodityCode));
			commodityCodeQuery.AddSubQuery(RefCommodityCodeSchema.RH_Code, RefCommodityCodeMapSchema.LC_RH_NKCommodityCode, localCodeSubQuery, JoinCondition.And);
			return commodityCodeQuery;
		}

		#endregion

		#region IsHazardous

		void AddIsHazardousFilter(ModuleFilterCollection filters)
		{
			var isHazardousFilter = filters.AddTextFilter(FilterCodes.IsHazardous, GetIsHazardousQuery, IsHazardousOptionsList);  
			isHazardousFilter.Category = FilterCategories.StatusAndFlags;
			isHazardousFilter.DefaultProperty = IsHazardousConstants.Code.STD;
			isHazardousFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCommodityCodeFilter|IsHazardous", "Is Hazardous");
		}

		ZQuery GetIsHazardousQuery(ZString includeHAZCommodities)
		{
			var result = new ZQuery();

			if (includeHAZCommodities == IsHazardousConstants.Code.STD)
			{
				result.AddToFilter(RefCommodityCodeSchema.RH_IsHazardous, false);
			}
			else if (includeHAZCommodities == IsHazardousConstants.Code.HAZ)
			{
				result.AddToFilter(RefCommodityCodeSchema.RH_IsHazardous, true);
			}

			return result; // ALL options means it does not matter whether hazardous or not
		}

		CodeDescriptionPairList IsHazardousOptionsList
		{
			get
			{
				var list = new CodeDescriptionPairList();

				list.AddPair(IsHazardousConstants.Code.STD, IsHazardousConstants.Description.STD);
				list.AddPair(IsHazardousConstants.Code.HAZ, IsHazardousConstants.Description.HAZ);
				list.AddPair(IsHazardousConstants.Code.ALL, IsHazardousConstants.Description.ALL);

				return list;
			}
		}

		public static class IsHazardousConstants
		{
			public static class Code
			{
				public const string STD = nameof(STD);
				public const string HAZ = nameof(HAZ);
				public const string ALL = nameof(ALL);
			}

			public static class Description
			{
				public static readonly ResourceString STD = ResString.GetMultilingualString("3316BFC5-9969-477E-BEAA-DFCECB44262E", "Show Non-Hazardous Commodities");
				public static readonly ResourceString HAZ = ResString.GetMultilingualString("43C52113-C1C7-44FA-B820-A82A1103B241", "Show Hazardous Commodities");
				public static readonly ResourceString ALL = ResString.GetMultilingualString("C3E2DD9C-4ACA-417E-A8F1-75E3224A705D", "Show All Commodities");
			}
		}

		#endregion

		#endregion
	}
}
