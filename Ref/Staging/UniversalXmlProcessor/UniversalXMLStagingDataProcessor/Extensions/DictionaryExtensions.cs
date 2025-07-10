using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public static class DictionaryExtensions
	{
		public static void AddIfNotExists<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
		{
			Argument.NotNull(dict, nameof(dict));
			Argument.NotNull(key, nameof(key));

			if (!dict.ContainsKey(key))
			{
				dict.Add(key, value);
			}
		}

		public static void MergeIfNotExists<TKey, TValue>(this Dictionary<TKey, TValue> dict, Dictionary<TKey, TValue> anotherDict)
		{
			Argument.NotNull(dict, nameof(dict));
			Argument.NotNull(anotherDict, nameof(anotherDict));

			foreach (var item in anotherDict)
			{
				dict.AddIfNotExists(item.Key, item.Value);
			}
		}
	}
}
