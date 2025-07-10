using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch))]
	sealed class ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranchRegistryItemTest : StronglyTypedRegistryItemTestCase<ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch>
	{
		protected override StronglyTypedRegistryItem<ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch, ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch> GetNewRegistryItem()
		{
			return new ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranchRegistryItem("", null, null, null, RegistryStorageFlags.Branch, RegistryOptions.Default);
		}
	}
}
