using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Tests
{
    public class StringExtensionsTest
    {
		[Test]
		public void TestEmptyIfNoInvalid()
		{
			var input = new[] { "Valid", " ", null, "String", "", "\t" };
			var expected = new[] { "Valid", "String" };

			var result = StringExtensions.EmptyIfNoInvalid(input);

			Assert.That(expected, Is.EqualTo(result));
		}

		[Test]
		[TestCase("Test\nline\n\n\nbreak", "Test line break")]
		[TestCase("Test\rline\rbreak", "Test line break")]
		[TestCase("Test\n\rline\n\rbreak", "Test line break")]
		[TestCase("\r\n\r", " ")]
		public void TestReplaceLineBreaks(string input, string expected)
		{
			string result = StringExtensions.ReplaceLineBreaks(input);
			Assert.That(expected,Is.EqualTo(result));
		}

		[Test]
		[TestCase("ABC   123", "123")]
		[TestCase("Tariff 56-78", "5678")]
		public void TestToPureTariffStrig(string input, string expected)
		{
			string result = StringExtensions.ToPureTariffString(input);
			Assert.That(expected, Is.EqualTo(result));
		}

		[TestCase("Hello, World!", "HELLOWORLD")]
		[TestCase(" 123 @#$ abc ", "123ABC")]
		[TestCase("!@#$%^&*()", "")]
		[TestCase("", "")]
		public void TestToSearchKey_Should_RemoveNonAlphanumericAndConvertToUppercase(string input, string expected)
		{
			var result = input.ToSearchKey();
			Assert.That(expected, Is.EqualTo(result));
		}

		[TestCase("hello world", "HELLO_WORLD", true)]
		[TestCase("hello world!", "hello world", true)]
		[TestCase("TEST123", "test123", true)]
		[TestCase("TEST123", "ABC123", false)]
		public void TestSameSearchKeyWith_Should_CompareSearchKeysCorrectly(string input, string compare, bool expected)
		{
			var result = input.SameSearchKeyWith(compare);
			Assert.That(expected, Is.EqualTo(result));
		}

		[TestCase("   hello  world   ", "helloworld")]
		[TestCase("\t\n\rTest \t String", "TestString")]
		[TestCase("NoSpaces", "NoSpaces")]
		[TestCase("", "")]
		public void TestClearWhiteSpaces_Should_RemoveAllSpaces(string input, string expected)
		{
			var result = input.ClearWhiteSpaces();
			Assert.AreEqual(expected, result);
		}

		[TestCase("Test, Split, By", new[] { "Test", "Split", "By" })]
		[TestCase("Test.Split.By", new[] { "Test", "Split", "By" })]
		[TestCase("apple . banana  grape", new[] { "apple", "bananagrape" })]
		[TestCase("  a,  b.  c ", new[] { "a", "b", "c" })]
		public void TestSplitBy_WithDefaultSeparator(string input, string[] expected)
		{
			var result = StringExtensions.SplitBy(input);
			Assert.That(expected, Is.EqualTo(result));
		}

		[TestCase("With - Custom - Separator", "-", new[] { "With", "Custom", "Separator" })]
		public void TestSplitBy_WithCustomSeparator(string input, string separator, string[] expected)
		{
			var result = StringExtensions.SplitBy(input, separator);
			Assert.That(expected, Is.EqualTo(result));
		}

		[TestCase("Similarity", "Similarity", 1.0)]
		[TestCase("Similarity", "Similarit", 0.9)]
		[TestCase("hello", "hxllo", 0.8)]
		[TestCase("get", "Similarity", 0)]
		public void GetSimilarity_ReturnsExpectedValue(string input1, string input2, decimal expected)
		{
			var result = StringExtensions.GetSimilarity(input1, input2);
			Assert.That(expected, Is.EqualTo(result));
		}
	}
}
