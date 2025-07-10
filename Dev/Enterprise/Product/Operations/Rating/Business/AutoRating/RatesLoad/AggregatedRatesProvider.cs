using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Integration;

namespace Enterprise.Rating.Business
{
	public class AggregatedRatesProvider : IRatesProvider
	{
		public AggregatedRatesProvider(params IRatesProvider[] rateProviders)
		{
			this.rateProviders = Argument.NotNull(rateProviders, "rateProviders");
		}

		public IEnumerable<IRateEntry> GetCostRateEntries(RatingCriteria criteria)
		{
			if (criteria.GatewayConfiguration.StopAutoratingCostFromCosting)
			{
				return Enumerable.Empty<IRateEntry>();
			}

			var rates = rateProviders.SelectMany(r => r.GetCostRateEntries(criteria)).ToList();
			return rates;
		}

		public void ReconfigureLogger(ILogger newLogger)
		{
			rateProviders.ToList().ForEach(provider => provider.ReconfigureLogger(newLogger));
		}

		readonly IRatesProvider[] rateProviders;
	}
}
