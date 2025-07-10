using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.MXReferenceData.Business;
using CargoWise.RefDbRepo.MXReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.MXReferenceData.Tests
{
	[TestFixture]
	public class CustomsFacilitiesParserTest
	{
		[Test]
		public void TestExportToXMLFile()
		{
			using (var expectedStream = TestUtils.GetManifestResourceStream($"CargoWise.RefDbRepo.MXReferenceData.Tests.CodeList.TestFiles.Output.RefCusCodeList_MX_FAC.xml"))
			using (var expectedTypeStream = TestUtils.GetManifestResourceStream($"CargoWise.RefDbRepo.MXReferenceData.Tests.CodeList.TestFiles.Output.RefCusCodeType_MX_FAC.xml"))
			{
				var publicationDate = new DateTime(2020, 10, 14, 09, 50, 00);
				var parser = new CustomsFacilitiesParser("MX - Customs Aduana Section");

				List<CustomsSectionDTO> downloadedList = new List<CustomsSectionDTO>()
				{
					new CustomsSectionDTO() { CustomCode = "01", SectionCode = "1", SectionDescription = "SectionDescription 1"},
					new CustomsSectionDTO() { CustomCode = "02", SectionCode = "2", SectionDescription = "SectionDescription 2"},
					new CustomsSectionDTO() { CustomCode = "03", SectionCode = "3", SectionDescription = "SectionDescription 3"},
					new CustomsSectionDTO() { CustomCode = "04", SectionCode = "4", SectionDescription = "SectionDescription 4"}
				};

				parser.ExportToXMLFile(downloadedList, TestOutputFilePath, TestTypeOutputFilePath, publicationDateTime: publicationDate);

				using (var inputStream = new FileStream(TestOutputFilePath, FileMode.Open))
				using (var outputStreamProfileType = new FileStream(TestTypeOutputFilePath, FileMode.Open))
				{ 
					StreamCompareHelper.CompareStreamContent(expectedStream, inputStream);
					StreamCompareHelper.CompareStreamContent(expectedTypeStream, outputStreamProfileType);
				}
			}
		}

		[TearDown]
		public void TestCleanup()
		{
			File.Delete(TestOutputFilePath);
			File.Delete(TestTypeOutputFilePath);
		}

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.MXReferenceData.Tests.CodeList.TestFiles.Output.Temp.RefCusCodeList_MX_FAC.xml");
		readonly string TestTypeOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.MXReferenceData.Tests.CodeList.TestFiles.Output.Temp.RefCusCodeType_MX_FAC.xml");
	}
}
