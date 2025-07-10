using System.Linq;
using CargoWise.Types;
using Enterprise.Rating.Business;

namespace Enterprise.Tracking.Business.Quotations
{
	class TrackingQuotationPropertiesProvider : ITrackingQuotePropertiesProvider
	{
		ZString ITrackingQuotePropertiesProvider.QuoteBusinessObjectName => ResString.GetMultilingualString("c4eeab47-cc2a-447e-8fa6-cbd135da4358", "Quotation");

		ZString[] ITrackingQuotePropertiesProvider.GetEmailAddresses(Quote quote)
		{
			var firstSignatory = quote.FirstSignatory;
			var secondSignatory = quote.SecondSignatory;
			var salesRepresentative = quote.HeaderStaffAssignments?.OverallSalesRepStaff;

			var emails = new[]
			{
				firstSignatory?.GS_EmailAddress ?? string.Empty,
				secondSignatory?.GS_EmailAddress ?? string.Empty,
				salesRepresentative?.GS_EmailAddress ?? string.Empty,
			}.Where(e => e != string.Empty).Distinct().ToArray();

			return emails;
		}
	}
}
