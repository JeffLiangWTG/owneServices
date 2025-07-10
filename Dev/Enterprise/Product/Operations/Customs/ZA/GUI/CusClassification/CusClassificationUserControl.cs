using CargoWise.Types;
using Enterprise.Customs.ZA.Business;

namespace Enterprise.Customs.ZA.GUI
{
	public class CusClassificationUserControl : Customs.GUI.GeneralCountryClassificationUserControl
	{
		protected override ZString UniversalTariffType => UniversalReferenceConstants.CusTariffCode.Schedule1Part1;
		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.SouthAfrica;
		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.SouthAfrica;
	}
}
