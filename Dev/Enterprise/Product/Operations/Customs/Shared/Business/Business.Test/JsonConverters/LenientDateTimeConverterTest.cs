using System;
using System.Text.Json;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing.JsonConverters;

class LenientDateTimeConverterTest : TestCase
{
	readonly JsonSerializerOptions options = new()
	{
		Converters = { new LenientDateTimeConverter() }
	};

	class DateTimeModel
	{
		public DateTime DateTime { get; set; }
	}

	public void TestReadIso8601DateTimeWithTimeZoneOffset()
	{
		var json = "{\"DateTime\":\"2023-06-30T23:22:41.000+0000\"}";
		var model = JsonSerializer.Deserialize<DateTimeModel>(json, options);

		AssertEquals(new DateTime(2023, 6, 30, 23, 22, 41), model.DateTime.ToUniversalTime());
	}

	public void TestReadIso8601DateTime()
	{
		var json = "{\"DateTime\":\"2023-06-30T23:22:41.000Z\"}";
		var model = JsonSerializer.Deserialize<DateTimeModel>(json, options);

		AssertEquals(new DateTime(2023, 6, 30, 23, 22, 41), model.DateTime);
	}

	public void TestReadUnexpectedToken()
	{
		var json = "{\"DateTime\":false}";
		AssertExceptionThrown<JsonException>(() => JsonSerializer.Deserialize<DateTimeModel>(json, options));
	}

	public void TestWrite()
	{
		var model = new DateTimeModel { DateTime = new DateTime(2023, 6, 30, 23, 22, 41) };
		var json = JsonSerializer.Serialize(model, options);

		AssertContains("2023-06-30T23:22:41", json);
	}
}
