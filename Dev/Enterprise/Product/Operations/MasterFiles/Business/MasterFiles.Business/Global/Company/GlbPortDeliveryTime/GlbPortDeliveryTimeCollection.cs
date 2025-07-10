using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPortDeliveryTimeCollection : ActiveBusinessObjectCollection<GlbPortDeliveryTime>
	{
		public GlbPortDeliveryTimeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public GlbPortDeliveryTimeCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
