using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public partial class RelatedRateEntriesLoader
	{
		public RelatedRateEntriesLoader(PricingPage pricingPage, BusinessObjectFactory factory)
		{
			this.factory = factory;

			freightRateEntriesLoader = new FreightRateEntriesLoader(pricingPage, factory);
			originRateEntriesLoader = new OriginRateEntriesLoader(pricingPage, factory);
			destinationRateEntriesLoader = new DestinationRateEntriesLoader(pricingPage, factory);
		}

		readonly BusinessObjectFactory factory;

		public RelatedRateEntries GetRelatedEntries(RateEntry parentRateEntry, bool exactMatch = false)
			=> factory.GetCachedValue(parentRateEntry.KeyForQuotationPricingPage(exactMatch), () => GetRelatedRateEntriesForPricingPage(parentRateEntry, exactMatch));

		RelatedRateEntries GetRelatedRateEntriesForPricingPage(RateEntry parentRateEntry, bool exactMatch)
		{
			var result = new RelatedRateEntries();
			result.FreightRateEntries = freightRateEntriesLoader.GetRelatedRateEntries(parentRateEntry, exactMatch);
			result.OriginRateEntries = originRateEntriesLoader.GetRelatedRateEntries(parentRateEntry, exactMatch);
			result.DestinationRateEntries = destinationRateEntriesLoader.GetRelatedRateEntries(parentRateEntry, exactMatch);

			return result;
		}

		readonly FreightRateEntriesLoader freightRateEntriesLoader;
		readonly OriginRateEntriesLoader originRateEntriesLoader;
		readonly DestinationRateEntriesLoader destinationRateEntriesLoader;
	}
}
