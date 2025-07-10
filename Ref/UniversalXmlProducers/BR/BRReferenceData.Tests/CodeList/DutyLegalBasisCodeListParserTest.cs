using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	class DutyLegalBasisCodeListParserTest
	{
		[Test]
		public void TestExportToXML()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var expectedCodeStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Output.LRTII.RefCusCodeList_BR_LRTII.xml"))
			using (var expectedTypeStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Output.LRTII.RefCusCodeType_BR_LRTII.xml"))
			using (var expectedAttrStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Output.LRTII.RefCusCodeListAttributeName_BR_LRTII.xml"))
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.LRTII.FundamentoLegalRegimeTributacaoII.xml"))
			{
				var publicationDate = new DateTime(2022, 07, 05, 09, 59, 20);
				var parser = new DutyLegalBasisCodeListParser("LRTII - Legal Basis Taxation Regime - Duty file");

				using (var codeOutputStream = new MemoryStream())
				using (var typeOutputStream = new MemoryStream())
				using (var attrOutputStream = new MemoryStream())
				{
					parser.ExportToStream(inputStream, codeOutputStream, typeOutputStream, attrOutputStream, publicationDateTime: publicationDate);

					StreamCompareHelper.CompareStreamContent(expectedCodeStream, codeOutputStream);
					StreamCompareHelper.CompareStreamContent(expectedTypeStream, typeOutputStream);
					StreamCompareHelper.CompareStreamContent(expectedAttrStream, attrOutputStream);
				}
			}
		}
	}
}
