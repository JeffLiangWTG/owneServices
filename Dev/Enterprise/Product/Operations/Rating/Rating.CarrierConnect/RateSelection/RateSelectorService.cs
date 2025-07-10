#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Rating.Business.RatingUsageCollector;

namespace Enterprise.Rating.CarrierConnect
{
	public class RateSelectorService(ILogger? logger = null)
	{
		readonly LoggerDecorator logger = new(logger);

		public RateSearchResponseDto SearchAndCalculateRates(RateQueryDto rateQueryDto)
		{
			var rateQueryBizo = new RateQueryBusinessObject(rateQueryDto);

			using (_Rating.Start(logger))
			using (_Rating.StartSubSession(rateQueryBizo))
			using (_Rating.StartCost())
			{
				var factory = new BusinessObjectFactory();
				var ratingAdapter = new RateSearchRatingAdapter(rateQueryBizo, factory);
				var ratingCriteria = CreateRatingCriteria(ratingAdapter, factory);

				return CurateAndCalculateRates(factory, ratingCriteria, rateQueryBizo);
			}
		}

		public RateSearchResponseDto SearchAndCalculateRatesUsingCriteria(RatingCriteria jobCriteria, RateQueryDto rateQueryDto, RateSelectorMetricsModel rateSelectorMetrics, string requestID)
		{
			var factory = new BusinessObjectFactory();
			var rateQueryBizo = new RateQueryBusinessObject(rateQueryDto);
			var ratingCriteria = CreateRatingCriteria(new JobRatingAdapter(jobCriteria, rateQueryBizo, factory), factory);

			return CurateAndCalculateRates(factory, ratingCriteria, rateQueryBizo, rateSelectorMetrics, requestID);
		}

		RateSearchResponseDto CurateAndCalculateRates(BusinessObjectFactory factory, RatingCriteria ratingCriteria, RateQueryBusinessObject rateQueryBizo, RateSelectorMetricsModel? rateSelectorMetrics = null, string? requestID = default)
		{
			var providers = ObjectFactory.Get<IRateSelectorProviderFactory>().CreateProviders(logger, factory).ToList();
			var curator = new RateCurator(factory, logger);

			logger.Information((NoResString)"***** RATE LOADING *****");
			var allRates = providers
				.SelectMany(provider =>
				{
					var stopwatch = Stopwatch.StartNew();
					var rates = provider.GetRatesAsync(ratingCriteria, rateQueryBizo, requestID).GetAwaiter().GetResult();
					TrySetRateSelectorTimeMetrics(rateSelectorMetrics, provider, stopwatch.ElapsedMilliseconds);
					return rates;
				}).ToList();
			logger.Information((NoResString)"***** END RATE LOADING *****");

			var includeRawData = DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.Value;
			var rawResponse = providers
				.OfType<UrsRateSelectorProvider>()
				.FirstOrDefault()?.RawResponse;

			var response = curator.CurateAndCalculateRates(ratingCriteria, allRates);
			response.RawResponse = includeRawData ? rawResponse : null;

			return response;
		}

		static RatingCriteria CreateRatingCriteria(IAutoRating ratingAdapter, BusinessObjectFactory factory)
		{
			return new RatingCriteria(ratingAdapter, factory)
			{
				IsManualCostSelectMode = true,
				IsLooseRateSearchForCarrierConnect = true,
				ValuesCanBeSet = true
			};
		}

		public static Dictionary<string, RateType> SupportedRateTypes { get; } = new ()
		{
			{ (NoResString)"Forwarding", RateType.Forwarding },
			{ "CFS", RateType.CFS },
			{ (NoResString)"Warehouse", RateType.Warehouse },
			{ "TransportBookings", RateType.TransportBookings },
			{ (NoResString)"Shipping", RateType.Shipping },
			{ "ShippingImportDetention", RateType.ShippingImportDetention },
			{ "ShippingExportDetention", RateType.ShippingExportDetention },
			{ "LocalTransport", RateType.LocalTransport },
			{ "ContainerYard", RateType.ContainerYard },
			{ "TransitWarehouse", RateType.TransitWarehouse },
			{ (NoResString)"Customs", RateType.Customs },
			{ "TransitWarehouseTransportationUnit", RateType.TransitWarehouseTransportationUnit },
		};

		void TrySetRateSelectorTimeMetrics(RateSelectorMetricsModel? rateSelectorMetrics, IRateSelectorProvider provider, long elapsedTime)
		{
			if (rateSelectorMetrics is null)
			{
				return;
			}

			switch (provider.Provider)
			{
				case RateProvider.URS:
					rateSelectorMetrics.URSSearchTime = elapsedTime;
					break;
				case RateProvider.CW1:
					rateSelectorMetrics.CW1SearchTime = elapsedTime;
					break;
			}
		}
	}
}
