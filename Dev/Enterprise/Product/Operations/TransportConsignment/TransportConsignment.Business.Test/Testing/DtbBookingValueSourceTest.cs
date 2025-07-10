using Enterprise.TransportCommon.Business.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbBookingValueSourceTest : DomesticValueSourceTest<DtbBookingConsignment>
	{
		#region Implementation

		protected override DtbBookingConsignment GetNewTransport()
		{
			return Factory.NewWithValidTestData<DtbBookingConsignment>();
		}

		#endregion
	}
}
