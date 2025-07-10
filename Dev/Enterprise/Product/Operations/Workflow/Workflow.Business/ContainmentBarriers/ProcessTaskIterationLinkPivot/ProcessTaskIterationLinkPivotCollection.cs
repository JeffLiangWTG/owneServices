using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public class ProcessTaskIterationLinkPivotCollection : ActiveBusinessObjectCollection<ProcessTaskIterationLinkPivot>, IProcessTaskIterationLinkPivotCollection
	{
		public ProcessTaskIterationLinkPivotCollection(ProcessTaskIterationLink iterationLink)
			: base(iterationLink.Factory, iterationLink, new ZQuery(), ProcessTaskIterationLinkPivotSchema.P9P_P9I_Iteration)
		{
			this.iterationLink = iterationLink;
		}

		readonly ProcessTaskIterationLink iterationLink;

		public IProcessTaskIterationLinkPivot AddNewForTask(IProcessTask task)
		{
			var pivot = AddNew();

			pivot.P9P_P9_Task = task.PK;
			pivot.P9P_ParentId = task.P9_ParentID;
			pivot.P9P_ParentTableCode = task.P9_ParentTableCode;

			return pivot;
		}

		protected override void SetDefaultsForNewElementCore(ProcessTaskIterationLinkPivot newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.P9P_P9I_Iteration = iterationLink.PK;
		}

		IProcessTaskIterationLinkPivot IProcessTaskIterationLinkPivotCollection.this[int i]
		{
			get { return base[i]; }
		}
	}
}
