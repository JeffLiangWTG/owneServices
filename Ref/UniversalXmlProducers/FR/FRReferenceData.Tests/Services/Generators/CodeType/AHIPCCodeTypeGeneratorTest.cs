using CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeType;
using System;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeType
{
	class AHIPCCodeTypeGeneratorTest : RefCusCodeTypeGeneratorTest<AHIPCCodeTypeGenerator>
	{
		protected override string OutputXMLFileName => "DIE Inward Processing Conditions List for AdHoc Authorisations Code Type.xml";
	}
}
