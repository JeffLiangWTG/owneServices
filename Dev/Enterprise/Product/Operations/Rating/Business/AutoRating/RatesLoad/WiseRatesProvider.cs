using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.AutoRating.RatesLoad;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Constants;
using WiseRates.Tools.Exceptions;
using static System.FormattableString;
using static WiseRates.Api.Model.RatesSearchRequest;

namespace Enterprise.Rating.Business
{
	public sealed class WiseRatesProvider : IWiseRatesProvider
	{
		#region SuppressResourceStringsCheckRegion

		readonly IWiseRatesClientFactory wiseRatesClientFactory;
		readonly IWiseRatesQueryBuilder queryBuilder;
		readonly IWiseRatesConverter ratesConverter;
		readonly BusinessObjectFactory factory;

		/// <summary>
		/// Original logger passed in constructor
		/// </summary>
		public ILogger Logger { get; private set; }

		/// <summary>
		/// Internal logger - modified from original to add a prefix to all logs
		/// </summary>
		ILogger logger;

		public const string CargoSphereProviderCode = WRConstants.RateProviders.CargoSphere;
		public const string CargoGuideProviderCode = WRConstants.RateProviders.CargoGuide;

		public WiseRatesProvider(BusinessObjectFactory factory, IWiseRatesClientFactory clientFactory, ILogger logger)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(clientFactory, nameof(clientFactory));

			ReconfigureLogger(logger);

			this.wiseRatesClientFactory = clientFactory;
			this.queryBuilder = new WiseRatesQueryBuilder(this.logger);
			this.ratesConverter = new WiseRatesConverter(factory, this.logger);
			this.factory = factory;
		}

		public string LastRawResponse { get; private set; }

