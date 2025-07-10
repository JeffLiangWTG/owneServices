using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.Module
{
	public class RefHarbourRateFilterStripBusinessObject : FilterStripBusinessObject
	{
		public RefHarbourRateFilterLookup Lookups
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
		RefHarbourRateFilterLookup lookups;

		protected virtual RefHarbourRateFilterLookup GetNewLookups()
		{
			return new RefHarbourRateFilterLookup(this);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var typeFilter = filters.AddTextFilter(Constants.RefHarbourRateFilters.Type, RefHarbourRateSchema.ZXF_Type);
			typeFilter.MaxLength = RefHarbourRateSchema.ZXF_Type.MaxLength;
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("RefHarbourRateFilter|Type",
				Constants.RefHarbourRateFilters.Type);

			var portFilter = filters.AddTextFilter(Constants.RefHarbourRateFilters.Port, RefHarbourRateSchema.ZXF_Port);
			portFilter.MaxLength = RefHarbourRateSchema.ZXF_Port.MaxLength;
			portFilter.MultilingualDescription = ResString.GetMultilingualString("RefHarbourRateFilter|Port",
				Constants.RefHarbourRateFilters.Port);

			var modeFilter = filters.AddTextFilter(Constants.RefHarbourRateFilters.Mode, GetModeQuery, Lookups.ModeList);
			modeFilter.MultilingualDescription = ResString.GetMultilingualString("RefHarbourRateFilter|Mode",
				Constants.RefHarbourRateFilters.Mode);

			var commodityFilter = filters.AddTextFilter(Constants.RefHarbourRateFilters.Commodity, RefHarbourRateSchema.ZXF_Commodity);
			commodityFilter.MaxLength = RefHarbourRateSchema.ZXF_Commodity.MaxLength;
			commodityFilter.MultilingualDescription = ResString.GetMultilingualString("RefHarbourRateFilter|Commodity",
				Constants.RefHarbourRateFilters.Commodity);

			var portTaxTypeFilter = filters.AddTextFilter(Constants.RefHarbourRateFilters.PortTaxType, RefHarbourRateSchema.ZXF_PortTaxType);
			portTaxTypeFilter.MaxLength = RefHarbourRateSchema.ZXF_PortTaxType.MaxLength;
			portTaxTypeFilter.MultilingualDescription = ResString.GetMultilingualString("RefHarbourRateFilter|PortTaxType",
				Constants.RefHarbourRateFilters.PortTaxType);

			var effectiveDateFilter = filters.AddSingleDateFilter(Constants.RefHarbourRateFilters.EffectiveDate, GetEffectiveDateQuery);
			effectiveDateFilter.Category = FilterCategories.Dates;
			effectiveDateFilter.MultilingualDescription = ResString.GetMultilingualString("RefHarbourRateFilter|EffectiveDate", Constants.RefHarbourRateFilters.EffectiveDate);
			effectiveDateFilter.Property1 = ZDateTime.Today;

			var countryOrGroupingFilter = filters.AddNkFilter(Constants.RefHarbourRateFilters.CountryOrGrouping, GetCountryOrGroupingQuery, ModuleIDs.Customs.Universal.RefDataGrouping, new RefDataGroupingCollection(Factory));
			countryOrGroupingFilter.DefaultProperty = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			countryOrGroupingFilter.ForeignCodeColumnOverride = RefDataGroupingSchema.ZZZ_DataGrouping;
			countryOrGroupingFilter.MultilingualDescription = ResString.GetMultilingualString("RefHarbourRateFilter|CountryOrGrouping",
				Constants.RefHarbourRateFilters.CountryOrGrouping);
			countryOrGroupingFilter.Visibility = FilterVisibility.AlwaysVisible;

			var rateFormulaFilter = filters.AddTextFilter(Constants.RefHarbourRateFilters.RateFormula, RefHarbourRateSchema.ZXF_RateFormula);
			rateFormulaFilter.MaxLength = RefHarbourRateSchema.ZXF_RateFormula.MaxLength;
			rateFormulaFilter.MultilingualDescription = ResString.GetMultilingualString("RefHarbourRateFilter|RateFormula",
				Constants.RefHarbourRateFilters.RateFormula);

			return filters;

			ZQuery GetModeQuery(ZString value)
			{
				return new ZQuery(RefHarbourRateSchema.ZXF_Mode, value);
			}

			ZQuery GetCountryOrGroupingQuery(ZString nK)
			{
				return new ZQuery(RefHarbourRateSchema.ZXF_ZZZ_NKDataGrouping, nK);
			}

			ZQuery GetEffectiveDateQuery(ZDateTime effectiveDate)
			{
				var query = new ZQuery();
				if (effectiveDate.IsValid)
				{
					query.AddToFilter(RefHarbourRateSchema.ZXF_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
					query.AddToFilter(RefHarbourRateSchema.ZXF_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
				}
				return query;
			}
		}
	}
}
