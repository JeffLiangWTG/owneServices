using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business
{
	[RegistryEditor("Enterprise.Freight.GUI.DelayAlertDeliveryRegistryItemEditor, Enterprise.Freight.GUI")]
	public class DelayAlertDeliveryRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DelayAlertDeliveryRuleCollection>
	{
		public DelayAlertDeliveryRegistryDataType()
			: base(new DelayAlertDeliveryRuleCollection()) { }

		public DelayAlertDeliveryRegistryDataType(DelayAlertDeliveryRuleCollection defaultValue)
			: base(defaultValue) { }
	}
}
