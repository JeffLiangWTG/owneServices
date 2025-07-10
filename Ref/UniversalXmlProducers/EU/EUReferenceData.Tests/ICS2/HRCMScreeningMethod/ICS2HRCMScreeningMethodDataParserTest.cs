using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.ICS2HRCMScreeningMethod.Business.Tests
{
	[TestFixture]
	class ICS2HRCMScreeningMethodDataParserTest : CommonXmlDataParserTest<ICS2HRCMScreeningMethodDataParser>
	{
		protected override Stream GetXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.HRCMScreeningMethod.TestFiles.Input.RD_ICS2_HRCMScreeningMethod.xml");
		}

		protected override Stream GetInvalidXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.HRCMScreeningMethod.TestFiles.Input.RD_ICS2_HRCMScreeningMethod-Err2.xml");
		}

		protected override string RDEntityAttributeValue => Constants.ICS2HRCMScreeningMethod.RDEntityAttributeValue;

		protected override Stream GetRDEntityNotFoundErrorFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.HRCMScreeningMethod.TestFiles.Input.RD_ICS2_HRCMScreeningMethod-Err3.xml");
		}
	}
}
