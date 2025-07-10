using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using CargoWise.RefDbRepo.TRReferenceData.Tests.Model;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests
{
	class DeclarationTariffTest : NameAttributeBaseTest<DeclarationTariff>
	{
		protected override Dictionary<string, string> ExpectedPropertyNameAndAttributeValue
			=> new Dictionary<string, string>()
			{
				{"TariffCode", "TariffCode" },
				{"AdditionalCode", "AdditionalCode" },
				{"Description", "Description" },
				{"Percent", "Percent" },
				{"Formula", "Formula" }
			};
	}
}
