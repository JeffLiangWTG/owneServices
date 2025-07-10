using Enterprise.TransportBookings.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbBookingConsignment))]
	sealed class DtbConsignmentPackageRelationshipTest : TransportPackageRelationshipTest<DtbBookingConsignment>
	{
		#region Implementation

		protected override DtbBookingConsignment GetNewTransportBizO()
		{
			return Helper.CreateBookingConsignment();
		}

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}
		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
