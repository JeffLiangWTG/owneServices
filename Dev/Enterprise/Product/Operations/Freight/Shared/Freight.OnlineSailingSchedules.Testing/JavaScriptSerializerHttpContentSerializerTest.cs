using System.Collections.Generic;
using System.Text.Json;
using NUnit.Framework;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	class JavaScriptSerializerHttpContentSerializerTest : TestCase
	{
		public void TestDeserialize_String()
		{
			AssertEquals("test", JavaScriptSerializerHttpContentSerializer.Deserialize<string>("\"test\""));
		}

		public void TestDeserialize_StringToIListOfStringDictionary()
		{
			var expected = new Dictionary<string, List<string>>()
			{
				["One"] = ["1"],
				["Two"] = ["1", "2"]
			};

			var actual = JavaScriptSerializerHttpContentSerializer.Deserialize<Dictionary<string, IList<string>>>("{\"One\":[\"1\"],\"Two\":[\"1\",\"2\"]}");

			AssertEquals(expected.Count, actual.Count);

			foreach (var expectedEntry in expected)
			{
				Assert($"Expected key '{expectedEntry.Key}' not found in actual dictionary.", actual.TryGetValue(expectedEntry.Key, out var actualValue));
				AssertContainsExactElementsInExactOrder($"Values for key '{expectedEntry.Key}' do not match.", expectedEntry.Value, actualValue);
			}
		}

		public void TestDeserialize_AutoFormatNumberToString()
		{
			AssertEquals("123", JavaScriptSerializerHttpContentSerializer.Deserialize<string>("123"));
			AssertEquals("123.456", JavaScriptSerializerHttpContentSerializer.Deserialize<string>("123.456"));

			var actual = JavaScriptSerializerHttpContentSerializer.Deserialize<NumberAsString>("{\"Number\": 1234567890}");
			AssertNotNull(actual);
			AssertEquals("1234567890", actual.Number);
		}

		public void TestDeserialize_AutoFormatObjectToString()
		{
			AssertExceptionThrown(typeof(JsonException), "Unsupported json token type: StartObject", () =>
				JavaScriptSerializerHttpContentSerializer.Deserialize<string>("{\"Number\": {\"val\":123}}"));
		}

		public void TestDeserialize_CaseInsensitivePropertyNames()
		{
			var actual = JavaScriptSerializerHttpContentSerializer.Deserialize<NumberAsString>("{\"nUMBER\": 1234567890}");
			AssertNotNull(actual);
			AssertEquals("1234567890", actual.Number);
		}

		class NumberAsString
		{
			public string Number { get; set; }
		}
	}
}
