using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Extensions
{
	public static class IDictionaryExtensionMethods
	{
		public static decimal GetValue(this IDictionary<string, decimal> keyValuePairs, ZString key)
		{
			var result = 0m;
			if (!key.IsEmpty && keyValuePairs.ContainsKey(key))
			{
				result = keyValuePairs[key];
			}
			return result;
		}
	}
}
