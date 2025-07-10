#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.CarrierConnect;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Rating.GUI.RateSelector.Services;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.BrowserInterop;
using Newtonsoft.Json;
using static System.FormattableString;
using static Enterprise.Rating.Business.RatingUsageCollector;
using static Enterprise.Rating.CarrierConnect.RateSelection.Models.RateResultDto;

namespace Enterprise.Rating.GUI.RateSelector
{
	public class GlowRateSelector
	{
		List<RateResultDto>? lastRateSearchResults;
		List<JobUpdateDto> acceptedChanges = [];
		Guid lastRequestID;
		GlowRateSelectorResult? result;
		bool isCurrentlySearching;
		readonly Stopwatch sessionTimeStopwatch = new ();

		[SuppressMessage("ReSharper", "AsyncVoidMethod", Justification = "We use our own exception handling in PerformAction.")]
		public GlowRateSelectorResult ShowDialog(IRatingContext ratingContext, RatingCriteria criteria)
		{
			sessionTimeStopwatch.Start();
			var browserWindowFactory = ObjectFactory.Get<IBrowserInteropWindowFactory>();

			const string glowModuleCode = "C3";
			URLHelpers.TryGenerateURL(glowModuleCode, out var glowModuleUrl);

			var jobShortcutUrl = ZFormUtilities.BusinessEntityShortcutUrl((ratingContext.DialogService as DialogService)?.ParentForm, useWebHyperlinks: true);
			var browserWindow = browserWindowFactory.CreateBrowserInteropWindow((NoResString)"CargoWise CarrierConnect", glowModuleUrl)!;

			var rateQueryFilterData = new RateQueryFilterData(jobShortcutUrl, criteria);
			browserWindow.AddBrowserListenerCommandHandler(() => rateQueryFilterData);

			browserWindow.MessageTransportLayer.AddCommandHandler<string>(GlowRateSelectorMessageType.RateSearchRequest, async void (browserEventArgs)
				=> await PerformAction(criteria, browserEventArgs, browserWindow, HandleRateSearchRequest));

			browserWindow.MessageTransportLayer.AddCommandHandler<string>(GlowRateSelectorMessageType.ApplyRatesRequest, async void (browserEventArgs)
				=> await PerformAction(criteria, browserEventArgs, browserWindow, HandleApplyRatesRequest));

			browserWindow.MessageTransportLayer.AddCommandHandler<string>(GlowRateSelectorMessageType.AbortSessionRequest, async void (browserEventArgs)
				=> await PerformAction(criteria, browserEventArgs, browserWindow, HandleAbortSessionRequest));

			browserWindow.ShowDialog();

			if (result?.Outcome == GlowRateSelectorOutcome.ApplyRates)
			{
				if (lastRequestID != Guid.Empty && result?.Rate?.Source != RateSource.CargoWise)
				{
					ratingContext.Logger.Information(Invariant($"Request rates from URS: TraceID = {ConvertRequestID(lastRequestID)}"));
				}

				GlowRateSelectorJobUpdater.CommitJobChanges(acceptedChanges, result?.ApplyRateRequest!);
				RateSearchResult.DeleteExpiredRecords();
			}

			ReportRateSelectorUsage();

			return result ?? new GlowRateSelectorResult(GlowRateSelectorOutcome.AbortSession);
		}

		string ConvertRequestID(Guid requestId) => requestId.ToString("N", CultureInfo.InvariantCulture).ToLowerInvariant();

		static async Task PerformAction<TDataType>(RatingCriteria criteria, BrowserMessageEventArgs<TDataType> browserEventArgs, IBrowserInteropWindow browserWindow, Func<RatingCriteria, BrowserMessageEventArgs<TDataType>, IBrowserInteropWindow, Task> action)
		{
			try
			{
				await action(criteria, browserEventArgs, browserWindow);
			}
			catch (Exception ex)
			{
				await browserWindow.MessageTransportLayer.SendToBrowserAsync(GlowRateSelectorMessageType.Exception, ex.Message);
			}
		}

		async Task HandleRateSearchRequest(RatingCriteria criteria, BrowserMessageEventArgs<string> browserEventArgs, IBrowserInteropWindow browserWindow)
		{
			var rateQuery = JsonConvert.DeserializeObject<RateQueryDto>(browserEventArgs.Payload);

			if (rateQuery is null || isCurrentlySearching)
			{
				return;
			}

			lastRequestID = Guid.NewGuid();
			var response = new AsyncRateResponseDto { RequestId = lastRequestID.ToString() };

			await browserWindow.MessageTransportLayer
				.SendToBrowserAsync(GlowRateSelectorMessageType.RateSearchResult, response);

			PerformRateSearch(criteria, rateQuery);
		}

		void PerformRateSearch(RatingCriteria criteria, RateQueryDto rateQueryDto)
		{
			try
			{
				var rateSelectorMetrics = new RateSelectorMetricsModel();
				isCurrentlySearching = true;
				var logger = new ElementaryLogger();
				var rateSelectorService = new RateSelectorService(logger);
				var response = rateSelectorService.SearchAndCalculateRatesUsingCriteria(criteria, rateQueryDto, rateSelectorMetrics, ConvertRequestID(lastRequestID));
				response.Log = string.Join(System.Environment.NewLine, logger.GetAllLogs());
				response.Warnings = [.. logger.Warnings];
				lastRateSearchResults = [.. response.Rates];

				var rateSearchResultFactory = new BusinessObjectFactory();
				var rateSearchResult = rateSearchResultFactory.NewWithPrimaryKey<RateSearchResult>(lastRequestID);
				rateSearchResult.RR_JsonContent = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
				rateSearchResultFactory.Save();

				ReportRateSelectorSearchUsage(rateSelectorMetrics);
			}
			catch (Exception ex)
			{
				ErrorReporter.ReportOnce("Exception occured in C3 PerformRateSearch", ex);
			}
			finally
			{
				isCurrentlySearching = false;
			}
		}

