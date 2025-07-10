using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using CargoWise.RefDbRepo.EUReferenceData.StateSubset.Business;
using NUnit.Framework;
using System.Reflection;
using System;

namespace CargoWise.RefDbRepo.EUReferenceData.StateSubset.Tests
{
	[TestFixture]
	class StateSubsetDataParserTest : CommonXmlDataParserTest<StateSubsetDataParser>
	{
		protected override Stream GetXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.StateSubset.TestFiles.Input.RD_ICS2_StateSubset.xml");
		}

		protected override Stream GetInvalidXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.StateSubset.TestFiles.Input.RD_ICS2_StateSubset_Invalid.xml");
		}

		protected override Stream GetRDEntityNotFoundErrorFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.StateSubset.TestFiles.Input.RD_ICS2_StateSubset_EntityNotFound.xml");
		}

		protected override string RDEntityAttributeValue => Constants.StateSubset.RDEntityAttributeValue;
	}
}
