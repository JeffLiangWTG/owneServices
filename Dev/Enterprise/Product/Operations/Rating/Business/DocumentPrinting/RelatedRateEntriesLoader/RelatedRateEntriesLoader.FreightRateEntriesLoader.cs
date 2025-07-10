using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.RatingEnums;

namespace Enterprise.Rating.Business
{
	public partial class RelatedRateEntriesLoader
	{
		sealed class FreightRateEntriesLoader : BaseRateEntriesLoader
		{
			public FreightRateEntriesLoader(PricingPage pricingPage, BusinessObjectFactory factory)
				: base(pricingPage, factory)
			{
			}

			protected override bool NeedToLoadInheritedRates(RatingHeader ratingHeader) => true;

			protected override bool NeedToLoad(RatingHeader ratingHeader) => true;

			protected override bool EmptyColumnFilterMatch(RateEntry rateEntry) => true;

			protected override ZString GetRateCategory(RateEntry rateEntry)
				=> rateEntry.IsFreightEntry()
					? rateEntry.TI_RateCategory
					: ZString.Empty;

			protected override RefContainerCollection GetContainersInSameClass(RefContainer container)
				=> container.GetContainersInSameClassForEntryType(EntryTypes.Freight);
		}
	}
}
