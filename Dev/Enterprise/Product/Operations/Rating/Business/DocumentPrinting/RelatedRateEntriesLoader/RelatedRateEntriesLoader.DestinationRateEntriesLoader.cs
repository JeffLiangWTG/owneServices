using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.RatingEnums;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public partial class RelatedRateEntriesLoader
	{
		sealed class DestinationRateEntriesLoader : BaseRateEntriesLoader
		{
			public DestinationRateEntriesLoader(PricingPage pricingPage, BusinessObjectFactory factory)
				: base(pricingPage, factory)
			{
			}

			protected override bool NeedToLoadInheritedRates(RatingHeader ratingHeader) => ratingHeader.TH_PrintInheritedDestinationCharges;

			protected override bool NeedToLoad(RatingHeader ratingHeader) => ratingHeader.TH_PrintRateLevelDestinationCharges;

			protected override bool EmptyColumnFilterMatch(RateEntry rateEntry) => !rateEntry.IsFreightEntry();

			protected override ZString GetRateCategory(RateEntry rateEntry)
			{
				if (rateEntry.IsOriginEntry())
				{
					return ZString.Empty;
				}

				var categories = RatingConstants.RateCategory.GetRateCategories(rateEntry.RateType(), RateCategoryGroup.Destination);

				return categories.Length > 0
					? (ZString)categories[0]
					: ZString.Empty;
			}

			protected override ZQuery BuildOriginFilter(RateEntry rateEntry)
				=> rateEntry.IsSupplementaryEntry()
					? BuildSimpleLocationFilter(RateEntrySchema.TI_OriginLRC, rateEntry)
					: base.BuildOriginFilter(rateEntry);

			protected override ZQuery BuildDestinationFilter(RateEntry rateEntry)
				=> rateEntry.IsSupplementaryEntry()
					? BuildSimpleLocationFilter(RateEntrySchema.TI_DestinationLRC, rateEntry)
					: BuildComplexLocationFilter(RateEntrySchema.TI_DestinationLRC, rateEntry, false);

			protected override ZQuery BuildServiceProviderFilter(RateEntry rateEntry)
				=> rateEntry.IsSupplementaryEntry()
					? base.BuildServiceProviderFilter(rateEntry)
					: new ZQuery();

			protected override RefContainerCollection GetContainersInSameClass(RefContainer container)
				=> container.GetContainersInSameClassForEntryType(EntryTypes.Destination);
		}
	}
}
