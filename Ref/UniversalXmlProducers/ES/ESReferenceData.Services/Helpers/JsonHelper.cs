using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.ESReferenceData.Services;

public static class JsonHelper
{
	public static List<T> GetListItemsFromJsonl<T>(TextReader content, StringBuilder errorBuilder, bool handleDates = false, Func<T, bool> filter = null) where T : JsonlSchema
	{
		var result = new List<T>();

		using (var jsonReader = new JsonTextReader(content))
		{
			jsonReader.SupportMultipleContent = true;
			JsonSerializer jsonSerializer;
			if (handleDates)
			{
				jsonSerializer = new JsonSerializer { DateParseHandling = DateParseHandling.DateTime };
				jsonSerializer.Converters.Add(new DateTimeConverter());
			}
			else
			{
				jsonSerializer = new JsonSerializer();
			}

			while (jsonReader.Read())
			{
				try
				{
					var item = jsonSerializer.Deserialize<T>(jsonReader);
					if (filter?.Invoke(item) ?? true)
					{
						result.Add(item);
					}
				}
				catch (JsonSerializationException e)
				{
					errorBuilder.AppendLine("Invalid json format.");
					errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Exception Details: {e}");
				}
			}
		}

		content.Dispose();

		return result;
	}

	public static List<T> GetListItemsFromJsonl<T>(string content, StringBuilder errorBuilder, bool handleDates = false, Func<T, bool> filter = null) where T : JsonlSchema
		=> GetListItemsFromJsonl(new StringReader(content), errorBuilder, handleDates, filter);

	public static T GetJsonItem<T>(TextReader content, StringBuilder errorBuilder) where T : JsonlSchema
	{
		T result = null;

		using (var jsonReader = new JsonTextReader(content))
		{
			jsonReader.SupportMultipleContent = true;
			var jsonSerializer = new JsonSerializer();

			while (jsonReader.Read())
			{
				try
				{
					result = jsonSerializer.Deserialize<T>(jsonReader);
				}
				catch (JsonSerializationException e)
				{
					errorBuilder.AppendLine("Invalid json format.");
					errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Exception Details: {e}");
				}
			}
		}

		content.Dispose();

		return result;
	}

	public static string ToJsonl(string exciseJsonContent)
	{
		exciseJsonContent = exciseJsonContent.Trim();
		if (exciseJsonContent.StartsWith('[') && exciseJsonContent.EndsWith(']'))
		{
			exciseJsonContent = exciseJsonContent[1..];
			exciseJsonContent = exciseJsonContent[..^1];
		}

		return exciseJsonContent;
	}

	class DateTimeConverter : Newtonsoft.Json.Converters.DateTimeConverterBase
	{
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.Value != null && DateTime.TryParseExact(reader.Value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var val))
			{
				return val;
			}

			return null;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(((DateTime)value).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
		}
	}
}
