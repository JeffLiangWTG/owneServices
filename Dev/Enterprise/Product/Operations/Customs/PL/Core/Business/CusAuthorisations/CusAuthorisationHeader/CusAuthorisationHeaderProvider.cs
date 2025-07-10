using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.PL.Business;

class CusAuthorisationHeaderProvider : EU.Business.CusAuthorisationHeaderProvider
{
	public CusAuthorisationHeaderProvider(ZString countryCode) : base(countryCode)
	{
	}

	protected override string GetRuleValueFromFieldTypeCore(CusAuthorisationRule cusAuthorisationRule) =>
		cusAuthorisationRule?.AuthorisationHeader is CusAuthorisationHeader header
		&& header != null
		&& cusAuthorisationRule.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.Location
			? nameof(FieldType.Text)
			: base.GetRuleDescriptionFieldTypeCore(cusAuthorisationRule);

	protected override ZInt GetRuleValueFromMaxLengthCore(CusAuthorisationRule cusAuthorisationRule)
	{
		const int maxLengthForRuleCodeLOC = 17;
		if (cusAuthorisationRule.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.Location)
		{
			return maxLengthForRuleCodeLOC;
		}
		return base.GetRuleValueFromMaxLengthCore(cusAuthorisationRule);
	}

	protected override Customs.Business.CusAuthorisationRuleValidation GetNewValidationCore(CusAuthorisationRule cusAuthorisationRule) => new CusAuthorisationRuleValidation(cusAuthorisationRule);

	protected override bool ShowCustomsCodeCore => true;
}
