using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.GBReferenceData.Business.CDSStandingData;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using NUnit.Framework;
using static CargoWise.RefDbRepo.GBReferenceData.Tests.Common.CommonHelpers;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData
{
	class AdditionalInformationParserTestFixture : CDSStandingDataTests
	{
		protected override string CodeListID => "ADDIN";

		protected override void RunParser(IWebClientWrapper wrapper)
		{
			var additionalInfo = new AdditionalInformationParser(
				TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.AdditionalInformation.html"),
				wrapper);
			additionalInfo.ExportXml(outputPath);
		}

		[Test]
		public void GetAdditionalData()
		{
			var html = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.AdditionalInformation.html");
			var parser = new AdditionalInformationParserForTest(html);

			var results = parser.GetAdditionalData_Exposed();
			Assert.That(results, Is.Not.Null);

			var columns = results.Columns.Cast<DataColumn>().Select(x => $"'{x.ColumnName}'").ToList();
			var columnNames = string.Join(",", columns);

			Assert.That(columnNames, Is.EqualTo("'Code','Description','Details','I/E/B'"));

			Assert.That(results.Columns.Count, Is.EqualTo(4));
			Assert.That(results.Rows.Count, Is.EqualTo(221));
		}

		[Test]
		public void ProduceData()
		{
			var wrapper = new ManifestClientWrapper
			{
				TestDate = new DateTime(2022, 2, 11, 12, 13, 14)
			};

			using (var expectedTestStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Output.RefCusCodeListZZ_CDS_{CodeListID}.xml"))
			{
				RunParser(wrapper);
				using (var resultStream = new FileStream(Path.Combine(outputPath, $"GB_RefCusCodeListZZ_CDS_{CodeListID}.xml"), FileMode.Open))
				{
					using (TextReader trResult = new StreamReader(resultStream))
					using (TextReader trExpected = new StreamReader(expectedTestStream))
					{
						var result = trResult.ReadToEnd();
						var expected = trExpected.ReadToEnd();

						Assert.That(result, Is.EqualTo(expected));
					}
				}
			}
		}

		[Test]
		public void GetLevel()
		{
			var itemLevel = "ITEM";
			var headerLevel = "HEADER";
			var parser = new AdditionalInformationParserForTest(string.Empty);

			var code = "ABC99";
			var description = "This should be ITEM level";
			Assert.That(parser.GetLevel_Exposed(code, description), Is.EqualTo(itemLevel));

			code = "RRS01";
			Assert.That(parser.GetLevel_Exposed(code, description), Is.EqualTo(headerLevel));

			code = "PRO02";
			description = "Authorisation by Customs Declaration only: Manufacture or process of tobacco goods. Note: PRO02 may only be used where AI code 00100 is also declared in DE 2/2.";
			Assert.That(parser.GetLevel_Exposed(code, description), Is.EqualTo(itemLevel));

			description = "Something special NoTe: ThIs mUsT Be eNtErEd At hEaDeR LeVeL.";
			Assert.That(parser.GetLevel_Exposed(code, description), Is.EqualTo(headerLevel));
		}
	}

	class AdditionalInformationParserForTest : AdditionalInformationParser
	{
		public AdditionalInformationParserForTest(string html) : base(html, new ManifestClientWrapper()) { }

		public DataTable GetAdditionalData_Exposed() => GetAdditionalData();
		public string GetLevel_Exposed(string code, string description) => GetLevel(code, description);
	}
}
