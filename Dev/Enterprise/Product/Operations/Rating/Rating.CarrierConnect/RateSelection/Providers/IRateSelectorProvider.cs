using System.Collections.Generic;
using System.Threading.Tasks;
using Enterprise.Rating.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using static Enterprise.Rating.Business.RatingUsageCollector;

namespace Enterprise.Rating.CarrierConnect
{
	public interface IRateSelectorProvider
	{
		internal Task<List<IRateEntry>> GetRatesAsync(RatingCriteria criteria, RateQueryBusinessObject rateQuery, string requestID = "");

		internal RateProvider Provider { get; }
	}
}
