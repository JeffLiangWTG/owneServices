using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.Testing
{
	public class PaymentPartyListTest : TestCaseWithFactory
	{
		public void TestGetCustomsCode()
		{
			AssertEquals("PaymentPartyList.GetCustomsCode(PaymentPartyList.Codes.CashPaidByBroker)", CustomsPaymentTypeList.Codes.CashPayment, PaymentMethodList.GetCustomsCode(PaymentMethodList.Codes.CashPaidByBroker));
			AssertEquals("PaymentPartyList.GetCustomsCode(PaymentPartyList.Codes.CashPaidByClient)", CustomsPaymentTypeList.Codes.CashPayment, PaymentMethodList.GetCustomsCode(PaymentMethodList.Codes.CashPaidByClient));
			AssertEquals("PaymentPartyList.GetCustomsCode(PaymentPartyList.Codes.BrokerDeferred)", CustomsPaymentTypeList.Codes.BrokerDeferred, PaymentMethodList.GetCustomsCode(PaymentMethodList.Codes.BrokerDeferred));
			AssertEquals("PaymentPartyList.GetCustomsCode(PaymentPartyList.Codes.ImporterDeferred)", CustomsPaymentTypeList.Codes.ClientDeferred, PaymentMethodList.GetCustomsCode(PaymentMethodList.Codes.ClientDeferred));
		}

		public void TestGetPaymentPartyCode()
		{
			AssertEquals(PaymentMethodList.Codes.BrokerDeferred, PaymentMethodList.GetPaymentPartyCode(CustomsPaymentTypeList.Codes.BrokerDeferred, PaymentMethodList.Codes.BrokerDeferred));
			AssertEquals(PaymentMethodList.Codes.CashPaidByBroker, PaymentMethodList.GetPaymentPartyCode(CustomsPaymentTypeList.Codes.CashPayment, PaymentMethodList.Codes.BrokerDeferred));
			AssertEquals(PaymentMethodList.Codes.ClientDeferred, PaymentMethodList.GetPaymentPartyCode(CustomsPaymentTypeList.Codes.ClientDeferred, PaymentMethodList.Codes.BrokerDeferred));

			AssertEquals(PaymentMethodList.Codes.BrokerDeferred, PaymentMethodList.GetPaymentPartyCode(CustomsPaymentTypeList.Codes.BrokerDeferred, PaymentMethodList.Codes.CashPaidByBroker));
			AssertEquals(PaymentMethodList.Codes.CashPaidByBroker, PaymentMethodList.GetPaymentPartyCode(CustomsPaymentTypeList.Codes.CashPayment, PaymentMethodList.Codes.CashPaidByBroker));
			AssertEquals(PaymentMethodList.Codes.ClientDeferred, PaymentMethodList.GetPaymentPartyCode(CustomsPaymentTypeList.Codes.ClientDeferred, PaymentMethodList.Codes.CashPaidByBroker));

			AssertEquals(PaymentMethodList.Codes.BrokerDeferred, PaymentMethodList.GetPaymentPartyCode(CustomsPaymentTypeList.Codes.BrokerDeferred, PaymentMethodList.Codes.CashPaidByClient));
			AssertEquals(PaymentMethodList.Codes.CashPaidByClient, PaymentMethodList.GetPaymentPartyCode(CustomsPaymentTypeList.Codes.CashPayment, PaymentMethodList.Codes.CashPaidByClient));
			AssertEquals(PaymentMethodList.Codes.ClientDeferred, PaymentMethodList.GetPaymentPartyCode(CustomsPaymentTypeList.Codes.ClientDeferred, PaymentMethodList.Codes.CashPaidByClient));

			AssertEquals(PaymentMethodList.Codes.BrokerDeferred, PaymentMethodList.GetPaymentPartyCode(CustomsPaymentTypeList.Codes.BrokerDeferred, PaymentMethodList.Codes.ClientDeferred));
			AssertEquals(PaymentMethodList.Codes.CashPaidByClient, PaymentMethodList.GetPaymentPartyCode(CustomsPaymentTypeList.Codes.CashPayment, PaymentMethodList.Codes.ClientDeferred));
			AssertEquals(PaymentMethodList.Codes.ClientDeferred, PaymentMethodList.GetPaymentPartyCode(CustomsPaymentTypeList.Codes.ClientDeferred, PaymentMethodList.Codes.ClientDeferred));
		}

		public void TestIsPaidByBroker()
		{
			Assert(PaymentMethodList.IsPaidByBroker(PaymentMethodList.Codes.BrokerDeferred));
			Assert(!PaymentMethodList.IsPaidByBroker(PaymentMethodList.Codes.ClientDeferred));
			Assert(PaymentMethodList.IsPaidByBroker(PaymentMethodList.Codes.CashPaidByBroker));
			Assert(!PaymentMethodList.IsPaidByBroker(PaymentMethodList.Codes.CashPaidByClient));
		}

		public void TestIsValidCode()
		{
			Assert(PaymentMethodList.IsValidCode(PaymentMethodList.Codes.BrokerDeferred));
			Assert(PaymentMethodList.IsValidCode(PaymentMethodList.Codes.ClientDeferred));
			Assert(PaymentMethodList.IsValidCode(PaymentMethodList.Codes.CashPaidByBroker));
			Assert(PaymentMethodList.IsValidCode(PaymentMethodList.Codes.CashPaidByClient));

			Assert(!PaymentMethodList.IsValidCode(FreightPaymentMethodList.Codes.CA));
			Assert(!PaymentMethodList.IsValidCode(FreightPaymentMethodList.Codes.FO));
		}
	}
}
