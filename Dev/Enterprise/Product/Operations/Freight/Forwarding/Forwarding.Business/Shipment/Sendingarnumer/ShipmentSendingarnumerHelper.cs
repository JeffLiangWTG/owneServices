using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ShipmentSendingarnumerHelper
	{
		public ShipmentSendingarnumerHelper(Sendingarnumer sendingarnumer)
		{
			Sendingarnumer = sendingarnumer;
			Sendingarnumer.CheckDigit = "0";
		}

		public ZString GetExpressCarrierNumber(ForwardingShipment shipment)
		{
			ZString result = ZString.Empty;

			if (shipment.JS_GoodsValue.IsEmpty || shipment.JS_GoodsValue == 0M)
			{
				result = GoodsValueZero;
			}
			else
			{
				ZBool isGoodsAndServicesTaxApplicable = ZBool.False;

				if (shipment.IsExport() && shipment.Consignor != null)
				{
					isGoodsAndServicesTaxApplicable = shipment.Consignor.CompanyData.IsARTaxApplicable;
				}
				else if (shipment.IsImport() && shipment.Consignee != null)
				{
					isGoodsAndServicesTaxApplicable = shipment.Consignee.CompanyData.IsARTaxApplicable;
				}

				if (!isGoodsAndServicesTaxApplicable)
				{
					result = GoodsAndServicesTaxNotApplicable;
				}
				else if (GetGoodsValueInEuro(shipment) <= FreightDataRegistry.Instance.AirExpressShipmentBreakValueInEuro.Value)
				{
					result = GoodsValueLessThanBreakValue;
				}
			}

			return result;
		}

		#region Implementation
		readonly Sendingarnumer Sendingarnumer;
		const string GoodsValueZero = "01";
		const string GoodsValueLessThanBreakValue = "02";
		const string GoodsAndServicesTaxNotApplicable = "03";

		ZDecimal GetGoodsValueInEuro(ForwardingShipment shipment)
		{
			ZDecimal result = 0m;

			if (shipment.GoodsValueCurr.RX_Code != "EUR")
			{
				RefCurrency euroCurrency = shipment.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "EUR");
				CurrencyConverter currencyConverter = CurrencyConverter.New(shipment.Factory);
				currencyConverter.DateForRate = ZDateTime.Today;

				Money convertedToLocalCurrency = currencyConverter.ConvertExact(new Money(shipment.JS_GoodsValue, shipment.GoodsValueCurr), GlbCompany.CurrentCompany.LocalCurrency);
				Money moneyInEuro = currencyConverter.ConvertExact(new Money(convertedToLocalCurrency.Amount, GlbCompany.CurrentCompany.LocalCurrency), euroCurrency);
				result = moneyInEuro.Amount;
			}
			else
			{
				result = shipment.JS_GoodsValue;
			}

			return result;
		}
		#endregion
	}
}
