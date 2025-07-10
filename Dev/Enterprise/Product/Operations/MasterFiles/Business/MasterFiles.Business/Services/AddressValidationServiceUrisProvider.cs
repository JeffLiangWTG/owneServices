using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Registry.Business;
using Newtonsoft.Json;

namespace Enterprise.MasterFiles.Business
{
	public static class AddressValidationServiceUrisProvider
	{
		public static AddressValidationServiceUris GetAddressValidationServiceUris()
		{
			return GetAddressValidationServiceUrisFromFeatureData()
				?? GetAddressValidationServiceUrisFromRegistry();
		}

		static AddressValidationServiceUris GetAddressValidationServiceUrisFromFeatureData()
		{
			var featureData = ObjectFactory
				.Get<IFeatureControlManager>()
				.GetFeatureData(LicenceFeatureCodeList.Codes.MDMAddressValidationServicesUrls);

			if (featureData is null)
			{
				return null;
			}

			try
			{
				var avsUris = featureData.DeserializeParameterAsJson<AddressValidationServiceUris>();
				if (avsUris is null || avsUris.Primary is null || avsUris.Secondary is null || avsUris.Background is null)
				{
					throw new ArgumentException($"Feature data [{LicenceFeatureCodeList.Codes.MDMAddressValidationServicesUrls}] requires a valid parameter!");
				}

				if (IsInvalidAddressValidationServiceUri(avsUris.Primary.Uri) ||
					IsInvalidAddressValidationServiceUri(avsUris.Secondary.Uri) ||
					IsInvalidAddressValidationServiceUri(avsUris.Background.Uri))
				{
					throw new ArgumentException($"One or more AVS URIs from the feature data [{LicenceFeatureCodeList.Codes.MDMAddressValidationServicesUrls}] is invalid!");
				}

				return avsUris;
			}
			catch (Exception exception) when (exception is ArgumentException or JsonException && !exception.IsCriticalException())
			{
				ErrorReporter.ReportOnce(nameof(GetAddressValidationServiceUris), exception.Message, exception);
				return null;
			}
		}

		static bool IsInvalidAddressValidationServiceUri(string uri)
		{
			return !UrlValidation.IsValidAbsoluteHttpOrHttpsUrl(uri) || !uri.EndsWith("/");
		}

		static AddressValidationServiceUris GetAddressValidationServiceUrisFromRegistry()
		{
			var uriCollection = OrganisationsDataRegistry.Instance.AvsWebServiceURIs;
			return new AddressValidationServiceUris
			{
				Primary = new AddressValidationServiceUri
				{
					Uri = uriCollection.Primary.ServiceUri,
					EnableS2STAuth = uriCollection.Primary.EnableSystemToSystemTrustAuthentication
				},
				Secondary = new AddressValidationServiceUri
				{
					Uri = uriCollection.Secondary.ServiceUri,
					EnableS2STAuth = uriCollection.Secondary.EnableSystemToSystemTrustAuthentication
				},
				Background = new AddressValidationServiceUri
				{
					Uri = uriCollection.Background.ServiceUri,
					EnableS2STAuth = uriCollection.Background.EnableSystemToSystemTrustAuthentication
				}
			};
		}
	}
}
