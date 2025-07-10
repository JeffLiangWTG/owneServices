using Enterprise.MasterFiles.Business;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkRequestProcessTaskCollection : ProcessTaskCollection
	{
		public WorkRequestProcessTaskCollection(WorkRequest parent)
			: base(parent)
		{
		}

		public new WorkRequestProcessTask this[int index] => (WorkRequestProcessTask)Elements[index];

		public new WorkRequestProcessTask AddNew() => (WorkRequestProcessTask)base.AddNew();

		public new WorkRequest Parent => (WorkRequest)base.Parent;
	}
}
