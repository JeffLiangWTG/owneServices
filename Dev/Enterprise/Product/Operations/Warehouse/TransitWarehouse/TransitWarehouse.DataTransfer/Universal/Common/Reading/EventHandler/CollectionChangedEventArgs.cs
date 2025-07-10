using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;

public class CollectionChangedEventArgs : EventArgs
{
	public enum CollectionType
	{
		DetachedLoadList,
		RemovedPackageState,
		ReassignedPackageState,
	}

	public IEnumerable<ZGuid> DetachedLoadListPKs { get; }

	public IEnumerable<WhsItemPackageState> RemovedPackageStates { get; }

	public IEnumerable<ZGuid> ReassignedPackageStatePKs { get; }

	public CollectionType type { get; }

	public CollectionChangedEventArgs(IEnumerable<ZGuid> guids, CollectionType collectionType)
	{
		if (collectionType == CollectionType.DetachedLoadList)
		{
			DetachedLoadListPKs = guids;
		}
		else
		{
			ReassignedPackageStatePKs = guids;
		}
		type = collectionType;
	}

	public CollectionChangedEventArgs(IEnumerable<WhsItemPackageState> removedPackageStates)
	{
		RemovedPackageStates = removedPackageStates;
		type = CollectionType.RemovedPackageState;
	}
}

