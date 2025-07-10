using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobSupplierBookingLineProcessTaskCollection))]
	sealed class JobSupplierBookingLineProcessTaskCollectionTest
		: ProcessTaskCollectionTest<JobSupplierBookingLineProcessTaskCollection>
	{
		#region Implementation

		protected override JobSupplierBookingLineProcessTaskCollection GetCollectionToTestCore()
		{
			return new JobSupplierBookingLineProcessTaskCollection(Factory.NewWithValidTestData<JobSupplierBookingLine>());
		}

		#endregion
	}
}
