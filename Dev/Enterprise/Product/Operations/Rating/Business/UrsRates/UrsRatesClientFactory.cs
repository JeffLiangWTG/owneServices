#nullable enable
using System;
using System.Linq;
using System.Threading;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Urs.Api.Integration;
using Constants = Enterprise.Core.Constants;
using RateProviders = WiseRates.Constants.WRConstants.RateProviders;

namespace Enterprise.Rating.Business
{
	public class UrsRatesClientFactory : IUrsRatesClientFactory
	{
		public IUrsClient? TryCreate(string transportMode, string containerMode, string correlationID, ILogger logger, TimeSpan secondsBeforeTokenExpiry, CancellationToken cancellationToken)
			=> TryCreate(transportMode, containerMode, correlationID, logger, secondsBeforeTokenExpiry, cancellationToken, new WTGAuthTokenProviderForRating());

		/// <summary>
		/// Must be used by tests only.
		/// </summary>
		internal static IUrsClient? TryCreate(
			string transportMode,
			string containerMode,
			string correlationID,
			ILogger logger,
			TimeSpan secondsBeforeTokenExpiry,
			CancellationToken cancellationToken,
			IAuthTokenProvider authProvider)
		{
			var provider = GetProviderName(transportMode);

			if (!CheckRegistryAccess(transportMode, containerMode, logger))
			{
				return null;
			}

			if (!HasUserAnyEmailAddress && !IsUserSupport)
			{
				logger.Error(CreateMessage(Res.GetString("3b0271a3-cbdc-4f19-8974-28606f6819f8", "Email address of current user in staff details is mandatory for accessing URS rates.")));
				return null;
			}

			if (!IsRateSearchEnabledFor(provider))
			{
				logger.Warning(CreateMessage(Res.GetString("77cece30-1bd9-48a3-a9d3-dd0251a37ec8", "Request will not be sent to Rates Service because {0} integration is disabled in the registry.", provider)));
				return null;
			}

			if (!IsUserAllowedToAccess(provider))
			{
				logger.Warning(CreateMessage(Res.GetString("f95ef70a-b691-453f-a21f-960e598912d1", "{0} won't be accessed as access for current user is denied", provider)));
				return null;
			}

			var (token, validationMessage) = authProvider.GetToken(correlationID, (int)secondsBeforeTokenExpiry.TotalSeconds, cancellationToken: cancellationToken);

			if (!string.IsNullOrEmpty(validationMessage))
			{
				logger.Error(CreateMessage(validationMessage));
				return null;
			}

			if (string.IsNullOrEmpty(RatingFeatureHelper.Urs.Url))
			{
				logger.Error(CreateMessage(Res.GetString("b23424d4-3523-476e-8dbd-292b32551bb1", "The Universal Rates Service URL is not configured")));
				return null;
			}

			if (!Uri.TryCreate(RatingFeatureHelper.Urs.Url, UriKind.Absolute, out _))
			{
				logger.Error(CreateMessage(Res.GetString("90628856-cb90-490f-9047-98d52f5c9b53", "The Universal Rates Service URL in the registry ({0}) is invalid.", RatingFeatureHelper.Urs.Url)));
				return null;
			}

			return token != null ? CreateClient(token) : null;
		}

		static bool CheckRegistryAccess(string transportMode, string containerMode, ILogger logger)
		{
			var valid = true;
			if (!DataRegistryRating.Instance.IsLicenseForRatesServiceValid(out string reason))
			{
				logger.Warning(CreateMessage(reason));
				valid = false;
			}

			if (!DataRegistryRating.Instance.IsRateServiceSubscriptionEnabled(transportMode, containerMode, out reason))
			{
				logger.Warning(CreateMessage(reason));
				valid = false;
			}

			if (!RatingFeatureHelper.Urs.IsEnabled)
			{
				logger.Warning(CreateMessage(Res.GetString("1fb1f309-8f4d-4cab-ad7b-8a16a212bb2a", "The Universal Rates Service in the registry is disabled.")));
				valid = false;
			}

			return valid;
		}

		static bool IsRateSearchEnabledFor(string provider)
		{
			return provider switch
			{
				RateProviders.CargoGuide => DataRegistryRating.Instance.CargoguideIntegrationEnabled.Value,
				RateProviders.CargoSphere => DataRegistryRating.Instance.CargoSphereIntegrationEnabled.Value,
				_ => false
			};
		}

		static bool IsUserAllowedToAccess(string provider)
		{
			return provider switch
			{
				RateProviders.CargoGuide => Env.Security.WiseRatesCargoguideRateSearch.IsAllowed,
				RateProviders.CargoSphere => Env.Security.WiseRatesCargoSphereRateSearch.IsAllowed,
				_ => false
			};
		}

		static string GetProviderName(string transportMode)
		{
			return transportMode switch
			{
				Constants.TransportModes.Air => RateProviders.CargoGuide,
				Constants.TransportModes.Sea => RateProviders.CargoSphere,
				_ => string.Empty
			};
		}

		static bool HasUserAnyEmailAddress => GlbStaff.CurrentUser.EmailAddresses.Any(x => !x.GSE_EmailAddress.IsEmpty);

		static bool IsUserSupport => GlbStaff.CurrentUser.IsSupportUser;

		static UrsClient CreateClient(string token)
		{
			var enableDiagnostics = DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.Value;
			return new UrsClient(RatingFeatureHelper.Urs.Url, token, enableDiagnostics);
		}

		static string CreateMessage(string innerMessage) =>
			ResString.GetMultilingualString("4b8be06e-e812-433a-9d13-bf723645ff49", "Unable to access Universal Rates Service because '{0}'", innerMessage);
	}
}
