using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class JobServiceCollection : ActiveBusinessObjectCollection<JobService>
	{
		public JobServiceCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter) { }

		public JobServiceCollection(BusinessObjectFactory factory)
			: base(factory) { }
	}
}
