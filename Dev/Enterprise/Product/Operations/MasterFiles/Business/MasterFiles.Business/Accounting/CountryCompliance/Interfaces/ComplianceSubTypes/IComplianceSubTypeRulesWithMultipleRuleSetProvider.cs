using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IComplianceSubTypeRulesWithMultipleRuleSetProvider
	{
		CodeDescriptionPairList GetRuleSet();
		string GetDefaultRuleSet();
		void SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode);
	}
}
