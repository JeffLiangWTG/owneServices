using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.KindOfPackages.Business;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.KindOfPackages.Tests
{
	[TestFixture]
	sealed class KindOfPackagesDataParserTest : CommonXmlDataParserTest<KindOfPackagesDataParser>
	{
		protected override string RDEntityAttributeValue => Constants.KindOfPackages.RDEntityAttributeValue;

		protected override Stream GetXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.KindOfPackages.TestFiles.Input.RD_ICS2_KindOfPackages.xml");
		}

		protected override Stream GetInvalidXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.KindOfPackages.TestFiles.Input.RD_ICS2_KindOfPackages-Err2.xml");
		}

		protected override Stream GetRDEntityNotFoundErrorFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.KindOfPackages.TestFiles.Input.RD_ICS2_KindOfPackages-Err3.xml");
		}

		protected override bool IsAdditionalTranslationSupporter => true;
	}
}
