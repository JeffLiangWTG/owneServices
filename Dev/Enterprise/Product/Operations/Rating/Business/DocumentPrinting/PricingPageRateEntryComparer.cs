using System.Collections.Generic;

namespace Enterprise.Rating.Business
{
	public class PricingPageRateEntryComparer : IEqualityComparer<RateEntry>
	{
		public
#if DEBUG
		virtual
#endif
		bool Equals(RateEntry entry, RateEntry otherEntry)
			=> entry.IsDuplicateForPricingPageGrouping(otherEntry, ignoreCategoryAndMode: false, checkMatchContainerRateClass: true);

		public int GetHashCode(RateEntry entry)
		{
			unchecked
			{
				int hash = 17;

				// I choose to not check the TI_TH for hashing as often the entries
				// belong to the same header.
				hash = hash * 31 + entry.TI_OriginLRC.GetHashCode();
				hash = hash * 31 + entry.TI_DestinationLRC.GetHashCode();
				hash = hash * 31 + entry.TI_RateStartDate.GetHashCode();
				hash = hash * 31 + entry.TI_RateEndDate.GetHashCode();

				// because the entry.IsDuplicateForPricingPageGrouping is called with
				// ignoreCategoryAndMode == false
				hash = hash * 31 + entry.TI_Mode.GetHashCode();
				hash = hash * 31 + entry.TI_RateCategory.GetHashCode();

				// because the entry.IsDuplicateForPricingPageGrouping is called with
				// checkMatchContainerRateClass == true
				hash = hash * 31 + entry.TI_MatchContainerRateClass.GetHashCode();

				return hash;
			}
		}
	}
}

