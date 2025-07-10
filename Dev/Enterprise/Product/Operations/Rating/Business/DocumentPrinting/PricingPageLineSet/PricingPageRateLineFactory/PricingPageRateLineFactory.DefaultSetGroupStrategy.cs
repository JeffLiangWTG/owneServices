using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public partial class PricingPageRateLineFactory
	{
		sealed class DefaultPricingPageRateLineListGroupStrategy : IPricingPageRateLineListGroupStrategy<int>
		{
			public const int Key = 0;

			public int GroupKey(RateEntry rateEntry) => Key;

			public bool IncludeContainer(int group, RefContainer container) => true;

			public void CrossPollinate(IDictionary<int, List<RateLine>> groups)
			{
			}

			public void Purge(IDictionary<int, List<PricingPageRateLineList>> groups)
			{
			}
		}
	}
}
