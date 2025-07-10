using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkItemRelatedItemCollection : WorkTaskRelatedItemCollection
	{
		public WorkItemRelatedItemCollection(IWorkTaskRelatedItemSource master, RelatedLinkType relatedLinkType = RelatedLinkType.TwoWay)
			: base(master)
		{
			this.relatedLinkType = relatedLinkType;
		}

		readonly RelatedLinkType relatedLinkType;

		protected override bool ShouldAddToCollection(BusinessObject relatedItem)
		{
			var result = base.ShouldAddToCollection(relatedItem);
			result &= relatedItem is IWorkItemRelatedItem;
			return result;
		}

		protected override IPivotBusinessObjectCollection[] GetNewPivotCollections(BusinessObject master)
		{
			var includeChildren = relatedLinkType != RelatedLinkType.MasterAlwaysChild;
			var includeParents = relatedLinkType != RelatedLinkType.MasterAlwaysParent;

			var genPivotCollection = new GenPivotCollection(master, Constants.GenPivotTypes.ProcessManagement, includeChildren, includeParents);
			var linksToCustomerServiceTicketsCollection = new WorkItemRequestLinkCollection(master);

			return new IPivotBusinessObjectCollection[] { genPivotCollection, linksToCustomerServiceTicketsCollection };
		}
	}
}
