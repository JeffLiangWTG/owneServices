using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ShipmentSendingarnumerHelperTest : TestCaseWithFactory
	{
		public void TestGetExpressCarrierNumber()
		{
			shipment.JS_GoodsValue = 0M;
			shipment.JS_RX_NKGoodsValueCurr = "EUR";
			AssertEquals("should be 1", "01", sendingarnumerHelper.GetExpressCarrierNumber(shipment));

			shipment.JS_GoodsValue = threshold + 1;
			shipment.Consignor.CompanyData.SetARTaxApplicable(false);
			AssertEquals("should be 3", "03", sendingarnumerHelper.GetExpressCarrierNumber(shipment));

			shipment.Consignor.CompanyData.SetARTaxApplicable(true);
			AssertEquals("should be empty", "", sendingarnumerHelper.GetExpressCarrierNumber(shipment));

			shipment.JS_GoodsValue = threshold;
			AssertEquals("should be 2", "02", sendingarnumerHelper.GetExpressCarrierNumber(shipment));

			shipment.JS_RX_NKGoodsValueCurr = "ISK";
			AssertEquals("should be 2", "02", sendingarnumerHelper.GetExpressCarrierNumber(shipment));

			shipment.JS_GoodsValue = threshold + 100M;
			shipment.JS_RX_NKGoodsValueCurr = "USD";
			AssertEquals("should be empty", "", sendingarnumerHelper.GetExpressCarrierNumber(shipment));

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "ISKEF";
			shipment.Consignee.CompanyData.SetARTaxApplicable(false);
			AssertEquals("should be 3", "03", sendingarnumerHelper.GetExpressCarrierNumber(shipment));
		}

		ShipmentSendingarnumerHelper sendingarnumerHelper;
		ForwardingShipment shipment;
		RefCurrency EuroCurrency;
		RefCurrency IcelandCurrency;
		RefCurrency USCurrency;
		ZDateTime today;
		OrgHeader consignor;
		OrgHeader consignee;
		ZDecimal threshold;

		protected override void SetUp()
		{
			base.SetUp();
			today = ZDateTime.Now;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			threshold = new ZDecimal(FreightDataRegistry.Instance.AirExpressShipmentBreakValueInEuro.Value);

			sendingarnumerHelper = new ShipmentSendingarnumerHelper(new Sendingarnumer("dummy code"));

			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "ISKEF";
			shipment.JS_RL_NKDestination = "AUSYD";

			consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			SetupCurrencies();
		}

		void SetupCurrencies()
		{
			EuroCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "EUR");
			AddBuySellRates(EuroCurrency, 0.5M, 1.5M);

			IcelandCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "ISK");
			AddBuySellRates(IcelandCurrency, 1M, 1M);

			USCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			AddBuySellRates(USCurrency, 1.5M, 0.5M);
		}

		void AddBuySellRates(RefCurrency currency, ZDecimal buy, ZDecimal sell)
		{
			RefExchangeRate exRate = currency.ExchangeRates.AddNew();
			exRate.RE_StartDate = today.AddDays(-1);
			exRate.RE_ExpiryDate = today.AddDays(1);
			exRate.RE_SellRate = sell;
			exRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;

			exRate = currency.ExchangeRates.AddNew();
			exRate.RE_StartDate = today.AddDays(-1);
			exRate.RE_ExpiryDate = today.AddDays(1);
			exRate.RE_SellRate = buy;
			exRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
		}
	}
}
