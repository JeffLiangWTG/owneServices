using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	internal class AllocationUsageSetTest : BaseAgencyTest
	{
		public void TestCanFit()
		{
			SlotAllocation allocation = Factory.New<SlotAllocation>();
			allocation.SetAspect(AllocationAspectTypes.Tonnes, 20);
			allocation.SetAspect(AllocationAspectTypes.Volume, 20);
			allocation.SetAspect(AllocationAspectTypes.PowerPoints, 4);
			allocation.SetAspect(AllocationAspectTypes.TEU, 10);
			allocation.SetAspect(AllocationAspectTypes.Area, 12);
			AllocationUsageSet set = new AllocationUsageSet(new AllocationUsage(3, 4, 5, 3, 10, 4, 3), allocation);
			allocation.E0_UseDefaultOverAllocation = false;
			allocation.E0_OverAllocationPercent = 0m;
			AssertEquals("Can fit", true, set.CanFit(new AllocationUsage(2, 3, 1, 10, 16, 9)));
			AssertEquals("Can fit", true, set.CanFit(new AllocationUsage(3, 2, 1, 10, 16, 9)));
			AssertEquals("Cant fit", false, set.CanFit(new AllocationUsage(3, 3, 1, 10, 16, 9)));
			AssertEquals("Cant fit", false, set.CanFit(new AllocationUsage(2, 3, 2, 10, 16, 9)));
			AssertEquals("Cant fit", false, set.CanFit(new AllocationUsage(2, 3, 1, 11, 16, 9)));
			AssertEquals("Cant fit", false, set.CanFit(new AllocationUsage(2, 3, 1, 10, 17, 9)));
			AssertEquals("Cant fit", false, set.CanFit(new AllocationUsage(2, 3, 1, 10, 16, 10)));
			allocation.E0_OverAllocationPercent = 100m;
			AssertEquals("Can fit", true, set.CanFit(new AllocationUsage(7, 8, 5, 30, 36, 21)));
			AssertEquals("Can fit", true, set.CanFit(new AllocationUsage(8, 7, 5, 30, 36, 21)));
			AssertEquals("Cant fit", false, set.CanFit(new AllocationUsage(8, 8, 5, 30, 36, 21)));
			AssertEquals("Cant fit", false, set.CanFit(new AllocationUsage(7, 8, 6, 30, 36, 21)));
			AssertEquals("Cant fit", false, set.CanFit(new AllocationUsage(7, 8, 5, 31, 36, 21)));
			AssertEquals("Cant fit", false, set.CanFit(new AllocationUsage(7, 8, 5, 30, 37, 21)));
			AssertEquals("Cant fit", false, set.CanFit(new AllocationUsage(7, 8, 5, 30, 36, 22)));
		}
	}
}
