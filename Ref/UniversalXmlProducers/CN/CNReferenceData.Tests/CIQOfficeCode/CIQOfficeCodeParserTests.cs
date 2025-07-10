using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.CNReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CNReferenceData.Tests
{
	[TestFixture]
	public class CIQOfficeCodeParserTests
	{
		[Test]
		public void TestExportRefCusCodeListToXMLFile()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var inputStream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.CNReferenceData.Tests.CIQOfficeCode.TestFiles.Input.CIQ Office Code.xls"))
			{
				var refCusCodeLists = CIQOfficeCodeParser.GetRefCusCodeLists(inputStream, 2, 1, 2);
				Assert.AreEqual(868, refCusCodeLists.Count);

				var xmlWriter = Helper.GenerateXmlWriter("CN CIQ Office Code", "CIQOF", new DateTime(2019, 4, 19, 21, 00, 00), refCusCodeLists);
				xmlWriter.SaveXml(TestOutputFilePath);

				TestHelper.AssertXmlFileContentEquals("CargoWise.RefDbRepo.CNReferenceData.Tests.CIQOfficeCode.TestFiles.Output.RefCusCodeList_CN_CIQOF.xml", TestOutputFilePath);

				File.Delete(TestOutputFilePath);
			}
		}

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CIQOfficeCode\TestFiles\Output\RefCusCodeList_CN_CIQOF.xml");
	}
}
