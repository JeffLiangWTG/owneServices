using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	sealed class HSNTariffRatesParserTest
	{
		[Test]
		public void ParserTest()
		{
			using var sJson = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.TariffRates.TariffRates.json");
			using var expectedXML = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.RefCusTariffRates_BR_HSN.xml");
			using (var inputReader = new StreamReader(sJson, Encoding.UTF8))
			{
				var json = inputReader.ReadToEnd();
				var parser = new HSNTariffRatesParser("Customs Tariff Rates file");

				Assert.DoesNotThrow(() => parser.ExportToXMLFile(JsonConvert.DeserializeObject<IEnumerable<TariffDTO>>(json), TestOutputFilePath, new System.DateTime(2025, 05, 16)));

				using var generatedFile = new FileStream(TestOutputFilePath,FileMode.Open);

				StreamCompareHelper.CompareStreamContent(expectedXML, generatedFile);
			}
		}

		[SetUp]
		[TearDown]
		public void Cleanup()
		{
			File.Delete(TestOutputFilePath);
		}

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.RefCusTariff_BR_Rates.xml");
	}
}
