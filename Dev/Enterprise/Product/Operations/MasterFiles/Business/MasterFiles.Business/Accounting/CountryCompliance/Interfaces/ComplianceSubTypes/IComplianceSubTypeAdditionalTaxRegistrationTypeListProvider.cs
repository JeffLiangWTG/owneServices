using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IComplianceSubTypeAdditionalTaxRegistrationTypeListProvider : IComplianceSubTypeTaxRegistrationTypeRuleProvider
	{
		CodeDescriptionPairList GetTaxRegistrationTypeList();
	}
}
