using Enterprise.Packing.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbCarrierBookingConsignment))]
	class DtbCarrierBookingConsignmentIPackingParentTest : PackingParentTestCase<DtbCarrierBookingConsignment>
	{
		protected override DtbCarrierBookingConsignment GetNewParent()
		{
			return Helper.CreateConsignment();
		}

		CarrierBookingTestHelper Helper
		{
			get { return helper ?? (helper = new CarrierBookingTestHelper(Factory)); }
		}

		CarrierBookingTestHelper helper;
	}
}

