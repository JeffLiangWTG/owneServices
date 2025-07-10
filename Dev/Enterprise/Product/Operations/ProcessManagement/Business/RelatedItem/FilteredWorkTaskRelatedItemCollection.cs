using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.ProcessManagement.Business
{
	public class FilteredWorkTaskRelatedItemCollection : BusinessObjectCollectionView<BusinessObject>
	{
		public FilteredWorkTaskRelatedItemCollection(WorkTaskRelatedItemCollection collectionToFilter)
			: base(collectionToFilter)
		{
			includeAllItems = true;
			Rebuild();
		}

		public new IWorkTaskRelatedItem this[int index]
		{
			get { return (IWorkTaskRelatedItem)Elements[index]; }
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			IWorkTaskRelatedItem item = (IWorkTaskRelatedItem)element;
			return (!item.IsClosedOrCancelled || IncludeAllItems);
		}

		#region IncludeAllItems

		public bool IncludeAllItems
		{
			get { return includeAllItems; }
			set
			{
				if (value != includeAllItems)
				{
					includeAllItems = value;
					Rebuild();
				}
			}
		}

		bool includeAllItems;

		#endregion
	}
}
