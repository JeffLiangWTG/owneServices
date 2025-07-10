using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class WarehousingSectorsCodeListParserTest
	{
		[Test]
		public void TestXmlExport()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Output.RefCusCodeList_BR_SCTR.xml"))
			using (var expectedTypeStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Output.RefCusCodeType_BR_SCTR.xml"))
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.SetorLotacao.xml"))
			{
				var publicationDate = new DateTime(2022, 5, 1, 00, 00, 00);
				var parser = new WarehousingSectorsCodeListParser($"BR {Constants.RefCusCodeTypes.WarehousingSectorsCodes.Description} Code List");

				using (var outputStream = new MemoryStream())
				using (var typeOutputStream = new MemoryStream())
				{
					parser.ExportToStream(inputStream, outputStream, typeOutputStream, publicationDateTime: publicationDate);
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
					StreamCompareHelper.CompareStreamContent(expectedTypeStream, typeOutputStream);
				}
			}
		}
	}
}
