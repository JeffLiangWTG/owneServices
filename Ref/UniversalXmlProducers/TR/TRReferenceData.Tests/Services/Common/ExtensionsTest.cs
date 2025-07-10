using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FlexCel.XlsAdapter;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Tests
{
	class ExtensionsTest
	{
		[TestCase(1, 1, "A1")]
		[TestCase(8, 9, "StringToTrim")]
		public void TestGetTrimmedStringFromCell(int row, int column, string expected)
		{
			var xlsFile = new XlsFile(Assembly.GetExecutingAssembly().GetManifestResourceStream(NPOIExtensionsTest.NPOITestFileName), aAllowOverwritingFiles: false) { ActiveSheet = 1 };
			var result = Extensions.GetTrimmedStringFromCell(xlsFile, row, column);
			Assert.That(result, Is.EqualTo(expected));
		}

		[TestCaseSource(nameof(TruncateToMinuteDatas))]
		public void TestTruncateToMinute(DateTime input, DateTime expected)
		{
			Assert.That(Extensions.TruncateToMinute(input), Is.EqualTo(expected));
		}

		static TestCaseData[] TruncateToMinuteDatas => new[]
		{
			new TestCaseData(new DateTime(2024,4,7,12,14,15), new DateTime(2024,4,7,12,14,0))
		};

		[TestCaseSource(nameof(GetOrAddNewDatas))]
		public void TestGetOrAddNew(IDictionary<string, int> dictionary, string inputKey, int expected)
		{
			Assert.That(Extensions.GetOrAddNew(dictionary, inputKey), Is.EqualTo(expected));
		}

		static TestCaseData[] GetOrAddNewDatas
		{
			get
			{
				var dictionary = new Dictionary<string, int>
				{
					{ "Key1", 100 },
					{ "Key2", 200 }
				};

				return new[]
				{
					new TestCaseData(dictionary, "Key2", 200),
					new TestCaseData(dictionary, "Key1", 100),
					new TestCaseData(dictionary, "KeyX", 0),
				};
			}
		}

		[TestCaseSource(nameof(ToArrayDatas))]
		public void TestToArray(IEnumerable<IEnumerable<string>> input, string[][] expected)
		{
			Assert.That(Extensions.ToArray(input), Is.EquivalentTo(expected));
		}

		[TestCase("One", 1)]
		[TestCase("Two", 2)]
		[TestCase("Four", 0)]
		public void TestGetOrDefault_WithInteger_ReturnsExpectedValue(string key, int expected)
		{
			var dictionary = new Dictionary<string, int>
			{
				{ "One", 1 },
				{ "Two", 2 }
			};

			var result = dictionary.GetOrDefault(key);

			Assert.That(expected, Is.EqualTo(result));
		}


		[TestCase("Key1", "Value1")]
		[TestCase("NonExistentKey", null)]
		public void TestGetOrDefault_WithString_ReturnsExpectedValue(string key, string expected)
		{
			var dictionary = new Dictionary<string, string>
			{
				{ "Key1", "Value1" }
			};

			var result = dictionary.GetOrDefault(key);

			Assert.That(expected, Is.EqualTo(result));
		}

		[Test]
		public void TestGetOrDefault_WithEmptyDictionary_ReturnsDefault()
		{
			var dictionary = new Dictionary<string, int>();

			var result = dictionary.GetOrDefault("AnyKey");

			Assert.That(default(int), Is.EqualTo(result));
		}

		[TestCase(new object[] { "Hello", null, "World", null, "!" }, new object[] { "Hello", "World", "!" })]
		[TestCase(new object[] { 1, 2, null, 3 }, new object[] { 1, 2, 3 })]
		[TestCase(new object[] { }, new object[] { })]
		public void TestWhereNotNull_RemovesNullValues(object[] input, object[] expected)
		{
			var result = input.Cast<object>().WhereNotNull().ToList();

			Assert.AreEqual(expected, result);
		}

		static IEnumerable<TestCaseData> ToArrayDatas
		{
			get
			{
				var input = new List<List<string>>
				{
					new List<string>{"11","12","13"},
					new List<string>{"21"},
					new List<string>{"31","32","33","34"}
				};

				var expected = new[] { new string[] { "11", "12", "13" }, new[] { "21" }, new[] { "31", "32", "33", "34" } };
				yield return new TestCaseData(input, expected);
			}
		}
	}
}
