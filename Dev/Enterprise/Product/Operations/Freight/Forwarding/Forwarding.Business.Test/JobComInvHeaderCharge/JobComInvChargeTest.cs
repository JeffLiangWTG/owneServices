using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobComInvCharge))]
	sealed class JobComInvChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIncludedInITOTReadOnly()
		{
			Order.JD_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvCharge charge = Order.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals(true, charge.J7_IsIncludedInITOTInfo.ReadOnly);

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
			AssertEquals(false, charge.J7_IsIncludedInITOTInfo.ReadOnly);

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			AssertEquals(false, charge.J7_IsIncludedInITOTInfo.ReadOnly);

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			AssertEquals(false, charge.J7_IsIncludedInITOTInfo.ReadOnly);

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			AssertEquals(false, charge.J7_IsIncludedInITOTInfo.ReadOnly);

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.ExWorks;
			AssertEquals(false, charge.J7_IsIncludedInITOTInfo.ReadOnly);

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
			AssertEquals(true, charge.J7_IsIncludedInITOTInfo.ReadOnly);

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			AssertEquals(false, charge.J7_IsIncludedInITOTInfo.ReadOnly);
		}

		public void TestJ7_ExchangeRateReadOnly()
		{
			JobComInvCharge charge = Order.Charges.AddNew();
			charge.J7_RX_NKCurrency = ZString.Empty;
			Assert("Ex-rate readonly as no currency has been entered", charge.J7_ExchangeRateInfo.ReadOnly);
			Assert(!charge.IsJ7_ExchangeRateUserEnterable);

			charge.J7_RX_NKCurrency = Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Assert("Ex-rate readonly as it is in local currency", charge.J7_ExchangeRateInfo.ReadOnly);
			AssertEquals(1m, charge.J7_ExchangeRate);
			Assert(!charge.IsJ7_ExchangeRateUserEnterable);

			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Afghanistan;
			Assert(charge.IsJ7_ExchangeRateUserEnterable);
			Assert("Ex-rate readonly as it is in foreign currency", !charge.J7_ExchangeRateInfo.ReadOnly);
		}

		public void TestDefaultExchangeRateFromRefExchangeRateTables()
		{
			//Customs Ex-rate type first and then BUY type
			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "ZZZ";
			RefExchangeRate rateCus = currency.ExchangeRates.AddNew();
			rateCus.RE_StartDate = new ZDateTime(2005, 1, 1);
			rateCus.RE_ExpiryDate = new ZDateTime(2005, 1, 5);
			rateCus.RE_SellRate = 1.50m;
			rateCus.RE_ExRateType = "CUS";

			RefExchangeRate rateBuy = currency.ExchangeRates.AddNew();
			rateBuy.RE_StartDate = new ZDateTime(2005, 1, 1);
			rateBuy.RE_ExpiryDate = new ZDateTime(2005, 1, 5);
			rateBuy.RE_SellRate = 1.52m;
			rateBuy.RE_ExRateType = "BUY";

			var order = Factory.New<Order>();
			order.UpdateEvent(Enterprise.ZArchitecture.Business.Events.Departure, new ZDateTimeOffset(2005, 1, 1));
			JobComInvCharge charge = order.Charges.AddNew();
			charge.J7_RX_NKCurrency = "ZZZ";
			AssertEquals("Ex-rate is defaulted", 1.5m, charge.J7_ExchangeRate);

			charge.J7_ExchangeRate = 1.6m;
			charge.J7_Amount = 10m;

			AssertEquals("however when conversion happens, it should use whatever users have entered(1.6)", 6.25m, charge.MoneyInLocalCurrency.Amount);
		}

		public void TestDefaultExRateFromOrderExRateIfNecessary()
		{
			Order.JD_RX_NKOrderCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			Order.JD_EstimatedExchangeRate = 0.77m;

			JobComInvCharge charge = Order.Charges.AddNew();
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals(0.77m, charge.J7_ExchangeRate);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Order.Charges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var buyer = factory.NewWithValidTestData<OrgHeader>();

			var orderData = factory.New<Order>();
			orderData.BuyerPK = buyer.PK;

			return orderData.Charges.AddNew();
		}

		Order Order
		{
			get { return order ?? (order = Factory.New<Order>()); }
		}
		Order order;
	}
}
