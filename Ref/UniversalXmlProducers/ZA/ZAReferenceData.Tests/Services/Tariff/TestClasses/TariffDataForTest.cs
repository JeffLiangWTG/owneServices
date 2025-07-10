using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestClasses
{
	internal class TariffDataForTest : TariffData
	{
		public void ApplyRulesExposed(ITariffHelper tariffHelper) => base.ApplyRules(tariffHelper);
	}
}
