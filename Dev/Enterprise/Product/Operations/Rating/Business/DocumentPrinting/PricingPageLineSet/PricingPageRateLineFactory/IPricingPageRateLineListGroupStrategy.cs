using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public interface IPricingPageRateLineListGroupStrategy<T>
	{
		T GroupKey(RateEntry rateEntry);
		bool IncludeContainer(T group, RefContainer container);

		void CrossPollinate(IDictionary<T, List<RateLine>> groups);
		void Purge(IDictionary<T, List<PricingPageRateLineList>> groups);
	}
}
