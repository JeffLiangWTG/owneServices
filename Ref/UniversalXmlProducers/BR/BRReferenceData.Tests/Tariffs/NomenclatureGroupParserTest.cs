using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class NomenclatureGroupParserTest
	{
		[Test]
		public void TestExportToXMLFile()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var inputStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.itemNcm.xml"))
			using (var expectedStream = Utils.GetManifestResourceStream(@"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.RefCusNomenclatureGroup_BR_20200902.xml"))
			{
				new NomenclatureGroupParser(NomenclatureGroupConstants.DataSource).ExportToXMLFile(inputStream, TestOutputFilePath, new DateTime(2020, 10, 14, 09, 50, 00));
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

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.RefCusNomenclatureGroup_BR_20200902.xml");
	}
}
