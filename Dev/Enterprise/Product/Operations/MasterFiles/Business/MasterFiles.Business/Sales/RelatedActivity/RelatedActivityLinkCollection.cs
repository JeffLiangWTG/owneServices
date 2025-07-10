using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class RelatedActivityLinkCollection : NonPersistentBusinessObjectCollection<RelatedActivityLink>
	{
		public RelatedActivityLinkCollection(IRelatableActivity fromActivity)
			: base(fromActivity.Factory)
		{
			FromActivity = fromActivity;

			RefreshItemsFromWrappedCollection();
			fromActivity.RelatedChildActivityPivotCollection.CountChanged += wrappedCollection_CountChanged;
			fromActivity.RelatedParentActivityPivotCollection.CountChanged += wrappedCollection_CountChanged;
		}

		void wrappedCollection_CountChanged(object sender, System.EventArgs e)
		{
			if (!SuspendWrappedCountChangeHandler)
			{
				RefreshItemsFromWrappedCollection();
			}
		}

		void RefreshItemsFromWrappedCollection()
		{
			var originalLinksLookupByPivotPk = new Dictionary<ZGuid, RelatedActivityLink>();
			var linksToRemoveLookupByPivotPk = new Dictionary<ZGuid, RelatedActivityLink>();
			var pivotsToAddLookup = new Dictionary<ZGuid, IRelatedActivityPivot>();
			foreach (RelatedActivityLink link in this)
			{
				var pivot = link.Pivot;
				originalLinksLookupByPivotPk.Add(pivot != null ? pivot.PK : ZGuid.Invalid, link);
				linksToRemoveLookupByPivotPk.Add(pivot != null ? pivot.PK : ZGuid.Invalid, link);
			}

			foreach (var pivot in FromActivity.RelatedChildActivityPivotCollection)
			{
				if (originalLinksLookupByPivotPk.ContainsKey(pivot.PK))
				{
					linksToRemoveLookupByPivotPk.Remove(pivot.PK);
				}
				else if (!pivotsToAddLookup.ContainsKey(pivot.PK))
				{
					pivotsToAddLookup.Add(pivot.PK, pivot);
				}
			}

			foreach (var pivot in FromActivity.RelatedParentActivityPivotCollection)
			{
				if (originalLinksLookupByPivotPk.ContainsKey(pivot.PK))
				{
					linksToRemoveLookupByPivotPk.Remove(pivot.PK);
				}
				else if (!pivotsToAddLookup.ContainsKey(pivot.PK))
				{
					pivotsToAddLookup.Add(pivot.PK, pivot);
				}
			}

			foreach (var linkToRemove in linksToRemoveLookupByPivotPk.Select(x => x.Value))
			{
				if (Contains(linkToRemove))
				{
					Remove(linkToRemove);
				}
			}

			foreach (var pivotToAddLink in pivotsToAddLookup)
			{
				Add(RelatedActivityLink.Get(pivotToAddLink.Value, FromActivity));
			}
		}

		public readonly IRelatableActivity FromActivity;

		#region Implementation

		bool SuspendWrappedCountChangeHandler;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			SuspendWrappedCountChangeHandler = true;

			var pivot = FromActivity.RelatedParentActivityPivotCollection.AddNew();

			SuspendWrappedCountChangeHandler = false;

			return RelatedActivityLink.Get(pivot, FromActivity);
		}

		#endregion
	}
}
