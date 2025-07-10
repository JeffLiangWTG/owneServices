using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Enterprise.Customs.Business;

public class LenientDateTimeConverter : JsonConverter<DateTime>
{
	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.String)
		{
			var dateString = reader.GetString();
			if (DateTime.TryParse(dateString, null, System.Globalization.DateTimeStyles.RoundtripKind, out var date))
			{
				return date;
			}
		}
		throw new JsonException($"Unable to convert \"{reader.GetString()}\" to DateTime.");
	}

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK"));
	}
}
