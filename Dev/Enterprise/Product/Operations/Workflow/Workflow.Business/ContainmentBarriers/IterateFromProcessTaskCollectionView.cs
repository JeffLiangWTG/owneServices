using System;
using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Workflow.Business
{
	public class IterateFromProcessTaskCollectionView : ProcessTaskCollectionView
	{
		public IterateFromProcessTaskCollectionView(ProcessTask containmentBarrierTask)
			: base(containmentBarrierTask.Parent.WorkflowItems)
		{
			containmentBarrierTask.RequireContainmentBarrierTask();
			this.containmentBarrierTask = containmentBarrierTask;

			Rebuild();
		}

		readonly ProcessTask containmentBarrierTask;

		public IProcessHeader SelectedWorkflow { get; set; }

		protected override void RebuildCore()
		{
			base.RebuildCore();
			Sort(GetDefaultOrderComparer());
		}

		protected override IComparer OverrideBaseComparer(IComparer baseComparer) => new IterateFromOrder(base.OverrideBaseComparer(baseComparer));

		class IterateFromOrder : IComparer
		{
			public IterateFromOrder(IComparer baseComparer)
			{
				this.baseComparer = baseComparer;
			}

			readonly IComparer baseComparer;

			public int Compare(object x, object y)
			{
				if (x is ProcessTask t1 && y is ProcessTask t2)
				{
					var result = t1.WorkflowSequence.CompareTo(t2.WorkflowSequence);
					if (result != 0)
					{
						return result;
					}
					result = t1.P9_Sequence.CompareTo(t2.P9_Sequence);
					if (result != 0)
					{
						return result;
					}
					result = t1.IsQualityContainmentBarrierTask().CompareTo(t2.IsQualityContainmentBarrierTask());
					if (result != 0)
					{
						return result;
					}
				}

				return baseComparer.Compare(x, y);
			}
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			if (containmentBarrierTask != null && !containmentBarrierTask.IsDeleted && base.IsThisPartOfTheCollection(element))
			{
				Func<IProcessTask, bool> containTask;

				if (SelectedWorkflow == null)
				{
					containTask = t => (BusinessObject)t == element;
				}
				else
				{
					containTask = t => (BusinessObject)t == element && t.P9_FH_ProcessHeader == SelectedWorkflow.PK;
				}
				return ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(containmentBarrierTask, checkWithinJobOnly: true).Any((containTask));
			}

			return false;
		}

		public IBusinessObjectCollection GetWorkflows()
		{
			var workflowsList = ObjectFactory.Get<IProcessHeaderCollectionProvider>().GetCollectionWithAdhocRelationship(containmentBarrierTask.Factory);

			foreach (var task in this)
			{
				var processHeader = ((ProcessTask)task).ProcessHeader;
				if (processHeader != null && !workflowsList.Contains(processHeader))
				{
					workflowsList.Add(processHeader);
				}
			}

			return workflowsList;
		}
	}
}
