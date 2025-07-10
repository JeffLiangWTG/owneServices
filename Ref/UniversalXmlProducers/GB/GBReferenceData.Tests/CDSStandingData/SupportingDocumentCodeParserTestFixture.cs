using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.GBReferenceData.Business.CDSStandingData;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using NUnit.Framework;
using static CargoWise.RefDbRepo.GBReferenceData.Tests.Common.CommonHelpers;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData
{
	class SupportingDocumentCodeParserTestFixture : CDSStandingDataTests
	{
		protected override string CodeListID => "DC44";

		protected override void RunParser(IWebClientWrapper wrapper)
		{
			var supportingDocument = new SupportingDocumentCodeParser(
				TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.SupportingDocNational.html"),
				TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.SupportingDocStatus.html"),
				TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.SupportingDocumentUnion.html"),
				wrapper);
			supportingDocument.ExportXml(outputPath);
		}

		[Test]
		public void PublishedDate()
		{
			var parser = new SupportingDocumentCodeParserForTest();
			var nationalPage = new CDSWebpageTableToDataTable("<html></html>");
			var unionPage = new CDSWebpageTableToDataTable("<html></html>");

			var publishDate = parser.CalculatePublishDate_Exposed(nationalPage, unionPage);
			Assert.That(publishDate, Is.EqualTo(new DateTime(1900, 01, 01, 00, 00, 00)));

			var nationalHtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.SupportingDocNational.html");
			var unionHtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.SupportingDocumentUnion.html");

			nationalPage = new CDSWebpageTableToDataTable(nationalHtml);
			unionPage = new CDSWebpageTableToDataTable(unionHtml);
			publishDate = parser.CalculatePublishDate_Exposed(nationalPage, unionPage);
			Assert.That(publishDate, Is.EqualTo(new DateTime(2021, 10, 29)));

			nationalPage = new CDSWebpageTableToDataTable(nationalHtml.Replace("23 June 2021", "7 December 2021"));
			publishDate = parser.CalculatePublishDate_Exposed(nationalPage, unionPage);
			Assert.That(publishDate, Is.EqualTo(new DateTime(2021, 12, 7)));
		}

		[Test]
		public void GetStatusCodes()
		{
			var statusHtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.SupportingDocStatus.html");
			var parser = new SupportingDocumentCodeParserForTest(
				SupportingDocumentCodeParserForTest.EmptyHtml,
				statusHtml,
				SupportingDocumentCodeParserForTest.EmptyHtml);

			var results = parser.GetStatusCodes_Exposed();

			Assert.That(results, Is.Not.Null.And.Not.Empty);
			Assert.That(results.Count, Is.EqualTo(41));
		}

		[Test]
		public void GetDocumentData()
		{
			var nationalHtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.SupportingDocNational.html");
			var unionHtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.SupportingDocumentUnion.html");
			var parser = new SupportingDocumentCodeParserForTest(
				nationalHtml,
				SupportingDocumentCodeParserForTest.EmptyHtml,
				unionHtml);

			var results = parser.GetDocumentData_Exposed();

			Assert.That(results, Is.Not.Null);

			var columns = results.Columns.Cast<DataColumn>().Select(x => $"'{x.ColumnName}'").ToList();
			var columnNames = string.Join(",", columns);

			Assert.That(columnNames, Is.EqualTo("'Code','I/E/B','Description','Details','StatusCodes','ID','Reason'"));

			Assert.That(results.Columns.Count, Is.EqualTo(7));
			Assert.That(results.Rows.Count, Is.EqualTo(638));
		}

		[TestCase("A status code is not required.", 0)]
		[TestCase("No document status code is required", 0)]
		[TestCase("No document status code is required.", 0)]
		[TestCase("No status code is required", 0)]
		[TestCase("No status code is required.", 0)]
		[TestCase("No status code required", 0)]
		[TestCase("AE, AF, AG, AP, AS, AT, GE, GP, JE, JP, LE, LP, UA, UE, UP, US, XB.", 17)]
		[TestCase("", 0)]
		public void TestGetStatusCodesForDoc(string docStatusStr, int expected)
		{
			var parser = new SupportingDocumentCodeParserForTest();
			var validStatusCodes = new List<string> { "AE", "AF", "AG", "AP", "AS", "AT", "GE", "GP", "JE", "JP", "LE", "LP", "UA", "UE", "UP", "US", "XB", "in", "No", "is", "A" };
			var statusCode = parser.GetStatusCodesForDoc(docStatusStr, validStatusCodes);
			Assert.That(statusCode.Count, Is.EqualTo(expected));
		}

		[TestCase("Complete statement \"Reg 2019/2122 exempt\".", "Reg 2019/2122 exempt", Description = "Double quotes (\"\")")]
		[TestCase("Complete statement 'Reg 2019/2122 exempt'.", "Reg 2019/2122 exempt", Description = "Single quotes ('')")]
		[TestCase("Complete Statement \"Reg 2019/2122 exempt\".", "Reg 2019/2122 exempt", Description = "Case sensitivity (Statement)")]
		[TestCase("Complete statement: ‘Handloom products stamped’ ...", "Handloom products stamped", Description = "Semi-colon after statement")]
		[TestCase("‘Complete statement \"Reg 765/2006 exempt\".", "Reg 765/2006 exempt", Description = "Line begin with 0x2018 (‘)")]
		[TestCase("‘Complete statement: ‘Reg 765/2006 exempt’.", "Reg 765/2006 exempt", Description = "Matching quotes (‘’)")]
		[TestCase("‘Complete statement: ‘Reg 765/2006 exempt\".", "Reg 765/2006 exempt", Description = "Mismatch quotes (‘\")")]
		[TestCase("Complete statement “Mercury laboratory use” in the Document Identifier (Second Component)", "Mercury laboratory use", Description = "Matching quotes (“”)")]
		[TestCase("Complete either statement ‘Education and taxidermy only‘ or ‘No cat or dog fur’.",
			new[] { "Education and taxidermy only", "No cat or dog fur" }, Description = "Mismatch quotes (‘‘)")]
		[TestCase("Complete statement: ‘Exempt sports calibre .22.’ Use of this code constitutes ...", "Exempt sports calibre .22", Description = "Statement with full-stop")]
		[TestCase("Complete 'Article 1e(3)(b) exemption' or 'Article 1f(3)(a) exemption' as appropriate,' for exemption covering ...\r\n" +
			"Complete 'Article 1e(3)(c) exemption' or 'Article 1f(3)(a) exemption' as appropriate, for exemption covering ...;\r\n",
			new[] { "Article 1e(3)(b) exemption", "Article 1e(3)(c) exemption", "Article 1f(3)(a) exemption" }, Description = "Mismatch quotes (') after comma")]
		[TestCase("Complete statement 'Reg 2017/1509 exempt'. Use of this code ... are travellers' personal effects ... for travellers' personal use ... " +
			"from Democratic People's Republic of Korea.", "Reg 2017/1509 exempt", Description = "Single quote used in other context (travellers', People's)")]
		[TestCase("Complete statement ' Exported from US before 10.11.2020 '.", "Exported from US before 10.11.2020", Description = "Single quotes ('') but text prepended/appended with space")]
		[TestCase("Complete statement 'Reg 833/2014 exempt'. " +
			"Use of this code ... of Regulation (EU) 833/2014 - 'The prohibition in paragraph 1 shall not apply to purchases in Russia ... and their immediate family members'.",
			"Reg 833/2014 exempt", Description = "Exclude long description in single quotes ('')")]
		[TestCase("‘Complete statement as follows depending on the entitlement to exemption - \r\n" +
			"Complete \"Article 1e(3)(a) exemption\" or \"Article 1f(3)(a) exemption\" as appropriate, for exemption covering ...\r\n" +
			"Complete \"Article 1e(3)(b) exemption\" or \"Article 1f(3)(a) exemption\" as appropriate, for exemption covering ...",
			new[] { "Article 1e(3)(a) exemption", "Article 1e(3)(b) exemption", "Article 1f(3)(a) exemption" }, Description = "Multi-line matches (with duplicated matches)")]
		[TestCase("Complete statement - \r\n" +
			"Import \"Not from Donetsk or Luhansk\"\r\n" +
			"Export \"Not for Donetsk or Luhansk\"\r\n" +
			"Use of this code constitutes ...",
			new[] { "Not for Donetsk or Luhansk", "Not from Donetsk or Luhansk" }, Description = "Line begin with \"Import\"/\"Export\"")]
		[TestCase("Complete statement: Exempt from Reg 1005/2008 Use of this code constitutes ...", "Exempt from Reg 1005/2008", Description = "Statement without quotes")]
		[TestCase("Complete ‘Article 1s(2)(f) exemption for exemption covering ensuring cyber-security ...", "Article 1s(2)(f) exemption", Description = "Missing end quote")]
		[TestCase("DO NOT complete statement \"Reg 2019/2122 exempt\" nor \"Reg 765/2006 exempt\" ...", new string[] { }, Description = "No match")]
		[TestCase("Complete a statement outlining which specific derogation is being claimed.", new string[] { }, Description = "No match")]
		[TestCase("", new string[] { }, Description = "Empty string")]
		public void TestGetStatementsToComplete(string detailsText, params string[] expected)
		{
			var parser = new SupportingDocumentCodeParserForTest();
			var statements = parser.GetStatementsToComplete(detailsText);
			CollectionAssert.AreEqual(expected, statements);
		}

		[Test]
		public void AttributeValueTruncatedWhenTooLong()
		{
			var expectedResult = "".PadRight(255, 'A');
			var valueWithExtra = $"{expectedResult}BBB IS Extra";

			var parser = new SupportingDocumentCodeParserForTest();
			var attribList = new List<RefCusCodeListAttribute>();

			parser.AddRefCusCodeListAttribute_Exposed(attribList, "ABC", valueWithExtra);

			var value = attribList.Single().ZZE_Value;

			Assert.That(value.Length, Is.EqualTo(255));
			Assert.That(value, Is.EqualTo(expectedResult));
		}
	}

	class SupportingDocumentCodeParserForTest : SupportingDocumentCodeParser
	{
		public const string EmptyHtml = "<html></html>";

		public SupportingDocumentCodeParserForTest() : this(EmptyHtml, EmptyHtml, EmptyHtml) { }
		public SupportingDocumentCodeParserForTest(string nationalHtml, string statusHtml, string unionHtml) : base(nationalHtml, statusHtml, unionHtml, new ManifestClientWrapper()) { }

		public DateTime CalculatePublishDate_Exposed(IWebpageTableToDataTable nationalPage, IWebpageTableToDataTable unionPage) => CalculatePublishDate(nationalPage, unionPage);
		public List<string> GetStatusCodes_Exposed() => base.GetStatusCodes();
		public DataTable GetDocumentData_Exposed() => base.GetDocumentData();
		public void AddRefCusCodeListAttribute_Exposed(List<RefCusCodeListAttribute> codelistAttributes, string zze_ZXE_NKName, string zze_Value) => AddRefCusCodeListAttribute(codelistAttributes, zze_ZXE_NKName, zze_Value);
	}
}
