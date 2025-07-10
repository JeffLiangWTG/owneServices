using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class PisCofinsLegalBasisCodeListParserTest
	{
		[Test]
		public void TestPisCofinsXMLGeneration()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Output.PIS_COFINS.RefCusCodeList_BR_LRTPC.xml"))
			using (var expectedTypeStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Output.PIS_COFINS.RefCusCodeType_BR_LRTPC.xml"))
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.PIS_COFINS.FundamentoLegalRegimeTributacaoPisCofins.xml"))
			{
				var publicationDate = new DateTime(2022, 07, 07, 12, 26, 49);
				var parser = new PisCofinsLegalBasisCodeListParser("BR Pis/Cofins Legal Basis Code List");

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