		async Task HandleApplyRatesRequest(RatingCriteria criteria, BrowserMessageEventArgs<string> browserEventArgs, IBrowserInteropWindow browserWindow)
		{
			var applyRateRequestDto = JsonConvert.DeserializeObject<ApplyRateRequestDto>(browserEventArgs.Payload);
			if (applyRateRequestDto is null)
			{
				return;
			}

			var cachedResult = lastRateSearchResults?.FirstOrDefault(rate => rate.ResultId == applyRateRequestDto.RateId);
			if (cachedResult is null)
			{
				return;
			}

			var selectedCharges = new AutoRateInfoCollection(criteria.Factory);
			selectedCharges.AddRange(
				cachedResult.Charges
					.Where(charge => applyRateRequestDto.ChargesToApply.Contains(charge.ChargeID))
					.Select(charge => charge.AutoRateInfo)
			);

			var jobUpdates = GlowRateSelectorJobUpdater
				.GetUpdateConfirmations(criteria, null!, selectedCharges, cachedResult, applyRateRequestDto.AutoratingDate.ToZDate());

			acceptedChanges = jobUpdates
				.JobUpdates
				.Where(field => !field.RequiresConfirmation || applyRateRequestDto.JobChangesAccepted.Any(dtoField => dtoField.Type == field.Type && dtoField.Accepted))
				.ToList();

			var requiredApprovals = jobUpdates
				.UpdatesRequiringConfirmation
				.Where(field => !field.IsOptional)
				.Except(acceptedChanges);

			if (applyRateRequestDto.JobChangesAccepted.Count != jobUpdates.UpdatesRequiringConfirmation.Count || requiredApprovals.Any())
			{
				await browserWindow.MessageTransportLayer
					.SendToBrowserAsync(GlowRateSelectorMessageType.JobUpdateRequired, jobUpdates);

				return;
			}

			result = new(
				GlowRateSelectorOutcome.ApplyRates,
				new RateSelectorAllChargesProvider(selectedCharges, criteria, new SimpleLogger(), applyZeroCharges: applyRateRequestDto.ApplyZeroCharges).GetAllCharges(),
				cachedResult,
				applyRateRequestDto
			);

			await browserWindow.MessageTransportLayer
				.SendToBrowserAsync(GlowRateSelectorMessageType.ApplyRatesResponse, "");

			browserWindow.Close();
		}

		async Task HandleAbortSessionRequest(RatingCriteria criteria, BrowserMessageEventArgs<string> browserEventArgs, IBrowserInteropWindow browserWindow)
		{
			var abortRequest = JsonConvert.DeserializeObject<AbortSessionRequestDto>(browserEventArgs.Payload);

			if (abortRequest is null)
			{
				return;
			}

			result = new GlowRateSelectorResult(abortRequest.ContinueAutorating ? GlowRateSelectorOutcome.AutorateWithoutSelection : GlowRateSelectorOutcome.AbortSession);

			await browserWindow.MessageTransportLayer
				.SendToBrowserAsync(GlowRateSelectorMessageType.AbortSessionResponse, new AbortSessionResponseDto
				{
					ContinueAutorating = abortRequest.ContinueAutorating
				});

			browserWindow.Close();
		}

		#region Usage Reporting

		void ReportRateSelectorUsage()
		{
			switch (result?.Outcome)
			{
				case GlowRateSelectorOutcome.ApplyRates:
					var selecetedProvider = RateSourceToRateProvider(result?.Rate?.Source);
					var searchResult = new UsageRatesSearchResult();
					searchResult.URS.TotalRates = lastRateSearchResults?.Count(r => r.Source != RateSource.CargoWise) ?? 0;
					searchResult.CW1.TotalRates = lastRateSearchResults?.Count(r => r.Source == RateSource.CargoWise) ?? 0;

					ReportRateSelector(RateSelectorAction.Select, selectedProvider: selecetedProvider, result: searchResult, sessionTime: sessionTimeStopwatch.ElapsedMilliseconds);
					break;
				case GlowRateSelectorOutcome.AutorateWithoutSelection:
					ReportRateSelector(RateSelectorAction.Skip);
					break;
				case GlowRateSelectorOutcome.AbortSession:
					ReportRateSelector(RateSelectorAction.Cancel);
					break;
			}
		}

		void ReportRateSelectorSearchUsage(RateSelectorMetricsModel rateSelectorMetrics)
		{
			var result = new UsageRatesSearchResult();
			var cw1Rates = lastRateSearchResults?.Where(r => r.Source == RateSource.CargoWise).ToArray() ?? [];
			var ursRates = lastRateSearchResults?.Where(r => r.Source != RateSource.CargoWise).ToArray() ?? [];

			if (RatingFeatureHelper.Urs.IsEnabled)
			{
				result.URS.TotalRates = ursRates.Length;
				result.URS.ElapsedTime = (int)rateSelectorMetrics.URSSearchTime;
			}

			result.CW1.TotalRates = cw1Rates.Length;
			result.CW1.ElapsedTime = (int)rateSelectorMetrics.CW1SearchTime;

			ReportRateSelectorSearch(result);
		}

		RateProvider RateSourceToRateProvider(RateSource? source) =>
			source switch
			{
				RateSource.Cargoguide => RateProvider.Cargoguide,
				RateSource.CargoSphere => RateProvider.CargoSphere,
				RateSource.CargoWise => RateProvider.CW1,
				_ => default,
			};

		#endregion
	}

	class AsyncRateResponseDto
	{
		public string RequestId { get; set; } = null!;
	}
}
