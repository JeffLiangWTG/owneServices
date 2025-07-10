using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Models;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Service.DataContractAdaptor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.NewService.Controllers
{
	[ApiController]
	[Route("[controller]/[action]")]
	public class DataSetControllerBase<T> : ControllerBase, IDataSetController
	{
		protected DataSetControllerBase(
			IReferenceDataService service,
			IDataAdaptor adaptor,
			IDataBlockCacheHelper dataBlockCacheHelper,
			IClientRecord clientRecord,
			ICacheWrapper cacheWrapper,
			ILogHelper logHelper)
		{
			Argument.NotNull(service, nameof(service));
			Argument.NotNull(adaptor, nameof(adaptor));
			Argument.NotNull(dataBlockCacheHelper, nameof(dataBlockCacheHelper));
			Argument.NotNull(clientRecord, nameof(clientRecord));
			Argument.NotNull(cacheWrapper, nameof(cacheWrapper));
			Argument.NotNull(logHelper, nameof(logHelper));

			this.service = service;
			this.adaptor = adaptor;
			this.dataBlockCacheHelper = dataBlockCacheHelper;
			this.clientRecord = clientRecord;
			this.cacheWrapper = cacheWrapper;
			this.logHelper = logHelper;
		}

		readonly IReferenceDataService service;
		readonly IDataAdaptor adaptor;
		readonly IDataBlockCacheHelper dataBlockCacheHelper;
		readonly IClientRecord clientRecord;
		readonly ICacheWrapper cacheWrapper;
		readonly ILogHelper logHelper;

		public IReferenceDataService<T> TypedService
		{
			get { return (IReferenceDataService<T>)service; }
		}

		protected IDataAdaptor Adaptor
		{
			get { return adaptor; }
		}

		[HttpGet]
		[ExtendedCacheOutput(ServerTimeSpan = 300)]
		[Authorize(AuthenticationSchemes = AuthType.CombinedAuth)]
		public DateTime GetServerTimestamp()
		{
			return TypedService.GetServerTimestamp();
		}

		[HttpGet]
		[Authorize(AuthenticationSchemes = AuthType.CombinedAuth)]
		public virtual IEnumerable<DataSetVersion> GetAvailableDataSetTimestamps([FromQuery] string version = null)
		{
			return GetAvailableDataSetTimestampsViaCache();
		}

		IEnumerable<DataSetVersion> GetAvailableDataSetTimestampsViaCache()
		{
			var cache = (List<Tuple<string, IEnumerable<DataSetVersion>>>)cacheWrapper.Get(AvailableDataSetTimestampsCacheKey);
			if (cache == null)
			{
				cache = service.GetAllDataSetTimestamps().ToList();
				cacheWrapper.Add(AvailableDataSetTimestampsCacheKey, cache, DateTimeOffset.UtcNow.AddMinutes(5));
			}
			foreach (var result in service.GetAvailableDataSetTimestamps(cache))
			{
				var mergeTimeout = -1;
				_ = int.TryParse(ApplicationConfig.GetMergeTimeout(result.Name), out mergeTimeout);
				result.MergeTimeout = mergeTimeout != -1 ? (int?)mergeTimeout : null;
				yield return result;
			}
		}

		const string AvailableDataSetTimestampsCacheKey = "AvailableDataSetTimestampsCacheKey";

		[HttpPost]
		[HttpGet]
		[ExtendedCacheOutput(ServerTimeSpan = 3600)]
		[Authorize(AuthenticationSchemes = AuthType.CombinedAuth)]
		public async Task<string> Report([FromQuery] string clientId, [FromQuery] string clientTimestamp, [FromQuery] string checkpoint, [FromQuery] string dataset = null, [FromQuery] string systemType = null, [FromQuery] bool isInUse = true)
		{
			var dataSetName = dataset ?? typeof(T).Name;
			await clientRecord.RecordAsync(dataSetName, clientId, systemType, JsonConvert.DeserializeObject<DateTime?>(clientTimestamp), checkpoint, isInUse);
			return string.Empty;
		}

		[HttpPost]
		[Authorize(AuthenticationSchemes = AuthType.CombinedAuth)]
		public async Task<IActionResult> GetDataStreamV2([FromBody] DataSetGet dataSetGet)
		{
			Argument.NotNull(dataSetGet, nameof(dataSetGet));
			var key = GetKey(dataSetGet);
			IActionResult response = null;

			if (EnableRawResponse(dataSetGet.Version))
			{
				int triesLeft = 3;
				while (triesLeft > 0)
				{
					try
					{
						response = await GetRawResponse(key);
						break;
					}
					catch (IOException ex) when (ex.HResult == -2147024864)
					{
						Thread.Sleep(1000);
						triesLeft--;
					}
				}
			}

			if (response == null)
			{
				response = GetDataStream(dataSetGet);
			}

			return response;
		}

		[HttpPost]
		[Authorize(AuthenticationSchemes = AuthType.CombinedAuth)]
		public IActionResult GetDataStream([FromBody] DataSetGet dataSetGet)
		{
			Argument.NotNull(dataSetGet, nameof(dataSetGet));
			var key = GetKey(dataSetGet);

			PrepareCaching(key, dataSetGet.Version);
			var compressionType = CompressionHelper.GetCompressionType(Request);
			UpdateContentEncodingOnHeader(compressionType);

			var serialiser = new JsonSerializer();
			using (var responseStream = CompressionHelper.SetupCompressedStream(Response.Body, compressionType))
			using (var streamWriter = new StreamWriter(responseStream))
			using (var jsonWriter = new JsonTextWriter(streamWriter))
			{
				var recordsDelivered = 0;
				var lastCheckpoint = dataSetGet.Checkpoint;
				T lastCachedDataSet = default(T);
				foreach (var cachedKey in dataBlockCacheHelper.GetSequentialKeys(key))
				{
					logHelper.LogInfo(HttpContext?.User?.Identity?.Name, $"Checkpoint cache exists. Key: {cachedKey.GetKey()}", Request?.GetEncodedPathAndQuery());
					foreach (var dataSet in dataBlockCacheHelper.GetData(cachedKey))
					{
						lastCachedDataSet = dataSet;
						recordsDelivered++;
						serialiser.Serialize(jsonWriter, TransformData(dataSet, dataSetGet.Version));
					}
				}

				if (lastCachedDataSet != null)
				{
					lastCheckpoint = (lastCachedDataSet as RefDataSet).Checkpoint;
				}
				if (lastCachedDataSet == null || !string.IsNullOrEmpty(lastCheckpoint))
				{
					key = new DataBlockKey<T>(key.LowerTimestamp, key.UpperTimestamp, lastCheckpoint, key.DataSet);
					foreach (var dataSet in GetData(key, dataSetGet.Version))
					{
						recordsDelivered++;
						serialiser.Serialize(jsonWriter, TransformData(dataSet, dataSetGet.Version));
					}
				}
				logHelper.LogInfo(HttpContext?.User?.Identity?.Name, $"No. of records delivered: {recordsDelivered}. Key: {key.GetKey()}", Request?.GetEncodedPathAndQuery());
			}

			return new EmptyResult();
		}

		void UpdateContentEncodingOnHeader(CompressionType compressionType)
		{
			if (compressionType == CompressionType.None)
			{
				return;
			}

#pragma warning disable CA1308 // Normalize strings to uppercase
			Response.Headers.ContentEncoding = new StringValues(compressionType.ToString().ToLowerInvariant());
#pragma warning restore CA1308 // Normalize strings to uppercase
		}

		[NonAction]
		public async Task<IActionResult> GetRawResponse(DataBlockKey<T> key)
		{
			using (var content = new MultipartContent())
			{
				string checkpoint = null;
				int idx = -1;
				var maxSize = 1048576;
				_ = int.TryParse(ApplicationConfig.RawResponseMaxSize, out maxSize);
				foreach (var item in dataBlockCacheHelper.GetCachePaths(key, maxSize))
				{
					idx++;
#pragma warning disable CA2000 // Dispose objects before losing scope
					var fileContent = new StreamContent(new FileStream(item.Item1, FileMode.Open, FileAccess.Read, FileShare.Read));
#pragma warning restore CA2000 // Dispose objects before losing scope
					fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue(DispositionTypeNames.Attachment)
					{
						Name = idx.ToString(CultureInfo.InvariantCulture),
						FileName = Path.GetFileName(item.Item1)
					};
					fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
					checkpoint = item.Item2;
					content.Add(fileContent);
				}
				if (checkpoint != null)
				{
					logHelper.LogInfo(HttpContext?.User?.Identity?.Name, $"Cache exists. Key: {key.GetKey()}", Request?.GetEncodedPathAndQuery());
#pragma warning disable CA2000 // Dispose objects before losing scope
					var checkoutContent = new StringContent(checkpoint);
#pragma warning restore CA2000 // Dispose objects before losing scope
					checkoutContent.Headers.ContentDisposition = new ContentDispositionHeaderValue(DispositionTypeNames.Attachment)
					{
						Name = "Checkpoint"
					};
					content.Add(checkoutContent);


					Response.Headers.ContentType = new StringValues(content.Headers.ContentType.ToString());
					await content.CopyToAsync(Response.Body);
					return new EmptyResult();
				}
			}
			return null;
		}

		protected virtual object TransformData(T dataSet, string version)
		{
			return adaptor.ToVersion(dataSet, version);
		}

		protected virtual bool EnableRawResponse(string version)
		{
			return bool.Parse(ApplicationConfig.EnableRawResponse) &&
				!adaptor.RequireTransform(version, typeof(T));
		}

		static DataBlockKey<T> GetKey(DataSetGet dataSetGet)
		{
			Argument.NotNull(dataSetGet, nameof(dataSetGet));

			var dataSetName = string.IsNullOrEmpty(dataSetGet.DataSet) ? typeof(T).Name : dataSetGet.DataSet;
			if (DataSetControllerBaseHelper.DoesCacheVersionMatter(dataSetName))
			{
				dataSetName += DataSetControllerBaseHelper.GetVersionForCacheString(dataSetName, dataSetGet.Version);
			}
			return new DataBlockKey<T>(dataSetGet.LowerTimestamp, dataSetGet.UpperTimestamp, dataSetGet.Checkpoint, dataSetName);
		}

		void PrepareCaching(DataBlockKey<T> firstKey, string version)
		{
			Argument.NotNull(firstKey, nameof(firstKey));
			var lastCachedKey = dataBlockCacheHelper.GetSequentialKeys(firstKey).LastOrDefault();
			T lastCachedData = default(T);
			var lastCachedCheckpoint = firstKey.Checkpoint;
			if (lastCachedKey != null)
			{
				lastCachedData = dataBlockCacheHelper.GetData(lastCachedKey).LastOrDefault();
				lastCachedCheckpoint = (lastCachedData as RefDataSet)?.Checkpoint;
			}
			var notAllDataInCache = lastCachedData == null || !string.IsNullOrEmpty(lastCachedCheckpoint);
			if (notAllDataInCache)
			{
				var key = new DataBlockKey<T>(firstKey.LowerTimestamp, firstKey.UpperTimestamp, lastCachedCheckpoint, firstKey.DataSet);
				logHelper.LogInfo(HttpContext?.User?.Identity?.Name, $"Cache does not exist. Key: {key.GetKey()}", Request?.GetEncodedPathAndQuery());
				dataBlockCacheHelper.WriteToCache(GetData(key, version), key, x => !string.IsNullOrEmpty((x as RefDataSet)?.Checkpoint));
			}
		}

		protected virtual IEnumerable<T> GetData(DataBlockKey<T> key, string version)
		{
			Argument.NotNull(key, nameof(key));

			logHelper.LogInfo(HttpContext?.User?.Identity?.Name, $"Getting data from {key.DataSet} service.", Request?.GetEncodedPathAndQuery());
			var dataSetId = TypedService.GetDataSetId(key.DataSet);
			var result = TypedService.GetData(key.LowerTimestamp, key.UpperTimestamp, CheckpointHelper.TryGet(key.Checkpoint), BlockSize, dataSetId);
			return result;
		}

		public const int BlockSize = 1000;

		protected IEnumerable<DataSetVersion> GetMinimumSupportedDataSetVersionsByContract(string clientVersion, int minimumSupportContractVersion)
		{
			if (!string.IsNullOrEmpty(clientVersion) && adaptor.IsSRDbVersion(clientVersion))
			{
				return GetAvailableDataSetTimestampsViaCache();
			}
			if (!string.IsNullOrEmpty(clientVersion))
			{
				var parsedVersion = adaptor.ParseVersion(clientVersion);
				if (parsedVersion.Item2 >= minimumSupportContractVersion)
				{
					return GetAvailableDataSetTimestampsViaCache();
				}
			}
			return [new DataSetVersion(typeof(T).Name, DateTime.MinValue)];
		}

		protected IEnumerable<DataSetVersion> GetMinimumSupportedDataSetVersionsBySRDbVersion(string clientVersion, int minimumSupportSRDbVersion)
		{
			if (!string.IsNullOrEmpty(clientVersion) && adaptor.IsSRDbVersion(clientVersion))
			{
				var parsedVersion = adaptor.ParseToSRDbVersion(clientVersion);
				if (parsedVersion >= minimumSupportSRDbVersion)
				{
					return GetAvailableDataSetTimestampsViaCache();
				}
			}
			return [new DataSetVersion(typeof(T).Name, DateTime.MinValue)];
		}
	}
}
