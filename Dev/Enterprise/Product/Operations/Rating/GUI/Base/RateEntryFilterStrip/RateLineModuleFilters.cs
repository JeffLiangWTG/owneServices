using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.GUI
{
	public class RateLineModuleFilters
	{
		#region SuppressResourceStringsCheckRegion

		public static class Constants
		{
			public static class Codes
			{
				public const string ActualPercentage = "Actual Percentage";
				public const string UseOnlyActualWeightMeasure = "Actual Weight/Volume";
				public const string Condition = "Condition";
				public const string ContainerOwnership = "Container Ownership";
				public const string ConversionFactor = "Conversion Factor";
				public const string Currency = "Currency – Rate Line";
				public const string ChargeCode = "Charge Code";
				public const string FeeChargeType = "Fees and Charge Type";
				public const string FeeChargeLevel = "Fees and Charge Level";
				public const string Rounding = "Rounding";
				public const string HasOverrideChargeDescription = "Override Description";
				public const string IsJobLevelCharge = "Is Job Level Charge";
				public const string UnitFactor = "Unit Factor";
				public const string UnitMultiple = "Unit Multiple";
				public const string Units = "Units";
				public const string StartDate = "Start Date - Rate Line";
				public const string EndDate = "Expiry Date - Rate Line";
				public const string EffectiveOn = "Effective On - Rate Line";
				public const string ShowExpired = "Show Expired - Rate Line";
			}

			public static class Description
			{
				public static readonly ResourceString ActualPercentage = ResString.GetMultilingualString("A95A332D-C3FC-4660-A005-8420109A7B49", "Actual Percentage");
				public static readonly ResourceString UseOnlyActualWeightMeasure = ResString.GetMultilingualString("DD72A269-013B-4B5C-81CD-104D82A840B3", "Actual Weight/Volume");
				public static readonly ResourceString Condition = ResString.GetMultilingualString("6a74f440-b792-446f-97c4-dea4888294cb", "Condition");
				public static readonly ResourceString ContainerOwnership = ResString.GetMultilingualString("1dc5b92d-7ace-492c-bbcf-d5260adf8281", "Container Ownership");
				public static readonly ResourceString ConversionFactor = ResString.GetMultilingualString("385c743b-f6bf-4558-a8c1-532075f80f54", "Conversion Factor");
				public static readonly ResourceString Currency = ResString.GetMultilingualString("8e39a6f8-9eee-4b92-ac1d-693040b02e23", "Currency – Rate Line");
				public static readonly ResourceString ChargeCode = ResString.GetMultilingualString("10d47bd9-83d1-460f-8f09-17a04cd498e3", "Charge Code");
				public static readonly ResourceString FeeChargeType = ResString.GetMultilingualString("a053fd63-7603-401a-9589-5887d78b9e52", "Fees and Charge Type");
				public static readonly ResourceString FeeChargeLevel = ResString.GetMultilingualString("c05b478a-5353-4540-a9d7-b4e6de2cbea7", "Fees and Charge Level");
				public static readonly ResourceString Rounding = ResString.GetMultilingualString("133d1ea5-7041-45b8-82fc-ba06e2f4bbaa", "Rounding");
				public static readonly ResourceString HasOverrideChargeDescription = ResString.GetMultilingualString("7636ac0e-a591-4155-b948-54f0d56c478e", "Override Description");
				public static readonly ResourceString IsJobLevelCharge = ResString.GetMultilingualString("f8ef4068-72c4-4c81-a837-af4e1c01d010", "Is Job Level Charge");
				public static readonly ResourceString UnitFactor = ResString.GetMultilingualString("a9607755-eb57-4357-a6b3-498354a73306", "Unit Factor");
				public static readonly ResourceString UnitMultiple = ResString.GetMultilingualString("9bb88143-a3da-45d0-9c8b-81cd95718e7b", "Unit Multiple");
				public static readonly ResourceString Units = ResString.GetMultilingualString("4a08910a-c45d-4ae1-b990-717e2fd496c4", "Units");
				public static readonly ResourceString StartDate = ResString.GetMultilingualString("97864fb1-f463-40e8-9fc8-1ff844b07309", "Start Date - Rate Line");
				public static readonly ResourceString EndDate = ResString.GetMultilingualString("12291355-efcc-494a-a566-c365a6a8460c", "Expiry Date - Rate Line");
				public static readonly ResourceString EffectiveOn = ResString.GetMultilingualString("1c821610-f16f-482a-831b-e6d28ae9fc85", "Effective On - Rate Line");
				public static readonly ResourceString ShowExpired = ResString.GetMultilingualString("9d3a8da6-334b-48c5-85ce-63ae5549584b", "Show Expired - Rate Line");
			}

			public static class Flag
			{
				public static readonly ResourceString UseOnlyActualWeightMeasure = ResString.GetMultilingualString("bb7fb400-a9cf-4111-9ce2-81ed251b0bac", "Act. W/V");
				public static readonly ResourceString ShowExpired = ResString.GetMultilingualString("50751708-aa1b-4083-8960-772e2b108c68", "Show Expired");
			}
		}

		#endregion

		/// <summary>
		/// Construct filters for the given rating header and category.
		/// </summary>
		/// <param name="factory">factory to use for loading lookup lists like currencies</param>
		/// <param name="ratingHeaderType">must be a value from <see cref="RatingConstants.RatingHeaderTypes"/>.</param>
		/// <param name="rateCategory">a value from <see cref="RatingConstants.RateCategory"/> or null/empty to allow filters and lists for all categories.</param>
		/// <param name="isGlobal">true for a global rate. Used to select global charge codes.</param>
		public RateLineModuleFilters(BusinessObjectFactory factory, string ratingHeaderType, string rateCategory, bool isGlobal)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNullOrEmpty(ratingHeaderType, nameof(ratingHeaderType));

			Factory = factory;
			RatingHeaderType = ratingHeaderType;
			RateCategory = rateCategory ?? string.Empty;
			IsGlobal = isGlobal;
			if (!string.IsNullOrEmpty(rateCategory))
			{
				RateTypeFromCategory = RatingConstants.RateCategory.GetRateType(RateCategory);
			}
			else
			{
				RateTypeFromCategory = 0;
				foreach (RateType option in Enum.GetValues(typeof(RateType)))
				{
					RateTypeFromCategory |= option;
				}
			}
		}

		public BusinessObjectFactory Factory { get; }
		public string RatingHeaderType { get; }
		public string RateCategory { get; }
		public bool IsGlobal { get; }
		public RateType RateTypeFromCategory { get; }

		public List<ModuleFilter> AddForRateEntryFilter(ModuleFilterCollection filters)
		{
			var addedFilters = CommonAdd(filters);

			var subGroup = new RateLineSubgroupForRateEntryFilters();
			foreach (var filter in addedFilters)
			{
				filter.SubGroup = subGroup;
			}

			return addedFilters;
		}

		public void AddForRatingHeaderFilter(ModuleFilterCollection filters)
		{
			var addedFilters = CommonAdd(filters);

			var subGroup = new RateLineSubgroupForRatingHeaderFilters();
			foreach (var filter in addedFilters)
			{
				filter.SubGroup = subGroup;
			}
		}

		internal List<ModuleFilter> CommonAdd(ModuleFilterCollection filters)
		{
			var newFilters = new List<ModuleFilter>()
			{
				AddActualPercentageFilter(filters),
				AddUseOnlyActualWeightMeasureFilter(filters),
				AddConditionFilter(filters),
				AddContainerOwnershipFilter(filters),
				AddConversionFactorFilter(filters),
				AddCurrencyFilter(filters),
				AddChargeCodeFilter(filters),
				AddFeeAndChargeTypeFilter(filters),
				AddFeeAndChargeLevelFilter(filters),
				AddHasOverrideChargeDescriptionFilter(filters),
				AddRoundingFilter(filters),
				AddUnitFactorFilter(filters),
				AddUnitMultipleFilter(filters),
				AddUnitsFilter(filters),
				AddIsJobLevelChargeFilter(filters),
				AddEffectiveOnFilter(filters),
				AddExpiryDateFilter(filters),
				AddStartDateFilter(filters),
				AddShowExpiredFilter(filters),
			};
			return newFilters;
		}

		ModuleFlagsFilter AddShowExpiredFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddFlagsFilter(Constants.Codes.ShowExpired, new string[] { Constants.Flag.ShowExpired }, new GetFlagsQuery[] { GetShowExpiredQuery });
			filter.MultilingualDescription = Constants.Description.ShowExpired;
			filter.Category = FilterCategories.Dates;
			return filter;
		}

		static ZQuery GetShowExpiredQuery(ZBool value)
		{
			var result = new ZQuery();
			if (!value)
			{
				result.AddToFilter(JoinCondition.Or, RateLinesSchema.TL_RateEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
				result.AddToFilter(JoinCondition.Or, RateLinesSchema.TL_RateEndDate, null);
			}
			return result;
		}

		ModuleDateFilter AddStartDateFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddDateFilter(Constants.Codes.StartDate, RateLinesSchema.TL_RateStartDate);
			filter.MultilingualDescription = Constants.Description.StartDate;
			filter.Category = FilterCategories.Dates;
			return filter;
		}

		ModuleDateFilter AddExpiryDateFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddDateFilter(Constants.Codes.EndDate, RateLinesSchema.TL_RateEndDate);
			filter.MultilingualDescription = Constants.Description.EndDate;
			filter.Category = FilterCategories.Dates;
			return filter;
		}

		ModuleSingleDateFilter AddEffectiveOnFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddSingleDateFilter(Constants.Codes.EffectiveOn, GetEffectiveOnQuery);
			filter.MultilingualDescription = Constants.Description.EffectiveOn;
			filter.Category = FilterCategories.Dates;
			return filter;
		}

		static ZQuery GetEffectiveOnQuery(ZDateTime value)
		{
			var startDateQuery = new ZQuery(RateLinesSchema.TL_RateStartDate, null);
			startDateQuery.AddToFilter(JoinCondition.Or, RateLinesSchema.TL_RateStartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, value);

			var endDateQuery = new ZQuery(RateLinesSchema.TL_RateEndDate, null);
			endDateQuery.AddToFilter(JoinCondition.Or, RateLinesSchema.TL_RateEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, value);

			return new ZQuery(startDateQuery, JoinCondition.And, endDateQuery);
		}

		ModuleFlagsFilter AddIsJobLevelChargeFilter(ModuleFilterCollection filters)
		{
			var desc = Constants.Description.IsJobLevelCharge;
			var filter = filters.AddFlagFilter(Constants.Codes.IsJobLevelCharge, desc, RateLinesSchema.TL_IsWhsJobLevelCharge, null);
			filter.MultilingualDescription = desc;
			filter.Category = FilterCategories.TextSearch;
			return filter;
		}

		ModuleTextFilter AddUnitsFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(Constants.Codes.Units, RateLinesSchema.TL_WeightVolume, GetUnitList);
			filter.MultilingualDescription = Constants.Description.Units;
			filter.Category = FilterCategories.TextSearch;
			return filter;
		}

		IList GetUnitList()
		{
			var countryCode = GlbCompany.CurrentCompany?.Country.Code;
			return Factory.GetCachedValue(nameof(RateLineModuleFilters) + nameof(GetUnitList) + "." + countryCode + "." + RateCategory, () =>
			{
				return UnitHelper.GetUnitList(Factory, RateTypeFromCategory, countryCode, Core.Constants.PluralState.Plural);
			});
		}

		ModuleFilter AddUnitMultipleFilter(ModuleFilterCollection filters)
		{
			var filter = new ModuleNumberRangeFilter(Constants.Codes.UnitMultiple, RateLinesSchema.TL_WeightVolumeMultiple);
			filter.Decimals = 0;
			filter.MinValue = 0;
			filter.DefaultPropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo.GetUnresolvedString();
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo.GetUnresolvedString();
			filter.MultilingualDescription = Constants.Description.UnitMultiple;
			filter.Category = FilterCategories.NumbersAndReferences;
			filters.AddFilter(filter);
			return filter;
		}

		ModuleTextFilter AddUnitFactorFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(Constants.Codes.UnitFactor, RateLinesSchema.TL_UnitFactor, GetUnitFactorList);
			filter.MultilingualDescription = Constants.Description.UnitFactor;
			filter.Category = FilterCategories.TextSearch;
			return filter;
		}

		IList GetUnitFactorList()
			=> RateLinesLookups.BuildUnitFactors(RatingHeaderType, RateTypeFromCategory);

		ModuleTextFilter AddRoundingFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(Constants.Codes.Rounding, RateLinesSchema.TL_Rounding, GetRoundingList);
			filter.MultilingualDescription = Constants.Description.Rounding;
			filter.Category = FilterCategories.TextSearch;
			return filter;
		}

		IList GetRoundingList()
			=> RateLinesLookups.GetCachedRoundings(Factory, RateCategory);

		ModuleFlagsFilter AddHasOverrideChargeDescriptionFilter(ModuleFilterCollection filters)
		{
			var desc = Constants.Description.HasOverrideChargeDescription;
			var filter = filters.AddFlagsFilter(Constants.Codes.HasOverrideChargeDescription, new string[] { desc }, new GetFlagsQuery[] { GetHasOverrideChargeDescriptionQuery });
			filter.MultilingualDescription = desc;
			filter.Category = FilterCategories.TextSearch;
			return filter;
		}

		ZQuery GetHasOverrideChargeDescriptionQuery(ZBool value)
			=> new ZQuery(RateLinesSchema.TL_RateDesc, value ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal, ZString.Empty);

		ModuleTextFilter AddFeeAndChargeTypeFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(Constants.Codes.FeeChargeType, RateLinesSchema.TL_FeeChargeType, RateLinesLookups.GetFeeChargeTypeList);
			filter.MultilingualDescription = Constants.Description.FeeChargeType;
			filter.Category = FilterCategories.TextSearch;
			return filter;
		}

		ModuleTextFilter AddFeeAndChargeLevelFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(Constants.Codes.FeeChargeLevel, RateLinesSchema.TL_FeeChargeLevel, GetFeeChargeLevelList);
			filter.MultilingualDescription = Constants.Description.FeeChargeLevel;
			filter.Category = FilterCategories.TextSearch;
			return filter;
		}

		static IList GetFeeChargeLevelList()
		{
			var result = new CodeDescriptionPairList();
			var regValue = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value;
			foreach (FeeChargeType type in regValue.FeeChargeTypes)
			{
				foreach (FeeChargeLevel level in type.FeeChargeLevels)
				{
					result.AddPairIfNotExist(level.Code, level.Description);
				}
			}
			return result;
		}

		ModuleFilter AddChargeCodeFilter(ModuleFilterCollection filters)
		{
			IBusinessObjectCollection list;
			ModuleIdentifier id;
			if (IsGlobal)
			{
				list = new GlobalChargeCodesCollection(Factory, new ZQuery(), Guid.Empty);
				id = ModuleIDs.AccGlobalChargeCode;
			}
			else
			{
				list = new AccChargeCodeCollection(Factory, new ZQuery(), Env.CurrentCompanyPK);
				id = ModuleIDs.AccChargeCode;
			}

			var filter = filters.AddGuidFilter(Constants.Codes.ChargeCode, id, GetChargeCodeQuery, list);
			filter.MultilingualDescription = Constants.Description.ChargeCode;
			filter.Category = FilterCategories.TextSearch;
			var ops = filter.ComparisonOperator_List;
			// Since charge code cannot be blank...
			ops.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			ops.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			return filter;
		}

		ZQuery GetChargeCodeQuery(SQLComparisonOperator comparisonOperator, object value)
		{
			var pk = (ZGuid)value;
			if (!IsGlobal)
			{
				var localChargeCode = Factory.Load<AccChargeCode>(pk);
				if (localChargeCode != null)
				{
					var globalQuery = new ZQuery(AccChargeCodeSchema.AC_Code, localChargeCode.AC_Code)
							.AddToFilter(AccChargeCodeSchema.AC_GC, null);
					var globalChargeCode = Factory.LoadTop1<AccChargeCode>(globalQuery);
					if (globalChargeCode != null)
					{
						return new ZQuery(RateLinesSchema.TL_AC, comparisonOperator, new[] { pk, globalChargeCode.PK });
					}
				}
			}

			return new ZQuery(RateLinesSchema.TL_AC, comparisonOperator, pk);
		}

		ModuleNkFilter AddCurrencyFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddNkFilter(Constants.Codes.Currency, RateLinesSchema.TL_RX_NKCurrency, ModuleIDs.RefCurrency, new RefCurrencyCollection(Factory));
			filter.MultilingualDescription = Constants.Description.Currency;
			filter.Category = FilterCategories.Other;
			return filter;
		}

		static ModuleTextFilter AddContainerOwnershipFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(Constants.Codes.ContainerOwnership, RateLinesSchema.TL_ContainerOwnership, RateLinesLookups.BuildContainerOwnershipList);
			filter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Exact;
			filter.MultilingualDescription = Constants.Description.ContainerOwnership;
			filter.Category = FilterCategories.TextSearch;
			return filter;
		}

		ModuleFilter AddConversionFactorFilter(ModuleFilterCollection filters)
		{
			var filter = new ConversionFactorFilter();
			filter.MultilingualDescription = Constants.Description.ConversionFactor;
			filter.Category = FilterCategories.Other;
			filters.AddFilter(filter);
			return filter;
		}

		ModuleTextFilter AddConditionFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(Constants.Codes.Condition, RateLinesSchema.TL_Condition, GetConditionsList);
			filter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Exact;
			filter.MultilingualDescription = Constants.Description.Condition;
			filter.Category = FilterCategories.TextSearch;
			return filter;
		}

		CodeDescriptionPairList GetConditionsList()
			=> RateLinesLookups.BuildConditionsList(RatingHeaderType, RateTypeFromCategory);

		ModuleFlagsFilter AddUseOnlyActualWeightMeasureFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddFlagsFilter(Constants.Codes.UseOnlyActualWeightMeasure, new string[] { Constants.Flag.UseOnlyActualWeightMeasure }, new GetFlagsQuery[] { GetUseOnlyActualWeightMeasureQuery });
			filter.MultilingualDescription = Constants.Description.UseOnlyActualWeightMeasure;
			filter.Category = FilterCategories.TextSearch;
			return filter;
		}

		ZQuery GetUseOnlyActualWeightMeasureQuery(ZBool value)
			=> new ZQuery(RateLinesSchema.TL_ActualPercentage, value ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, (ZByte)100);

		static ModuleNumberRangeFilter AddActualPercentageFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddNumberRangeFilter(Constants.Codes.ActualPercentage, RateLinesSchema.TL_ActualPercentage);
			filter.MultilingualDescription = Constants.Description.ActualPercentage;
			filter.Category = FilterCategories.TextSearch;
			filter.MinValue = 0;
			filter.MaxValue = 100;
			filter.DefaultPropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo.GetUnresolvedString();
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo.GetUnresolvedString();
			return filter;
		}

		class RateLineSubgroupForRateEntryFilters : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var rateLine = new ZDBOnlySubQuery(typeof(RateLine), RateLinesSchema.TL_TI);
				rateLine.AddToFilter(filter);

				var rateEntry = new ZDBOnlyQuery(typeof(RateEntry));
				rateEntry.AddSubQuery(rateLine, JoinCondition.And);

				return rateEntry;
			}
		}

		class RateLineSubgroupForRatingHeaderFilters : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var rateLine = new ZDBOnlySubQuery(typeof(RateLine), RateLinesSchema.TL_TI);
				rateLine.AddToFilter(filter);

				var rateEntry = new ZDBOnlySubQuery(typeof(RateEntry), RateEntrySchema.TI_TH);
				rateEntry.AddSubQuery(rateLine, JoinCondition.And);

				var ratingHeader = new ZDBOnlyQuery(typeof(RatingHeader));
				ratingHeader.AddSubQuery(rateEntry, JoinCondition.And);

				return ratingHeader;
			}
		}
	}
}
