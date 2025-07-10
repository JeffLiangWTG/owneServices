using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using CargoWise.RefDbRepo.TRReferenceData.Tests.Services.Loaders;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs
{
	sealed class HsnTariffEXCListIDutyRatesLoaderTest : HsnTariffExcListDutyRatesLoaderBaseTestCase
	{
		protected override HsnTariffEXCListDutyRatesLoaderBase RateLoader => new HsnTariffEXCListIDutyRatesLoader();
		protected override int ExpectedLength => 131;
		protected override IEnumerable<HsnTariffEXCListDutyRate> ExpectedSampleData => new List<HsnTariffEXCListDutyRate>
		{
			new HsnTariffEXCListDutyRate(
				"2711",
				"Petrol gazları ve diğer gazlı hidrokarbonlar (2711.11.00.00.00; 2711.12; 2711.13; 2711.19.00.00.11; 2711.21.00.00.00; 2711.29.00.00.11 ve 2711.29.00.00.12 hariç)",
				"0",
				"KGM",
				null,
				null,
				new string[]
				{
					"271111000000",
					"271112",
					"271113",
					"271119000011",
					"271121000000",
					"271129000011",
					"271129000012"
				},
				string.Empty,
				"EXC",
				"93",
				"0 * [KGM]",
				new DateTime(2023, 01, 01, 00, 00, 00).ToString("O"),
				new DateTime(2079, 06, 06, 23, 59, 00).ToString("O")
			),
			new HsnTariffEXCListDutyRate(
				"340399000000",
				"(Yağlama müstahzarları) Diğerleri",
				"5.1504 TL",
				"KGM",
				null,
				null,
				new string[] { },
				string.Empty,
				"EXC",
				"93",
				"5.1504 * [KGM]",
				new DateTime(2023, 01, 01, 00, 00, 00).ToString("O"),
				new DateTime(2079, 06, 06, 23, 59, 00).ToString("O")
			)
		};

		protected override Tuple<string, string> DataFileName => new Tuple<string, string>("OTV Liste I.xlsx", "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.OTV Liste I.xlsx");
	}
}
