using System.Collections.Generic;
using System.Text.Json;
using CargoWise.Types;

namespace Enterprise.Customs.Business;

public static class MessageHelper
{
	public static Dictionary<string, string> GetHeaderTextDictionary(ZString headerText)
	{
		var result = new Dictionary<string, string>();
		if (!headerText.IsEmpty)
		{
			var headerData = JsonSerializer.Deserialize<Dictionary<string, string>>(headerText, new JsonSerializerOptions()
			{
				AllowTrailingCommas = true
			});
			foreach (var dataKey in headerData.Keys)
			{
				result.AddToDictionaryIfValid(dataKey, headerData[dataKey]);
			}
		}

		return result;
	}

	static void AddToDictionaryIfValid(this Dictionary<string, string> dict, string keyToAdd, string valueToAdd)
	{
		if (!string.IsNullOrEmpty(valueToAdd) && !string.IsNullOrEmpty(keyToAdd))
		{
			if (!dict.ContainsKey(keyToAdd))
			{
				dict[keyToAdd] = valueToAdd;
			}
		}
	}
}
