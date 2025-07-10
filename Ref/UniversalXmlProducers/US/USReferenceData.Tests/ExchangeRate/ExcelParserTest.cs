using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.USReferenceData.Business.ExchangeRate;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	[SetCulture("en-AU")]
	sealed class ExcelParserTest
	{
		[TestCase]
		public void TestParse()
		{
			var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ExchangeRate\TestFiles\Input");
			var filePath = Path.Combine(dir, @"Test_Daily_Currency_Notice.xlsx");

			using (var stream = new FileStream(filePath, FileMode.Open))
			{
				var schemas = ExcelParser.Parse(stream, new ExcelParserConfiguration() { StartingRow = 9, LastRow = 10 }, new DateTime(2024, 1, 3), out var publishDate);

				var expectedSchemas = @"AU/AUSTRALIA/0.7604/Q/AUD
VE/VENEZUELA/N/A/D/VEF";
				var actualSchemas = string.Join(Environment.NewLine, schemas.Select(c => ConvertSchemaToString(c)));

				Assert.AreEqual(new DateTime(2021, 4, 20), publishDate);
				Assert.AreEqual(expectedSchemas, actualSchemas, "Should only get data from these rows which row index are between the StartingRow and the LastRow of ExcelParserConfiguration.");
			}
		}

		[TestCase]
		public void TestParseWithInvalidDate()
		{
			var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ExchangeRate\TestFiles\Input");
			var filePath = Path.Combine(dir, @"Test_Daily_Currency_Notice_InvalidDate.xlsx");

			using (var stream = new FileStream(filePath, FileMode.Open))
			{
				ExcelParser.Parse(stream, new ExcelParserConfiguration() { StartingRow = 9, LastRow = 10 }, new DateTime(2024, 1, 3), out var publishDate);
				Assert.Null(publishDate);
			}
		}

		string ConvertSchemaToString(ExchangeRateSchema schema)
		{
			return $"{schema.ISOCode}/{schema.CountryUnion}/{schema.Rate}/{schema.IND}/{schema.CURCode}";
		}
	}
}
