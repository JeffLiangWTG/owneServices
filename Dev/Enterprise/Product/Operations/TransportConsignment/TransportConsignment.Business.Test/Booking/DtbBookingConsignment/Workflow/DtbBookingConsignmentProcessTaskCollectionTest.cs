using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbBookingConsignmentProcessTaskCollection))]
	class DtbBookingConsignmentProcessTaskCollectionTest : ProcessTaskCollectionTest<DtbBookingConsignmentProcessTaskCollection>
	{
		#region Implementation

		protected override DtbBookingConsignmentProcessTaskCollection GetCollectionToTestCore()
		{
			return new DtbBookingConsignmentProcessTaskCollection(Helper.CreateBookingConsignment());
		}

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
