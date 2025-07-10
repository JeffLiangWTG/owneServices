using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public abstract class FilteredCollection<TBusinessObject> : SubsetBusinessObjectCollection<TBusinessObject>
		where TBusinessObject : BusinessObject
	{
		protected FilteredCollection(BusinessObjectCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		public void ClearFilter()
		{
			ClearFilterCore();
		}

		#region Implementation

		public override bool ReadOnly
		{
			get
			{
				return CollectionToFilter.ReadOnly;
			}
		}

		public override void RemoveAndDelete(BusinessObject bObject)
		{
			if (!IsNonCommittedCollectionElement(bObject))
			{
				CollectionToFilter.RemoveAndDelete(bObject);
			}
			else
			{
				base.RemoveAndDelete(bObject);
			}
		}

		public override void SetupNewElementButDoNotAddIt(BusinessObject bObject, bool setupCollectionRelationships)
		{
			CollectionToFilter.SetupNewElementButDoNotAddIt(bObject, setupCollectionRelationships);
		}

		protected override bool AllowNewCore
		{
			get
			{
				return CollectionToFilter.AllowNew;
			}
		}

		protected override void OnAdded(BusinessObject bObject)
		{
			if (!IsRebuilding && !CollectionToFilter.Contains(bObject))
			{
				CollectionToFilter.Add(bObject);
			}
			base.OnAdded(bObject);
		}

		protected override void RebuildOnConstruction()
		{
			// don't rebuild, filter is not set at this moment
		}

		protected override void SetDefaultsForNewChild(BusinessObject bObject)
		{
			CollectionToFilter.SetupNewElementButDoNotAddIt(bObject, true);
		}

		protected abstract bool IsFilterEmpty
		{
			get;
		}

		protected abstract void ClearFilterCore();

		#endregion // Implementation
	}
}
