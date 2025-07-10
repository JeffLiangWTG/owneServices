using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Urs.Api.Integration;
using Urs.Api.Integration.DTOs;
using Urs.Api.Integration.DTOs.Request;
using WiseRates.Tools.Exceptions;
using static System.FormattableString;
using static Enterprise.Rating.Business.RatingUsageCollector;

namespace Enterprise.Rating.CarrierConnect
{
	class UrsRateSelectorProvider(BusinessObjectFactory factory, LoggerDecorator logger) : IRateSelectorProvider
	{
		static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
		static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(DataRegistryRating.Instance.RatesServiceRateSearchRequestTimeout.Value);

		public RateProvider Provider => RateProvider.URS;

		public string RawResponse { get; private set; } // In the future WI, this will be used to check response from URS

		public Task<List<IRateEntry>> GetRatesAsync(RatingCriteria criteria, RateQueryBusinessObject rateQuery, string requestID = "")
		{
			requestID = string.IsNullOrEmpty(requestID) ? requestID : GenerateRequestID();
			using var cts = new CancellationTokenSource(RequestTimeout);
			var ursRatesClientFactory = ObjectFactory.Get<IUrsRatesClientFactory>();
			var ursClient = ursRatesClientFactory.TryCreate(rateQuery.TransportMode, rateQuery.ContainerMode, requestID, logger, DefaultTimeout, cts.Token);
			if (ursClient == null)
			{
				return Task.FromResult(new List<IRateEntry>());
			}

			var queryRequest = CarrierConnectUrsRatesQueryBuilder.Build(rateQuery);

			var tradeServices = FetchTradeServices(ursClient, queryRequest, requestID, cts.Token);
			RawResponse = ursClient.LastRequest ?? tradeServices.ToJSON();
			var rates = ConvertServicesToRateEntries(tradeServices, criteria, requestID);
			return Task.FromResult(rates);
		}

		static string GenerateRequestID() => Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture).ToLowerInvariant();

		public IEnumerable<TradeServiceDto> FetchTradeServices(
			IUrsClient ursClient,
			QueryRequestDto queryRequest,
			string requestID,
			CancellationToken cancellationToken)
		{
			try
			{
				return ursClient
					.GetTradeServicesAsync(queryRequest, requestID, cancellationToken)
					.GetAwaiter()
					.GetResult();
			}
			catch (Exception ex)
			{
				var (errorCode, errorDetail) = ex switch
				{
					HttpResponseException httpEx => (httpEx.StatusCode.ToString(), httpEx.Message),
					_ => ("UNKNOWN", ex.Message)
				};

				logger.Error(Invariant($"[URS-SERVICE] Request failure | RequestID:{requestID} | Code:{errorCode} | Detail:{errorDetail}"));
				return Enumerable.Empty<TradeServiceDto>();
			}
		}

		List<IRateEntry> ConvertServicesToRateEntries(
			IEnumerable<TradeServiceDto> tradeServices,
			RatingCriteria criteria,
			string requestID)
		{
			var ratesConverter = new UrsRatesConverter(factory, logger, criteria, requestID);
			return ratesConverter
				.ConvertTradeServiceDtoToWiseEntries(tradeServices)
				.ToList();
		}
	}
}
