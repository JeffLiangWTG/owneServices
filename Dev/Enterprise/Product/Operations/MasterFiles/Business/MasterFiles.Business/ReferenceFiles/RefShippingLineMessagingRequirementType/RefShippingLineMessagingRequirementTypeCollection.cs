using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefShippingLineMessagingRequirementTypeCollection : ActiveBusinessObjectCollection<RefShippingLineMessagingRequirementType>
	{
		public RefShippingLineMessagingRequirementTypeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public RefShippingLineMessagingRequirementTypeCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
