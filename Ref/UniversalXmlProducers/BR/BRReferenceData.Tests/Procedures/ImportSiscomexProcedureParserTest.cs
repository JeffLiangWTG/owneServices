using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class ImportSiscomexProcedureParserTest
	{
		[Test]
		public void TestExportToXMLFile()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var inputStream1 = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Input.FundamentoLegalRegimeTributacaoII.xml"))
			using (var inputStream2 = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Input.FundamentoLegalRegimeTributacaoPisCofins.xml"))
			using (var inputStream3 = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Input.TipoDeclaracaoRegimeTributario.xml"))
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Output.RefCusProcedure_BR_ISW.xml"))
			{
				var publicationDate = new DateTime(2023, 06, 09, 00, 00, 00);
				new ImportSiscomexProcedureParser("BR Import Siscomex Procedure").ExportToXMLFile(inputStream1, inputStream2, inputStream3, TestOutputFilePath, publicationDate);
				using (var outputStream = new FileStream(TestOutputFilePath, FileMode.Open))
				{
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
				}
			}
		}

		[TearDown]
		public void TestCleanup()
		{
			File.Delete(TestOutputFilePath);
		}

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Output.Temp.RefCusProcedure_BR_ISW.xml");
	}
}
