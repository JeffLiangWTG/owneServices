using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	class HSNTariffDutyRatePdfParserTest
	{
		[Test]
		public void TestXlsToXmlExport()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var inputFile_AnnexVII = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_vii_lista_covid.xlsx"))
			using (var inputFile_AnnexVI = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_vi_lebit_bk.xlsx"))
			using (var inputFile_AnnexIV = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_iv_desabastecimento.xlsx"))
			using (var inputFile_AnnexV = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_v_letec.xlsx"))
			using (var inputFile_AnnexII = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.Anexo_II_Res_272_2021.xlsx"))
			using (var expectedStream = Utils.GetManifestResourceStream(@"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.RefCusTariffVigentRateList_BR_DTY.xml"))
			{
				List<Stream> files = new List<Stream>(new Stream[] { inputFile_AnnexVII, inputFile_AnnexVI, inputFile_AnnexV, inputFile_AnnexIV, inputFile_AnnexII });
				var parser = new HSNTariffDutyRatePdfParser("BR HSN Tariff Duty Rate");
				parser.ExportToXMLFile(files, TestOutputFilePath, new DateTime(2022, 10, 18, 13, 38, 58));
				using (var outputStream = new FileStream(TestOutputFilePath, FileMode.Open))
				{
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
				}
			}
		}

		[TearDown]
		public void TearDownCleanup()
		{
			if (File.Exists(TestOutputFilePath))
			{
				File.Delete(TestOutputFilePath);
			}
		}

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.RefCusTariffVigentRateList_BR_DTY.xml");
	}
}
