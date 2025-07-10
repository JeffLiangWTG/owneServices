using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.NewService;

public interface IDataBlockCacheHelper
{
	IEnumerable<T> GetData<T>(IDataBlockKey<T> key);
	IEnumerable<(string, string)> GetCachePaths<T>(IDataBlockKey<T> key, long maxSize);
	IEnumerable<IDataBlockKey<T>> GetSequentialKeys<T>(IDataBlockKey<T> firstKey);
	void WriteToCache<T>(IEnumerable<T> dataFactory, IDataBlockKey<T> startKey, Func<T, bool> doFlush);
}
