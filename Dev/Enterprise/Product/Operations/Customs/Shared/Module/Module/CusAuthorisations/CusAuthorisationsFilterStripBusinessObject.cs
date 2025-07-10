using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using FilterConstants = Enterprise.Customs.Business.CusAuthorisationHeaderCollection.FilterConstants;

namespace Enterprise.Customs.Module
{
	public class CusAuthorisationsFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var countryFilter = result.AddNkFilter(FilterConstants.Country, nK => new ZQuery(), ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			countryFilter.MultilingualDescription = ResString.GetMultilingualString("C05A8409-0DAC-432A-8906-CBF98C65846E", FilterConstants.Country);
			countryFilter.DefaultProperty = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			countryFilter.Visibility = FilterVisibility.AlwaysVisible;
			countryFilter.ReadOnly = true;

			var authorisationHolderFilter = result.AddGuidFilter(FilterConstants.AuthorisationHolder, ModuleIDs.Organisation, value => new ZQuery(CusPermitHeaderSchema.CPH_OH_PermitHolder, value), Lookups.OrganisationList);
			authorisationHolderFilter.Category = FilterCategories.Organisations;
			authorisationHolderFilter.MultilingualDescription = ResString.GetMultilingualString("3D9EBB09-722B-4563-A772-D478945DC237", FilterConstants.AuthorisationHolder);

			var startDateFilter = result.AddDateFilter(FilterConstants.StartDate, GetStartDateQuery);
			startDateFilter.Category = FilterCategories.Dates;
			startDateFilter.MultilingualDescription = ResString.GetMultilingualString("0501ABE1-500A-43C1-A11F-AF9D9B580861", FilterConstants.StartDate);

			var endDateFilter = result.AddDateFilter(FilterConstants.EndDate, GetEndDateQuery);
			endDateFilter.Category = FilterCategories.Dates;
			endDateFilter.MultilingualDescription = ResString.GetMultilingualString("C4776087-2BEB-478C-88F7-41B125BCA104", FilterConstants.EndDate);

			var authorisationNumberFilter = result.AddTextFilter(FilterConstants.AuthorisationNumber, (com, value) => new ZQuery(CusPermitHeaderSchema.CPH_Number, com, value));
			authorisationNumberFilter.MaxLength = CusPermitHeaderSchema.CPH_Number.MaxLength;
			authorisationNumberFilter.Category = FilterCategories.TextSearch;
			authorisationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("90A42D98-7836-4A50-BC7E-691967865E86", FilterConstants.AuthorisationNumber);

			var authorisationTypeFilter = result.AddTextFilter(FilterConstants.AuthorisationType, value => new ZQuery(CusPermitHeaderSchema.CPH_Type, value), () => AuthorisationTypeList);
			authorisationTypeFilter.Category = FilterCategories.TextSearch;
			authorisationTypeFilter.MultilingualDescription = ResString.GetMultilingualString("2C307FB8-535E-4C72-B43F-CEC8FFA7C280", FilterConstants.AuthorisationType);

			var currentStatusFilter = result.AddTextFilter(FilterConstants.IsCurrent, GetCurrentStatusQuery, Lookups.CurrentStatusList);
			currentStatusFilter.Category = FilterCategories.StatusAndFlags;
			currentStatusFilter.DefaultProperty = CurrentStatusList.Codes.ShowCurrentOnly;
			currentStatusFilter.MultilingualDescription = ResString.GetMultilingualString("209C6803-8740-4E45-86C6-58BAC8C20761", FilterConstants.IsCurrent);

			var authorisationDescriptionFilter = result.AddTextFilter(FilterConstants.AuthorisationDescription, (com, value) => new ZQuery(CusPermitHeaderSchema.CPH_PermitDescription, com, value));
			authorisationDescriptionFilter.MaxLength = CusPermitHeaderSchema.CPH_PermitDescription.MaxLength;
			authorisationDescriptionFilter.Category = FilterCategories.TextSearch;
			authorisationDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("60F5C3E1-74F4-43E2-8EE3-18CCA18645FF", FilterConstants.AuthorisationDescription);

			var authorisationAddressFilter = result.AddTextFilter(FilterConstants.AuthorisationAddress, GetAuthorisationAddressQuery);
			authorisationAddressFilter.MaxLength = AutoOrgAddress.Schema.OA_Address1MaxLength;
			authorisationAddressFilter.Category = FilterCategories.TextSearch;
			authorisationAddressFilter.MultilingualDescription = ResString.GetMultilingualString("7AE22114-8573-4625-9574-F6D4E9EB867A", FilterConstants.AuthorisationAddress);
			authorisationAddressFilter.SupportsBlankComparisonOperators = false;
			authorisationAddressFilter.PropertyValidation = info =>
			{
				if (ActiveModuleFilters.OfType<ModuleTextFilter>().Where(x => x.Description.StartsWith(FilterConstants.AuthorisationAddress, StringComparison.OrdinalIgnoreCase)).Take(2).Count() > 1)
				{
					info.AddError(ResString.GetMultilingualString("D3E036B2-1D0F-4C98-B064-8D99524B6E98", "Only one {0} filter is allowed.", FilterConstants.AuthorisationAddress));
				}
			};

			if (CusAuthorisationHeaderProvider.EnableAdHoc)
			{
				var authorisationAdHocFilter = result.AddFlagsFilter(FilterConstants.AdHoc, new[] { Enterprise.Customs.Module.Res.GetString("E3DB3965-0C39-40F4-BCE6-1BCAF1366D56", "Ad Hoc") }, new GetFlagsQuery[] { GetIsAdHocQuery });
				authorisationAdHocFilter.Category = FilterCategories.StatusAndFlags;
				authorisationAdHocFilter.MultilingualDescription = ResString.GetMultilingualString("39B59109-FC1B-455B-ACB0-C52D58633C33", FilterConstants.AdHoc);
			}

			var ruleDetailsFilter = new CusAuthorisationsRuleModuleFilter(FilterConstants.RuleDetails, GetRuleDetailsQuery, Lookups.RuleCodeList);
			ruleDetailsFilter.MultilingualDescription = ResString.GetMultilingualString("B10F996E-AA3F-4D89-8E71-2CAA6600E934", FilterConstants.RuleDetails);
			result.AddFilter(ruleDetailsFilter);

			return result;
		}

