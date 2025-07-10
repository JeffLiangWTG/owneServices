using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class WorkflowEventContextCollection : NonPersistentBusinessObjectCollection<WorkflowEventContext>
	{
		internal WorkflowEventContextCollection(WorkflowDescriptor workflowDescriptor)
		{
			this.workflowDescriptor = workflowDescriptor;
		}

		readonly WorkflowDescriptor workflowDescriptor;

		public void Load(IEnumerable<WorkflowEventContextPair> contextPath)
		{
			RemoveAndDeleteAll();
			foreach (var contextPair in contextPath)
			{
				Add(new WorkflowEventContext(contextPair));
			}
		}

		#region RefreshAll

#if DEBUG
		internal
#endif
		void RefreshAll()
		{
			foreach (WorkflowEventContext element in Elements)
			{
				element.AvailableContextStepPairs = workflowDescriptor.GetFollowingContextSteps(GetPathUpToElement(element)).ToArray();
			}

			if (Refreshed != null)
			{
				Refreshed(this, EventArgs.Empty);
			}
		}

		public event EventHandler Refreshed;

		#endregion

		#region Collection overrides

		protected override bool AllowSort
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WorkflowEventContext { AvailableContextStepPairs = workflowDescriptor.GetFollowingContextSteps(GetPathUpToElement(null)).ToArray() };
		}

		#endregion

		#region Elements hooking

		protected override void OnAdded(BusinessObject bizo)
		{
			base.OnAdded(bizo);

			var eventContext = bizo as WorkflowEventContext;
			if (eventContext != null)
			{
				eventContext.HasChangesChanged -= ItemChangedHandler;
				eventContext.HasChangesChanged += ItemChangedHandler;
				RefreshAll();
			}
		}

		protected override void OnRemoved(BusinessObject bizo)
		{
			base.OnRemoved(bizo);

			var eventContext = bizo as WorkflowEventContext;
			if (eventContext != null)
			{
				eventContext.HasChangesChanged -= ItemChangedHandler;
				RefreshAll();
			}
		}

		void ItemChangedHandler(object sender, EventArgs e)
		{
			RefreshAll();
		}

		#endregion

		#region Context Path

		public IEnumerable<WorkflowEventContextPair> GetPathUpToElement(WorkflowEventContext element)
		{
			return Elements.Cast<WorkflowEventContext>().TakeWhile(item => item != element).Select(item => item.ToWorkflowEventContextPair());
		}

		#endregion
	}
}