		bool CanAccess(RatingCriteria criteria)
		{
			if (criteria.ConsumerType == null || !criteria.ConsumerType.SupportsWiseRates)
			{
				logger.Information(Invariant($"No Search Request is sent to Rates Service as Rates Service is NOT supported for {criteria.ConsumerType?.Description}"));
				return false;
			}

			if (criteria.GatewayConfiguration.IsGatewayShipment)
			{
				logger.Information(Invariant($"No Search Request is sent to Rates Service for Gateway shipments"));
				return false;
			}

			return true;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		public IEnumerable<IRateEntry> GetCostRateEntries(RatingCriteria criteria)
		{
			if (criteria.IsServicesOnly)
			{
				return Enumerable.Empty<IRateEntry>();
			}

			if (!CanAccess(criteria))
			{
				return Enumerable.Empty<IRateEntry>();
			}

			var (ratesQuery, error) = queryBuilder.Build(criteria);
			if (!error.IsNullOrEmpty())
			{
				logger.Warning(Invariant($"Searching from Rates Service cannot proceed - {error}"));
				return Enumerable.Empty<IRateEntry>();
			}

			if (ratesQuery.TransportMode.Any(x => x == WRConstants.TransportModes.SEA))
			{
				queryBuilder.AddMeasureChargeableVolume(ratesQuery, criteria);
			}

			var convertedEntries = GetRates(ratesQuery, criteria, Operation.Autorating, out var response);

			LogInvalidEntries(convertedEntries, Logger);

			var filteredEntries = FilterByReservedRates(convertedEntries, criteria, logger);
			var validEntries = filteredEntries
				.Where(c => c.IsValidRate())
				.Cast<IRateEntry>();

			return RateEntryFilter.Filter(criteria, true, validEntries, factory, logger);
		}

		internal static IEnumerable<WiseEntry> FilterByReservedRates(IEnumerable<WiseEntry> entries, RatingCriteria criteria, ILogger logger)
		{
			if (entries.Count() > 1 && !string.IsNullOrWhiteSpace(criteria?.JobID))
			{
				var reservedRates = entries.Where(x => x.ReservedForJobIDs != null && x.ReservedForJobIDs.Contains((string)criteria.JobID));
				if (reservedRates.Any())
				{
					logger.Information(ZString.Format("{0} converted rate(s) including {1} reserved. The reserved rate(s) will take priority and the rest will be filtered", entries.Count(), reservedRates.Count()));
					return reservedRates;
				}
			}

			return entries;
		}

		public static void LogInvalidEntries(IEnumerable<WiseEntry> entries, ILogger logger, bool isUrs = false)
		{
			var invalidRates = entries.Where(e => !e.IsValidRate()).ToArray();
			if (!invalidRates.Any())
			{
				return;
			}

			var msg = new StringBuilder();
			var serviceName = isUrs ? "URS" : "Rates Service";
			msg.AppendLine($"{invalidRates.Length} Entries from {serviceName} failed to convert. Reasons:");

			var reasons = invalidRates
				.SelectMany(r => r.ErrorsIncludingChildren)
				.Distinct()
				.ToList();

			foreach (var reason in reasons)
			{
				msg.AppendLine(reason);
			}

			logger.Warning(msg.ToString());
		}

		public WiseRatesSearchRequestAsync BeginGetRawRates(RatingCriteria criteria, RatesQuery ratesQuery, int pageID)
		{
			if (!CanAccess(criteria))
			{
				return null;
			}

			return BeginSearchRequest(ratesQuery, pageID, Operation.Autorating);
		}

		void LogWarnings(RatesSearchResponseDTO response)
		{
			if (response.RatesSearchResponse.Warnings.Any())
			{
				logger.Warning(ZString.Format("Rates Service encountered following problems: {0}{1}", System.Environment.NewLine, response.RatesSearchResponse.Warnings.ToStringWithNewLineBetweenStrings()));
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "ConvertRates is called along this method every time so just move it inside. Eventually we will remove the response.")]
		public IEnumerable<WiseEntry> GetRates(RatesQuery ratesQuery, RatingCriteria criteria, Operation contextOperation, out RatesSearchResponseDTO response)
		{
			response = SendRatesRequest(ratesQuery, contextOperation);
			if (response?.RatesSearchResponse == null)
			{
				return Array.Empty<WiseEntry>();
			}

			LogWarnings(response);

			return ConvertRates(response, criteria);
		}

		//ToDo: remove CA1031 suppression
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		internal RatesSearchResponseDTO SendRatesRequest(RatesQuery ratesQuery, Operation contextOperation)
		{
			var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DataRegistryRating.Instance.RatesServiceRateSearchRequestTimeout.Value));
			var traceId = WiseRatesClient.GenerateTraceID();

			logger.Information(Invariant($"{LogMessages.RequestingAccessTokenMessage}: RequestID: {traceId}"));

			var (client, failureMessage) = wiseRatesClientFactory.TryCreate(traceId, cancellationToken: cts.Token, logger: Logger);
			if (client == null)
			{
				logger.Warning(failureMessage);
				cts.Dispose();
				return null;
			}

