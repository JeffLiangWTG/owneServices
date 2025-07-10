using System.Collections.Generic;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using CargoWise.RefDbRepo.TRReferenceData.Tests.Model;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests
{
	sealed class BanderolTariffRateTest : NameAttributeBaseTest<BanderolTariffRate>
	{
		protected override Dictionary<string, string> ExpectedPropertyNameAndAttributeValue =>
			new Dictionary<string, string>
			{
				{"TariffCode","TariffCode"},
				{"Description","Description"},
				{"RateFormula","RateFormula"},
				{"UOM","UOM"},
				{"Currency","Currency"},
			};
	}
}