		ZQuery GetIsAdHocQuery(ZBool value)
		{
			var comparisonOperator = value ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
			return new ZQuery(CusPermitHeaderSchema.CPH_IsAdHoc, comparisonOperator, true);
		}

		ZDBOnlyQuery GetRuleDetailsQuery(ZString ruleCode, SQLComparisonOperator comparisonOperator, ZString ruleValue)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(CusAuthorisationRule), CusPermitRuleSchema.CPR_CPH_PermitHeader);
			if (!ruleCode.IsEmpty)
			{
				subQuery.AddToFilter(CusPermitRuleSchema.CPR_RuleCode, ruleCode);
			}
			if (!ruleValue.IsEmpty)
			{
				subQuery.AddToFilter(CusPermitRuleSchema.CPR_ValueFrom, comparisonOperator, ruleValue);
			}

			var result = new ZDBOnlyQuery(typeof(CusAuthorisationHeader));
			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		#region AuthorisationTypeList

		public CodeDescriptionPairList AuthorisationTypeList
		{
			get
			{
				var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				return Factory.GetCachedValue("CusAuthorisationsFilterStripBusinessObject.AuthorisationTypeList:" + countryCode, () =>
				{
					return CusAuthorisationHeaderProvider.GetAuthorisationTypeList(Factory);
				});
			}
		}

		#endregion

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

		ZQuery GetCurrentStatusQuery(ZString status)
		{
			var query = new ZQuery();
			if (status == CurrentStatusList.Codes.ShowNonCurrentOnly)
			{
				AddDateTimeRange(query, DateComparisonOperator.HasDateInRange, JoinCondition.And, CusPermitHeaderSchema.CPH_StartDate, ZDate.Today.AddDays(1), ZDate.Empty);
				AddDateTimeRange(query, DateComparisonOperator.HasDateInRange, JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, ZDate.Empty, ZDate.Today.AddDays(-1));
			}
			else if (status == CurrentStatusList.Codes.ShowCurrentOnly)
			{
				var endDateQuery = new ZQuery();
				AddDateTimeRange(endDateQuery, DateComparisonOperator.HasDateInRange, JoinCondition.And, CusPermitHeaderSchema.CPH_EndDate, ZDate.Today, ZDate.Empty);
				endDateQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, null);

				AddDateTimeRange(query, DateComparisonOperator.HasDateInRange, JoinCondition.And, CusPermitHeaderSchema.CPH_StartDate, ZDate.Empty, ZDate.Today);
				query.AddToFilter(endDateQuery, JoinCondition.And);
			}
			return query;
		}

		ZQuery GetAuthorisationAddressQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			subQuery.AddToFilter(OrgAddressSchema.OA_Address1, comparisonOperator, value);

			var result = new ZDBOnlyQuery(typeof(CusAuthorisationHeader));
			result.AddSubQuery(CusPermitHeaderSchema.CPH_OA_AppliesTo, subQuery, JoinCondition.And);
			return result;
		}

		CusAuthorisationHeaderProvider CusAuthorisationHeaderProvider => cusAuthorisationHeaderProvider ?? (cusAuthorisationHeaderProvider = CusAuthorisationHeaderProvider.GetByCountryCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
		CusAuthorisationHeaderProvider cusAuthorisationHeaderProvider;

		public CusAuthorisationsFilterLookups Lookups => lookups ?? (lookups = new CusAuthorisationsFilterLookups(this));
		CusAuthorisationsFilterLookups lookups;
	}
}
