using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DefaultOSMGRegistryItem))]
	sealed class DefaultOSMGRegistryItemTest : StronglyTypedRegistryItemTestCase<DefaultOSMG>
	{
		protected override StronglyTypedRegistryItem<DefaultOSMG, DefaultOSMG> GetNewRegistryItem()
		{
			return new DefaultOSMGRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new DefaultOSMG());
		}
	}
}
