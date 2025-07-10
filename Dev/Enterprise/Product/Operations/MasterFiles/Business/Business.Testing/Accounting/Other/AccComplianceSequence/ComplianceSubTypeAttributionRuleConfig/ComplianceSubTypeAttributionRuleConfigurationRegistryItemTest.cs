using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceSubTypeAttributionRuleConfigurationRegistryItem))]
	sealed class ComplianceSubTypeAttributionRuleConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<ComplianceSubTypeAttributionRuleConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<ComplianceSubTypeAttributionRuleConfigurationCollection, ComplianceSubTypeAttributionRuleConfigurationCollection> GetNewRegistryItem()
		{
			return new ComplianceSubTypeAttributionRuleConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}
	}
}
