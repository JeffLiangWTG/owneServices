using System;
using System.Threading;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.Rating.Business.WiseRates;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using WiseRates.Api.Client;

namespace Enterprise.Rating.Business
{
	public class WiseRatesClientFactory : IWiseRatesClientFactory
	{
		public WiseRatesClientFactory()
			: this(new WTGAuthTokenProviderForRating())
		{
		}

		/// <summary>
		///		Must be sued by tests only.
		/// </summary>
		internal WiseRatesClientFactory(IAuthTokenProvider authTokenProvider)
		{
			AuthTokenProvider = Argument.NotNull(authTokenProvider, nameof(authTokenProvider));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public (IWiseRatesClient client, string failureMessage) TryCreate(
			string correlationID,
			int secondsBeforeTokenExpiry = 30,
			CancellationToken cancellationToken = default,
			ILogger logger = null)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				return (new WiseRatesClientMock(), string.Empty);
			}
#endif
			var (token, reason) = CheckAccess(correlationID, secondsBeforeTokenExpiry);
			if (!string.IsNullOrEmpty(reason))
			{
				return (null, reason);
			}

			var client = CreateClient(logger, token);
			return (client, string.Empty);
		}

		static RatesServiceClient CreateClient(ILogger realLogger, string token)
		{
			var enableDiagnostics = DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.Value;
			var wiseRatesClient = new WiseRatesClientWithCache(RatingDataRegistry.Instance.RatesServiceUrl.Value, token, enableDiagnostics: enableDiagnostics);
			var ursClient = new UrsWiseRatesClient(RatingFeatureHelper.Urs.Url, token, wiseRatesClient, realLogger, enableDiagnostics: enableDiagnostics);

			// If URS integration is enabled, we will send requests to both - Rates Service and URS to get results
			// from both and compare them, to make sure that URS integration is correlated with Rates Service integration.
			var client = RatingFeatureHelper.Urs.IsEnabledForLegacy
				? new RatesServiceClient(new UrsWiseRatesClientWithRatesComparison(wiseRatesClient, ursClient))
				: new RatesServiceClient(wiseRatesClient);
			return client;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		internal (string, string) CheckAccess(string correlationID, int secondsBeforeTokenExpiry)
		{
			if (!DataRegistryRating.Instance.IsLicenseForRatesServiceValid(out string reason))
			{
				return (null, reason);
			}

			if (!DataRegistryRating.Instance.IsRateServiceSubscriptionEnabled(out reason))
			{
				return (null, reason);
			}

			var (token, validationMessage) = AuthTokenProvider.GetToken(correlationID, secondsBeforeTokenExpiry);

			if (!string.IsNullOrEmpty(validationMessage))
			{
				return (null, validationMessage);
			}

			if (string.IsNullOrEmpty(RatingDataRegistry.Instance.RatesServiceUrl.Value))
			{
				return (null, Res.GetString("aca71582-4e7e-11e8-abcf-1c1b0d09faa1", "The Rates Service URL is not configured"));
			}

			if (!Uri.TryCreate(RatingDataRegistry.Instance.RatesServiceUrl.Value, UriKind.Absolute, out Uri serviceUri))
			{
				return (null, Res.GetString("de1d7105-95e5-417b-a38c-02f155a72a9b", "The Rates Service URL in the registry ({0}) is invalid.", RatingDataRegistry.Instance.RatesServiceUrl.Value));
			}

			if (RatingFeatureHelper.Urs.IsEnabledForLegacy)
			{
				if (string.IsNullOrEmpty(RatingFeatureHelper.Urs.Url))
				{
					return (null,
						Res.GetString("bcc7da74-6faa-4f95-8079-9c5244f2249a",
							"The Universal Rates Service URL is not configured"));
				}

				if (!Uri.TryCreate(RatingFeatureHelper.Urs.Url, UriKind.Absolute, out Uri ursServiceUri))
				{
					return (null,
						Res.GetString("19cc7745-fa5a-4d5d-b63b-c7e81f164204",
							"The Universal Rates Service URL in the registry ({0}) is invalid.", RatingFeatureHelper.Urs.Url));
				}
			}

			return (token, string.Empty);
		}

		public IAuthTokenProvider AuthTokenProvider { get; }
	}
}
