using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Business.AutoRating.RatesLoad
{
	public interface IWiseRatesQueryBuilder
	{
		(RatesQuery query, string error) Build(RatingCriteria criteria, IEnumerable<OrgWithSource> overridenCriteriaCarriers = null, IEnumerable<string> contractNumbersFromFilters = null, IEnumerable<OrgWithSource> carriersFromFilters = null);
		void AddMeasureChargeableVolume(RatesQuery query, RatingCriteria criteria);
	}
}
