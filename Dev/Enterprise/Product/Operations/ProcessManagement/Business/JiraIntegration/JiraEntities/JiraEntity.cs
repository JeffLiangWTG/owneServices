using System;
using CargoWise.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.ProcessManagement.Business
{
	public abstract class JiraEntity
	{
		#region Token Parsing

		protected static string ParseStringField(JToken token, string fieldConstant)
		{
			return (string)(GetValueToParse(token, fieldConstant) ?? string.Empty);
		}

		protected static bool ParseBooleanField(JToken token, string fieldConstant)
		{
			return (bool)(GetValueToParse(token, fieldConstant) ?? false);
		}

		protected static bool TryParseToken(JToken token, string key, out JToken result)
		{
			result = token[key];

			return IsJTokenUseable(result);
		}

		protected static bool IsJTokenUseable(JToken token)
		{
			return JiraToCargoWiseDecoder.IsJTokenUseable(token);
		}

		protected ZDateTime ParseDateField(JToken token, string fieldConstant)
		{
			var settings = new JsonSerializerSettings
			{
				DateFormatHandling = DateFormatHandling.IsoDateFormat,
				DateTimeZoneHandling = DateTimeZoneHandling.Utc
			};

			var value = GetValueToParse(token, fieldConstant);

			if (value != null)
			{
				var parent = value.Parent;
				var parentString = "{ " + parent + " }";

				var converted = JsonConvert.DeserializeObject<JsonCreatedDate>(parentString, settings);

				return converted.created;
			}

			return ZDateTime.Invalid;
		}

		static JToken GetValueToParse(JToken token, string fieldConstant)
		{
			if (IsJTokenUseable(token))
			{
				var valueToAssign = token[fieldConstant];

				if (valueToAssign != null)
				{
					return valueToAssign;
				}
			}

			return null;
		}

		class JsonCreatedDate
		{
			public DateTime created { get; set; }
		}

		#endregion Token Parsing
	}
}
