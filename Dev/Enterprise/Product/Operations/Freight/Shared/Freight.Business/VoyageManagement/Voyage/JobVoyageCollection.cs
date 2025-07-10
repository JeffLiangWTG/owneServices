using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class JobVoyageCollection : BusinessObjectCollection<JobVoyage>, Integration.IJobVoyageCollection
	{
		public JobVoyageCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public JobVoyageCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
