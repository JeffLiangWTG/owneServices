using System;
using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class TariffTributaryParserTest
	{
		[Test]
		public void TestListHasUnknownData()
		{
			using (var stream = new StreamReader(Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.ttce-importacao-dados-para-servico-duimp-atualizado-20231107_Unknown.json")))
			{
				var tariffTributary = JsonConvert.DeserializeObject<TariffTributary>(stream.ReadToEnd());

				var parser = new TariffTributaryParser("Test");
				parser.ExportToXMLFile(tariffTributary);
				var ex = Assert.Throws<InvalidOperationException>(() => { ParserErrorCollector.Instance.ReportErrors(); });
				Assert.AreEqual("Unknown block name founded (UNIÃO EUROPÉIA - UE)\r\nUnknown tax code founded (DON'T KNOW)\r\n", ex.Message);
			}
		}
	}
}
