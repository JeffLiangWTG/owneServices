using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobSlotAllocationAspectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestNoRangeValidation()
		{
			SlotAllocation allocation = Factory.New<SlotAllocation>();
			SlotAllocationAspect aspect = allocation.Aspects.AddNew();

			aspect.D5_Value = 1000000;
			AssertNoNotifications("adding range validation here would duplicate the notifications shown by the wrapper object.", aspect.D5_ValueInfo);
		}
	}
}
