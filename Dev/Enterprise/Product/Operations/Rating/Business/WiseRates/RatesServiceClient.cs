using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Constants;
using static WiseRates.Api.Model.RatesSearchRequest;
using RefServiceLevel = WiseRates.Api.Model.RefServiceLevel;

namespace Enterprise.Rating.Business.WiseRates
{
	public class RatesServiceClient : IWiseRatesClient
	{
		readonly IWiseRatesClient client;

		public RatesServiceClient(IWiseRatesClient client)
		{
			this.client = client ?? throw new ArgumentNullException(nameof(client));
		}

#if DEBUG
		// These properties are used in tests to determine which client is being used.
		// A bit of a hack, but it's the easiest way to determine which client is being used in current design.
		// Also, it is a temporary solution until Rates Service is retired.
		public bool IsForUrs => client is UrsWiseRatesClient || client is UrsWiseRatesClientWithRatesComparison;
		public bool IsForRatesService => client is WiseRatesClient || client is UrsWiseRatesClientWithRatesComparison;
#endif

		/// <summary>
		/// Will only send requests to the rateservice where they are allowed by the registry settings.
		/// Any already-set AcceptedProviders will be ignored and will be replaced by ones chosen in this function.
		/// </summary>
		public async Task<RatesSearchResponse> SearchAsync(RatesSearchRequest request, string correlationID = null, CancellationToken cancellationToken = default)
		{
			if (!(request.RatesQuery.TransportMode?.Any(x => !string.IsNullOrWhiteSpace(x)) ?? false))
			{
				var paramNameForTransportMode = nameof(request.RatesQuery.TransportMode);
				throw new ArgumentException("RateSearchRequest.RatesQuery.TransportMode must contain at least one non-blank item", paramNameForTransportMode);
			}
			if (!(request.RatesQuery.ContainerMode?.Any(x => !string.IsNullOrWhiteSpace(x)) ?? false))
			{
				var paramNameForContainerMode = nameof(request.RatesQuery.ContainerMode);
				throw new ArgumentException("RateSearchRequest.RatesQuery.ContainerMode must contain at least one non-blank item", paramNameForContainerMode);
			}

			var logger = new ElementaryLogger();

			PopulateProviderDetails(request, logger);

			// If a provider is disabled in the registry, there is no point to request rates it provides. For example,
			// CG is AIR rates provider, so, if CG provider is disabled in the registry, there is no point to request
			// AIR rates.
			RemoveDisabledModes(request, request.RatesQuery.AcceptedProviders);

			var allCombinationsDisabled =
				!request.RatesQuery.AcceptedProviders.Any() ||
				!request.RatesQuery.ContainerMode.Any() ||
				!request.RatesQuery.TransportMode.Any();

			if (allCombinationsDisabled)
			{
				return new RatesSearchResponse()
				{
					Warnings = logger.Warnings.ToArray()
				};
			}

			var result = await client.SearchAsync(request, correlationID, cancellationToken).ConfigureAwait(false);
			result.Warnings = result.Warnings.Union(logger.Warnings).ToArray();

			return result;
		}

		void RemoveDisabledModes(RatesSearchRequest request, IEnumerable<string> acceptedProviders)
		{
			var newTransportModes = new List<string>();

			foreach (var transportMode in request.RatesQuery.TransportMode)
			{
				if (transportMode == WRConstants.TransportModes.AIR && acceptedProviders.Contains(WRConstants.RateProviders.CargoGuide))
				{
					newTransportModes.Add(transportMode);
				}

				if (transportMode == WRConstants.TransportModes.SEA && acceptedProviders.Contains(WRConstants.RateProviders.CargoSphere))
				{
					newTransportModes.Add(transportMode);
				}
			}

			request.RatesQuery.TransportMode = newTransportModes.Distinct().ToList();

			if (!newTransportModes.Any())
			{
				request.RatesQuery.ContainerMode = new List<string>();
			}
		}

