using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public class ProcessTaskIterationLinkCollection : ActiveBusinessObjectCollection<ProcessTaskIterationLink>, IProcessTaskIterationLinkCollection
	{
		public ProcessTaskIterationLinkCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ProcessTaskIterationLinkCollection(ProcessTask containmentBarrierTask)
			: base(containmentBarrierTask.Factory, new CollectionRelationship(containmentBarrierTask))
		{
		}

		public ProcessTaskIterationLinkCollection(IProcessHeader workflow)
			: base(workflow.Factory, (BusinessObject)workflow, new ZQuery(), ProcessTaskIterationLinkSchema.P9I_FH_IterationWorkflow)
		{
		}

		#region IProcessTaskIterationLinkCollection Members

		IProcessTaskIterationLink IProcessTaskIterationLinkCollection.this[int index]
		{
			get { return this[index]; }
		}

		#endregion

		#region Relationship

		class CollectionRelationship : DependentRelationship
		{
			internal CollectionRelationship(ProcessTask containmentBarrierTask)
				: base(containmentBarrierTask, typeof(ProcessTaskIterationLink), new ZQuery(), ProcessTaskIterationLinkSchema.P9I_P9_ContainmentBarrierTask)
			{
				this.containmentBarrierTask = containmentBarrierTask;
			}

			readonly ProcessTask containmentBarrierTask;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
			protected override void AddToRelationship(BusinessObject businessObject)
			{
				containmentBarrierTask.RequireContainmentBarrierTask("Can only add iteration links for containment barrier tasks");

				base.AddToRelationship(businessObject);
			}
		}

		#endregion
	}
}
