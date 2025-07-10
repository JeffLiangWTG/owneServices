#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Urs.Api.Integration;
using Urs.Api.Integration.DTOs;
using Urs.Api.Integration.DTOs.Request;
using WiseRates.Tools.Exceptions;
using static System.FormattableString;

namespace Enterprise.Rating.Business;

public sealed class UrsRatesProvider(ILogger logger, IUrsRatesClientFactory ursRatesClientFactory, BusinessObjectFactory factory) : IUrsRatesProvider
{
	public string LastRawResponse { get; private set; } = string.Empty;

	public ILogger Logger { get; private set; } = logger;

	static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
	static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(DataRegistryRating.Instance.RatesServiceRateSearchRequestTimeout.Value);

	public IEnumerable<IRateEntry> GetCostRateEntries(RatingCriteria criteria)
	{
		if (criteria.IsServicesOnly)
		{
			return [];
		}

		var requestID = GenerateRequestID();
		Logger.Information(Invariant($"{LogMessages.RequestingAccessTokenMessage}: RequestID: {requestID}"));

		using var cts = new CancellationTokenSource(RequestTimeout);

		var (ursRequest, transportMode) = BuildRatesQuery(criteria);
		if (ursRequest is null)
		{
			return [];
		}

		var queryLog = ZString.Format((NoResString)"Searching for costs on URS with the following filter:{0}{1}", System.Environment.NewLine, ursRequest.ToYAML());
		Logger.Debug(queryLog);

		var ursClient = CreateUrsClient(transportMode, criteria.ContainerMode, requestID, cts.Token);
		if (ursClient is null)
		{
			return [];
		}

		var tradeServices = FetchTradeServices(ursClient, ursRequest, requestID, cts.Token);
		if (tradeServices is null)
		{
			return [];
		}

		return ProcessTradeServices(tradeServices, criteria, requestID);
	}

	(QueryRequestDto? ursRequest, string transportMode) BuildRatesQuery(RatingCriteria criteria)
	{
		var queryBuilder = new UrsRatesQueryBuilder(Logger);
		var (ratesQuery, error) = queryBuilder.Build(criteria);

		if (!string.IsNullOrEmpty(error))
		{
			Logger.Warning(Invariant($"Searching from URS cannot proceed - {error}"));
			return (null, string.Empty);
		}

		return (ratesQuery, queryBuilder.TransportMode);
	}

	IUrsClient? CreateUrsClient(string transportMode, string containerMode, string requestID, CancellationToken cancellationToken)
	{
		return ursRatesClientFactory.TryCreate(transportMode, containerMode, requestID, Logger, DefaultTimeout, cancellationToken);
	}

	IEnumerable<TradeServiceDto>? FetchTradeServices(IUrsClient ursClient, QueryRequestDto ratesQuery, string requestID, CancellationToken cancellationToken)
	{
		try
		{
			var tradeServices = ursClient.GetTradeServicesAsync(ratesQuery, requestID, cancellationToken).GetAwaiter().GetResult();
			LastRawResponse = ursClient.LastRequest ?? tradeServices.ToJSON();

			Logger.Information(ZString.Format((NoResString)"{0} entries from URS found.", tradeServices.Count()));

			return tradeServices;
		}
		catch (HttpRequestException ex)
		{
			HandleHttpRequestException(ex, ursClient, requestID);
			return null;
		}
		catch (HttpResponseException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized)
		{
			Logger.Warning(Res.GetString("9fcd9953-afa9-43a7-8dba-2742e11412c5", "The service doesn't authorize current CW1 system"));
			return null;
		}
		catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
		{
			Logger.Warning(Res.GetString("ef590207-ef35-45f9-86e0-64a0e20a261a", "Timeout connecting to URS. Please try again. If problem persists, please raise an eRequest."));
			return null;
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			HandleGeneralException(ex, requestID);
			return null;
		}
		finally
		{
			LastRawResponse = string.IsNullOrEmpty(LastRawResponse)
				? ursClient.LastRequest ?? string.Empty
				: LastRawResponse;
		}
	}

	void HandleHttpRequestException(HttpRequestException ex, IUrsClient ursClient, string requestID)
	{
		Logger.Warning(Res.GetString("6afe5b1f-3499-4d0b-b53e-2f410f159332", "Unable to connect to URS. Please try again later. If problem still exists, please contact your system administrator."));
		Logger.Debug(GetErrorMessage(requestID, ex));
		ex.NotifyUserIfRequired(((UrsClient)ursClient).ServiceUrl);
	}

	void HandleGeneralException(Exception ex, string requestID)
	{
		Logger.Warning(Res.GetString("1dd75f9d-2e45-461c-9753-e15bf456069f", "Error connecting URS with report(s) sent to WTG. Please raise an eRequest accordingly."));
		var errorMessage = GetErrorMessage(requestID, ex);
		Logger.Debug(errorMessage);
		ErrorReporter.ReportOnce(errorMessage, ex);
	}

	IEnumerable<IRateEntry> ProcessTradeServices(IEnumerable<TradeServiceDto> tradeServices, RatingCriteria criteria, string requestID)
	{
		var convertedEntries = ConvertServicesToRateEntries(tradeServices, criteria, requestID);
		WiseRatesProvider.LogInvalidEntries(convertedEntries.Cast<WiseEntry>(), Logger, isUrs: true);

		var filteredEntries = WiseRatesProvider.FilterByReservedRates(convertedEntries.Cast<WiseEntry>(), criteria, Logger);
		var validEntries = filteredEntries
			.Where(c => c.IsValidRate())
			.Cast<IRateEntry>();

		return RateEntryFilter.Filter(criteria, true, validEntries, factory, Logger);
	}

	static string GenerateRequestID() => Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture).ToLowerInvariant();

	List<IRateEntry> ConvertServicesToRateEntries(
		IEnumerable<TradeServiceDto> tradeServices,
		RatingCriteria criteria,
		string requestID)
	{
		var ratesConverter = new UrsRatesConverter(factory, Logger, criteria, requestID);
		return ratesConverter
			.ConvertTradeServiceDtoToWiseEntries(tradeServices)
			.ToList();
	}

	static string GetErrorMessage(string correlationID, Exception exception) =>
		Invariant($"""
			An error occurred when connecting URS:
			Occurred Exception:
			{exception},

			Configuration:
			  Correlation ID: {correlationID}
			""");

	public void ReconfigureLogger(ILogger newLogger)
	{
		Logger = newLogger.WithPrefix(ZString.Format("{0}: ", (NoResString)"URS"));
	}
}
