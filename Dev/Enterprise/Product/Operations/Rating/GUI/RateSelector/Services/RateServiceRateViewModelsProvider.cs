using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.Business.WiseRates;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.ZArchitecture.Core;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Tools.Exceptions;
using static System.FormattableString;

namespace Enterprise.Rating.GUI.RateSelector.Services
{
	public class RateServiceRateViewModelsProvider : IRateViewModelsProvider
	{
		public RateServiceRateViewModelsProvider(IWiseRatesClientFactory ratesServiceProvider, IDialogService dialogService, MemoryLogger logger)
		{
			RatesServiceProvider = ratesServiceProvider;
			Logger = logger;
			this.DialogService = Argument.NotNull(dialogService, nameof(dialogService));
		}

		public RateProviderType ProviderType => RateProviderType.RatesService;
		
		public async Task<RateViewModelsProviderResult> GetRatesAsync(RateSelectorFilterStripBusinessObject filter, CancellationToken cancellationToken = default)
		{
			var stopwatch = Stopwatch.StartNew();

			try
			{
				// LoadRatesAsync may internally execute in a separate thread but once it returns
				// to this function it should execute in the main thread so that the factory and
				// bizo's can be owned by the main thread.
				var response = await LoadRatesAsync(filter, cancellationToken);
				if (response != null)
				{
					var factory = new ReadOnlyBusinessObjectFactory();
					var context = new RateSelectorContext
					{
						Factory = factory,
						Filters = filter,
						Logger = Logger,
						RatesServiceResponse = response.RatesSearchResponse,
						RatesServiceTraceID = response.TraceID,
						CurrencyConverter = new RefCurrenciesCurrencyConverter(factory),
						DialogService = DialogService
					};

					var ratesViewModel = response.RatesSearchResponse.Rates
						.Select(r => new CargoguideRateViewModel(r, context))
						.Where(x => x != null)
						.Cast<RateViewModel>()
						.ToList();

					return new RateViewModelsProviderResult
					{
						Rates = ratesViewModel,
						ProviderType = ProviderType,
						ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
					};
				}

				return new RateViewModelsProviderResult
				{
					Rates = Enumerable.Empty<RateViewModel>(),
					ProviderType = ProviderType,
					ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
				};
			}
			finally
			{
				stopwatch.Stop();
			}
		}

		public bool IsApplicable(RateSelectorFilterStripBusinessObject filter) => true;

		CancellationToken GetCancellationTokenWithRateServiceTimeout()
		{
			var rateServiceTimeout = DataRegistryRating.Instance.RatesServiceRateSearchRequestTimeout.Value;
			var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(rateServiceTimeout));

			return cancellationTokenSource.Token;
		}

		async Task<RatesSearchResponseDTO> LoadRatesAsync(RateSelectorFilterStripBusinessObject filter, CancellationToken cancellationToken)
		{
			if (!RatesServiceClient.CargoguideRateSearchAllowed(RatesSearchRequest.Operation.Autorating, GetLogger()))
			{
				return null;
			}

			var (ratesQuery, _) = filter.BuildRatesQuery(GetLogger());
			if (ratesQuery == null)
			{
				return null;
			}

			var request = new RatesSearchRequest
			{
				RatesQuery = ratesQuery,
				ContextOperation = RatesSearchRequest.Operation.Autorating,
				DiagnosticSettings = new DiagnosticSettings
				{
					IncludeRawResponses = DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.Value
				},
			};

			var traceID = WiseRatesClient.GenerateTraceID();

			var (client, failureMessage) = RatesServiceProvider.TryCreate(traceID, 30, cancellationToken, logger: Logger);
			if (client == null)
			{
				GetLogger()?.Error(ResString.GetMultilingualString("dffc10db-a5f0-4d27-92dc-29eacfcf797c", "Unable to access Rates Service because '{0}'", failureMessage));
				return null;
			}

			var timeoutCancellationToken = GetCancellationTokenWithRateServiceTimeout();
			var combinedCancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(timeoutCancellationToken, cancellationToken).Token;

			try
			{
				GetLogger()?.Debug(Invariant($"Request rates from Rates Service: ServiceAddress = {client.ServiceURL}"));   // Just a log string
				GetLogger()?.Debug(Invariant($"Request rates from Rates Service: Query Info = {request.RatesQuery.ToJsonSafe()}"));   // Just a log string
				GetLogger()?.Information(Invariant($"Request rates from Rates Service: TraceID = {traceID}"));   // Just a log string
				var response = await client.SearchAsync(request, traceID, combinedCancellationTokens).ConfigureAwait(false);
				GetLogger()?.Information(Invariant($"Received {response?.Rates?.Length ?? 0} rates"));    // Just a log string
				LogHBLRIfPresent(response, GetLogger());

				if (response?.Warnings != null)
				{
					foreach (var msg in response.Warnings)
					{
						GetLogger()?.Warning(msg);
					}
				}

				return new RatesSearchResponseDTO()
				{
					RatesSearchResponse = response,
					TraceID = traceID
				};
			}
			catch (HttpRequestException ex)
			{
				GetLogger()?.Error(ex.Message);
				ex.NotifyUserIfRequired(client.ServiceURL);
			}
			catch (HttpResponseException ex)
			{
				GetLogger()?.Error(Invariant($"{(int)ex.StatusCode} - {ex.StatusCode}: {ex.Message}")); // for logging
			}
			catch (OperationCanceledException) when (timeoutCancellationToken.IsCancellationRequested)
			{
				GetLogger()?.Warning(ResString.GetMultilingualString("84c23f87-437d-46de-b73b-1ef84df8af43", "Timeout connecting to Rates Service. Please try again. If problem persists, please raise an eRequest."));
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				// Cancelled by the caller to this method.
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (ObjectFactory.Get<Enterprise.Integration.Licensing.IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Production)
				{
					ErrorReporter.ReportOnce("RateServiceRateViewModelsProvider_GetRatesAsync|Error",
						Invariant($"Failed to get rates from Rates Service with unexpected error: {ex.Message}"), ex);
				}
			}

			return null;

			MemoryLogger GetLogger() => cancellationToken.IsCancellationRequested ? null : Logger;
		}

		void LogHBLRIfPresent(RatesSearchResponse response, MemoryLogger logger)
		{
			var hasHBLR = response?.Rates?.Any(r => r.Charges.Any(c => c.IsHigherBreakLowerRate)) ?? false;
			if (hasHBLR)
			{
				logger?.Information((NoResString)"One or more rates have HBLR applied");
			}
		}

		readonly MemoryLogger Logger;
		readonly IDialogService DialogService;
		readonly IWiseRatesClientFactory RatesServiceProvider;
	}
}
