using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services.Loaders.TariffEXC
{
	internal sealed class HsnTariffEXCListIIIDutyRatesLoaderTest : HsnTariffExcListDutyRatesLoaderBaseTestCase
	{
		protected override HsnTariffEXCListDutyRatesLoaderBase RateLoader => new HsnTariffEXCListIIIDutyRatesLoader();

		protected override int ExpectedLength => 37;

		protected override IEnumerable<HsnTariffEXCListDutyRate> ExpectedSampleData => new List<HsnTariffEXCListDutyRate>
		{
			new HsnTariffEXCListDutyRate(
				"220300",
				"Malttan üretilen biralar",
				"63",
				"LTR",
				"LPA",
				"OTB",
				new string[] {},
				"",
				"EXC",
				"51",
				"MAX(6.1915 * [LPA] * [LTR], 63 / 100 * VDF)",
				new DateTime(2023, 01, 01, 00, 00, 00).ToString("O"),
				new DateTime(2079, 06, 06, 23, 59, 00).ToString("O")
			),
			new HsnTariffEXCListDutyRate(
				"2204",
				"Taze üzüm şarabı (kuvvetlendirilmiş şaraplar dahil); üzüm şırası (20.09 pozisyonunda yer alanlar hariç)\n(2204.10 Köpüklü şaraplar ve 2204.30 Diğer üzüm şıraları hariç)",
				"0",
				"LTR",
				"",
				"OTB",
				new string[] { "220410", "220430" },
				"",
				"EXC",
				"51",
				"MAX(30.4445 * [LTR], 0 / 100 * VDF)",
				new DateTime(2023, 01, 01, 00, 00, 00).ToString("O"),
				new DateTime(2079, 06, 06, 23, 59, 00).ToString("O")
			)
		};

		protected override Tuple<string, string> DataFileName => new Tuple<string, string>("OTV Liste III.xlsx", "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.OTV Liste III.xlsx");
	}
}
