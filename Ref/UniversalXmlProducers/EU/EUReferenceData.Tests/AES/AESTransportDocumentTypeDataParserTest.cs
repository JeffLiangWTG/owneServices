using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.AESTransportDocumentType.Business;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	class AESTransportDocumentTypeDataParserTest : CommonXmlDataParserTest<AESTransportDocumentTypeDataParser>
	{
		protected override Stream GetXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_TransportDocumentType.xml");
		}

		protected override Stream GetInvalidXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_TransportDocumentType-Err2.xml");
		}

		protected override Stream GetRDEntityNotFoundErrorFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_TransportDocumentType-Err3.xml");
		}

		protected override string RDEntityAttributeValue => Constants.AESTransportDocumentType.RDEntityAttributeValue;
	}
}
