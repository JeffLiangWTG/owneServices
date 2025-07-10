using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch : ComplianceSubTypeAttributionCommonRuleSet
	{
		public ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch()
		{
		}

		public ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch(FallbackLevel fallbackLevel, BusinessObjectFactory factory, string ruleSetCode)
			: base(fallbackLevel, factory, ruleSetCode)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			CurrentFallbackLevel = fallbackLevel;
			return new ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch(fallbackLevel, factory, RuleSetCode);
		}
	}
}
