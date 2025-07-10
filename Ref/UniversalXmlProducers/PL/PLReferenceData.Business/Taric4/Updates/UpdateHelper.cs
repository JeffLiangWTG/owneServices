using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService;
using CargoWise.RefDbRepo.PLReferenceData.Business.Message;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using System.Threading;
using System.Globalization;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.Updates.UpdateStrategy;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.Updates;

static class UpdateHelper
{
	public static bool GenerateAndRunUpdateRequests(IUpdateStrategy strategy)
	{
		var requests = GenerateUpdateRequests(strategy.FromDate, strategy.ToDate, strategy.SaveRequestsFilePath);

		var puescServiceConfig = new PuescServiceConfiguration();
		var puescServiceFactory = new PuescServiceFactory(puescServiceConfig);

		var taricConfig = new TaricUpdateParserConfig();
		var updateParser = new TaricUpdateParser(taricConfig);

		var messageSender = new MessageSender(puescServiceFactory);
		var requestTaricUpdate = new TaricUpdateInitiator(messageSender);
		var getTaricUpdate = new TaricUpdateCollector(messageSender);

		if (!SendAndGetTaricUpdatesFromRequestList(requests, requestTaricUpdate, getTaricUpdate, updateParser, strategy.ShouldWaitForResponses))
		{
			return false;
		}
		strategy.OnSuccessfulUpdate();
		return true;
	}

	public static IReadOnlyCollection<UpdateRequest> GenerateUpdateRequests(DateTime startDate, DateTime untilDate, string saveFilePath = null)
	{
		var requestDate = startDate;
		var requests = new List<UpdateRequest>();
		while (untilDate > requestDate)
		{
			var endDate = requestDate.AddDays(1);
			requests.Add(new UpdateRequest() { StartDate = requestDate, EndDate = endDate.AddTicks(-1) });
			requestDate = endDate;
		}

		if (!string.IsNullOrEmpty(saveFilePath))
		{
			var dataToSave = XmlParser.Serialize(requests);
			using var file = File.Create(saveFilePath);
			dataToSave.Save(file);
		}

		return requests;
	}

	public static bool SendAndGetTaricUpdatesFromRequestList(IReadOnlyCollection<UpdateRequest> requests,
		TaricUpdateInitiator requestTaricUpdate,
		ITaricUpdateCollector getTaricUpdate,
		ITaricUpdateParser parseTaricUpdate,
		bool shouldWaitForResponse = true)
	{
		var updateRequestResponses = new List<UpdateRequestResponse>();
		foreach (var request in requests)
		{
			var requestResponse = requestTaricUpdate.GenerateAndSendTariffUpdateRequest(request);
			updateRequestResponses.Add(new UpdateRequestResponse(request, requestResponse));
		}

		return GetTaricUpdates(getTaricUpdate, parseTaricUpdate, updateRequestResponses, shouldWaitForResponse);
	}

	public static bool GetTaricUpdates(ITaricUpdateCollector updateCollector, ITaricUpdateParser updateParser, ICollection<UpdateRequestResponse> updateRequestResponseList, bool shouldWaitForResponse = true)
		=> GetTaricUpdates(updateCollector, updateParser, updateRequestResponseList, shouldWaitForResponse, retryNumber: 0);

	static bool GetTaricUpdates(ITaricUpdateCollector updateCollector, ITaricUpdateParser updateParser,
		ICollection<UpdateRequestResponse> updateRequestResponseList, bool shouldWaitForResponse, int retryNumber)
	{
		if (retryNumber >= Taric4Constants.MaxUpdateDownloadRetry)
		{
			foreach (var request in updateRequestResponseList)
			{
				var updateRequest = request.UpdateRequest;
				var errorMessage = $"Failed getting update for request {GetDateString(updateRequest.StartDate)} - {GetDateString(updateRequest.EndDate)} - Maximum tries exceeded";
				Console.Error.WriteLine(errorMessage);
			}
			return false;
		}

		if (shouldWaitForResponse)
		{
			var sleepTimeInMinutes = 3;
			var oneMinuteInMs = 60000;
			Thread.Sleep(oneMinuteInMs * sleepTimeInMinutes);
		}
		var failedRequests = new List<UpdateRequestResponse>();

		foreach (var requestResponse in updateRequestResponseList)
		{
			var fileResponse = updateCollector.GenerateAndSendTariffGetRequest(requestResponse.RequestResponse);

			if (fileResponse is null
				|| !updateParser.ParseAndSave(fileResponse))
			{
				failedRequests.Add(requestResponse);
			}
		}

		return failedRequests.Count == 0
			|| GetTaricUpdates(updateCollector, updateParser, failedRequests, shouldWaitForResponse, retryNumber + 1);
	}

	static string GetDateString(DateTime dateTime) => dateTime.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
}


