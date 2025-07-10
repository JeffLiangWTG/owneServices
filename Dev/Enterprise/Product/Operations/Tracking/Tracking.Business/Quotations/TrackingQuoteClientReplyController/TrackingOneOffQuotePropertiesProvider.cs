using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Tracking.Business.Quotations
{
	class TrackingOneOffQuotePropertiesProvider : ITrackingQuotePropertiesProvider
	{
		ZString ITrackingQuotePropertiesProvider.QuoteBusinessObjectName => ResString.GetMultilingualString("d7eb2bb7-7d07-4021-8c78-fda134909ca6", "One Off Quote");

		ZString[] ITrackingQuotePropertiesProvider.GetEmailAddresses(Quote quote)
		{
			var jobHeader = new JobHeader.Loader(quote).Load();

			var repOps = jobHeader?.RepOps;
			var repSales = jobHeader?.RepSales;
			var salesRepresentative = quote.HeaderStaffAssignments?.OverallSalesRepStaff;

			var emails = new[]
			{
				repOps?.GS_EmailAddress ?? string.Empty,
				repSales?.GS_EmailAddress ?? string.Empty,
				salesRepresentative?.GS_EmailAddress ?? string.Empty,
			}.Where(e => e != string.Empty).Distinct().ToArray();

			return emails;
		}
	}
}
