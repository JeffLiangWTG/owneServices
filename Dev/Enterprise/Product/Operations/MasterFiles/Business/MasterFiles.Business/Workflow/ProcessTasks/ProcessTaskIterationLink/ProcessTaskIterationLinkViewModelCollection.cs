using System;
using CargoWise.EntityFramework;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskIterationLinkViewModelCollection : NonPersistentBusinessObjectCollection<ProcessTaskIterationLinkViewModel>
	{
		public ProcessTaskIterationLinkViewModelCollection(ProcessTask task)
			: base(task?.Factory)
		{
			this.task = task;
		}

		public void Build()
		{
			if (task != null)
			{
				RemoveAll();

				var query = GetQuery(task);

				foreach (var taskIteration in Factory.Load<IProcessTaskIterationLink>(query))
				{
					Add(new ProcessTaskIterationLinkViewModel(taskIteration));
				}
			}
		}

		#region NonPersistentBusinessObjectCollection Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotSupportedException();

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		#endregion

		#region Implementation

		readonly ProcessTask task;

		static ZQuery GetQuery(ProcessTask task)
		{
			var query = new ZQuery(ProcessTaskIterationLinkSchema.P9I_P9_ContainmentBarrierTask, task.PK);
			query.AddToFilter(ProcessTaskIterationLinkSchema.P9I_LinkType, IterationLinkTypeList.Codes.QualityIterationTask);
			query.OrderBy = ProcessTaskIterationLinkSchema.P9I_SystemLastEditTimeUtc.Name + " DESC, " + ProcessTaskIterationLinkSchema.P9I_SystemCreateTimeUtc.Name + " DESC";

			return query;
		}

		#endregion
	}
}