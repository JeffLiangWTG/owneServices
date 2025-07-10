using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RateCreator
	{
		public RateCreator(RatingHeader ratingHeader)
		{
			this.ratingHeader = Argument.NotNull(ratingHeader, nameof(ratingHeader));
		}

		readonly RatingHeader ratingHeader;

		#region Load or Create Globally Published RatingHeaders for Local RatingHeaders

		public RatingHeader LoadOrCreateGlobalRatingHeader(BusinessObjectFactory factory)
		{
			var result = LoadOrCreateGlobalRatingHeader(factory, true);
			factory.ClearCachedValue<RatingHeader>(ratingHeader.GlobalRatingHeaderKey);

			return result;
		}

		public RatingHeader LoadGlobalRatingHeader(BusinessObjectFactory factory)
		{
			return LoadOrCreateGlobalRatingHeader(factory, false);
		}

		RatingHeader LoadOrCreateGlobalRatingHeader(BusinessObjectFactory factory, bool shouldCreateNewGlobalRate)
		{
			if (!ratingHeader.SupportsRateEntryPublish())
			{
				return null;
			}

			if (ratingHeader.IsCosting())
			{
				return LoadOrCreateGlobalCosting(factory, shouldCreateNewGlobalRate);
			}

			if (ratingHeader.IsTariff() && ratingHeader.IsLevelOneTariff())
			{
				return LoadOrCreateGlobalTariff(factory, shouldCreateNewGlobalRate);
			}

			if (ratingHeader.IsClientRate() && ratingHeader.Header != null)
			{
				return LoadOrCreateGlobalClientRate(factory, shouldCreateNewGlobalRate);
			}

			return null;
		}

		RatingHeader LoadOrCreateGlobalCosting(BusinessObjectFactory factory, bool shouldCreateNewGlobalRate)
		{
			var costing = factory.LoadTop1<Costing>(GlobalRatingHeaderQuery);
			if (costing == null && shouldCreateNewGlobalRate && Env.Security.GlobalCostingRatesNew.IsAllowed)
			{
				costing = factory.New<Costing>();
				costing.TH_GC = ZGuid.Empty;
				costing.TH_OH = ratingHeader.TH_OH;
			}

			return costing;
		}

		RatingHeader LoadOrCreateGlobalTariff(BusinessObjectFactory factory, bool shouldCreateNewGlobalRate)
		{
			var globalTariffQuery = GlobalRatingHeaderQuery;
			globalTariffQuery.AddToFilter(RatingHeaderSchema.TH_GlobalRateLevel, new ZByte(1));
			var globalTariff = factory.LoadTop1<CompanyTariff>(GlobalRatingHeaderQuery); //Due to binding we have to use CompanyTariff and not GlobalTariff
			if (globalTariff == null && shouldCreateNewGlobalRate && Env.Security.GlobalTariffRatesNew.IsAllowed)
			{
				globalTariff = factory.New<CompanyTariff>();
				globalTariff.TH_GC = ZGuid.Empty;
				globalTariff.TH_GlobalRateLevel = new ZByte(1);
				globalTariff.TH_GlobalRateDescription = Res.GetString("03508f08-d213-4fb4-aa30-ed9b71612c53", "Global Base Tariff");
			}

			return globalTariff;
		}

		RatingHeader LoadOrCreateGlobalClientRate(BusinessObjectFactory factory, bool shouldCreateNewGlobalRate)
		{
			var clientRate = factory.LoadTop1<ClientRate>(GlobalRatingHeaderQuery);
			if (clientRate == null && shouldCreateNewGlobalRate && Env.Security.GlobalClientRatesNew.IsAllowed)
			{
				clientRate = factory.New<ClientRate>();
				clientRate.TH_GC = ZGuid.Empty;
				clientRate.TH_OH = ratingHeader.TH_OH;
			}

			return clientRate;
		}

		ZQuery GlobalRatingHeaderQuery
		{
			get
			{
				var globalRatingHeaderQuery = new ZQuery(RatingHeaderSchema.TH_GC, null);
				globalRatingHeaderQuery.AddToFilter(RatingHeaderSchema.TH_RateType, ratingHeader.TH_RateType);
				if (ratingHeader.TH_OH.IsEmpty)
				{
					globalRatingHeaderQuery.AddToFilter(RatingHeaderSchema.TH_OH, null);
				}
				else
				{
					globalRatingHeaderQuery.AddToFilter(RatingHeaderSchema.TH_OH, ratingHeader.TH_OH);
				}

				return globalRatingHeaderQuery;
			}
		}

		#endregion

		#region Create Quote From Existing Rate Entries

		public Quote CreateQuoteFromExistingRateEntries(BusinessObject[] entries, bool includeRelatedEntries, IRateEntrySecurityUIIntractor uiIntractor = null)
		{
			var objectFactory = new BusinessObjectFactory();
			var newQuote = objectFactory.New<Quote>();
			newQuote.TH_OH = ratingHeader.TH_OH;

			var entryAndRelatedEntryList = GetEntriesAndRelatedEntryList(entries, includeRelatedEntries);
			foreach (var existingEntry in entryAndRelatedEntryList)
			{
				var clonedEntry = existingEntry.Clone(newQuote.EntryCollections[existingEntry.TI_RateCategory], uiIntractor);

				if (clonedEntry == null)
				{
					continue;
				}

				clonedEntry.TI_RateStartDate = newQuote.TH_QuoteDate;
				clonedEntry.TI_RateEndDate = newQuote.TH_QuoteEndDate;

				CreateQuoteLinesFromExistingRateLines(existingEntry, clonedEntry);
			}

			return newQuote;
		}

		static IEnumerable<RateEntry> GetEntriesAndRelatedEntryList(BusinessObject[] entries, bool includeRelatedEntries)
		{
			var result = entries.Cast<RateEntry>().ToList();
			if (!includeRelatedEntries)
			{
				return result;
			}

			var relatedEntries = new List<RateEntry>();
			foreach (var entry in result)
			{
				if (entry.IsFreightEntry())
				{
					relatedEntries.AddRange(entry.GetRelatedEntries(RateEntry.RelatedEntriesToFindType.Origin));
					relatedEntries.AddRange(entry.GetRelatedEntries(RateEntry.RelatedEntriesToFindType.Destination));
				}
				else
				{
					relatedEntries.AddRange(entry.GetRelatedEntries(RateEntry.RelatedEntriesToFindType.Freight));
				}
			}

			return result.Union(relatedEntries);
		}

		void CreateQuoteLinesFromExistingRateLines(RateEntry entryToClone, RateEntry clone)
		{
			foreach (RateLine line in entryToClone.RateLines)
			{
				var newLine = line.Clone(clone.RateLines);

				if (!ratingHeader.IsTariff())
				{
					newLine.RateLineItems.Clone(line);
				}
				else // If we're creating a quote from a Company Tariff, then we should use the CompanyTariff Calculator
				{
					newLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
				}
			}
		}

		#endregion
	}
}

