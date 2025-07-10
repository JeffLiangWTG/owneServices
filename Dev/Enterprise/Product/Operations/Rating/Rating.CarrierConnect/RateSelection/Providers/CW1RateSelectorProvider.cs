#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Rating.Business.RatingUsageCollector;

namespace Enterprise.Rating.CarrierConnect
{
	class CW1RateSelectorProvider(BusinessObjectFactory factory, LoggerDecorator logger) : IRateSelectorProvider
	{
		public RateProvider Provider => RateProvider.CW1;

		public Task<List<IRateEntry>> GetRatesAsync(RatingCriteria criteria, RateQueryBusinessObject rateQuery, string requestID = "")
		{
			var costRateEntries = GetCostRateEntries(criteria);

			return Task.FromResult(costRateEntries);
		}

		List<IRateEntry> GetCostRateEntries(RatingCriteria criteria)
		{
			using var mode = SetTemporaryManualSelectionSearchingMode(criteria);
			var carrierOrganisationsFilter = GetAdditionalCarrierOrganisationFilter();

			var ratesLoader = new CostRatesLoader(factory, logger);
			var standardRates = ratesLoader.Load(criteria, excludeCarrierFilter: true, carrierOrganisationsFilter);

			return RateEntryFilter.Filter(criteria, isCosting: true, standardRates, factory, logger).ToList();
		}

		static ZDBOnlyQuery GetAdditionalCarrierOrganisationFilter()
		{
			var query = new ZDBOnlyQuery(typeof(RatingHeader));

			// Include all applicable (e.g carrier) organisations.
			var serviceProviderQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			serviceProviderQuery.AddToFilter(ServiceProviderCollection.GetServiceProviderFilter());
			query.AddSubQuery(RatingHeaderSchema.TH_OH, serviceProviderQuery, JoinCondition.Or);

			// Include standard costings.
			query.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_OH, null);

			return query;
		}

		static DisposableAction SetTemporaryManualSelectionSearchingMode(RatingCriteria criteria)
		{
			var originalIsManualCostSelectSearchingCW1Rates = criteria.IsManualCostSelectSearchingCW1Rates;

			return new DisposableAction(
				() => criteria.IsManualCostSelectSearchingCW1Rates = true,
				() => criteria.IsManualCostSelectSearchingCW1Rates = originalIsManualCostSelectSearchingCW1Rates);
		}
	}
}
