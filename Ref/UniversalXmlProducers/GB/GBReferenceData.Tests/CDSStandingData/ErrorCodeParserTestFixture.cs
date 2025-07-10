using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.GBReferenceData.Business.CDSStandingData;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using NUnit.Framework;
using static CargoWise.RefDbRepo.GBReferenceData.Tests.Common.CommonHelpers;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData
{
	class ErrorCodeParserTestFixture : CDSStandingDataTests
	{
		protected override string CodeListID => "ERRCD";

		protected override void RunParser(IWebClientWrapper wrapper)
		{
			var errorCodeParser = new ErrorCodeParser(wrapper,
				TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.CDSErrorCodePage.html"));
			errorCodeParser.ExportXml(outputPath);
		}

		[Test]
		public void GetData()
		{
			var html = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.CDSErrorCodePage.html");
			var parser = new ErrorCodeParserForTest(html);

			var results = parser.GetData_Exposed();

			Assert.That(results, Is.Not.Null);

			var columns = results.Columns.Cast<DataColumn>().Select(x => $"'{x.ColumnName}'").ToList();
			var columnNames = string.Join(",", columns);

			Assert.That(columnNames, Is.EqualTo("'Code','Description','Explanation'"));

			Assert.That(results.Columns.Count, Is.EqualTo(3));
			Assert.That(results.Rows.Count, Is.EqualTo(162));
		}

		[Test]
		public void FormatDescription()
		{
			var parser = new ErrorCodeParserForTest(string.Empty);

			var description = "Random description:\nHere is a line feed.";
			var explanation = @"An arb explanation

Which has got blank lines and line feeds in it.";

			var result = parser.FormatDescription_Exposed(description, explanation);
			Assert.That(result, Is.EqualTo("An arb explanation; Which has got blank lines and line feeds in it. Random description:; Here is a line feed."));

			description = null;
			explanation = "EXPL NOT NULL";
			result = parser.FormatDescription_Exposed(description, explanation);
			Assert.That(result, Is.EqualTo("EXPL NOT NULL"));

			description = "DESC NOT NULL";
			explanation = null;
			result = parser.FormatDescription_Exposed(description, explanation);
			Assert.That(result, Is.EqualTo("DESC NOT NULL"));
		}
	}

	class ErrorCodeParserForTest : ErrorCodeParser
	{
		public ErrorCodeParserForTest(string errorCodePageHtml) : base(new ManifestClientWrapper(), errorCodePageHtml)
		{
		}

		public DataTable GetData_Exposed() => base.GetData();
		public string FormatDescription_Exposed(string description, string explanation) => FormatDescription(description, explanation);
	}
}