			try
			{
				using (client)
				{
					var queryLog = ZString.Format(@"Searching for costs on Rates Service with the following filter:
{0}", ratesQuery.ToYAML());
					logger.Debug(queryLog);

					var request = new RatesSearchRequest
					{
						RatesQuery = ratesQuery,
						ContextOperation = contextOperation,
						DiagnosticSettings = new DiagnosticSettings
						{
							IncludeRawResponses = DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.Value
						}
					};

					var response = client.SearchAsync(request, traceId, cts.Token).GetAwaiter().GetResult();

					LogFound(response);

					if (response.Warnings.Any())
					{
						var warningsWithCorrelationId = response.Warnings.ToList();
						warningsWithCorrelationId.Add(ZString.Format("Correlation ID: {0}", traceId));

						response.Warnings = warningsWithCorrelationId.ToArray();
					}

					return new RatesSearchResponseDTO
					{
						RatesSearchResponse = response,
						RawResponse = client.LastRequest ?? response.ToJSON(),
						TraceID = traceId
					};
				}
			}
			catch (HttpRequestException ex)
			{
				logger.Warning(UnableToConnectToRatesServiceMessage);
				logger.Debug(GetErrorMessage(traceId, ex));

				ex.NotifyUserIfRequired(client.ServiceURL);
			}
			catch (HttpResponseException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
			{
				logger.Warning(Res.GetString("a8339e73-f8f4-4d23-9a6d-72385ed2ec90", "The service doesn't authorize current CW1 system")); // for logging
			}
			catch (OperationCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
			{
				logger.Warning(SendRatesRequestTimeoutMessage);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Warning(SendRatesRequestErrorMessage);

				var errorMessage = GetErrorMessage(traceId, ex);
				logger.Debug(errorMessage);
				ErrorReporter.ReportOnce(errorMessage, ex);
			}
			finally
			{
				LastRawResponse = client.LastRequest;
				cts.Dispose();
			}

			return new RatesSearchResponseDTO
			{
				RatesSearchResponse = null,
				RawResponse = client.LastRequest,
				TraceID = traceId
			};
		}

		public static string GetErrorMessage(string corrolationID, Exception exception) =>
			Invariant($@"An error occurred when connecting Rates Service:
Occured Exception:
{exception},

Configuration:
  Correlation ID: {corrolationID}
  Rates Service Subscription:
  {GetRatesServiceRegistrySettings(DataRegistryRating.Instance.RatesServiceSubscription)}
  Rates Service Url: {RatingDataRegistry.Instance.RatesServiceUrl.Value},
  Auth Service Url: {RatingDataRegistry.Instance.WTGAuthServiceUrl.Value},
  CargoSphere Integration Enabled: {DataRegistryRating.Instance.CargoSphereIntegrationEnabled.Value},
  CargoSphere Rate Search Url: {DataRegistryRating.Instance.CargoSphereRateSearchUrl.Value},
  Cargiguide Integration Enabled: {DataRegistryRating.Instance.CargoguideIntegrationEnabled.Value},
  Cargiguide Rate Search Url: {DataRegistryRating.Instance.CargoguideRateSearchUrl.Value},
  Rates Service Rate Selector:
  {GetRatesServiceRegistrySettings(DataRegistryRating.Instance.RatesServiceRateSelector)}");

		static string GetRatesServiceRegistrySettings(RatesServiceSettingsRegistryItem registryItem)
		{
			var settings = registryItem.Value.Cast<RatesServiceRegistrySettings>()
				.Select(x => Invariant($"{x.TransportMode} : {x.ContainerMode} : {x.IsSubscriptionEnabled}"))
				.ToArray();

			return string.Join(Invariant($",{System.Environment.NewLine}  "), settings);
		}

		WiseRatesSearchRequestAsync BeginSearchRequest(RatesQuery ratesQuery, int pageID, Operation contextOperation)
		{
			var requestID = WiseRatesClient.GenerateTraceID();

			logger.Information(Invariant($"{LogMessages.RequestingAccessTokenMessage}: RequestID: {requestID}"));

			var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DataRegistryRating.Instance.RatesServiceRateSearchRequestTimeout.Value));
			(var client, var failureMessage) = wiseRatesClientFactory.TryCreate(requestID, cancellationToken: cts.Token, logger: logger);

			if (client == null)
			{
				logger.Warning(failureMessage);
				cts.Dispose();
				return null;
			}

			var queryLog = ZString.Format(@"Searching for costs with the following filter:
{0}", ratesQuery.ToYAML());
			logger.Debug(queryLog);

			var request = new RatesSearchRequest
			{
				RatesQuery = ratesQuery,
				PageID = pageID,
				ContextOperation = contextOperation,
				DiagnosticSettings = new DiagnosticSettings
				{
					IncludeRawResponses = DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.Value
				}
			};

			logger.Debug(Invariant($"{LogMessages.SendingRequestToRatesServiceMessage}: Request = \'{request.RatesQuery}\'"));
			logger.Information(Invariant($"{LogMessages.SendingRequestToRatesServiceMessage}:  RequestID = {requestID}"));

			var result = new WiseRatesSearchRequestAsync(client, request, requestID, cts);
			result.BeginRequest();
			return result;
		}

		public RatesSearchResponseDTO EndGetRawRates(WiseRatesSearchRequestAsync request)
		{
			if (request == null)
			{
				return null;
			}

			var result = request.CachedResponseDTO;
			if (result != null)
			{
				return result;
			}

			try
			{
				var response = request.GetResponse();

				LogFound(response);

				if (response.Warnings.Any())
				{
					var warningsWithCorrelationId = response.Warnings.ToList();
					warningsWithCorrelationId.Add(ZString.Format("Correlation ID: {0}", request.RequestID));

					response.Warnings = warningsWithCorrelationId.ToArray();
				}

				result = new RatesSearchResponseDTO
				{
					RatesSearchResponse = response,
					RawResponse = request.Client.LastRequest,
					TraceID = request.RequestID
				};

				LogWarnings(result);
				return result;
			}
			catch (HttpRequestException ex)
			{
				logger.Warning(UnableToConnectToRatesServiceMessage);
				logger.Debug(GetErrorMessage(request.RequestID, ex));

				ex.NotifyUserIfRequired(request.Client.ServiceURL);
			}
			catch (OperationCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
			{
				logger.Warning(SendRatesRequestTimeoutMessage);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Warning(SendRatesRequestErrorMessage);

				var errorMessage = GetErrorMessage(request.RequestID, ex);
				logger.Debug(errorMessage);
				ErrorReporter.ReportOnce(errorMessage, ex);
			}

			return new RatesSearchResponseDTO
			{
				RatesSearchResponse = null,
				RawResponse = request.Client.LastRequest,
				TraceID = request.RequestID
			};
		}

		public static ResourceString UnableToConnectToRatesServiceMessage => ResString.GetMultilingualString("452E6B6E-C795-4364-9C65-3983B76471B1", "Unable to connect to Rates Service. Please try again later. If problem still exists, please contact your system administrator.");
		public static ResourceString SendRatesRequestErrorMessage => ResString.GetMultilingualString("EDA25C98-B251-45F7-960C-136FD145FEF2", "Error connecting Rates Service with report(s) sent to WTG. Please raise an eRequest accordingly.");
		public static string SendRatesRequestTimeoutMessage => Res.GetString("1772D79D-D7C5-40CD-98CF-11613EE2BE7B", "Timeout connecting to Rates Service. Please try again. If problem persists, please raise an eRequest.");

		void LogFound(RatesSearchResponse response)
		{
			var groupsStr = string.Empty;
			if (response.Rates.Any())
			{
				var groupsByProvider = response.Rates.GroupBy(r => r.Provider, (x, y) => ZString.Format("{0} {1}", y.Count(), x)).ToList();

				var reservedForJobID = response.Rates.Where(r => r.ReservedForJobIDs != null && r.ReservedForJobIDs.Any(x => !string.IsNullOrWhiteSpace(x)));
				if (reservedForJobID.Any())
				{
					groupsByProvider.Add(ZString.Format("{0} reserved", reservedForJobID.Count()));
				}

				groupsStr = string.Join(", ", groupsByProvider);
			}

			logger.Information(ZString.Format("{0} entries from Rates Service found. {1}", response.Rates.Length, groupsStr).Trim());
		}

		public IEnumerable<WiseEntry> ConvertRates(RatesSearchResponseDTO dto, RatingCriteria criteria)
		{
			ConversionOptions options;
			options.AddRateModeAndCategoryValidation = true;
			var response = dto?.RatesSearchResponse;
			var traceID = dto?.TraceID ?? string.Empty;

			return ratesConverter.Convert(new WiseRatesConversionContext(response, criteria, options, traceID), response?.Rates);
		}

		public void ReconfigureLogger(ILogger newLogger)
		{
			this.Logger = newLogger;
			this.logger = newLogger.WithPrefix(ZString.Format("{0}: ", "Rates Service"));
		}

		#endregion
	}
}
