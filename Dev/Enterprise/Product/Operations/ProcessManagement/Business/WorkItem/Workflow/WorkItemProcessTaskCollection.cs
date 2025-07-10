using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkItemProcessTaskCollection : ProcessTaskCollection
	{
		public WorkItemProcessTaskCollection(WorkItem workItem) : base(workItem) { }
		public WorkItemProcessTaskCollection(BusinessObjectFactory factory) : base(factory) { }

		public new WorkItemProcessTask this[int index]
		{
			get { return (WorkItemProcessTask)Elements[index]; }
		}

		public new WorkItemProcessTask AddNew()
		{
			return (WorkItemProcessTask)base.AddNew();
		}

		public new WorkItem Parent
		{
			get { return (WorkItem)base.Parent; }
		}
	}
}
