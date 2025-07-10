using CargoWise.Types;
using Enterprise.Rating.Business;

namespace Enterprise.Tracking.Business.Quotations
{
	interface ITrackingQuotePropertiesProvider
	{
		ZString QuoteBusinessObjectName { get; }

		ZString[] GetEmailAddresses(Quote quote);
	}
}
