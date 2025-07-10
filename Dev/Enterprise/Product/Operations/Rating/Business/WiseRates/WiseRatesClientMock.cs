#region SuppressResourceStringsCheckRegion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Constants;

namespace Enterprise.Rating.Business.WiseRates
{
	public sealed class WiseRatesClientMock : IWiseRatesClient
	{
		public string ServiceURL => "https://wiserates.com";
		public string AccessToken => "some token";
		public string LastRequest { get; }

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public RefChargeCode[] GetChargeCodes(string correlationID = null)
		{
			return new[]
			{
				new RefChargeCode { Code = "BBB", Description = "Bunker Adjustment Factor" },
				new RefChargeCode { Code = "DDD", Description = "Destination Security Surcharge" },
				new RefChargeCode { Code = "FRT", Description = "International Freight" },
			};
		}

		public Task<RefChargeCode[]> GetChargeCodesAsync(string correlationID = null)
		{
			return Task.FromResult(GetChargeCodes(correlationID));
		}

		public RefCommodityGroup[] GetCommodityGroups(string correlationID = null)
		{
			return new[]
			{
				new RefCommodityGroup { Code = "CAR", Description = "Cars" },
				new RefCommodityGroup { Code = "HUM", Description = "Humans" },
			};
		}

		public Task<RefCommodityGroup[]> GetCommodityGroupsAsync(string correlationID = null)
		{
			return Task.FromResult(GetCommodityGroups(correlationID));
		}

		public Task<RatesSearchResponse> SearchAsync(RatesSearchRequest request, string correlationID = null, CancellationToken cancellationToken = default)
		{
			return Task.FromResult(new RatesSearchResponse());
		}

		public RefServiceLevel[] GetServiceLevels(string correlationID = null)
		{
			return new[]
			{
				new RefServiceLevel { Code = "AWS", Description = "Bunker Adjustment Factor" },
				new RefServiceLevel { Code = "BBK", Description = "Break Bulk" },
				new RefServiceLevel { Code = "UNS", Description = "Unspecified" },
			};
		}

		public Task<RefServiceLevel[]> GetServiceLevelsAsync(string correlationID = null)
		{
			return Task.FromResult(GetServiceLevels(correlationID));
		}

		public RatesServiceConfiguration GetConfiguration(string corellationID = null)
		{
			return new RatesServiceConfiguration
			{
				Providers = new ProvidersConfig
				{
					CargoSphere = new CargoSphereConfig
					{
						ServiceUrl = "CargoSphere/ServiceUrl",
						SiteSearchUrl = "CargoSphere/SiteSearchUrl",
						SiteSudsUrl = "CargoSphere/SiteSudsUrl"
					},
					Cargoguide = new CargoguideConfig
					{
						ServiceUrl = "Cargoguide/ServiceUrl",
						SiteSearchUrl = "Cargoguide/SiteSearchUrl"
					}
				}
			};
		}

		public Task<RatesServiceConfiguration> GetConfigurationAsync(string correlationID = null)
		{
			return Task.FromResult(GetConfiguration(correlationID));
		}

		public ChargeCodeWithMappingInfo[] GetChargeCodesWithMappingInfo(string correlationID = null)
		{
			return new[]
			{
				new ChargeCodeWithMappingInfo { Provider = WRConstants.RateProviders.CargoSphere, Code = "ABC", Description = "Code Description", ForeignCode = "AAAC",  ForeignName = "CN",  Carrier = "CNAI" },
			};
		}

		public Task<ChargeCodeWithMappingInfo[]> GetChargeCodesWithMappingInfoAsync(string correlationID = null)
		{
			return Task.FromResult(GetChargeCodesWithMappingInfo(correlationID));
		}

		public ChargeCodeWithMappingInfo[] GetAllChargeCodesWithMappingInfo(string correlationID = null)
		{
			return new[]
			{
				new ChargeCodeWithMappingInfo { Provider = WRConstants.RateProviders.CargoSphere, Code = "ABC", Description = "Code Description", ForeignCode = "AAAC",  ForeignName = "CN",  Carrier = "CNAI" },
			};
		}

		public Task<ChargeCodeWithMappingInfo[]> GetAllChargeCodesWithMappingInfoAsync(string correlationID = null)
		{
			return Task.FromResult(GetAllChargeCodesWithMappingInfo(correlationID));
		}

		public Task SendKafkaMessageAsync(IEnumerable<string> kafkaMessages)
		{
			throw new NotImplementedException();
		}

		public HashSet<string> GetNamedAccounts(string correlationID = null)
		{
			return new HashSet<string>
			{
				"Test Account",
				"Nike",
				"Nike Inc.",
			};
		}
	}
}

#endregion
