using CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeType;
using System;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeType
{
	class AHCONCodeTypeGeneratorTest : RefCusCodeTypeGeneratorTest<AHCONCodeTypeGenerator>
	{
		protected override string OutputXMLFileName => "DIE Legal Conditions List for AdHoc Authorisations Code Type.xml";
	}
}
