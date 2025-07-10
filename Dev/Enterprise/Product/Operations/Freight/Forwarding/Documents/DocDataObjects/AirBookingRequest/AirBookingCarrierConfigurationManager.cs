using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class AirBookingCarrierConfigurationManager
	{
		public static bool IsSupported(string airlinePrefix, out string errorMessage)
		{
			errorMessage = null;

			if (!FreightDataRegistry.Instance.EnableAirBookingCarrierConfiguration.Value)
			{
				return true;
			}

			const int airlinePrefixLength = 3;

			if (string.IsNullOrWhiteSpace(airlinePrefix)
				|| airlinePrefix.Length != airlinePrefixLength)
			{
				return false;
			}

			var airlines = IsCacheExpired
				? (supportedAirlines = GetSupportedAirlines(out errorMessage))
				: SupportedAirlines;

			if (!string.IsNullOrWhiteSpace(errorMessage))
			{
				return false;
			}

			return airlines.Keys.Contains(airlinePrefix);
		}

		public static AirlineConfig GetAirlineConfig(string airlinePrefix)
		{
			return FreightDataRegistry.Instance.EnableAirBookingCarrierConfiguration.Value
				&& SupportedAirlines.TryGetValue(airlinePrefix, out var airlineConfig)
					? airlineConfig
					: null;
		}

		public static IDictionary<string, AirlineConfig> SupportedAirlines
		{
			get
			{
				if (supportedAirlines == null
					|| IsCacheExpired)
				{
					supportedAirlines = GetSupportedAirlines(out _);
				}

				return supportedAirlines;
			}
		}

		static IDictionary<string, AirlineConfig> GetSupportedAirlines(out string errorMessage)
		{
			errorMessage = null;

			var config = GetConfigFromCache(out errorMessage);

			var res = new Dictionary<string, AirlineConfig>();

			if (config != null)
			{
				foreach (var airline in config)
				{
					res[airline.Prefix] = airline;
				}
			}

			return res;
		}

#if DEBUG
		public static void ClearSupportedAirlinesApplicationCache()
		{
			supportedAirlines = null;
		}
#endif

		[ThreadStatic]
		static IDictionary<string, AirlineConfig> supportedAirlines;

		static AirlineConfig[] GetConfigFromCache(out string errorMessage)
		{
			errorMessage = null;
			var cache = FreightDataRegistry.Instance.EBookingCarrierConfiguration.Value;

			try
			{
				var doUpdateCache = IsCacheExpired;

				string settingsJson = cache.LastResponse;

				if (doUpdateCache)
				{
					settingsJson = UpdateCache(out errorMessage) ?? settingsJson;
				}

				return JsonConvert.DeserializeObject<AirlineConfig[]>(settingsJson);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				errorMessage = ex.Message;
				return null;
			}
		}

		static bool IsCacheExpired
		{
			get
			{
				const int cacheDurationInHours = 8;

				var cache = FreightDataRegistry.Instance.EBookingCarrierConfiguration.Value;

				return cache.LastUpdatedTime.IsEmpty
					|| !cache.LastUpdatedTime.IsValid
					|| (ZDateTime.Now - cache.LastUpdatedTime.ToLocalBranchTime()).TotalHours > cacheDurationInHours;
			}
		}

		static string UpdateCache(out string errorMessage)
		{
			errorMessage = null;

			try
			{
				var json = GetCarrierConfigurationFromAPI();

				if (string.IsNullOrWhiteSpace(json))
				{
					return null;
				}

				var config = JsonConvert.DeserializeObject<AirlineConfig[]>(json);

				var settings = new EBookingCarrierConfiguration
				{
					LastUpdatedTime = ZDateTime.Now.ToUniversalBranchTime(),
					LastResponse = JsonConvert.SerializeObject(config, Formatting.Indented)
				};

				FreightDataRegistry.Instance.EBookingCarrierConfiguration.SetValue(
					Guid.Empty,
					Guid.Empty,
					Guid.Empty,
					settings);

				return json;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				errorMessage = AirBookingErrorMessageHelper.GetHumanReadableError(ex);
				return null;
			}
		}

		static int GetRequestTimeoutInSeconds()
		{
			const int defaultTimeOutInSeconds = 5;

			var res = FreightDataRegistry.Instance.EBookingCarrierConfigurationApiTimeoutInSeconds.Value;

			if (res <= 0
				|| res == int.MaxValue)
			{
				res = defaultTimeOutInSeconds;
			}

			return res;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		static string GetCarrierConfigurationFromAPI()
		{
#if DEBUG
			if (Globals.IsTest)
			{
				return null;
			}
#endif
			(var uri, var errorMessage) = AirBookingUriProvider.GetCarrierConfigurationUri();

			if (uri == null)
			{
				return null;
			}

			var timeoutInSeconds = GetRequestTimeoutInSeconds();

			const string mediaType = "application/json";
			const string basic = "Basic";

			using (var httpClient = new HttpClient())
			{
				httpClient.BaseAddress = uri;
				httpClient.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
				httpClient.DefaultRequestHeaders.Accept.Clear();
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
				if (FreightDataRegistry.Instance.IncludeAuthorizationWhenSendingEBookings.Value)
				{
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(basic, Environment.Env.CurrentCompany.GetEHubAuthValue());
				}

				var response = httpClient
					.GetAsync(uri)
					.ConfigureAwait(false)
					.GetAwaiter()
					.GetResult()
					.EnsureSuccessStatusCode();

				return response
					.Content
					?.ReadAsStringAsync()
					.ConfigureAwait(false)
					.GetAwaiter()
					.GetResult();
			}
		}
	}
}