		void PopulateProviderDetails(RatesSearchRequest request, ILogger logger)
		{
			var acceptedProviders = new List<string>();
			var isAir = request.RatesQuery.TransportMode.Contains(WRConstants.TransportModes.AIR);
			var isSea = request.RatesQuery.TransportMode.Contains(WRConstants.TransportModes.SEA);

			request.ProviderAccounts = new Dictionary<string, string>();

			if (isAir && IsRateSearchAllowed(request.ContextOperation, WRConstants.RateProviders.CargoGuide, logger))
			{
				acceptedProviders.Add(WRConstants.RateProviders.CargoGuide);

				var cgDetails = GetCargoGuideProviderAccount(logger);
				if (!string.IsNullOrEmpty(cgDetails))
				{
					request.ProviderAccounts.Add(WRConstants.RateProviders.CargoGuide, cgDetails);
				}
			}

			if (isSea && IsRateSearchAllowed(request.ContextOperation, WRConstants.RateProviders.CargoSphere, logger))
			{
				acceptedProviders.Add(WRConstants.RateProviders.CargoSphere);

				var cgDetails = GetCargoSphereProviderAccount(logger);
				if (!string.IsNullOrEmpty(cgDetails))
				{
					request.ProviderAccounts.Add(WRConstants.RateProviders.CargoSphere, cgDetails);
				}
			}

			request.RatesQuery.AcceptedProviders = acceptedProviders;
		}

		string GetCargoSphereProviderAccount(ILogger logger)
		{
			var cargoSphereOverridenAPIURL = DataRegistryRating.Instance.CGSPApiURLOverride?.Value ?? "";

			if (!string.IsNullOrEmpty(cargoSphereOverridenAPIURL) && !Uri.IsWellFormedUriString(cargoSphereOverridenAPIURL, UriKind.Absolute))
			{
				logger.Warning(ZString.Format(InvalidUrl, "CargoSphere")); // Product name
				return string.Empty;
			}

			var cargoSphereLogin = (string)DataRegistryRating.Instance.CargoSphereCredentials?.Value.Login ?? "";
			var cargoSpherePassword = (string)DataRegistryRating.Instance.CargoSphereCredentials?.Value.Password ?? "";
			var cargoSphereSystemCode = (string)DataRegistryRating.Instance.CargoSphereCredentials?.Value.SystemCode ?? "";

			var sb = new ZStringBuilder(cargoSphereLogin);
			sb.Append(cargoSpherePassword);
			sb.Append(cargoSphereSystemCode);

			sb.Append(cargoSphereOverridenAPIURL);
			var accountDetailsPlainText = sb.ToStringWithNewLineBetweenAppends();
			var accountDetailsBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(accountDetailsPlainText));

			return accountDetailsBase64;
		}

		string GetCargoGuideProviderAccount(ILogger logger)
		{
			var cargoGuideOverridenApiURL = DataRegistryRating.Instance.CGGDApiSettings?.Value.ApiURL ?? "";

			if (!cargoGuideOverridenApiURL.IsEmpty && !Uri.IsWellFormedUriString(cargoGuideOverridenApiURL, UriKind.Absolute))
			{
				logger.Warning(ZString.Format(InvalidUrl, (NoResString)"Cargoguide")); // product name
				return string.Empty;
			}

			var cargoGuideLogin = (string)DataRegistryRating.Instance.CargoguideCredentials?.Value.Login ?? "";
			var cargoGuidePassword = (string)DataRegistryRating.Instance.CargoguideCredentials?.Value.Password ?? "";
			var cargoGuideOverridenApiVersion = DataRegistryRating.Instance.CGGDApiSettings?.Value.ApiVersion ?? "";

			var sb = new ZStringBuilder(cargoGuideLogin);
			sb.Append(cargoGuidePassword);
			sb.Append(cargoGuideOverridenApiURL);
			sb.Append(cargoGuideOverridenApiVersion);

			var accountDetailsPlainText = sb.ToStringWithNewLineBetweenAppends();
			var accountDetailsBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(accountDetailsPlainText));

