using System;
using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class ExportProcedureParserTest
	{
		[Test]
		public void TestExportToXMLFile()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Input.EnquadramentoOperacao.csv"))
			using (var reader = new StreamReader(inputStream, CodePagesEncodingProvider.Instance.GetEncoding(1252)))
			using (var expectedStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Output.BRRefCusProcedure20210808.xml"))
			{
				var publicationDate = new DateTime(2021, 08, 08, 09, 50, 00);
				var parser = new ExportProcedureParser("BR RefCusProcedure20210808");

				parser.ExportToXMLFile(reader, TestOutputFilePath, publicationDate);
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

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Output.Temp.BRRefCusProcedure20210808.xml");
	}
}
