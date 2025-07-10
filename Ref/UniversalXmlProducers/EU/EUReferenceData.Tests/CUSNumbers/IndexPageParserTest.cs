using System;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Business;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Tests
{
	[TestFixture]
	class IndexPageParserTest
	{
		[Test]
		public void GetPublicationDate_WithDatePresentInExpectedFormat_ShouldParsePublicationDate()
		{
			var content = TestHelper.ReadManifestResourceContentAsString("CargoWise.RefDbRepo.EUReferenceData.Tests.CUSNumbers.TestFiles.Input.Index.html");
			var expected = new DateTime(2021, 8, 2);

			var actual = new IndexPageParser(content).ExtractPublicationTime();
			Assert.That(actual, Is.EqualTo(expected), "Parse from DD-MM-YYYY");
		}

		[Test]
		public void GetPublicationDate_WithDatePresentInNonParsableFormat_ShouldReturnDefaultDate()
		{
			var content = TestHelper.ReadManifestResourceContentAsString("CargoWise.RefDbRepo.EUReferenceData.Tests.CUSNumbers.TestFiles.Input.Index.html");
			var expected = DateTime.MinValue;

			var actual = new IndexPageParser(ReplacePublicationDate(content, "Mar 29th 2025")).ExtractPublicationTime();
			Assert.That(actual, Is.EqualTo(expected), "Parse from unsupported format");
		}

		[Test]
		public void GetPublicationDate_WithNoDatePresent_ShouldReturnDefaultDate()
		{
			var content = TestHelper.ReadManifestResourceContentAsString("CargoWise.RefDbRepo.EUReferenceData.Tests.CUSNumbers.TestFiles.Input.Index.html");
			var expectedPublicationDate = DateTime.MinValue;

			var actual = new IndexPageParser(ReplacePublicationDate(content, string.Empty)).ExtractPublicationTime();
			Assert.That(actual, Is.EqualTo(expectedPublicationDate), "Parse from empty string");
		}

		static string ReplacePublicationDate(string html, string publicationDate) =>
			Regex.Replace(html, $"<span>\\s*{Constants.CUSNumbers.LASTUPDATESTRING}\\s*\\d{{2}}-\\d{{2}}-\\d{{4}}\\s*<\\/span>", $"<span>Last update: {publicationDate}</span>", RegexOptions.Multiline);
	}
}
