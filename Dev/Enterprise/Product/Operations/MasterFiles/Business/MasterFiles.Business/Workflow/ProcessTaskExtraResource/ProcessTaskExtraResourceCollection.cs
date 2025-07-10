using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskExtraResourceCollection : DependentBusinessObjectCollection<ProcessTaskExtraResource, ProcessTask>
	{
		public ProcessTaskExtraResourceCollection(ProcessTask task)
			: base(task, GetAdditionaQuery(task))
		{
		}

		static ZQuery GetAdditionaQuery(ProcessTask task)
		{
			return task.IsTask ? new ZQuery() : ZQuery.NoResultQuery;
		}

		protected override bool AllowNewCore => !Master.ReadOnly;
	}
}
