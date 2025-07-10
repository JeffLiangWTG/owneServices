using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(AutoBillingGroupNotificationRegistryItem))]
	sealed class AutoBillingGroupNotificationRegistryItemTest : StronglyTypedRegistryItemTestCase<AutoBillingGroupNotification>
	{
		protected override StronglyTypedRegistryItem<AutoBillingGroupNotification, AutoBillingGroupNotification> GetNewRegistryItem()
		{
			return new AutoBillingGroupNotificationRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
