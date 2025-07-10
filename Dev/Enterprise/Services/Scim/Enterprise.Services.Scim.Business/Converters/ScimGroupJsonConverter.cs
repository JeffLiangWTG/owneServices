using System;
using System.Linq;
using Enterprise.Services.Scim.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.Services.Scim.Business
{
	public class ScimGroupJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(ScimGroup);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return reader != null ? JObject.Load(reader).ToObject<ScimGroup>() : null;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			var t = JToken.FromObject(value);
			var o = (JObject)t;

			var propertyNames = o.Properties().Select(p => p.Name).ToList();
			o.Remove(AttributeNames.Id);
			o.WriteTo(writer);
		}
	}
}
