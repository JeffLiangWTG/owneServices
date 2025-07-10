using CargoWise.EntityFramework;
using Enterprise.Rating.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Quotations.Testing
{
	[TestedType(typeof(TrackingQuoteContainerDependentCollection))]
	public class TrackingQuoteContainerDependentCollectionTest : RateOneOffContainersCollectionTest<TrackingQuoteContainer>
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			TrackingQuote testQuote = Factory.New<TrackingQuote>();
			testQuote.TH_OneTimeQuote = true;
			return new TrackingQuoteContainerDependentCollection(testQuote, false);
		}
	}
}
