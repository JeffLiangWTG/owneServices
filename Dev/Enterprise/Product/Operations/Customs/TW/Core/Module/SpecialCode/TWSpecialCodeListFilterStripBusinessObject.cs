using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.TW.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.TW.Module
{
	public class TWSpecialCodeListFilterStripBusinessObject : ZZRefCusCodeListWrapperFilterStripBusinessObject
	{
		public TWSpecialCodeListFilterStripBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			var countryOrGroupingFilter = filters.AddNkFilter(Constants.ZZRefCusCodeListFilters.CountryOrGrouping, GetCountryOrGroupingQuery, ModuleIDs.Customs.Universal.RefDataGrouping, new RefDataGroupingCollection(Factory));
			countryOrGroupingFilter.DefaultProperty = Core.Constants.CountryCodes.Taiwan;
			countryOrGroupingFilter.Visibility = FilterVisibility.AlwaysVisible;
			countryOrGroupingFilter.ReadOnly = true;
			countryOrGroupingFilter.MultilingualDescription = ResString.GetMultilingualString("TWSpecialCodeListFilter|CountryOrGrouping", Constants.ZZRefCusCodeListFilters.CountryOrGrouping);

			var listTypeFilter = filters.AddTextFilter(Constants.ZZRefCusCodeListFilters.ListType, GetListTypeQuery, GetTypeList);
			listTypeFilter.DefaultProperty = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SpecialCodesForExemptionOfControllingAgencies;
			listTypeFilter.Visibility = FilterVisibility.AlwaysVisible;
			listTypeFilter.ReadOnly = true;
			listTypeFilter.MultilingualDescription = ResString.GetMultilingualString("TWSpecialCodeListFilter|ListType", Constants.ZZRefCusCodeListFilters.ListType);

			var effectiveDateFilter = filters.AddSingleDateFilter(Constants.ZZRefCusCodeListFilters.EffectiveDate, (ZDateTime date) =>
			{
				var dateQuery = new ZQuery();
				if (date.IsValid)
				{
					dateQuery.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, date);
					dateQuery.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, date);
				}
				return dateQuery;
			});
			effectiveDateFilter.Property1 = ZDateTime.Today;
			effectiveDateFilter.Visibility = FilterVisibility.AlwaysVisible;
			effectiveDateFilter.MultilingualDescription = ResString.GetMultilingualString("TWSpecialCodeListFilter|EffectiveDate", Constants.ZZRefCusCodeListFilters.EffectiveDate);

			var controllingAgencyFilter = filters.AddTextFilter(RefCusCodeListAttributes.ControllingAgency, GetControllingAgencyQuery, GetControllingAgencyTypeForValueList);
			controllingAgencyFilter.Category = FilterCategories.AttributeSearch;
			controllingAgencyFilter.Visibility = FilterVisibility.AlwaysVisible;
			controllingAgencyFilter.ComparisonOperator_List.Clear();
			controllingAgencyFilter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);
			controllingAgencyFilter.MultilingualDescription = ResString.GetMultilingualString("TWSpecialCodeListFilter|Controlling Agency", "Controlling Agency");

			var remarksFilter = filters.AddTextFilter(RefCusCodeListAttributes.Remarks, GetRemarksQuery);
			remarksFilter.Category = FilterCategories.AttributeSearch;
			remarksFilter.Visibility = FilterVisibility.AlwaysVisible;
			remarksFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			remarksFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			remarksFilter.MultilingualDescription = ResString.GetMultilingualString("TWSpecialCodeListFilter|Remarks", "Remarks");

			var sourceFilter = filters.AddTextFilter(RefCusCodeListAttributes.Source, GetSourceQuery);
			sourceFilter.Category = FilterCategories.AttributeSearch;
			sourceFilter.Visibility = FilterVisibility.AlwaysVisible;
			sourceFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			sourceFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			sourceFilter.MultilingualDescription = ResString.GetMultilingualString("TWSpecialCodeListFilter|Source", "Source");

			return filters;
		}

		ZQuery GetCountryOrGroupingQuery(ZString nK)
		{
			return new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, nK);
		}

		ZQuery GetListTypeQuery(ZString value)
		{
			return new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, value);
		}

		ZQuery GetControllingAgencyQuery(ZString value)
		{
			var attributeNameQuery = new ZQuery(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, RefCusCodeListAttributes.ControllingAgency);
			attributeNameQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_Value, SQLComparisonOperator.Equal, value);
			return GetAttributeSubQuery(attributeNameQuery);
		}

		ZQuery GetRemarksQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var remarksQuery = new ZQuery(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, RefCusCodeListAttributes.Remarks);
			remarksQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_Value, comparisonOperator, value);
			return GetAttributeSubQuery(remarksQuery);
		}

		ZQuery GetSourceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var remarksQuery = new ZQuery(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, RefCusCodeListAttributes.Source);
			remarksQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_Value, comparisonOperator, value);
			return GetAttributeSubQuery(remarksQuery);
		}

		public ZQuery GetAttributeSubQuery(ZQuery filter)
		{
			var result = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
			var attributeSubQuery = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList);
			attributeSubQuery.AddToFilter(filter);
			result.AddSubQuery(attributeSubQuery, JoinCondition.And);
			return result;
		}

		IList GetControllingAgencyTypeForValueList() => Enterprise.Customs.TW.Business.TWSpecialCode.GetControllingAgencyTypeForValueList(Factory);

		IList GetTypeList() => RefCusCodeTypeList.GetListByCountry(Factory, Core.Constants.CountryCodes.Taiwan, false);
	}
}
