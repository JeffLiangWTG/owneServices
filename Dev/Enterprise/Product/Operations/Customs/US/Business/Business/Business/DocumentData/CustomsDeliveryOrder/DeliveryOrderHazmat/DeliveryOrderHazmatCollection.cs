using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class DeliveryOrderHazmatCollection : DependentCusAddInfoCollection<DeliveryOrderHazmat, DeliveryOrderHeader>
	{
		public DeliveryOrderHazmatCollection(DeliveryOrderHeader master)
			: base(master, CusAddInfoTypeAttribute.Codes.USDeliveryOrderHazmat)
		{
		}
	}
}
