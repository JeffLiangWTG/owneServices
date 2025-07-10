using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class AllocationUsageSet
	{
		public AllocationUsageSet(AllocationUsage usage, SlotAllocation allocation)
		{
			this.Used = usage;
			this.Allocation = allocation;
		}

		public bool CanFit(AllocationUsage usage)
		{
			return Used.TEU + usage.TEU <= Allocation.GetAspectOverallocation(AllocationAspectTypes.TEU)
				&& Used.PowerPoints + usage.PowerPoints <= Allocation.GetAspectOverallocation(AllocationAspectTypes.PowerPoints)
				&& Used.Tonnes + usage.Tonnes <= Allocation.GetAspectOverallocation(AllocationAspectTypes.Tonnes)
				&& Used.Volume + usage.Volume <= Allocation.GetAspectOverallocation(AllocationAspectTypes.Volume)
				&& Used.Area + usage.Area <= Allocation.GetAspectOverallocation(AllocationAspectTypes.Area);
		}

		public readonly AllocationUsage Used;
		public readonly SlotAllocation Allocation;
	}
}
