using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Quotations.Testing
{
	[TestedType(typeof(TrackingQuoteContainer))]
	public class TrackingQuoteContainerTest : RateOneOffContainersTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			TrackingQuote quote = Factory.New<TrackingQuote>();
			RateOneOffShipment oneOffShipment = quote.OneOffQuote.AddNew();
			TrackingQuoteContainer container = Factory.New<TrackingQuoteContainer>();
			oneOffShipment.Containers.Add(container);
			return container;
		}

		#endregion
	}
}
