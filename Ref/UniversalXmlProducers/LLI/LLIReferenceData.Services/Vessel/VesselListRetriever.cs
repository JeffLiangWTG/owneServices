using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.LLIReferenceData.Services.Entities;
using CargoWise.RefDbRepo.LLIReferenceData.Services.Helpers;

namespace CargoWise.RefDbRepo.LLIReferenceData.Services.Vessel;

public static class VesselListRetriever
{
	const string TokenSuccessMessage = "Success";
	const string VesselAdvancedCharacteristicsQueryParams = "characteristicTypes=dimension,capacities,design";

	static readonly IReadOnlyList<string> VesselListQueryParams =
	[
		"vesselType=BBU,BCB,DSS,GGC,GPC,GRF,MVE,OBA,OFY,OHB,OHL,OLC,ORN,PRR,UBC,UCC,UCR,URC,URR,ZZZ",
		"vesselStatus=I,L,O,R,U,X"
	];

	public static async Task<LliVesselsData> GetVesselsDataAsync(string userName, string password, int parallelRequests)
	{
		using var client = new HttpClient();

		var token = await GetAuthTokenAsync(client, userName, password);
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
		var vessels = await GetVesselListAsync(client, parallelRequests);
		var vesselBasicCharacteristics = await GetVesselBasicCharacteristicsAsync(client, vessels, parallelRequests);
		var vesselAdvancedCharacteristics = await GetVesselAdvancedCharacteristicsAsync(client, vessels, parallelRequests);

		return new LliVesselsData(vessels, vesselBasicCharacteristics, vesselAdvancedCharacteristics);
	}

	static async Task<string> GetAuthTokenAsync(HttpClient client, string userName, string password)
	{
		var queryUri = new Uri($"{ApplicationConfig.VesselApiMainURL}{ApplicationConfig.TokenUriPath}");

		Console.WriteLine($"Requesting auth token on {queryUri}...");

		var tokenResponse = await HttpHelper.PostAsJsonAsync(client, queryUri, new TokenRequest(userName, password));

		var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
		if (tokenResult.Message != TokenSuccessMessage || string.IsNullOrEmpty(tokenResult.Payload))
		{
			throw new InvalidOperationException($"Unable to obtain auth token: {queryUri}. Message: {tokenResult.Message}");
		}

		Console.WriteLine("Auth token successfully obtained.");

		return tokenResult.Payload;
	}

	static async Task<IReadOnlyList<Entities.Vessel>> GetVesselListAsync(HttpClient client, int parallelRequests)
	{
		Console.WriteLine("Starting fetching the Vessel List.");

		static async Task<(IReadOnlyList<Entities.Vessel> Vessels, int TotalPages)> RequestVesselsAsync(HttpClient client, int page)
		{
			var queryUriBuilder = new UriBuilder($"{ApplicationConfig.VesselApiMainURL}{ApplicationConfig.VesselListUriPath}");
			queryUriBuilder.Query = $"pageNumber={page}&{string.Join('&', VesselListQueryParams)}";
			var queryUri = queryUriBuilder.Uri;

			Console.WriteLine($"Requesting {queryUri}...");
			var vesselListResponse = await HttpHelper.GetAsync(client, queryUri);

			var vesselListResult = await vesselListResponse.Content.ReadFromJsonAsync<VesselListResponse>();
			var vessels = vesselListResult?.Data?.Vessels
				?? throw new InvalidOperationException($"Vessels list not specified in the result. Errors: {string.Join(",", vesselListResult?.Data?.Errors ?? [])}");

			return (vessels, vesselListResult?.Data?.TotalPages ?? 1);
		}

		var (firstPage, totalPages) = await RequestVesselsAsync(client, 1);

		var restOfPages = await ParallelHttpRequestsAsync(
			Enumerable.Range(2, totalPages - 1),
			page => RequestVesselsAsync(client, page),
			parallelRequests);

		Console.WriteLine("Finished fetching the vessel list.");

		return firstPage
			.Concat(restOfPages.SelectMany(p => p.Vessels))
			.Where(v => !string.IsNullOrEmpty(v.VesselImo))
			.OrderBy(v => v.VesselImo)
			.ToList()
			.AsReadOnly();
	}