			return accountDetailsBase64;
		}

		#region IWiseRatesClient functions mapped to `this.client`

		public string ServiceURL => client.ServiceURL;

		public string AccessToken => client.AccessToken;
		public string LastRequest => client.LastRequest;

		public void Dispose() => client.Dispose();

		#endregion

		#region Non-critical exception safe IWiseRatesClient functions

		public RefChargeCode[] GetChargeCodes(string correlationID = null)
		{
			try
			{
				return client.GetChargeCodes(correlationID);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex, correlationID, nameof(GetChargeCodes));

				return Array.Empty<RefChargeCode>();
			}
		}

		public async Task<RefChargeCode[]> GetChargeCodesAsync(string correlationID = null)
		{
			try
			{
				return await client.GetChargeCodesAsync(correlationID).ConfigureAwait(false);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex, correlationID, nameof(GetChargeCodesAsync));

				return Array.Empty<RefChargeCode>();
			}
		}

		public RefCommodityGroup[] GetCommodityGroups(string correlationID = null)
		{
			try
			{
				return client.GetCommodityGroups(correlationID);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex, correlationID, nameof(GetCommodityGroups));

				return Array.Empty<RefCommodityGroup>();
			}
		}

		public async Task<RefCommodityGroup[]> GetCommodityGroupsAsync(string correlationID = null)
		{
			try
			{
				return await client.GetCommodityGroupsAsync(correlationID).ConfigureAwait(false);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex, correlationID, nameof(GetCommodityGroupsAsync));

				return Array.Empty<RefCommodityGroup>();
			}
		}

		public RefServiceLevel[] GetServiceLevels(string correlationID = null)
		{
			try
			{
				return client.GetServiceLevels(correlationID);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex, correlationID, nameof(GetServiceLevels));

				return Array.Empty<RefServiceLevel>();
			}
		}

		public async Task<RefServiceLevel[]> GetServiceLevelsAsync(string correlationID = null)
		{
			try
			{
				return await client.GetServiceLevelsAsync(correlationID).ConfigureAwait(false);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex, correlationID, nameof(GetServiceLevelsAsync));

				return Array.Empty<RefServiceLevel>();
			}
		}

		public RatesServiceConfiguration GetConfiguration(string correlationID = null)
		{
			try
			{
				return client.GetConfiguration(correlationID);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex, correlationID, nameof(GetConfiguration));

				return null;
			}
		}

		public async Task<RatesServiceConfiguration> GetConfigurationAsync(string correlationID = null)
		{
			try
			{
				return await client.GetConfigurationAsync(correlationID).ConfigureAwait(false);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex, correlationID, nameof(GetConfigurationAsync));

				return null;
			}
		}

		public ChargeCodeWithMappingInfo[] GetChargeCodesWithMappingInfo(string correlationID = null)
		{
			try
			{
				return client.GetChargeCodesWithMappingInfo(correlationID);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex, correlationID, nameof(GetChargeCodesWithMappingInfo));

				return Array.Empty<ChargeCodeWithMappingInfo>();
			}
		}

		public async Task<ChargeCodeWithMappingInfo[]> GetChargeCodesWithMappingInfoAsync(string correlationID = null)
		{
			try
			{
				return await client.GetChargeCodesWithMappingInfoAsync(correlationID).ConfigureAwait(false);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex, correlationID, nameof(GetChargeCodesWithMappingInfoAsync));

				return Array.Empty<ChargeCodeWithMappingInfo>();
			}
		}

		public ChargeCodeWithMappingInfo[] GetAllChargeCodesWithMappingInfo(string correlationID = null)
		{
			try
			{
				return client.GetAllChargeCodesWithMappingInfo(correlationID);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex, correlationID, nameof(GetAllChargeCodesWithMappingInfo));

				return Array.Empty<ChargeCodeWithMappingInfo>();
			}
		}

		public async Task<ChargeCodeWithMappingInfo[]> GetAllChargeCodesWithMappingInfoAsync(string correlationID = null)
		{
			try
			{
				return await client.GetAllChargeCodesWithMappingInfoAsync(correlationID).ConfigureAwait(false);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex, correlationID, nameof(GetAllChargeCodesWithMappingInfoAsync));

				return Array.Empty<ChargeCodeWithMappingInfo>();
			}
		}

		public Task SendKafkaMessageAsync(IEnumerable<string> kafkaMessages)
		{
			throw new NotImplementedException();
		}

		public HashSet<string> GetNamedAccounts(string correlationID = null)
		{
			try
			{
				return client.GetNamedAccounts(correlationID);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex, correlationID, nameof(GetNamedAccounts));

				return new HashSet<string>();
			}
		}

		void HandleException(Exception ex, string correlationID, string contextKey)
		{
			if (!ex.IsCriticalException())
			{
				if (ex is HttpRequestException httpRequestException)
				{
					httpRequestException.NotifyUserIfRequired(ServiceURL);
				}
				else
				{
					if (ObjectFactory.Get<Enterprise.Integration.Licensing.IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Production)
					{
						var message = FormattableString.Invariant($@"TraceID: {correlationID ?? string.Empty}
ServiceURL: {ServiceURL}
ExceptionMessage: {ex.Message}"); // Internal Log Message

						ErrorReporter.ReportOnce(
							FormattableString.Invariant($"{contextKey}|Error"),
							message,
							ex);
					}
				}
			}
			else
			{
				throw ex;
			}
		}

		#endregion

		#region Access and security checks

		internal static bool IsRateSearchAllowed(Operation contextOperation, string rateProvider, ILogger logger = null)
		{
			rateProvider = Argument.NotNullOrEmpty(rateProvider, nameof(rateProvider));

			if (IsRateSearchEnabled(rateProvider))
			{
				var allowedBySecurityCargoguide = rateProvider == WRConstants.RateProviders.CargoGuide && Env.Security.WiseRatesCargoguideRateSearch.IsAllowed;
				var allowedBySecurityCargoSphere = rateProvider == WRConstants.RateProviders.CargoSphere && Env.Security.WiseRatesCargoSphereRateSearch.IsAllowed;
				var allowedBySecurity = allowedBySecurityCargoguide || allowedBySecurityCargoSphere;

				var allowedForAutorating = contextOperation == Operation.Autorating;
				var allowedForRatesSearch = contextOperation == Operation.WiseRateSearch && allowedBySecurity;

				if (allowedForAutorating || allowedForRatesSearch)
				{
					if (HasUserAnyEmailAddress)
					{
						return true;
					}

					logger?.Warning(ZString.Format(LogMessageEmailNotSpecified, rateProvider));
					return false;
				}

				logger?.Warning(ZString.Format(LogMessageUserNotAllowed, rateProvider));
				return false;
			}

			logger?.Warning(ZString.Format(LogMessageProviderDisabled, rateProvider));
			return false;
		}

		static bool IsRateSearchEnabled(string ratesProvider)
		{
			if (ratesProvider == WRConstants.RateProviders.CargoGuide)
			{
				return DataRegistryRating.Instance.CargoguideIntegrationEnabled.Value;
			}

			if (ratesProvider == WRConstants.RateProviders.CargoSphere)
			{
				return DataRegistryRating.Instance.CargoSphereIntegrationEnabled.Value;
			}

			return false;
		}

		public static bool CargoSphereRateSearchAllowed(Operation contextOperation, ILogger logger = null)
			=> IsRateSearchAllowed(contextOperation, WRConstants.RateProviders.CargoSphere, logger);

		public static bool CargoguideRateSearchAllowed(Operation contextOperation, ILogger logger = null)
			=> IsRateSearchAllowed(contextOperation, WRConstants.RateProviders.CargoGuide, logger);

		#endregion

		static bool HasUserAnyEmailAddress => GlbStaff.CurrentUser.EmailAddresses.Any(x => !x.GSE_EmailAddress.IsEmpty);

		static MultilingualString InvalidUrl => ResString.GetMultilingualString("830ff642-c9f7-49f7-8212-c7fe544f02f2", "{0} API URL is invalid.  Please send eRequest to WTG.");
		static MultilingualString LogMessageEmailNotSpecified => ResString.GetMultilingualString("1ffa3c45-f64e-45e3-9874-0362a72e7725", "Email address of current user in staff details is mandatory for accessing {0}");
		static MultilingualString LogMessageProviderDisabled => ResString.GetMultilingualString("2FA14599-0E3A-4C0C-982B-6323F18A5AD2", "Request will not be sent to Rates Service because {0} integration is disabled in the registry");
		static MultilingualString LogMessageUserNotAllowed => ResString.GetMultilingualString("03be3185-801a-4f85-9ede-360d4d43252b", "{0} won't be accessed as access for current user is denied");
	}
}
