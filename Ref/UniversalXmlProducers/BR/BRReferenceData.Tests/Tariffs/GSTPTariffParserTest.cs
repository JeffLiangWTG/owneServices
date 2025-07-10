using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	public class GSTPTariffParserTest
	{
		[Test]
		public void TestExportToXMLFile()
		{
			var assembly = Assembly.GetExecutingAssembly();

			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.RefCusTariff_BR_GSTP.xml"))
			using (var inputStreamNCM = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.Tabela_NCM_2022_v26.04.22.xlsx"))
			using (var inputStreamTEC = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsTariffRate.tec_20211126.xlsx"))
			using (var inputStreamGSTP = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.GSTP.sgpc_com_ex.xlsx"))
			{
				new GSTPTariffParser("BR GSTP Tariff").ExportToXMLFile(inputStreamGSTP, inputStreamNCM, inputStreamTEC, TestOutputFilePath, new DateTime(2022, 07, 06, 00, 00, 00));
				using (var outputStream = new FileStream(TestOutputFilePath, FileMode.Open))
				{
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
				}
				File.Delete(TestOutputFilePath);
			}
		}

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.BRRefCusTariffGSTP.xml");
	}
}
