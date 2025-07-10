using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class CusPermitFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string PermitHolder = CusPermitHeaderCollection.FilterConstants.PermitHolder;
			public const string PermitNumber = CusPermitHeaderCollection.FilterConstants.PermitNumber;
			public const string StartDate = CusPermitHeaderCollection.FilterConstants.StartDate;
			public const string EndDate = CusPermitHeaderCollection.FilterConstants.EndDate;
			public const string PermitTypeSubType = CusPermitHeaderCollection.FilterConstants.PermitTypeSubType;
			public const string PermitRule = CusPermitHeaderCollection.FilterConstants.PermitRule;
			public const string Country = CusPermitHeaderCollection.FilterConstants.Country;
			public const string IsCurrent = CusPermitHeaderCollection.FilterConstants.IsCurrent;
			public const string QtyValIndicator = CusPermitHeaderCollection.FilterConstants.QtyValIndicator;
			public const string AppliesTo = CusPermitHeaderCollection.FilterConstants.AppliesTo;
		}

		#region New Properties

		IEnumerable<ModuleNkFilter> CountryFilters => ActiveModuleFilters.OfType<ModuleNkFilter>().Where(x => x.Description.StartsWith(Schema.Country, StringComparison.Ordinal));

		#endregion

		protected virtual MultilingualString PermitTypeFilterDescription => ResString.GetMultilingualString("CusPermitFilter|PermitTypeSubType", Schema.PermitTypeSubType);

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddNkFilter(Schema.Country, GetCountryQuery, ModuleIDs.RefCountry, new RefCountryCollection(Factory)).MultilingualDescription
				= ResString.GetMultilingualString("CusPermitFilter|Country", Schema.Country);

			var permitHolderFilter = result.AddGuidFilter(Schema.PermitHolder, ModuleIDs.Organisation, GetPermitHolderQuery, OrganisationList);
			permitHolderFilter.Category = FilterCategories.Organisations;
			permitHolderFilter.MultilingualDescription = ResString.GetMultilingualString("CusPermitFilter|PermitHolder", Schema.PermitHolder);

			var permitNumberFilter = result.AddTextFilter(Schema.PermitNumber, GetPermitNumberQuery);
			permitNumberFilter.Category = FilterCategories.NumbersAndReferences;
			permitNumberFilter.MaxLength = CusPermitHeaderSchema.CPH_Number.MaxLength;
			permitNumberFilter.MultilingualDescription = ResString.GetMultilingualString("CusPermitFilter|PermitNumber", Schema.PermitNumber);

			var startDateFilter = result.AddDateFilter(Schema.StartDate, GetStartDateQuery);
			startDateFilter.Category = FilterCategories.Dates;
			startDateFilter.MultilingualDescription = ResString.GetMultilingualString("CusPermitFilter|StartDate", Schema.StartDate);

			var endDateFilter = result.AddDateFilter(Schema.EndDate, GetEndDateQuery);
			endDateFilter.Category = FilterCategories.Dates;
			endDateFilter.MultilingualDescription = ResString.GetMultilingualString("CusPermitFilter|EndDate", Schema.EndDate);

			var permitTypeSubTypeFilter = GetPermitTypeModuleFilter(Schema.PermitTypeSubType, GetPermitTypeQuery, GetCountryList);
			permitTypeSubTypeFilter.DefaultProperty0 = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			permitTypeSubTypeFilter.MultilingualDescription = PermitTypeFilterDescription;
			result.AddFilter(permitTypeSubTypeFilter);

			var permitRuleFilter = new PermitRuleModuleFilter(Schema.PermitRule, GetPermitRuleQuery, GetCountryList);
			permitRuleFilter.DefaultProperty0 = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			permitRuleFilter.MultilingualDescription = ResString.GetMultilingualString("CusPermitFilter|PermitRule", Schema.PermitRule);
			result.AddFilter(permitRuleFilter);

			var isCurrentFilter = result.AddFlagsFilter(Schema.IsCurrent, new string[] { Schema.IsCurrent }, new GetFlagsQuery[] { PermitFindBoxCollection.GetIsCurrentQuery });
			isCurrentFilter.Category = FilterCategories.StatusAndFlags;
			isCurrentFilter.MultilingualDescription = ResString.GetMultilingualString("CusPermitFilter|IsCurrent", Schema.IsCurrent);

			var qtyValIndicatorFilter = result.AddTextFilter(Schema.QtyValIndicator, PermitFindBoxCollection.GetQtyValIndicatorQuery, new PermitQtyValIndicatorList());
			qtyValIndicatorFilter.Category = FilterCategories.ModesAndTypes;
			qtyValIndicatorFilter.MultilingualDescription = ResString.GetMultilingualString("CusPermitFilter|QtyValIndicator", Schema.QtyValIndicator);

			var appliesToFilter = result.AddGuidFilter(Schema.AppliesTo, ModuleIDs.Organisation, GetAppliesToQuery, OrganisationList);
			appliesToFilter.Category = FilterCategories.Organisations;
			appliesToFilter.MultilingualDescription = ResString.GetMultilingualString("CusPermitFilter|AppliesTo", Schema.AppliesTo);

			return result;
		}

		protected virtual PermitTypeModuleFilter GetPermitTypeModuleFilter(ZString description, PermitTypeModuleFilter.GetPermitTypeQuery queryDelegate, GetList getCountries)
		{
			return new PermitTypeModuleFilter(description, queryDelegate, getCountries);
		}

		ZQuery GetCountryQuery(ZString nK)
		{
			return new ZQuery(CusPermitHeaderSchema.CPH_RN_NKCountryCode, nK);
		}

		IList GetCountryList()
		{
			var result = new CodeDescriptionPairList();

			foreach (var countryFilter in CountryFilters)
			{
				var country = countryFilter.Property;
				if (!country.IsEmpty && !result.ContainsCode(country))
				{
					var description = RefCountry.LoadFromCountryCode(Factory, country)?.Description ?? ZString.Empty;
					result.AddPair(country, description);
				}
			}

			return result;
		}

		ZQuery GetPermitTypeQuery(ZString country, ZString type, ZString subtype)
		{
			ZQuery result = new ZQuery();
			if (!country.IsEmpty)
			{
				result.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, country);
			}

			if (!type.IsEmpty)
			{
				result.AddToFilter(CusPermitHeaderSchema.CPH_Type, type);
				if (!subtype.IsEmpty)
				{
					result.AddToFilter(CusPermitHeaderSchema.CPH_SubType, subtype);
				}
			}
			else if (!subtype.IsEmpty)
			{
				result.AddToFilter(CusPermitHeaderSchema.CPH_SubType, subtype);
			}
			return result;
		}

		public OrganisationsFindBoxCollection OrganisationList
		{
			get
			{
				return Factory.GetCachedValue("OrganisationList", delegate
				{
					return new OrganisationsFindBoxCollection(Factory);
				});
			}
		}

		ZQuery GetAppliesToQuery(ZGuid value)
		{
			return PermitFindBoxCollection.GetAppliesToQuery(value);
		}

		ZQuery GetPermitHolderQuery(ZGuid value)
		{
			return new ZQuery(CusPermitHeaderSchema.CPH_OH_PermitHolder, value);
		}

		ZQuery GetPermitNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(CusPermitHeaderSchema.CPH_Number, comparisonOperator, value);
		}

		ZQuery GetPermitRuleQuery(ZString country, ZString ruleCode, ZString value)
		{
			return PermitFindBoxCollection.GetRuleQueryForCodePartOnly(country, ruleCode);
		}

		ZQuery GetStartDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZQuery();
			AddDateTimeRange(result, comparisonOperator, JoinCondition.And, CusPermitHeaderSchema.CPH_StartDate, value1, value2);
			return result;
		}

		ZQuery GetEndDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZQuery();
			AddDateTimeRange(result, comparisonOperator, JoinCondition.And, CusPermitHeaderSchema.CPH_EndDate, value1, value2);
			if (comparisonOperator == DateComparisonOperator.HasDateInRange && !value1.IsEmpty && value2.IsEmpty)
			{
				result.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, null);
			}
			return result;
		}

		public bool MatchesFilter(BaseCusPermitHeader permitHeader)
		{
			return MatchesPermitRuleModuleFilterIncludingValuePart(permitHeader);
		}

		bool MatchesPermitRuleModuleFilterIncludingValuePart(BaseCusPermitHeader permitHeader)
		{
			var isMatch = true;
			var orCategoryGroups = GroupFiltersByOrCategory(ActiveModuleFiltersForQuery.OfType<PermitRuleModuleFilter>());

			foreach (var group in orCategoryGroups)
			{
				var orGroup = group.Value;
				Func<PermitRuleModuleFilter, bool> predicate = filter => filter.Property2.IsEmpty || permitHeader.CusPermitRules.Any(x => x.MatchesValue(filter.Property2));
				if (group.Key == FilterOrCategory.None)
				{
					isMatch &= orGroup.OfType<PermitRuleModuleFilter>().All(predicate);
				}
				else
				{
					isMatch &= orGroup.OfType<PermitRuleModuleFilter>().Any(predicate);
				}

				if (!isMatch)
				{
					break;
				}
			}

			return isMatch;
		}

		Dictionary<FilterOrCategory, OrGroup> GroupFiltersByOrCategory(IEnumerable<ModuleFilter> activeModuleFiltersForQuery)
		{
			var result = new Dictionary<FilterOrCategory, OrGroup>();

			foreach (var filter in activeModuleFiltersForQuery)
			{
				var orCategory = filter.OrCategory;
				OrGroup orGroup;
				if (!result.TryGetValue(orCategory, out orGroup))
				{
					orGroup = new OrGroup();
					result.Add(orCategory, orGroup);
				}

				orGroup.Add(filter);
			}

			return result;
		}

		class OrGroup : List<ModuleFilter>
		{
		}
	}
}
