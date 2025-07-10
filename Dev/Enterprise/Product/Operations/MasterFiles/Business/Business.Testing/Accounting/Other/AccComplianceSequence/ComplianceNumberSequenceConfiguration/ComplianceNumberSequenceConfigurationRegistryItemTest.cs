using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceNumberSequenceConfigurationRegistryItem))]
	sealed class ComplianceNumberSequenceConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<ComplianceNumberSequenceConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<ComplianceNumberSequenceConfigurationCollection, ComplianceNumberSequenceConfigurationCollection> GetNewRegistryItem()
		{
			return new ComplianceNumberSequenceConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}
	}
}
