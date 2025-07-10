using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobSlotAllocationValidationTest : BusinessObjectValidationTestCase
	{
		#region NoNotifications by default

		public void TestNoNotifications()
		{
			AssertNoNotifications("No Notifications on default VoyageAllocation", Allocation);
		}

		#endregion

		#region Implementation

		SlotAllocation Allocation;

		protected override void SetUp()
		{
			base.SetUp();

			Allocation = Factory.New<SlotAllocation>();
		}

		#endregion
	}
}