	static async Task<IReadOnlyList<VesselBasicCharacteristic>> GetVesselBasicCharacteristicsAsync(
		HttpClient client,
		IReadOnlyList<Entities.Vessel> vessels,
		int parallelRequests)
	{
		Console.WriteLine("Starting fetching the Vessel Basic Characteristics.");

		static async Task<IReadOnlyList<VesselBasicCharacteristic>> RequestVesselBasicCharacteristicsAsync(HttpClient client, IGrouping<int, Entities.Vessel> vesselGrouping)
		{
			var queryUriBuilder = new UriBuilder($"{ApplicationConfig.VesselApiMainURL}{ApplicationConfig.VesselBasicCharacteristicsUriPath}");
			queryUriBuilder.Query = $"vesselId={string.Join(',', vesselGrouping.Select(v => v.VesselId))}";
			var queryUri = queryUriBuilder.Uri;

			Console.WriteLine($"Requesting batch {vesselGrouping.Key}: {queryUri}...");
			var response = await HttpHelper.GetAsync(client, queryUri);
			var result = await response.Content.ReadFromJsonAsync<VesselBasicCharacteristicsResponse>();

			return result?.Data?.Vessels
				?? throw new InvalidOperationException($"Failed to get vessel basic characteristics for batch '{vesselGrouping.Key}'");
		}

		var batchedVessels = Batch(vessels, 100);
		var results = await ParallelHttpRequestsAsync(
			batchedVessels,
			grp => RequestVesselBasicCharacteristicsAsync(client, grp),
			parallelRequests);

		Console.WriteLine("Finished fetching the Vessel Basic Characteristics.");

		return results
			.SelectMany(v => v)
			.ToList()
			.AsReadOnly();
	}

	static async Task<IReadOnlyList<VesselAdvancedCharacteristic>> GetVesselAdvancedCharacteristicsAsync(
		HttpClient client,
		IReadOnlyList<Entities.Vessel> vessels,
		int parallelRequests)
	{
		Console.WriteLine("Starting fetching the vessela advanced characteristics.");

		static async Task<IReadOnlyList<VesselAdvancedCharacteristic>> RequestVesselAdvancedCharacteristicsAsync(HttpClient client, IGrouping<int, Entities.Vessel> vesselGrouping)
		{
			var queryUriBuilder = new UriBuilder($"{ApplicationConfig.VesselApiMainURL}{ApplicationConfig.VesselAdvancedCharacteristicsUriPath}");
			queryUriBuilder.Query = $"{VesselAdvancedCharacteristicsQueryParams}&vesselId={string.Join(',', vesselGrouping.Select(v => v.VesselId))}";
			var queryUri = queryUriBuilder.Uri;

			Console.WriteLine($"Requesting batch {vesselGrouping.Key}: {queryUri}...");
			var response = await HttpHelper.GetAsync(client, queryUri);
			var result = await response.Content.ReadFromJsonAsync<VesselAdvancedCharacteristicsResponse>();

			return result?.Data?.Items
				?? throw new InvalidOperationException($"Failed to get vessel advanced characteristics for batch '{vesselGrouping.Key}'");
		}

		var batchedVessels = Batch(vessels, 100);
		var results = await ParallelHttpRequestsAsync(
			batchedVessels,
			grp => RequestVesselAdvancedCharacteristicsAsync(client, grp),
			parallelRequests);

		Console.WriteLine("Finished fetching the vessel advanced characteristics.");

		return results
			.SelectMany(v => v)
			.ToList()
			.AsReadOnly();
	}

	static async Task<IEnumerable<TResult>> ParallelHttpRequestsAsync<TItem, TResult>(
		IEnumerable<TItem> source,
		Func<TItem, Task<TResult>> requestTask,
		int parallelRequests)
	{
		ConcurrentBag<TResult> results = [];

		await Parallel.ForEachAsync(
			source,
			new ParallelOptions { MaxDegreeOfParallelism = parallelRequests },
			async (item, _) => results.Add(await requestTask(item)));

		return results;
	}

	public static IEnumerable<IGrouping<int, T>> Batch<T>(IEnumerable<T> source, int batchSize)
	{
		return source
			.Select((item, index) => (item, index, batchNumber: index / batchSize))
			.GroupBy(x => x.batchNumber, x => x.item);
	}
}
