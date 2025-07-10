using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class CusCalculationRulesFilterBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter strings")]
		public static class FilterConstants
		{
			public const string EffectiveDate = "Effective Date";
			public const string RuleType = "Rule Type";
			public const string TransportMode = "Transport Mode";
			public const string Importer = "Importer";
		}

		#region Filter

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddDatesFilters(filters);
			AddModesAndTypeFilters(filters);
			AddOrganisationsFilters(filters);
			AddCurrentCompanyFilter(filters);
			return filters;
		}

		void AddDatesFilters(ModuleFilterCollection filters)
		{
			var effectiveDateFilter = filters.AddSingleDateFilter(FilterConstants.EffectiveDate, GetEffectiveDateQuery);
			effectiveDateFilter.Category = FilterCategories.Dates;
			effectiveDateFilter.MultilingualDescription = ResString.GetMultilingualString("FA106066-4DC6-46C7-9CEA-A7F2A4A2B4B3", FilterConstants.EffectiveDate);
		}

		void AddModesAndTypeFilters(ModuleFilterCollection filters)
		{
			var ruleTypeFilter = filters.AddTextFilter(FilterConstants.RuleType, CusCalculationRuleSchema.CCR_RuleType, Lookups.RuleTypeList);
			ruleTypeFilter.Category = FilterCategories.ModesAndTypes;
			ruleTypeFilter.MultilingualDescription = ResString.GetMultilingualString("3C8E4533-A176-4DC9-8645-D22BF3C41D96", FilterConstants.RuleType);

			var transportModeFilter = filters.AddTextFilter(FilterConstants.TransportMode, CusCalculationRuleSchema.CCR_TransportMode, Lookups.TransportModeList);
			transportModeFilter.Category = FilterCategories.ModesAndTypes;
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("B82C34A5-7852-4A74-BF2C-C33A7114762D", FilterConstants.TransportMode);
		}

		void AddOrganisationsFilters(ModuleFilterCollection filters)
		{
			var importerFilter = filters.AddGuidFilter(FilterConstants.Importer, ModuleIDs.Organisation, CusCalculationRuleSchema.CCR_OH_Importer, Lookups.ImporterList);
			importerFilter.Category = FilterCategories.Organisations;
			importerFilter.MultilingualDescription = ResString.GetMultilingualString("9452E496-C0D5-41C9-95FA-D9A543B18495", FilterConstants.Importer);
		}

		void AddCurrentCompanyFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Current Company", GetCurrentCompanyFilter);
			filter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
		}

		ZQuery GetCurrentCompanyFilter(SQLComparisonOperator sqlOperator, ZString value)
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(CusCalculationRuleSchema.CCR_GC_Company, GlbCompany.CurrentCompany.PK);

			return result;
		}

		ZQuery GetEffectiveDateQuery(ZDateTime effectiveDate)
		{
			if (effectiveDate.IsValid)
			{
				effectiveDate = TimeFactory.Instance.GetUtcFromLocalTime(effectiveDate.ToDateTime());
			}
			var query = new ZQuery(CusCalculationRuleSchema.CCR_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
			query.AddToFilter(CusCalculationRuleSchema.CCR_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
			return query;
		}

		#endregion

		#region Lookups

		public CusCalculationRulesFilterLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = GetNewLookups();
				}
				return lookups;
			}
		}
		CusCalculationRulesFilterLookups lookups;

		protected virtual CusCalculationRulesFilterLookups GetNewLookups()
		{
			return new CusCalculationRulesFilterLookups(this);
		}

		#endregion
	}
}
