using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ProcessManagement.Business
{
	public enum RelatedLinkType
	{
		MasterAlwaysChild,
		MasterAlwaysParent,
		TwoWay
	}

	public class RelatedItemEventArgs : EventArgs
	{
		public RelatedItemEventArgs(BusinessObject businessObject, bool isParent)
		{
			BusinessObject = businessObject;
			IsParent = isParent;
		}

		public BusinessObject BusinessObject { get; }

		public bool IsParent { get; }
	}

	/// <summary>
	/// Collection of IWorkTaskRelatedItem objects which are related items for Work Items, Incidents, Projects etc.
	/// Used in Related Items tab of the relevant form.
	/// </summary>
	public abstract class WorkTaskRelatedItemCollection : BusinessObjectCollection<BusinessObject>
	{
		protected WorkTaskRelatedItemCollection(IWorkTaskRelatedItemSource master)
			: base(master.Factory)
		{
			Master = master;

			workflowProvidersLinkageService = ObjectFactory.Get<IWorkflowProvidersLinkageService>();
		}

		readonly IWorkflowProvidersLinkageService workflowProvidersLinkageService;

		public new IWorkTaskRelatedItem this[int index] => (IWorkTaskRelatedItem)Elements[index];

		public IEnumerable<T> GetElements<T>() where T : class, IWorkTaskRelatedItem
		{
			return GetElements<T>(null);
		}

		public IEnumerable<T> GetElements<T>(Predicate<T> additionalFilter) where T : class, IWorkTaskRelatedItem
		{
			foreach (var element in this.Cast<IWorkTaskRelatedItem>())
			{
				var typedElement = element as T;

				if (typedElement != null && (additionalFilter == null || additionalFilter(typedElement)))
				{
					yield return (T)element;
				}
			}
		}

		public override void Add(BusinessObject businessObject)
		{
			if (ShouldAddToCollection(businessObject))
			{
				base.Add(businessObject);
			}
		}

		public sealed override void Load()
		{
			try
			{
				isLoading = true;
				LoadCore();
			}
			finally
			{
				isLoading = false;
				IsLoaded = true;
			}
		}

		bool isLoading;

		protected virtual void LoadCore()
		{
			RemoveAll();

			foreach (var pivot in PivotCollections.SelectMany(x => x))
			{
				var relatedItem = pivot.Relation1ID == ((BusinessObject)Master).PK ? pivot.Relation2Object : pivot.Relation1Object;

				if (relatedItem != null && ShouldAddToCollection(relatedItem) && !Contains(relatedItem.PK))
				{
					Add(relatedItem);
				}
			}
		}

		protected virtual bool ShouldAddToCollection(BusinessObject relatedItem)
		{
			return relatedItem is IWorkTaskRelatedItem;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (!isLoading)
			{
				var addAsParent = false;
				using (SuspendPivotCollectionCountChangedEventHandler())
				{
					var collection = GetCollectionForRelatedItem((IWorkTaskRelatedItem)bizOAdded);
					if (bizOAdded is IWorkTaskRelatedItemSource source)
					{
						addAsParent = source.ShouldAddRelatedItemAsParent;
					}
					collection.AddRelatedIfNotExist(bizOAdded, addAsParent);
				}

				RelatedItemAdded?.Invoke(this, new RelatedItemEventArgs(bizOAdded, addAsParent));
				AddLogs(bizOAdded, AutoEvents.Attached, (NoResString)"{0} attached to {1}"); // Logging message shouldn't be translated.

				if (Master is IWorkflowProvider sourceWorkflowProvider && bizOAdded is IWorkflowProvider newlyConnectedWorkflowProvider)
				{
					workflowProvidersLinkageService.WorkflowProvidersLinked(sourceWorkflowProvider, newlyConnectedWorkflowProvider, Factory);
				}
			}
		}

		protected virtual IPivotBusinessObjectCollection GetCollectionForRelatedItem(IWorkTaskRelatedItem item)
		{
			if (PivotCollections.Count() == 1)
			{
				return PivotCollections.Single();
			}

			var collectionType = item.PivotCollectionType;
			var collectionsOfCorrectType = PivotCollections.Where(x => x.GetType() == collectionType).ToArray();

			if (collectionsOfCorrectType.Length != 1)
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Could not determine which collection to use for the related object, because a single collection of the appropriate type doesn't exist. Type: {0}, collections: {1}", collectionType.Name, string.Join(", ", PivotCollections.Select(x => x.GetType().Name))));
				return null;
			}

			return collectionsOfCorrectType.Single();
		}

		protected override void OnRemoved(BusinessObject item)
		{
			base.OnRemoved(item);

			if (!isLoading)
			{
				var isParent = false;
				using (SuspendPivotCollectionCountChangedEventHandler())
				{
					var collection = GetCollectionForRelatedItem((IWorkTaskRelatedItem)item);
					var pivot = collection.FindRelated(item.PK);
					if (pivot != null)
					{
						isParent = pivot.Relation1ID == item.PK;
						pivot.Delete();
					}
				}

				RelatedItemRemoved?.Invoke(this, new RelatedItemEventArgs(item, isParent));
				AddLogs(item, AutoEvents.Detached, (NoResString)"{0} detached from {1}"); // Logging message shouldn't be translated.

				if (Master is IWorkflowProvider sourceWorkflowProvider && item is IWorkflowProvider disconnectedWorkflowProvider)
				{
					workflowProvidersLinkageService.WorkflowProvidersUnLinked(sourceWorkflowProvider, disconnectedWorkflowProvider, Factory);
				}
			}
		}

		void AddLogs(BusinessObject itemAddedOrRemoved, Event eventType, string logFormat)
		{
			if (!itemAddedOrRemoved.IsDeleted)
			{
				var log = string.Format(CultureInfo.InvariantCulture, logFormat, ((IWorkTaskRelatedItem)itemAddedOrRemoved).Number, ((IWorkTaskRelatedItem)Master).Number);
				((EnterpriseBusinessObject)Master).Logs.AddNew(eventType, log);
				((EnterpriseBusinessObject)itemAddedOrRemoved).Logs.AddNew(eventType, log);
			}
		}

		IDisposable SuspendPivotCollectionCountChangedEventHandler()
		{
			shouldHandlePivotCollectionCountChanged = false;
			return new DisposableAction(delegate
			{ shouldHandlePivotCollectionCountChanged = true; });
		}

		bool shouldHandlePivotCollectionCountChanged = true;

		public event EventHandler<RelatedItemEventArgs> RelatedItemAdded;
		public event EventHandler<RelatedItemEventArgs> RelatedItemRemoved;

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			Type result = typeof(IWorkTaskRelatedItem);
			return result;
		}

		protected override bool AllowNewCore => false;

		public IWorkTaskRelatedItemSource Master { get; }

		void PivotCollection_CountChanged(object sender, EventArgs e)
		{
			if (shouldHandlePivotCollectionCountChanged)
			{
				Load();
			}
		}

		IEnumerable<IPivotBusinessObjectCollection> PivotCollections
		{
			get
			{
				if (pivotCollections == null)
				{
					var master = (BusinessObject)Master;
					pivotCollections = GetNewPivotCollections(master);

					foreach (var collection in pivotCollections)
					{
						collection.CountChanged += PivotCollection_CountChanged;
						master.RegisterEditableChildObject(collection);
					}
				}

				return pivotCollections;
			}
		}

		IPivotBusinessObjectCollection[] pivotCollections;

		protected abstract IPivotBusinessObjectCollection[] GetNewPivotCollections(BusinessObject master);

#pragma warning disable 0809
		[Obsolete("You cannot call Load(ZQuery) on this collection as it is custom loaded", true)]
		public override void Load(ZQuery additionalFilter)
		{
		}
#pragma warning restore 0809
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "persistent name not to be translated")]
	public static class WorkTaskRelatedItemTypes
	{
		public const string CustomerServiceTicket = "Customer Service Ticket";
		public const string Project = "Project";
		public const string WorkItem = "Work Item";
		public const string Opportunity = "Opportunity";
	}
}
