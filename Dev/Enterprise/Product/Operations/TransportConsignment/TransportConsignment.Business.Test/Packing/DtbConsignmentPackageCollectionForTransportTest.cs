using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(PackageCollectionForTransport))]
	sealed class DtbConsignmentPackageCollectionForTransportTest : PackageCollectionForTransportTest<DtbBookingConsignment>
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
