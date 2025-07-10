using System;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Customs.Universal.Internal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.Module
{
	public class RefCusTariffFilterStripBusinessObject : FilterStripBusinessObject, INomenclatureEnabler
	{
		public RefCusTariffFilterStripBusinessObject()
		{
			shouldIncludeNomenclatureGroup = true;
		}

		public RefCusTariffFilterStripBusinessObject(ChildTariffViewCollection collection)
		{
			this.QueryObjectType = typeof(ChildTariffViewCollection);
			this.collection = collection;
			if (collection != null && this is IFilterStripBusinessObjectInternals fsbObjectInternals
				&& (!collection.DataGroupingCode.IsEmpty || (!string.IsNullOrEmpty(collection.TariffType))))
			{
				fsbObjectInternals.LayoutContext = string.Join("_", collection.DataGroupingCode, collection.TariffType);
			}
		}

		protected readonly ChildTariffViewCollection collection;

		bool INomenclatureEnabler.Enable { get => shouldIncludeNomenclatureGroup; set => shouldIncludeNomenclatureGroup = value; }

		readonly ZString countryCode = Env.CurrentCompany?.Country?.Code ?? ZString.Empty;

		internal ZDateTimePickerFormat DateTimeFormat
		{
			get
			{
				if (fDateTimeFormat == null)
				{
					var provider = UniversalReferenceBusinessProvider.GetProvider(Factory, countryCode);
					fDateTimeFormat = provider?.TariffDateTimeFormat ?? ZDateTimePickerFormat.Short;
				}
				return fDateTimeFormat.Value;
			}
		}
		ZDateTimePickerFormat? fDateTimeFormat;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			var typeFilter = new DataGroupingRelatedFilter(
				Constants.RefCusTariffFilters.TariffType,
				GetTariffTypeSubQuery,
				Factory,
				ZArchitecture.FieldType.TextDropEdit,
				TariffViewSchema.ZZ1_ZZI_TariffType.MaxLength,
				DataGroupingRelatedFilterHelper.ResourceStringGetters.TariffTypeResourceString,
				DataGroupingRelatedFilterHelper.ListGetters.TariffTypeGetter);
			typeFilter.MaxLength = RefCusTariffTypeSchema.ZZI_TariffType.MaxLength;
			typeFilter.Category = FilterCategories.ModesAndTypes;
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("RefCusTariffFilter|TariffType", Constants.RefCusTariffFilters.TariffType);
			filters.AddFilter(typeFilter);
			var tariffCodeFilter = new ModuleTariffFilter();
			filters.AddFilter(tariffCodeFilter);
			tariffCodeFilter.MultilingualDescription = ResString.GetMultilingualString("RefCusTariffFilter|TariffCode", Constants.RefCusTariffFilters.TariffCode);
			filters.AddTextFilter(Constants.RefCusTariffFilters.DefaultLanguageDescription, TariffViewSchema.ZZ1_Description).MultilingualDescription = ResString.GetMultilingualString("RefCusTariffFilter|Description (Default Language)", Constants.RefCusTariffFilters.DefaultLanguageDescription);

			var alternateLanguageDescriptionFilter = filters.AddTextFilter(Constants.RefCusTariffFilters.AlternateLanguageDescription, GetAlternateLanguageDescriptionQuery);
			alternateLanguageDescriptionFilter.MaxLength = CusRefTariffLanguageViewSchema.ZX7_Description.MaxLength;
			alternateLanguageDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("RefCusTariffFilter|Description (Alternate Language)", Constants.RefCusTariffFilters.AlternateLanguageDescription);

			// A single if statement reduces 'code complexity'
			ModuleSingleDateFilter effectiveDateFilter;
			if (DateTimeFormat == ZDateTimePickerFormat.Long)
			{
				effectiveDateFilter = filters.AddSingleDateFilter(Constants.RefCusTariffFilters.EffectiveDate, DateTimeFilterFunc);
				effectiveDateFilter.Property1 = ZDateTime.Now;
			}
			else
			{
				effectiveDateFilter = filters.AddSingleDateFilter(Constants.RefCusTariffFilters.EffectiveDate, DateOnlyFilterFunc);
				effectiveDateFilter.Property1 = ZDateTime.Today;
			}
			effectiveDateFilter.DateTimeFormat = DateTimeFormat;
			effectiveDateFilter.Visibility = FilterVisibility.AlwaysVisible;
			effectiveDateFilter.MultilingualDescription = ResString.GetMultilingualString("RefCusTariffFilter|EffectiveDate", Constants.RefCusTariffFilters.EffectiveDate);

			filters.AddDateFilter(Constants.RefCusTariffFilters.PublishedDate, TariffViewSchema.ZZ1_PublishedDate).MultilingualDescription = ResString.GetMultilingualString("RefCusTariffFilter|PublishedDate", Constants.RefCusTariffFilters.PublishedDate);

			var countryOrGroupingFilter = filters.AddNkFilter(Constants.RefCusTariffFilters.CountryOrGrouping, GetCountryQuery, ModuleIDs.Customs.Universal.RefDataGrouping, new RefDataGroupingCollection(Factory));
			countryOrGroupingFilter.ForeignCodeColumnOverride = RefDataGroupingSchema.ZZZ_DataGrouping;
			countryOrGroupingFilter.MultilingualDescription = ResString.GetMultilingualString("RefCusTariffFilter|CountryOrGrouping", Constants.RefCusTariffFilters.CountryOrGrouping);
			countryOrGroupingFilter.Visibility = FilterVisibility.AlwaysVisible;

			var tariffRestrictionFilter = filters.AddTextFilterForExactComparison(Constants.RefCusTariffFilters.TariffRestriction, GetTariffRestrictionQuery);
			tariffRestrictionFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			tariffRestrictionFilter.MultilingualDescription = ResString.GetMultilingualString("RefCusTariffFilter|TariffRestriction", Constants.RefCusTariffFilters.TariffRestriction);

			var uq1Filter = filters.AddTextFilter(Constants.RefCusTariffFilters.UnitOfQuantity1, GetUnitOfQuantity1Query);
			uq1Filter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank);
			uq1Filter.MaxLength = RefCusTariffUOMSchema.ZZ8_UOM.MaxLength;
			uq1Filter.MultilingualDescription = ResString.GetMultilingualString("RefCusTariffFilter|UnitOfQuantity1", Constants.RefCusTariffFilters.UnitOfQuantity1);

			var uq2Filter = filters.AddTextFilter(Constants.RefCusTariffFilters.UnitOfQuantity2, GetUnitOfQuantity2Query);
			uq2Filter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank);
			uq2Filter.MaxLength = RefCusTariffUOMSchema.ZZ8_UOM.MaxLength;
			uq2Filter.MultilingualDescription = ResString.GetMultilingualString("RefCusTariffFilter|UnitOfQuantity2", Constants.RefCusTariffFilters.UnitOfQuantity2);

			var uq3Filter = filters.AddTextFilter(Constants.RefCusTariffFilters.UnitOfQuantity3, GetUnitOfQuantity3Query);
			uq3Filter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank);
			uq3Filter.MaxLength = RefCusTariffUOMSchema.ZZ8_UOM.MaxLength;
			uq3Filter.MultilingualDescription = ResString.GetMultilingualString("RefCusTariffFilter|UnitOfQuantity3", Constants.RefCusTariffFilters.UnitOfQuantity3);
			AddDynamicFilters(filters);
			return filters;
		}

		protected virtual void AddDynamicFilters(ModuleFilterCollection filters)
		{
			if (collection != null && !string.IsNullOrEmpty(collection.TariffType))
			{
				var attributeNames = collection.MandatoryAttributeNames.GroupBy(t => t.ZY6_Name).Select(grp => grp.First());
				foreach (var attribute in attributeNames)
				{
					filters.AddTextFilter(attribute.ZY6_ColumnCaption,
										(comparisonOperator, value) => GetAttributeTextQueryWithOperator(attribute, comparisonOperator, value));
				}
			}
		}

		protected ZQuery GetAttributeTextQueryWithOperator(RefCusTariffAttributeName attribute, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(TariffView));
			var attributeSubQuery = new ZDBOnlySubQuery(typeof(RefCusTariffAttribute), RefCusTariffAttributeSchema.ZZ3_ZZ1_Tariff);
			attributeSubQuery.AddToFilter(RefCusTariffAttributeSchema.ZZ3_Name, attribute.ZY6_Name);
			attributeSubQuery.AddToFilter(RefCusTariffAttributeSchema.ZZ3_Value, comparisonOperator, value);
			result.AddSubQuery(attributeSubQuery, JoinCondition.And);
			return result;
		}

		protected override FilterVisibility IsSystemDefinedStatusFilterVisibility => IsSelfManagedTariffCountry ? FilterVisibility.AlwaysVisible : base.IsSystemDefinedStatusFilterVisibility;

		protected override string IsSystemDefinedDefaultProperty => IsSelfManagedTariffCountry ? DefinedStatusNotSystemCode : base.IsSystemDefinedDefaultProperty;

		bool IsSelfManagedTariffCountry => ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsSelfManagedTariffCountry(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		ZQuery GetAlternateLanguageDescriptionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(TariffView));
			var workingLanguageCode = TranslationHelper.GetCurrentLanguageCode();
			var countryLanguageCode = TranslationHelper.GetCurrentCountryLanguageCode();
			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				var blankSubQuery = GetAlternateLanguageDescriptionSubQuery(comparisonOperator, value, workingLanguageCode, countryLanguageCode, true);
				query.AddSubQuery(RefCusTariffSchema.PK, blankSubQuery, JoinCondition.Or);
			}
			else if (comparisonOperator.IsNegativeSQLOperator())
			{
				var subQuery = GetAlternateLanguageDescriptionSubQuery(comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value, workingLanguageCode, countryLanguageCode, true);
				query.AddSubQuery(RefCusTariffSchema.PK, subQuery, JoinCondition.And);
			}
			else
			{
				var subQuery = GetAlternateLanguageDescriptionSubQuery(comparisonOperator, value, workingLanguageCode, countryLanguageCode, false);
				query.AddSubQuery(RefCusTariffSchema.PK, subQuery, JoinCondition.And);
			}
			return query;
		}

		ZDBOnlySubQuery GetAlternateLanguageDescriptionSubQuery(SQLComparisonOperator comparisonOperator, ZString description, ZString workinglanguageCode, ZString countryLanguageCode, bool notIn)
		{
			var query = new ZDBOnlySubQuery(typeof(CusRefTariffLanguageView), CusRefTariffLanguageViewSchema.ZX7_ZZ1_Tariff, notIn);
			if (comparisonOperator != SQLComparisonOperator.IsBlank)
			{
				var languageQuery = new ZQuery();
				languageQuery.AddToFilter(CusRefTariffLanguageViewSchema.ZX7_ZX6_NKLanguage, SQLComparisonOperator.StartsWith, workinglanguageCode);
				languageQuery.AddToFilter(JoinCondition.Or, CusRefTariffLanguageViewSchema.ZX7_ZX6_NKLanguage, SQLComparisonOperator.StartsWith, countryLanguageCode);
				query.AddToFilter(RefCusTariffSchema.PK, SQLComparisonOperator.Equal, CusRefTariffLanguageViewSchema.ZX7_ZZ1_Tariff);
				query.AddToFilter(CusRefTariffLanguageViewSchema.ZX7_Description, comparisonOperator, description);
				query.AddToFilter(languageQuery);
			}
			return query;
		}

		ZQuery DateTimeFilterFunc(ZDateTime dateTime)
		{
			var dateQuery = new ZQuery();
			if (dateTime.IsValid)
			{
				dateQuery.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualTo, dateTime);
				dateQuery.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, dateTime);
				dateQuery.AddToFilter(GetEffectiveVersionQuery(dateTime));
			}
			return dateQuery;
		}

		ZQuery DateOnlyFilterFunc(ZDateTime date)
		{
			var dateQuery = new ZQuery();
			if (date.IsValid)
			{
				dateQuery.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, date);
				dateQuery.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, date);
				dateQuery.AddToFilter(GetEffectiveVersionQuery(date));
			}
			return dateQuery;
		}

		ZDBOnlyQuery GetEffectiveVersionQuery(object value)
		{
			var query = new ZDBOnlyQuery(typeof(TariffView));
			query.AddToFilter(TariffViewSchema.ZZ1_CRT_NKTariffVersion, SQLComparisonOperator.Equal, ZString.Empty);

			if (value is ZDateTime effectiveDate)
			{
				query.AddFilterAndZSQLParameterCollection(GetVersionQuerySql(effectiveDate), new ZSqlParameterCollection(), JoinCondition.Or);
			}
			else
			{
				var versionQuery = new ZDBOnlySubQuery(typeof(CusRefTariffVersion), CusRefTariffVersionSchema.CRT_Version);
				versionQuery.AddToFilter(CusRefTariffVersionSchema.CRT_RN_NKCountryCode, value);
				query.AddSubQuery(TariffViewSchema.ZZ1_CRT_NKTariffVersion, versionQuery, JoinCondition.Or);
			}

			return query;
		}

		string GetVersionQuerySql(ZDateTime effectiveDate) => string.Format(CultureInfo.InvariantCulture, (NoResString)@"
ZZ1_CRT_NKTariffVersion IN 
(
SELECT TariffVersion.CRT_Version 
FROM
(
	SELECT	
		CRT_Version,
		VersionSequence = ROW_NUMBER()
			OVER
			(
				PARTITION BY CRT_RN_NKCountryCode
				ORDER BY CRT_EffectiveDate DESC
			)
	FROM dbo.CusRefTariffVersion
	WHERE CRT_EffectiveDate <= '{0}'
) AS TariffVersion
WHERE TariffVersion.VersionSequence = 1
)", effectiveDate.ToISO8601String());

		ZQuery GetUnitOfQuantity1Query(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetUnitOfQuantityQuery(Constants.UnitOfMeasureTypes.StatisticalUOMType, comparisonOperator, value);
		}

		ZQuery GetUnitOfQuantity2Query(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetUnitOfQuantityQuery(Constants.UnitOfMeasureTypes.AdditionalUOMType, comparisonOperator, value);
		}

		ZQuery GetUnitOfQuantity3Query(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetUnitOfQuantityQuery(Constants.UnitOfMeasureTypes.CustomsUOM3Type, comparisonOperator, value);
		}

		ZQuery GetUnitOfQuantityQuery(ZString type, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(TariffView));
			var tariffUOMSubQuery = new ZDBOnlySubQuery(typeof(TariffUOMView), TariffUOMViewSchema.ZZ8_ZZ1_ParentTariffOrNationalCode);
			tariffUOMSubQuery.AddToFilter(RefCusTariffUOMSchema.ZZ8_Type, type);
			tariffUOMSubQuery.AddToFilter(RefCusTariffUOMSchema.ZZ8_UOM, comparisonOperator, value);
			result.AddSubQuery(tariffUOMSubQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetTariffRestrictionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZQuery();
			if (!value.IsEmpty)
			{
				var tariffQuery = new ZDBOnlyQuery(typeof(TariffView));
				tariffQuery.AddFilterAndZSQLParameterCollection(value, new ZSqlParameterCollection());
				result.AddToFilter(tariffQuery);
			}
			return result;
		}

		ZDBOnlyQuery GetTariffTypeSubQuery(ZString country, ZString type)
		{
			var result = new ZDBOnlyQuery(typeof(TariffView));

			var typeQuery = new ZDBOnlyQuery(typeof(TariffView));
			typeQuery.AddToFilter(TariffViewSchema.ZZ1_ZZI_NKTariffType, SQLComparisonOperator.StartsWith, type);
			result.AddToFilter(typeQuery);

			result.AddToFilter(GetCountryQuery(country), JoinCondition.And);

			return result;
		}

		ZQuery GetCountryQuery(ZString value)
		{
			return GetCountryQuery(value, typeof(TariffView), TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, shouldIncludeNomenclatureGroup);
		}

		ZQuery GetCountryQuery(ZString value, Type bizObjType, SchemaStringColumn foreignKey, bool shouldIncludeParent)
		{
			var result = new ZDBOnlyQuery(bizObjType);
			result.AddToFilter(new ZQuery(foreignKey, value), JoinCondition.Or);
			if (shouldIncludeParent)
			{
				var countrySubQuery = new ZDBOnlySubQuery(typeof(RefDataGrouping), RefDataGroupingSchema.ZZZ_ZZZ_Grouping);
				countrySubQuery.AddToFilter(RefDataGroupingSchema.ZZZ_DataGrouping, value);

				var parentSubQuery = new ZDBOnlySubQuery(typeof(RefDataGrouping), foreignKey);
				parentSubQuery.AddSubQuery(RefDataGroupingSchema.PK, RefDataGroupingSchema.ZZZ_ZZZ_Grouping, countrySubQuery, JoinCondition.And);
				result.AddSubQuery(foreignKey, RefDataGroupingSchema.ZZZ_DataGrouping, parentSubQuery, JoinCondition.Or);
			}

			if (!value.IsEmpty)
			{
				result.AddToFilter(GetEffectiveVersionQuery(value));
			}

			return result;
		}

		bool shouldIncludeNomenclatureGroup;
	}
}
