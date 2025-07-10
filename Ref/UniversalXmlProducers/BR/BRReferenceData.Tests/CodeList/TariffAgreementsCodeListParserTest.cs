using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class TariffAgreementsCodeListParserTest
	{
		[Test]
		public void TestGetLaiaAgreementDTOs()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var agreementStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.TabelaAcordosTarifarios.html"))
			using (var reader = new StreamReader(agreementStream))
			{
				string agreementHtmlString = reader.ReadToEnd();
				var parser = TariffAgreementsCodeListParser.GetLaiaAgreementDTOs(agreementHtmlString);

				Assert.AreEqual(28, parser.Count);

				var agreementDTO = parser[0];
				Assert.AreEqual("N/A", agreementDTO.Id);
				Assert.AreEqual("Argentina", agreementDTO.Country);
				Assert.AreEqual("SGPC", agreementDTO.Subject);
				Assert.AreEqual("DEC/EXEC 6500/2008", agreementDTO.LegalAct);

				agreementDTO = parser[1];
				Assert.AreEqual("336", agreementDTO.Id);
				Assert.AreEqual("Bolivia", agreementDTO.Country);
				Assert.AreEqual("Aladi", agreementDTO.Subject);
				Assert.AreEqual("DEC/EXEC 2240/1997", agreementDTO.LegalAct);
			}
		}

		[Test]
		public void TestExportToXml()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Output.RefCusCodeList_BR_Tariff Agreements.xml"))
			using (var expectedTypeStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Output.RefCusCodeType_BR_Tariff Agreements.xml"))
			using (var agreementHTMLStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.TabelaAcordosTarifarios.html"))
			using (var agreementXLSXStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.Agreements.xlsx"))
			using (var reader = new StreamReader(agreementHTMLStream))
			{
				string agreementString = reader.ReadToEnd();
				var publicationDate = new DateTime(2022, 06, 15, 00, 00, 00);
				var parser = new TariffAgreementsCodeListParser($"BR {Constants.RefCusCodeTypes.TariffAgreementsCodes.Description} Code List");

				using (var outputStream = new MemoryStream())
				using (var typeOutputStream = new MemoryStream())
				{
					parser.ExportToStream((agreementXLSXStream, agreementString), outputStream, typeOutputStream, publicationDateTime: publicationDate);
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
					StreamCompareHelper.CompareStreamContent(expectedTypeStream, typeOutputStream);
				}
			}
		}
	}
}
