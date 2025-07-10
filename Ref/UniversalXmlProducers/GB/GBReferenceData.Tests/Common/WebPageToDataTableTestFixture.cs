using System;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Common
{
	[TestFixture]
	public class WebPageToDataTableTestFixture
	{
		[Test]
		public void WebPageToDataTableFixture()
		{
			var dataXpath = @"//html[1]//body[1]//div[5]//main[1]//div[3]//div[1]//div[1]//div[1]//div[1]//table[1]//tbody[1]//tr";
			var headerXpath = @"//html[1]//body[1]//div[5]//main[1]//div[3]//div[1]//div[1]//div[1]//div[1]//table[1]//thead[1]//tr";
			var html = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.PreviousDocumentCodes.html");
			var cdsWebPageToDataTable = new CDSWebpageTableToDataTable(html);
			var dt = cdsWebPageToDataTable.ExtractDatatableFromGrid(headerXpath, dataXpath);

			Assert.That(dt.Rows.Count, Is.EqualTo(39));
			Assert.That(dt.Columns[0].ColumnName, Is.EqualTo("Code to be declared"));
			Assert.That(dt.Columns[1].ColumnName, Is.EqualTo("Document name/type to be used as the previous reference"));
			Assert.That(dt.Rows[0]["Code to be declared"], Is.EqualTo("235"));
			Assert.That(dt.Rows[0]["Document name/type to be used as the previous reference"], Is.EqualTo("Container list"));
		}

		[Test]
		public void ExtractPublishDate()
		{
			var dateXpath = @"//*[@id='history']/text()[2]";
			var html = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.SupportingDocumentUnion.html");
			var cdsWebPageToDataTable = new CDSWebpageTableToDataTable(html);

			var utcNow = DateTime.UtcNow;
			var defaultDate = new DateTime(1945, 8, 22);

			var publishDate = cdsWebPageToDataTable.ExtractPublishDate(dateXpath);
			Assert.That(publishDate, Is.EqualTo(new DateTime(2021, 10, 29)));

			var invalidXPath = @"//*[@id='this-is-wrong']/text()[2]";
			publishDate = cdsWebPageToDataTable.ExtractPublishDate(invalidXPath);
			Assert.That(publishDate, Is.GreaterThanOrEqualTo(utcNow));

			publishDate = cdsWebPageToDataTable.ExtractPublishDate(invalidXPath, defaultDate);
			Assert.That(publishDate, Is.EqualTo(defaultDate));
		}

		[Test]
		public void ExtractUrlFromAnchor()
		{
			var unionDataAnchorText = "Data Element 2/3 Documents and Other Reference Codes (Union) (Appendix 5A)";
			var html = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.SupportingDocumentUnion.html");
			var cdsWebPageToDataTable = new CDSWebpageTableToDataTable(html);

			var url = cdsWebPageToDataTable.ExtractUrlFromAnchor(unionDataAnchorText);
			Assert.That(url.OriginalString, Is.EqualTo("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.SupportingDocumentUnionCodes.ods"));
		}

		[Test]
		public void ExtractUrlFromAnchor_NoAnchors()
		{
			var anchorText = "Any text";
			var htmlWithNoAnchors = "<html></html>";
			var cdsWebPageToDataTable = new CDSWebpageTableToDataTable(htmlWithNoAnchors);

			var url = cdsWebPageToDataTable.ExtractUrlFromAnchor(anchorText);
			Assert.That(url, Is.Null);
		}

		[TestCase("ods")]
		[TestCase(".ods")]
		[TestCase(".ODS")]
		public void ExtractSingleUrlFromFileExtension(string fileExtension)
		{
			var html = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.ExtractSingleUrlFromFileExtension.html");
			var cdsWebPageToDataTable = new CDSWebpageTableToDataTable(html);

			var url = cdsWebPageToDataTable.ExtractSingleUrlFromFileExtension(fileExtension);
			Assert.That(url.OriginalString, Is.EqualTo("https://example.com/path/to/link-4.ods"));
		}

		[TestCase(".xls")]
		[TestCase(".xlsx")]
		public void ExtractSingleUrlFromFileExtension_NoSingleUrl(string fileExtension)
		{
			var html = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.ExtractSingleUrlFromFileExtension.html");
			var cdsWebPageToDataTable = new CDSWebpageTableToDataTable(html);

			var url = cdsWebPageToDataTable.ExtractSingleUrlFromFileExtension(fileExtension);
			Assert.That(url, Is.Null);
		}
	}
}
