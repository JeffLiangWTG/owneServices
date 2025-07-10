using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class AirBookingUriProvider
	{
		static string GetBaseUrl()
		{
			if (!string.IsNullOrEmpty(FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.Value))
			{
				return FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.Value.TrimEnd('/'); // programmatic constant;
			}
			return FreightDataRegistry.Instance.EBookingServicesApiUrl.Value.SelectedUrl.TrimEnd('/'); // programmatic constant
		}

		public static (Uri uri, string errorMessage) GetCarrierConfigurationUri()
		{
			var baseUrl = GetBaseUrl();
			var carriersUrl = $"{baseUrl}/carriers"; // programmatic constant
			return GetUri(baseUrl, carriersUrl);
		}

		public static (Uri uri, string errorMessage) GetBookingUri(string mAwb)
		{
			var baseUrl = GetBaseUrl();
			var cancellationUrl = string.Concat(baseUrl, '/', (NoResString)"booking", '/', mAwb); // programmatic constant
			return GetUri(baseUrl, cancellationUrl);
		}

		public static (Uri uri, string errorMessage) GetCancellationUri(string mAwb)
		{
			var baseUrl = GetBaseUrl();
			var cancellationUrl = string.Concat(baseUrl, '/', (NoResString)"booking", '/', mAwb, '/', (NoResString)"cancel"); // programmatic constant
			return GetUri(baseUrl, cancellationUrl);
		}

		static (Uri, string) GetUri(string baseUrl, string serviceUriString)
		{
			if (string.IsNullOrWhiteSpace(baseUrl))
			{
				return (null, Res.GetString("26b09fd4-acea-4656-baef-c982936fd23d",
					"eBooking API URL hasn't been configured in the registry. Please configure it here: {0}",
					FreightDataRegistry.Instance.EBookingServicesApiUrl.GetLocationInEnglish()));
			}

			try
			{
				return (new Uri(serviceUriString), null);
			}
			catch (UriFormatException)
			{
				return (null, Res.GetString("e2e9496b-ac7a-42d1-b341-d2e77027ff39",
						"The eBooking API URL is not formatted correctly in {0}",
						FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.GetLocationInEnglish()));
			}
		}
	}
}
