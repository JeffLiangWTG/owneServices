using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceSubTypeDependencyConfigurationRegistryItem))]
	sealed class ComplianceSubTypeDependencyConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<ComplianceSubTypeDependencyConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<ComplianceSubTypeDependencyConfigurationCollection, ComplianceSubTypeDependencyConfigurationCollection> GetNewRegistryItem()
		{
			return new ComplianceSubTypeDependencyConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}
	}
}
