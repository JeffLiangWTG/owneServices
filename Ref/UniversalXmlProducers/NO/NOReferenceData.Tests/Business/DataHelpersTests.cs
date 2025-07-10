using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Tests
{
	sealed class DataHelpersTests
	{
		[TestCase("", true, "2079-06-06T23:59:00")]
		[TestCase("1999-12-24 12:00:00", true, "2000-01-01T23:59:00")]
		[TestCase("2022-02-11 00:00:00", true, "2022-02-11T23:59:00")]
		[TestCase("2099-12-31 19:02:43", true, "2079-06-06T23:59:00")]
		[TestCase("2022-13-32", false, "0001-01-01T23:59:00")]
		public void GetEndDateTime(string inputValue, bool expectedParseResult, DateTime expectedDateTime)
		{
			var (result, parsedDate) = DataHelpers.TryParseEndDateTime(inputValue, Constants.FullDateTimeFormat);

			Assert.Multiple(() =>
			{
				Assert.That(result, Is.EqualTo(expectedParseResult), "Parsing of date");
				Assert.That(parsedDate, Is.EqualTo(expectedDateTime), "Date Time value");
			});
		}

		[TestCase("1999-12-24 12:34:57", true, "2000-01-01T00:00:00")]
		[TestCase("2022-02-11 15:31:22", true, "2022-02-11T15:31:22")]
		[TestCase("2099-12-31 19:02:43", true, "2079-06-06T23:59:00")]
		[TestCase("2022-13-32", false, "0001-01-01T00:00:00")]
		public void GetDateTime(string inputValue, bool expectedParseResult, DateTime expectedDateTime)
		{
			var (result, parsedDate) = DataHelpers.TryParseDateTime(inputValue, "yyyy-MM-dd HH:mm:ss");

			Assert.Multiple(() =>
			{
				Assert.That(result, Is.EqualTo(expectedParseResult), "Parsing of date");
				Assert.That(parsedDate, Is.EqualTo(expectedDateTime), "Date Time value");
			});
		}

		[TestCase("123", "123")]
		[TestCase("0,123", "0,123")]
		[TestCase(",1", ",1")]
		[TestCase("123A", "123")]
		[TestCase("X,123", ",123")]
		[TestCase("0.1", "01")]
		[TestCase(",X", ",")]
		public void KeepNumerics(string inputValue, string expectedValue)
		{
			var result = DataHelpers.KeepNumerics(inputValue);
			Assert.That(result, Is.EqualTo(expectedValue));
		}

		[TestCase(@"abcde", @"abcde")]
		[TestCase(@"&amp;#34;Cocks&amp;#96; foot&amp;#34;", "&#34;Cocks&#96; foot&#34;")]
		public void TestCleanString(string input, string expected)
		{
			var result = DataHelpers.CleanHtmlStringIfApplicable(input);
			Assert.That(result, Is.EqualTo(expected));
		}
	}
}
