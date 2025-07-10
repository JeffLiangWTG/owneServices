using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class AgreementsLAIACodeListParserTest
	{
		[Test]
		public void TestXmlExport()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Output.RefCusCodeList_BR_ALAIA.xml"))
			using (var expectedTypeStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Output.RefCusCodeType_BR_ALAIA.xml"))
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.AcordoAladi.xml"))
			using (var agreementStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.TabelaAcordosTarifarios.html"))
			using (var reader = new StreamReader(agreementStream))
			{
				string agreementString = reader.ReadToEnd();
				var publicationDate = new DateTime(2022, 06, 15, 00, 00, 00);
				var parser = new AgreementsLAIACodeListParser($"BR {Constants.RefCusCodeTypes.DutyLaiaAgreementCodes.Description} Code List");

				using (var outputStream = new MemoryStream())
				using (var typeOutputStream = new MemoryStream())
				{
					parser.ExportToStream((inputStream, agreementString), outputStream, typeOutputStream, publicationDateTime: publicationDate);
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
					StreamCompareHelper.CompareStreamContent(expectedTypeStream, typeOutputStream);
				}
			}
		}
	}
}
