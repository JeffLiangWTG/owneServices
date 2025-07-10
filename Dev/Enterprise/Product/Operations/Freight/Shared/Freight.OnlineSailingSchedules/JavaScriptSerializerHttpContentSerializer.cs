using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Freight.Integration.ApiClient;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public class JavaScriptSerializerHttpContentSerializer : IHttpContentSerializer
	{
		public async Task<T> FromHttpContentAsync<T>(HttpContent content, CancellationToken cancellationToken = default)
		{
			var data = await content.ReadAsStringAsync().ConfigureAwait(false);
			return Deserialize<T>(data);
		}

		public async Task<Exception> SerializerErrorHandler(Exception e, HttpResponseMessage message)
		{
			var data = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
			var errorMessage = ResString.GetMultilingualString("9C5BF947-9EA5-4FF2-9992-6C901A42CCDA", @"{0}
Data: {1}", e.Message, data);
			return new System.Runtime.Serialization.SerializationException(errorMessage);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "MakeReadOnly() has been called, so it is thread safe")]
		static readonly Lazy<JsonSerializerOptions> jsonSerializerOptions = new(() =>
		{
			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};
			options.Converters.Add(new AutoNumberToStringConverter());
			options.TypeInfoResolver = JsonSerializerOptions.Default.TypeInfoResolver;
			options.MakeReadOnly();
			return options;
		});

		internal static T Deserialize<T>(string data)
		{
			return JsonSerializer.Deserialize<T>(data, jsonSerializerOptions.Value);
		}

		class AutoNumberToStringConverter : JsonConverter<string>
		{
			public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				return reader.TokenType switch
				{
					JsonTokenType.Number => reader.TryGetInt64(out long l)
											? l.ToString()
											: reader.GetDouble().ToString(),
					JsonTokenType.String => reader.GetString(),
					_ => throw new JsonException($"Unsupported json token type: {reader.TokenType}"),
				};
			}

			public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
			{
				writer.WriteStringValue(value);
			}
		}
	}
}
