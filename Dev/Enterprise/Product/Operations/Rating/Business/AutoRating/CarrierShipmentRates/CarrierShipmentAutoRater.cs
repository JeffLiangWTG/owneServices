using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business;

sealed class CarrierShipmentAutoRater : ICarrierShipmentAutoRater
{
	public CarrierShipmentRateResult AutoRateCarrierShipment(
		CarrierShipmentRateQueryDto rateQueryDto,
		BusinessObjectFactory factory,
		IRateChooserServices chooserServices,
		ILogger logger)
	{
		var loggerDecorator = new LoggerDecorator(logger);
		_ = loggerDecorator.DumpLog();
		using (_Rating.Start(loggerDecorator))
		{
			var results = new List<(AutoRateResult ratingResult, RatingCriteria criteria, CostSell costOrSell)>();
			if (rateQueryDto.GetRatesForCosts)
			{
				results.AddRange(GetCarrierShipmentRatesInternal(rateQueryDto,
					OceanCarrierDataRegistry.Instance.TariffMatchingModeForCost.Value, _Rating.StartCost,
					CostSell.Cost, factory, loggerDecorator));
			}

			if (rateQueryDto.GetRatesForSales)
			{
				results.AddRange(GetCarrierShipmentRatesInternal(rateQueryDto,
					OceanCarrierDataRegistry.Instance.TariffMatchingModeForRevenue.Value, _Rating.StartSell,
					CostSell.Revenue, factory, loggerDecorator));
			}

			var rateResultDtoConverter = new RateResultDtoConverter(factory, chooserServices);
			var logs = loggerDecorator.GetLogs(x => x.Type == LogType.Error ? null : x.Message).Where(x => x != null).ToArray();
			var errors = loggerDecorator.GetLogs(x => x.Type != LogType.Error ? null : x.Message).Where(x => x != null)
				.ToArray();
			var rates = rateResultDtoConverter.ConvertFromRateChargeDtoCollection(results);
			return new(rates, logs, errors);
		}
	}

	List<(AutoRateResult ratingResult, RatingCriteria criteria, CostSell costOrSell)> GetCarrierShipmentRatesInternal(
		CarrierShipmentRateQueryDto rateQueryDto,
		string mode,
		Func<IDisposable> startMethod,
		CostSell costOrSell,
		BusinessObjectFactory factory,
		ILogger logger)
	{
		using (startMethod())
		{
			if (mode == TariffMatchingModeCodeList.Codes.E2E)
			{
				var origin = rateQueryDto.RouteLegs.First().FromAddress;
				var destination = rateQueryDto.RouteLegs.Last().ToAddress;

				return [AutoRateShipment(origin, destination, rateQueryDto, costOrSell, factory, logger)];
			}

			var results = new List<(AutoRateResult ratingResult, RatingCriteria criteria, CostSell costOrSell)>();
			foreach (var leg in rateQueryDto.RouteLegs)
			{
				var origin = leg.FromAddress;
				var destination = leg.ToAddress;

				results.Add(AutoRateShipment(origin, destination, rateQueryDto, costOrSell, factory, logger));
			}

			return results;
		}
	}

	(AutoRateResult ratingResult, RatingCriteria criteria, CostSell costOrSell) AutoRateShipment(
		string origin,
		string destination,
		CarrierShipmentRateQueryDto rateQueryDto,
		CostSell costOrSell,
		BusinessObjectFactory factory,
		ILogger logger)
	{
		var rateQueryBizo = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, origin, destination);
		var ratingAdapter = new CarrierShipmentRatingAdapter(rateQueryBizo, costOrSell, factory);
		var ratingProxy = new AutoRatingProxy(ratingAdapter);
		var ratingCriteria = new RatingCriteria(ratingProxy, factory);
		var freightAutoRater = new FreightAutoRater(new RatingContext(logger));
		var autoRateResult = freightAutoRater.AutoRate(ratingCriteria, costOrSell);
		return (autoRateResult, ratingCriteria, costOrSell);
	}
}
