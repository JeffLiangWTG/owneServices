using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.PL.NCTS.Module;

public class AuthorisationRuleFilterBusinessObject : FilterStripBusinessObject
{
	public AuthorisationRuleFilterBusinessObject()
	{
		QueryObjectType = typeof(CusAuthorisationRule);
	}

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		var filters = new ModuleFilterCollection();

		var ruleCodeFilter = filters.AddTextFilter(PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.RuleCode, code => new ZQuery(CusPermitRuleSchema.CPR_RuleCode, code), new CusAuthorisationRuleTypeList());
		ruleCodeFilter.MultilingualDescription = ResString.GetMultilingualString("e85124e8-0a98-4b21-ae27-b5ba4c596594", PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.RuleCode);
		ruleCodeFilter.DefaultProperty = CusAuthorisationRuleTypeList.Codes.Location;
		ruleCodeFilter.Visibility = FilterVisibility.AlwaysVisible;
		ruleCodeFilter.ReadOnly = true;
		var valueFilter = filters.AddTextFilter(PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.Value, CusPermitRuleSchema.CPR_ValueFrom);
		valueFilter.MultilingualDescription = ResString.GetMultilingualString("f1d65204-caef-43c1-a419-8f69f4496c09", PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.Value);
		var descriptionFilter = filters.AddTextFilter(PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.Description, CusPermitRuleSchema.CPR_Description);
		descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("e5e18985-a106-4a87-abbf-aeaac629e0ed", PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.Description);
		AddRelatedItemFilter(filters);
		return filters;
	}

	void AddRelatedItemFilter(ModuleFilterCollection filters)
	{
		var subGroup = new AuthorisationHeaderSubGroup();
		var typeFilter = filters.AddTextFilter(PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.AuthorizationType, type => new ZQuery(CusPermitHeaderSchema.CPH_Type, type), AuthorisationTypeList);
		typeFilter.SubGroup = subGroup;
		typeFilter.MultilingualDescription = ResString.GetMultilingualString("aaaeb9c3-2ae7-4235-9aed-ff5ce2ae723d", PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.AuthorizationType);
		var numberFilter = filters.AddTextFilter(PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.AuthorizationNumber, CusPermitHeaderSchema.CPH_Number);
		numberFilter.SubGroup = subGroup;
		numberFilter.MultilingualDescription = ResString.GetMultilingualString("7b8b3630-16c8-4ee4-868d-7c11674d85d3", PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.AuthorizationNumber);
		var holderFilter = filters.AddGuidFilter(PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.AuthorizationHolder, ModuleIDs.Organisation, holder => new ZQuery(CusPermitHeaderSchema.CPH_OH_PermitHolder, holder), new OrganisationsFindBoxCollection(Factory));
		holderFilter.SubGroup = subGroup;
		holderFilter.MultilingualDescription = ResString.GetMultilingualString("0912145d-becf-4476-a01f-85bf1a748c60", PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.AuthorizationHolder);
		var startDateFilter = filters.AddDateFilter(PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.StartDate, CusPermitHeaderSchema.CPH_StartDate);
		startDateFilter.SubGroup = subGroup;
		startDateFilter.MultilingualDescription = ResString.GetMultilingualString("f7c4d3bb-3c1f-4c8b-8465-e1c42bf8206c", PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.StartDate);
		var endDateFilter = filters.AddDateFilter(PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.EndDate, CusPermitHeaderSchema.CPH_EndDate);
		endDateFilter.SubGroup = subGroup;
		endDateFilter.MultilingualDescription = ResString.GetMultilingualString("cf06e601-4de4-412a-b38f-807bbba78a44", PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.EndDate);
		var countryFilter = filters.AddNkFilter(PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.Country, nK => new ZQuery(CusPermitHeaderSchema.CPH_RN_NKCountryCode, nK), ModuleIDs.RefCountry, new RefCountryCollection(Factory));
		countryFilter.MultilingualDescription = ResString.GetMultilingualString("2e58a8fd-e8eb-46df-9f30-c2ad48f70323", PL.NCTS.Business.CusAuthorisationRuleCollection.FilterConstants.Country);
		countryFilter.SubGroup = subGroup;
		countryFilter.DefaultProperty = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		countryFilter.Visibility = FilterVisibility.AlwaysVisible;
		countryFilter.ReadOnly = true;
	}

	sealed class AuthorisationHeaderSubGroup : ModuleFilterSubGroup
	{
		public override ZQuery GetSubQuery(ZQuery filter)
		{
			var result = new ZDBOnlyQuery(typeof(CusAuthorisationRule));
			var query = new ZDBOnlySubQuery(typeof(CusAuthorisationHeader), CusPermitRuleSchema.CPR_CPH_PermitHeader);
			query.AddToFilter(filter);
			result.AddSubQuery(query, JoinCondition.And);
			return result;
		}
	}

	public CodeDescriptionPairList AuthorisationTypeList => CusAuthorisationHeaderProvider.GetAuthorisationTypeList(Factory);

	CusAuthorisationHeaderProvider CusAuthorisationHeaderProvider => cusAuthorisationHeaderProvider ?? (cusAuthorisationHeaderProvider = CusAuthorisationHeaderProvider.GetByCountryCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
	CusAuthorisationHeaderProvider cusAuthorisationHeaderProvider;
}
