using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class SpecialClearanceAttributesProcedureParserTest
	{
		[Test]
		public void TestExportToXMLFile()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Input.enq_due.xlsx"))
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Output.RefCusProcedureAttribute_BR_EXP.xml"))
			{
				var publicationDate = new DateTime(2023, 06, 09, 00, 00, 00);
				new SpecialClearanceAttributesProcedureParser("BR Export Special Clearance Attributes Procedure").ExportToXMLFile(inputStream, TestOutputFilePath, publicationDate);
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

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Output.Temp.RefCusProcedureAttribute_BR_EXP.xml");
	}
}
