using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class RateChooserRelatedChargesProvider
	{
		readonly BusinessObjectFactory factory;
		readonly RatingCriteria criteria;
		readonly ICW1RatesProvider cw1RatesProvider;
		readonly ILogger logger;
		readonly OrgHeader selectedCarrier;
		AutoRateInfoCollection allCW1Charges;

		public RateChooserRelatedChargesProvider(BusinessObjectFactory factory, ILogger logger, RatingCriteria criteria, ICW1RatesProvider cw1RatesProvider, OrgHeader selectedCarrier)
		{
			this.factory = factory;
			this.criteria = criteria;
			this.cw1RatesProvider = cw1RatesProvider;
			this.logger = logger;
			this.selectedCarrier = selectedCarrier;
		}

		/// <summary>
		/// return CW1 charges where the provider and charge are different from carrierCharges
		/// </summary>
		/// <param name="carrierCharges">
		/// If the carrierCharges originated from a RateService rate, then there must be
		/// at least one charge whose IsFromRateService is true - despite any subject-to-fallback
		/// that may have occured.
		/// </param>
		public IEnumerable<AutoRateInfo> GetNonCarrierCharges(IEnumerable<AutoRateInfo> carrierCharges)
		{
			var rateServiceCarrierPK = carrierCharges
				.Where(c => c.IsFromRatesService)
				.Select(c => c.ProviderPK)
				.ToHashSet();

			var carrierChargeCodes = carrierCharges
				.Select(c => c.ChargeCode.AC_Code)
				.ToHashSet();

			return GetAllCW1Charges()
				.Where(c => !rateServiceCarrierPK.Contains(c.ProviderPK))
				.Where(c => !carrierChargeCodes.Contains(c.ChargeCode.AC_Code))
				.ToList();
		}

		public AutoRateInfoCollection GetAllCW1Charges()
		{
			if (allCW1Charges == null)
			{
				using (RemoveUnusedCarriers(criteria, selectedCarrier))
				{
					var allEntries = cw1RatesProvider.GetCostRateEntries(
						criteria: criteria,
						ignoreCachedRates: true);

					var context = new RatingContext(new LoggerDecorator(logger), factory, null, null, null, false);
					var freightAutoRater = new FreightAutoRater(context);

					var results = freightAutoRater.CalculateResultsForBestMatches(CostSell.Cost, criteria, allEntries);
					results.SumUpSameCharges();
					allCW1Charges = results;
				}
			}
			return allCW1Charges;
		}

		/// <summary>
		/// Job can have multiple possible carriers, but after running Rate Selector
		/// the user will have picked only one. This removes the others from the criteria
		/// to stop their costs being rated.
		/// </summary>
		/// <returns>IDisposable that will restore the criteria to its original state</returns>
		static IDisposable RemoveUnusedCarriers(RatingCriteria criteria, OrgHeader carrierToKeep)
		{
			var possibleCarriers = criteria.PossibleCarriers;
			if (possibleCarriers == null || !possibleCarriers.Any())
			{
				return null;
			}

			var result = new RestoreCarriersAndCreditors(criteria);

			var carrierPKsToRemove = new HashSet<ZGuid>();
			AddIfNotEqual(carrierPKsToRemove, criteria.Carrier, carrierToKeep);
			foreach (var possible in possibleCarriers)
			{
				AddIfNotEqual(carrierPKsToRemove, possible, carrierToKeep);
			}

			if (carrierPKsToRemove.Count == 0)
			{
				return null;
			}

			Creditors newCreditors = null;
			var originalCreditors = result.OriginalCreditors;

			var originalValueCanBeSet = criteria.ValuesCanBeSet;
			criteria.ValuesCanBeSet = true;

			foreach (var chargeGroup in originalCreditors.ChargeCodeGroups)
			{
				var orgs = originalCreditors[chargeGroup]
					.Where(x => x.Org != null && !carrierPKsToRemove.Contains(x.Org.PK));
				if (newCreditors == null)
				{
					if (string.IsNullOrEmpty(chargeGroup))
					{
						newCreditors = Creditors.New(orgs);
					}
					else
					{
						newCreditors = new Creditors();
						newCreditors.Add(chargeGroup, new OrgPrioritizedList(orgs));
					}
				}
				else
				{
					newCreditors.Add(chargeGroup, new OrgPrioritizedList(orgs));
				}

				criteria.Carrier = carrierToKeep;
				criteria.PossibleCarriers = Enumerable.Empty<OrgHeader>();
				criteria.Creditors = newCreditors;
			}

			criteria.ValuesCanBeSet = originalValueCanBeSet;

			return result;
		}

		static void AddIfNotEqual(HashSet<ZGuid> pks, BusinessObject bizToAdd, BusinessObject bizToExclude)
		{
			if (bizToAdd != null &&
				(bizToExclude == null || bizToAdd.PK != bizToExclude.PK))
			{
				pks.Add(bizToAdd.PK);
			}
		}

		/// <summary>
		/// Takes a snapshot of criteria carriers and creditors
		/// and restores them when disposed.
		/// </summary>
		class RestoreCarriersAndCreditors : IDisposable
		{
			readonly RatingCriteria criteria;
			readonly OrgHeader originalCarrier;
			readonly IEnumerable<OrgHeader> possibleCarriers;

			public RestoreCarriersAndCreditors(RatingCriteria criteria)
			{
				this.criteria = criteria;
				originalCarrier = criteria.Carrier;
				OriginalCreditors = criteria.Creditors;
				possibleCarriers = criteria.PossibleCarriers;
			}

			public Creditors OriginalCreditors { get; }

			public void Dispose()
			{
				var originalValueCanBeSet = criteria.ValuesCanBeSet;
				criteria.ValuesCanBeSet = true;

				criteria.Carrier = originalCarrier;
				criteria.Creditors = OriginalCreditors;
				criteria.PossibleCarriers = possibleCarriers;

				criteria.ValuesCanBeSet = originalValueCanBeSet;
			}
		}
	}
}
