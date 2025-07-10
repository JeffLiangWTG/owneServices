using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class DeliveryOrderLineCollection : DependentCusAddInfoCollection<DeliveryOrderLine, DeliveryOrderHeader>
	{
		public DeliveryOrderLineCollection(DeliveryOrderHeader master)
			: base(master, CusAddInfoTypeAttribute.Codes.USDeliveryOrderLine)
		{
		}
	}
}
