using System.Text.Json;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing.JsonConverters;

class NumberToStringConverterTest : TestCase
{
	readonly JsonSerializerOptions options = new()
	{
		Converters = { new NumberToStringConverter() }
	};

	class Person
	{
		public string Name { get; set; }
		public string Age { get; set; }
	}

	public void TestRead()
	{
		var json = "{\"Name\":\"John Doe\",\"Age\":43}";
		var model = JsonSerializer.Deserialize<Person>(json, options);

		AssertEquals("John Doe", model.Name);
		AssertEquals("43", model.Age);
	}

	public void TestReadUnexpectedToken()
	{
		var json = "{\"Name\":true,\"Age\":43}";
		AssertExceptionThrown<JsonException>(() => JsonSerializer.Deserialize<Person>(json, options));
	}

	public void TestWrite()
	{
		var model = new Person { Name = "John Doe", Age = "43" };
		var json = JsonSerializer.Serialize(model, options);

		AssertEquals("{\"Name\":\"John Doe\",\"Age\":\"43\"}", json);
	}
}
