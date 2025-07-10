using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportBookings.Shared.Testing
{
	public abstract class DtbBookingSharedTestCaseWithFactory : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBookingShared);
		}

		protected TransportBookingSharedTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingSharedTestHelper(Factory)); }
		}

		TransportBookingSharedTestHelper helper;
	}
}
