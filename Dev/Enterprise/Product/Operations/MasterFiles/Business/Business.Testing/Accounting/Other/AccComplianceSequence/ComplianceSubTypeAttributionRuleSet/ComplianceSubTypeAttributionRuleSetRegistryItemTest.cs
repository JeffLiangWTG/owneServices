using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceSubTypeAttributionRuleSetRegistryItem))]
	sealed class ComplianceSubTypeAttributionRuleSetRegistryItemTest : StronglyTypedRegistryItemTestCase<ComplianceSubTypeAttributionRuleSet>
	{
		protected override StronglyTypedRegistryItem<ComplianceSubTypeAttributionRuleSet, ComplianceSubTypeAttributionRuleSet> GetNewRegistryItem()
		{
			return new ComplianceSubTypeAttributionRuleSetRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}
	}
}
