using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(TransitOperationWithBindingItineraryZeroProvider))]
sealed class TransitOperationWithBindingItineraryZeroProviderTest : TransitOperationProviderAbstractTest<TransitOperationWithBindingItineraryZeroProvider>
{
	public override void TestBindingItinerary()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No Routing itinerary defined", false, GetProvider().BindingItinerary);
			header.CountriesOfRouting.AddNew();
			AssertEquals("Routing itinerary defined", false, GetProvider().BindingItinerary);
		});
	}
}
