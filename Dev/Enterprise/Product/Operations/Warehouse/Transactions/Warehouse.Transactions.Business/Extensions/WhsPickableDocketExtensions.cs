using System;

namespace Enterprise.Warehouse.Transactions.Business
{
	static class WhsPickableDocketExtensions
	{
		internal static void FireReleaseLineAddedOrRemoved(this WhsPickableDocket order, WhsReleaseLine releaseLine, bool releaseLineWasAdded)
		{
			(order as WhsOrder)?.FireReleaseLineAddedOrRemoved(releaseLine, releaseLineWasAdded);
		}

		internal static IDisposable SuspendPackableItemParentsCountChanged(this WhsPickableDocket order)
		{
			return (order as WhsOrder)?.SuspendPackableItemParentsCountChanged();
		}

		internal static void RegisterOrderLineWithClearedReleaseLines(this WhsPickableDocket order, WhsPickableDocketLine orderLine)
		{
			(order as WhsOrder)?.RegisterOrderLineWithClearedReleaseLines(orderLine);
		}
	}
}
