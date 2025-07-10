using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public static class TariffHelper
	{
		public static ZDateTime GetHSCodeEffectiveDate(CommonShipment shipment)
		{
			if (shipment == null)
			{
				return ZDateTime.Today;
			}

			return shipment.JS_E_DEP.IsValid
				? shipment.JS_E_DEP
				: shipment.IsInDatabase ? shipment.JS_SystemCreateTimeUtc : ZDateTime.Today;
		}

		public static ZDateTime GetHSCodeEffectiveDate(CommonConsol consol)
		{
			if (consol == null)
			{
				return ZDateTime.Today;
			}

			return consol.JK_JX_JA_E_FirstDEP.IsValid
				? consol.JK_JX_JA_E_FirstDEP
				: consol.IsInDatabase ? consol.JK_SystemCreateTimeUtc : ZDateTime.Today;
		}
	}
}
