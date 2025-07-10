using CargoWise.EntityFramework;

namespace Enterprise.Freight.Common.Business
{
	public class CommonCartageTypeCollection : ActiveBusinessObjectCollection<CommonCartageType>
	{
		public CommonCartageTypeCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public CommonCartageTypeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
