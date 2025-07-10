using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public class USDeliveryOrderLineAddInfoLookups : AutoUSDeliveryOrderLineAddInfoLookups
	{
		public USDeliveryOrderLineAddInfoLookups(AutoUSDeliveryOrderLineAddInfo parent)
			: base(parent)
		{
		}

		public ShippingOrPackingingUnitList PackageTypeList
		{
			get { return Factory.GetCachedValue<ShippingOrPackingingUnitList>(); }
		}
	}
}
