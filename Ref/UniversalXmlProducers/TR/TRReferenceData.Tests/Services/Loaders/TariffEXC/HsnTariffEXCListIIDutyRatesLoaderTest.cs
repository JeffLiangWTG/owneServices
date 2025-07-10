using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using CargoWise.RefDbRepo.TRReferenceData.Tests.Services.Loaders;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs
{
	sealed class HsnTariffEXCListIIDutyRatesLoaderTest : HsnTariffExcListDutyRatesLoaderBaseTestCase
	{
		protected override HsnTariffEXCListDutyRatesLoaderBase RateLoader => new HsnTariffEXCListIIDutyRatesLoader();
		protected override int ExpectedLength => 97;
		protected override IEnumerable<HsnTariffEXCListDutyRate> ExpectedSampleData => new List<HsnTariffEXCListDutyRate>
		{
			new HsnTariffEXCListDutyRate(
				"8701",
				"Traktörler (87.09 pozisyonuna giren traktörler hariç) [Yalnız ATV (her türlü arazide kullanılan araç) ve UTV (çok amaçlı hizmet aracı)]",
				"25",
				null,
				null,
				null,
				new string[]
				{
					"870121",
					"870122",
					"870123",
					"870124",
					"870129",
				},
				string.Empty,
				"EXC",
				"50",
				"VFD * 0.25",
				new DateTime(2023, 01, 01, 00, 00, 00).ToString("O"),
				new DateTime(2079, 06, 06, 23, 59, 00).ToString("O")
			),
			new HsnTariffEXCListDutyRate(
				"890110900011",
				"Yolcu ve gezinti gemileri\n(denizde seyretmeye mahsus olanlar)",
				"0",
				null,
				null,
				null,
				new string[] { },
				string.Empty,
				"EXC",
				"50",
				"0",
				new DateTime(2023, 01, 01, 00, 00, 00).ToString("O"),
				new DateTime(2079, 06, 06, 23, 59, 00).ToString("O")
			)
		};

		protected override Tuple<string, string> DataFileName => new Tuple<string, string>("OTV Liste II.xlsx", "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.OTV Liste II.xlsx");
	}
}
