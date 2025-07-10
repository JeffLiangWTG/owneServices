using System.Text.Json;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing.JsonConverters;

class StringToNumberConverterTest : TestCase
{
	readonly JsonSerializerOptions options = new()
	{
		Converters = { new StringToNumberConverter(false) }
	};

	class Person
	{
		public string Name { get; set; }
		public int Age { get; set; }
		public int Tenure { get; set; }
	}

	public void TestRead()
	{
		var json = "{\"Name\":\"John Doe\",\"Age\":\"43\", \"Tenure\": 3}";
		var model = JsonSerializer.Deserialize<Person>(json, options);

		AssertEquals(43, model.Age);
		AssertEquals(3, model.Tenure);
	}

	public void TestReadUnexpectedToken()
	{
		var json = "{\"Name\":\"John Doe\",\"Age\":\"43\", \"Tenure\": false}";
		AssertExceptionThrown<JsonException>(() => JsonSerializer.Deserialize<Person>(json, options));
	}

	public void TestReadWithNull()
	{
		var json = "{\"Name\":\"John Doe\",\"Age\":null, \"Tenure\": 3}";
		var model = JsonSerializer.Deserialize<Person>(json,
			new JsonSerializerOptions { Converters = { new StringToNumberConverter(true) } });

		AssertEquals(default(int), model.Age);
		AssertEquals(3, model.Tenure);
	}

	public void TestWrite()
	{
		var model = new Person { Name = "John Doe", Age = 43, Tenure = 3 };
		var json = JsonSerializer.Serialize(model, options);

		AssertEquals("{\"Name\":\"John Doe\",\"Age\":43,\"Tenure\":3}", json);
	}
}
