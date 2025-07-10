using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business
{
	public interface ITariffDetail
	{
		TariffView UniversalTariff { get; }
		RefCusTariffType UniversalTariffType { get; }
		ZString FormulaSpecificValue { get; }
		ZString FormulaSpecificQuestion { get; }
		int FormulaSpecificValueScale { get; }
		ZString Tariff { get; }
		ZString Type { get; }

		ZBool ShouldBeExcluded { get; }
	}
}
