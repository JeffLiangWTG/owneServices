using System;
using System.Collections.Generic;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class CodeDataObjectCache
	{
		Dictionary<string, ICodeDataObject> cache { get; } = new Dictionary<string, ICodeDataObject>();

		public string BuildKey(params string[] parameters)
		{
			var key = string.Join("-", parameters);
			return key;
		}

		public void Clear(string key)
		{
			cache.Remove(key);
		}

		public T GetValue<T>(string key) where T : ICodeDataObject
		{
			if (!cache.TryGetValue(key, out var value))
			{
				return default;
			}

			return (T)value;
		}

		public T GetValue<T>(string key, Func<T> getValueDelegate) where T : ICodeDataObject
		{
			if (!cache.TryGetValue(key, out var value))
			{
				value = getValueDelegate();
				cache[key] = value;
			}

			return (T)value;
		}
	}
}
