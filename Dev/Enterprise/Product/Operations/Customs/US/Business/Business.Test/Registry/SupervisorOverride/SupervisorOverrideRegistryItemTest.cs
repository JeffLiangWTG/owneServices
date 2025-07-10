using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(SupervisorOverrideRegistryItem))]
	sealed class SupervisorOverrideRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<SupervisorOverrideData>
	{
		protected override StronglyTypedRegistryItem<SupervisorOverrideData, SupervisorOverrideData> GetNewRegistryItem()
		{
			return new SupervisorOverrideRegistryItem("", null, null, null, RegistryStorageFlags.System, new SupervisorOverrideData());
		}
	}
}
