using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class TariffCharacteristicNCMParserTest
	{
		[Test]
		public void TestXmlExportForNcm()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.RefCusTariffBRCharacteristicNcm_BR_HSN.xml"))
			{
				var publicationDate = new DateTime(2021, 11, 26, 00, 00, 00);
				var parser = new TariffCharacteristicNCMParser("BR Tariff Characteristic NCM");

				parser.ExportToXMLFile(attributes, tariffs, TestOutputFilePath, publicationDate, Constants.RefCusTariffBRCharacteristicCodes.CharacteristicTypes.NCM);
				using (var outputStream = new FileStream(TestOutputFilePath, FileMode.Open))
				{
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
				}
			}
		}

		[Test]
		public void TestXmlExportForNcmTe()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.RefCusTariffBRCharacteristicNcmTest_BR_HSN.xml"))
			{
				var publicationDate = new DateTime(2021, 11, 26, 00, 00, 00);
				var parser = new TariffCharacteristicNCMParser("BR Tariff Characteristic NCM Test");

				parser.ExportToXMLFile(attributes, tariffs, TestOutputFilePath, publicationDate, Constants.RefCusTariffBRCharacteristicCodes.CharacteristicTypes.NCMTE);
				using (var outputStream = new FileStream(TestOutputFilePath, FileMode.Open))
				{
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
				}
			}
		}

		[Test]
		public void TestStylesNotFoundLog()
		{
			var publicationDate = new DateTime(2021, 11, 26, 00, 00, 00);
			var parser = new TariffCharacteristicNCMParser("BR Tariff Characteristic NCM Test");

			parser.ExportToXMLFile(attributes, tariffs, TestOutputFilePath, publicationDate, Constants.RefCusTariffBRCharacteristicCodes.CharacteristicTypes.NCMTE);

			var ex = Assert.Throws<InvalidOperationException>(() => { ParserErrorCollector.Instance.ReportErrors(); });
			Assert.AreEqual("Style not found: DATA\r\nStyle not found: DATA_HORA\r\nStyle not found: LISTA_ESTATICA2\r\n", ex.Message);
		}

		[TearDown]
		public void TestCleanup()
		{
			File.Delete(TestOutputFilePath);
			ParserErrorCollector.Instance.Clear();
		}

		[SetUp]
		public void SetUp()
		{
			tariffs = new List<NCM>
				{
					new NCM() { codigoNcm = "3003.90.17", listaAtributos = new List<Atributo>()
					{
						new Atributo() { codigo = "ATT_4070", modalidade = "Exportação", obrigatorio = true },
						new Atributo() { codigo = "ATT_3313", modalidade = "Exportação", obrigatorio = false },
						new Atributo() { codigo = "ATT_59", modalidade = "Importação", obrigatorio = true },
						new Atributo() { codigo = "ATT_7853", modalidade = "Importação", obrigatorio = true },
						new Atributo() { codigo = "ATT_7854", modalidade = "Importação", obrigatorio = true },
						new Atributo() { codigo = "ATT_7856", modalidade = "Importação", obrigatorio = true },
						new Atributo() { codigo = "ATT_3522", modalidade = "Importação", obrigatorio = true },
						new Atributo() { codigo = "ATT_13584", modalidade = "Importação", obrigatorio = true },
						new Atributo() { codigo = "ATT_11968", modalidade = "Importação", obrigatorio = true },
						new Atributo() { codigo = "ATT_11972", modalidade = "Importação", obrigatorio = true },
						new Atributo() { codigo = "ATT_4492", modalidade = "Importação", obrigatorio = true },
					} },
					new NCM() { codigoNcm = "3003.90.19", listaAtributos = new List<Atributo>()
					{
						new Atributo() { codigo = "ATT_4070", modalidade = "Exportação", obrigatorio = true },
						new Atributo() { codigo = "ATT_66", modalidade = "Exportação", obrigatorio = false },
						new Atributo() { codigo = "ATT_68", modalidade = "Importação", obrigatorio = true },
					} }
				};

			using (var inputStreamAttributes = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NCM.ATRIBUTOS_2023_10_11.json"))
			using (var streamReader = new StreamReader(inputStreamAttributes))
			{
				attributes = JsonConvert.DeserializeObject<Attributes>(streamReader.ReadToEnd()).atributos;
			}
		}

		List<Atributo> attributes;
		List<NCM> tariffs;

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.RefCusTariffBRCharacteristicNcm_BR_HSN.xml");
	}
}
