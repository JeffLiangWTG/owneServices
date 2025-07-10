using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	/// <summary>
	///		Finds costs from CW1 database for autorating engine matching <see cref="RatingCriteria"/>.
	///
	///		It is purely for autorating so that we could supply costs from multiple sources:
	///			- From local CW1 database (<see cref="CW1RatesProvider"/>)
	/// 		- From RatesService/URS (<see cref="WiseRatesProvider"/>)
	///
	///		Please don't put here any unrelated logic.
	/// </summary>
	public sealed class CW1RatesProvider : ICW1RatesProvider
	{
		readonly BusinessObjectFactory factory;
		readonly Dictionary<Guid, IEnumerable<IRateEntry>> ratesCacheByCriteria;
		ILogger logger;

		public CW1RatesProvider(BusinessObjectFactory factory, ILogger logger)
		{
			this.factory = factory;
			this.logger = logger;
			this.ratesCacheByCriteria = new Dictionary<Guid, IEnumerable<IRateEntry>>();
		}

		#region Cost

		public IEnumerable<IRateEntry> GetCostRateEntries(RatingCriteria criteria) =>
			GetCostRateEntries(criteria, ignoreCachedRates: false);

		public IEnumerable<IRateEntry> GetCostRateEntries(RatingCriteria criteria, bool ignoreCachedRates)
		{
			var ratesLoader = new CostRatesLoader(factory, logger);
			var watch = Stopwatch.StartNew();
			var standardRates = ignoreCachedRates
				? ratesLoader.Load(criteria)
				: ratesCacheByCriteria.GetOrAdd(criteria.ID, () => ratesLoader.Load(criteria));

			var filteredStandardRates = RateEntryFilter.Filter(criteria, true, standardRates, factory, logger);
			watch.Stop();

			CostRatesLoader.ReportRatesLoadedStats(new RatesLoadedStats
			{
				LoadedRatesCount = standardRates.Count(),
				FilteredRatesCount = filteredStandardRates.Count()
			}, watch.ElapsedMilliseconds);

			var spotRates = ratesLoader.LoadSpotRates(criteria);

			var costs = new List<IRateEntry>();
			costs.AddRange(filteredStandardRates);
			costs.AddRange(spotRates);

			return costs;
		}

		#endregion
		public void ReconfigureLogger(ILogger newLogger)
		{
			this.logger = newLogger;
		}
	}
}
