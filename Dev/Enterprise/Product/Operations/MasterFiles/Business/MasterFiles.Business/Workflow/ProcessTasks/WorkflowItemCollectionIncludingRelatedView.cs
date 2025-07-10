using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public abstract class WorkflowItemCollectionIncludingRelatedView : WorkflowItemCollectionView
	{
		public WorkflowItemCollectionIncludingRelatedView(ProcessTaskCollection collection)
			: base(collection)
		{
			collection.AddView(this);
		}

		public override void Load()
		{
			using (SuppressRebuild())
			{
				using (SuspendListChanged())
				{
					CollectionToFilter.Load();
					ForceRebuild();
				}
			}
		}

		public void Reload()
		{
			using (Parent?.SuspendSettingHasChanges())
			{
				Load();
			}
		}
		internal ZBool IsRelatedItem(ProcessTask item)
		{
			return item.P9_ParentID != Parent.PK;
		}

		public bool HasRelatedItems => true;

		#region Sorting

		protected override IComparer OverrideBaseComparer(IComparer baseComparer) => new RelatedItemComparer(this, baseComparer);

		protected class RelatedItemComparer : IComparer<ProcessTask>, IComparer
		{
			public RelatedItemComparer(WorkflowItemCollectionIncludingRelatedView view, IComparer baseComparer)
			{
				this.view = view;
				this.baseComparer = baseComparer;
			}

			protected readonly WorkflowItemCollectionIncludingRelatedView view;
			readonly IComparer baseComparer;
			IDictionary<ZGuid, int> relatedEventBizos;

			int IComparer.Compare(object x, object y) => Compare((ProcessTask)x, (ProcessTask)y);

			public virtual int Compare(ProcessTask x, ProcessTask y)
			{
				int result = GetRelatedBusinessObjectOrder(x).CompareTo(GetRelatedBusinessObjectOrder(y));
				if (result == 0)
				{
					result = baseComparer.Compare(x, y);
				}
				return result;
			}

			IDictionary<ZGuid, int> BusinessObjectsWithRelatedEvents
			{
				get { return relatedEventBizos ?? (relatedEventBizos = GetBusinessObjecsWithRelatedEventsOrder()); }
			}

			IDictionary<ZGuid, int> GetBusinessObjecsWithRelatedEventsOrder()
			{
				return (((IStmALogParent)view.Parent).BusinessObjectsWithRelatedEvents ?? Enumerable.Empty<BusinessObject>())
					.Where(t => t != null)
					.Select((b, i) => new { Bizo = b.PK, Index = i })
					.DistinctBy(t => t.Bizo)
					.ToImmutableDictionary(t => t.Bizo, t => t.Index);
			}

			int GetRelatedBusinessObjectOrder(ProcessTask item)
			{
				if (view.IsRelatedItem(item) && BusinessObjectsWithRelatedEvents.TryGetValue(item.P9_ParentID, out int index))
				{
					return index;
				}
				else
				{
					return int.MaxValue;
				}
			}
		}

		#endregion

		#region Remove / Delete

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			ProcessTask task = (ProcessTask)elementToDelete;
			bool isRelatedTask = task.Parent != WorkflowItems.Parent;
			if (isRelatedTask)
			{
				throw new CannotDeleteException("You cannot delete a related item. Open '" + (task.Parent == null ? (ZString)"Related Item" : ((BusinessObject)task.Parent).HumanReadableName) + "' and navigate to the 'Tracking' tab to delete this item.");
			}
			else
			{
				base.RemoveAndDelete(elementToDelete);
			}
		}

#pragma warning disable 0809
		[Obsolete("This method would cause related items to be deleted", true)]
		public sealed override void RemoveAndDeleteAll()
		{
			throw new NotSupportedException("This method would cause related items to be deleted");
		}
