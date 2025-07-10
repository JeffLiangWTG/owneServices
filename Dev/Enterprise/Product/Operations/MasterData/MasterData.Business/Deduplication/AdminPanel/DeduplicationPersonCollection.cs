using CargoWise.EntityFramework;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationPersonCollection : BusinessObjectCollection<DeduplicationPerson>
	{
		public DeduplicationPersonCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public DeduplicationPersonCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
