using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(DelayAlertDeliveryRegistryItem))]
	sealed class DelayAlertDeliveryRegistryItemTest : StronglyTypedRegistryItemTestCase<DelayAlertDeliveryRuleCollection>
	{
		#region Implementation

		protected override StronglyTypedRegistryItem<DelayAlertDeliveryRuleCollection, DelayAlertDeliveryRuleCollection> GetNewRegistryItem()
		{
			return new DelayAlertDeliveryRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		#endregion
	}
}
