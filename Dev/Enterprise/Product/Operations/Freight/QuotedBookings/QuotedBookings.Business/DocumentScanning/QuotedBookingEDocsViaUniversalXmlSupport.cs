using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	internal class QuotedBookingEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
	{
		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			Argument.NotNull(factory, nameof(factory));

			var query = new ZQuery(ViewQuotedBookingSchema.VB_QuoteNumber, code);
			var viewQuotedBooking = factory.LoadTop1<ViewQuotedBooking>(query);
			if (viewQuotedBooking == null)
			{
				return null;
			}

			var jobShipmentQuery = new ZQuery(JobShipmentSchema.PK, viewQuotedBooking.VB_JS);
			return factory.LoadTop1<ForwardingShipment>(jobShipmentQuery);
		}

		public ZString ExpectedCodeFormat { get; } = "QuoteNumber";
		public ZString ExampleCodeFormat { get; } = "00009999";
	}
}
