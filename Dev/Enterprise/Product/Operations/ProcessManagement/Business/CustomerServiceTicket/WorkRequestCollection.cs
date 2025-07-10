using CargoWise.EntityFramework;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkRequestCollection : ActiveBusinessObjectCollection<WorkRequest>
	{
		public WorkRequestCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WorkRequestCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
