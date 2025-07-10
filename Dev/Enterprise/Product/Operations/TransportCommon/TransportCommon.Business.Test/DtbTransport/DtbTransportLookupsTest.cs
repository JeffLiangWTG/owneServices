using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestBookingTemplates

		public void TestBookingTemplates()
		{
			var transport = GetNewTransport();
			AssertContainsExactElementsInAnyOrder(GetExpectedTransportTmplCollection(), transport.Lookups.BookingTemplates);
		}

		protected abstract DtbTransportTmplCollection GetExpectedTransportTmplCollection();

		#endregion

		#region Implementation

		protected abstract DtbTransport GetNewTransport();

		#endregion
	}
}
