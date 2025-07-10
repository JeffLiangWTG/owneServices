using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class JobMawbCollection : BusinessObjectCollection<JobMawb>
	{
		public JobMawbCollection(BusinessObjectFactory factory, ZQuery query) : base(factory, query)
		{
		}

		public JobMawbCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
