using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Integration;
using Urs.Api.Integration;
using WiseRates.Api.Client;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Business.WiseRates
{
	public class UrsWiseRatesClient : IWiseRatesClient
	{
		public UrsWiseRatesClient(string serviceUrl, string authToken, IWiseRatesClient ratesServiceClient, ILogger logger, bool enableDiagnostics = false)
		{
			this.ursClient = new UrsClient(serviceUrl, authToken, enableDiagnostics);
			this.converter = new (logger);
			this.ratesServiceClient = ratesServiceClient;
			this.ServiceURL = serviceUrl;
		}

		public void Dispose()
		{
			(ursClient as IDisposable)?.Dispose();
			ratesServiceClient.Dispose();
		}

		public async Task<RatesSearchResponse> SearchAsync(RatesSearchRequest request, string correlationID = null, CancellationToken cancellationToken = new CancellationToken())
		{
			var ursRequest = converter.Map(request);
			var ursRates = await ursClient.GetTradeServicesAsync(ursRequest, correlationID, cancellationToken).ConfigureAwait(false);
			var response = converter.Map(ursRates, correlationID);

			return response;
		}

		public RefChargeCode[] GetChargeCodes(string correlationID = null)
		{
			return ratesServiceClient.GetChargeCodes(correlationID);
		}

		public Task<RefChargeCode[]> GetChargeCodesAsync(string correlationID = null)
		{
			return ratesServiceClient.GetChargeCodesAsync(correlationID);
		}

		public RefCommodityGroup[] GetCommodityGroups(string correlationID = null)
		{
			return ratesServiceClient.GetCommodityGroups(correlationID);
		}

		public Task<RefCommodityGroup[]> GetCommodityGroupsAsync(string correlationID = null)
		{
			return ratesServiceClient.GetCommodityGroupsAsync(correlationID);
		}

		public RefServiceLevel[] GetServiceLevels(string correlationID = null)
		{
			return ratesServiceClient.GetServiceLevels(correlationID);
		}

		public Task<RefServiceLevel[]> GetServiceLevelsAsync(string correlationID = null)
		{
			return ratesServiceClient.GetServiceLevelsAsync(correlationID);
		}

		public RatesServiceConfiguration GetConfiguration(string correlationID = null)
		{
			return ratesServiceClient.GetConfiguration(correlationID);
		}

		public Task<RatesServiceConfiguration> GetConfigurationAsync(string correlationID = null)
		{
			return ratesServiceClient.GetConfigurationAsync(correlationID);
		}

		public ChargeCodeWithMappingInfo[] GetChargeCodesWithMappingInfo(string correlationID = null)
		{
			return ratesServiceClient.GetChargeCodesWithMappingInfo(correlationID);
		}

		public Task<ChargeCodeWithMappingInfo[]> GetChargeCodesWithMappingInfoAsync(string correlationID = null)
		{
			return ratesServiceClient.GetChargeCodesWithMappingInfoAsync(correlationID);
		}

		public ChargeCodeWithMappingInfo[] GetAllChargeCodesWithMappingInfo(string correlationID = null)
		{
			return ratesServiceClient.GetAllChargeCodesWithMappingInfo(correlationID);
		}

		public Task<ChargeCodeWithMappingInfo[]> GetAllChargeCodesWithMappingInfoAsync(string correlationID = null)
		{
			return ratesServiceClient.GetAllChargeCodesWithMappingInfoAsync(correlationID);
		}

		public Task SendKafkaMessageAsync(IEnumerable<string> kafkaMessages)
		{
			throw new NotImplementedException();
		}

		public HashSet<string> GetNamedAccounts(string correlationID = null)
		{
			var result = ursClient.GetNamedAccounts(correlationID);
			return result.Select(result => result.Name).ToHashSet();
		}

		public string ServiceURL { get; }
		public string AccessToken { get; }
		public string LastRequest => ursClient.LastRequest;

		readonly IUrsClient ursClient;
		readonly IWiseRatesClient ratesServiceClient;
		readonly UrsWiseRatesConverter converter;
	}
}
