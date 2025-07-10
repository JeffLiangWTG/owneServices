using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobSupplierBookingLineCollection))]
	sealed class JobSupplierBookingLineCollectionBOTest : ActiveBusinessObjectCollectionTestCase<JobSupplierBookingLineCollection>
	{
		#region Implementation

		protected override JobSupplierBookingLineCollection GetCollectionToTest()
		{
			var supplierBooking = Factory.New<JobSupplierBooking>();
			return new JobSupplierBookingLineCollection(supplierBooking);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<JobSupplierBookingLine>();
		}

		#endregion
	}
}
