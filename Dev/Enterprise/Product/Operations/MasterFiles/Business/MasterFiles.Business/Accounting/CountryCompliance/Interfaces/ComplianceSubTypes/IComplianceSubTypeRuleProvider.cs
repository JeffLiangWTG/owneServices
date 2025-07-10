using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IComplianceSubTypeRuleProvider
	{
		void SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection);
	}
}
