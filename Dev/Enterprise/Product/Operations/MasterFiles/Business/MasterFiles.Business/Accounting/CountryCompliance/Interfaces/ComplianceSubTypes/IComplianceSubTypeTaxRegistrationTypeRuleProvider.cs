using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IComplianceSubTypeTaxRegistrationTypeRuleProvider
	{
		bool IsTaxRegistrationTypeRuleApplicable(IAccComplianceRule rule, OrgHeader header);
	}
}