#pragma warning restore 0809

		#endregion

		#region Synchronizing with underlying collection

		protected override void OnAdded(BusinessObject bizo)
		{
			var task = (ProcessTask)bizo;
			if (!IsRelatedItem(task))
			{
				WorkflowItems.Add(task);
				base.OnAdded(task);
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			if (WorkflowItems != null && WorkflowItems.Contains(bizO))
			{
				WorkflowItems.Remove(bizO);
				base.OnRemoved(bizO);
			}
		}

		#endregion

		#region IWorkflowProvider

		protected override ProcessTaskCollection WorkflowItems => ((IWorkflowProvider)CollectionToFilter)?.WorkflowItems;

		#endregion

		#region Implementation

		protected class SupersetRebuilder : WorkflowItemCollectionViewRebuilder
		{
			public SupersetRebuilder(WorkflowItemCollectionIncludingRelatedView view)
				: base(view)
			{
				this.view = view;
			}
			readonly WorkflowItemCollectionIncludingRelatedView view;
			HashSet<ProcessTask> tasks;

			HashSet<ProcessTask> GetRelatedTasks()
			{
				if (tasks == null)
				{
					tasks = new HashSet<ProcessTask>(GetRelatedTasksCore(), new TaskEqualityComparer());
				}

				return tasks;
			}

			protected virtual IEnumerable<ProcessTask> GetRelatedTasksCore() => view.GetRelatedProcessTasks();

			protected sealed override IEnumerable<BusinessObject> GetElementsForRebuild()
			{
				return base.GetElementsForRebuild().Concat(GetRelatedTasks());
			}

			protected sealed override bool IsInFilteredCollection(BusinessObject element)
			{
				return base.IsInFilteredCollection(element) || GetRelatedTasks().Contains((ProcessTask)element);
			}

			class TaskEqualityComparer : IEqualityComparer<ProcessTask>
			{
				public bool Equals(ProcessTask x, ProcessTask y) => x.PK == y.PK;
				public int GetHashCode(ProcessTask obj) => obj.PK.GetHashCode();
			}
		}

		protected override Rebuilder GetRebuilder() => new SupersetRebuilder(this);

		IEnumerable<ProcessTask> GetRelatedProcessTasks()
		{
			var providers = GetRelatedWorkflowProviders().ToArray();
			if (providers.Length <= WorkflowDataRegistry.Instance.RelatedWorkflowItemDisplayLimit.Value)
			{
				isShowingRelatedItems = true;
				return GetRelatedProcessTasksCore(providers);
			}
			else
			{
				isShowingRelatedItems = false;
				return Enumerable.Empty<ProcessTask>();
			}
		}

		protected virtual IEnumerable<ProcessTask> GetRelatedProcessTasksCore(IWorkflowProvider[] providers)
		{
			foreach (var provider in providers.Where(t => !ProcessTaskCollection.IsParentLoaded(Factory, t.PK)))
			{
				Factory.AddFetchHint(ProcessTasksSchema.Instance, new ZQuery(ProcessTasksSchema.P9_ParentID, provider.PK));
			}

			return providers.SelectMany(x => x.WorkflowItems).Cast<ProcessTask>().Where(p => !p.IsDeleted && IsTypeMatch(p));
		}

		bool? isShowingRelatedItems;

		public bool CanShowRelatedItems()
		{
			if (!isShowingRelatedItems.HasValue)
			{
				Load();
			}

			return isShowingRelatedItems ?? throw new InvalidOperationException("Loading failed to initialize value.");
		}

		protected IEnumerable<IWorkflowProvider> GetRelatedWorkflowProviders()
		{
			var workflowProviderWithRelated = Parent as IWorkflowProviderIncludingRelated;

			return workflowProviderWithRelated != null
				? workflowProviderWithRelated.RelatedIWorkflowProviders
				: ((IStmALogParent)Parent).BusinessObjectsWithRelatedEvents.OfType<IWorkflowProvider>();
		}

		protected new ProcessTaskCollection CollectionToFilter
		{
			get { return (ProcessTaskCollection)base.CollectionToFilter; }
		}

		protected BusinessObject Parent
		{
			get { return CollectionToFilter.Parent; }
		}

		#endregion
	}
}
