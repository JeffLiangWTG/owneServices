
using NUnit.Framework;
namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(ViewTransportBookingParents))]
	public class ViewTransportBookingParentsTest : DtbBookingBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported()
		{
			return false;
		}
	}
}
