using Enterprise.Freight.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Test
{
	class QuotedBookingContainerValueObjectDataAdapterForTest : QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>
	{
		public QuotedBookingContainerValueObjectDataAdapterForTest(QuotedBooking quotedBooking)
			: base(quotedBooking)
		{
		}

		public bool ValueOfRegistryDefaultForImporting()
		{
			return base.RegistryDefaultForImporting;
		}
	}
}
