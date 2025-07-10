using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Rating.Business;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Conversion;

namespace Enterprise.Rating.Web
{
	/// <summary>
	/// This is a utility class to autorate a RateQuery. 
	/// </summary>
	public class RatesAPIsAutoRater
	{
		/// <summary>
		/// AutoRate
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="rateQuery"></param>
		/// <param name="logger"></param>
		/// <returns></returns>
		public virtual IReadOnlyCollection<Rate> AutoRate(BusinessObjectFactory factory, RateQuery rateQuery, ILogger logger)
		{
			var interactor = new LoggerDecorator(logger);
			var ratingContext = new RatingContext(interactor, factory, null);
			ratingContext.IsInRebateCalculationMode = false;

			var businessObject = new RateQueryBusinessObject(factory, rateQuery, SourceEndpoint.JobCharges);
			var ratingAdapter = new RateQueryRatingAdapter(businessObject);
			var ratingCriteria = new RatingCriteria(ratingAdapter, factory);
			var autoRater = new FreightAutoRater(ratingContext);
			var autoRateInfos = new AutoRateInfoCollection(factory);

			if
				(
					rateQuery.CalculationScope == RateQuery.CalculationScopes.BuyRatesOnly
					|| rateQuery.CalculationScope == RateQuery.CalculationScopes.BuyAndSellRates
				)
			{
				using (_Rating.Start(interactor))
				using (_Rating.StartCost())
				{
					autoRateInfos.AddRange(autoRater.AutoRate(ratingCriteria, CostSell.Cost).RateInfoCollection);
				}
			}

			if
				(
					rateQuery.CalculationScope == RateQuery.CalculationScopes.SellRatesOnly
					|| rateQuery.CalculationScope == RateQuery.CalculationScopes.BuyAndSellRates
				)
			{
				using (_Rating.Start(interactor))
				using (_Rating.StartSell())
				{
					autoRateInfos.AddRange(autoRater.AutoRate(ratingCriteria, CostSell.Revenue).RateInfoCollection);
				}
			}

			using (_Rating.Start(interactor))
			{
				var converter = new AutoRateInfoToRateConverter(factory);
				return converter.Convert(ratingAdapter, autoRateInfos, logger);
			}
		}
	}
}
