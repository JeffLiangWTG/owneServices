namespace Enterprise.eManifest.Integration.Testing
{
	using CargoWise.EntityFramework;
	using NUnit.Framework;

	class CreatingSupplierBookingLineBizOsPreventerTest : TransactionedTestCase
	{
		#region TestIsCreatingHVLVTransportBookings

		public void TestIsCreatingHVLVTransportBookings()
		{
			using (CreatingSupplierBookingLineBizOsPreventer.SuspendCreatingSupplierBookingLineBizOs(Factory))
			{
				using (CreatingSupplierBookingLineBizOsPreventer.SuspendCreatingSupplierBookingLineBizOs(Factory))
				{
					AssertEquals(true, CreatingSupplierBookingLineBizOsPreventer.IsCreatingSupplierBookingLineBizOsSuspended(Factory));
					AssertEquals(false, CreatingSupplierBookingLineBizOsPreventer.IsCreatingSupplierBookingLineBizOsSuspended(new BusinessObjectFactory()));
				}

				AssertEquals(true, CreatingSupplierBookingLineBizOsPreventer.IsCreatingSupplierBookingLineBizOsSuspended(Factory));
			}

			AssertEquals(false, CreatingSupplierBookingLineBizOsPreventer.IsCreatingSupplierBookingLineBizOsSuspended(Factory));
		}

		#endregion

		#region Implementation

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		#endregion
	}
}
