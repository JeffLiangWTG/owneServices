using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Enterprise.Customs.Business;

public class StringToNumberConverter(bool nullAsDefault) : JsonConverter<int>
{
	public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Number)
		{
			return reader.GetInt32();
		}
		else if (reader.TokenType == JsonTokenType.String)
		{
			if (int.TryParse(reader.GetString(), out var result))
			{
				return result;
			}
		}
		else if (nullAsDefault && reader.TokenType == JsonTokenType.Null)
		{
			return 0;
		}
		throw new JsonException("Unexpected token type");
	}

	public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
	{
		writer.WriteNumberValue(value);
	}
}
