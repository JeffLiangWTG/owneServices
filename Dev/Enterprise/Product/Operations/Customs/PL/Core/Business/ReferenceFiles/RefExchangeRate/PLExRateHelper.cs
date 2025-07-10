using CargoWise.Types;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

public class PLExRateHelper : Integration.Customs.Shared.IExRateHelper
{
	public PLExRateHelper() { }

	public ZInt DecimalPlacesForSellRate(ZString currency)
	{
		if (currency == Core.Constants.CurrencyCodes.Indonesia || currency == Core.Constants.CurrencyCodes.VietNam)
		{
			return DecimalPlacesForCurrency.DecimalPlacesForIDRandVND;
		}
		return DecimalPlacesForCurrency.DecimalPlacesForGeneral;
	}
}
