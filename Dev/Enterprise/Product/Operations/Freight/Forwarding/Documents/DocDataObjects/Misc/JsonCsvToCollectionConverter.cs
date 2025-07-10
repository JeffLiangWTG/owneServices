using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class JsonCsvToCollectionConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType) => objectType.IsAssignableFrom(typeof(IReadOnlyCollection<string>));

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value is IReadOnlyCollection<string> collection
				&& collection.Count > 0)
			{
				var elements = collection
					.Where(v => !string.IsNullOrWhiteSpace(v))
					.Select(v => v.Trim());

				writer.WriteValue(string.Join(",", elements));
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return reader
				?.Value
				?.ToString()
				.Split(',')
				.Where(v => !string.IsNullOrWhiteSpace(v))
				.Select(v => v.Trim())
				.ToArray();
		}
	}
}
