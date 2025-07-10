using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using CargoWise.RefDbRepo.EUReferenceData.TypeOfMeansOfTransport.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.TypeOfMeansOfTransport.Tests
{
	[TestFixture]
	class TypeOfMeansOfTransportDataParserTest : CommonXmlDataParserTest<TypeOfMeansOfTransportDataParser>
	{
		protected override Stream GetXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.TypeOfMeansOfTransport.TestFiles.Input.RD_ICS2_TypeOfMeansOfTransport.xml");
		}

		protected override Stream GetInvalidXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.TypeOfMeansOfTransport.TestFiles.Input.RD_ICS2_TypeOfMeansOfTransport_Invalid.xml");

		}

		protected override Stream GetRDEntityNotFoundErrorFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.TypeOfMeansOfTransport.TestFiles.Input.RD_ICS2_TypeOfMeansOfTransport_EntityNotFound.xml");
		}

		protected override string RDEntityAttributeValue => Constants.TypeOfMeansOfTransport.RDEntityAttributeValue;
	}
}
