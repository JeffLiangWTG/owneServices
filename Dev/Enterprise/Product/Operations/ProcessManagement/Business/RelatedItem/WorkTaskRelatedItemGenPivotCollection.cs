using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ProcessManagement.Business
{
	/// <summary>
	/// Collection of related items for Work Items, Incidents, Projects etc
	/// where the related items are of a single type.
	/// </summary>
	public class WorkTaskRelatedItemGenPivotCollection<T> : WorkTaskRelatedItemGenPivotCollection
		where T : BusinessObject, IWorkTaskRelatedItem
	{
		public WorkTaskRelatedItemGenPivotCollection(IWorkTaskRelatedItemSource master, RelatedLinkType relatedLinkType = RelatedLinkType.TwoWay)
			: base(master, relatedLinkType)
		{
		}

		public new T this[int index] => (T)base[index];

		public new T AddNew()
		{
			return (T)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(T);
		}

		protected override bool ShouldAddToCollection(BusinessObject relatedItem)
		{
			return base.ShouldAddToCollection(relatedItem) && relatedItem is T;
		}
	}

	/// <summary>
	/// Collection of IWorkTaskRelatedItem objects which are related items for Work Items, Incidents, Projects etc.
	/// Used in Related Items tab of the relevant form.
	/// </summary>
	public class WorkTaskRelatedItemGenPivotCollection : WorkTaskRelatedItemCollection
	{
		public WorkTaskRelatedItemGenPivotCollection(IWorkTaskRelatedItemSource master, RelatedLinkType relatedLinkType = RelatedLinkType.TwoWay)
			: base(master)
		{
			this.relatedLinkType = relatedLinkType;
		}

		readonly RelatedLinkType relatedLinkType;

		protected override IPivotBusinessObjectCollection[] GetNewPivotCollections(BusinessObject master)
		{
			var includeChildren = relatedLinkType != RelatedLinkType.MasterAlwaysChild;
			var includeParents = relatedLinkType != RelatedLinkType.MasterAlwaysParent;

			return new IPivotBusinessObjectCollection[] { new GenPivotCollection(master, Core.Constants.GenPivotTypes.ProcessManagement, includeChildren, includeParents) };
		}
	}
}
