using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.GUI
{
	public class RefCusTariffFilterStripBusinessObject : FilterStripBusinessObject
	{
		public RefCusTariffFilterStripBusinessObject(TariffSearchHelper helper)
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = ModuleIDs.Customs.Universal.RefCusTariff.ID.ToString();
			QueryObjectType = typeof(TariffView);
			this.helper = helper;
		}

		public RefCusTariffFilterStripBusinessObject()
		{
		}

		public readonly TariffSearchHelper helper;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			var tariffCode = new ModuleTariffFilter(Constants.RefCusTariffFilters.TariffCode, TariffViewSchema.ZZ1_TariffCode, helper?.TariffFormatter);
			tariffCode.Visibility = FilterVisibility.AlwaysVisible;
			tariffCode.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains);
			tariffCode.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank);
			tariffCode.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank);
			tariffCode.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual);
			tariffCode.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotContain);
			tariffCode.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotStartsWith);
			tariffCode.MultilingualDescription = ResString.GetMultilingualString("RefCusTariffFilter|GUI|TariffCode", Constants.RefCusTariffFilters.TariffCode);
			filters.AddFilter(tariffCode);
			var description = filters.AddTextFilter(Constants.RefCusTariffFilters.DefaultLanguageDescription, GetDescriptionFilter);
			description.Visibility = FilterVisibility.AlwaysVisible;
			description.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank);
			description.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank);
			description.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual);
			description.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotContain);
			description.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotStartsWith);
			description.MultilingualDescription = ResString.GetMultilingualString("RefCusTariffFilter|GUI|Description", Constants.RefCusTariffFilters.DefaultLanguageDescription);
			Action<ZPropertyInfo, IEnumerable<ModuleTextFilter>, ZString> mandatoryValidationFunc = (ZPropertyInfo parentInfo, IEnumerable<ModuleTextFilter> relatedParents, ZString type) =>
			{
				if (relatedParents.All(relatedParent => relatedParent.Property.IsEmpty) && parentInfo.Value.IsEmpty)
				{
					parentInfo.AddError(Res.GetString("C1C97B8B-C1FB-4757-AFBB-BC5615F7893D", "Please enter a {0}.", type));
				}
			};

			tariffCode.PropertyValidation = (ZPropertyInfo info) =>
			{
				if (!info.Value.IsEmpty && ((ZString)info.Value).Length < 2)
				{
					info.AddError(TariffCodeMustBeAtLeast2Characters);
				}
				else
				{
					if (ActiveModuleFilters.OfType<ModuleTariffFilter>().Where(x => x.Description.StartsWith(Constants.RefCusTariffFilters.TariffCode, StringComparison.OrdinalIgnoreCase)).Take(2).Count() > 1)
					{
						info.AddError(OnlyOneTariffCodeFilterIsAllowed);
					}
					else
					{
						var descriptions = ActiveModuleFilters.OfType<ModuleTextFilter>().Where(x => x.Description.StartsWith(Constants.RefCusTariffFilters.TariffCode, StringComparison.OrdinalIgnoreCase) || x.Description.StartsWith(Constants.RefCusTariffFilters.DefaultLanguageDescription, StringComparison.OrdinalIgnoreCase));
						mandatoryValidationFunc(info, descriptions, ResString.GetMultilingualString("RefCusTariffFilter|GUI|TariffCode", Constants.RefCusTariffFilters.TariffCode));
					}
				}
			};

			description.PropertyValidation = (ZPropertyInfo info) =>
			{
				var desc = (ZString)info.Value;
				if (!desc.IsEmpty && (desc.Length < (helper?.PartialDescriptionMinLength ?? 3)))
				{
					info.AddError(PartialDescriptionNotLongEnough);
				}
				else
				{
					if (ActiveModuleFilters.OfType<ModuleTextFilter>().Where(x => x.Description.StartsWith(Constants.RefCusTariffFilters.DefaultLanguageDescription, StringComparison.OrdinalIgnoreCase)).Take(2).Count() > 1)
					{
						info.AddError(OnlyOneDescriptionFilterIsAllowed);
					}
					else if (!desc.IsEmpty && ((ZString)OtherText).StartsWith(desc, StringComparison.OrdinalIgnoreCase) && !ActiveModuleFilters.OfType<ModuleTextFilter>().Any(x => !x.Property.IsEmpty && x.Description.StartsWith(Constants.RefCusTariffFilters.TariffCode, StringComparison.OrdinalIgnoreCase)))
					{
						info.AddError(OtherDescriptionIsNotAllowedWithoutTariff(info.Value.ToString()));
					}
					else
					{
						var descriptions = ActiveModuleFilters.OfType<ModuleTextFilter>().Where(x => x.Description.StartsWith(Constants.RefCusTariffFilters.TariffCode, StringComparison.OrdinalIgnoreCase) || x.Description.StartsWith(Constants.RefCusTariffFilters.DefaultLanguageDescription, StringComparison.OrdinalIgnoreCase));
						mandatoryValidationFunc(info, descriptions, ResString.GetMultilingualString("RefCusTariffFilter|GUI|Description", Constants.RefCusTariffFilters.DefaultLanguageDescription));
					}
				}
			};

			// A single if statement reduces 'code complexity'
			ModuleSingleDateFilter effectiveDateFilter;
			var provider = UniversalReferenceBusinessProvider.GetProvider(Factory, helper?.DataGroupingCode ?? ZString.Empty);
			var dateTimeFormat = provider?.TariffDateTimeFormat ?? ZDateTimePickerFormat.Short;
			if (dateTimeFormat == ZDateTimePickerFormat.Long)
			{
				effectiveDateFilter = filters.AddSingleDateFilter(Constants.RefCusTariffFilters.EffectiveDate, DateTimeFilterFunc);
			}
			else
			{
				effectiveDateFilter = filters.AddSingleDateFilter(Constants.RefCusTariffFilters.EffectiveDate, DateOnlyFilterFunc);
			}
			effectiveDateFilter.DateTimeFormat = dateTimeFormat;
			effectiveDateFilter.Visibility = FilterVisibility.AlwaysApplied;
			effectiveDateFilter.Property1Validation = (ZPropertyInfo info) =>
			{
				if (info.Value.IsEmpty)
				{
					info.AddError(EffectiveDateIsRequired);
				}
				else
				{
					if (ActiveModuleFilters.OfType<ModuleSingleDateFilter>().Where(x => x.Description.StartsWith(Constants.RefCusTariffFilters.EffectiveDate, StringComparison.OrdinalIgnoreCase)).Take(2).Count() > 1)
					{
						info.AddError(OnlyOneEffectiveDateFilterIsAllowed);
					}
				}
			};

			effectiveDateFilter.MultilingualDescription = ResString.GetMultilingualString("RefCusTariffFilter|GUI|EffectiveDate", Constants.RefCusTariffFilters.EffectiveDate);
			filters.AddDateFilter(Constants.RefCusTariffFilters.PublishedDate, TariffViewSchema.ZZ1_PublishedDate).MultilingualDescription = ResString.GetMultilingualString("RefCusTariffFilter|GUI|PublishedDate", Constants.RefCusTariffFilters.PublishedDate);

			var tariffRestrictionFilter = filters.AddTextFilterForExactComparison(Constants.RefCusTariffFilters.TariffRestriction, GetTariffRestrictionQuery);
			tariffRestrictionFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			tariffRestrictionFilter.MultilingualDescription = ResString.GetMultilingualString("RefCusTariffFilter|GUI|TariffRestriction", Constants.RefCusTariffFilters.TariffRestriction);

			AddFlagFilters(filters);

			return filters;
		}
		const string OtherText = "OTHER";

		internal string OtherDescriptionIsNotAllowedWithoutTariff(string otherText) => ResString.GetMultilingualString("{6C67B383-48CF-4D88-A31D-93B455763AC2}", "Searching for '{0}' in Description should only be done when Tariff Code is specified.", otherText);
		internal string EffectiveDateIsRequired => ResString.GetMultilingualString("{8814038E-E54F-46D8-9EC9-6FB1DD528AA8}", "Effective Date must be specified.");
		internal string OnlyOneEffectiveDateFilterIsAllowed => ResString.GetMultilingualString("{0AD9BAAA-439C-4B7F-B19D-C3B85C3205F6}", "Only one Effective Date filter is allowed.");
		internal string OnlyOneDescriptionFilterIsAllowed => ResString.GetMultilingualString("{42B655DE-1F85-48F5-A08E-49FC5511BA4F}", "Only one Description filter is allowed.");
		internal string OnlyOneShowExpandedResultsFilterIsAllowed => ResString.GetMultilingualString("{1DA564B2-EAC2-4AAD-BD6B-104162BC606D}", "Only one Show Expanded Results filter is allowed.");
		internal string PartialDescriptionNotLongEnough => ResString.GetMultilingualString("0CA37F27-3968-4AD5-B6C4-E9DF3C57931E", "Description must be at least {0} characters long.", helper?.PartialDescriptionMinLength ?? 3);
		internal string OnlyOneTariffCodeFilterIsAllowed => ResString.GetMultilingualString("{4D427F5F-63FA-4B95-9BFE-119C98215168}", "Only one Tariff Code filter is allowed.");
		ZQuery DateTimeFilterFunc(ZDateTime dateTime)
		{
			var dateQuery = new ZQuery();
			if (dateTime.IsValid)
			{
				dateQuery.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualTo, dateTime);
				dateQuery.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, dateTime);
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
			}
			return dateQuery;
		}

		internal string TariffCodeMustBeAtLeast2Characters => ResString.GetMultilingualString("F39D9393-DCB7-40E6-9556-71D3FCE7AD7D", "Tariff Code must be at least 2 characters long.");

		ZQuery GetDescriptionFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(TariffView));
			result.AddToFilter(TariffViewSchema.ZZ1_Description, comparisonOperator, value);
			var subQuery = new ZDBOnlySubQuery(typeof(CusRefTariffLanguageView), CusRefTariffLanguageViewSchema.ZX7_ZZ1_Tariff);
			subQuery.AddToFilter(CusRefTariffLanguageViewSchema.ZX7_Description, comparisonOperator, value);
			subQuery.AddToFilter(CusRefTariffLanguageViewSchema.ZX7_ZX6_NKLanguage, TranslationHelper.GetLanguageCode(helper.Language));
			result.AddSubQuery(subQuery, JoinCondition.Or);
			return result;
		}

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var showExpandedResultsDescription = ResString.GetMultilingualString("RefCusTariffFilter|GUI|ShowExpandedResults", Constants.RefCusTariffFilters.ShowExpandedResults);
			var expandfilter = filters.AddFlagsFilter(Constants.RefCusTariffFilters.ShowExpandedResults, new string[] { showExpandedResultsDescription }, new GetFlagsQuery[] { (ZBool value) => new ZQuery() });
			expandfilter.Visibility = FilterVisibility.AlwaysVisible;
			expandfilter.DefaultProperties[showExpandedResultsDescription] = true;

			expandfilter.PropertyValidation = (ZPropertyInfo info) =>
			{
				if (info.Name == "Property0")
				{
					if (ActiveModuleFilters.OfType<ModuleFlagsFilter>().Where(x => x.Description.StartsWith(Constants.RefCusTariffFilters.ShowExpandedResults, StringComparison.OrdinalIgnoreCase)).Take(2).Count() > 1)
					{
						info.AddError(OnlyOneShowExpandedResultsFilterIsAllowed);
					}
				}
			};
			expandfilter.MultilingualDescription = showExpandedResultsDescription;
		}

		static ZQuery GetTariffRestrictionQuery(SQLComparisonOperator comparisonOperator, ZString value)
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
	}
}
