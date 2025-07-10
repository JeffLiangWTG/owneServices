using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Models;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.NewService;

public class DataBlockCacheHelper : IDataBlockCacheHelper
{
	public DataBlockCacheHelper(IFileCacheWrapper fileCache)
	{
		Argument.NotNull(fileCache, nameof(fileCache));

		this.cache = fileCache;
		jsonSerializer = new JsonSerializer();
	}

	public IEnumerable<IDataBlockKey<T>> GetSequentialKeys<T>(IDataBlockKey<T> firstKey)
	{
		var currentKey = firstKey;
		while (currentKey != null && cache.Contains(currentKey.GetKey()) && cache.Contains(currentKey.GetKey(), CheckpointRegion))
		{
			yield return currentKey;
			var checkpoint = (string)cache.Get(currentKey.GetKey(), CheckpointRegion);
			currentKey = currentKey.CaculateNextKey(checkpoint);
		}
	}

	public IEnumerable<T> GetData<T>(IDataBlockKey<T> key)
	{
		var data = (byte[])cache.Get(key.GetKey());
		if (data != null)
		{
			using (var ms = new MemoryStream(data))
			{
				var stream = new GZipStream(ms, CompressionMode.Decompress, true);
				var streamReader = new StreamReader(stream);
				using (var jsonReader = new JsonTextReader(streamReader) { SupportMultipleContent = true })
				{
					while (jsonReader.Read())
					{
						yield return jsonSerializer.Deserialize<T>(jsonReader);
					}
				}
			}
		}
	}

	void Flush<T>(T[] buffer, IDataBlockKey<T> key, out IDataBlockKey<T> nextKey)
	{
		Argument.NotNull(buffer, nameof(buffer));
		Argument.GreaterThan(buffer.Length, 0, nameof(buffer.Length));
		Argument.NotNull(key, nameof(key));

		using (var ms = new MemoryStream())
		{
			var stream = new GZipStream(ms, CompressionMode.Compress, true);
			var streamWriter = new StreamWriter(stream);
			using (var jsonWriter = new JsonTextWriter(streamWriter))
			{
				foreach (var b in buffer)
				{
					jsonSerializer.Serialize(jsonWriter, b);
				}
			}
			var checkpoint = (buffer[buffer.Length - 1] as RefDataSet)?.Checkpoint;

			using (var mutex = new Mutex(false, key.GetKey() + CheckpointRegion))
			{
				var hasHandle = false;
				try
				{
					hasHandle = mutex.WaitOne(TimeSpan.FromMinutes(1), false);
					cache.Remove(key.GetKey(), CheckpointRegion);
					cache.Add(key.GetKey(), ms.ToArray());
					cache.Add(key.GetKey(), checkpoint ?? string.Empty, CheckpointRegion);
				}
				finally
				{
					if (hasHandle)
					{
						mutex.ReleaseMutex();
					}
				}
				nextKey = key.CaculateNextKey(checkpoint);
			}
		}
	}

	public void WriteToCache<T>(IEnumerable<T> dataFactory, IDataBlockKey<T> startKey, Func<T, bool> doFlush)
	{
		using (var mutex = new Mutex(false, startKey.GetKey()))
		{
			var hasHandle = false;
			try
			{
				hasHandle = mutex.WaitOne(TimeSpan.FromMinutes(5), false);
				if (!cache.Contains(startKey.GetKey()) || !cache.Contains(startKey.GetKey(), CheckpointRegion))
				{
					var buffer = new List<T>();
					var currentKey = startKey;
					foreach (var data in dataFactory)
					{
						buffer.Add(data);
						if (doFlush(data))
						{
							Flush(buffer.ToArray(), currentKey, out currentKey);
							buffer = new List<T>();
						}
					}
					if (buffer.Count > 0)
					{
						Flush(buffer.ToArray(), currentKey, out currentKey);
					}
				}
			}
			finally
			{
				if (hasHandle)
				{
					mutex.ReleaseMutex();
				}
			}
		}
		cache.ShrinkCacheSize();
	}

	public IEnumerable<(string, string)> GetCachePaths<T>(IDataBlockKey<T> key, long maxSize)
	{
		var size = (long)0;
		var iterKey = key;
		while (iterKey != null && size < maxSize)
		{
			var keyString = iterKey.GetKey();
			var filePath = cache.GetCachePath(keyString);
			var fileInfo = new FileInfo(filePath);
			string checkpoint = null;
			if (cache.Contains(keyString, CheckpointRegion))
			{
				checkpoint = (string)cache.Get(keyString, CheckpointRegion);
			}
			if (fileInfo.Exists && checkpoint != null)
			{
				yield return (filePath, checkpoint);
				iterKey = iterKey.CaculateNextKey(checkpoint);
				size += fileInfo.Length;
			}
			else
			{
				break;
			}
		}
	}

	readonly IFileCacheWrapper cache;
	readonly JsonSerializer jsonSerializer;
	public const string CheckpointRegion = "Checkpoint";
}
