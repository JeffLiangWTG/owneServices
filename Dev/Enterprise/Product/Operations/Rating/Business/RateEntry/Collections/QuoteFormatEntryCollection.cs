using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class QuoteFormatEntryCollection : BusinessObjectCollection<QuoteEntry>
	{
		public QuoteFormatEntryCollection(BusinessObjectFactory factory, Quote quote)
			: base(factory)
		{
			this.quote = quote;
		}

		readonly RatingHeader quote;

		public void LoadEntries()
		{
			RemoveAll();

			if (!quote.HasErrors)
			{
				//Ensure the PricingPageCollection.CanAddtoPricingPage method retrieves the most recent RelatedEntries by clearing the cache in RelatedEntriesLoader.GetRelatedEntries.
				foreach (var rateEntry in quote.AllEntries)
				{
					quote.Factory.ClearCachedValue<RelatedRateEntries>(rateEntry.KeyForQuotationPricingPage(exactMatch: false));
				}

				var pricingPages = new PricingPageCollection(quote);
				pricingPages.LoadStandard();

				foreach (PricingPage page in pricingPages)
				{
					AddRange(page.RateEntries);
				}
			}
		}
	}
}

