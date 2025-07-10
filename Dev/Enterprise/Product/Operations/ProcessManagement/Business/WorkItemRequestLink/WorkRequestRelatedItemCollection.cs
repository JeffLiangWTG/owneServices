using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkRequestRelatedItemCollection : WorkTaskRelatedItemCollection
	{
		public WorkRequestRelatedItemCollection(IWorkTaskRelatedItemSource master)
			: base(master)
		{
		}

		protected override IPivotBusinessObjectCollection[] GetNewPivotCollections(BusinessObject master)
		{
			return new IPivotBusinessObjectCollection[] { new WorkItemRequestLinkCollection(master) };
		}

		public new WorkItem this[int index] => (WorkItem)base[index];

		public new WorkItem AddNew()
		{
			return (WorkItem)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(WorkItem);
		}
	}
}
