using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Business.Registry;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WTG.Foundation.Http;

namespace Enterprise.Telematics.ServiceTasks.GpsRoadType
{
	class RoadTypeCalculator : IRoadTypeCalculator, IDisposable
	{
		readonly AddressValidationServiceUri avsUri;

		public RoadTypeCalculator(IHttpClientFactory httpClientFactory, ILogger logger)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			avsUri = AddressValidationServiceUrisProvider.GetAddressValidationServiceUris().Primary;
			httpClient = GetHttpClient(httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory)));
			avsHelper = new AddressValidationServiceHelper();
		}

		public void Dispose()
		{
			httpClient.Dispose();
		}

		public void MarkLocationRoadType(IEnumerable<GlbDeviceLocation> locations, CancellationToken cancellationToken)
		{
			if (TelematicsConfigurationRegistry.Instance.ProcessGlobalPositionDataOnRoadType.Value)
			{
				MarkLocationRoadTypeBasedOnEndpointResponse(locations, cancellationToken);
			}
			else
			{
				MarkLocationsPublic(locations, cancellationToken);
			}
		}

		void MarkLocationsPublic(IEnumerable<GlbDeviceLocation> locations, CancellationToken cancellationToken)
		{
			foreach (var location in locations)
			{
				location.V2_RoadType = GlbDeviceLocationRoadTypes.Codes.Public;
			}
		}

		void MarkLocationRoadTypeBasedOnEndpointResponse(IEnumerable<GlbDeviceLocation> locations, CancellationToken cancellationToken)
		{
			var locationsList = locations.ToList();
			var message = JsonConvert.SerializeObject(locationsList
				.Select(location => new Dictionary<string, double?>()
				{
					{ (NoResString)"Latitude", location.Location.Latitude },
					{ (NoResString)"Longitude", location.Location.Longitude }
				}));

			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}

			var content = new StringContent(message, Encoding.UTF8, "application/json");
			httpClient.DefaultRequestHeaders.Authorization = avsHelper.GetAuthenticationHeaderValue(avsUri.EnableS2STAuth);
			using (var response = httpClient
				.PostAsync((NoResString)"land-parcel/status-check", content, cancellationToken)
				.GetAwaiter()
				.GetResult())
			{
				if (CheckStatusCode(response.StatusCode))
				{
					var results = ConvertResponseToList(response, locationsList);
					foreach (var (location, i) in locationsList.Select((value, i) => (value, i)))
					{
						SetLocationStatus(results[i], location);
					}
				}
			}
		}

		bool CheckStatusCode(HttpStatusCode status)
		{
			if ((int)status >= 200 && (int)status < 300)
			{
				return true;
			}
			if ((int)status >= 500 && (int)status < 600)
			{
				logger.Log(LogType.Error, $"Encountered Server error trying to post, status code is: ({(int)status})");
				return false;
			}

			throw new HttpRequestException($"Response status code does not indicate success: {(int)status}");
		}

		void SetLocationStatus(Dictionary<string, object> landParcelResult, GlbDeviceLocation location)
		{
			try
			{
				var coordinate = (Dictionary<string, double>)((JObject)landParcelResult["Coordinate"]).ToObject(typeof(Dictionary<string, double>));
				if (CoordinateEqualsLocation(coordinate, location.Location))
				{
					location.V2_RoadType = (bool)landParcelResult["IsLandParcel"]
						? GlbDeviceLocationRoadTypes.Codes.Private
						: GlbDeviceLocationRoadTypes.Codes.Public;
				}
				else
				{
					logger.Log(LogType.Error, $"Returned location does not match given location, expected: ({location.Location.Latitude}, {location.Location.Longitude}) received: ({coordinate["Latitude"]}, {coordinate["Longitude"]})");
					location.V2_RoadType = GlbDeviceLocationRoadTypes.Codes.Failed;
				}
			}
			catch (Exception exception)
			{
				location.V2_RoadType = GlbDeviceLocationRoadTypes.Codes.Failed;
				throw new GpsLocationRoadTypeDecodeException(location.Location, exception);
			}

			bool CoordinateEqualsLocation(Dictionary<string, double> retrievedCoordinate, ZGeography storedCoordinate)
			{
				return Math.Abs(retrievedCoordinate["Longitude"] - storedCoordinate.Longitude.Value) < 0.00001 &&
					Math.Abs(retrievedCoordinate["Latitude"] - storedCoordinate.Latitude.Value) < 0.00001;
			}
		}

		List<Dictionary<string, object>> ConvertResponseToList(HttpResponseMessage response, IEnumerable<GlbDeviceLocation> locations)
		{
			try
			{
				return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(
					response.Content.ReadAsStringAsync()
						.GetAwaiter()
						.GetResult());
			}
			catch (Exception exception)
				when (exception is JsonSerializationException ||
					exception is JsonReaderException)
			{
				foreach (var location in locations)
				{
					location.V2_RoadType = GlbDeviceLocationRoadTypes.Codes.Failed;
				}

				throw;
			}
		}

		HttpClient GetHttpClient(IHttpClientFactory httpClientFactory)
		{
			var client = httpClientFactory.Create(TelematicsConfigurationRegistry.Instance.PublicPrivateRoadEndpointTimeout);
			client.BaseAddress = new Uri(AddSlashSuffixIfNecessary(avsUri.Uri.Trim()));

			return client;
		}

		string AddSlashSuffixIfNecessary(string uri)
		{
			if (!uri.EndsWith("/", StringComparison.OrdinalIgnoreCase))
			{
				uri += "/";
			}

			return uri;
		}

		readonly ILogger logger;
		readonly HttpClient httpClient;
		readonly AddressValidationServiceHelper avsHelper;
	}
}
