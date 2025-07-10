using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(LiquidationGroupNotificationRegistryItem))]
	sealed class LiquidationGroupNotificationRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<LiquidationGroupNotification>
	{
		protected override StronglyTypedRegistryItem<LiquidationGroupNotification, LiquidationGroupNotification> GetNewRegistryItem()
		{
			return new LiquidationGroupNotificationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, LiquidationGroupNotification.Default);
		}
	}
}
