using CargoWise.RefDbRepo.EUReferenceData.Business;
using System.IO;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class CCIMethodOfPaymentDataParserTest : CommonXmlDataParserTest<CCIMethodOfPaymentDataParser>
	{
		protected override Stream GetXMLFile() => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.CCI.TestFiles.Input.RD_CCI_MethodOfPayment.xml");

		protected override Stream GetInvalidXMLFile() => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.CCI.TestFiles.Input.RD_CCI_MethodOfPayment_Invalid.xml");

		protected override Stream GetRDEntityNotFoundErrorFile() => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.CCI.TestFiles.Input.RD_CCI_MethodOfPayment_EntityNotFound.xml");

		protected override string RDEntityAttributeValue => Constants.CCIMethodOfPayment.RDEntityAttributeValue;
	}
}
