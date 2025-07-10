
using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Accounting.Helpers;

namespace Enterprise.MasterFiles.Business
{
	public class BusinessObjectCollectionSnapshotManager
	{
		readonly IBusinessObjectCollection collection;
		readonly BusinessObjectCollectionCopier copier;
		IComparable[][] lastSnapshot;
		public BusinessObjectCollectionSnapshotManager(BusinessObjectCollectionCopier copier, IBusinessObjectCollection collection)
		{
			this.copier = copier;
			this.collection = collection;
		}

		public void TakeSnapshot()
		{
			lastSnapshot = GetCurrentSnapshot();
		}

		public IComparable[][] GetLastSnapshot()
		{
			return lastSnapshot;
		}

		public IComparable[][] GetCurrentSnapshot()
		{
			return copier.ConvertToNestedList(collection);
		}

		public bool HasChanged
		{
			get
			{
				return !BusinessObjectCollectionCopier.AreEqual(lastSnapshot, GetCurrentSnapshot());
			}
		}

		public void ImportSnapshot(IComparable[][] snapshot)
		{
			copier.CopyTo(snapshot, collection);
		}

		public IBusinessObjectCollection Collection
		{
			get
			{
				return collection;
			}
		}
	}
}
