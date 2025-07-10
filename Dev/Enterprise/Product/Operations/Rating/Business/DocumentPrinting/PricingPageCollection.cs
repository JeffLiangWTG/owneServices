using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business
{
	public sealed class PricingPageCollection : NonPersistentBusinessObjectCollection<PricingPage>
	{
		public PricingPageCollection(RatingHeader ratingHeader)
			: base(ratingHeader.Factory)
		{
			this.ratingHeader = ratingHeader;
		}

		public void LoadStandard() => Load(PricingPaginationStrategy.StandardStyle);

		/// <summary>
		/// If there is a PricingPaginationStrategy Style, then this Collection is cleared and reloaded
		/// according to the PricingPage Style.
		/// </summary>
		public void Load(PricingPaginationStrategy strategy)
		{
			var styleStrategy = strategy & PricingPaginationStrategy.StyleMask;
			if (styleStrategy == PricingPaginationStrategy.None)
			{
				return;
			}

			RemoveAll();

			var comparer = GetPageEqualityComparer(styleStrategy);
			var lookup = new Dictionary<RateEntry, PricingPage>(comparer);
			var pricingPageStyle = GetPricingPageStyle(styleStrategy);
			var rateType = GetRateType(strategy);
			var viewAgentRates = strategy.HasFlag(PricingPaginationStrategy.IsAgentPricingPage);

			Add(lookup, pricingPageStyle, viewAgentRates, rateType, RateCategoryGroup.Freight);

			if (ratingHeader.TH_PrintRateLevelOriginCharges)
			{
				Add(lookup, pricingPageStyle, viewAgentRates, rateType, RateCategoryGroup.Origin);
			}

			if (ratingHeader.TH_PrintRateLevelDestinationCharges)
			{
				Add(lookup, pricingPageStyle, viewAgentRates, rateType, RateCategoryGroup.Destination);
			}

			Add(lookup, pricingPageStyle, viewAgentRates, rateType, RateCategoryGroup.OtherSupplementary);
		}

		PageEqualityComparer GetPageEqualityComparer(PricingPaginationStrategy strategyStyle)
		{
			switch (strategyStyle)
			{
				case PricingPaginationStrategy.LandscapeSimpleStyle:
					return new LandscapeSimplePageEqualityComparer();

				case PricingPaginationStrategy.LandscapeComplexStyle:
					return new LandscapeComplexPageEqualityComparer();

				case PricingPaginationStrategy.LandscapeCompactStyle:
					return new CompactPageEqualityComparer();

				case PricingPaginationStrategy.StandardStyle:
					return new StandardPageEqualityComparer();

				default:
					throw new NotSupportedException("Pricing Page cannot be loaded as no clear style has been included in the strategy enum");
			}
		}

		PricingPageStyle GetPricingPageStyle(PricingPaginationStrategy styleStrategy)
			=> styleStrategy == PricingPaginationStrategy.StandardStyle
				? PricingPageStyle.Standard
				: PricingPageStyle.Landscape;

		RateType GetRateType(PricingPaginationStrategy strategy)
		{
			var categoryFilter = strategy & PricingPaginationStrategy.CategoryFilterMask;
			switch (categoryFilter)
			{
				case PricingPaginationStrategy.ForwardingCategoryFilter:
					return RateType.Forwarding;

				case PricingPaginationStrategy.ShippingCategoryFilter:
					return RateType.Shipping;

				case PricingPaginationStrategy.ShippingDetentionCategoryFilter:
					return RateType.ShippingExportDetention | RateType.ShippingImportDetention;

				case PricingPaginationStrategy.CFSCategoryFilter:
					return RateType.CFS;

				default:
					return RateType.Forwarding
						| RateType.CFS
						| RateType.Warehouse
						| RateType.TransportBookings
						| RateType.Shipping
						| RateType.ShippingImportDetention
						| RateType.ShippingExportDetention
						| RateType.LocalTransport
						| RateType.ContainerYard
						| RateType.TransitWarehouse
						| RateType.TransitWarehouseTransportationUnit;
			}
		}

		static int WeightFreightCategory(string freightCategory)
		{
			switch (freightCategory)
			{
				case RatingConstants.RateCategory.AIR:
					return 1;
				case RatingConstants.RateCategory.LCL:
					return 2;
				case RatingConstants.RateCategory.SNC:
					return 3;
				case RatingConstants.RateCategory.FCL:
					return 4;
				case RatingConstants.RateCategory.SCO:
					return 5;
				default:
					return 6;
			}
		}

		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotSupportedException("Can't add QuoteFormatTable directly");

		void Add(Dictionary<RateEntry, PricingPage> lookup, PricingPageStyle style, bool viewAgentRates, RateType rateType, RateCategoryGroup group)
		{
			var entries = GetRateEntriesByRateCategoryGroup(rateType, group, viewAgentRates);
			foreach (var entry in entries)
			{
				if (!lookup.TryGetValue(entry, out PricingPage pricingPage))
				{
					pricingPage = new PricingPage(entry, Factory, style, viewAgentRates);
					Add(pricingPage);
					lookup.Add(entry, pricingPage);
				}
				else
				{
					pricingPage.AddRateEntry(entry);
				}
			}
		}

		RateEntry[] GetRateEntriesByRateCategoryGroup(RateType rateType, RateCategoryGroup group, bool viewAgentRates)
		{
			var categories = RatingConstants.RateCategory.GetRateCategories(rateType, group);
			var isFreightGroup = group == RateCategoryGroup.Freight;
			if (isFreightGroup)
			{
				categories.StableSort((x, y) => WeightFreightCategory(x).CompareTo(WeightFreightCategory(y)));
			}

			return GetRateEntriesByCategories(categories, group, viewAgentRates);
		}

		RateEntry[] GetRateEntriesByCategories(IEnumerable<string> categories, RateCategoryGroup group, bool viewAgentRates)
		{
			var rateEntries = new HashSet<RateEntry>(BusinessObjectEqualityComparer<RateEntry>.PKOnlyComparer);
			foreach (var category in categories)
			{
				var rateEntryCollection = ratingHeader.EntryCollectionsExcludingSummary[category].LoadedCollection.Cast<RateEntry>();
				foreach (var rateEntry in rateEntryCollection)
				{
					if (CanAddToPricingPage(rateEntry, group, viewAgentRates))
					{
						rateEntries.Add(rateEntry);
					}
				}
			}

			return rateEntries.ToArray();
		}

		bool CanAddToPricingPage(RateEntry rateEntry, RateCategoryGroup group, bool viewAgentRates)
		{
			if (!rateEntry.IsDeleted)
			{
				if (group == RateCategoryGroup.Freight)
				{
					return true;
				}

				var visibleRateLines = rateEntry.RateLines.Cast<RateLine>()
					.Where(x => PricingPageRateLineFactory.IsVisibleOnPricingPage(x, rateEntry, viewAgentRates))
					.ToArray();

				bool canAdd;
				switch (group)
				{
					case RateCategoryGroup.Origin:
						canAdd = !this.Cast<PricingPage>().Any(page => page.IsOriginEntryPrintedByFreightRelatedLineSet(rateEntry, visibleRateLines));
						break;

					case RateCategoryGroup.Destination:
						canAdd = !this.Cast<PricingPage>().Any(page => page.IsDestinationEntryPrintedByFreightRelatedLineSet(rateEntry, visibleRateLines));
						break;

					default:
						canAdd = true;
						break;
				}

				return canAdd;
			}

			return false;
		}

		readonly RatingHeader ratingHeader;
	}
}

