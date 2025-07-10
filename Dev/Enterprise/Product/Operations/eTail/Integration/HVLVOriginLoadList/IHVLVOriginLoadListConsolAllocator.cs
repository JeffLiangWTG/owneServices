using System;
using System.Collections.Generic;
using CargoWise.Types;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVOriginLoadListConsolAllocator
	{
		bool TryAcquireApplicationLocks(IList<ZGuid> pks, Action<IEnumerable<ZGuid>> actionForLockedRows, out string errorMessage);
		bool TryAttachToConsol(IForwardingConsol consol, IEnumerable<IHVLVOriginLoadList> loadLists, out string errorMessage);
		bool TryCreateConsolAndAttachLoadLists(IEnumerable<IHVLVOriginLoadList> loadLists, out string errorMessage);
		IForwardingConsol AllocatedConsol { get; }
	}
}
