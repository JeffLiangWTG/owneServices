using System;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Common
{
	public class JsonEdmDateConverter : DateTimeConverterBase
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Date?) || objectType == typeof(Date);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.String)
			{
				var dateString = (string)reader.Value;
				if (Date.TryParse(dateString, out Date dateTime))
				{
					return dateTime;
				}
			}
			return null;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value is Date date)
			{
				writer.WriteValue(date.ToString());
			}
		}
	}
}
