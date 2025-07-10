
using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public abstract class JobStorageCollection : BusinessObjectCollection<JobStorage>
	{
		public JobStorageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}

