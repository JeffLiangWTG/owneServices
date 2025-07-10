
using CargoWise.Types;

namespace Enterprise.Customs.TW.GUI
{
	public class CusClassificationUserControl : Customs.GUI.GeneralCountryClassificationUserControl
	{
		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.Taiwan;
		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.Taiwan;
	}
}
