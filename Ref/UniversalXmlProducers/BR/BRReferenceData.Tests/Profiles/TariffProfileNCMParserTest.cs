using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.CmdLine;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	sealed class TariffProfileNCMParserTest
	{
		[TestCase(true)]
		[TestCase(false)]
		public void TestExportXml(bool isProduction)
		{
			var profileType = GetProfileType(isProduction);
			using (var expectedStreamProfile = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Output.RefCusProfile_BR_{profileType}.xml"))
			using (var expectedStreamProfileType = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Output.RefCusProfileType_BR_{profileType}.xml"))
			using (var expectedStreamQuestion = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Output.RefCusProfileQuestion_BR_{profileType}.xml"))
			using (var expectedStreamQuestionPathway = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Output.RefCusProfileQuestionPathway_BR_{profileType}.xml"))
			{
				var testTime = TariffProfileNCMProgram.GetDateFromFileName("ATRIBUTOS_2023_10_11.json");
				var parser = new TariffProfileNCMParser("BR Tariff Attributes NCM Test", isProduction, testTime, testTime);

				parser.OverridePublicationDate(new DateTime(2021, 11, 26, 08, 42, 21));

				parser.ExportToXMLFile(tariffAttributesDTO,
					outputFileName: OutputPathProfile(profileType),
					outputTypeFileName: OutputPathProfileType(profileType),
					outputQuestionFileName: OutputPathProfileQuestion(profileType),
					outputQuestionPathwayFileName: OutputPathProfileQuestionPathway(profileType)
				);

				using (var outputStreamProfile = new FileStream(OutputPathProfile(profileType), FileMode.Open))
				using (var outputStreamProfileType = new FileStream(OutputPathProfileType(profileType), FileMode.Open))
				using (var outputStreamQuestion = new FileStream(OutputPathProfileQuestion(profileType), FileMode.Open))
				using (var outputStreamQuestionPathway = new FileStream(OutputPathProfileQuestionPathway(profileType), FileMode.Open))
				{
					StreamCompareHelper.CompareStreamContent(expectedStreamProfile, outputStreamProfile);
					StreamCompareHelper.CompareStreamContent(expectedStreamProfileType, outputStreamProfileType);
					StreamCompareHelper.CompareStreamContent(expectedStreamQuestion, outputStreamQuestion);
					StreamCompareHelper.CompareStreamContent(expectedStreamQuestionPathway, outputStreamQuestionPathway);
				}
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void TestStylesNotFoundLog(bool isProduction)
		{
			tariffAttributesDTO.Tariffs = new List<NCM>
				{
					new NCM() { codigoNcm = "3003.90.17", listaAtributos = new List<Atributo>()
					{
						new Atributo() { codigo = "ATT_82", modalidade = "Exportação", obrigatorio = true, dataInicioVigencia = "2024-10-10" , dataFimVigencia = "2024-10-12" },
						new Atributo() { codigo = "ATT_1", modalidade = "Exportação", obrigatorio = false, dataInicioVigencia = "2024-10-10" , dataFimVigencia = "2024-10-12" },
						new Atributo() { codigo = "ATT_102", modalidade = "Importação", obrigatorio = true, dataInicioVigencia = "2024-10-10" , dataFimVigencia = "2024-10-12" },
					} },
					new NCM() { codigoNcm = "3003.90.19", listaAtributos = new List<Atributo>()
					{
						new Atributo() { codigo = "ATT_82", modalidade = "Exportação", obrigatorio = true, dataInicioVigencia = "2024-10-10" , dataFimVigencia = "2024-10-12" },
						new Atributo() { codigo = "ATT_1", modalidade = "Exportação", obrigatorio = false, dataInicioVigencia = "2024-10-10" , dataFimVigencia = "2024-10-12" },
						new Atributo() { codigo = "ATT_102", modalidade = "Exportação", obrigatorio = false, dataInicioVigencia = "2024-10-10" , dataFimVigencia = "2024-10-12"},
						new Atributo() { codigo = "ATT_160", modalidade = "Importação", obrigatorio = true, dataInicioVigencia = "2024-10-10" , dataFimVigencia = "2024-10-12" }
					} }
				};

			var profileType = GetProfileType(isProduction);
			var testTime = TariffProfileNCMProgram.GetDateFromFileName("ATRIBUTOS_2023_10_11.json");
			var parser = new TariffProfileNCMParser("BR Tariff Attributes NCM Test", isProduction, testTime, testTime);

			parser.OverridePublicationDate(new DateTime(2021, 11, 26, 08, 42, 21));

			parser.ExportToXMLFile(tariffAttributesDTO,
				outputFileName: OutputPathProfile(profileType),
				outputTypeFileName: OutputPathProfileType(profileType),
				outputQuestionFileName: OutputPathProfileQuestion(profileType),
				outputQuestionPathwayFileName: OutputPathProfileQuestionPathway(profileType)
			);

			var ex = Assert.Throws<InvalidOperationException>(() => { ParserErrorCollector.Instance.ReportErrors(); });
			Assert.AreEqual("Style not found: LISTA_ESTATICA2\r\n", ex.Message);
		}

		[TearDown]
		public void TestCleanup()
		{
			foreach (var type in new[] { Constants.ProfileTypes.Codes.Tariff, Constants.ProfileTypes.Codes.TariffTest })
			{
				File.Delete(OutputPathProfile(type));
				File.Delete(OutputPathProfileType(type));
				File.Delete(OutputPathProfileQuestion(type));
				File.Delete(OutputPathProfileQuestionPathway(type));
			}
			ParserErrorCollector.Instance.Clear();
		}

		[SetUp]
		public void SetUp()
		{
			tariffAttributesDTO = new TariffAttributesDTO();
			tariffAttributesDTO.Tariffs = new List<NCM>
				{
					new NCM() { codigoNcm = "3003.90.17", listaAtributos = new List<Atributo>()
					{
						new Atributo() { codigo = "ATT_82", modalidade = "Exportação", obrigatorio = true, dataInicioVigencia = "2024-10-10" , dataFimVigencia = "2024-10-12", multivalorado = true},
						new Atributo() { codigo = "ATT_1", modalidade = "Exportação", obrigatorio = false, dataInicioVigencia = "2024-10-10" , dataFimVigencia = "2024-10-12", multivalorado = true },
						new Atributo() { codigo = "ATT_102", modalidade = "Importação", obrigatorio = true, dataInicioVigencia = "2024-10-10" , dataFimVigencia = "2024-10-12" },
					} },
					new NCM() { codigoNcm = "3003.90.19", listaAtributos = new List<Atributo>()
					{
						new Atributo() { codigo = "ATT_82", modalidade = "Exportação", obrigatorio = true, dataInicioVigencia = "2024-10-10" , dataFimVigencia = "2024-10-12"  },
						new Atributo() { codigo = "ATT_1", modalidade = "Exportação", obrigatorio = false, dataInicioVigencia = "2024-10-10" , dataFimVigencia = "2024-10-12", multivalorado = true  },
						new Atributo() { codigo = "ATT_102", modalidade = "Exportação", obrigatorio = false, dataInicioVigencia = "2024-10-10" , dataFimVigencia = "2024-10-12"  },
					} }
				};

			using (var inputStreamAttributes = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.NCM.ATRIBUTOS_2023_10_11.json"))
			using (var streamReader = new StreamReader(inputStreamAttributes))
			{
				var json = streamReader.ReadToEnd();
				tariffAttributesDTO.Attributes = JsonConvert.DeserializeObject<Attributes>(json.Replace("\\u001A", string.Empty)).atributos;
			}
		}

		string GetProfileType(bool isProduction) => isProduction ? Constants.ProfileTypes.Codes.Tariff : Constants.ProfileTypes.Codes.TariffTest;

		TariffAttributesDTO tariffAttributesDTO;

		string OutputPathProfile(string profileType) => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $@"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Output.Temp.RefCusProfile_BR_{profileType}.xml");
		string OutputPathProfileType(string profileType) => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $@"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Output.Temp.RefCusProfileType_BR_{profileType}.xml");
		string OutputPathProfileQuestion(string profileType) => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $@"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Output.Temp.RefCusProfileQuestion_BR_{profileType}.xml");
		string OutputPathProfileQuestionPathway(string profileType) => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $@"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Output.Temp.RefCusProfileQuestionPathway_BR_{profileType}.xml");
	}
}
