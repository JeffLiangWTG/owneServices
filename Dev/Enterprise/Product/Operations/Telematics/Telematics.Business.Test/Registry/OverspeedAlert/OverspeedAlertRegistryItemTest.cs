using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Registry.Testing
{
	[TestedType(typeof(OverspeedAlertRegistryItem))]
	class OverspeedAlertRegistryItemTest : StronglyTypedRegistryItemTestCase<OverspeedAlertConfiguration>
	{
		protected override StronglyTypedRegistryItem<OverspeedAlertConfiguration, OverspeedAlertConfiguration> GetNewRegistryItem()
		{
			return new OverspeedAlertRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}
	}
}
