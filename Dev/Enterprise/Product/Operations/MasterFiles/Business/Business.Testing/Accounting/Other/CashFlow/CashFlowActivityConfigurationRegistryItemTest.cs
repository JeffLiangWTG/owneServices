using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CashFlowActivityConfigurationRegistryItem))]
	sealed class CashFlowActivityConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<CashFlowActivityConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<CashFlowActivityConfigurationCollection, CashFlowActivityConfigurationCollection> GetNewRegistryItem()
		{
			return new CashFlowActivityConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}
	}
}
